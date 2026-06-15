using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Infrastructure.FluentConfigs;

public class MovieDetailConfig : IEntityTypeConfiguration<MovieDetail>
{
  public void Configure(EntityTypeBuilder<MovieDetail> builder)
  {
    builder.Property(m => m.Id)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("NEWID()");
    builder.Property(m => m.CreatedAt)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("SYSUTCDATETIME()");
    builder.Property(m => m.UpdatedAt)
      .ValueGeneratedOnAddOrUpdate()
      .HasDefaultValueSql("SYSUTCDATETIME()");

    builder.ToTable(tb => tb.HasTrigger("TR_MovieDetails_UpdatedAt"));

    builder.HasOne(d => d.Movie)
      .WithOne(m => m.Details)
      .HasForeignKey<MovieDetail>(d => d.MovieId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
