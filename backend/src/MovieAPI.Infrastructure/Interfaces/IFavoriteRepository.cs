using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Infrastructure.Interfaces;

public interface IFavoriteRepository
{
  Task<bool> ExistsAsync(Guid userId, Guid movieId, CancellationToken cancellationToken);
  Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
  Task AddAsync(Guid userId, Guid movieId, CancellationToken cancellationToken);
  Task RemoveAsync(Guid userId, Guid movieId, CancellationToken cancellationToken);
  Task<(IEnumerable<MovieListItem>, PaginationMetadata?)> GetForUserReadOnlyAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken);
}
