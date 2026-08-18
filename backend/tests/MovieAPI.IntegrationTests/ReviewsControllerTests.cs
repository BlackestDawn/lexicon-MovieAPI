using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Constants;
using MovieAPI.Infrastructure;
using MovieAPI.IntegrationTests.Infrastructure;

namespace MovieAPI.IntegrationTests;

public class ReviewsControllerTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
  [Fact]
  public async Task CreateReview_WithValidData_Returns201WithLocation()
  {
    var movieId = await CreateMovieAsync();

    var response = await Client.PostAsJsonAsync($"/api/v1/movies/{movieId}/reviews", TestData.ValidReview());

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.NotNull(response.Headers.Location);

    var created = await response.Content.ReadFromJsonAsync<ReviewDto>();
    Assert.NotNull(created);
    Assert.Equal(8, created!.Score);
  }

  [Fact]
  public async Task CreateReview_ExposesCreatingUsersIdAsUserId()
  {
    var movieId = await CreateMovieAsync();
    var (userId, client) = await CreateUserAndClientAsync(Roles.User);

    var response = await client.PostAsJsonAsync($"/api/v1/movies/{movieId}/reviews", TestData.ValidReview());

    var created = await response.Content.ReadFromJsonAsync<ReviewDto>();
    Assert.Equal(userId, created!.UserId);
  }

  [Fact]
  public async Task CreateReview_WithUnknownMovieId_Returns404()
  {
    var response = await Client.PostAsJsonAsync($"/api/v1/movies/{Guid.NewGuid()}/reviews", TestData.ValidReview());

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task CreateReview_WithInvalidScore_Returns400()
  {
    var movieId = await CreateMovieAsync();

    var response = await Client.PostAsJsonAsync($"/api/v1/movies/{movieId}/reviews", TestData.ValidReview(score: 0));

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task GetReviews_ReturnsCreatedReviewsWithPaginationHeader()
  {
    var movieId = await CreateMovieAsync();
    await CreateReviewAsync(movieId);
    await CreateReviewAsync(movieId);

    var response = await Client.GetAsync($"/api/v1/movies/{movieId}/reviews");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.True(response.Headers.Contains("X-Pagination"));

    var reviews = await response.Content.ReadFromJsonAsync<List<ReviewDto>>();
    Assert.Equal(2, reviews!.Count);
  }

  [Fact]
  public async Task GetReview_WithExistingId_Returns200()
  {
    var movieId = await CreateMovieAsync();
    var created = await CreateReviewAsync(movieId);

    var response = await Client.GetAsync($"/api/v1/movies/{movieId}/reviews/{created.Id}");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var review = await response.Content.ReadFromJsonAsync<ReviewDto>();
    Assert.Equal(created.Id, review!.Id);
  }

  [Fact]
  public async Task GetReview_WithUnknownId_Returns404()
  {
    var movieId = await CreateMovieAsync();

    var response = await Client.GetAsync($"/api/v1/movies/{movieId}/reviews/{Guid.NewGuid()}");

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task UpdateReview_WithExistingId_Returns204AndPersistsChange()
  {
    var movieId = await CreateMovieAsync();
    var created = await CreateReviewAsync(movieId);

    var response = await Client.PutAsJsonAsync($"/api/v1/movies/{movieId}/reviews/{created.Id}",
      TestData.ValidReview(5));

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var fetched = await Client.GetFromJsonAsync<ReviewDto>($"/api/v1/movies/{movieId}/reviews/{created.Id}");
    Assert.Equal(5, fetched!.Score);
  }

  // Regression test: AuthorName is no longer client-supplied - it's derived from the
  // authenticated poster's account DisplayName, both on create and on every later edit.
  [Fact]
  public async Task CreateReview_SetsAuthorNameFromAccountDisplayName()
  {
    var movieId = await CreateMovieAsync();
    var (client, displayName) = await RegisterAndLoginAsync();

    var response = await client.PostAsJsonAsync($"/api/v1/movies/{movieId}/reviews", TestData.ValidReview());

    var created = await response.Content.ReadFromJsonAsync<ReviewDto>();
    Assert.Equal(displayName, created!.AuthorName);
  }

  [Fact]
  public async Task UpdateReview_KeepsAuthorNameInSyncWithOwnersDisplayName()
  {
    var movieId = await CreateMovieAsync();
    var (client, displayName) = await RegisterAndLoginAsync();
    var createResponse = await client.PostAsJsonAsync($"/api/v1/movies/{movieId}/reviews", TestData.ValidReview());
    var created = (await createResponse.Content.ReadFromJsonAsync<ReviewDto>())!;

    await client.PutAsJsonAsync($"/api/v1/movies/{movieId}/reviews/{created.Id}", TestData.ValidReview(3));

    var fetched = await client.GetFromJsonAsync<ReviewDto>($"/api/v1/movies/{movieId}/reviews/{created.Id}");
    Assert.Equal(displayName, fetched!.AuthorName);
  }

  [Fact]
  public async Task UpdateReview_WithUnknownId_Returns404()
  {
    var movieId = await CreateMovieAsync();

    var response = await Client.PutAsJsonAsync($"/api/v1/movies/{movieId}/reviews/{Guid.NewGuid()}", TestData.ValidReview());

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task PatchReview_WithExistingId_Returns204AndPersistsChange()
  {
    var movieId = await CreateMovieAsync();
    var created = await CreateReviewAsync(movieId);
    var patch = new[] { new { op = "replace", path = "/score", value = 3 } };

    var response = await PatchJsonPatchAsync($"/api/v1/movies/{movieId}/reviews/{created.Id}", patch);

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var fetched = await Client.GetFromJsonAsync<ReviewDto>($"/api/v1/movies/{movieId}/reviews/{created.Id}");
    Assert.Equal(3, fetched!.Score);
  }

  [Fact]
  public async Task DeleteReview_WithExistingId_Returns204AndSubsequentGetReturns404()
  {
    var movieId = await CreateMovieAsync();
    var created = await CreateReviewAsync(movieId);

    var deleteResponse = await Client.DeleteAsync($"/api/v1/movies/{movieId}/reviews/{created.Id}");
    Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

    var getResponse = await Client.GetAsync($"/api/v1/movies/{movieId}/reviews/{created.Id}");
    Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
  }

  [Fact]
  public async Task DeleteReview_WithUnknownId_ReturnsNoContent()
  {
    var movieId = await CreateMovieAsync();

    var response = await Client.DeleteAsync($"/api/v1/movies/{movieId}/reviews/{Guid.NewGuid()}");

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

  private async Task<ReviewDto> CreateReviewAsync(Guid movieId)
  {
    var response = await Client.PostAsJsonAsync($"/api/v1/movies/{movieId}/reviews", TestData.ValidReview());
    return (await response.Content.ReadFromJsonAsync<ReviewDto>())!;
  }

  // Registers a brand-new account with a distinct DisplayName, rather than reusing the
  // shared Administrator Client, so the review's derived AuthorName can be asserted
  // against a known value.
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
