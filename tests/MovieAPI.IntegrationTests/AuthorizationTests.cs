using System.Net;
using System.Net.Http.Json;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Constants;
using MovieAPI.IntegrationTests.Infrastructure;

namespace MovieAPI.IntegrationTests;

public class AuthorizationTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
  [Fact]
  public async Task GetGenres_WithNoToken_Returns200()
  {
    var anonymous = Factory.CreateClient();

    var response = await anonymous.GetAsync("/api/genres");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task CreateGenre_WithNoToken_Returns401()
  {
    var anonymous = Factory.CreateClient();

    var response = await anonymous.PostAsJsonAsync("/api/genres", TestData.ValidGenre());

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact]
  public async Task CreateGenre_AsUser_Returns403()
  {
    var user = await CreateClientWithRoleAsync(Roles.User);

    var response = await user.PostAsJsonAsync("/api/genres", TestData.ValidGenre());

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  [Fact]
  public async Task CreateGenre_AsModerator_Returns201()
  {
    var moderator = await CreateClientWithRoleAsync(Roles.Moderator);

    var response = await moderator.PostAsJsonAsync("/api/genres", TestData.ValidGenre());

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
  }

  [Fact]
  public async Task DeleteGenre_AsModerator_Returns403()
  {
    var created = await CreateGenreAsync();
    var moderator = await CreateClientWithRoleAsync(Roles.Moderator);

    var response = await moderator.DeleteAsync($"/api/genres/{created.Id}");

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  [Fact]
  public async Task DeleteGenre_AsAdministrator_Returns204()
  {
    var created = await CreateGenreAsync();
    var admin = await CreateClientWithRoleAsync(Roles.Administrator);

    var response = await admin.DeleteAsync($"/api/genres/{created.Id}");

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
  }

  [Fact]
  public async Task CreateMovie_AsUser_Returns403()
  {
    var (genreId, personId) = await CreateGenreAndPersonAsync();
    var user = await CreateClientWithRoleAsync(Roles.User);

    var response = await user.PostAsJsonAsync("/api/movies", TestData.ValidMovie(genreId, personId));

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  [Fact]
  public async Task CreateMovie_AsPowerUser_Returns201()
  {
    var (genreId, personId) = await CreateGenreAndPersonAsync();
    var powerUser = await CreateClientWithRoleAsync(Roles.PowerUser);

    var response = await powerUser.PostAsJsonAsync("/api/movies", TestData.ValidMovie(genreId, personId));

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
  }

  [Fact]
  public async Task DeleteMovie_AsPowerUser_Returns403()
  {
    var (genreId, personId) = await CreateGenreAndPersonAsync();
    var movie = await CreateMovieAsync(genreId, personId);
    var powerUser = await CreateClientWithRoleAsync(Roles.PowerUser);

    var response = await powerUser.DeleteAsync($"/api/movies/{movie.Id}");

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  [Fact]
  public async Task DeleteMovie_AsModerator_Returns204()
  {
    var (genreId, personId) = await CreateGenreAndPersonAsync();
    var movie = await CreateMovieAsync(genreId, personId);
    var moderator = await CreateClientWithRoleAsync(Roles.Moderator);

    var response = await moderator.DeleteAsync($"/api/movies/{movie.Id}");

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
  }

  [Fact]
  public async Task CreateReview_AsUser_Returns201()
  {
    var (genreId, personId) = await CreateGenreAndPersonAsync();
    var movie = await CreateMovieAsync(genreId, personId);
    var user = await CreateClientWithRoleAsync(Roles.User);

    var response = await user.PostAsJsonAsync($"/api/movies/{movie.Id}/reviews", TestData.ValidReview());

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
  }

  [Fact]
  public async Task UpdateReview_AsOwner_Returns204()
  {
    var (genreId, personId) = await CreateGenreAndPersonAsync();
    var movie = await CreateMovieAsync(genreId, personId);
    var owner = await CreateClientWithRoleAsync(Roles.User);
    var created = await CreateReviewAsync(owner, movie.Id);

    var response = await owner.PutAsJsonAsync($"/api/movies/{movie.Id}/reviews/{created.Id}", TestData.ValidReview("Owner Updated"));

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
  }

  [Fact]
  public async Task UpdateReview_AsDifferentUser_Returns403()
  {
    var (genreId, personId) = await CreateGenreAndPersonAsync();
    var movie = await CreateMovieAsync(genreId, personId);
    var owner = await CreateClientWithRoleAsync(Roles.User);
    var created = await CreateReviewAsync(owner, movie.Id);

    var otherUser = await CreateClientWithRoleAsync(Roles.User);
    var response = await otherUser.PutAsJsonAsync($"/api/movies/{movie.Id}/reviews/{created.Id}", TestData.ValidReview("Hijacked"));

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  [Fact]
  public async Task DeleteReview_AsDifferentUser_Returns403()
  {
    var (genreId, personId) = await CreateGenreAndPersonAsync();
    var movie = await CreateMovieAsync(genreId, personId);
    var owner = await CreateClientWithRoleAsync(Roles.User);
    var created = await CreateReviewAsync(owner, movie.Id);

    var otherUser = await CreateClientWithRoleAsync(Roles.User);
    var response = await otherUser.DeleteAsync($"/api/movies/{movie.Id}/reviews/{created.Id}");

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  [Fact]
  public async Task UpdateReview_AsModerator_Returns204EvenWhenNotOwner()
  {
    var (genreId, personId) = await CreateGenreAndPersonAsync();
    var movie = await CreateMovieAsync(genreId, personId);
    var owner = await CreateClientWithRoleAsync(Roles.User);
    var created = await CreateReviewAsync(owner, movie.Id);

    var moderator = await CreateClientWithRoleAsync(Roles.Moderator);
    var response = await moderator.PutAsJsonAsync($"/api/movies/{movie.Id}/reviews/{created.Id}", TestData.ValidReview("Moderated"));

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
  }

  [Fact]
  public async Task DeleteReview_AsModerator_Returns204EvenWhenNotOwner()
  {
    var (genreId, personId) = await CreateGenreAndPersonAsync();
    var movie = await CreateMovieAsync(genreId, personId);
    var owner = await CreateClientWithRoleAsync(Roles.User);
    var created = await CreateReviewAsync(owner, movie.Id);

    var moderator = await CreateClientWithRoleAsync(Roles.Moderator);
    var response = await moderator.DeleteAsync($"/api/movies/{movie.Id}/reviews/{created.Id}");

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
  }

  private async Task<GenreDto> CreateGenreAsync()
  {
    var response = await Client.PostAsJsonAsync("/api/genres", TestData.ValidGenre());
    return (await response.Content.ReadFromJsonAsync<GenreDto>())!;
  }

  private async Task<(Guid GenreId, Guid PersonId)> CreateGenreAndPersonAsync()
  {
    var genreResponse = await Client.PostAsJsonAsync("/api/genres", TestData.ValidGenre());
    var genre = (await genreResponse.Content.ReadFromJsonAsync<GenreDto>())!;

    var personResponse = await Client.PostAsJsonAsync("/api/people", TestData.ValidPerson());
    var person = (await personResponse.Content.ReadFromJsonAsync<PersonDto>())!;

    return (genre.Id, person.Id);
  }

  private async Task<MovieDto> CreateMovieAsync(Guid genreId, Guid personId)
  {
    var response = await Client.PostAsJsonAsync("/api/movies", TestData.ValidMovie(genreId, personId));
    return (await response.Content.ReadFromJsonAsync<MovieDto>())!;
  }

  private static async Task<ReviewDto> CreateReviewAsync(HttpClient client, Guid movieId)
  {
    var response = await client.PostAsJsonAsync($"/api/movies/{movieId}/reviews", TestData.ValidReview());
    return (await response.Content.ReadFromJsonAsync<ReviewDto>())!;
  }
}
