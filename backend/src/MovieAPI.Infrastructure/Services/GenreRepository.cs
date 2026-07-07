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

  public Task<Genre?> GetGenreAsync(Guid id, CancellationToken cancellationToken)
  {
    return Context.Genres.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
  }

  public Task<Genre?> GetGenreReadOnlyAsync(Guid id, CancellationToken cancellationToken)
  {
    return Context.Genres.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
  }
}
