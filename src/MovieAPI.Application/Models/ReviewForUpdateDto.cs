namespace MovieAPI.Application.Models;

public class ReviewForUpdateDto
{
  public string AuthorName { get; set; } = string.Empty;
  public string Body { get; set; } = string.Empty;
  public int Score { get; set; }
}
