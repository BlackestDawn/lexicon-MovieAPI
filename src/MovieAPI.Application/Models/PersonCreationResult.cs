namespace MovieAPI.Application.Models;

public class PersonCreationResult
{
  public bool Success { get; init; }
  public Exception? Error { get; init; }
  public PersonDto? Person { get; init; }

  public static PersonCreationResult Successful(PersonDto person) =>
    new() { Success = true, Person = person };
  public static PersonCreationResult Failed(Exception error) =>
    new() { Success = false, Error = error };
}
