using System.Net;
using System.Net.Http.Json;
using MovieAPI.Application.Models;
using MovieAPI.IntegrationTests.Infrastructure;

namespace MovieAPI.IntegrationTests;

// V3's only difference from V2 is the route ("persons" instead of "people") and the
// includePersons query parameter naming - the DTOs and behavior are otherwise identical,
// so this focuses on the route itself rather than re-covering every V2 case.
public class PersonsV3ControllerTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
  [Fact]
  public async Task CreatePerson_Returns201AndLocationUnderV3Persons()
  {
    var response = await Client.PostAsJsonAsync("/api/v3/persons", TestData.ValidPersonV2(middleName: "Augusta"));

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.NotNull(response.Headers.Location);
    Assert.Contains("/api/v3/persons/", response.Headers.Location!.ToString());

    var created = await response.Content.ReadFromJsonAsync<PersonDto>();
    Assert.Equal("Ada", created!.GivenName);
    Assert.Equal("Augusta", created.MiddleName);
  }

  [Fact]
  public async Task GetPersons_ReturnsCreatedPersonsWithPaginationHeader()
  {
    await CreatePersonAsync("Ada", "Lovelace");
    await CreatePersonAsync("Grace", "Hopper");

    var response = await Client.GetAsync("/api/v3/persons");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.True(response.Headers.Contains("X-Pagination"));

    var persons = await response.Content.ReadFromJsonAsync<List<PersonDto>>();
    Assert.Equal(2, persons!.Count);
  }

  // Regression test: the year filter must match the person's own birth year, not the
  // release year of a movie they appeared in. TestData.ValidPersonV2 sets DateOfBirth to
  // 1980, while ValidMovie sets ReleaseDate to 2020, so the two years being disjoint
  // exposes the bug where the query joined through CastCrews to the movie's ReleaseDate.
  [Fact]
  public async Task GetPersons_FiltersByBirthYear_NotMovieReleaseYear()
  {
    var genreResponse = await Client.PostAsJsonAsync("/api/v3/genres", TestData.ValidGenre());
    var genre = (await genreResponse.Content.ReadFromJsonAsync<GenreDto>())!;

    var created = await CreatePersonAsync("Ada", "Lovelace");
    await Client.PostAsJsonAsync("/api/v3/movies", TestData.ValidMovie(genre.Id, created.Id));

    var byBirthYear = await Client.GetAsync("/api/v3/persons?year=1980");
    var byMovieReleaseYear = await Client.GetAsync("/api/v3/persons?year=2020");

    var matchedByBirthYear = await byBirthYear.Content.ReadFromJsonAsync<List<PersonDto>>();
    var matchedByReleaseYear = await byMovieReleaseYear.Content.ReadFromJsonAsync<List<PersonDto>>();

    Assert.Single(matchedByBirthYear!);
    Assert.Empty(matchedByReleaseYear!);
  }

  [Fact]
  public async Task GetPerson_WithExistingId_ReturnsPerson()
  {
    var created = await CreatePersonAsync(middleName: "Augusta");

    var response = await Client.GetAsync($"/api/v3/persons/{created.Id}");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var person = await response.Content.ReadFromJsonAsync<PersonExtendedDto>();
    Assert.Equal("Ada", person!.GivenName);
    Assert.Equal("Augusta", person.MiddleName);
  }

  [Fact]
  public async Task UpdatePerson_ChangesMiddleName()
  {
    var created = await CreatePersonAsync(middleName: "Augusta");

    var response = await Client.PutAsJsonAsync($"/api/v3/persons/{created.Id}", TestData.ValidPersonV2(middleName: "Byron"));

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var fetched = await Client.GetFromJsonAsync<PersonExtendedDto>($"/api/v3/persons/{created.Id}");
    Assert.Equal("Byron", fetched!.MiddleName);
  }

  [Fact]
  public async Task PatchPerson_ReplaceMiddleName_Returns204AndPersistsChange()
  {
    var created = await CreatePersonAsync();
    var patch = new[] { new { op = "replace", path = "/middleName", value = "Augusta" } };

    var response = await PatchJsonPatchAsync($"/api/v3/persons/{created.Id}", patch);

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var fetched = await Client.GetFromJsonAsync<PersonExtendedDto>($"/api/v3/persons/{created.Id}");
    Assert.Equal("Augusta", fetched!.MiddleName);
  }

  [Fact]
  public async Task DeletePerson_RemovesPerson()
  {
    var created = await CreatePersonAsync();

    var deleteResponse = await Client.DeleteAsync($"/api/v3/persons/{created.Id}");
    Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

    var getResponse = await Client.GetAsync($"/api/v3/persons/{created.Id}");
    Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
  }

  private async Task<PersonDto> CreatePersonAsync(string givenName = "Ada", string? middleName = null, string lastName = "Lovelace")
  {
    var response = await Client.PostAsJsonAsync("/api/v3/persons", TestData.ValidPersonV2(givenName, middleName, lastName));
    return (await response.Content.ReadFromJsonAsync<PersonDto>())!;
  }
}
