using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.Mvc;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;

namespace MovieAPI.Api.Controllers;

[ApiController]
[Route("api/genres")]
public class GenresController(IGenreService service) : ControllerBase
{
  [HttpGet]
  public async Task<IActionResult> GetGenres(CancellationToken cancellationToken = default)
  {
    var result = await service.GetMany(cancellationToken);
    return Ok(result);
  }

  [HttpGet("{id}", Name = "GetGenre")]
  public async Task<IActionResult> GetGenre(Guid id, bool includeMovies = true, CancellationToken cancellationToken = default)
  {
    var result = await service.GetOne(id, includeMovies, cancellationToken);

    if (result == null)
    {
      return NotFound();
    }

    return Ok(result);
  }

  [HttpPost]
  public async Task<IActionResult> CreateGenre(GenreForChangeDto newGenre, CancellationToken cancellationToken = default)
  {
    var result = await service.Create(newGenre, cancellationToken);

    if (!result.Success)
    {
      return BadRequest(result.Error!.Message);
    }

    return CreatedAtRoute("GetGenre", new { result.Genre!.Id }, result.Genre);
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateGenre(Guid id, GenreForChangeDto updatedGenre, CancellationToken cancellationToken = default)
  {
    var (success, message) = await service.Update(id, updatedGenre, cancellationToken);
    if (!success)
    {
      return BadRequest(message);
    }

    return NoContent();
  }

  [HttpPatch("{id}")]
  public async Task<IActionResult> PatchGenre(Guid id, JsonPatchDocument<GenreForChangeDto> patch, CancellationToken cancellationToken = default)
  {
    var (success, message) = await service.Update(id, patch, cancellationToken);
    if (!success)
    {
      return BadRequest(message);
    }

    return NoContent();
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteGenre(Guid id, CancellationToken cancellationToken = default)
  {
    await service.Remove(id, cancellationToken);
    return NoContent();
  }
}
