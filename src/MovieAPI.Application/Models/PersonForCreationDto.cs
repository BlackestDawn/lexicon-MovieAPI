namespace MovieAPI.Application.Models;

public class PersonForCreationDto
{
  public string FirstName { get; set; } = string.Empty;
  public string LastName { get; set; } = string.Empty;
  public DateOnly DateOfBirth { get; set; }
  public ICollection<MovieRoleForCreationDto> MovieRoles { get; set; } = [];
}
