using System.Net;
using System.Net.Http.Json;
using MovieAPI.Application.Models;
using MovieAPI.IntegrationTests.Infrastructure;

namespace MovieAPI.IntegrationTests;

public class ReviewsControllerTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
  [Fact]
  public async Task CreateReview_WithValidData_Returns201WithLocation()
  {
    var movieId = await CreateMovieAsync();

    var response = await Client.PostAsJsonAsync($"/api/movies/{movieId}/reviews", TestData.ValidReview());

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.NotNull(response.Headers.Location);

    var created = await response.Content.ReadFromJsonAsync<ReviewDto>();
    Assert.NotNull(created);
    Assert.Equal(8, created!.Score);
  }

  [Fact]
  public async Task CreateReview_WithUnknownMovieId_Returns404()
  {
    var response = await Client.PostAsJsonAsync($"/api/movies/{Guid.NewGuid()}/reviews", TestData.ValidReview());

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task CreateReview_WithInvalidScore_Returns400()
  {
    var movieId = await CreateMovieAsync();

    var response = await Client.PostAsJsonAsync($"/api/movies/{movieId}/reviews", TestData.ValidReview(score: 0));

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task GetReviews_ReturnsCreatedReviewsWithPaginationHeader()
  {
    var movieId = await CreateMovieAsync();
    await CreateReviewAsync(movieId, "Reviewer One");
    await CreateReviewAsync(movieId, "Reviewer Two");

    var response = await Client.GetAsync($"/api/movies/{movieId}/reviews");

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

    var response = await Client.GetAsync($"/api/movies/{movieId}/reviews/{created.Id}");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var review = await response.Content.ReadFromJsonAsync<ReviewDto>();
    Assert.Equal(created.Id, review!.Id);
  }

  [Fact]
  public async Task GetReview_WithUnknownId_Returns404()
  {
    var movieId = await CreateMovieAsync();

    var response = await Client.GetAsync($"/api/movies/{movieId}/reviews/{Guid.NewGuid()}");

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task UpdateReview_WithExistingId_Returns204AndPersistsChange()
  {
    var movieId = await CreateMovieAsync();
    var created = await CreateReviewAsync(movieId);

    var response = await Client.PutAsJsonAsync($"/api/movies/{movieId}/reviews/{created.Id}",
      TestData.ValidReview("Updated Reviewer", 5));

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var fetched = await Client.GetFromJsonAsync<ReviewDto>($"/api/movies/{movieId}/reviews/{created.Id}");
    Assert.Equal("Updated Reviewer", fetched!.AuthorName);
    Assert.Equal(5, fetched.Score);
  }

  [Fact]
  public async Task UpdateReview_WithUnknownId_Returns404()
  {
    var movieId = await CreateMovieAsync();

    var response = await Client.PutAsJsonAsync($"/api/movies/{movieId}/reviews/{Guid.NewGuid()}", TestData.ValidReview());

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task PatchReview_WithExistingId_Returns204AndPersistsChange()
  {
    var movieId = await CreateMovieAsync();
    var created = await CreateReviewAsync(movieId);
    var patch = new[] { new { op = "replace", path = "/score", value = 3 } };

    var response = await PatchJsonPatchAsync($"/api/movies/{movieId}/reviews/{created.Id}", patch);

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var fetched = await Client.GetFromJsonAsync<ReviewDto>($"/api/movies/{movieId}/reviews/{created.Id}");
    Assert.Equal(3, fetched!.Score);
  }

  [Fact]
  public async Task DeleteReview_WithExistingId_Returns204AndSubsequentGetReturns404()
  {
    var movieId = await CreateMovieAsync();
    var created = await CreateReviewAsync(movieId);

    var deleteResponse = await Client.DeleteAsync($"/api/movies/{movieId}/reviews/{created.Id}");
    Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

    var getResponse = await Client.GetAsync($"/api/movies/{movieId}/reviews/{created.Id}");
    Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
  }

  [Fact]
  public async Task DeleteReview_WithUnknownId_ReturnsNoContent()
  {
    var movieId = await CreateMovieAsync();

    var response = await Client.DeleteAsync($"/api/movies/{movieId}/reviews/{Guid.NewGuid()}");

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
  }

  private async Task<Guid> CreateMovieAsync()
  {
    var genreResponse = await Client.PostAsJsonAsync("/api/genres", TestData.ValidGenre());
    var genre = (await genreResponse.Content.ReadFromJsonAsync<GenreDto>())!;

    var personResponse = await Client.PostAsJsonAsync("/api/people", TestData.ValidPerson());
    var person = (await personResponse.Content.ReadFromJsonAsync<PersonDto>())!;

    var movieResponse = await Client.PostAsJsonAsync("/api/movies", TestData.ValidMovie(genre.Id, person.Id));
    var movie = (await movieResponse.Content.ReadFromJsonAsync<MovieDto>())!;

    return movie.Id;
  }

  private async Task<ReviewDto> CreateReviewAsync(Guid movieId, string author = "Reviewer")
  {
    var response = await Client.PostAsJsonAsync($"/api/movies/{movieId}/reviews", TestData.ValidReview(author));
    return (await response.Content.ReadFromJsonAsync<ReviewDto>())!;
  }
}
