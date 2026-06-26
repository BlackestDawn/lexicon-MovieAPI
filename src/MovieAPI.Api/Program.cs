using System.Security.Claims;
using System.Text;
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MovieAPI.Api.Middleware;
using MovieAPI.Api.Swagger;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Application.Services;
using MovieAPI.Application.validators;
using MovieAPI.Domain.Constants;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure;
using MovieAPI.Infrastructure.Interfaces;
using MovieAPI.Infrastructure.Services;
using Serilog;
using Serilog.Sinks.Elasticsearch;

// Bootstrap logger captures startup failures (e.g. missing config) that happen
// before the host's own Serilog pipeline is wired up below.
Log.Logger = new LoggerConfiguration()
  .WriteTo.Console()
  .CreateBootstrapLogger();

try
{
  var builder = WebApplication.CreateBuilder(args);

  builder.Host.UseSerilog((context, services, loggerConfig) =>
  {
    loggerConfig.ReadFrom.Configuration(context.Configuration);

    // The Elasticsearch sink is added in code rather than appsettings because it's
    // required in Production only; this mirrors the fail-fast pattern used for the
    // Redis output-cache connection string below.
    if (context.HostingEnvironment.IsProduction())
    {
      var elasticsearchUri = context.Configuration["Elasticsearch:Uri"]
        ?? throw new InvalidOperationException("Configuration value 'Elasticsearch:Uri' is not configured.");

      loggerConfig.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticsearchUri))
      {
        IndexFormat = "movieapi-logs-{0:yyyy.MM}",
        AutoRegisterTemplate = true,
        AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
        EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog,
      });
    }
  });

  builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
  builder.Services.AddProblemDetails();

  builder.Services.AddOpenApi();
  builder.Services.AddApiVersioning(options =>
  {
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
      new UrlSegmentApiVersionReader(),
      new QueryStringApiVersionReader("apiversion"),
      new HeaderApiVersionReader("X_API_VERSION")
    );
  })
  .AddMvc()
  .AddApiExplorer(options =>
  {
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
  });
  builder.Services.AddEndpointsApiExplorer();
  builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
  builder.Services.AddSwaggerGen(options =>
  {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
      Name = "Authorization",
      Type = SecuritySchemeType.ApiKey,
      Scheme = "Bearer",
      BearerFormat = "JWT",
      In = ParameterLocation.Header,
      Description = "Paste the access token from /api/auth/login or /api/auth/register here - no \"Bearer \" prefix needed.",
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
      [new OpenApiSecuritySchemeReference("Bearer", document)] = [],
    });
  });

  builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
      builder.Configuration.GetConnectionString("sqlserver")
      ?? throw new InvalidProgramException()
    ));

  // AddIdentityCore (not AddIdentity) since this is an API project: it registers
  // UserManager/RoleManager without pulling in the cookie-auth middleware that
  // AddIdentity assumes. The actual bearer scheme is configured below via AddJwtBearer.
  builder.Services.AddIdentityCore<ApplicationUser>(options => options.User.RequireUniqueEmail = true)
    .AddRoles<ApplicationRole>()
    .AddSignInManager()
    .AddEntityFrameworkStores<AppDbContext>()
    // Backs GeneratePasswordResetTokenAsync/ResetPasswordAsync - without this,
    // forgot-password fails at runtime with "No IUserTwoFactorTokenProvider<TUser>
    // named 'Default' is registered."
    .AddDefaultTokenProviders();

  // The Jwt:* lookups happen inside this callback (not eagerly above) because
  // it's only invoked once JwtBearerOptions are actually resolved, after the host
  // is built. Reading them eagerly here would run before WebApplicationFactory's
  // test configuration overrides are merged in, breaking integration tests - the
  // same reason the sqlserver connection string above is read inside AddDbContext's
  // options callback rather than directly against builder.Configuration.
  builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
      var jwtIssuer = builder.Configuration["Jwt:Issuer"]
        ?? throw new InvalidOperationException("Configuration value 'Jwt:Issuer' is not configured.");
      var jwtAudience = builder.Configuration["Jwt:Audience"]
        ?? throw new InvalidOperationException("Configuration value 'Jwt:Audience' is not configured.");
      var jwtKey = builder.Configuration["Jwt:Key"]
        ?? throw new InvalidOperationException("Configuration value 'Jwt:Key' is not configured.");

      options.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
      };

      // Runs on every authenticated request, after signature/expiry checks pass.
      // A token's security-stamp claim is fixed at issuance; comparing it against
      // the user's current stamp means anything that bumps the stamp (password
      // change/reset) invalidates every access token issued before that point,
      // without needing a token blacklist. Deliberately a real per-request DB read,
      // not cached - the whole point is no staleness window.
      options.Events = new JwtBearerEvents
      {
        OnTokenValidated = async context =>
        {
          // The default inbound claim map rewrites "sub" to the long ClaimTypes.NameIdentifier
          // URI on the validated principal, so that's the key to look up here, not the
          // short JWT claim name actually embedded in the token (see ClaimsPrincipalExtensions).
          var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
          var tokenStamp = context.Principal?.FindFirstValue(CustomClaimTypes.SecurityStamp);

          if (userId is null || tokenStamp is null)
          {
            context.Fail("Token is missing required claims.");
            return;
          }

          var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
          var user = await userManager.FindByIdAsync(userId);

          if (user is null || !string.Equals(user.SecurityStamp, tokenStamp, StringComparison.Ordinal))
          {
            context.Fail("Token is no longer valid.");
          }
        },
      };
    });

  builder.Services.AddAuthorization();

  builder.Services.AddAutoMapper(config => {},
    AppDomain.CurrentDomain.GetAssemblies());
  builder.Services.AddControllers();

  // Redis backs the output cache in Production so it's shared across instances; every
  // other environment (Development, Testing, ...) keeps the default in-memory store.
  if (builder.Environment.IsProduction())
  {
    builder.Services.AddStackExchangeRedisOutputCache(options =>
    {
      options.Configuration = builder.Configuration.GetConnectionString("redis")
        ?? throw new InvalidOperationException("Connection string 'redis' is not configured.");
    });
  }

  builder.Services.AddOutputCache(options =>
  {
    // Movies, genres, people and reviews embed each other's data in their
    // "extended"/detail DTOs, so a write to any one of them can make cached
    // responses from the others stale. Sharing one tag keeps invalidation correct
    // at the cost of evicting more than strictly necessary on each write.
    options.AddPolicy("CatalogCache", policy =>
      policy.Expire(TimeSpan.FromMinutes(5)).Tag("catalog"));
  });

  builder.Services.AddScoped<IMovieRepository, MovieRepository>();
  builder.Services.AddScoped<IGenreRepository, GenreRepository>();
  builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
  builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
  builder.Services.AddScoped<IPersonRepository, PersonRepository>();
  builder.Services.AddScoped<IMovieService, MovieService>();
  builder.Services.AddScoped<IGenreService, GenreService>();
  builder.Services.AddScoped<IPersonService, PersonService>();
  builder.Services.AddScoped<IReviewService, ReviewService>();
  builder.Services.AddScoped<IAuthService, AuthService>();
  builder.Services.AddScoped<IAdminUserService, AdminUserService>();
  builder.Services.AddScoped<ITokenService, TokenService>();
  // Logs reset tokens instead of emailing them - see LoggingEmailSender for why
  // that's a deliberate placeholder rather than a missing integration.
  builder.Services.AddScoped<IEmailSender, LoggingEmailSender>();

  builder.Services.AddScoped<IValidator<MovieForChangeDto>, MovieChangeValidator>();
  builder.Services.AddScoped<IValidator<PersonForChangeDto>, PersonChangeValidator>();
  builder.Services.AddScoped<IValidator<ReviewForChangeDto>, ReviewChangeValidator>();
  builder.Services.AddScoped<IValidator<GenreForChangeDto>, GenreChangeValidator>();
  builder.Services.AddScoped<IValidator<RegisterDto>, RegisterValidator>();
  builder.Services.AddScoped<IValidator<LoginDto>, LoginValidator>();
  builder.Services.AddScoped<IValidator<UserForUpdateDto>, UserUpdateValidator>();
  builder.Services.AddScoped<IValidator<ChangePasswordDto>, ChangePasswordValidator>();
  builder.Services.AddScoped<IValidator<RefreshTokenDto>, RefreshTokenValidator>();
  builder.Services.AddScoped<IValidator<ForgotPasswordDto>, ForgotPasswordValidator>();
  builder.Services.AddScoped<IValidator<ResetPasswordDto>, ResetPasswordValidator>();
  builder.Services.AddScoped<IValidator<AdminUserForCreationDto>, AdminUserCreationValidator>();
  builder.Services.AddScoped<IValidator<AdminUserForUpdateDto>, AdminUserUpdateValidator>();

  var app = builder.Build();

  app.UseSerilogRequestLogging();

  // Defaults an unversioned request (e.g. /api/people) to /api/v1/people. This has to be
  // a path rewrite rather than a second [Route] template on the v1 controllers: ASP.NET
  // Core requires every attribute route sharing a Name (e.g. "GetPerson", used by
  // CreatedAtRoute) to resolve to the exact same template string, so a versioned and an
  // unversioned template can't coexist under one name - the app fails at startup.
  //
  // This must run before endpoint matching, so UseRouting is called explicitly right
  // after it - the minimal-hosting model otherwise auto-inserts routing as the very
  // first middleware in the pipeline, ahead of anything added here via app.Use.
  app.Use((context, next) =>
  {
    var path = context.Request.Path.Value;

    if (path is not null && path.StartsWith("/api/", StringComparison.Ordinal))
    {
      var afterApi = path["/api/".Length..];
      var firstSegmentEnd = afterApi.IndexOf('/');
      var firstSegment = firstSegmentEnd >= 0 ? afterApi[..firstSegmentEnd] : afterApi;
      var isVersioned = firstSegment.Length > 1 && firstSegment[0] is 'v' or 'V' && char.IsDigit(firstSegment[1]);

      if (!isVersioned)
      {
        context.Request.Path = "/api/v1/" + afterApi;
      }
    }

    return next(context);
  });
  app.UseRouting();

  app.UseExceptionHandler();

  if (app.Environment.IsDevelopment())
  {
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
      foreach (var desc in app.DescribeApiVersions().Reverse())
      {
        options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", desc.GroupName.ToUpperInvariant());
      }
    });
    await DbSeeder.SeedAsync(app.Services);
  }

  // Skipped in the "Testing" environment: integration tests apply migrations lazily
  // inside IntegrationTestWebAppFactory.InitializeAsync, after the host (and this
  // line) has already run, so the AspNetRoles table wouldn't exist yet here. The
  // test factory seeds roles itself once migration is done.
  if (!app.Environment.IsEnvironment("Testing"))
  {
    await RoleSeeder.SeedAsync(app.Services);
    // No-op unless Seed:AdminEmail/Seed:AdminPassword are configured - see
    // AdminUserSeeder for why that's the deliberate default outside Development.
    await AdminUserSeeder.SeedAsync(app.Services);
  }

  app.UseHttpsRedirection();

  app.UseAuthentication();
  app.UseAuthorization();

  app.UseOutputCache();

  app.MapControllers();

  app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
  Log.Fatal(ex, "MovieAPI terminated unexpectedly during startup");
}
finally
{
  Log.CloseAndFlush();
}

public partial class Program { }
