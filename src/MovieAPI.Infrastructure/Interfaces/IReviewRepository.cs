using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Infrastructure.Interfaces;

public interface IReviewRepository : IRepositoryBase<Review>
{
  Task<(IEnumerable<Review>, PaginationMetadata?)> GetReviewsForMovieAsync(Guid movieId, ReviewSearchParams searchParams, int page, int pageSize, CancellationToken cancellationToken);
  Task<(IEnumerable<Review>, PaginationMetadata?)> GetReviewsForMovieReadOnlyAsync(Guid movieId, ReviewSearchParams searchParams, int page, int pageSize, CancellationToken cancellationToken);
  Task<Review?> GetReviewAsync(Guid movieId, Guid reviewId, CancellationToken cancellationToken);
}
