namespace MovieAPI.Application.Models;

public class RegisterDto
{
  public string Email { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;

  // Falls back to the local part of the email when omitted - see AuthService.Register.
  public string? DisplayName { get; set; }
}
