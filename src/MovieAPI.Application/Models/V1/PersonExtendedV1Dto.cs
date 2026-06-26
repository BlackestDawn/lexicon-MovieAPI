namespace MovieAPI.Application.Models.V1;

public class PersonExtendedV1Dto
{
  public Guid Id { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public string FirstName { get; set; } = string.Empty;
  public string LastName { get; set; } = string.Empty;
  public DateOnly DateOfBirth { get; set; }

  public ICollection<MovieRoleDto> MovieRoles { get; set; } = [];
}
