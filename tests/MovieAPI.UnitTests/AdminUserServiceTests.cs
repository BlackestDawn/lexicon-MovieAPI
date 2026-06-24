using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Moq;
using MovieAPI.Application.Exceptions;
using MovieAPI.Application.Models;
using MovieAPI.Application.Services;
using MovieAPI.Domain.Constants;
using MovieAPI.Domain.Entities;

namespace MovieAPI.UnitTests;

public class AdminUserServiceTests
{
  private readonly Mock<UserManager<ApplicationUser>> _userManager;
  private readonly Mock<IValidator<AdminUserForCreationDto>> _creationValidator = new();
  private readonly Mock<IValidator<AdminUserForUpdateDto>> _updateValidator = new();
  private readonly AdminUserService _sut;

  public AdminUserServiceTests()
  {
    _userManager = IdentityMocks.MockUserManager();
    _creationValidator
      .Setup(v => v.ValidateAsync(It.IsAny<AdminUserForCreationDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult());
    _updateValidator
      .Setup(v => v.ValidateAsync(It.IsAny<AdminUserForUpdateDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult());
    _userManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync([]);

    _sut = new AdminUserService(_userManager.Object, _creationValidator.Object, _updateValidator.Object);
  }

  // Helpers

  private static ApplicationUser MakeUser(Guid? id = null, string email = "user@test.com") =>
    new() { Id = id ?? Guid.NewGuid(), Email = email, UserName = email, CreatedAt = DateTime.UtcNow };

  private static AdminUserForCreationDto MakeCreationDto() =>
    new() { Email = "new@test.com", Password = "Password123!", Role = Roles.User };

  private static AdminUserForUpdateDto MakeUpdateDto(string role = Roles.PowerUser) =>
    new() { Email = "updated@test.com", Role = role };

  // GetMany

  [Fact]
  public async Task GetMany_ReturnsPagedUsersWithRoles()
  {
    var users = Enumerable.Range(0, 3).Select(i => MakeUser(email: $"user{i}@test.com")).ToList();
    _userManager.Setup(m => m.Users).Returns(users.AsQueryable());
    _userManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync([Roles.User]);

    var (result, pagination) = await _sut.GetMany(1, 2, CancellationToken.None);

    Assert.Equal(2, result.Count());
    Assert.Equal(3, pagination.TotalItemCount);
    Assert.All(result, u => Assert.Equal(Roles.User, u.Role));
  }

  // GetOne

  [Fact]
  public async Task GetOne_WhenNotFound_ThrowsNotFoundException()
  {
    _userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

    await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetOne(Guid.NewGuid(), CancellationToken.None));
  }

  [Fact]
  public async Task GetOne_WhenFound_ReturnsDtoWithRole()
  {
    var user = MakeUser();
    _userManager.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
    _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync([Roles.Moderator]);

    var result = await _sut.GetOne(user.Id, CancellationToken.None);

    Assert.Equal(user.Id, result.Id);
    Assert.Equal(Roles.Moderator, result.Role);
  }

  // Create

  [Fact]
  public async Task Create_WhenValidationFails_ThrowsValidationException()
  {
    _creationValidator
      .Setup(v => v.ValidateAsync(It.IsAny<AdminUserForCreationDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult([new ValidationFailure("Role", "Invalid")]));

    await Assert.ThrowsAsync<ValidationException>(() => _sut.Create(MakeCreationDto(), CancellationToken.None));
  }

  [Fact]
  public async Task Create_WhenIdentityCreateFails_ThrowsValidationException()
  {
    _userManager
      .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
      .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "DuplicateEmail", Description = "Email taken" }));

