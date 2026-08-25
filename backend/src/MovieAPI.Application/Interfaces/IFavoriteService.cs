using MovieAPI.Application.Models;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Application.Interfaces;

public interface IFavoriteService
{
  Task<bool> Exists(Guid movieId, Guid userId, CancellationToken token = default);
  Task Add(Guid movieId, Guid userId, CancellationToken token = default);
  Task Remove(Guid movieId, Guid userId, CancellationToken token = default);
  Task<(IEnumerable<MovieDto>, PaginationMetadata?)> GetForUser(Guid userId, int? page, int? pageSize, CancellationToken token = default);
}
