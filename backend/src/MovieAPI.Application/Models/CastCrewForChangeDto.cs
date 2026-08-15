using MovieAPI.Domain.Models;

namespace MovieAPI.Application.Models;

public class CastCrewForChangeDto
{
  public Guid PersonId { get; set; }
  public PersonRole Role { get; set; }
}
