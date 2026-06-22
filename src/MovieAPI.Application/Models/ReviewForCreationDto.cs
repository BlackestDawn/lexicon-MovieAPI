namespace MovieAPI.Application.Models;

public class ReviewForCreationDto
{
  public string AuthorName { get; set; } = string.Empty;
  public string Body { get; set; } = string.Empty;
  public int Score { get; set; }
}
