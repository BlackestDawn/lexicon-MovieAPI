using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MovieAPI.Application.Interfaces;
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
  public async Task<IActionResult> GetReview(Guid movieId, Guid id, CancellationToken cancellationToken)
  {
    var result = await service.GetOne(movieId, id, cancellationToken);

    if (result == null)
    {
      return NotFound();
    }

    return Ok(result);
  }
}
