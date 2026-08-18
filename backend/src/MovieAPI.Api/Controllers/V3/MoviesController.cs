using System.Text.Json;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Constants;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Api.Controllers.V3;

/// <summary>
/// V3 Controller for handling movies
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/movies")]
[ApiVersion("3.0")]
public class MoviesController(IMovieService service, IOutputCacheStore cacheStore) : ControllerBase
{
  /// <summary>
  /// Fetch paginated and filterable list of movies
  /// </summary>
  /// <param name="name">Filter on movie title</param>
  /// <param name="search">Search in synopsis</param>
  /// <param name="genre">Filter on genre</param>
  /// <param name="year">Filter on release year</param>
  /// <param name="minRating">Filter on minimum rating</param>
  /// <param name="maxRating">Filter on maximum rating</param>
  /// <param name="page">Page to display, defaults to 1</param>
  /// <param name="pageSize">Amount per page, defaults to 10</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>List of MovieDto objects</returns>
  [HttpGet]
  [OutputCache(PolicyName = "CatalogCache")]
  public async Task<IActionResult> GetMovies(string? name, string? search, string? genre,
    int? year, decimal? minRating, decimal? maxRating,
    int? page, int? pageSize,
    CancellationToken cancellationToken = default)
  {
    var (result, pagination) = await service.GetMany(
      new MovieSearchParams(name, search, genre, year, minRating, maxRating),
      page, pageSize, cancellationToken);

    if (pagination != null)
    {
      Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(pagination));
    }

    return Ok(result);
  }

  /// <summary>
  /// Get specific movie with extended information
  /// </summary>
  /// <param name="id">GUID of movie</param>
  /// <param name="includePersons">Whether to include persons participating in movie</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>MovieExtendedDto object</returns>
  [HttpGet("{id}", Name = "GetMovie")]
  [OutputCache(PolicyName = "CatalogCache")]
  public async Task<IActionResult> GetMovie(Guid id, bool includePersons = true,
    CancellationToken cancellationToken = default)
  {
    var result = await service.GetOne(id, includePersons, cancellationToken);
    return Ok(result);
  }

  /// <summary>
  /// Create new movie, only available to power users and above
  /// </summary>
  /// <param name="newMovie">MovieForChangeDto object</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>MovieDto object and route to said object</returns>
  [Authorize(Roles = Roles.PowerUserAndAbove)]
  [HttpPost]
  public async Task<IActionResult> CreateMovie(MovieForChangeDto newMovie,
    CancellationToken cancellationToken = default)
  {
    var result = await service.Create(newMovie, cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return CreatedAtRoute("GetMovie", new { result.Id }, result);
  }

  /// <summary>
  /// Full object update of a movie, only available to power users and above
  /// </summary>
  /// <param name="id">GUID of movie</param>
  /// <param name="updatedMovie">MovieForChangeDto object</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>HTTP code: 204</returns>
  [Authorize(Roles = Roles.PowerUserAndAbove)]
  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateMovie(Guid id, MovieForChangeDto updatedMovie,
    CancellationToken cancellationToken = default)
  {
    await service.Update(id, updatedMovie, cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return NoContent();
  }

  /// <summary>
  /// Update movie through JSON patch, only available to power users and above
  /// </summary>
  /// <param name="id">GUID of movie</param>
  /// <param name="patch">JSON patch document</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>HTTP code: 204</returns>
  [Authorize(Roles = Roles.PowerUserAndAbove)]
  [HttpPatch("{id}")]
  public async Task<IActionResult> PatchMovie(Guid id, JsonPatchDocument<MovieForChangeDto> patch,
    CancellationToken cancellationToken = default)
  {
    await service.Update(id, patch, cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return NoContent();
  }

  /// <summary>
  /// Remove a movie, only available to moderators and above
  /// </summary>
  /// <param name="id">GUID of movie</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>HTTP code: 204</returns>
  [Authorize(Roles = Roles.ModeratorAndAbove)]
  [HttpDelete("{id}")]
  public async Task<IActionResult> RemoveMovie(Guid id,
    CancellationToken cancellationToken = default)
  {
    await service.Remove(id, cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return NoContent();
  }
}
