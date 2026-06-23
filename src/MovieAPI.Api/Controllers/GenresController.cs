using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;

namespace MovieAPI.Api.Controllers;

[ApiController]
[Route("api/genres")]
public class GenresController(IGenreService service, IOutputCacheStore cacheStore) : ControllerBase
{
  [HttpGet]
  [OutputCache(PolicyName = "CatalogCache")]
  public async Task<IActionResult> GetGenres(CancellationToken cancellationToken = default)
  {
    var result = await service.GetMany(cancellationToken);
    return Ok(result);
  }

  [HttpGet("{id}", Name = "GetGenre")]
  [OutputCache(PolicyName = "CatalogCache")]
  public async Task<IActionResult> GetGenre(Guid id, bool includeMovies = true, CancellationToken cancellationToken = default)
  {
    var result = await service.GetOne(id, includeMovies, cancellationToken);
    return Ok(result);
  }

  [HttpPost]
  public async Task<IActionResult> CreateGenre(GenreForChangeDto newGenre, CancellationToken cancellationToken = default)
  {
    var result = await service.Create(newGenre, cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return CreatedAtRoute("GetGenre", new { result.Id }, result);
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateGenre(Guid id, GenreForChangeDto updatedGenre, CancellationToken cancellationToken = default)
  {
    await service.Update(id, updatedGenre, cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return NoContent();
  }

  [HttpPatch("{id}")]
  public async Task<IActionResult> PatchGenre(Guid id, JsonPatchDocument<GenreForChangeDto> patch, CancellationToken cancellationToken = default)
  {
    await service.Update(id, patch, cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return NoContent();
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteGenre(Guid id, CancellationToken cancellationToken = default)
  {
    await service.Remove(id, cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return NoContent();
  }
}
