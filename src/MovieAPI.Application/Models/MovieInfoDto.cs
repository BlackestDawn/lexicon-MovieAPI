namespace MovieAPI.Application.Models;

public record class MovieInfoDto
{
  public Guid Id { get; set; }
  public string Synopsis { get; set; } = string.Empty;
  public string Language { get; set; } = string.Empty;
  public int Budget { get; set; }
}
