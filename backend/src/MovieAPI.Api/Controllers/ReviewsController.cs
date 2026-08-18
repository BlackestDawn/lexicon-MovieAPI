using System.Text.Json;
using Asp.Versioning;
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

/// <summary>
/// Controller for handling user reviews
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/movies/{movieId}/reviews")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[ApiVersion("3.0")]
[ApiVersion("3.1")]
public class ReviewsController(IReviewService service, IOutputCacheStore cacheStore) : ControllerBase
{
  /// <summary>
  /// Fetch a paginated and filterable list of reviews for a specific movie
  /// </summary>
  /// <param name="movieId">GUID of movie</param>
  /// <param name="search">Free-text search of review content</param>
  /// <param name="minScore">Filter on minimum score</param>
  /// <param name="maxScore">Filter on maximum score</param>
  /// <param name="page">Page to view, defaults to 1</param>
  /// <param name="pageSize">Amount per page, defaults to 10</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>List of ReviewDto objects</returns>
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

  /// <summary>
  /// Get a review belonging to a specific movie
  /// </summary>
  /// <param name="movieId">GUID of movie</param>
  /// <param name="id">GUID of review</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>ReviewDto object</returns>
  [HttpGet("{id}", Name = "GetReview")]
  [OutputCache(PolicyName = "CatalogCache")]
  public async Task<IActionResult> GetReview(Guid movieId, Guid id, CancellationToken cancellationToken = default)
  {
    var result = await service.GetOne(movieId, id, cancellationToken);
    return Ok(result);
  }

  /// <summary>
  /// Create a new review for specified movie, only available to logged in users
  /// </summary>
  /// <param name="movieId">GUID of movie</param>
  /// <param name="newReview">ReviewForChangeDto object</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>ReviewDto object and route to said object</returns>
  [Authorize]
  [HttpPost]
  public async Task<IActionResult> CreateReview(Guid movieId, ReviewForChangeDto newReview,
    CancellationToken cancellationToken = default)
  {
    var result = await service.Create(movieId, newReview, User.GetUserId(), cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return CreatedAtRoute("GetReview", new { movieId, result.Id }, result);
  }

  /// <summary>
  /// Full object update of review, only available to logged in users
  /// </summary>
  /// <param name="movieId">GUID of movie</param>
  /// <param name="id">GUID of review</param>
  /// <param name="updatedReview">ReviewForChangeDto object</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>HTTP code: 204</returns>
  [Authorize]
  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateReview(Guid movieId, Guid id, ReviewForChangeDto updatedReview,
    CancellationToken cancellationToken = default)
  {
    await service.Update(movieId, id, updatedReview, User.GetUserId(), CanModerate, cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return NoContent();
  }

  /// <summary>
  /// Update review through JSON patch, only available to logged in users
  /// </summary>
  /// <param name="movieId">GUID of movie</param>
  /// <param name="id">GUID of review</param>
  /// <param name="patch">JSON patch document with changes</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>HTTP code: 204</returns>
  [Authorize]
  [HttpPatch("{id}")]
  public async Task<IActionResult> PatchReview(Guid movieId, Guid id, JsonPatchDocument<ReviewForChangeDto> patch,
    CancellationToken cancellationToken = default)
  {
    await service.Update(movieId, id, patch, User.GetUserId(), CanModerate, cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return NoContent();
  }

  /// <summary>
  /// Remove a review, only available to logged in users
  /// </summary>
  /// <param name="movieId">GUID of movie</param>
  /// <param name="id">GUID of review</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>HTTP code: 204</returns>
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
