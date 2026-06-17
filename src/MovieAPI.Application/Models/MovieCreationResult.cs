namespace MovieAPI.Application.Models;

public class MovieCreationResult
{
  public bool Success { get; init; }
  public Exception? Error { get; init; }
  public MovieDto? Movie { get; init; }

  public static MovieCreationResult Successful(MovieDto movie) =>
    new() { Success = true, Movie = movie };
  public static MovieCreationResult Failed(Exception error) =>
    new() { Success = false, Error = error };
}
