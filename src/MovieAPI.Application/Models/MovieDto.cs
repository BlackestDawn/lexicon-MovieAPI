namespace MovieAPI.Application.Models;

public record class MovieDto
{
  public Guid Id { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public string Title { get; set; } = string.Empty;
  public DateOnly ReleaseDate { get; set; }
  public string PlotSummery { get; set; } = string.Empty;
  public int RuntimeMinutes { get; set; }
  public ICollection<GenreDto> Genres { get; set; } = [];
  public decimal AverageRating { get; set; }
}
