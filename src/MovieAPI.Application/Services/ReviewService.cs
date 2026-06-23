using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using MovieAPI.Application.Exceptions;
using MovieAPI.Application.Helpers;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Interfaces;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Application.Services;

public class ReviewService(
  IReviewRepository repository,
  IMovieRepository movieRepository,
  IMapper mapper,
  IValidator<ReviewForChangeDto> validator) : IReviewService
{
  public async Task<ReviewDto> Create(Guid movieId, ReviewForChangeDto newReview, CancellationToken token = default)
  {
    // movieId is part of the route, not the body, so a missing movie is treated like a bad route segment
    // and checked before bothering to validate the body
    if (!await movieRepository.ExistsAsync(movieId, token))
    {
      throw new NotFoundException($"Movie '{movieId}' not found");
    }

    var validationResult = await validator.ValidateAsync(newReview, token);

    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }

    var reviewEntity = mapper.Map<Review>(newReview);
    reviewEntity.MovieId = movieId;

    await repository.AddAsync(reviewEntity, token);
    await repository.SaveChangesAsync(token);

    return mapper.Map<ReviewDto>(reviewEntity);
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

    var (result, pagination) = await repository.GetReviewsForMovieReadOnlyAsync(movieId, searchParams, (int)page, (int)pageSize, token);

    return (mapper.Map<IEnumerable<ReviewDto>>(result), pagination);
  }

  public async Task<ReviewDto> GetOne(Guid movieId, Guid id, CancellationToken token = default)
  {
    var result = await repository.GetReviewReadOnlyAsync(movieId, id, token) ?? throw new NotFoundException($"Review '{id}' not found");
    return mapper.Map<ReviewDto>(result);
  }

  public async Task Remove(Guid movieId, Guid id, CancellationToken token = default)
  {
    var entity = await repository.GetReviewAsync(movieId, id, token);
    if (entity == null)
    {
      return;
    }

    repository.Delete(entity);
    await repository.SaveChangesAsync(token);
  }

  public async Task Update(Guid movieId, Guid id, ReviewForChangeDto updatedReview, CancellationToken token = default)
  {
    if (!await movieRepository.ExistsAsync(movieId, token))
    {
      throw new NotFoundException($"Movie '{movieId}' not found");
    }

    var entity = await repository.GetReviewAsync(movieId, id, token) ?? throw new NotFoundException($"Review '{id}' not found");
    await ApplyUpdateAsync(entity, updatedReview, token);
  }

  public async Task Update(Guid movieId, Guid id, JsonPatchDocument<ReviewForChangeDto> patchDocument, CancellationToken token = default)
  {
    if (!await movieRepository.ExistsAsync(movieId, token))
    {
      throw new NotFoundException($"Movie '{movieId}' not found");
    }

    var entity = await repository.GetReviewAsync(movieId, id, token) ?? throw new NotFoundException($"Review '{id}' not found");
    var dto = mapper.Map<ReviewForChangeDto>(entity);
    patchDocument.ApplyTo(dto);

    await ApplyUpdateAsync(entity, dto, token);
  }

  private async Task ApplyUpdateAsync(Review entity, ReviewForChangeDto updatedReview, CancellationToken token)
  {
    var validationResult = await validator.ValidateAsync(updatedReview, token);

    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }

    entity.AuthorName = updatedReview.AuthorName;
    entity.Body = updatedReview.Body;
    entity.Score = updatedReview.Score;

    await repository.SaveChangesAsync(token);
  }
}
