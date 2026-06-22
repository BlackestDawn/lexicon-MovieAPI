namespace MovieAPI.Application.Models;

public class GenreCreationResult
{
  public bool Success { get; init; }
  public Exception? Error { get; init; }
  public GenreDto? Genre { get; init; }

  public static GenreCreationResult Successful(GenreDto genre) =>
    new() { Success = true, Genre = genre };
  public static GenreCreationResult Failed(Exception error) =>
    new() { Success = false, Error = error };
}
