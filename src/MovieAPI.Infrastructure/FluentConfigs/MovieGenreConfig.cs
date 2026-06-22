using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Infrastructure.FluentConfigs;

public class MovieGenreConfig : IEntityTypeConfiguration<MovieGenre>
{
  public void Configure(EntityTypeBuilder<MovieGenre> builder)
  {
    builder.Property(mg => mg.Id)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("NEWID()");
    builder.Property(mg => mg.CreatedAt)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("SYSUTCDATETIME()");
    builder.Property(mg => mg.UpdatedAt)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("SYSUTCDATETIME()");

    builder.HasKey(mg => new { mg.MovieId, mg.GenreId });

    builder.HasOne(mg => mg.Movie)
      .WithMany(m => m.MovieGenres)
      .HasForeignKey(mg => mg.MovieId);
    builder.HasOne(mg => mg.Genre)
      .WithMany(g => g.MovieGenres)
      .HasForeignKey(mg => mg.GenreId);
  }
}
