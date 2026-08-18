namespace MovieAPI.Application.Models;

public class AdminUserForCreationDto
{
  public string Email { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
  public string Role { get; set; } = string.Empty;

  // Falls back to the local part of the email when omitted - see AdminUserService.Create.
  public string? DisplayName { get; set; }
}
