using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using MovieAPI.Application.Models;
using MovieAPI.Infrastructure;
using MovieAPI.IntegrationTests.Infrastructure;

namespace MovieAPI.IntegrationTests;

public class FavoritesControllerTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
  [Fact]
  public async Task GetMine_WithoutToken_Returns401()
  {
    var anonymous = Factory.CreateClient();

    var response = await anonymous.GetAsync("/api/v3.2/favorites");

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact]
  public async Task GetMine_ReturnsOnlyCurrentUsersFavorites()
  {
    var movieId = await CreateMovieAsync();
    var (client, _) = await RegisterAndLoginAsync();

    var addResponse = await client.PostAsync($"/api/v3.2/favorites/{movieId}", null);
    Assert.Equal(HttpStatusCode.NoContent, addResponse.StatusCode);

    // The shared Administrator client favorites it too - shouldn't leak into the other user's list.
    await Client.PostAsync($"/api/v3.2/favorites/{movieId}", null);

    var response = await client.GetAsync("/api/v3.2/favorites");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.True(response.Headers.Contains("X-Pagination"));

    var favorites = await response.Content.ReadFromJsonAsync<List<MovieDto>>();
    var favorite = Assert.Single(favorites!);
    Assert.Equal(movieId, favorite.Id);
  }

  [Fact]
  public async Task Add_WithUnknownMovieId_Returns404()
  {
    var response = await Client.PostAsync($"/api/v3.2/favorites/{Guid.NewGuid()}", null);

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task Add_CalledTwice_IsIdempotentAndListsOnce()
  {
    var movieId = await CreateMovieAsync();

    var first = await Client.PostAsync($"/api/v3.2/favorites/{movieId}", null);
    var second = await Client.PostAsync($"/api/v3.2/favorites/{movieId}", null);

    Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
    Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);

    var favorites = await Client.GetFromJsonAsync<List<MovieDto>>("/api/v3.2/favorites");
    Assert.Single(favorites!);
  }

  [Fact]
  public async Task GetOne_WhenFavorited_Returns200()
  {
    var movieId = await CreateMovieAsync();
    await Client.PostAsync($"/api/v3.2/favorites/{movieId}", null);

    var response = await Client.GetAsync($"/api/v3.2/favorites/{movieId}");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task GetOne_WhenNotFavorited_Returns404()
  {
    var movieId = await CreateMovieAsync();

    var response = await Client.GetAsync($"/api/v3.2/favorites/{movieId}");

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task Remove_WithExistingFavorite_Returns204AndSubsequentGetReturns404()
  {
    var movieId = await CreateMovieAsync();
    await Client.PostAsync($"/api/v3.2/favorites/{movieId}", null);

    var deleteResponse = await Client.DeleteAsync($"/api/v3.2/favorites/{movieId}");
    Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

    var getResponse = await Client.GetAsync($"/api/v3.2/favorites/{movieId}");
    Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
  }

  [Fact]
  public async Task Remove_WithUnknownFavorite_ReturnsNoContent()
  {
    var movieId = await CreateMovieAsync();

    var response = await Client.DeleteAsync($"/api/v3.2/favorites/{movieId}");

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
  }

  private async Task<Guid> CreateMovieAsync()
  {
    var genreResponse = await Client.PostAsJsonAsync("/api/v1/genres", TestData.ValidGenre());
    var genre = (await genreResponse.Content.ReadFromJsonAsync<GenreDto>())!;

    var personResponse = await Client.PostAsJsonAsync("/api/v1/people", TestData.ValidPerson());
    var person = (await personResponse.Content.ReadFromJsonAsync<PersonDto>())!;

    var movieResponse = await Client.PostAsJsonAsync("/api/v1/movies", TestData.ValidMovie(genre.Id, person.Id));
    var movie = (await movieResponse.Content.ReadFromJsonAsync<MovieDto>())!;

    return movie.Id;
  }

  private async Task<(HttpClient Client, string DisplayName)> RegisterAndLoginAsync()
  {
    const string password = "Password123!";
    var email = $"test_{Guid.NewGuid():N}@test.com";
    var displayName = $"Display Name {Guid.NewGuid():N}";
    var client = Factory.CreateClient();

    var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register",
      new RegisterDto { Email = email, Password = password, DisplayName = displayName });
    registerResponse.EnsureSuccessStatusCode();

    var tokenResponse = await client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
    {
      ["grant_type"] = "password",
      ["client_id"] = OpenIddictClientSeeder.ClientId,
      ["username"] = email,
      ["password"] = password,
    }));
    tokenResponse.EnsureSuccessStatusCode();
    var token = (await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>())!;

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
    return (client, displayName);
  }

  private sealed record TokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken);
}
