namespace MovieAPI.Infrastructure.Models;

public class ReviewSearchParams(string? search, int? minScore, int? maxScore)
{
  public string? Search { get; init; } = search;
  public int? MinScore { get; init; } = minScore;
  public int? MaxScore { get; init; } = maxScore;
}
