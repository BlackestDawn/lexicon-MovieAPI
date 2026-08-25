using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Infrastructure.FluentConfigs;

public class FavoriteConfig : IEntityTypeConfiguration<Favorite>
{
  public void Configure(EntityTypeBuilder<Favorite> builder)
  {
    builder.Property(f => f.Id)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("gen_random_uuid()");
    builder.Property(f => f.CreatedAt)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("clock_timestamp()");
    builder.Property(f => f.UpdatedAt)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("clock_timestamp()");

    builder.HasKey(f => new { f.UserId, f.MovieId });

    builder.HasOne(f => f.User)
      .WithMany()
      .HasForeignKey(f => f.UserId);
    builder.HasOne(f => f.Movie)
      .WithMany()
      .HasForeignKey(f => f.MovieId);
  }
}
