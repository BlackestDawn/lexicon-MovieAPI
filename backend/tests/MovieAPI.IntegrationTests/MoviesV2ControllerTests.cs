using System.Net;
using System.Net.Http.Json;
using MovieAPI.Application.Models;
using MovieAPI.IntegrationTests.Infrastructure;

namespace MovieAPI.IntegrationTests;

public class MoviesV2ControllerTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
  // V2's MovieExtendedDto embeds CastCrewDto as-is (GivenName/MiddleName), unlike V1
  // which translates it down to CastCrewV1Dto (FirstName only) - see GetMovie_WithExistingId_ReturnsCastCrewWithFirstName
  // in MoviesControllerTests.cs for the V1 contrast.
  [Fact]
  public async Task GetMovie_WithExistingId_ReturnsCastCrewWithGivenNameAndMiddleName()
  {
    var (genreId, personId) = await CreateGenreAndPersonAsync(middleName: "Augusta");
    var created = await CreateMovieAsync(genreId, personId);

    var response = await Client.GetAsync($"/api/v2/movies/{created.Id}");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var movie = await response.Content.ReadFromJsonAsync<MovieExtendedDto>();
    var castCrew = Assert.Single(movie!.CastCrews!);
    Assert.Equal("Ada", castCrew.GivenName);
    Assert.Equal("Augusta", castCrew.MiddleName);
  }

  private async Task<(Guid GenreId, Guid PersonId)> CreateGenreAndPersonAsync(string? middleName = null)
  {
    var genreResponse = await Client.PostAsJsonAsync("/api/v2/genres", TestData.ValidGenre());
    var genre = (await genreResponse.Content.ReadFromJsonAsync<GenreDto>())!;

    var personResponse = await Client.PostAsJsonAsync("/api/v2/people", TestData.ValidPersonV2(middleName: middleName));
    var person = (await personResponse.Content.ReadFromJsonAsync<PersonDto>())!;

    return (genre.Id, person.Id);
  }

  private async Task<MovieDto> CreateMovieAsync(Guid genreId, Guid personId, string title = "Test Movie")
  {
    var response = await Client.PostAsJsonAsync("/api/v2/movies", TestData.ValidMovie(genreId, personId, title));
    return (await response.Content.ReadFromJsonAsync<MovieDto>())!;
  }
}
