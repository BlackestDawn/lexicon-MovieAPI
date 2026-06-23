using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MovieAPI.Api.Middleware;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Application.Services;
using MovieAPI.Application.validators;
using MovieAPI.Infrastructure;
using MovieAPI.Infrastructure.Interfaces;
using MovieAPI.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddAutoMapper(config => {},
  AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddControllers();

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

public partial class Program { }
