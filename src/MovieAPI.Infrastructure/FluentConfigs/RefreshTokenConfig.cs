using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Infrastructure.FluentConfigs;

public class RefreshTokenConfig : IEntityTypeConfiguration<RefreshToken>
{
  public void Configure(EntityTypeBuilder<RefreshToken> builder)
  {
    builder.Property(r => r.Id)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("NEWSEQUENTIALID()");
    builder.Property(r => r.CreatedAt)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("SYSUTCDATETIME()");
    builder.Property(r => r.UpdatedAt)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("SYSUTCDATETIME()");

    builder.HasIndex(r => r.TokenHash).IsUnique();

    // Cascade because refresh tokens are pure session artifacts with no value once
    // their user is gone, unlike Review's Restrict (where the content has standalone
    // value even after the original author's account disappears).
    builder.HasOne(r => r.User).WithMany().OnDelete(DeleteBehavior.Cascade);
  }
}
