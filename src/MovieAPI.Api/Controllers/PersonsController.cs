using System.Text.Json;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.Mvc;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
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

  [HttpPost]
  public async Task<IActionResult> CreatePerson(PersonForCreationDto newPerson, CancellationToken cancellationToken = default)
  {
    var result = await service.Create(newPerson, cancellationToken);

    if (!result.Success)
    {
      return BadRequest(result.Error!.Message);
    }

    return CreatedAtRoute("GetPerson", new { result.Person!.Id }, result.Person);
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateMovie(Guid id, PersonForUpdateDto updatedPerson,
    CancellationToken cancellationToken)
  {
    var (success, message) = await service.Update(id, updatedPerson, cancellationToken);

    if (!success)
    {
      return BadRequest(message);
    }

    return NoContent();
  }

  [HttpPatch("{id}")]
  public async Task<IActionResult> PatchMovie(Guid id, JsonPatchDocument<PersonForUpdateDto> patch,
    CancellationToken cancellationToken)
  {
    var (success, message) = await service.Update(id, patch, cancellationToken);

    if (!success)
    {
      return BadRequest(message);
    }

    return NoContent();
  }
}
