using System.Net;
using System.Net.Http.Json;
using MovieAPI.Application.Models;
using MovieAPI.IntegrationTests.Infrastructure;

namespace MovieAPI.IntegrationTests;

// V3's only difference from V2 is the "persons" route/DTO naming carried over from
// PersonsV3ControllerTests, plus the renamed includePersons query parameter here
// (was includePeople in V1/V2) - the DTOs and behavior are otherwise identical.
public class MoviesV3ControllerTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
  [Fact]
  public async Task GetMovie_DefaultsToIncludingCastCrews()
  {
    var (genreId, personId) = await CreateGenreAndPersonAsync();
    var created = await CreateMovieAsync(genreId, personId);

    var response = await Client.GetAsync($"/api/v3/movies/{created.Id}");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var movie = await response.Content.ReadFromJsonAsync<MovieExtendedDto>();
    Assert.Single(movie!.CastCrews!);
  }

  [Fact]
  public async Task GetMovie_WithIncludePersonsFalse_OmitsCastCrews()
  {
    var (genreId, personId) = await CreateGenreAndPersonAsync();
    var created = await CreateMovieAsync(genreId, personId);

    var response = await Client.GetAsync($"/api/v3/movies/{created.Id}?includePersons=false");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var movie = await response.Content.ReadFromJsonAsync<MovieExtendedDto>();
    Assert.Empty(movie!.CastCrews!);
  }

  [Fact]
  public async Task GetMovies_FiltersByMaxRating()
  {
    var (genreId, personId) = await CreateGenreAndPersonAsync();
    var highRated = await CreateMovieAsync(genreId, personId, "High Rated");
    var lowRated = await CreateMovieAsync(genreId, personId, "Low Rated");

    await Client.PostAsJsonAsync($"/api/v3/movies/{highRated.Id}/reviews", TestData.ValidReview(9));
    await Client.PostAsJsonAsync($"/api/v3/movies/{lowRated.Id}/reviews", TestData.ValidReview(3));

    var response = await Client.GetAsync("/api/v3/movies?maxRating=5");

    var movies = await response.Content.ReadFromJsonAsync<List<MovieDto>>();
    Assert.Contains(movies!, m => m.Id == lowRated.Id);
    Assert.DoesNotContain(movies!, m => m.Id == highRated.Id);
  }

  private async Task<(Guid GenreId, Guid PersonId)> CreateGenreAndPersonAsync()
  {
    var genreResponse = await Client.PostAsJsonAsync("/api/v3/genres", TestData.ValidGenre());
    var genre = (await genreResponse.Content.ReadFromJsonAsync<GenreDto>())!;

    var personResponse = await Client.PostAsJsonAsync("/api/v3/persons", TestData.ValidPersonV2());
    var person = (await personResponse.Content.ReadFromJsonAsync<PersonDto>())!;

    return (genre.Id, person.Id);
  }

  private async Task<MovieDto> CreateMovieAsync(Guid genreId, Guid personId, string title = "Test Movie")
  {
    var response = await Client.PostAsJsonAsync("/api/v3/movies", TestData.ValidMovie(genreId, personId, title));
    return (await response.Content.ReadFromJsonAsync<MovieDto>())!;
  }
}
