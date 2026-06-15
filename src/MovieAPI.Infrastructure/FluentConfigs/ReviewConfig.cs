using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Infrastructure.FluentConfigs;

public class ReviewConfig : IEntityTypeConfiguration<Review>
{
  public void Configure(EntityTypeBuilder<Review> builder)
  {
    builder.Property(r => r.Id).ValueGeneratedOnAdd();
    builder.Property(r => r.CreatedAt).ValueGeneratedOnAdd();
    builder.Property(r => r.UpdatedAt).ValueGeneratedOnAddOrUpdate();

    builder.HasOne(r => r.Movie).WithMany(m => m.Reviews).OnDelete(DeleteBehavior.Cascade);
  }
}
