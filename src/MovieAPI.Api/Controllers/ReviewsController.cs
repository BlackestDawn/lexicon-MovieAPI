using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using MovieAPI.Api.Extensions;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Constants;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Api.Controllers;

[ApiController]
[Route("api/movies/{movieId}/reviews")]
public class ReviewsController(IReviewService service, IOutputCacheStore cacheStore) : ControllerBase
{
  [HttpGet]
  [OutputCache(PolicyName = "CatalogCache")]
  public async Task<IActionResult> GetReviews(Guid movieId,
    string? search, int? minScore, int? maxScore,
    int? page, int? pageSize,
    CancellationToken cancellationToken = default)
  {
    var (result, pagination) = await service.GetMany(movieId,
      new ReviewSearchParams(search, minScore, maxScore),
      page, pageSize, cancellationToken);

    if (pagination != null)
    {
      Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(pagination));
    }

    return Ok(result);
  }

  [HttpGet("{id}", Name = "GetReview")]
  [OutputCache(PolicyName = "CatalogCache")]
  public async Task<IActionResult> GetReview(Guid movieId, Guid id, CancellationToken cancellationToken = default)
  {
    var result = await service.GetOne(movieId, id, cancellationToken);
    return Ok(result);
  }

  [Authorize]
  [HttpPost]
  public async Task<IActionResult> CreateReview(Guid movieId, ReviewForChangeDto newReview,
    CancellationToken cancellationToken = default)
  {
    var result = await service.Create(movieId, newReview, User.GetUserId(), cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return CreatedAtRoute("GetReview", new {movieId, result.Id}, result);
  }

  [Authorize]
  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateReview(Guid movieId, Guid id, ReviewForChangeDto updatedReview,
    CancellationToken cancellationToken = default)
  {
    await service.Update(movieId, id, updatedReview, User.GetUserId(), CanModerate, cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return NoContent();
  }

  [Authorize]
  [HttpPatch("{id}")]
  public async Task<IActionResult> PatchReview(Guid movieId, Guid id, JsonPatchDocument<ReviewForChangeDto> patch,
    CancellationToken cancellationToken = default)
  {
    await service.Update(movieId, id, patch, User.GetUserId(), CanModerate, cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return NoContent();
  }

  [Authorize]
  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteReview(Guid movieId, Guid id, CancellationToken cancellationToken = default)
  {
    await service.Remove(movieId, id, User.GetUserId(), CanModerate, cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return NoContent();
  }

  private bool CanModerate => User.IsInRole(Roles.Moderator) || User.IsInRole(Roles.Administrator);
}
