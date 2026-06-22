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

  [HttpPut]
  public async Task<IActionResult> CreateGenre(GenreForChangeDto newGenre, CancellationToken cancellationToken)
  {
    var result = await service.Create(newGenre, cancellationToken);

    if (!result.Success)
    {
      return BadRequest(result.Error!.Message);
    }

    return CreatedAtRoute("GetGenre", new { result.Genre!.Id }, result.Genre);
  }
}
