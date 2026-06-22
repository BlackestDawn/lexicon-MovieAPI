namespace MovieAPI.Application.Models;

public class ReviewCreationResult
{
  public bool Success { get; init; }
  public Exception? Error { get; init; }
  public ReviewDto? Review { get; init; }

  public static ReviewCreationResult Successful(ReviewDto review) =>
    new() { Success = true, Review = review };
  public static ReviewCreationResult Failed(Exception error) =>
    new() { Success = false, Error = error };
}
