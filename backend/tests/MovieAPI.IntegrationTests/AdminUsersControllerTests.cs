using System.Net;
using System.Net.Http.Json;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Constants;
using MovieAPI.IntegrationTests.Infrastructure;

namespace MovieAPI.IntegrationTests;

public class AdminUsersControllerTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
  [Fact]
  public async Task GetUsers_AsAdministrator_Returns200()
  {
    var response = await Client.GetAsync("/api/v1/admin/users");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.True(response.Headers.Contains("X-Pagination"));
  }

  [Fact]
  public async Task GetUsers_AsNonAdministrator_Returns403()
  {
    var moderator = await CreateClientWithRoleAsync(Roles.Moderator);

    var response = await moderator.GetAsync("/api/v1/admin/users");

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  [Fact]
  public async Task GetUsers_WithoutToken_Returns401()
  {
    var anonymous = Factory.CreateClient();

    var response = await anonymous.GetAsync("/api/v1/admin/users");

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact]
  public async Task CreateUser_WithValidData_Returns201WithRequestedRole()
  {
    var dto = new AdminUserForCreationDto
    {
      Email = $"created_{Guid.NewGuid():N}@test.com",
      Password = "Password123!",
      Role = Roles.PowerUser,
    };

    var response = await Client.PostAsJsonAsync("/api/v1/admin/users", dto);

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    var created = await response.Content.ReadFromJsonAsync<AdminUserDto>();
    Assert.NotNull(created);
    Assert.Equal(dto.Email, created!.Email);
    Assert.Equal(Roles.PowerUser, created.Role);
  }

  [Fact]
  public async Task CreateUser_WithInvalidRole_Returns400()
  {
    var dto = new AdminUserForCreationDto
    {
      Email = $"created_{Guid.NewGuid():N}@test.com",
      Password = "Password123!",
      Role = "NotARealRole",
    };

    var response = await Client.PostAsJsonAsync("/api/v1/admin/users", dto);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task GetUser_WithExistingId_Returns200()
  {
    var created = await CreateUserAsync(Roles.User);

    var response = await Client.GetAsync($"/api/v1/admin/users/{created.Id}");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var fetched = await response.Content.ReadFromJsonAsync<AdminUserDto>();
    Assert.Equal(created.Id, fetched!.Id);
  }

  [Fact]
  public async Task GetUser_WithUnknownId_Returns404()
  {
    var response = await Client.GetAsync($"/api/v1/admin/users/{Guid.NewGuid()}");

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task UpdateUser_ChangingRole_Returns200AndPersistsNewRole()
  {
    var created = await CreateUserAsync(Roles.User);

    var response = await Client.PutAsJsonAsync($"/api/v1/admin/users/{created.Id}",
      new AdminUserForUpdateDto { Email = created.Email, Role = Roles.Moderator });

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var fetched = await Client.GetFromJsonAsync<AdminUserDto>($"/api/v1/admin/users/{created.Id}");
    Assert.Equal(Roles.Moderator, fetched!.Role);
  }

  [Fact]
  public async Task UpdateUser_WithUnknownId_Returns404()
  {
    var response = await Client.PutAsJsonAsync($"/api/v1/admin/users/{Guid.NewGuid()}",
      new AdminUserForUpdateDto { Email = "ghost@test.com", Role = Roles.User });

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task UpdateUser_SelfDemotingAwayFromAdministrator_Returns403()
  {
    var (adminId, adminClient) = await CreateUserAndClientAsync(Roles.Administrator);

    var response = await adminClient.PutAsJsonAsync($"/api/v1/admin/users/{adminId}",
      new AdminUserForUpdateDto { Email = $"self_{Guid.NewGuid():N}@test.com", Role = Roles.PowerUser });

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  [Fact]
  public async Task UpdateUser_SelfUpdateKeepingAdministratorRole_Returns200()
  {
    var (adminId, adminClient) = await CreateUserAndClientAsync(Roles.Administrator);

    var response = await adminClient.PutAsJsonAsync($"/api/v1/admin/users/{adminId}",
      new AdminUserForUpdateDto { Email = $"self_{Guid.NewGuid():N}@test.com", Role = Roles.Administrator });

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task DeleteUser_WithExistingId_Returns204AndSubsequentGetReturns404()
  {
    var created = await CreateUserAsync(Roles.User);

    var deleteResponse = await Client.DeleteAsync($"/api/v1/admin/users/{created.Id}");
    Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

    var getResponse = await Client.GetAsync($"/api/v1/admin/users/{created.Id}");
    Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
  }

  [Fact]
  public async Task DeleteUser_SelfDelete_Returns403()
  {
    var (adminId, adminClient) = await CreateUserAndClientAsync(Roles.Administrator);

    var response = await adminClient.DeleteAsync($"/api/v1/admin/users/{adminId}");

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  private async Task<AdminUserDto> CreateUserAsync(string role)
  {
    var dto = new AdminUserForCreationDto
    {
      Email = $"created_{Guid.NewGuid():N}@test.com",
      Password = "Password123!",
      Role = role,
    };

    var response = await Client.PostAsJsonAsync("/api/v1/admin/users", dto);
    return (await response.Content.ReadFromJsonAsync<AdminUserDto>())!;
  }
}
