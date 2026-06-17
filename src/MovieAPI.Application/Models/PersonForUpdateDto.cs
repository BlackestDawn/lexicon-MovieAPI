namespace MovieAPI.Application.Models;

public class PersonForUpdateDto
{
  public string FirstName { get; set; } = string.Empty;
  public string LastName { get; set; } = string.Empty;
  public DateOnly DateOfBirth { get; set; }
}
