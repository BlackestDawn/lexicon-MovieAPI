using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieAPI.Api.Extensions;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;

namespace MovieAPI.Api.Controllers;

/// <summary>
/// Controller for user authentication and self-service options
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/auth")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class AuthController(IAuthService service) : ControllerBase
{
  /// <summary>
  /// Fetch the currently authenticated user's profile
  /// </summary>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>CurrentUserDto object</returns>
  [Authorize]
  [HttpGet("me")]
  public async Task<IActionResult> GetMe(CancellationToken cancellationToken = default)
  {
    var result = await service.GetCurrent(User.GetUserId(), cancellationToken);
    return Ok(result);
  }

  /// <summary>
  /// Self-registration for new users. Log in afterwards via POST /connect/token
  /// (grant_type=password) to obtain an access/refresh token pair.
  /// </summary>
  /// <param name="newUser">RegisterDto object</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>UserDto object</returns>
  [HttpPost("register")]
  public async Task<IActionResult> Register(RegisterDto newUser, CancellationToken cancellationToken = default)
  {
    var result = await service.Register(newUser, cancellationToken);
    return Ok(result);
  }

  /// <summary>
  /// Update self, only available to logged in users
  /// </summary>
  /// <param name="updatedUser">UserForUpdateDto object</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>UserDto object</returns>
  [Authorize]
  [HttpPut("me")]
  public async Task<IActionResult> UpdateMe(UserForUpdateDto updatedUser, CancellationToken cancellationToken = default)
  {
    var result = await service.Update(User.GetUserId(), updatedUser, cancellationToken);
    return Ok(result);
  }

  /// <summary>
  /// Change own password, only available to logged in users
  /// </summary>
  /// <param name="changePassword">ChangePasswordDto object</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>HTTP code: 204</returns>
  [Authorize]
  [HttpPut("me/password")]
  public async Task<IActionResult> ChangePassword(ChangePasswordDto changePassword, CancellationToken cancellationToken = default)
  {
    await service.ChangePassword(User.GetUserId(), changePassword, cancellationToken);
    return NoContent();
  }

  /// <summary>
  /// Initiate password change request without need for login
  /// </summary>
  /// <param name="request">ForgotPasswordDto object</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>HTTP code: 204</returns>
  [HttpPost("forgot-password")]
  public async Task<IActionResult> ForgotPassword(ForgotPasswordDto request, CancellationToken cancellationToken = default)
  {
    await service.ForgotPassword(request, cancellationToken);
    return NoContent();
  }

  /// <summary>
  /// Reset password
  /// </summary>
  /// <param name="request">ResetPasswordDto object</param>
  /// <param name="cancellationToken">Notification token for canceling operations</param>
  /// <returns>HTTP code: 204</returns>
  [HttpPost("reset-password")]
  public async Task<IActionResult> ResetPassword(ResetPasswordDto request, CancellationToken cancellationToken = default)
  {
    await service.ResetPassword(request, cancellationToken);
    return NoContent();
  }
}
