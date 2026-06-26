using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieAPI.Api.Extensions;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;

namespace MovieAPI.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/auth")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
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

  [HttpPost("refresh")]
  public async Task<IActionResult> Refresh(RefreshTokenDto request, CancellationToken cancellationToken = default)
  {
    var result = await service.Refresh(request, cancellationToken);
    return Ok(result);
  }

  [Authorize]
  [HttpPost("logout")]
  public async Task<IActionResult> Logout(RefreshTokenDto request, CancellationToken cancellationToken = default)
  {
    await service.Logout(User.GetUserId(), request, cancellationToken);
    return NoContent();
  }

  [Authorize]
  [HttpPut("me")]
  public async Task<IActionResult> UpdateMe(UserForUpdateDto updatedUser, CancellationToken cancellationToken = default)
  {
    var result = await service.Update(User.GetUserId(), updatedUser, cancellationToken);
    return Ok(result);
  }

  [Authorize]
  [HttpPut("me/password")]
  public async Task<IActionResult> ChangePassword(ChangePasswordDto changePassword, CancellationToken cancellationToken = default)
  {
    await service.ChangePassword(User.GetUserId(), changePassword, cancellationToken);
    return NoContent();
  }

  [HttpPost("forgot-password")]
  public async Task<IActionResult> ForgotPassword(ForgotPasswordDto request, CancellationToken cancellationToken = default)
  {
    await service.ForgotPassword(request, cancellationToken);
    return NoContent();
  }

  [HttpPost("reset-password")]
  public async Task<IActionResult> ResetPassword(ResetPasswordDto request, CancellationToken cancellationToken = default)
  {
    await service.ResetPassword(request, cancellationToken);
    return NoContent();
  }
}
