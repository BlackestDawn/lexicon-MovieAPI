using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
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
  // AddIdentity assumes. The actual auth scheme (JWT, etc.) is a later step.
  builder.Services.AddIdentityCore<ApplicationUser>()
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<AppDbContext>();

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
  builder.Services.AddScoped<IPersonRepository, PersonRepository>();
  builder.Services.AddScoped<IMovieService, MovieService>();
  builder.Services.AddScoped<IGenreService, GenreService>();
  builder.Services.AddScoped<IPersonService, PersonService>();
  builder.Services.AddScoped<IReviewService, ReviewService>();

  builder.Services.AddScoped<IValidator<MovieForChangeDto>, MovieChangeValidator>();
  builder.Services.AddScoped<IValidator<PersonForChangeDto>, PersonChangeValidator>();
  builder.Services.AddScoped<IValidator<ReviewForChangeDto>, ReviewChangeValidator>();
  builder.Services.AddScoped<IValidator<GenreForChangeDto>, GenreChangeValidator>();

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

  app.UseHttpsRedirection();

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
