namespace MovieAPI.Domain.Entities;

public class Genre
{
  public Guid Id { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Slug { get; set; } = string.Empty;

  // Navigation
  public ICollection<Movie> Movies {get; set;} = [];
}
