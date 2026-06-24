using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using MovieAPI.Application.Exceptions;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Interfaces;

namespace MovieAPI.Application.Services;

public class AuthService(
  UserManager<ApplicationUser> userManager,
  SignInManager<ApplicationUser> signInManager,
  ITokenService tokenService,
  IMapper mapper,
  IValidator<RegisterDto> registerValidator,
  IValidator<LoginDto> loginValidator,
  IValidator<UserForUpdateDto> updateValidator) : IAuthService
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

    return await BuildAuthResponseAsync(user);
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

    return await BuildAuthResponseAsync(user);
  }

  public async Task Logout(Guid userId, CancellationToken token = default)
  {
    var user = await userManager.FindByIdAsync(userId.ToString())
      ?? throw new NotFoundException($"User '{userId}' not found");

    // Access tokens aren't tracked/blacklisted yet, so there's nothing to revoke
    // directly. Bumping the security stamp invalidates anything issued before this
    // point and becomes real revocation once token validation checks the stamp.
    await userManager.UpdateSecurityStampAsync(user);
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

  private async Task<AuthResponseDto> BuildAuthResponseAsync(ApplicationUser user)
  {
    var roles = await userManager.GetRolesAsync(user);
    var (accessToken, expiresAtUtc) = tokenService.GenerateToken(user, roles);

    return new AuthResponseDto
    {
      User = mapper.Map<UserDto>(user),
      AccessToken = accessToken,
      ExpiresAtUtc = expiresAtUtc,
    };
  }

  private static IEnumerable<ValidationFailure> ToValidationFailures(IEnumerable<IdentityError> errors) =>
    errors.Select(e => new ValidationFailure(e.Code, e.Description));
}
