namespace MovieAPI.Infrastructure.Models;

public class PeopleSearchParams(string? name, string? genre, int? year)
{
  public string? Name { get; init; } = name;
  public string? Genre { get; init; } = genre;
  public int? Year { get; init; } = year;
}
