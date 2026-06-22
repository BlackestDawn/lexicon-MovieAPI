using Microsoft.EntityFrameworkCore;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Interfaces;

namespace MovieAPI.Infrastructure.Services;

public class GenreRepository(AppDbContext context) : RepositoryBase<Genre>(context), IGenreRepository
{
  protected override DbSet<Genre> Set => Context.Genres;

  public async Task<IEnumerable<Genre>> GetGenresAsync(CancellationToken cancellationToken)
  {
    return await Context.Genres.AsNoTracking().ToListAsync(cancellationToken);
  }

  public async Task<Genre?> GetGenreAsync(Guid id, bool includeMovies, CancellationToken cancellationToken)
  {
    var query = Context.Genres.AsQueryable();

    if (includeMovies)
    {
      query = query.Include(g => g.MovieGenres).ThenInclude(gm => gm.Movie);
    }

    return await query.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
  }
}
