using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Infrastructure.Interfaces;

public interface IMovieRepository : IRepositoryBase<Movie>
{
  Task<(IEnumerable<Movie>, PaginationMetadata?)> GetMoviesAsync(MovieSearchParams searchParams, int page, int pageSize, CancellationToken cancellationToken);
  Task<(IEnumerable<Movie>, PaginationMetadata?)> GetMoviesReadOnlyAsync(MovieSearchParams searchParams, int page, int pageSize, CancellationToken cancellationToken);
  Task<Movie?> GetMovieAsync(Guid id, bool includePeople, CancellationToken cancellationToken);
  Task<Movie?> GetMovieReadOnlyAsync(Guid id, bool includePeople, CancellationToken cancellationToken);
}
