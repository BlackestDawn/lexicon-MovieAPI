namespace MovieAPI.Domain.Entities;

public class Review
{
  public Guid Id { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public string AuthorName { get; set; } = string.Empty;
  public string Body { get; set; } = string.Empty;
  public int Score { get; set; }

  // Navigation
  public Guid MovieId { get; set; }
  public Movie Movie { get; set; }
}
