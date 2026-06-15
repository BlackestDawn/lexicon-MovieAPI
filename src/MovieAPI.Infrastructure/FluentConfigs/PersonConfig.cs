using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Infrastructure.FluentConfigs;

public class PersonConfig : IEntityTypeConfiguration<Person>
{
  public void Configure(EntityTypeBuilder<Person> builder)
  {
    builder.Property(c => c.Id).ValueGeneratedOnAdd();
    builder.Property(c => c.CreatedAt).ValueGeneratedOnAdd();
    builder.Property(c => c.UpdatedAt).ValueGeneratedOnAddOrUpdate();

    builder.HasIndex(c => c.FirstName);
    builder.HasIndex(c => c.LastName);
  }
}
