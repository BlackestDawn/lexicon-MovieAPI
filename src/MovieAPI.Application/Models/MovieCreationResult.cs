namespace MovieAPI.Application.Models;

public class MovieCreationResult
{
  public bool Success { get; init; }
  public string? ErrorMessage { get; init; }
  public MovieDto? Movie { get; init; }

  public static MovieCreationResult Successful(MovieDto movie) =>
    new() { Success = true, Movie = movie };
  public static MovieCreationResult Failed(string message) =>
    new() { Success = false, ErrorMessage = message };
}
