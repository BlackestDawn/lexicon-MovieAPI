using AutoMapper;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using MovieAPI.Application.Helpers;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Infrastructure.Models;
using MovieAPI.Infrastructure.Services;

namespace MovieAPI.Application.Services;

public class ReviewService(
  IMovieRepository repository,
  IMapper mapper) : IReviewService
{
  public Task<ReviewCreationResult> Create(ReviewForCreationDto newReview, CancellationToken token = default)
  {
    throw new NotImplementedException();
  }

  public async Task<(IEnumerable<ReviewDto>, PaginationMetadata?)> GetMany(Guid movieId, ReviewSearchParams searchParams, int? page, int? pageSize, CancellationToken token = default)
  {
    if (page == null || page < DefaultValues.Page)
    {
      page = DefaultValues.Page;
    }

    if (pageSize == null || pageSize <= 0)
    {
      pageSize = DefaultValues.PageSize;
    }

    var (result, pagination) = await repository.GetReviewsForMovieAsync(movieId, searchParams, (int)page, (int)pageSize, token);

    return (mapper.Map<IEnumerable<ReviewDto>>(result), pagination);
  }

  public async Task<ReviewDto?> GetOne(Guid movieId, Guid id, CancellationToken token = default)
  {
    var result = await repository.GetReviewAsync(movieId, id, token);

    if (result == null)
    {
      return null;
    }

    return mapper.Map<ReviewDto>(result);
  }

  public Task Remove(Guid id, CancellationToken token = default)
  {
    throw new NotImplementedException();
  }

  public Task<(bool, string?)> Update(Guid id, ReviewForUpdateDto updatedReview, CancellationToken token = default)
  {
    throw new NotImplementedException();
  }

  public Task<(bool, string?)> Update(Guid id, JsonPatchDocument<ReviewForUpdateDto> patchDocument, CancellationToken token = default)
  {
    throw new NotImplementedException();
  }
}
