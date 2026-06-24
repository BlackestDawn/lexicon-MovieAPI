namespace MovieAPI.Application.Models;

public class AuthResponseDto
{
  public UserDto User { get; set; } = null!;
  public string AccessToken { get; set; } = string.Empty;
  public DateTime ExpiresAtUtc { get; set; }
}
