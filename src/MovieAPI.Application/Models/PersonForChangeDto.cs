namespace MovieAPI.Application.Models;

public class PersonForChangeDto
{
  public string GivenName { get; set; } = string.Empty;
  public string? MiddleName { get; set; }
  public string LastName { get; set; } = string.Empty;
  public DateOnly DateOfBirth { get; set; }
  public ICollection<MovieRoleForCreationDto> MovieRoles { get; set; } = [];
}
