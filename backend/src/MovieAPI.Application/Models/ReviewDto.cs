namespace MovieAPI.Application.Models;

public class ReviewDto
{
  public Guid Id { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public string AuthorName { get; set; } = string.Empty;
  public string Body { get; set; } = string.Empty;
  public int Score { get; set; }
  public Guid? UserId { get; set; }
}
