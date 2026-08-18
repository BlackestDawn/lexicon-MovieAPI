namespace MovieAPI.Application.Models;

public class AdminUserForUpdateDto
{
  public string Email { get; set; } = string.Empty;
  public string Role { get; set; } = string.Empty;

  // Left unchanged when omitted - see AdminUserService.Update.
  public string? DisplayName { get; set; }
}
