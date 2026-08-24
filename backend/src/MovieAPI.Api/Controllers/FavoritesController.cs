using System.Text.Json;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieAPI.Api.Extensions;
using MovieAPI.Application.Interfaces;

namespace MovieAPI.Api.Controllers;

/// <summary>
/// Controller for handling a logged-in user's favorited movies
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/favorites")]
[Authorize]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[ApiVersion("3.0")]
[ApiVersion("3.1")]
public class FavoritesController(IFavoriteService service) : ControllerBase
{
  /// <summary>
  /// Fetch a paginated list of the current user's favorited movies
  /// </summary>
  /// <param name="page">Page to view, defaults to 1</param>
  /// <param name="pageSize">Amount per page, defaults to 10</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>List of MovieDto objects</returns>
  [HttpGet]
  public async Task<IActionResult> GetMine(int? page, int? pageSize, CancellationToken cancellationToken = default)
  {
    var (result, pagination) = await service.GetForUser(User.GetUserId(), page, pageSize, cancellationToken);

    if (pagination != null)
    {
      Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(pagination));
    }

    return Ok(result);
  }

  /// <summary>
  /// Check whether the current user has favorited a specific movie
  /// </summary>
  /// <param name="movieId">GUID of movie</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>HTTP code: 200 if favorited, 404 if not</returns>
  [HttpGet("{movieId}")]
  public async Task<IActionResult> GetOne(Guid movieId, CancellationToken cancellationToken = default)
  {
    var exists = await service.Exists(movieId, User.GetUserId(), cancellationToken);
    return exists ? Ok() : NotFound();
  }

  /// <summary>
  /// Favorite a movie, idempotent
  /// </summary>
  /// <param name="movieId">GUID of movie</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>HTTP code: 204</returns>
  [HttpPost("{movieId}")]
  public async Task<IActionResult> Add(Guid movieId, CancellationToken cancellationToken = default)
  {
    await service.Add(movieId, User.GetUserId(), cancellationToken);
    return NoContent();
  }

  /// <summary>
  /// Unfavorite a movie, idempotent
  /// </summary>
  /// <param name="movieId">GUID of movie</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>HTTP code: 204</returns>
  [HttpDelete("{movieId}")]
  public async Task<IActionResult> Remove(Guid movieId, CancellationToken cancellationToken = default)
  {
    await service.Remove(movieId, User.GetUserId(), cancellationToken);
    return NoContent();
  }
}
