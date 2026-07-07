namespace MovieAPI.Application.Models;

public class MovieSimpleDto
{
  public Guid Id { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public string Title { get; set; } = string.Empty;
  public DateOnly ReleaseDate { get; set; }
  public int RuntimeMinutes { get; set; }
  public decimal AverageRating { get; set; }
}
