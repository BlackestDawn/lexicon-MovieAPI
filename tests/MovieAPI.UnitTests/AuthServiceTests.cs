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

namespace MovieAPI.UnitTests;

public class AuthServiceTests
{
  private readonly Mock<UserManager<ApplicationUser>> _userManager;
  private readonly Mock<SignInManager<ApplicationUser>> _signInManager;
  private readonly Mock<IMapper> _mapper = new();
  private readonly Mock<IValidator<RegisterDto>> _registerValidator = new();
  private readonly Mock<IValidator<LoginDto>> _loginValidator = new();
  private readonly Mock<IValidator<UserForUpdateDto>> _updateValidator = new();
  private readonly AuthService _sut;

  public AuthServiceTests()
  {
    _userManager = MockUserManager();
    _signInManager = MockSignInManager(_userManager.Object);
    _sut = new AuthService(
      _userManager.Object,
      _signInManager.Object,
      _mapper.Object,
      _registerValidator.Object,
      _loginValidator.Object,
      _updateValidator.Object);
  }

  // Helpers

  private static Mock<UserManager<ApplicationUser>> MockUserManager()
  {
    var store = new Mock<IUserStore<ApplicationUser>>();
    return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
  }

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

    Assert.Equal(user.Id, result.Id);
  }

  // Logout

  [Fact]
  public async Task Logout_WhenUserNotFound_ThrowsNotFoundException()
  {
    _userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

    await Assert.ThrowsAsync<NotFoundException>(() => _sut.Logout(Guid.NewGuid(), CancellationToken.None));
  }

  [Fact]
  public async Task Logout_WhenUserExists_UpdatesSecurityStamp()
  {
    var user = MakeUser();
    _userManager.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
    _userManager.Setup(m => m.UpdateSecurityStampAsync(user)).ReturnsAsync(IdentityResult.Success);

    await _sut.Logout(user.Id, CancellationToken.None);

    _userManager.Verify(m => m.UpdateSecurityStampAsync(user), Times.Once);
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
}
