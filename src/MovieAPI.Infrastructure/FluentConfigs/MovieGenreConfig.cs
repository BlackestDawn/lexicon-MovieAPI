using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Infrastructure.FluentConfigs;

public class MovieGenreConfig : IEntityTypeConfiguration<MovieGenre>
{
  public void Configure(EntityTypeBuilder<MovieGenre> builder)
  {
    builder.Property(m => m.Id).ValueGeneratedOnAdd();
    builder.Property(m => m.CreatedAt).ValueGeneratedOnAdd();
    builder.Property(m => m.UpdatedAt).ValueGeneratedOnAddOrUpdate();

    builder.HasKey(m => new { m.MovieId, m.GenreId });

    builder.HasOne(mg => mg.Movie)
      .WithMany(m => m.MovieGenres)
      .HasForeignKey(mg => mg.MovieId);
    builder.HasOne(mg => mg.Genre)
      .WithMany(g => g.MovieGenres)
      .HasForeignKey(mg => mg.GenreId);
  }
}
