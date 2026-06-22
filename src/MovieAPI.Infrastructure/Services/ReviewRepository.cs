using Microsoft.EntityFrameworkCore;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Interfaces;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Infrastructure.Services;

public class ReviewRepository(AppDbContext context) : RepositoryBase<Review>(context), IReviewRepository
{
  protected override DbSet<Review> Set => Context.Reviews;

  public async Task<(IEnumerable<Review>, PaginationMetadata?)> GetReviewsForMovieAsync(Guid movieId, ReviewSearchParams searchParams, int page, int pageSize, CancellationToken cancellationToken)
  {
    var query = Context.Reviews.AsQueryable();
    query = query.Where(r => r.MovieId == movieId);

    if (!string.IsNullOrWhiteSpace(searchParams.Search))
    {
      query = query.Where(r => r.Body.Contains(searchParams.Search));
    }

    if (searchParams.MinScore.HasValue)
    {
      query = query.Where(r => r.Score >= (int)searchParams.MinScore);
    }

    if (searchParams.MaxScore.HasValue)
    {
      query = query.Where(r => r.Score <= (int)searchParams.MaxScore);
    }

    var totalCount = await query.CountAsync(cancellationToken);
    var pagination = new PaginationMetadata(totalCount, pageSize, page);

    var reviews = await query
      .OrderBy(r => r.CreatedAt)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync(cancellationToken);

    return (reviews, pagination);
  }

  public async Task<Review?> GetReviewAsync(Guid movieId, Guid reviewId, CancellationToken cancellationToken)
  {
    return await Context.Reviews
      .FirstOrDefaultAsync(r => r.MovieId == movieId && r.Id == reviewId, cancellationToken);
  }

}
