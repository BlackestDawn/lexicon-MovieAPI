using System.Reflection;
using System.Security.Cryptography;
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MovieAPI.Api.Authentication;
using MovieAPI.Api.Middleware;
using MovieAPI.Api.Swagger;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Application.Services;
using MovieAPI.Application.validators;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure;
using MovieAPI.Infrastructure.Interfaces;
using MovieAPI.Infrastructure.Services;
using OpenIddict.Validation.AspNetCore;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.GoogleCloudLogging;
using static OpenIddict.Abstractions.OpenIddictConstants;

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

    // Extra sinks beyond what appsettings.json's Serilog:WriteTo already declares
    // (Console always, File in dev) are chosen via config instead of gated on
    // environment name, so any environment can opt in - mirrors the fail-fast
    // pattern used for OpenIddict's signing/encryption keys below.
    var logSink = context.Configuration.GetValue("Logging:Sink", "Console");
    switch (logSink)
    {
      case "Console":
        break;

      case "GoogleCloudLogging":
        var projectId = context.Configuration["GoogleCloudLogging:ProjectId"]
          ?? throw new InvalidOperationException("Configuration value 'GoogleCloudLogging:ProjectId' is not configured.");

        // On Cloud Run/GCE/GKE, Application Default Credentials resolve automatically
        // to the running service's own identity - no key file or extra config needed
        // beyond granting that identity roles/logging.logWriter.
        loggerConfig.WriteTo.GoogleCloudLogging(new GoogleCloudLoggingSinkOptions
        {
          ProjectId = projectId,
        });
        break;

      case "Elasticsearch":
        var elasticsearchUri = context.Configuration["Elasticsearch:Uri"]
          ?? throw new InvalidOperationException("Configuration value 'Elasticsearch:Uri' is not configured.");

        loggerConfig.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticsearchUri))
        {
          IndexFormat = "movieapi-logs-{0:yyyy.MM}",
          AutoRegisterTemplate = true,
          AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
          EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog,
        });
        break;

      default:
        throw new InvalidOperationException($"Unknown 'Logging:Sink' value '{logSink}'.");
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
    // Lets Swagger UI's Authorize dialog do a real password-grant login against
    // /connect/token instead of requiring a token to be pasted in by hand.
    options.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
    {
      Type = SecuritySchemeType.OAuth2,
      Flows = new OpenApiOAuthFlows
      {
        Password = new OpenApiOAuthFlow
        {
          TokenUrl = new Uri("/connect/token", UriKind.Relative),
        },
      },
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
      [new OpenApiSecuritySchemeReference("OAuth2", document)] = [],
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
  });

  builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
      builder.Configuration.GetConnectionString("postgres")
      ?? throw new InvalidProgramException()
    ));

  // AddIdentityCore (not AddIdentity) since this is an API project: it registers
  // UserManager/RoleManager without pulling in the cookie-auth middleware that
  // AddIdentity assumes. The actual bearer scheme is configured below via OpenIddict.
  builder.Services.AddIdentityCore<ApplicationUser>(options => options.User.RequireUniqueEmail = true)
    .AddRoles<ApplicationRole>()
    .AddSignInManager()
    .AddEntityFrameworkStores<AppDbContext>()
    // Backs GeneratePasswordResetTokenAsync/ResetPasswordAsync - without this,
    // forgot-password fails at runtime with "No IUserTwoFactorTokenProvider<TUser>
    // named 'Default' is registered."
    .AddDefaultTokenProviders();

  // The OpenIddict:* lookups happen inside this callback (not eagerly above) because
  // it's only invoked once the server options are actually resolved, after the host
  // is built. Reading them eagerly here would run before WebApplicationFactory's
  // test configuration overrides are merged in, breaking integration tests - the
  // same reason the sqlserver connection string above is read inside AddDbContext's
  // options callback rather than directly against builder.Configuration.
  builder.Services.AddOpenIddict()
    .AddCore(options => options.UseEntityFrameworkCore().UseDbContext<AppDbContext>())
    .AddServer(options =>
    {
      options.SetTokenEndpointUris("connect/token");
      options.SetRevocationEndpointUris("connect/token/revoke");

      options.AllowPasswordFlow();
      options.AllowRefreshTokenFlow();
      // Rolling refresh tokens (the default - not disabled here) make each one
      // single-use: redeeming one immediately revokes it and issues a new one.
      // Replaying an already-redeemed refresh token (e.g. a stolen one racing the
      // legitimate client) is treated as theft and automatically revokes every other
      // token tied to the same authorization - built into OpenIddict, no custom
      // detection code needed. The reuse leeway defaults to 30 seconds (tolerating
      // network retries); zeroed out here to match the zero-tolerance behavior the
      // old hand-rolled refresh system had.
      options.SetRefreshTokenReuseLeeway(TimeSpan.Zero);

      options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles, Scopes.OfflineAccess);

      var accessTokenMinutes = builder.Configuration.GetValue("OpenIddict:AccessTokenLifetimeMinutes", 60);
      var refreshTokenDays = builder.Configuration.GetValue("OpenIddict:RefreshTokenLifetimeDays", 7);
      options.SetAccessTokenLifetime(TimeSpan.FromMinutes(accessTokenMinutes));
      options.SetRefreshTokenLifetime(TimeSpan.FromDays(refreshTokenDays));

      // Ephemeral keys are fine outside Production - nothing needs to survive a
      // restart, and it avoids depending on an OS certificate store inside containers.
      // Production uses a fixed configured key so already-issued tokens keep working
      // across deploys/restarts, the same way the old static Jwt:Key did.
      if (builder.Environment.IsProduction())
      {
        var signingKey = builder.Configuration["OpenIddict:SigningKey"]
          ?? throw new InvalidOperationException("Configuration value 'OpenIddict:SigningKey' is not configured.");
        var encryptionKey = builder.Configuration["OpenIddict:EncryptionKey"]
          ?? throw new InvalidOperationException("Configuration value 'OpenIddict:EncryptionKey' is not configured.");

        // OpenIddict unconditionally requires at least one asymmetric signing key (it
        // also publishes the public key via the JWKS discovery endpoint) - a symmetric
        // key is only ever valid for the encryption key below, never for signing.
        // Generate a PKCS#8 DER-encoded RSA private key with:
        //   openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 | openssl pkcs8 -topk8 -nocrypt -outform DER | base64 -w0
        var rsa = RSA.Create();
        rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(signingKey), out _);
        options.AddSigningKey(new RsaSecurityKey(rsa));
        options.AddEncryptionKey(new SymmetricSecurityKey(Convert.FromBase64String(encryptionKey)));
      }
      else
      {
        options.AddEphemeralEncryptionKey();
        options.AddEphemeralSigningKey();
      }

      options.UseAspNetCore()
        .EnableTokenEndpointPassthrough()
        // This app is never the one terminating TLS - not in local/test hosting, and
        // not in the demo docker-compose stack either (it exposes plain HTTP on 8080,
        // same as the old JWT login endpoint did). A real deployment would terminate
        // TLS in front of Kestrel.
        .DisableTransportSecurityRequirement();
    })
    .AddValidation(options =>
    {
      options.UseLocalServer();
      options.UseAspNetCore();

      // Runs on every authenticated request, after OpenIddict's own signature/expiry
      // checks pass. A token's security-stamp claim is fixed at issuance; comparing it
      // against the user's current stamp means anything that bumps the stamp (password
      // change/reset) invalidates every access token issued before that point, without
      // needing a token blacklist. Deliberately a real per-request DB read, not cached -
      // the whole point is no staleness window.
      options.AddEventHandler<OpenIddict.Validation.OpenIddictValidationEvents.ProcessAuthenticationContext>(handler =>
        handler.UseScopedHandler<ValidateSecurityStampHandler>().SetOrder(int.MaxValue));
    });

  builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
  builder.Services.AddAuthorization();

  builder.Services.AddAutoMapper(_ => {},
    AppDomain.CurrentDomain.GetAssemblies());
  builder.Services.AddControllers();
  builder.Services.AddHealthChecks();

  // Output cache backing store is chosen via config instead of gated on environment
  // name, so it can be swapped independently of ASPNETCORE_ENVIRONMENT.
  var outputCacheProvider = builder.Configuration.GetValue("OutputCache:Provider", "Memory");
  switch (outputCacheProvider)
  {
    case "Redis":
      builder.Services.AddStackExchangeRedisOutputCache(options =>
      {
        options.Configuration = builder.Configuration.GetConnectionString("redis")
          ?? throw new InvalidOperationException("Connection string 'redis' is not configured.");
      });
      break;

    case "Memory":
      break;

    default:
      throw new InvalidOperationException($"Unknown 'OutputCache:Provider' value '{outputCacheProvider}'.");
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
  builder.Services.AddScoped<IPersonRepository, PersonRepository>();
  builder.Services.AddScoped<IMovieService, MovieService>();
  builder.Services.AddScoped<IGenreService, GenreService>();
  builder.Services.AddScoped<IPersonService, PersonService>();
  builder.Services.AddScoped<IReviewService, ReviewService>();
  builder.Services.AddScoped<IAuthService, AuthService>();
  builder.Services.AddScoped<IAdminUserService, AdminUserService>();
  // Logs reset tokens instead of emailing them - see LoggingEmailSender for why
  // that's a deliberate placeholder rather than a missing integration.
  builder.Services.AddScoped<IEmailSender, LoggingEmailSender>();

  builder.Services.AddScoped<IValidator<MovieForChangeDto>, MovieChangeValidator>();
  builder.Services.AddScoped<IValidator<PersonForChangeDto>, PersonChangeValidator>();
  builder.Services.AddScoped<IValidator<ReviewForChangeDto>, ReviewChangeValidator>();
  builder.Services.AddScoped<IValidator<GenreForChangeDto>, GenreChangeValidator>();
  builder.Services.AddScoped<IValidator<RegisterDto>, RegisterValidator>();
  builder.Services.AddScoped<IValidator<UserForUpdateDto>, UserUpdateValidator>();
  builder.Services.AddScoped<IValidator<ChangePasswordDto>, ChangePasswordValidator>();
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

    if (path?.StartsWith("/api/", StringComparison.Ordinal) is true)
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
    // Off by default - a real production deployment applies migrations through its
    // own release pipeline, not by racing multiple container replicas against the
    // same database at boot. The demo docker-compose stack turns this on since it
    // has no separate migration step.
    if (builder.Configuration.GetValue<bool>("ApplyMigrationsOnStartup"))
    {
      using var scope = app.Services.CreateScope();
      var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
      await db.Database.MigrateAsync();
    }

    await RoleSeeder.SeedAsync(app.Services);
    // No-op unless Seed:AdminEmail/Seed:AdminPassword are configured - see
    // AdminUserSeeder for why that's the deliberate default outside Development.
    await AdminUserSeeder.SeedAsync(app.Services);
    await OpenIddictClientSeeder.SeedAsync(app.Services);
  }

  app.UseHttpsRedirection();

  app.UseAuthentication();
  app.UseAuthorization();

  app.UseOutputCache();

  app.MapHealthChecks("/health");
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
