using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Infrastructure.FluentConfigs;

public class PersonConfig : IEntityTypeConfiguration<Person>
{
  public void Configure(EntityTypeBuilder<Person> builder)
  {
    builder.Property(p => p.Id)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("NEWSEQUENTIALID()");
    builder.Property(p => p.CreatedAt)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("SYSUTCDATETIME()");
    builder.Property(p => p.UpdatedAt)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("SYSUTCDATETIME()");

    builder.HasIndex(p => p.FirstName);
    builder.HasIndex(p => p.LastName);
  }
}
