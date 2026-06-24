using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using MovieAPI.Api.Middleware;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Application.Services;
using MovieAPI.Application.validators;
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
  builder.Services.AddEndpointsApiExplorer();
  builder.Services.AddSwaggerGen();

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
    .AddEntityFrameworkStores<AppDbContext>();

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

  builder.Services.AddScoped<IValidator<MovieForChangeDto>, MovieChangeValidator>();
  builder.Services.AddScoped<IValidator<PersonForChangeDto>, PersonChangeValidator>();
  builder.Services.AddScoped<IValidator<ReviewForChangeDto>, ReviewChangeValidator>();
  builder.Services.AddScoped<IValidator<GenreForChangeDto>, GenreChangeValidator>();
  builder.Services.AddScoped<IValidator<RegisterDto>, RegisterValidator>();
  builder.Services.AddScoped<IValidator<LoginDto>, LoginValidator>();
  builder.Services.AddScoped<IValidator<UserForUpdateDto>, UserUpdateValidator>();
  builder.Services.AddScoped<IValidator<ChangePasswordDto>, ChangePasswordValidator>();
  builder.Services.AddScoped<IValidator<RefreshTokenDto>, RefreshTokenValidator>();
  builder.Services.AddScoped<IValidator<AdminUserForCreationDto>, AdminUserCreationValidator>();
  builder.Services.AddScoped<IValidator<AdminUserForUpdateDto>, AdminUserUpdateValidator>();

  var app = builder.Build();

  app.UseSerilogRequestLogging();

  app.UseExceptionHandler();

  if (app.Environment.IsDevelopment())
  {
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
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
