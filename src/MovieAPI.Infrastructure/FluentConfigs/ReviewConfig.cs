using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Infrastructure.FluentConfigs;

public class ReviewConfig : IEntityTypeConfiguration<Review>
{
  public void Configure(EntityTypeBuilder<Review> builder)
  {
    builder.Property(r => r.Id)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("NEWID()");
    builder.Property(r => r.CreatedAt)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("SYSUTCDATETIME()");
    builder.Property(r => r.UpdatedAt)
      .ValueGeneratedOnAddOrUpdate()
      .HasDefaultValueSql("SYSUTCDATETIME()");

    builder.ToTable(tb => tb.HasTrigger("TR_Reviews_UpdatedAt"));

    builder.HasOne(r => r.Movie).WithMany(m => m.Reviews).OnDelete(DeleteBehavior.Cascade);
  }
}
