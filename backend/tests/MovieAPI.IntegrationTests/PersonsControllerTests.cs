using System.Net;
using System.Net.Http.Json;
using MovieAPI.Application.Models.V1;
using MovieAPI.IntegrationTests.Infrastructure;

namespace MovieAPI.IntegrationTests;

public class PersonsControllerTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
  [Fact]
  public async Task CreatePerson_WithValidData_Returns201WithLocation()
  {
    var response = await Client.PostAsJsonAsync("/api/v1/people", TestData.ValidPerson());

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.NotNull(response.Headers.Location);

    var created = await response.Content.ReadFromJsonAsync<PersonV1Dto>();
    Assert.NotNull(created);
    Assert.Equal("Ada", created!.FirstName);
  }

  [Fact]
  public async Task CreatePerson_WithEmptyFirstName_Returns400()
  {
    var response = await Client.PostAsJsonAsync("/api/v1/people", TestData.ValidPerson(firstName: ""));

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task GetPersons_ReturnsCreatedPersonsWithPaginationHeader()
  {
    await CreatePersonAsync("Ada", "Lovelace");
    await CreatePersonAsync("Grace", "Hopper");

    var response = await Client.GetAsync("/api/v1/people");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.True(response.Headers.Contains("X-Pagination"));

    var persons = await response.Content.ReadFromJsonAsync<List<PersonV1Dto>>();
    Assert.Equal(2, persons!.Count);
  }

  [Fact]
  public async Task GetPerson_WithExistingId_Returns200()
  {
    var created = await CreatePersonAsync();

    var response = await Client.GetAsync($"/api/v1/people/{created.Id}");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var person = await response.Content.ReadFromJsonAsync<PersonExtendedV1Dto>();
    Assert.Equal(created.Id, person!.Id);
  }

  [Fact]
  public async Task GetPerson_WithUnknownId_Returns404()
  {
    var response = await Client.GetAsync($"/api/v1/people/{Guid.NewGuid()}");

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task UpdatePerson_WithExistingId_Returns204AndPersistsChange()
  {
    var created = await CreatePersonAsync();

    var response = await Client.PutAsJsonAsync($"/api/v1/people/{created.Id}", TestData.ValidPerson("Updated", "Name"));

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var fetched = await Client.GetFromJsonAsync<PersonExtendedV1Dto>($"/api/v1/people/{created.Id}");
    Assert.Equal("Updated", fetched!.FirstName);
  }

  [Fact]
  public async Task UpdatePerson_WithUnknownId_Returns404()
  {
    var response = await Client.PutAsJsonAsync($"/api/v1/people/{Guid.NewGuid()}", TestData.ValidPerson());

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task PatchPerson_WithExistingId_Returns204AndPersistsChange()
  {
    var created = await CreatePersonAsync();
    var patch = new[] { new { op = "replace", path = "/lastName", value = "Patched" } };

    var response = await PatchJsonPatchAsync($"/api/v1/people/{created.Id}", patch);

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var fetched = await Client.GetFromJsonAsync<PersonExtendedV1Dto>($"/api/v1/people/{created.Id}");
    Assert.Equal("Patched", fetched!.LastName);
  }

  [Fact]
  public async Task DeletePerson_WithExistingId_Returns204AndSubsequentGetReturns404()
  {
    var created = await CreatePersonAsync();

    var deleteResponse = await Client.DeleteAsync($"/api/v1/people/{created.Id}");
    Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

    var getResponse = await Client.GetAsync($"/api/v1/people/{created.Id}");
    Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
  }

  [Fact]
  public async Task DeletePerson_WithUnknownId_ReturnsNoContent()
  {
    var response = await Client.DeleteAsync($"/api/v1/people/{Guid.NewGuid()}");

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
  }

  private async Task<PersonV1Dto> CreatePersonAsync(string firstName = "Ada", string lastName = "Lovelace")
  {
    var response = await Client.PostAsJsonAsync("/api/v1/people", TestData.ValidPerson(firstName, lastName));
    return (await response.Content.ReadFromJsonAsync<PersonV1Dto>())!;
  }
}
