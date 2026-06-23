using System.Net;
using System.Net.Http.Json;
using MovieAPI.Application.Models;
using MovieAPI.IntegrationTests.Infrastructure;

namespace MovieAPI.IntegrationTests;

public class MoviesControllerTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
  [Fact]
  public async Task CreateMovie_WithValidData_Returns201WithLocation()
  {
    var (genreId, personId) = await CreateGenreAndPersonAsync();

    var response = await Client.PostAsJsonAsync("/api/movies", TestData.ValidMovie(genreId, personId));

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.NotNull(response.Headers.Location);

    var created = await response.Content.ReadFromJsonAsync<MovieDto>();
    Assert.NotNull(created);
    Assert.Equal("Test Movie", created!.Title);
    Assert.Single(created.Genres);
  }

  [Fact]
  public async Task CreateMovie_WithoutGenres_Returns400()
  {
    var (_, personId) = await CreateGenreAndPersonAsync();
    var movie = TestData.ValidMovie(Guid.NewGuid(), personId);
    movie.Genres.Clear();

    var response = await Client.PostAsJsonAsync("/api/movies", movie);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task CreateMovie_WithUnknownGenreId_Returns404()
  {
    var (_, personId) = await CreateGenreAndPersonAsync();

    var response = await Client.PostAsJsonAsync("/api/movies", TestData.ValidMovie(Guid.NewGuid(), personId));

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task GetMovies_ReturnsCreatedMoviesWithPaginationHeader()
  {
    var (genreId, personId) = await CreateGenreAndPersonAsync();
    await CreateMovieAsync(genreId, personId, "Movie One");
    await CreateMovieAsync(genreId, personId, "Movie Two");

    var response = await Client.GetAsync("/api/movies");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.True(response.Headers.Contains("X-Pagination"));

    var movies = await response.Content.ReadFromJsonAsync<List<MovieDto>>();
    Assert.Equal(2, movies!.Count);
  }

  [Fact]
  public async Task GetMovie_WithExistingId_Returns200()
  {
    var (genreId, personId) = await CreateGenreAndPersonAsync();
    var created = await CreateMovieAsync(genreId, personId);

    var response = await Client.GetAsync($"/api/movies/{created.Id}");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var movie = await response.Content.ReadFromJsonAsync<MovieExtendedDto>();
    Assert.Equal(created.Id, movie!.Id);
  }

  [Fact]
  public async Task GetMovie_WithUnknownId_Returns404()
  {
    var response = await Client.GetAsync($"/api/movies/{Guid.NewGuid()}");

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task UpdateMovie_WithExistingId_Returns204AndPersistsChange()
  {
    var (genreId, personId) = await CreateGenreAndPersonAsync();
    var created = await CreateMovieAsync(genreId, personId);

    var response = await Client.PutAsJsonAsync($"/api/movies/{created.Id}",
      TestData.ValidMovie(genreId, personId, "Updated Title"));

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var fetched = await Client.GetFromJsonAsync<MovieExtendedDto>($"/api/movies/{created.Id}");
    Assert.Equal("Updated Title", fetched!.Title);
  }

  [Fact]
  public async Task UpdateMovie_WithUnknownId_Returns404()
  {
    var (genreId, personId) = await CreateGenreAndPersonAsync();

    var response = await Client.PutAsJsonAsync($"/api/movies/{Guid.NewGuid()}", TestData.ValidMovie(genreId, personId));

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task PatchMovie_WithExistingId_Returns204AndPersistsChange()
  {
    var (genreId, personId) = await CreateGenreAndPersonAsync();
    var created = await CreateMovieAsync(genreId, personId);
    var patch = new[] { new { op = "replace", path = "/title", value = "Patched Title" } };

    var response = await PatchJsonPatchAsync($"/api/movies/{created.Id}", patch);

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var fetched = await Client.GetFromJsonAsync<MovieExtendedDto>($"/api/movies/{created.Id}");
    Assert.Equal("Patched Title", fetched!.Title);
  }

  [Fact]
  public async Task DeleteMovie_WithExistingId_Returns204AndSubsequentGetReturns404()
  {
    var (genreId, personId) = await CreateGenreAndPersonAsync();
    var created = await CreateMovieAsync(genreId, personId);

    var deleteResponse = await Client.DeleteAsync($"/api/movies/{created.Id}");
    Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

    var getResponse = await Client.GetAsync($"/api/movies/{created.Id}");
    Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
  }

  [Fact]
  public async Task DeleteMovie_WithUnknownId_ReturnsNoContent()
  {
    var response = await Client.DeleteAsync($"/api/movies/{Guid.NewGuid()}");

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
  }

  private async Task<(Guid GenreId, Guid PersonId)> CreateGenreAndPersonAsync()
  {
    var genreResponse = await Client.PostAsJsonAsync("/api/genres", TestData.ValidGenre());
    var genre = (await genreResponse.Content.ReadFromJsonAsync<GenreDto>())!;

    var personResponse = await Client.PostAsJsonAsync("/api/people", TestData.ValidPerson());
    var person = (await personResponse.Content.ReadFromJsonAsync<PersonDto>())!;

    return (genre.Id, person.Id);
  }

  private async Task<MovieDto> CreateMovieAsync(Guid genreId, Guid personId, string title = "Test Movie")
  {
    var response = await Client.PostAsJsonAsync("/api/movies", TestData.ValidMovie(genreId, personId, title));
    return (await response.Content.ReadFromJsonAsync<MovieDto>())!;
  }
}
