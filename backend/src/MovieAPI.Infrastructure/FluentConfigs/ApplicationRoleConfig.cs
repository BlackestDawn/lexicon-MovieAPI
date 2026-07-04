using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Infrastructure.FluentConfigs;

public class ApplicationRoleConfig : IEntityTypeConfiguration<ApplicationRole>
{
  public void Configure(EntityTypeBuilder<ApplicationRole> builder)
  {
    builder.Property(r => r.Id)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("NEWSEQUENTIALID()");
  }
}
