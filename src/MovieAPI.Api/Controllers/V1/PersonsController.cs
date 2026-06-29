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

/// <summary>
/// V1 Controller for handling person in/with movies
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/people")]
[ApiVersion("1.0")]
public class PersonsController(
  IPersonService service,
  IOutputCacheStore cacheStore,
  IMapper mapper) : ControllerBase
{
  /// <summary>
  /// Fetch paginated and filterable list of persons
  /// </summary>
  /// <param name="name">Filter on name</param>
  /// <param name="genre">Filter on genre for movie they've been part of</param>
  /// <param name="year">Filter on birth year</param>
  /// <param name="page">page to display, defaults to 1</param>
  /// <param name="pageSize">Amount per page, defaults to 1</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>List of PersonV1Dto objects</returns>
  [HttpGet]
  [OutputCache(PolicyName = "CatalogCache")]
  public async Task<IActionResult> GetPeople(string? name, string? genre, int? year,
    int? page, int? pageSize, CancellationToken cancellationToken = default)
  {
    var (result, pagination) = await service.GetMany(new PeopleSearchParams(name, genre, year),
      page, pageSize, cancellationToken);

    if (pagination != null)
    {
      Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(pagination));
    }

    return Ok(mapper.Map<IEnumerable<PersonV1Dto>>(result));
  }

  /// <summary>
  /// Get specific person with extended information
  /// </summary>
  /// <param name="id">GUID of person</param>
  /// <param name="includeMovies">if to include movie they've been part of</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>PersonExtendedV1Dto object</returns>
  [HttpGet("{id}", Name = "GetPerson")]
  [OutputCache(PolicyName = "CatalogCache")]
  public async Task<IActionResult> GetPerson(Guid id, bool includeMovies = true, CancellationToken cancellationToken = default)
  {
    var result = await service.GetOne(id, includeMovies, cancellationToken);
    return Ok(mapper.Map<PersonExtendedV1Dto>(result));
  }

  /// <summary>
  /// Create a new person, needs power user or above
  /// </summary>
  /// <param name="newPerson">PersonForChangeV1Dto object</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>PersonV1Dto object with route to said object</returns>
  [Authorize(Roles = Roles.PowerUserAndAbove)]
  [HttpPost]
  public async Task<IActionResult> CreatePerson(PersonForChangeV1Dto newPerson, CancellationToken cancellationToken = default)
  {
    var result = await service.Create(mapper.Map<PersonForChangeDto>(newPerson), cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return CreatedAtRoute("GetPerson", new { result.Id }, mapper.Map<PersonV1Dto>(result));
  }

  /// <summary>
  /// Whole object update of person, needs power user or above
  /// </summary>
  /// <param name="id">GUID of person</param>
  /// <param name="updatedPerson">PersonForChangeV1Dto object</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>HTTP code: 204</returns>
  [Authorize(Roles = Roles.PowerUserAndAbove)]
  [HttpPut("{id}")]
  public async Task<IActionResult> UpdatePerson(Guid id, PersonForChangeV1Dto updatedPerson,
    CancellationToken cancellationToken = default)
  {
    await service.Update(id, mapper.Map<PersonForChangeDto>(updatedPerson), cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return NoContent();
  }

  /// <summary>
  /// Update person through JSON patch, needs power user or above
  /// </summary>
  /// <param name="id">GUID of person</param>
  /// <param name="patch">JSON patch document</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>HTTP code: 204</returns>
  [Authorize(Roles = Roles.PowerUserAndAbove)]
  [HttpPatch("{id}")]
  public async Task<IActionResult> PatchPerson(Guid id, JsonPatchDocument<PersonForChangeV1Dto> patch,
    CancellationToken cancellationToken = default)
  {
    await service.Update(id, patch, cancellationToken);
    await cacheStore.EvictByTagAsync("catalog", cancellationToken);
    return NoContent();
  }

  /// <summary>
  /// Remove a person, needs moderator or above
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
