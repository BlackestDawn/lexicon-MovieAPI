using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MovieAPI.Domain.Entities;
using MovieAPI.Domain.Interfaces;

namespace MovieAPI.Infrastructure;

public class UpdatedAtInterceptor : SaveChangesInterceptor
{
  public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
  {
    SetUpdatedAt(eventData.Context);
    return base.SavingChanges(eventData, result);
  }

  public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
  {
    SetUpdatedAt(eventData.Context);
    return base.SavingChangesAsync(eventData, result, ct);
  }

  private static void SetUpdatedAt(DbContext? context)
  {
    if (context is null) return;

    foreach (var entry in context.ChangeTracker.Entries<MovieDetail>())
    {
      if (entry.State is EntityState.Modified or EntityState.Added or EntityState.Deleted)
      {
        var movie = entry.Entity.Movie
          ?? context.ChangeTracker.Entries<Movie>()
            .FirstOrDefault(e => e.Entity.Id == entry.Entity.MovieId)?.Entity;

        if (movie is not null)
        {
          var movieEntry = context.Entry(movie);
          if (movieEntry.State == EntityState.Unchanged)
          {
            movieEntry.State = EntityState.Modified;
          }
        }
      }
    }

    foreach (var entry in context.ChangeTracker.Entries<ITrackable>())
    {
      if (entry.State == EntityState.Modified)
      {
        entry.Entity.UpdatedAt = DateTime.UtcNow;
      }
    }
  }
}
