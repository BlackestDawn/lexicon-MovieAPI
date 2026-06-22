using MovieAPI.Domain.Entities;

namespace MovieAPI.Infrastructure.Interfaces;

public interface IGenreRepository : IRepositoryBase<Genre>
{
  Task<IEnumerable<Genre>> GetGenresAsync(CancellationToken cancellationToken);
  Task<Genre?> GetGenreAsync(Guid id, bool includeMovies, CancellationToken cancellationToken);
}
