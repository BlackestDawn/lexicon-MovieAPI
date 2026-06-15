using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Infrastructure.FluentConfigs;

public class CastCrewConfig : IEntityTypeConfiguration<CastCrew>
{
  public void Configure(EntityTypeBuilder<CastCrew> builder)
  {
    builder.Property(c => c.Id).ValueGeneratedOnAdd();
    builder.Property(c => c.CreatedAt).ValueGeneratedOnAdd();
    builder.Property(c => c.UpdatedAt).ValueGeneratedOnAddOrUpdate();

    builder.HasKey(c => new { c.MovieId, c.PersonId });
  }
}
