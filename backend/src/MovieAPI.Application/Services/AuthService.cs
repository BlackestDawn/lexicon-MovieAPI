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
using OpenIddict.Abstractions;

namespace MovieAPI.Application.Services;

public class AuthService(
  UserManager<ApplicationUser> userManager,
  IOpenIddictAuthorizationManager authorizationManager,
  IEmailSender emailSender,
  IMapper mapper,
  IValidator<RegisterDto> registerValidator,
  IValidator<UserForUpdateDto> updateValidator,
  IValidator<ChangePasswordDto> changePasswordValidator,
  IValidator<ForgotPasswordDto> forgotPasswordValidator,
  IValidator<ResetPasswordDto> resetPasswordValidator) : IAuthService
{
  public async Task<CurrentUserDto> GetCurrent(Guid userId, CancellationToken token = default)
  {
    var user = await userManager.FindByIdAsync(userId.ToString())
      ?? throw new NotFoundException($"User '{userId}' not found");

    var roles = await userManager.GetRolesAsync(user);

    return new CurrentUserDto
    {
      Id = user.Id,
      Email = user.Email ?? string.Empty,
      Role = roles.FirstOrDefault() ?? string.Empty,
      DisplayName = user.DisplayName,
    };
  }

  public async Task<UserDto> Register(RegisterDto newUser, CancellationToken token = default)
  {
    var validationResult = await registerValidator.ValidateAsync(newUser, token);
    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }

    var displayName = string.IsNullOrWhiteSpace(newUser.DisplayName)
      ? newUser.Email.Split('@')[0]
      : newUser.DisplayName.Trim();

    var user = new ApplicationUser { UserName = newUser.Email, Email = newUser.Email, DisplayName = displayName };

    var result = await userManager.CreateAsync(user, newUser.Password);
    if (!result.Succeeded)
    {
      throw new ValidationException(ToValidationFailures(result.Errors));
    }

    await userManager.AddToRoleAsync(user, Roles.User);

    // No tokens are minted here - the client logs in via POST /connect/token
    // (grant_type=password) right after registering, same as any other login.
    return mapper.Map<UserDto>(user);
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

    if (!string.IsNullOrWhiteSpace(updatedUser.DisplayName))
    {
      user.DisplayName = updatedUser.DisplayName.Trim();
      await userManager.UpdateAsync(user);
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

    // A password change is as strong a signal as a password reset that any outstanding
    // sessions should not be trusted to continue silently. Access tokens already issued
    // are invalidated immediately by ValidateSecurityStampHandler since the stamp just
    // changed; revoking authorizations here additionally kills outstanding refresh
    // tokens, which aren't on that per-request validation path.
    await RevokeAllAuthorizationsAsync(userId, token);
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

    await RevokeAllAuthorizationsAsync(user.Id, token);
  }

  private async Task RevokeAllAuthorizationsAsync(Guid userId, CancellationToken token)
  {
    await foreach (var authorization in authorizationManager.FindBySubjectAsync(userId.ToString(), token))
    {
      await authorizationManager.TryRevokeAsync(authorization, token);
    }
  }

  private static IEnumerable<ValidationFailure> ToValidationFailures(IEnumerable<IdentityError> errors) =>
    errors.Select(e => new ValidationFailure(e.Code, e.Description));
}
