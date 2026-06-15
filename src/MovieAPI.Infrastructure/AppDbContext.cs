using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<Movie> Movies { get; set; }
  public DbSet<Person> Persons { get; set; }
  public DbSet<Review> Reviews { get; set; }
  public DbSet<Genre> Genres { get; set; }
  public DbSet<CastCrew> CastCrews { get; set; }
  public DbSet<MovieGenre> MovieGenres { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }
}
