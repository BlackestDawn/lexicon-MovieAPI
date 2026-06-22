namespace MovieAPI.Application.Models;

public class GenreExtendedDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Slug { get; set; } = string.Empty;
  public ICollection<MovieDto> Movies { get; set; } = [];
}
