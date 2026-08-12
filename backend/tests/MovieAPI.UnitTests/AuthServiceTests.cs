using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Moq;
using MovieAPI.Application.Exceptions;
using MovieAPI.Application.Models;
using MovieAPI.Application.Services;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Interfaces;
using OpenIddict.Abstractions;

namespace MovieAPI.UnitTests;

public class AuthServiceTests
{
  private readonly Mock<UserManager<ApplicationUser>> _userManager;
  private readonly Mock<IOpenIddictAuthorizationManager> _authorizationManager = new();
  private readonly Mock<IMapper> _mapper = new();
  private readonly Mock<IValidator<RegisterDto>> _registerValidator = new();
  private readonly Mock<IValidator<UserForUpdateDto>> _updateValidator = new();
  private readonly Mock<IValidator<ChangePasswordDto>> _changePasswordValidator = new();
  private readonly Mock<IValidator<ForgotPasswordDto>> _forgotPasswordValidator = new();
  private readonly Mock<IValidator<ResetPasswordDto>> _resetPasswordValidator = new();
  private readonly Mock<IEmailSender> _emailSender = new();
  private readonly AuthService _sut;

  public AuthServiceTests()
  {
    _userManager = IdentityMocks.MockUserManager();
    _userManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync([]);
    _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
    _authorizationManager
      .Setup(m => m.FindBySubjectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .Returns(EmptyAuthorizations());
    _sut = new AuthService(
      _userManager.Object,
      _authorizationManager.Object,
      _emailSender.Object,
      _mapper.Object,
      _registerValidator.Object,
      _updateValidator.Object,
      _changePasswordValidator.Object,
      _forgotPasswordValidator.Object,
      _resetPasswordValidator.Object);
  }

  // Helpers

  private static async IAsyncEnumerable<object> EmptyAuthorizations()
  {
    await Task.CompletedTask;
    yield break;
  }

  private static async IAsyncEnumerable<object> SingleAuthorization(object authorization)
  {
    await Task.CompletedTask;
    yield return authorization;
  }

  private static RegisterDto MakeRegisterDto() => new() { Email = "user@test.com", Password = "Password123!" };

  private static UserForUpdateDto MakeUpdateDto() => new() { Email = "new@test.com" };

  private static ChangePasswordDto MakeChangePasswordDto() => new() { CurrentPassword = "OldPassword123!", NewPassword = "NewPassword123!" };

  private static ForgotPasswordDto MakeForgotPasswordDto() => new() { Email = "user@test.com" };

  private static ResetPasswordDto MakeResetPasswordDto() => new() { Email = "user@test.com", Token = "reset-token", NewPassword = "NewPassword123!" };

  private static ApplicationUser MakeUser(Guid? id = null, string email = "user@test.com") =>
    new() { Id = id ?? Guid.NewGuid(), Email = email, UserName = email };

  private static UserDto MakeUserDto(ApplicationUser user) => new() { Id = user.Id, Email = user.Email! };

  private void SetupValidatorsValid()
  {
    _registerValidator
      .Setup(v => v.ValidateAsync(It.IsAny<RegisterDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult());
    _updateValidator
      .Setup(v => v.ValidateAsync(It.IsAny<UserForUpdateDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult());
    _changePasswordValidator
      .Setup(v => v.ValidateAsync(It.IsAny<ChangePasswordDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult());
    _forgotPasswordValidator
      .Setup(v => v.ValidateAsync(It.IsAny<ForgotPasswordDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult());
    _resetPasswordValidator
      .Setup(v => v.ValidateAsync(It.IsAny<ResetPasswordDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult());
  }

  // Register

  [Fact]
  public async Task Register_WhenValidationFails_ThrowsValidationException()
  {
    _registerValidator
      .Setup(v => v.ValidateAsync(It.IsAny<RegisterDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult([new ValidationFailure("Email", "Required")]));

    await Assert.ThrowsAsync<ValidationException>(() => _sut.Register(MakeRegisterDto(), CancellationToken.None));
  }

  [Fact]
  public async Task Register_WhenIdentityCreateFails_ThrowsValidationException()
  {
    SetupValidatorsValid();
    _userManager
      .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
      .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "DuplicateEmail", Description = "Email taken" }));

    await Assert.ThrowsAsync<ValidationException>(() => _sut.Register(MakeRegisterDto(), CancellationToken.None));
  }

  [Fact]
  public async Task Register_WhenSucceeds_CreatesUserWithEmailAsUserNameAndReturnsMappedDto()
  {
    SetupValidatorsValid();
    var dto = MakeRegisterDto();
    ApplicationUser? createdUser = null;
    _userManager
      .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
      .Callback<ApplicationUser, string>((u, _) => createdUser = u)
      .ReturnsAsync(IdentityResult.Success);
    _mapper
      .Setup(m => m.Map<UserDto>(It.IsAny<ApplicationUser>()))
      .Returns((ApplicationUser u) => MakeUserDto(u));

    var result = await _sut.Register(dto, CancellationToken.None);

    Assert.Equal(dto.Email, result.Email);
    Assert.Equal(dto.Email, createdUser!.Email);
    Assert.Equal(dto.Email, createdUser.UserName);
  }

  [Fact]
  public async Task Register_WhenSucceeds_AssignsDefaultUserRole()
  {
    SetupValidatorsValid();
    var dto = MakeRegisterDto();
    _userManager
      .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
      .ReturnsAsync(IdentityResult.Success);
    _mapper
      .Setup(m => m.Map<UserDto>(It.IsAny<ApplicationUser>()))
      .Returns((ApplicationUser u) => MakeUserDto(u));

    await _sut.Register(dto, CancellationToken.None);

    _userManager.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), MovieAPI.Domain.Constants.Roles.User), Times.Once);
  }

  // Update

  [Fact]
  public async Task Update_WhenValidationFails_ThrowsValidationException()
  {
    _updateValidator
      .Setup(v => v.ValidateAsync(It.IsAny<UserForUpdateDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult([new ValidationFailure("Email", "Required")]));

    await Assert.ThrowsAsync<ValidationException>(() => _sut.Update(Guid.NewGuid(), MakeUpdateDto(), CancellationToken.None));
  }

  [Fact]
  public async Task Update_WhenUserNotFound_ThrowsNotFoundException()
  {
    SetupValidatorsValid();
    _userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

    await Assert.ThrowsAsync<NotFoundException>(() => _sut.Update(Guid.NewGuid(), MakeUpdateDto(), CancellationToken.None));
  }

  [Fact]
  public async Task Update_WhenEmailUnchanged_DoesNotCallSetEmail()
  {
    SetupValidatorsValid();
    var user = MakeUser(email: "same@test.com");
    var dto = new UserForUpdateDto { Email = "same@test.com" };
    _userManager.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
    _mapper.Setup(m => m.Map<UserDto>(user)).Returns(MakeUserDto(user));

    await _sut.Update(user.Id, dto, CancellationToken.None);

    _userManager.Verify(m => m.SetEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task Update_WhenSetEmailFails_ThrowsValidationException()
  {
    SetupValidatorsValid();
    var user = MakeUser(email: "old@test.com");
    _userManager.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
    _userManager
      .Setup(m => m.SetEmailAsync(user, "new@test.com"))
      .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "InvalidEmail", Description = "Bad email" }));

    await Assert.ThrowsAsync<ValidationException>(() => _sut.Update(user.Id, MakeUpdateDto(), CancellationToken.None));
  }

  [Fact]
  public async Task Update_WhenSucceeds_UpdatesEmailAndUserNameAndReturnsMappedDto()
  {
    SetupValidatorsValid();
    var user = MakeUser(email: "old@test.com");
    _userManager.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
    _userManager.Setup(m => m.SetEmailAsync(user, "new@test.com")).ReturnsAsync(IdentityResult.Success);
    _userManager.Setup(m => m.SetUserNameAsync(user, "new@test.com")).ReturnsAsync(IdentityResult.Success);
    _mapper.Setup(m => m.Map<UserDto>(user)).Returns(MakeUserDto(user));

    var result = await _sut.Update(user.Id, MakeUpdateDto(), CancellationToken.None);

    _userManager.Verify(m => m.SetEmailAsync(user, "new@test.com"), Times.Once);
    _userManager.Verify(m => m.SetUserNameAsync(user, "new@test.com"), Times.Once);
    Assert.Equal(user.Id, result.Id);
  }

  // ChangePassword

  [Fact]
  public async Task ChangePassword_WhenValidationFails_ThrowsValidationException()
  {
    _changePasswordValidator
      .Setup(v => v.ValidateAsync(It.IsAny<ChangePasswordDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult([new ValidationFailure("NewPassword", "Required")]));

    await Assert.ThrowsAsync<ValidationException>(
      () => _sut.ChangePassword(Guid.NewGuid(), MakeChangePasswordDto(), CancellationToken.None));
  }

  [Fact]
  public async Task ChangePassword_WhenUserNotFound_ThrowsNotFoundException()
  {
    SetupValidatorsValid();
    _userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

    await Assert.ThrowsAsync<NotFoundException>(
      () => _sut.ChangePassword(Guid.NewGuid(), MakeChangePasswordDto(), CancellationToken.None));
  }

  [Fact]
  public async Task ChangePassword_WhenCurrentPasswordIsWrong_ThrowsValidationException()
  {
    SetupValidatorsValid();
    var user = MakeUser();
    var dto = MakeChangePasswordDto();
    _userManager.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
    _userManager
      .Setup(m => m.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword))
      .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "PasswordMismatch", Description = "Incorrect password." }));

    await Assert.ThrowsAsync<ValidationException>(() => _sut.ChangePassword(user.Id, dto, CancellationToken.None));
  }

  [Fact]
  public async Task ChangePassword_WhenSucceeds_CallsChangePasswordAsync()
  {
    SetupValidatorsValid();
    var user = MakeUser();
    var dto = MakeChangePasswordDto();
    _userManager.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
    _userManager
      .Setup(m => m.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword))
      .ReturnsAsync(IdentityResult.Success);

    await _sut.ChangePassword(user.Id, dto, CancellationToken.None);

    _userManager.Verify(m => m.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword), Times.Once);
  }

  [Fact]
  public async Task ChangePassword_WhenSucceeds_RevokesAllAuthorizations()
  {
    SetupValidatorsValid();
    var user = MakeUser();
    var dto = MakeChangePasswordDto();
    var authorization = new object();
    _userManager.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
    _userManager
      .Setup(m => m.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword))
      .ReturnsAsync(IdentityResult.Success);
    _authorizationManager
      .Setup(m => m.FindBySubjectAsync(user.Id.ToString(), It.IsAny<CancellationToken>()))
      .Returns(SingleAuthorization(authorization));

    await _sut.ChangePassword(user.Id, dto, CancellationToken.None);

    _authorizationManager.Verify(m => m.TryRevokeAsync(authorization, It.IsAny<CancellationToken>()), Times.Once);
  }

  // ForgotPassword

  [Fact]
  public async Task ForgotPassword_WhenValidationFails_ThrowsValidationException()
  {
    _forgotPasswordValidator
      .Setup(v => v.ValidateAsync(It.IsAny<ForgotPasswordDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult([new ValidationFailure("Email", "Required")]));

    await Assert.ThrowsAsync<ValidationException>(() => _sut.ForgotPassword(MakeForgotPasswordDto(), CancellationToken.None));
  }

  [Fact]
  public async Task ForgotPassword_WhenEmailUnknown_DoesNotSendEmail()
  {
    SetupValidatorsValid();
    _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

    await _sut.ForgotPassword(MakeForgotPasswordDto(), CancellationToken.None);

    _emailSender.Verify(
      e => e.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
      Times.Never);
  }

  [Fact]
  public async Task ForgotPassword_WhenEmailKnown_SendsResetEmail()
  {
    SetupValidatorsValid();
    var user = MakeUser();
    var dto = MakeForgotPasswordDto();
    _userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
    _userManager.Setup(m => m.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");

    await _sut.ForgotPassword(dto, CancellationToken.None);

    _emailSender.Verify(e => e.SendPasswordResetEmailAsync(dto.Email, "reset-token", It.IsAny<CancellationToken>()), Times.Once);
  }

  // ResetPassword

  [Fact]
  public async Task ResetPassword_WhenValidationFails_ThrowsValidationException()
  {
    _resetPasswordValidator
      .Setup(v => v.ValidateAsync(It.IsAny<ResetPasswordDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult([new ValidationFailure("Token", "Required")]));

    await Assert.ThrowsAsync<ValidationException>(() => _sut.ResetPassword(MakeResetPasswordDto(), CancellationToken.None));
  }

  [Fact]
  public async Task ResetPassword_WhenEmailUnknown_ThrowsAuthenticationException()
  {
    SetupValidatorsValid();
    _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

    await Assert.ThrowsAsync<AuthenticationException>(() => _sut.ResetPassword(MakeResetPasswordDto(), CancellationToken.None));
  }

  [Fact]
  public async Task ResetPassword_WhenTokenInvalid_ThrowsValidationException()
  {
    SetupValidatorsValid();
    var user = MakeUser();
    var dto = MakeResetPasswordDto();
    _userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
    _userManager
      .Setup(m => m.ResetPasswordAsync(user, dto.Token, dto.NewPassword))
      .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "InvalidToken", Description = "Invalid token." }));

    await Assert.ThrowsAsync<ValidationException>(() => _sut.ResetPassword(dto, CancellationToken.None));
  }

  [Fact]
  public async Task ResetPassword_WhenSucceeds_RevokesAllAuthorizations()
  {
    SetupValidatorsValid();
    var user = MakeUser();
    var dto = MakeResetPasswordDto();
    var authorization = new object();
    _userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
    _userManager.Setup(m => m.ResetPasswordAsync(user, dto.Token, dto.NewPassword)).ReturnsAsync(IdentityResult.Success);
    _authorizationManager
      .Setup(m => m.FindBySubjectAsync(user.Id.ToString(), It.IsAny<CancellationToken>()))
      .Returns(SingleAuthorization(authorization));

    await _sut.ResetPassword(dto, CancellationToken.None);

    _authorizationManager.Verify(m => m.TryRevokeAsync(authorization, It.IsAny<CancellationToken>()), Times.Once);
  }
}
