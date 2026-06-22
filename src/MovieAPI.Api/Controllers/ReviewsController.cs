using System.Text.Json;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.Mvc;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Api.Controllers;

[ApiController]
[Route("api/movies/{movieId}/reviews")]
public class ReviewsController(IReviewService service) : ControllerBase
{
  [HttpGet]
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
  public async Task<IActionResult> GetReview(Guid movieId, Guid id, CancellationToken cancellationToken = default)
  {
    var result = await service.GetOne(movieId, id, cancellationToken);

    if (result == null)
    {
      return NotFound();
    }

    return Ok(result);
  }

  [HttpPost]
  public async Task<IActionResult> CreateReview(Guid movieId, ReviewForChangeDto newReview,
    CancellationToken cancellationToken = default)
  {
    var result = await service.Create(movieId, newReview, cancellationToken);

    if (!result.Success)
    {
      return BadRequest(result.Error!.Message);
    }

    return CreatedAtRoute("GetReview", new {movieId, result.Review!.Id}, result.Review);
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateReview(Guid movieId, Guid id, ReviewForChangeDto updatedReview,
    CancellationToken cancellationToken = default)
  {
    var (success, message) = await service.Update(movieId, id, updatedReview, cancellationToken);

    if (!success)
    {
      return BadRequest(message);
    }

    return NoContent();
  }

  [HttpPatch("{id}")]
  public async Task<IActionResult> PatchReview(Guid movieId, Guid id, JsonPatchDocument<ReviewForChangeDto> patch,
    CancellationToken cancellationToken = default)
  {
    var (success, message) = await service.Update(movieId, id, patch, cancellationToken);

    if (!success)
    {
      return BadRequest(message);
    }

    return NoContent();
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteReview(Guid movieId, Guid id, CancellationToken cancellationToken = default)
  {
    await service.Remove(movieId, id, cancellationToken);
    return NoContent();
  }
}
