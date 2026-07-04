namespace MovieAPI.Application.Models;

public class PersonExtendedDto
{
  public Guid Id { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public string GivenName { get; set; } = string.Empty;
  public string? MiddleName { get; set; }
  public string LastName { get; set; } = string.Empty;
  public DateOnly DateOfBirth { get; set; }

  public ICollection<MovieRoleDto> MovieRoles { get; set; } = [];
}
