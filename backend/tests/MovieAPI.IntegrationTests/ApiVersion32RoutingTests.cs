using System.Net;
using MovieAPI.IntegrationTests.Infrastructure;

namespace MovieAPI.IntegrationTests;

// 3.2 is a non-breaking marker version, like 3.1 before it. It's stacked onto the same
// controllers that already answer 3.1 for the two resources that changed on this pass -
// Reviews (cross-movie GET /reviews/mine, plus movieId/movieTitle on ReviewDto) and Auth
// (CurrentUserDto gained createdAt). Favorites is a brand-new resource introduced alongside
// 3.2, so unlike Reviews/Auth it declares ONLY [ApiVersion("3.2")] - it never existed at any
// earlier version, so there's no 1.0-3.1 history to stack onto.
public class ApiVersion32RoutingTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
  [Fact]
  public async Task GetReviewsMine_V3_2_Returns200()
  {
    var response = await Client.GetAsync("/api/v3.2/reviews/mine");
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task GetMe_V3_2_Returns200()
  {
    var response = await Client.GetAsync("/api/v3.2/auth/me");
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task GetFavorites_V3_2_Returns200()
  {
    var response = await Client.GetAsync("/api/v3.2/favorites");
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task GetFavorites_V3_1_IsNotSupported()
  {
    // Unlike Reviews/Auth above, Favorites has no 3.1 (or earlier) implementation to fall
    // back to - it's new at 3.2.
    var response = await Client.GetAsync("/api/v3.1/favorites");
    Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task GetGenres_V3_2_IsNotSupported()
  {
    // Genres never changed - it was already left off 3.1, and stays capped at 3.0.
    var response = await Client.GetAsync("/api/v3.2/genres");
    Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task GetAdminUsers_V3_2_IsNotSupported()
  {
    // Admin user management wasn't touched this pass, so it stays capped at 3.1.
    var response = await Client.GetAsync("/api/v3.2/admin/users");
    Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task GetMovies_V3_2_IsNotSupported()
  {
    // Movies wasn't touched this pass, so it stays capped at 3.1.
    var response = await Client.GetAsync("/api/v3.2/movies");
    Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
  }
}
