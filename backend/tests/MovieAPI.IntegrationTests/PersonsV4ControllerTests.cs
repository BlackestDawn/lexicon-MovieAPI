using System.Net;
using System.Net.Http.Json;
using MovieAPI.Application.Models;
using MovieAPI.IntegrationTests.Infrastructure;

namespace MovieAPI.IntegrationTests;

// V4's only difference from V3 is the version segment itself - the route ("persons"),
// DTOs, and behavior are otherwise identical, so this focuses on the route being wired
// up rather than re-covering every V3 case.
public class PersonsV4ControllerTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
  [Fact]
  public async Task CreatePerson_Returns201AndLocationUnderV4Persons()
  {
    var response = await Client.PostAsJsonAsync("/api/v4/persons", TestData.ValidPersonV2(middleName: "Augusta"));

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.NotNull(response.Headers.Location);
    Assert.Contains("/api/v4/persons/", response.Headers.Location!.ToString());

    var created = await response.Content.ReadFromJsonAsync<PersonDto>();
    Assert.Equal("Ada", created!.GivenName);
    Assert.Equal("Augusta", created.MiddleName);
  }

  [Fact]
  public async Task GetPersons_ReturnsCreatedPersonsWithPaginationHeader()
  {
    await Client.PostAsJsonAsync("/api/v4/persons", TestData.ValidPersonV2("Ada", lastName: "Lovelace"));
    await Client.PostAsJsonAsync("/api/v4/persons", TestData.ValidPersonV2("Grace", lastName: "Hopper"));

    var response = await Client.GetAsync("/api/v4/persons");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.True(response.Headers.Contains("X-Pagination"));

    var persons = await response.Content.ReadFromJsonAsync<List<PersonDto>>();
    Assert.Equal(2, persons!.Count);
  }

  [Fact]
  public async Task GetPersons_FiltersByBirthYear()
  {
    await Client.PostAsJsonAsync("/api/v4/persons", TestData.ValidPersonV2());

    var response = await Client.GetAsync("/api/v4/persons?year=1980");

    var matched = await response.Content.ReadFromJsonAsync<List<PersonDto>>();
    Assert.Single(matched!);
  }
}
