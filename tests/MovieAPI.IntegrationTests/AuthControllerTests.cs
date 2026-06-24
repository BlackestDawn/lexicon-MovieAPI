using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MovieAPI.Application.Models;
using MovieAPI.IntegrationTests.Infrastructure;

namespace MovieAPI.IntegrationTests;

public class AuthControllerTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
  private const string Password = "Password123!";

  [Fact]
  public async Task ChangePassword_WithoutToken_Returns401()
  {
    var anonymous = Factory.CreateClient();

    var response = await anonymous.PutAsJsonAsync("/api/auth/me/password",
      new ChangePasswordDto { CurrentPassword = Password, NewPassword = "NewPassword123!" });

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact]
  public async Task ChangePassword_WithWrongCurrentPassword_Returns400()
  {
    var anonymous = Factory.CreateClient();
    await RegisterAsync(anonymous);

    var response = await anonymous.PutAsJsonAsync("/api/auth/me/password",
      new ChangePasswordDto { CurrentPassword = "WrongPassword123!", NewPassword = "NewPassword123!" });

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task ChangePassword_WithValidData_Returns204AndOldPasswordNoLongerWorks()
  {
    var client = Factory.CreateClient();
    var email = await RegisterAsync(client);

    var response = await client.PutAsJsonAsync("/api/auth/me/password",
      new ChangePasswordDto { CurrentPassword = Password, NewPassword = "NewPassword123!" });

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var loginWithOldPassword = await Factory.CreateClient().PostAsJsonAsync("/api/auth/login",
      new LoginDto { Email = email, Password = Password });
    Assert.Equal(HttpStatusCode.Unauthorized, loginWithOldPassword.StatusCode);

    var loginWithNewPassword = await Factory.CreateClient().PostAsJsonAsync("/api/auth/login",
      new LoginDto { Email = email, Password = "NewPassword123!" });
    Assert.Equal(HttpStatusCode.OK, loginWithNewPassword.StatusCode);
  }

  private static async Task<string> RegisterAsync(HttpClient client)
  {
    var email = $"test_{Guid.NewGuid():N}@test.com";
    var response = await client.PostAsJsonAsync("/api/auth/register",
      new RegisterDto { Email = email, Password = Password });

    var auth = (await response.Content.ReadFromJsonAsync<AuthResponseDto>())!;
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

    return email;
  }
}
