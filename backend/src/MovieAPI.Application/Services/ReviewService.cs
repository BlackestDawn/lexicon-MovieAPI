using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
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
  UserManager<ApplicationUser> userManager,
  IMapper mapper,
  IValidator<ReviewForChangeDto> validator) : IReviewService
{
  public async Task<ReviewDto> Create(Guid movieId, ReviewForChangeDto newReview, Guid currentUserId, CancellationToken token = default)
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
    reviewEntity.UserId = currentUserId;
    reviewEntity.AuthorName = await ResolveAuthorNameAsync(currentUserId);

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

  public async Task Remove(Guid movieId, Guid id, Guid currentUserId, bool canModerate, CancellationToken token = default)
  {
    var entity = await repository.GetReviewAsync(movieId, id, token);
    if (entity == null)
    {
      return;
    }

    EnsureOwnerOrModerator(entity, currentUserId, canModerate);

    repository.Delete(entity);
    await repository.SaveChangesAsync(token);
  }

  public async Task Update(Guid movieId, Guid id, ReviewForChangeDto updatedReview, Guid currentUserId, bool canModerate, CancellationToken token = default)
  {
    if (!await movieRepository.ExistsAsync(movieId, token))
    {
      throw new NotFoundException($"Movie '{movieId}' not found");
    }

    var entity = await repository.GetReviewAsync(movieId, id, token) ?? throw new NotFoundException($"Review '{id}' not found");
    EnsureOwnerOrModerator(entity, currentUserId, canModerate);
    await ApplyUpdateAsync(entity, updatedReview, token);
  }

  public async Task Update(Guid movieId, Guid id, JsonPatchDocument<ReviewForChangeDto> patchDocument, Guid currentUserId, bool canModerate, CancellationToken token = default)
  {
    if (!await movieRepository.ExistsAsync(movieId, token))
    {
      throw new NotFoundException($"Movie '{movieId}' not found");
    }

    var entity = await repository.GetReviewAsync(movieId, id, token) ?? throw new NotFoundException($"Review '{id}' not found");
    EnsureOwnerOrModerator(entity, currentUserId, canModerate);

    var dto = mapper.Map<ReviewForChangeDto>(entity);
    patchDocument.ApplyTo(dto);

    await ApplyUpdateAsync(entity, dto, token);
  }

  private static void EnsureOwnerOrModerator(Review entity, Guid currentUserId, bool canModerate)
  {
    if (canModerate)
    {
      return;
    }

    if (entity.UserId != currentUserId)
    {
      throw new ForbiddenException("You can only modify your own review");
    }
  }

  // Reviews seeded as sample/demo data have no UserId to resolve a name from, so their
  // AuthorName (freeform, set at seed time) is left as-is - only account-linked reviews
  // get their AuthorName kept in sync with the owner's current display name.
  private async Task<string> ResolveAuthorNameAsync(Guid userId)
  {
    var user = await userManager.FindByIdAsync(userId.ToString())
      ?? throw new NotFoundException($"User '{userId}' not found");

    return user.DisplayName;
  }

  private async Task ApplyUpdateAsync(Review entity, ReviewForChangeDto updatedReview, CancellationToken token)
  {
    var validationResult = await validator.ValidateAsync(updatedReview, token);

    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }

    if (entity.UserId.HasValue)
    {
      entity.AuthorName = await ResolveAuthorNameAsync(entity.UserId.Value);
    }

    entity.Body = updatedReview.Body;
    entity.Score = updatedReview.Score;

    await repository.SaveChangesAsync(token);
  }
}
