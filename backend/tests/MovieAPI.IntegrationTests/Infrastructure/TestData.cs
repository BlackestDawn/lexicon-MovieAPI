using MovieAPI.Application.Models;
using MovieAPI.Application.Models.V1;
using MovieAPI.Domain.Models;

namespace MovieAPI.IntegrationTests.Infrastructure;

public static class TestData
{
  public static GenreForChangeDto ValidGenre(string name = "Sci-Fi", string slug = "sci-fi") =>
    new() { Name = name, Slug = slug };

  // Callers POST/PUT this against /api/v1/people, which binds PersonForChangeV1Dto
  // (FirstName, no MiddleName) - not the canonical PersonForChangeDto shape.
  public static PersonForChangeV1Dto ValidPerson(string firstName = "Ada", string lastName = "Lovelace") =>
    new() { FirstName = firstName, LastName = lastName, DateOfBirth = new DateOnly(1980, 1, 1) };

  // For /api/v2/people, which binds the canonical PersonForChangeDto shape
  // (GivenName + optional MiddleName) directly.
  public static PersonForChangeDto ValidPersonV2(string givenName = "Ada", string? middleName = null, string lastName = "Lovelace") =>
    new() { GivenName = givenName, MiddleName = middleName, LastName = lastName, DateOfBirth = new DateOnly(1980, 1, 1) };

  public static ReviewForChangeDto ValidReview(string author = "Reviewer", int score = 8) =>
    new() { AuthorName = author, Body = "A solid watch.", Score = score };

  public static MovieForChangeDto ValidMovie(Guid genreId, Guid personId, string title = "Test Movie") =>
    new()
    {
      Title = title,
      ReleaseDate = new DateOnly(2020, 1, 1),
      PlotSummery = "A movie used for testing.",
      RuntimeMinutes = 120,
      Synopsis = "Longer synopsis for testing purposes.",
      Language = "English",
      Budget = 1_000_000,
      Genres = [genreId],
      CastCrews = [new CastCrewForChangeDto { PersonId = personId, Role = PersonRole.Director }],
    };
}
