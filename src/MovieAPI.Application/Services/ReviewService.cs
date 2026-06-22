using System.Runtime.CompilerServices;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using MovieAPI.Application.Helpers;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Application.validators;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Interfaces;
using MovieAPI.Infrastructure.Models;
using MovieAPI.Infrastructure.Services;

namespace MovieAPI.Application.Services;

public class ReviewService(
  IReviewRepository repository,
  IMovieRepository movieRepository,
  IMapper mapper,
  IValidator<ReviewForCreationDto> createValidator,
  IValidator<ReviewForUpdateDto> updateValidator) : IReviewService
{
  public async Task<ReviewCreationResult> Create(Guid movieId, ReviewForCreationDto newReview, CancellationToken token = default)
  {
    var validationResult = createValidator.Validate(newReview);

    if (!validationResult.IsValid)
    {
      return ReviewCreationResult.Failed(new ValidationException(validationResult.Errors));
    }

    if (!await movieRepository.ExistsAsync(movieId, token))
    {
      return ReviewCreationResult.Failed(new ArgumentException($"Movie '{movieId}' not found"));
    }

    var reviewEntity = mapper.Map<Review>(newReview);
    reviewEntity.MovieId = movieId;

    await repository.AddAsync(reviewEntity, token);
    await repository.SaveChangesAsync(token);

    return ReviewCreationResult.Successful(mapper.Map<ReviewDto>(reviewEntity));
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

  public async Task<ReviewDto?> GetOne(Guid movieId, Guid id, CancellationToken token = default)
  {
    var result = await repository.GetReviewAsync(movieId, id, token);

    if (result == null)
    {
      return null;
    }

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

  public async Task<(bool, string?)> Update(Guid movieId, Guid id, ReviewForUpdateDto updatedReview, CancellationToken token = default)
  {
    if (!await movieRepository.ExistsAsync(movieId, token))
    {
      return (false, $"Movie '{movieId}' not found");
    }

    var entity = await repository.GetReviewAsync(movieId, id, token);
    if (entity == null)
    {
      return (false, $"Review '{id}' not found");
    }

    return await ApplyUpdateAsync(entity, updatedReview, token);
  }

  public async Task<(bool, string?)> Update(Guid movieId, Guid id, JsonPatchDocument<ReviewForUpdateDto> patchDocument, CancellationToken token = default)
  {
    if (!await movieRepository.ExistsAsync(movieId, token))
    {
      return (false, $"Movie '{movieId}' not found");
    }

    var entity = await repository.GetReviewAsync(movieId, id, token);
    if (entity == null)
    {
      return (false, $"Review '{id}' not found");
    }

    var dto = mapper.Map<ReviewForUpdateDto>(entity);
    patchDocument.ApplyTo(dto);

    return await ApplyUpdateAsync(entity, dto, token);
  }

  private async Task<(bool, string?)> ApplyUpdateAsync(Review entity, ReviewForUpdateDto updatedReview, CancellationToken token)
  {
    var validationResult = updateValidator.Validate(updatedReview);

    if (!validationResult.IsValid)
    {
      return (false, new ValidationException(validationResult.Errors).Message);
    }

    entity.AuthorName = updatedReview.AuthorName;
    entity.Body = updatedReview.Body;
    entity.Score = updatedReview.Score;

    await repository.SaveChangesAsync(token);
    return (true, null);
  }
}
