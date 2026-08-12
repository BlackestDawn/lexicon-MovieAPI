using MovieAPI.Application.Models;

namespace MovieAPI.Application.Interfaces;

public interface IAuthService
{
  Task<CurrentUserDto> GetCurrent(Guid userId, CancellationToken token = default);
  Task<UserDto> Register(RegisterDto newUser, CancellationToken token = default);
  Task<UserDto> Update(Guid userId, UserForUpdateDto updatedUser, CancellationToken token = default);
  Task ChangePassword(Guid userId, ChangePasswordDto changePassword, CancellationToken token = default);
  Task ForgotPassword(ForgotPasswordDto request, CancellationToken token = default);
  Task ResetPassword(ResetPasswordDto request, CancellationToken token = default);
}
