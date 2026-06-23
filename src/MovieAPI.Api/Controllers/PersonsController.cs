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
  public async Task<IActionResult> GetPerson(Guid id, bool includeMovies = true, CancellationToken cancellationToken = default)
  {
    var result = await service.GetOne(id, includeMovies, cancellationToken);
    return Ok(result);
  }

  [HttpPost]
  public async Task<IActionResult> CreatePerson(PersonForChangeDto newPerson, CancellationToken cancellationToken = default)
  {
    var result = await service.Create(newPerson, cancellationToken);
    return CreatedAtRoute("GetPerson", new { result.Id }, result);
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> UpdatePerson(Guid id, PersonForChangeDto updatedPerson,
    CancellationToken cancellationToken = default)
  {
    await service.Update(id, updatedPerson, cancellationToken);
    return NoContent();
  }

  [HttpPatch("{id}")]
  public async Task<IActionResult> PatchPerson(Guid id, JsonPatchDocument<PersonForChangeDto> patch,
    CancellationToken cancellationToken = default)
  {
    await service.Update(id, patch, cancellationToken);
    return NoContent();
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> DeletePerson(Guid id, CancellationToken cancellationToken = default)
  {
    await service.Remove(id, cancellationToken);
    return NoContent();
  }
}
