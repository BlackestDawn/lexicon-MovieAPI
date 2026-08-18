using System.Net;
using System.Net.Http.Json;
using MovieAPI.Application.Models;
using MovieAPI.IntegrationTests.Infrastructure;

namespace MovieAPI.IntegrationTests;

// 3.1 is a non-breaking marker version, stacked onto the same controllers that already
// answer 3.0 (see the [ApiVersion] attributes) rather than a forked implementation like
// V1/V2/V3 - these just prove the version tag actually resolves for the resources it
// was added to, and stays unresolved for ones it wasn't (Genres, unchanged since 3.0).
public class ApiVersion31RoutingTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
  [Fact]
  public async Task GetPersons_V3_1_Returns200()
  {
    var response = await Client.GetAsync("/api/v3.1/persons");
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task GetMovies_V3_1_Returns200()
  {
    var response = await Client.GetAsync("/api/v3.1/movies");
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task GetReviews_V3_1_Returns200()
  {
    var genreResponse = await Client.PostAsJsonAsync("/api/v1/genres", TestData.ValidGenre());
    var genre = (await genreResponse.Content.ReadFromJsonAsync<GenreDto>())!;
    var personResponse = await Client.PostAsJsonAsync("/api/v1/people", TestData.ValidPerson());
    var person = (await personResponse.Content.ReadFromJsonAsync<PersonDto>())!;
    var movieResponse = await Client.PostAsJsonAsync("/api/v1/movies", TestData.ValidMovie(genre.Id, person.Id));
    var movie = (await movieResponse.Content.ReadFromJsonAsync<MovieDto>())!;

    var response = await Client.GetAsync($"/api/v3.1/movies/{movie.Id}/reviews");
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task GetMe_V3_1_Returns200()
  {
    var response = await Client.GetAsync("/api/v3.1/auth/me");
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task GetAdminUsers_V3_1_Returns200()
  {
    var response = await Client.GetAsync("/api/v3.1/admin/users");
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task GetGenres_V3_1_IsNotSupported()
  {
    // Genres never changed, so it was deliberately left off 3.1 - unlike Persons/Movies/
    // Reviews/Auth/AdminUsers above, this route shouldn't resolve under that version.
    var response = await Client.GetAsync("/api/v3.1/genres");
    Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
  }
}
