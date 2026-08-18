using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Infrastructure.FluentConfigs;

public class CastCrewConfig : IEntityTypeConfiguration<CastCrew>
{
  public void Configure(EntityTypeBuilder<CastCrew> builder)
  {
    builder.Property(c => c.Id)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("gen_random_uuid()");
    builder.Property(c => c.CreatedAt)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("clock_timestamp()");
    builder.Property(c => c.UpdatedAt)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("clock_timestamp()");

    builder.HasKey(c => new { c.MovieId, c.PersonId });

    builder.HasOne(c => c.Movie)
      .WithMany(m => m.CastCrews)
      .HasForeignKey(c => c.MovieId);
    builder.HasOne(c => c.Person)
      .WithMany(p => p.CastCrews)
      .HasForeignKey(c => c.PersonId);
  }
}
