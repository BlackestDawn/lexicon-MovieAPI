using MovieAPI.Application.Models;

namespace MovieAPI.Application.Interfaces;

public interface IAuthService
{
  Task<AuthResponseDto> Register(RegisterDto newUser, CancellationToken token = default);
  Task<AuthResponseDto> Login(LoginDto credentials, CancellationToken token = default);
  Task<AuthResponseDto> Refresh(RefreshTokenDto request, CancellationToken token = default);
  Task Logout(Guid userId, RefreshTokenDto request, CancellationToken token = default);
  Task<UserDto> Update(Guid userId, UserForUpdateDto updatedUser, CancellationToken token = default);
  Task ChangePassword(Guid userId, ChangePasswordDto changePassword, CancellationToken token = default);
}
