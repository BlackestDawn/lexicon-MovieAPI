using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Application.Services;
using MovieAPI.Application.Validators;
using MovieAPI.Infrastructure;
using MovieAPI.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IGenreService, GenreService>();

builder.Services.AddScoped<IValidator<MovieForCreationDto>, MovieCreationValidator>();
builder.Services.AddScoped<IValidator<MovieForUpdateDto>, MovieUpdateValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.UseSwagger();
  app.UseSwaggerUI();
  await DbSeeder.SeedAsync(app.Services);
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
