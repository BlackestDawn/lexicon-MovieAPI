using System.Net;
using System.Net.Http.Json;
using MovieAPI.Application.Models.V1;
using MovieAPI.IntegrationTests.Infrastructure;

namespace MovieAPI.IntegrationTests;

// A request with no version anywhere (no URL segment, query string, or header) should
// behave exactly like /api/v1/... - see Program.cs's path-rewrite middleware.
public class UnversionedRoutingTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
  [Fact]
  public async Task GetPeople_Unversioned_ReturnsV1Shape()
  {
    await Client.PostAsJsonAsync("/api/people", TestData.ValidPerson());

    var response = await Client.GetAsync("/api/people");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var people = await response.Content.ReadFromJsonAsync<List<PersonV1Dto>>();
    Assert.Equal("Ada", Assert.Single(people!).FirstName);
  }

  [Fact]
  public async Task CreatePerson_Unversioned_LocationHeaderPointsAtV1()
  {
    var response = await Client.PostAsJsonAsync("/api/people", TestData.ValidPerson());

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.NotNull(response.Headers.Location);
    Assert.Contains("/api/v1/people/", response.Headers.Location!.ToString());
  }

  [Fact]
  public async Task GetGenres_Unversioned_Returns200()
  {
    await Client.PostAsJsonAsync("/api/genres", TestData.ValidGenre());

    var response = await Client.GetAsync("/api/genres");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task UpdateMe_Unversioned_RouteResolvesInsteadOf404()
  {
    var anonymous = Factory.CreateClient();
    var response = await anonymous.PutAsJsonAsync("/api/auth/me", new { email = "irrelevant@test.com" });

    // No token, but the point is the route resolves at all (not 404) -
    // unauthenticated/unversioned and versioned should fail identically.
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact]
  public async Task VersionedAndUnversioned_StillBothWork_SideBySide()
  {
    var unversioned = await Client.GetAsync("/api/genres");
    var versioned = await Client.GetAsync("/api/v1/genres");

    Assert.Equal(HttpStatusCode.OK, unversioned.StatusCode);
    Assert.Equal(HttpStatusCode.OK, versioned.StatusCode);
  }
}
