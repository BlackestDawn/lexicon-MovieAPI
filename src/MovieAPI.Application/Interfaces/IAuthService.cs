using MovieAPI.Application.Models;

namespace MovieAPI.Application.Interfaces;

public interface IAuthService
{
  Task<UserDto> Register(RegisterDto newUser, CancellationToken token = default);
  Task<UserDto> Login(LoginDto credentials, CancellationToken token = default);
  Task Logout(Guid userId, CancellationToken token = default);
  Task<UserDto> Update(Guid userId, UserForUpdateDto updatedUser, CancellationToken token = default);
}
