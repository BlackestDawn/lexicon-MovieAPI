using System.Text.Json;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Application.Models.V1;
using MovieAPI.Domain.Constants;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Api.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/movies")]
[ApiVersion("1.0")]
public class MoviesController(
  IMovieService service,
  IOutputCacheStore cacheStore,
  IMapper mapper) : ControllerBase
{
  [HttpGet]
  [OutputCache(PolicyName = "CatalogCache")]
  public async Task<IActionResult> GetMovies(string? name, string? search, string? genre,
    int? year, decimal? minRating,
    int? page, int? pageSize,
    CancellationToken cancellationToken = default)
  {
    var (result, pagination) = await service.GetMany(
      new MovieSearchParams(name, search, genre, year, minRating),
      page, pageSize, cancellationToken);

    if (pagination != null)
    {
      Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(pagination));
    }

    return Ok(result);
  }

  [HttpGet("{id}", Name = "GetMovie")]
  [OutputCache(PolicyName = "CatalogCache")]
  public async Task<IActionResult> GetMovie(Guid id, bool includePeople = true,
    CancellationToken cancellationToken = default)
  {
    var result = await service.GetOne(id, includePeople, cancellationToken);
    return Ok(mapper.Map<MovieExtendedV1Dto>(result));
  }

  [Authorize(Roles = Roles.PowerUserAndAbove)]
  [HttpPost]
  public async Task<IActionResult> CreateMovie(MovieForChangeDto newMovie,
    CancellationToken cancellationToken = default)
  {
    var result = await service.Create(newMovie, cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return CreatedAtRoute("GetMovie", new { result.Id }, result);
  }

  [Authorize(Roles = Roles.PowerUserAndAbove)]
  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateMovie(Guid id, MovieForChangeDto updatedMovie,
    CancellationToken cancellationToken = default)
  {
    await service.Update(id, updatedMovie, cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return NoContent();
  }

  [Authorize(Roles = Roles.PowerUserAndAbove)]
  [HttpPatch("{id}")]
  public async Task<IActionResult> PatchMovie(Guid id, JsonPatchDocument<MovieForChangeDto> patch,
    CancellationToken cancellationToken = default)
  {
    await service.Update(id, patch, cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return NoContent();
  }

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
