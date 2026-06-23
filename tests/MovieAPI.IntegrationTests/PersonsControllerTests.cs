using System.Net;
using System.Net.Http.Json;
using MovieAPI.Application.Models;
using MovieAPI.IntegrationTests.Infrastructure;

namespace MovieAPI.IntegrationTests;

public class PersonsControllerTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
  [Fact]
  public async Task CreatePerson_WithValidData_Returns201WithLocation()
  {
    var response = await Client.PostAsJsonAsync("/api/people", TestData.ValidPerson());

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.NotNull(response.Headers.Location);

    var created = await response.Content.ReadFromJsonAsync<PersonDto>();
    Assert.NotNull(created);
    Assert.Equal("Ada", created!.FirstName);
  }

  [Fact]
  public async Task CreatePerson_WithEmptyFirstName_Returns400()
  {
    var response = await Client.PostAsJsonAsync("/api/people", TestData.ValidPerson(firstName: ""));

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task GetPeople_ReturnsCreatedPeopleWithPaginationHeader()
  {
    await CreatePersonAsync("Ada", "Lovelace");
    await CreatePersonAsync("Grace", "Hopper");

    var response = await Client.GetAsync("/api/people");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.True(response.Headers.Contains("X-Pagination"));

    var people = await response.Content.ReadFromJsonAsync<List<PersonDto>>();
    Assert.Equal(2, people!.Count);
  }

  [Fact]
  public async Task GetPerson_WithExistingId_Returns200()
  {
    var created = await CreatePersonAsync();

    var response = await Client.GetAsync($"/api/people/{created.Id}");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var person = await response.Content.ReadFromJsonAsync<PersonExtendedDto>();
    Assert.Equal(created.Id, person!.Id);
  }

  [Fact]
  public async Task GetPerson_WithUnknownId_Returns404()
  {
    var response = await Client.GetAsync($"/api/people/{Guid.NewGuid()}");

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task UpdatePerson_WithExistingId_Returns204AndPersistsChange()
  {
    var created = await CreatePersonAsync();

    var response = await Client.PutAsJsonAsync($"/api/people/{created.Id}", TestData.ValidPerson("Updated", "Name"));

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var fetched = await Client.GetFromJsonAsync<PersonExtendedDto>($"/api/people/{created.Id}");
    Assert.Equal("Updated", fetched!.FirstName);
  }

  [Fact]
  public async Task UpdatePerson_WithUnknownId_Returns404()
  {
    var response = await Client.PutAsJsonAsync($"/api/people/{Guid.NewGuid()}", TestData.ValidPerson());

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task PatchPerson_WithExistingId_Returns204AndPersistsChange()
  {
    var created = await CreatePersonAsync();
    var patch = new[] { new { op = "replace", path = "/lastName", value = "Patched" } };

    var response = await PatchJsonPatchAsync($"/api/people/{created.Id}", patch);

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var fetched = await Client.GetFromJsonAsync<PersonExtendedDto>($"/api/people/{created.Id}");
    Assert.Equal("Patched", fetched!.LastName);
  }

  [Fact]
  public async Task DeletePerson_WithExistingId_Returns204AndSubsequentGetReturns404()
  {
    var created = await CreatePersonAsync();

    var deleteResponse = await Client.DeleteAsync($"/api/people/{created.Id}");
    Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

    var getResponse = await Client.GetAsync($"/api/people/{created.Id}");
    Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
  }

  [Fact]
  public async Task DeletePerson_WithUnknownId_ReturnsNoContent()
  {
    var response = await Client.DeleteAsync($"/api/people/{Guid.NewGuid()}");

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
  }

  private async Task<PersonDto> CreatePersonAsync(string firstName = "Ada", string lastName = "Lovelace")
  {
    var response = await Client.PostAsJsonAsync("/api/people", TestData.ValidPerson(firstName, lastName));
    return (await response.Content.ReadFromJsonAsync<PersonDto>())!;
  }
}
