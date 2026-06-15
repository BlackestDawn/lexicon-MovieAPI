namespace MovieAPI.Domain.Entities;

// Movie <-> Genre junction table
public class MovieGenre
{
  public Guid Id { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }

  public Guid MovieId { get; set; }
  public Movie Movie { get; set; }

  public Guid GenreId { get; set; }
  public Genre Genre { get; set; }
}
