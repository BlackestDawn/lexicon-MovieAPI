using MovieAPI.Domain.Entities;

namespace MovieAPI.Infrastructure.Interfaces;

public interface IGenreRepository : IRepositoryBase<Genre>
{
  Task<IEnumerable<Genre>> GetGenresAsync(CancellationToken cancellationToken);
  Task<IEnumerable<Genre>> GetGenresReadOnlyAsync(CancellationToken cancellationToken);
  Task<Genre?> GetGenreAsync(Guid id, bool includeMovies, CancellationToken cancellationToken);
  Task<Genre?> GetGenreReadOnlyAsync(Guid id, bool includeMovies, CancellationToken cancellationToken);
}
