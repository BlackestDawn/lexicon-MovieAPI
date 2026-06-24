using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MovieAPI.Application.Exceptions;
using MovieAPI.Application.Models;
using MovieAPI.Application.Services;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Interfaces;

namespace MovieAPI.UnitTests;

public class AuthServiceTests
{
  private readonly Mock<UserManager<ApplicationUser>> _userManager;
  private readonly Mock<SignInManager<ApplicationUser>> _signInManager;
  private readonly Mock<ITokenService> _tokenService = new();
  private readonly Mock<IMapper> _mapper = new();
  private readonly Mock<IValidator<RegisterDto>> _registerValidator = new();
  private readonly Mock<IValidator<LoginDto>> _loginValidator = new();
  private readonly Mock<IValidator<UserForUpdateDto>> _updateValidator = new();
  private readonly Mock<IValidator<ChangePasswordDto>> _changePasswordValidator = new();
  private readonly Mock<IValidator<RefreshTokenDto>> _refreshTokenValidator = new();
  private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
  private readonly AuthService _sut;

  public AuthServiceTests()
  {
    _userManager = IdentityMocks.MockUserManager();
    _signInManager = MockSignInManager(_userManager.Object);
    _userManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync([]);
    _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
    _tokenService
      .Setup(t => t.GenerateToken(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()))
      .Returns(("access-token", DateTime.UtcNow.AddMinutes(60)));
    _tokenService
      .Setup(t => t.GenerateRefreshToken())
      .Returns(("refresh-token", DateTime.UtcNow.AddDays(7)));
    _tokenService
      .Setup(t => t.HashToken(It.IsAny<string>()))
      .Returns<string>(raw => $"hash:{raw}");
    _refreshTokenRepository
      .Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);
    _refreshTokenRepository
      .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(true);
    _sut = new AuthService(
      _userManager.Object,
      _signInManager.Object,
      _tokenService.Object,
      _refreshTokenRepository.Object,
      _mapper.Object,
      _registerValidator.Object,
      _loginValidator.Object,
      _updateValidator.Object,
      _changePasswordValidator.Object,
      _refreshTokenValidator.Object);
  }

  // Helpers

  private static Mock<SignInManager<ApplicationUser>> MockSignInManager(UserManager<ApplicationUser> userManager)
  {
    return new Mock<SignInManager<ApplicationUser>>(
      userManager,
      new HttpContextAccessor(),
      Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
      Mock.Of<IOptions<IdentityOptions>>(),
      Mock.Of<ILogger<SignInManager<ApplicationUser>>>(),
      Mock.Of<IAuthenticationSchemeProvider>(),
      Mock.Of<IUserConfirmation<ApplicationUser>>());
  }

  private static RegisterDto MakeRegisterDto() => new() { Email = "user@test.com", Password = "Password123!" };

  private static LoginDto MakeLoginDto() => new() { Email = "user@test.com", Password = "Password123!" };

  private static UserForUpdateDto MakeUpdateDto() => new() { Email = "new@test.com" };

  private static ChangePasswordDto MakeChangePasswordDto() => new() { CurrentPassword = "OldPassword123!", NewPassword = "NewPassword123!" };

  private static RefreshTokenDto MakeRefreshTokenDto(string token = "raw-refresh-token") => new() { RefreshToken = token };

  private static RefreshToken MakeRefreshTokenEntity(Guid? userId = null, string tokenHash = "hash:raw-refresh-token", bool revoked = false, bool expired = false) => new()
  {
    Id = Guid.NewGuid(),
    UserId = userId ?? Guid.NewGuid(),
    TokenHash = tokenHash,
    ExpiresAtUtc = expired ? DateTime.UtcNow.AddDays(-1) : DateTime.UtcNow.AddDays(7),
    RevokedAtUtc = revoked ? DateTime.UtcNow.AddMinutes(-1) : null,
  };

  private static ApplicationUser MakeUser(Guid? id = null, string email = "user@test.com") =>
    new() { Id = id ?? Guid.NewGuid(), Email = email, UserName = email };

  private static UserDto MakeUserDto(ApplicationUser user) => new() { Id = user.Id, Email = user.Email! };

  private void SetupValidatorsValid()
  {
    _registerValidator
      .Setup(v => v.ValidateAsync(It.IsAny<RegisterDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult());
    _loginValidator
      .Setup(v => v.ValidateAsync(It.IsAny<LoginDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult());
    _updateValidator
      .Setup(v => v.ValidateAsync(It.IsAny<UserForUpdateDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult());
    _changePasswordValidator
      .Setup(v => v.ValidateAsync(It.IsAny<ChangePasswordDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult());
    _refreshTokenValidator
      .Setup(v => v.ValidateAsync(It.IsAny<RefreshTokenDto>(), It.IsAny<CancellationToken>()))
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

    Assert.Equal(dto.Email, result.User.Email);
    Assert.Equal("access-token", result.AccessToken);
    Assert.Equal("refresh-token", result.RefreshToken);
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

  [Fact]
  public async Task Register_WhenSucceeds_PersistsRefreshToken()
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

    _refreshTokenRepository.Verify(
      r => r.AddAsync(It.Is<RefreshToken>(rt => rt.TokenHash == "hash:refresh-token"), It.IsAny<CancellationToken>()),
      Times.Once);
    _refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  // Login

  [Fact]
  public async Task Login_WhenValidationFails_ThrowsValidationException()
  {
    _loginValidator
      .Setup(v => v.ValidateAsync(It.IsAny<LoginDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult([new ValidationFailure("Email", "Required")]));

    await Assert.ThrowsAsync<ValidationException>(() => _sut.Login(MakeLoginDto(), CancellationToken.None));
  }

  [Fact]
  public async Task Login_WhenUserNotFound_ThrowsAuthenticationException()
  {
    SetupValidatorsValid();
    _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

    await Assert.ThrowsAsync<AuthenticationException>(() => _sut.Login(MakeLoginDto(), CancellationToken.None));
  }

  [Fact]
  public async Task Login_WhenPasswordIncorrect_ThrowsAuthenticationException()
  {
    SetupValidatorsValid();
    var user = MakeUser();
    _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
    _signInManager
      .Setup(m => m.CheckPasswordSignInAsync(user, It.IsAny<string>(), true))
      .ReturnsAsync(SignInResult.Failed);

    await Assert.ThrowsAsync<AuthenticationException>(() => _sut.Login(MakeLoginDto(), CancellationToken.None));
  }

  [Fact]
  public async Task Login_WhenSucceeds_ReturnsMappedDto()
  {
    SetupValidatorsValid();
    var user = MakeUser();
    _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
    _signInManager
      .Setup(m => m.CheckPasswordSignInAsync(user, It.IsAny<string>(), true))
      .ReturnsAsync(SignInResult.Success);
    _mapper.Setup(m => m.Map<UserDto>(user)).Returns(MakeUserDto(user));

    var result = await _sut.Login(MakeLoginDto(), CancellationToken.None);

    Assert.Equal(user.Id, result.User.Id);
    Assert.Equal("access-token", result.AccessToken);
    Assert.Equal("refresh-token", result.RefreshToken);
  }

  // Refresh

  [Fact]
  public async Task Refresh_WhenValidationFails_ThrowsValidationException()
  {
    _refreshTokenValidator
      .Setup(v => v.ValidateAsync(It.IsAny<RefreshTokenDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult([new ValidationFailure("RefreshToken", "Required")]));

    await Assert.ThrowsAsync<ValidationException>(() => _sut.Refresh(MakeRefreshTokenDto(), CancellationToken.None));
  }

  [Fact]
  public async Task Refresh_WhenTokenNotFound_ThrowsAuthenticationException()
  {
    SetupValidatorsValid();
    _refreshTokenRepository
      .Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((RefreshToken?)null);

    await Assert.ThrowsAsync<AuthenticationException>(() => _sut.Refresh(MakeRefreshTokenDto(), CancellationToken.None));
  }

  [Fact]
  public async Task Refresh_WhenTokenExpired_ThrowsAuthenticationException()
  {
    SetupValidatorsValid();
    var existing = MakeRefreshTokenEntity(expired: true);
    _refreshTokenRepository
      .Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(existing);

    await Assert.ThrowsAsync<AuthenticationException>(() => _sut.Refresh(MakeRefreshTokenDto(), CancellationToken.None));
  }

  [Fact]
  public async Task Refresh_WhenTokenAlreadyRevoked_RevokesAllActiveForUserAndThrows()
  {
    SetupValidatorsValid();
    var existing = MakeRefreshTokenEntity(revoked: true);
    _refreshTokenRepository
      .Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(existing);

    await Assert.ThrowsAsync<AuthenticationException>(() => _sut.Refresh(MakeRefreshTokenDto(), CancellationToken.None));

    _refreshTokenRepository.Verify(r => r.RevokeAllActiveForUserAsync(existing.UserId, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task Refresh_WhenUserNotFound_ThrowsAuthenticationException()
  {
    SetupValidatorsValid();
    var existing = MakeRefreshTokenEntity();
    _refreshTokenRepository
      .Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(existing);
    _userManager.Setup(m => m.FindByIdAsync(existing.UserId.ToString())).ReturnsAsync((ApplicationUser?)null);

    await Assert.ThrowsAsync<AuthenticationException>(() => _sut.Refresh(MakeRefreshTokenDto(), CancellationToken.None));
  }

  [Fact]
  public async Task Refresh_WhenTokenValid_RevokesOldTokenAndReturnsNewTokens()
  {
    SetupValidatorsValid();
    var user = MakeUser();
    var existing = MakeRefreshTokenEntity(userId: user.Id);
    _refreshTokenRepository
      .Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(existing);
    _userManager.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
    _mapper.Setup(m => m.Map<UserDto>(user)).Returns(MakeUserDto(user));

    var result = await _sut.Refresh(MakeRefreshTokenDto(), CancellationToken.None);

    Assert.NotNull(existing.RevokedAtUtc);
    Assert.Equal("access-token", result.AccessToken);
    Assert.Equal("refresh-token", result.RefreshToken);
  }

  // Logout

  [Fact]
  public async Task Logout_WhenTokenUnknown_DoesNotRevokeAnything()
  {
    SetupValidatorsValid();
    _refreshTokenRepository
      .Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((RefreshToken?)null);

    await _sut.Logout(Guid.NewGuid(), MakeRefreshTokenDto(), CancellationToken.None);

    _refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Logout_WhenTokenBelongsToDifferentUser_DoesNotRevokeIt()
  {
    SetupValidatorsValid();
    var existing = MakeRefreshTokenEntity();
    _refreshTokenRepository
      .Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(existing);

    await _sut.Logout(Guid.NewGuid(), MakeRefreshTokenDto(), CancellationToken.None);

    Assert.Null(existing.RevokedAtUtc);
    _refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Logout_WhenTokenAlreadyRevoked_DoesNotSaveAgain()
  {
    SetupValidatorsValid();
    var existing = MakeRefreshTokenEntity(revoked: true);
    _refreshTokenRepository
      .Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(existing);

    await _sut.Logout(existing.UserId, MakeRefreshTokenDto(), CancellationToken.None);

    _refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Logout_WhenTokenValid_RevokesIt()
  {
    SetupValidatorsValid();
    var existing = MakeRefreshTokenEntity();
    _refreshTokenRepository
      .Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(existing);

    await _sut.Logout(existing.UserId, MakeRefreshTokenDto(), CancellationToken.None);

    Assert.NotNull(existing.RevokedAtUtc);
    _refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
}
