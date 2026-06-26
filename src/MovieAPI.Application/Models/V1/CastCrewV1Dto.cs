using MovieAPI.Domain.Models;

namespace MovieAPI.Application.Models.V1;

public class CastCrewV1Dto
{
  public Guid PersonId { get; set; }
  public string FirstName { get; set; } = string.Empty;
  public string LastName { get; set; } = string.Empty;
  public PersonRole Role { get; set; }
}
