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
/// V3 Controller for handling persons associated with movies
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/persons")]
[ApiVersion("3.0")]
[ApiVersion("3.1")]
public class PersonsController(IPersonService service, IOutputCacheStore cacheStore) : ControllerBase
{
  /// <summary>
  /// Fetch paginated and filterable list of persons
  /// </summary>
  /// <param name="name">Filter on name</param>
  /// <param name="genre">Filter on genre for movie they've been part of</param>
  /// <param name="year">Filter on birth year</param>
  /// <param name="page">Page to display, defaults to 1</param>
  /// <param name="pageSize">Amount per page, defaults to 10</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>List of PersonDto objects</returns>
  [HttpGet]
  [OutputCache(PolicyName = "CatalogCache")]
  public async Task<IActionResult> GetPersons(string? name, string? genre, int? year,
    int? page, int? pageSize, CancellationToken cancellationToken = default)
  {
    var (result, pagination) = await service.GetMany(new PersonSearchParams(name, genre, year),
      page, pageSize, cancellationToken);

    if (pagination != null)
    {
      Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(pagination));
    }

    return Ok(result);
  }

  /// <summary>
  /// Get specific person with extended information
  /// </summary>
  /// <param name="id">GUID of person</param>
  /// <param name="includeMovies">Whether to include movies they've been part of</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>PersonExtendedDto object</returns>
  [HttpGet("{id}", Name = "GetPersonV3")]
  [OutputCache(PolicyName = "CatalogCache")]
  public async Task<IActionResult> GetPerson(Guid id, bool includeMovies = true, CancellationToken cancellationToken = default)
  {
    var result = await service.GetOne(id, includeMovies, cancellationToken);
    return Ok(result);
  }

  /// <summary>
  /// Create a new person, only available to power users and above
  /// </summary>
  /// <param name="newPerson">PersonForChangeDto object</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>PersonDto object and route to said object</returns>
  [Authorize(Roles = Roles.PowerUserAndAbove)]
  [HttpPost]
  public async Task<IActionResult> CreatePerson(PersonForChangeDto newPerson, CancellationToken cancellationToken = default)
  {
    var result = await service.Create(newPerson, cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return CreatedAtRoute("GetPersonV3", new { result.Id }, result);
  }

  /// <summary>
  /// Full object update of person, only available to power users and above
  /// </summary>
  /// <param name="id">GUID of person</param>
  /// <param name="updatedPerson">PersonForChangeDto object</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>HTTP code: 204</returns>
  [Authorize(Roles = Roles.PowerUserAndAbove)]
  [HttpPut("{id}")]
  public async Task<IActionResult> UpdatePerson(Guid id, PersonForChangeDto updatedPerson,
    CancellationToken cancellationToken = default)
  {
    await service.Update(id, updatedPerson, cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return NoContent();
  }

  /// <summary>
  /// Update person through JSON patch, only available to power users and above
  /// </summary>
  /// <param name="id">GUID of person</param>
  /// <param name="patch">JSON patch document</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>HTTP code: 204</returns>
  [Authorize(Roles = Roles.PowerUserAndAbove)]
  [HttpPatch("{id}")]
  public async Task<IActionResult> PatchPerson(Guid id, JsonPatchDocument<PersonForChangeDto> patch,
    CancellationToken cancellationToken = default)
  {
    await service.Update(id, patch, cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return NoContent();
  }

  /// <summary>
  /// Remove a person, only available to moderators and above
  /// </summary>
  /// <param name="id">GUID of person</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>HTTP code: 204</returns>
  [Authorize(Roles = Roles.ModeratorAndAbove)]
  [HttpDelete("{id}")]
  public async Task<IActionResult> DeletePerson(Guid id, CancellationToken cancellationToken = default)
  {
    await service.Remove(id, cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return NoContent();
  }
}
