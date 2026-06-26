using System.Net;
using System.Net.Http.Json;
using MovieAPI.Application.Models;
using MovieAPI.IntegrationTests.Infrastructure;

namespace MovieAPI.IntegrationTests;

public class PersonsV2ControllerTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
  [Fact]
  public async Task CreatePerson_WithGivenNameAndMiddleName_Returns201AndPersistsBoth()
  {
    var response = await Client.PostAsJsonAsync("/api/v2/people", TestData.ValidPersonV2(middleName: "Augusta"));

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.NotNull(response.Headers.Location);

    var created = await response.Content.ReadFromJsonAsync<PersonDto>();
    Assert.NotNull(created);
    Assert.Equal("Ada", created!.GivenName);
    Assert.Equal("Augusta", created.MiddleName);
  }

  [Fact]
  public async Task CreatePerson_WithoutMiddleName_PersistsNullMiddleName()
  {
    var response = await Client.PostAsJsonAsync("/api/v2/people", TestData.ValidPersonV2());

    var created = await response.Content.ReadFromJsonAsync<PersonDto>();
    Assert.Null(created!.MiddleName);
  }

  [Fact]
  public async Task CreatePerson_WithEmptyGivenName_Returns400()
  {
    var response = await Client.PostAsJsonAsync("/api/v2/people", TestData.ValidPersonV2(givenName: ""));

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task GetPerson_WithExistingId_ReturnsGivenNameAndMiddleName()
  {
    var created = await CreatePersonAsync(middleName: "Augusta");

    var response = await Client.GetAsync($"/api/v2/people/{created.Id}");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var person = await response.Content.ReadFromJsonAsync<PersonExtendedDto>();
    Assert.Equal("Ada", person!.GivenName);
    Assert.Equal("Augusta", person.MiddleName);
  }

  [Fact]
  public async Task UpdatePerson_ChangesMiddleName()
  {
    var created = await CreatePersonAsync(middleName: "Augusta");

    var response = await Client.PutAsJsonAsync($"/api/v2/people/{created.Id}", TestData.ValidPersonV2(middleName: "Byron"));

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var fetched = await Client.GetFromJsonAsync<PersonExtendedDto>($"/api/v2/people/{created.Id}");
    Assert.Equal("Byron", fetched!.MiddleName);
  }

  [Fact]
  public async Task UpdatePerson_OmittingMiddleName_ClearsIt()
  {
    var created = await CreatePersonAsync(middleName: "Augusta");

    var response = await Client.PutAsJsonAsync($"/api/v2/people/{created.Id}", TestData.ValidPersonV2());

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var fetched = await Client.GetFromJsonAsync<PersonExtendedDto>($"/api/v2/people/{created.Id}");
    Assert.Null(fetched!.MiddleName);
  }

  [Fact]
  public async Task PatchPerson_ReplaceMiddleName_Returns204AndPersistsChange()
  {
    var created = await CreatePersonAsync();
    var patch = new[] { new { op = "replace", path = "/middleName", value = "Augusta" } };

    var response = await PatchJsonPatchAsync($"/api/v2/people/{created.Id}", patch);

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var fetched = await Client.GetFromJsonAsync<PersonExtendedDto>($"/api/v2/people/{created.Id}");
    Assert.Equal("Augusta", fetched!.MiddleName);
  }

  // Regression test for the V1 patch endpoint's MiddleName-preserving merge logic:
  // a V1 patch only knows about FirstName/LastName/DateOfBirth, so it must not wipe
  // out a MiddleName that was set through V2.
  [Fact]
  public async Task PatchViaV1_DoesNotClearMiddleNameSetViaV2()
  {
    var created = await CreatePersonAsync(middleName: "Augusta");
    var patch = new[] { new { op = "replace", path = "/lastName", value = "Byron" } };

    var response = await PatchJsonPatchAsync($"/api/v1/people/{created.Id}", patch);

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var fetched = await Client.GetFromJsonAsync<PersonExtendedDto>($"/api/v2/people/{created.Id}");
    Assert.Equal("Byron", fetched!.LastName);
    Assert.Equal("Augusta", fetched.MiddleName);
  }

  private async Task<PersonDto> CreatePersonAsync(string givenName = "Ada", string? middleName = null, string lastName = "Lovelace")
  {
    var response = await Client.PostAsJsonAsync("/api/v2/people", TestData.ValidPersonV2(givenName, middleName, lastName));
    return (await response.Content.ReadFromJsonAsync<PersonDto>())!;
  }
}
