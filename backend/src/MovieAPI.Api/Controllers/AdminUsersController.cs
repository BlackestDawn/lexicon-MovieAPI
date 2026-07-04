using System.Text.Json;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieAPI.Api.Extensions;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Constants;

namespace MovieAPI.Api.Controllers;

/// <summary>
/// Controller for handling users, only accessible to Administrators
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/admin/users")]
[Authorize(Roles = Roles.Administrator)]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class AdminUsersController(IAdminUserService service) : ControllerBase
{
  /// <summary>
  /// Fetch a lists of users in paginated format
  /// </summary>
  /// <param name="page">Page to view, defaults to 1</param>
  /// <param name="pageSize">Amount per page, defaults to 10</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>List of AdminUserDto objects</returns>
  [HttpGet]
  public async Task<IActionResult> GetUsers(int? page, int? pageSize, CancellationToken cancellationToken = default)
  {
    var (result, pagination) = await service.GetMany(page, pageSize, cancellationToken);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(pagination));
    return Ok(result);
  }

  /// <summary>
  /// Fetch a specific user
  /// </summary>
  /// <param name="id">GUID of user</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>AdminUserDto object</returns>
  [HttpGet("{id}", Name = "GetAdminUser")]
  public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken = default)
  {
    var result = await service.GetOne(id, cancellationToken);
    return Ok(result);
  }

  /// <summary>
  /// Create a new user
  /// </summary>
  /// <param name="newUser">AdminForCreationDto object</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>AdminUserDto object and route to said object</returns>
  [HttpPost]
  public async Task<IActionResult> CreateUser(AdminUserForCreationDto newUser, CancellationToken cancellationToken = default)
  {
    var result = await service.Create(newUser, cancellationToken);
    return CreatedAtRoute("GetAdminUser", new { result.Id }, result);
  }

  /// <summary>
  /// Updates specified user
  /// </summary>
  /// <param name="id">GUID of user</param>
  /// <param name="updatedUser">AdminUserForUpdateDto object</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>Updated AdminUserDto object</returns>
  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateUser(Guid id, AdminUserForUpdateDto updatedUser, CancellationToken cancellationToken = default)
  {
    var result = await service.Update(id, updatedUser, User.GetUserId(), cancellationToken);
    return Ok(result);
  }

  /// <summary>
  /// Removes a user
  /// </summary>
  /// <param name="id">GUID of user</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>HTTP code: 204</returns>
  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken = default)
  {
    await service.Remove(id, User.GetUserId(), cancellationToken);
    return NoContent();
  }
}
