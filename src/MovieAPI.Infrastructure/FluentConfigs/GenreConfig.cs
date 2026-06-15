using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Infrastructure.FluentConfigs;

public class GenreConfig : IEntityTypeConfiguration<Genre>
{
  public void Configure(EntityTypeBuilder<Genre> builder)
  {
    builder.Property(g => g.Id).ValueGeneratedOnAdd();
    builder.Property(g => g.CreatedAt).ValueGeneratedOnAdd();
    builder.Property(g => g.UpdatedAt).ValueGeneratedOnAddOrUpdate();

    builder.HasIndex(g => g.Name);
  }
}
