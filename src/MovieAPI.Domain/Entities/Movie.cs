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

  // Navigation
  public ICollection<Person> People { get; set; } = [];
  public ICollection<Genre> Genres { get; set; } = [];
  public ICollection<Review> Reviews { get; set; } = [];

  // Computed
  public decimal AverageRating => Reviews.Sum(r => r.Score) / Reviews.Count;
}
