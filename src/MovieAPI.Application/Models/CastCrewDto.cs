using MovieAPI.Domain.Models;

namespace MovieAPI.Application.Models;

public class CastCrewDto
{
  public Guid PersonId { get; set; }
  public string FirstName { get; set; } = string.Empty;
  public string LastName { get; set; } = string.Empty;
  public PersonRole Role { get; set; }
}
