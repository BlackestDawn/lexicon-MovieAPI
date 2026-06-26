using System.Text.Json;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieAPI.Api.Extensions;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Constants;

namespace MovieAPI.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/admin/users")]
[Authorize(Roles = Roles.Administrator)]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class AdminUsersController(IAdminUserService service) : ControllerBase
{
  [HttpGet]
  public async Task<IActionResult> GetUsers(int? page, int? pageSize, CancellationToken cancellationToken = default)
  {
    var (result, pagination) = await service.GetMany(page, pageSize, cancellationToken);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(pagination));
    return Ok(result);
  }

  [HttpGet("{id}", Name = "GetAdminUser")]
  public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken = default)
  {
    var result = await service.GetOne(id, cancellationToken);
    return Ok(result);
  }

  [HttpPost]
  public async Task<IActionResult> CreateUser(AdminUserForCreationDto newUser, CancellationToken cancellationToken = default)
  {
    var result = await service.Create(newUser, cancellationToken);
    return CreatedAtRoute("GetAdminUser", new { result.Id }, result);
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateUser(Guid id, AdminUserForUpdateDto updatedUser, CancellationToken cancellationToken = default)
  {
    var result = await service.Update(id, updatedUser, User.GetUserId(), cancellationToken);
    return Ok(result);
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken = default)
  {
    await service.Remove(id, User.GetUserId(), cancellationToken);
    return NoContent();
  }
}
