using Microsoft.EntityFrameworkCore;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Interfaces;

namespace MovieAPI.Infrastructure.Services;

public class GenreRepository(AppDbContext context) : RepositoryBase<Genre>(context), IGenreRepository
{
  protected override DbSet<Genre> Set => Context.Genres;

  public async Task<IEnumerable<Genre>> GetGenresAsync(CancellationToken cancellationToken)
  {
    return await Context.Genres.ToListAsync(cancellationToken);
  }

  public async Task<IEnumerable<Genre>> GetGenresReadOnlyAsync(CancellationToken cancellationToken)
  {
    return await Context.Genres.AsNoTracking().ToListAsync(cancellationToken);
  }

  public async Task<Genre?> GetGenreAsync(Guid id, bool includeMovies, CancellationToken cancellationToken)
  {
    return await GetGenreInternalAsync(id, includeMovies, false, cancellationToken);
  }

  public async Task<Genre?> GetGenreReadOnlyAsync(Guid id, bool includeMovies, CancellationToken cancellationToken)
  {
    return await GetGenreInternalAsync(id, includeMovies, true, cancellationToken);
  }

  private async Task<Genre?> GetGenreInternalAsync(Guid id, bool includeMovies, bool readOnly, CancellationToken cancellationToken)
  {
    var query = Context.Genres.AsQueryable();

    if (readOnly)
    {
      query = query.AsNoTracking();
    }

    if (includeMovies)
    {
      query = query.Include(g => g.MovieGenres).ThenInclude(gm => gm.Movie);
    }

    return await query.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
  }
}
