namespace MovieAPI.Application.Models;

public record class GenreForChangeDto
{
  public string Name { get; set; } = string.Empty;
  public string Slug { get; set; } = string.Empty;
}
