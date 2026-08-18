namespace MovieAPI.Infrastructure.Models;

public class MovieSearchParams(string? name, string? freeSearch, string? genre, int? year, decimal? minRating, decimal? maxRating)
{
  public string? Name { get; init; } = name;
  public string? Search { get; init; } = freeSearch;
  public string? Genre { get; init; } = genre;
  public int? Year { get; init; } = year;
  public decimal? MinRating { get; init; } = minRating;
  public decimal? MaxRating { get; init; } = maxRating;
}
