using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Infrastructure.FluentConfigs;

public class CastCrewConfig : IEntityTypeConfiguration<CastCrew>
{
  public void Configure(EntityTypeBuilder<CastCrew> builder)
  {
    builder.Property(c => c.Id)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("NEWID()");
    builder.Property(c => c.CreatedAt)
      .ValueGeneratedOnAdd()
      .HasDefaultValueSql("SYSUTCDATETIME()");
    builder.Property(c => c.UpdatedAt)
      .ValueGeneratedOnAddOrUpdate()
      .HasDefaultValueSql("SYSUTCDATETIME()");

    builder.ToTable(tb => tb.HasTrigger("TR_CastCrews_UpdatedAt"));

    builder.HasKey(c => new { c.MovieId, c.PersonId });

    builder.HasOne(c => c.Movie)
      .WithMany(m => m.CastCrews)
      .HasForeignKey(c => c.MovieId);
    builder.HasOne(c => c.Person)
      .WithMany(p => p.CastCrews)
      .HasForeignKey(c => c.PersonId);
  }
}
