using System.Text.Json;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Constants;

namespace MovieAPI.Api.Controllers;

/// <summary>
/// Controller for handling genres
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/genres")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class GenresController(IGenreService service, IOutputCacheStore cacheStore) : ControllerBase
{
  /// <summary>
  /// Fetch list of all genres
  /// </summary>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>List of GenreDto objects</returns>
  [HttpGet]
  [OutputCache(PolicyName = "CatalogCache")]
  public async Task<IActionResult> GetGenres(CancellationToken cancellationToken = default)
  {
    var result = await service.GetMany(cancellationToken);
    return Ok(result);
  }

  /// <summary>
  /// Get single genre with optionally a list of movies for that genre
  /// </summary>
  /// <param name="id">GUID of genre</param>
  /// <param name="page">Page to display, defaults to 1</param>
  /// <param name="pageSize">Amount per page, defaults to 10</param>
  /// <param name="includeMovies">Whether to include movies, defaults to true</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>GenreExtendedDto object</returns>
  [HttpGet("{id}", Name = "GetGenre")]
  [OutputCache(PolicyName = "CatalogCache")]
  public async Task<IActionResult> GetGenre(Guid id, int? page, int? pageSize, bool includeMovies = true, CancellationToken cancellationToken = default)
  {
    var (result, pagination) = await service.GetOne(id, includeMovies, page, pageSize, cancellationToken);

    if (pagination != null)
    {
      Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(pagination));
    }

    return Ok(result);
  }

  /// <summary>
  /// Create new genre, only available to moderators and above
  /// </summary>
  /// <param name="newGenre">GenreForChangeDto object</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>GenreDto object and route to said object</returns>
  [Authorize(Roles = Roles.ModeratorAndAbove)]
  [HttpPost]
  public async Task<IActionResult> CreateGenre(GenreForChangeDto newGenre, CancellationToken cancellationToken = default)
  {
    var result = await service.Create(newGenre, cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return CreatedAtRoute("GetGenre", new { result.Id }, result);
  }

  /// <summary>
  /// Full object update of existing genre, only available to moderators and above
  /// </summary>
  /// <param name="id">GUID of genre</param>
  /// <param name="updatedGenre">GenreForChangeDto object</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>HTTP code: 204</returns>
  [Authorize(Roles = Roles.ModeratorAndAbove)]
  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateGenre(Guid id, GenreForChangeDto updatedGenre, CancellationToken cancellationToken = default)
  {
    await service.Update(id, updatedGenre, cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return NoContent();
  }

  /// <summary>
  /// Update existing genre through JSON patch, only available to moderators and above
  /// </summary>
  /// <param name="id">GUID of genre</param>
  /// <param name="patch">JSON patch document of changes</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>HTTP code: 204</returns>
  [Authorize(Roles = Roles.ModeratorAndAbove)]
  [HttpPatch("{id}")]
  public async Task<IActionResult> PatchGenre(Guid id, JsonPatchDocument<GenreForChangeDto> patch, CancellationToken cancellationToken = default)
  {
    await service.Update(id, patch, cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return NoContent();
  }

  /// <summary>
  /// Remove a genre, only available to administrators
  /// </summary>
  /// <param name="id">GUID of genre</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>HTTP code: 204</returns>
  [Authorize(Roles = Roles.Administrator)]
  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteGenre(Guid id, CancellationToken cancellationToken = default)
  {
    await service.Remove(id, cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return NoContent();
  }
}
