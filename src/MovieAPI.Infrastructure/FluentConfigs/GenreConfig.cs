using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Infrastructure.FluentConfigs;

public class GenreConfig : IEntityTypeConfiguration<Genre>
{
  public void Configure(EntityTypeBuilder<Genre> builder)
  {
    builder.Property(g => g.Id)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("NEWSEQUENTIALID()");
    builder.Property(g => g.CreatedAt)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("SYSUTCDATETIME()");
    builder.Property(g => g.UpdatedAt)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("SYSUTCDATETIME()");

    builder.HasIndex(g => g.Name);
    builder.HasIndex(g => g.Slug);
  }
}
