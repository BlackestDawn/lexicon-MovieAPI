using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MovieAPI.Application.Interfaces;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Api.Controllers;

[ApiController]
[Route("api/people")]
public class PersonsController(IPersonService service) : ControllerBase
{
  [HttpGet]
  public async Task<IActionResult> GetPeople(string? name, string? genre, int? year,
    int? page, int? pageSize, CancellationToken cancellationToken = default)
  {
    var (result, pagination) = await service.GetMany(new PeopleSearchParams(name, genre, year),
      page, pageSize, cancellationToken);

    if (pagination != null)
    {
      Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(pagination));
    }

    return Ok(result);
  }

  [HttpGet("{id}", Name = "GetPerson")]
  public async Task<IActionResult> GetPerson(Guid id, bool includeMovies, CancellationToken cancellationToken = default)
  {
    var result = await service.GetOne(id, includeMovies, cancellationToken);

    if (result == null)
    {
      return NotFound();
    }

    return Ok(result);
  }
}
