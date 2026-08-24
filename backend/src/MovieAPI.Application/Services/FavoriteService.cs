using AutoMapper;
using MovieAPI.Application.Exceptions;
using MovieAPI.Application.Helpers;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Infrastructure.Interfaces;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Application.Services;

public class FavoriteService(
  IFavoriteRepository repository,
  IMovieRepository movieRepository,
  IMapper mapper) : IFavoriteService
{
  public Task<bool> Exists(Guid movieId, Guid userId, CancellationToken token = default)
  {
    return repository.ExistsAsync(userId, movieId, token);
  }

  public async Task Add(Guid movieId, Guid userId, CancellationToken token = default)
  {
    if (!await movieRepository.ExistsAsync(movieId, token))
    {
      throw new NotFoundException($"Movie '{movieId}' not found");
    }

    // Idempotent - favoriting an already-favorited movie is a no-op rather than an
    // error, so the frontend toggle doesn't need to track state to avoid duplicates.
    if (await repository.ExistsAsync(userId, movieId, token))
    {
      return;
    }

    await repository.AddAsync(userId, movieId, token);
    await repository.SaveChangesAsync(token);
  }

  public async Task Remove(Guid movieId, Guid userId, CancellationToken token = default)
  {
    await repository.RemoveAsync(userId, movieId, token);
    await repository.SaveChangesAsync(token);
  }

  public async Task<(IEnumerable<MovieDto>, PaginationMetadata?)> GetForUser(Guid userId, int? page, int? pageSize, CancellationToken token = default)
  {
    if (page == null || page < DefaultValues.Page)
    {
      page = DefaultValues.Page;
    }
    if (pageSize == null || pageSize <= 0)
    {
      pageSize = DefaultValues.PageSize;
    }

    var (result, pagination) = await repository.GetForUserReadOnlyAsync(userId, (int)page, (int)pageSize, token);

    var movies = result.Select(item =>
    {
      var dto = mapper.Map<MovieDto>(item.Movie);
      dto.AverageRating = item.AverageRating;
      return dto;
    });

    return (movies, pagination);
  }
}
