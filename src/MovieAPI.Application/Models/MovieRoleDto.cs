using MovieAPI.Domain.Models;

namespace MovieAPI.Application.Models;

public class MovieRoleDto
{
  public Guid MovieId { get; set; }
  public string Title { get; set; } = string.Empty;
  public PersonRole Role { get; set; }
}
