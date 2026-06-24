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
    var auth = await RegisterAsync(client);

    var response = await client.PutAsJsonAsync("/api/auth/me/password",
      new ChangePasswordDto { CurrentPassword = Password, NewPassword = "NewPassword123!" });

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var loginWithOldPassword = await Factory.CreateClient().PostAsJsonAsync("/api/auth/login",
      new LoginDto { Email = auth.User.Email, Password = Password });
    Assert.Equal(HttpStatusCode.Unauthorized, loginWithOldPassword.StatusCode);

    var loginWithNewPassword = await Factory.CreateClient().PostAsJsonAsync("/api/auth/login",
      new LoginDto { Email = auth.User.Email, Password = "NewPassword123!" });
    Assert.Equal(HttpStatusCode.OK, loginWithNewPassword.StatusCode);
  }

  [Fact]
  public async Task Refresh_WithValidToken_Returns200WithNewTokens()
  {
    var client = Factory.CreateClient();
    var auth = await RegisterAsync(client);

    var response = await Factory.CreateClient().PostAsJsonAsync("/api/auth/refresh",
      new RefreshTokenDto { RefreshToken = auth.RefreshToken });

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var refreshed = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
    Assert.NotNull(refreshed);
    Assert.NotEqual(auth.AccessToken, refreshed!.AccessToken);
    Assert.NotEqual(auth.RefreshToken, refreshed.RefreshToken);
  }

  [Fact]
  public async Task Refresh_WithUnknownToken_Returns401()
  {
    var response = await Factory.CreateClient().PostAsJsonAsync("/api/auth/refresh",
      new RefreshTokenDto { RefreshToken = "not-a-real-token" });

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact]
  public async Task Refresh_WithAlreadyRotatedToken_Returns401AndRevokesTheNewOneToo()
  {
    var client = Factory.CreateClient();
    var auth = await RegisterAsync(client);

    var firstRefresh = await Factory.CreateClient().PostAsJsonAsync("/api/auth/refresh",
      new RefreshTokenDto { RefreshToken = auth.RefreshToken });
    var rotated = (await firstRefresh.Content.ReadFromJsonAsync<AuthResponseDto>())!;

    // Reusing the now-rotated-away original token simulates a stolen token being
    // replayed - the API should treat this as theft and kill the whole chain,
    // including the token that was legitimately issued by the call just above.
    var reuseAttempt = await Factory.CreateClient().PostAsJsonAsync("/api/auth/refresh",
      new RefreshTokenDto { RefreshToken = auth.RefreshToken });
    Assert.Equal(HttpStatusCode.Unauthorized, reuseAttempt.StatusCode);

    var rotatedNowRevokedToo = await Factory.CreateClient().PostAsJsonAsync("/api/auth/refresh",
      new RefreshTokenDto { RefreshToken = rotated.RefreshToken });
    Assert.Equal(HttpStatusCode.Unauthorized, rotatedNowRevokedToo.StatusCode);
  }

  [Fact]
  public async Task Logout_RevokesOnlyTheSuppliedToken()
  {
    var client = Factory.CreateClient();
    var auth = await RegisterAsync(client);

    var logoutResponse = await client.PostAsJsonAsync("/api/auth/logout",
      new RefreshTokenDto { RefreshToken = auth.RefreshToken });
    Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

    var refreshAfterLogout = await Factory.CreateClient().PostAsJsonAsync("/api/auth/refresh",
      new RefreshTokenDto { RefreshToken = auth.RefreshToken });
    Assert.Equal(HttpStatusCode.Unauthorized, refreshAfterLogout.StatusCode);
  }

  [Fact]
  public async Task Logout_WithoutToken_Returns401()
  {
    var anonymous = Factory.CreateClient();

    var response = await anonymous.PostAsJsonAsync("/api/auth/logout", new RefreshTokenDto { RefreshToken = "irrelevant" });

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact]
  public async Task Logout_WithSomeoneElsesToken_Returns204ButDoesNotRevokeIt()
  {
    var ownerClient = Factory.CreateClient();
    var owner = await RegisterAsync(ownerClient);

    var attackerClient = Factory.CreateClient();
    await RegisterAsync(attackerClient);

    // Logout is a no-op for tokens that aren't yours rather than an error - it
    // shouldn't leak whether the token exists, and the attacker's own logout call
    // succeeding has no bearing on the victim's session.
    var logoutResponse = await attackerClient.PostAsJsonAsync("/api/auth/logout",
      new RefreshTokenDto { RefreshToken = owner.RefreshToken });
    Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

    var ownerCanStillRefresh = await Factory.CreateClient().PostAsJsonAsync("/api/auth/refresh",
      new RefreshTokenDto { RefreshToken = owner.RefreshToken });
    Assert.Equal(HttpStatusCode.OK, ownerCanStillRefresh.StatusCode);
  }

  private static async Task<AuthResponseDto> RegisterAsync(HttpClient client)
  {
    var email = $"test_{Guid.NewGuid():N}@test.com";
    var response = await client.PostAsJsonAsync("/api/auth/register",
      new RegisterDto { Email = email, Password = Password });

    var auth = (await response.Content.ReadFromJsonAsync<AuthResponseDto>())!;
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

    return auth;
  }
}