    await Assert.ThrowsAsync<ValidationException>(() => _sut.Create(MakeCreationDto(), CancellationToken.None));
  }

  [Fact]
  public async Task Create_WhenSucceeds_AssignsRequestedRole()
  {
    var dto = MakeCreationDto();
    _userManager
      .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
      .ReturnsAsync(IdentityResult.Success);

    var result = await _sut.Create(dto, CancellationToken.None);

    Assert.Equal(dto.Email, result.Email);
    _userManager.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), dto.Role), Times.Once);
  }

  // Update

  [Fact]
  public async Task Update_WhenValidationFails_ThrowsValidationException()
  {
    _updateValidator
      .Setup(v => v.ValidateAsync(It.IsAny<AdminUserForUpdateDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult([new ValidationFailure("Role", "Invalid")]));

    await Assert.ThrowsAsync<ValidationException>(
      () => _sut.Update(Guid.NewGuid(), MakeUpdateDto(), Guid.NewGuid(), CancellationToken.None));
  }

  [Fact]
  public async Task Update_WhenSelfDemotingAwayFromAdministrator_ThrowsForbiddenException()
  {
    var adminId = Guid.NewGuid();

    await Assert.ThrowsAsync<ForbiddenException>(
      () => _sut.Update(adminId, MakeUpdateDto(Roles.PowerUser), adminId, CancellationToken.None));

    _userManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task Update_WhenSelfUpdateKeepsAdministratorRole_Succeeds()
  {
    var admin = MakeUser();
    _userManager.Setup(m => m.FindByIdAsync(admin.Id.ToString())).ReturnsAsync(admin);
    _userManager.Setup(m => m.GetRolesAsync(admin)).ReturnsAsync([Roles.Administrator]);
    _userManager.Setup(m => m.SetEmailAsync(admin, It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
    _userManager.Setup(m => m.SetUserNameAsync(admin, It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

    var result = await _sut.Update(admin.Id, MakeUpdateDto(Roles.Administrator), admin.Id, CancellationToken.None);

    Assert.Equal(admin.Id, result.Id);
  }

  [Fact]
  public async Task Update_WhenUserNotFound_ThrowsNotFoundException()
  {
    _userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

    await Assert.ThrowsAsync<NotFoundException>(
      () => _sut.Update(Guid.NewGuid(), MakeUpdateDto(), Guid.NewGuid(), CancellationToken.None));
  }

  [Fact]
  public async Task Update_WhenRoleChanges_RemovesOldRoleAndAddsNewOne()
  {
    var user = MakeUser(email: "updated@test.com");
    _userManager.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
    _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync([Roles.User]);

    await _sut.Update(user.Id, MakeUpdateDto(Roles.PowerUser), Guid.NewGuid(), CancellationToken.None);

    _userManager.Verify(m => m.RemoveFromRolesAsync(user, new[] { Roles.User }), Times.Once);
    _userManager.Verify(m => m.AddToRoleAsync(user, Roles.PowerUser), Times.Once);
  }

  [Fact]
  public async Task Update_WhenRoleUnchanged_DoesNotReassignRoles()
  {
    var user = MakeUser(email: "updated@test.com");
    _userManager.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
    _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync([Roles.PowerUser]);

    await _sut.Update(user.Id, MakeUpdateDto(Roles.PowerUser), Guid.NewGuid(), CancellationToken.None);

    _userManager.Verify(m => m.RemoveFromRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    _userManager.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
  }

  // Remove

  [Fact]
  public async Task Remove_WhenSelfDelete_ThrowsForbiddenException()
  {
    var adminId = Guid.NewGuid();

    await Assert.ThrowsAsync<ForbiddenException>(() => _sut.Remove(adminId, adminId, CancellationToken.None));

    _userManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task Remove_WhenUserNotFound_DoesNotThrow()
  {
    _userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

    await _sut.Remove(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

    _userManager.Verify(m => m.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Never);
  }

  [Fact]
  public async Task Remove_WhenUserFound_DeletesIt()
  {
    var user = MakeUser();
    _userManager.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
    _userManager.Setup(m => m.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

    await _sut.Remove(user.Id, Guid.NewGuid(), CancellationToken.None);

    _userManager.Verify(m => m.DeleteAsync(user), Times.Once);
  }
}
