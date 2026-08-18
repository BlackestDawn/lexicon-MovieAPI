namespace MovieAPI.Application.Models;

public class AdminUserDto
{
  public Guid Id { get; set; }
  public string Email { get; set; } = string.Empty;
  public string Role { get; set; } = string.Empty;
  public string DisplayName { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
}
