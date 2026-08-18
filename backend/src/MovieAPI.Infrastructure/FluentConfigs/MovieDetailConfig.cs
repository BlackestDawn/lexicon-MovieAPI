using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Infrastructure.FluentConfigs;

public class MovieDetailConfig : IEntityTypeConfiguration<MovieDetail>
{
  public void Configure(EntityTypeBuilder<MovieDetail> builder)
  {
    builder.Property(d => d.Id)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("gen_random_uuid()");
    builder.Property(d => d.CreatedAt)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("clock_timestamp()");
    builder.Property(d => d.UpdatedAt)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("clock_timestamp()");

    builder.HasOne(d => d.Movie)
      .WithOne(m => m.Details)
      .HasForeignKey<MovieDetail>(d => d.MovieId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
