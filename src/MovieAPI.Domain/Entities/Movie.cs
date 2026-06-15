namespace MovieAPI.Domain.Entities;

public class Movie
{
  public Guid Id { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public string Title { get; set; } = string.Empty;
  public DateOnly ReleaseDate { get; set; }
  public string PlotSummery { get; set; } = string.Empty;
  public int RuntimeMinutes { get; set; }
}
