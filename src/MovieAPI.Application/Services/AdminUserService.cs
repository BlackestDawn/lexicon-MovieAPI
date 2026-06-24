using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using MovieAPI.Application.Exceptions;
using MovieAPI.Application.Helpers;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Constants;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Application.Services;

public class AdminUserService(
  UserManager<ApplicationUser> userManager,
  IValidator<AdminUserForCreationDto> creationValidator,
  IValidator<AdminUserForUpdateDto> updateValidator) : IAdminUserService
{
  public async Task<(IEnumerable<AdminUserDto>, PaginationMetadata)> GetMany(int? page, int? pageSize, CancellationToken token = default)
  {
    var currentPage = page is null || page < DefaultValues.Page ? DefaultValues.Page : page.Value;
    var size = pageSize is null || pageSize <= 0 ? DefaultValues.PageSize : pageSize.Value;

    // Synchronous LINQ rather than CountAsync/ToListAsync: UserManager.Users is just
    // an IQueryable, and EF Core's async query extensions only work against an EF
    // query provider - they'd throw against the plain in-memory queryable used to
    // unit test this method. EF still translates this to a single SQL query either way.
    var totalCount = userManager.Users.Count();
    var users = userManager.Users
      .OrderBy(u => u.Email)
      .Skip((currentPage - 1) * size)
      .Take(size)
      .ToList();

    var dtos = new List<AdminUserDto>();
    foreach (var user in users)
    {
      dtos.Add(await MapToDtoAsync(user));
    }

    return (dtos, new PaginationMetadata(totalCount, size, currentPage));
  }

  public async Task<AdminUserDto> GetOne(Guid id, CancellationToken token = default)
  {
    var user = await userManager.FindByIdAsync(id.ToString())
      ?? throw new NotFoundException($"User '{id}' not found");

    return await MapToDtoAsync(user);
  }

  public async Task<AdminUserDto> Create(AdminUserForCreationDto newUser, CancellationToken token = default)
  {
    var validationResult = await creationValidator.ValidateAsync(newUser, token);
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

    await userManager.AddToRoleAsync(user, newUser.Role);

    return await MapToDtoAsync(user);
  }

  public async Task<AdminUserDto> Update(Guid id, AdminUserForUpdateDto updatedUser, Guid currentAdminId, CancellationToken token = default)
  {
    var validationResult = await updateValidator.ValidateAsync(updatedUser, token);
    if (!validationResult.IsValid)
    {
      throw new ValidationException(validationResult.Errors);
    }

    if (id == currentAdminId && !string.Equals(updatedUser.Role, Roles.Administrator, StringComparison.Ordinal))
    {
      throw new ForbiddenException("You cannot change your own role away from Administrator");
    }

    var user = await userManager.FindByIdAsync(id.ToString())
      ?? throw new NotFoundException($"User '{id}' not found");

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

    var currentRoles = await userManager.GetRolesAsync(user);
    if (!currentRoles.Contains(updatedUser.Role))
    {
      if (currentRoles.Count > 0)
      {
        await userManager.RemoveFromRolesAsync(user, currentRoles);
      }

      await userManager.AddToRoleAsync(user, updatedUser.Role);
    }

    return await MapToDtoAsync(user);
  }

  public async Task Remove(Guid id, Guid currentAdminId, CancellationToken token = default)
  {
    if (id == currentAdminId)
    {
      throw new ForbiddenException("You cannot delete your own account");
    }

    var user = await userManager.FindByIdAsync(id.ToString());
    if (user is null)
    {
      return;
    }

    await userManager.DeleteAsync(user);
  }

  private async Task<AdminUserDto> MapToDtoAsync(ApplicationUser user)
  {
    var roles = await userManager.GetRolesAsync(user);

    return new AdminUserDto
    {
      Id = user.Id,
      Email = user.Email ?? string.Empty,
      Role = roles.FirstOrDefault() ?? string.Empty,
      CreatedAt = user.CreatedAt,
    };
  }

  private static IEnumerable<ValidationFailure> ToValidationFailures(IEnumerable<IdentityError> errors) =>
    errors.Select(e => new ValidationFailure(e.Code, e.Description));
}
