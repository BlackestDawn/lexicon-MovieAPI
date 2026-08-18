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
      .HasDefaultValueSql("gen_random_uuid()");
    builder.Property(p => p.CreatedAt)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("clock_timestamp()");
    builder.Property(p => p.UpdatedAt)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("clock_timestamp()");

    builder.HasIndex(p => p.GivenName);
    builder.HasIndex(p => p.MiddleName);
    builder.HasIndex(p => p.LastName);
  }
}
