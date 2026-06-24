using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using MovieAPI.Application.Exceptions;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Constants;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Interfaces;

namespace MovieAPI.Application.Services;

public class AuthService(
  UserManager<ApplicationUser> userManager,
  SignInManager<ApplicationUser> signInManager,
  ITokenService tokenService,
  IRefreshTokenRepository refreshTokenRepository,
  IEmailSender emailSender,
  IMapper mapper,
  IValidator<RegisterDto> registerValidator,
  IValidator<LoginDto> loginValidator,
  IValidator<UserForUpdateDto> updateValidator,
  IValidator<ChangePasswordDto> changePasswordValidator,
  IValidator<RefreshTokenDto> refreshTokenValidator,
  IValidator<ForgotPasswordDto> forgotPasswordValidator,
  IValidator<ResetPasswordDto> resetPasswordValidator) : IAuthService
{
  public async Task<AuthResponseDto> Register(RegisterDto newUser, CancellationToken token = default)
  {
    var validationResult = await registerValidator.ValidateAsync(newUser, token);
    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }

    var user = new ApplicationUser { UserName = newUser.Email, Email = newUser.Email };

    var result = await userManager.CreateAsync(user, newUser.Password);
    if (!result.Succeeded)
    {
      throw new ValidationException(ToValidationFailures(result.Errors));
    }

    await userManager.AddToRoleAsync(user, Roles.User);

    return await BuildAuthResponseAsync(user, token);
  }

  public async Task<AuthResponseDto> Login(LoginDto credentials, CancellationToken token = default)
  {
    var validationResult = await loginValidator.ValidateAsync(credentials, token);
    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }

    var user = await userManager.FindByEmailAsync(credentials.Email)
      ?? throw new AuthenticationException("Invalid email or password");

    var result = await signInManager.CheckPasswordSignInAsync(user, credentials.Password, lockoutOnFailure: true);
    if (!result.Succeeded)
    {
      throw new AuthenticationException("Invalid email or password");
    }

    return await BuildAuthResponseAsync(user, token);
  }

  public async Task<AuthResponseDto> Refresh(RefreshTokenDto request, CancellationToken token = default)
  {
    var validationResult = await refreshTokenValidator.ValidateAsync(request, token);
    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }

    var tokenHash = tokenService.HashToken(request.RefreshToken);
    var existing = await refreshTokenRepository.GetByTokenHashAsync(tokenHash, token);

    if (existing is null || existing.ExpiresAtUtc <= DateTime.UtcNow)
    {
      throw new AuthenticationException("Invalid refresh token");
    }

    if (existing.RevokedAtUtc is not null)
    {
      // A revoked token being presented again means it was already rotated away (or
      // explicitly logged out) - treat this as possible token theft and kill every
      // active session for the user rather than just rejecting this one request.
      await RevokeAllRefreshTokensAsync(existing.UserId, token);
      throw new AuthenticationException("Invalid refresh token");
    }

    var user = await userManager.FindByIdAsync(existing.UserId.ToString())
      ?? throw new AuthenticationException("Invalid refresh token");

    existing.RevokedAtUtc = DateTime.UtcNow;

    return await BuildAuthResponseAsync(user, token);
  }

  public async Task Logout(Guid userId, RefreshTokenDto request, CancellationToken token = default)
  {
    var validationResult = await refreshTokenValidator.ValidateAsync(request, token);
    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }

    var tokenHash = tokenService.HashToken(request.RefreshToken);
    var existing = await refreshTokenRepository.GetByTokenHashAsync(tokenHash, token);

    // An unknown, already-revoked, or someone-else's token is treated as a no-op:
    // the end state the caller wants (this session not logged in) is already true.
    if (existing is null || existing.UserId != userId || existing.RevokedAtUtc is not null)
    {
      return;
    }

    existing.RevokedAtUtc = DateTime.UtcNow;
    await refreshTokenRepository.SaveChangesAsync(token);
  }

  public async Task<UserDto> Update(Guid userId, UserForUpdateDto updatedUser, CancellationToken token = default)
  {
    var validationResult = await updateValidator.ValidateAsync(updatedUser, token);
    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }

    var user = await userManager.FindByIdAsync(userId.ToString())
      ?? throw new NotFoundException($"User '{userId}' not found");

    if (!string.Equals(user.Email, updatedUser.Email, StringComparison.OrdinalIgnoreCase))
    {
      var emailResult = await userManager.SetEmailAsync(user, updatedUser.Email);
      if (!emailResult.Succeeded)
      {
        throw new ValidationException(ToValidationFailures(emailResult.Errors));
      }

      var userNameResult = await userManager.SetUserNameAsync(user, updatedUser.Email);
      if (!userNameResult.Succeeded)
      {
        throw new ValidationException(ToValidationFailures(userNameResult.Errors));
      }
    }

    return mapper.Map<UserDto>(user);
  }

  public async Task ChangePassword(Guid userId, ChangePasswordDto changePassword, CancellationToken token = default)
  {
    var validationResult = await changePasswordValidator.ValidateAsync(changePassword, token);
    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }

    var user = await userManager.FindByIdAsync(userId.ToString())
      ?? throw new NotFoundException($"User '{userId}' not found");

    var result = await userManager.ChangePasswordAsync(user, changePassword.CurrentPassword, changePassword.NewPassword);
    if (!result.Succeeded)
    {
      throw new ValidationException(ToValidationFailures(result.Errors));
    }

    // A password change is as strong a signal as a password reset that any
    // outstanding sessions should not be trusted to continue silently.
    await RevokeAllRefreshTokensAsync(userId, token);
  }

  public async Task ForgotPassword(ForgotPasswordDto request, CancellationToken token = default)
  {
    var validationResult = await forgotPasswordValidator.ValidateAsync(request, token);
    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }

    var user = await userManager.FindByEmailAsync(request.Email);
    if (user is null)
    {
      // Don't reveal whether the email exists - the caller sees the same response
      // either way.
      return;
    }

    var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
    await emailSender.SendPasswordResetEmailAsync(request.Email, resetToken, token);
  }

  public async Task ResetPassword(ResetPasswordDto request, CancellationToken token = default)
  {
    var validationResult = await resetPasswordValidator.ValidateAsync(request, token);
    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }

    var user = await userManager.FindByEmailAsync(request.Email)
      ?? throw new AuthenticationException("Invalid email, token, or password");

    var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
    if (!result.Succeeded)
    {
      throw new ValidationException(ToValidationFailures(result.Errors));
    }

    await RevokeAllRefreshTokensAsync(user.Id, token);
  }

  private async Task RevokeAllRefreshTokensAsync(Guid userId, CancellationToken token)
  {
    await refreshTokenRepository.RevokeAllActiveForUserAsync(userId, token);
    await refreshTokenRepository.SaveChangesAsync(token);
  }

  private async Task<AuthResponseDto> BuildAuthResponseAsync(ApplicationUser user, CancellationToken token)
  {
    var roles = await userManager.GetRolesAsync(user);
    var (accessToken, accessExpiresAtUtc) = tokenService.GenerateToken(user, roles);
    var (refreshToken, refreshExpiresAtUtc) = tokenService.GenerateRefreshToken();

    await refreshTokenRepository.AddAsync(new RefreshToken
    {
      UserId = user.Id,
      TokenHash = tokenService.HashToken(refreshToken),
      ExpiresAtUtc = refreshExpiresAtUtc,
    }, token);
    await refreshTokenRepository.SaveChangesAsync(token);

    return new AuthResponseDto
    {
      User = mapper.Map<UserDto>(user),
      AccessToken = accessToken,
      ExpiresAtUtc = accessExpiresAtUtc,
      RefreshToken = refreshToken,
      RefreshTokenExpiresAtUtc = refreshExpiresAtUtc,
    };
  }

  private static IEnumerable<ValidationFailure> ToValidationFailures(IEnumerable<IdentityError> errors) =>
    errors.Select(e => new ValidationFailure(e.Code, e.Description));
}
