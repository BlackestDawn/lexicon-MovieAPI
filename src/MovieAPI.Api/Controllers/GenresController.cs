using Microsoft.AspNetCore.Mvc;
using MovieAPI.Application.Interfaces;

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

  [HttpGet("{id}")]
  public async Task<IActionResult> GetGenre(Guid id, CancellationToken cancellationToken)
  {
    var result = await service.GetOne(id, cancellationToken);

    if (result == null)
    {
      return NotFound();
    }

    return Ok(result);
  }
}
