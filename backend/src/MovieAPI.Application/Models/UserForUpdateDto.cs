namespace MovieAPI.Application.Models;

public class UserForUpdateDto
{
  public string Email { get; set; } = string.Empty;

  // Left unchanged when omitted - see AuthService.Update.
  public string? DisplayName { get; set; }
}
