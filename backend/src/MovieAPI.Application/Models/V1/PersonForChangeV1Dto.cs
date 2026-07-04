namespace MovieAPI.Application.Models.V1;

public class PersonForChangeV1Dto
{
  public string FirstName { get; set; } = string.Empty;
  public string LastName { get; set; } = string.Empty;
  public DateOnly DateOfBirth { get; set; }
  public ICollection<MovieRoleForCreationDto> MovieRoles { get; set; } = [];
}
