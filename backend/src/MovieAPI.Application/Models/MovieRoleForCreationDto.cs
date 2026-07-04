using MovieAPI.Domain.Models;

namespace MovieAPI.Application.Models;

public class MovieRoleForCreationDto
{
  public Guid MovieId { get; set; }
  public PersonRole Role { get; set; }
}
