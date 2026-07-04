using System.Net;
using System.Net.Http.Json;
using MovieAPI.Application.Models;
using MovieAPI.IntegrationTests.Infrastructure;

namespace MovieAPI.IntegrationTests;

public class GenresControllerTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
  [Fact]
  public async Task CreateGenre_WithValidData_Returns201WithLocation()
  {
    var response = await Client.PostAsJsonAsync("/api/v1/genres", TestData.ValidGenre());

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.NotNull(response.Headers.Location);

    var created = await response.Content.ReadFromJsonAsync<GenreDto>();
    Assert.NotNull(created);
    Assert.Equal("Sci-Fi", created!.Name);
    Assert.NotEqual(Guid.Empty, created.Id);
  }

  [Fact]
  public async Task CreateGenre_WithEmptyName_Returns400()
  {
    var response = await Client.PostAsJsonAsync("/api/v1/genres", TestData.ValidGenre(name: ""));

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task GetGenres_ReturnsAllCreatedGenres()
  {
    await Client.PostAsJsonAsync("/api/v1/genres", TestData.ValidGenre("Drama", "drama"));
    await Client.PostAsJsonAsync("/api/v1/genres", TestData.ValidGenre("Comedy", "comedy"));

    var genres = await Client.GetFromJsonAsync<List<GenreDto>>("/api/v1/genres");

    Assert.NotNull(genres);
    Assert.Equal(2, genres!.Count);
    Assert.Contains(genres, g => g.Name == "Drama");
    Assert.Contains(genres, g => g.Name == "Comedy");
  }

  [Fact]
  public async Task GetGenre_WithExistingId_Returns200()
  {
    var created = await CreateGenreAsync();

    var response = await Client.GetAsync($"/api/v1/genres/{created.Id}");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var genre = await response.Content.ReadFromJsonAsync<GenreExtendedDto>();
    Assert.Equal(created.Id, genre!.Id);
  }

  [Fact]
  public async Task GetGenre_WithUnknownId_Returns404()
  {
    var response = await Client.GetAsync($"/api/v1/genres/{Guid.NewGuid()}");

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task UpdateGenre_WithExistingId_Returns204AndPersistsChange()
  {
    var created = await CreateGenreAsync();

    var response = await Client.PutAsJsonAsync($"/api/v1/genres/{created.Id}", TestData.ValidGenre("Updated", "updated"));

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var fetched = await Client.GetFromJsonAsync<GenreExtendedDto>($"/api/v1/genres/{created.Id}");
    Assert.Equal("Updated", fetched!.Name);
  }

  [Fact]
  public async Task UpdateGenre_WithUnknownId_Returns404()
  {
    var response = await Client.PutAsJsonAsync($"/api/v1/genres/{Guid.NewGuid()}", TestData.ValidGenre());

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task PatchGenre_WithExistingId_Returns204AndPersistsChange()
  {
    var created = await CreateGenreAsync();
    var patch = new[] { new { op = "replace", path = "/name", value = "Patched" } };

    var response = await PatchJsonPatchAsync($"/api/v1/genres/{created.Id}", patch);

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var fetched = await Client.GetFromJsonAsync<GenreExtendedDto>($"/api/v1/genres/{created.Id}");
    Assert.Equal("Patched", fetched!.Name);
  }

  [Fact]
  public async Task DeleteGenre_WithExistingId_Returns204AndSubsequentGetReturns404()
  {
    var created = await CreateGenreAsync();

    var deleteResponse = await Client.DeleteAsync($"/api/v1/genres/{created.Id}");
    Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

    var getResponse = await Client.GetAsync($"/api/v1/genres/{created.Id}");
    Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
  }

  [Fact]
  public async Task DeleteGenre_WithUnknownId_ReturnsNoContent()
  {
    var response = await Client.DeleteAsync($"/api/v1/genres/{Guid.NewGuid()}");

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
  }

  private async Task<GenreDto> CreateGenreAsync()
  {
    var response = await Client.PostAsJsonAsync("/api/v1/genres", TestData.ValidGenre());
    return (await response.Content.ReadFromJsonAsync<GenreDto>())!;
  }
}
