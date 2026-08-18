using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Infrastructure.FluentConfigs;

public class ApplicationUserConfig : IEntityTypeConfiguration<ApplicationUser>
{
  public void Configure(EntityTypeBuilder<ApplicationUser> builder)
  {
    builder.Property(u => u.Id)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("gen_random_uuid()");
    builder.Property(u => u.CreatedAt)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("clock_timestamp()");
    builder.Property(u => u.UpdatedAt)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("clock_timestamp()");
  }
}
