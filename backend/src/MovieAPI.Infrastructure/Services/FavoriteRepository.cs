using Microsoft.EntityFrameworkCore;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Interfaces;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Infrastructure.Services;

public class FavoriteRepository(AppDbContext context) : IFavoriteRepository
{
  public Task<bool> ExistsAsync(Guid userId, Guid movieId, CancellationToken cancellationToken)
  {
    return context.Favorites.AnyAsync(f => f.UserId == userId && f.MovieId == movieId, cancellationToken);
  }

  public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
  {
    return await context.SaveChangesAsync(cancellationToken) > 0;
  }

  public async Task AddAsync(Guid userId, Guid movieId, CancellationToken cancellationToken)
  {
    await context.Favorites.AddAsync(new Favorite { UserId = userId, MovieId = movieId }, cancellationToken);
  }

  public async Task RemoveAsync(Guid userId, Guid movieId, CancellationToken cancellationToken)
  {
    var favorite = await context.Favorites
      .FirstOrDefaultAsync(f => f.UserId == userId && f.MovieId == movieId, cancellationToken);

    if (favorite != null)
    {
      context.Favorites.Remove(favorite);
    }
  }

  public async Task<(IEnumerable<MovieListItem>, PaginationMetadata?)> GetForUserReadOnlyAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken)
  {
    var query = context.Favorites
      .AsNoTracking()
      .Where(f => f.UserId == userId);

    var totalCount = await query.CountAsync(cancellationToken);
    var pagination = new PaginationMetadata(totalCount, pageSize, page);

    var favorites = await query
      .OrderByDescending(f => f.CreatedAt)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .Include(f => f.Movie).ThenInclude(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
      .Select(f => new MovieListItem(f.Movie, (decimal)(f.Movie.Reviews.Average(r => (double?)r.Score) ?? 0)))
      .ToListAsync(cancellationToken);

    return (favorites, pagination);
  }
}
