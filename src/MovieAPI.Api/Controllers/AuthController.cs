using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;

namespace MovieAPI.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService service) : ControllerBase
{
  [HttpPost("register")]
  public async Task<IActionResult> Register(RegisterDto newUser, CancellationToken cancellationToken = default)
  {
    var result = await service.Register(newUser, cancellationToken);
    return Ok(result);
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login(LoginDto credentials, CancellationToken cancellationToken = default)
  {
    var result = await service.Login(credentials, cancellationToken);
    return Ok(result);
  }

  [Authorize]
  [HttpPost("logout")]
  public async Task<IActionResult> Logout(CancellationToken cancellationToken = default)
  {
    await service.Logout(CurrentUserId, cancellationToken);
    return NoContent();
  }

  [Authorize]
  [HttpPut("me")]
  public async Task<IActionResult> UpdateMe(UserForUpdateDto updatedUser, CancellationToken cancellationToken = default)
  {
    var result = await service.Update(CurrentUserId, updatedUser, cancellationToken);
    return Ok(result);
  }

  private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
