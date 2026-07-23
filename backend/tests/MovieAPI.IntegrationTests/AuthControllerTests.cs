using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Constants;
using MovieAPI.Infrastructure;
using MovieAPI.IntegrationTests.Infrastructure;

namespace MovieAPI.IntegrationTests;

public class AuthControllerTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
  private const string Password = "Password123!";

  [Fact]
  public async Task ChangePassword_WithoutToken_Returns401()
  {
    var anonymous = Factory.CreateClient();

    var response = await anonymous.PutAsJsonAsync("/api/v1/auth/me/password",
      new ChangePasswordDto { CurrentPassword = Password, NewPassword = "NewPassword123!" });

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact]
  public async Task ChangePassword_WithWrongCurrentPassword_Returns400()
  {
    var auth = await RegisterAsync(Factory.CreateClient());

    var response = await auth.Client.PutAsJsonAsync("/api/v1/auth/me/password",
      new ChangePasswordDto { CurrentPassword = "WrongPassword123!", NewPassword = "NewPassword123!" });

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task ChangePassword_WithValidData_Returns204AndOldPasswordNoLongerWorks()
  {
    var auth = await RegisterAsync(Factory.CreateClient());

    var response = await auth.Client.PutAsJsonAsync("/api/v1/auth/me/password",
      new ChangePasswordDto { CurrentPassword = Password, NewPassword = "NewPassword123!" });

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var loginWithOldPassword = await PostTokenAsync(Factory.CreateClient(), auth.Email, Password);
    Assert.Equal(HttpStatusCode.BadRequest, loginWithOldPassword.StatusCode);

    var loginWithNewPassword = await PostTokenAsync(Factory.CreateClient(), auth.Email, "NewPassword123!");
    Assert.Equal(HttpStatusCode.OK, loginWithNewPassword.StatusCode);
  }

  [Fact]
  public async Task ChangePassword_WhenSucceeds_RevokesExistingRefreshTokens()
  {
    var auth = await RegisterAsync(Factory.CreateClient());

    var response = await auth.Client.PutAsJsonAsync("/api/v1/auth/me/password",
      new ChangePasswordDto { CurrentPassword = Password, NewPassword = "NewPassword123!" });
    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var refreshWithOldToken = await PostRefreshAsync(Factory.CreateClient(), auth.RefreshToken);
    Assert.Equal(HttpStatusCode.BadRequest, refreshWithOldToken.StatusCode);
  }

  [Fact]
  public async Task ForgotPassword_WithUnknownEmail_Returns204()
  {
    var anonymous = Factory.CreateClient();

    var response = await anonymous.PostAsJsonAsync("/api/v1/auth/forgot-password",
      new ForgotPasswordDto { Email = $"nobody_{Guid.NewGuid():N}@test.com" });

    // Same response whether the email exists or not - the endpoint must not leak
    // account existence.
    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
  }

  [Fact]
  public async Task ForgotPassword_WithKnownEmail_Returns204()
  {
    var auth = await RegisterAsync(Factory.CreateClient());

    var response = await Factory.CreateClient().PostAsJsonAsync("/api/v1/auth/forgot-password",
      new ForgotPasswordDto { Email = auth.Email });

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
  }

  [Fact]
  public async Task ResetPassword_WithInvalidToken_Returns400()
  {
    var auth = await RegisterAsync(Factory.CreateClient());

    var response = await Factory.CreateClient().PostAsJsonAsync("/api/v1/auth/reset-password",
      new ResetPasswordDto { Email = auth.Email, Token = "not-a-real-token", NewPassword = "NewPassword123!" });

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task ResetPassword_WithValidToken_Returns204AndNewPasswordWorks()
  {
    var auth = await RegisterAsync(Factory.CreateClient());
    var resetToken = await Factory.GeneratePasswordResetTokenAsync(auth.Email);

    var response = await Factory.CreateClient().PostAsJsonAsync("/api/v1/auth/reset-password",
      new ResetPasswordDto { Email = auth.Email, Token = resetToken, NewPassword = "NewPassword123!" });
    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var loginWithOldPassword = await PostTokenAsync(Factory.CreateClient(), auth.Email, Password);
    Assert.Equal(HttpStatusCode.BadRequest, loginWithOldPassword.StatusCode);

    var loginWithNewPassword = await PostTokenAsync(Factory.CreateClient(), auth.Email, "NewPassword123!");
    Assert.Equal(HttpStatusCode.OK, loginWithNewPassword.StatusCode);
  }

  [Fact]
  public async Task ResetPassword_WhenSucceeds_RevokesExistingRefreshTokens()
  {
    var auth = await RegisterAsync(Factory.CreateClient());
    var resetToken = await Factory.GeneratePasswordResetTokenAsync(auth.Email);

    var response = await Factory.CreateClient().PostAsJsonAsync("/api/v1/auth/reset-password",
      new ResetPasswordDto { Email = auth.Email, Token = resetToken, NewPassword = "NewPassword123!" });
    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    var refreshWithOldToken = await PostRefreshAsync(Factory.CreateClient(), auth.RefreshToken);
    Assert.Equal(HttpStatusCode.BadRequest, refreshWithOldToken.StatusCode);
  }

  [Fact]
  public async Task Refresh_WithValidToken_Returns200WithNewTokens()
  {
    var auth = await RegisterAsync(Factory.CreateClient());

    var response = await PostRefreshAsync(Factory.CreateClient(), auth.RefreshToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var refreshed = await response.Content.ReadFromJsonAsync<TokenResponse>();
    Assert.NotNull(refreshed);
    Assert.NotEqual(auth.RefreshToken, refreshed!.RefreshToken);
  }

  [Fact]
  public async Task Refresh_WithUnknownToken_Returns400()
  {
    var response = await PostRefreshAsync(Factory.CreateClient(), "not-a-real-token");

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task Refresh_WithAlreadyRotatedToken_Returns400AndRevokesTheNewOneToo()
  {
    var auth = await RegisterAsync(Factory.CreateClient());

    var firstRefresh = await PostRefreshAsync(Factory.CreateClient(), auth.RefreshToken);
    var rotated = (await firstRefresh.Content.ReadFromJsonAsync<TokenResponse>())!;

    // Reusing the now-rotated-away original token simulates a stolen token being
    // replayed - OpenIddict's rolling refresh tokens treat this as theft and kill the
    // whole authorization chain, including the token that was legitimately issued by
    // the call just above.
    var reuseAttempt = await PostRefreshAsync(Factory.CreateClient(), auth.RefreshToken);
    Assert.Equal(HttpStatusCode.BadRequest, reuseAttempt.StatusCode);

    var rotatedNowRevokedToo = await PostRefreshAsync(Factory.CreateClient(), rotated.RefreshToken!);
    Assert.Equal(HttpStatusCode.BadRequest, rotatedNowRevokedToo.StatusCode);
  }

  [Fact]
  public async Task Revoke_RevokesTheSuppliedRefreshToken()
  {
    var auth = await RegisterAsync(Factory.CreateClient());

    var revokeResponse = await PostRevokeAsync(Factory.CreateClient(), auth.RefreshToken);
    Assert.Equal(HttpStatusCode.OK, revokeResponse.StatusCode);

    var refreshAfterRevoke = await PostRefreshAsync(Factory.CreateClient(), auth.RefreshToken);
    Assert.Equal(HttpStatusCode.BadRequest, refreshAfterRevoke.StatusCode);
  }

  // Security-stamp validation (every authenticated request re-checks the token's
  // embedded stamp against the user's current one)

  [Fact]
  public async Task AccessToken_WhenNothingChanged_StillAuthorizes()
  {
    var auth = await RegisterAsync(Factory.CreateClient());

    var response = await auth.Client.PutAsJsonAsync("/api/v1/auth/me", new UserForUpdateDto { Email = auth.Email });

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task AccessToken_AfterPasswordChange_NoLongerAuthorizes()
  {
    var auth = await RegisterAsync(Factory.CreateClient());

    var changeResponse = await auth.Client.PutAsJsonAsync("/api/v1/auth/me/password",
      new ChangePasswordDto { CurrentPassword = Password, NewPassword = "NewPassword123!" });
    Assert.Equal(HttpStatusCode.NoContent, changeResponse.StatusCode);

    // client still carries the access token issued before the password change.
    var staleTokenResponse = await auth.Client.PutAsJsonAsync("/api/v1/auth/me", new UserForUpdateDto { Email = auth.Email });

    Assert.Equal(HttpStatusCode.Unauthorized, staleTokenResponse.StatusCode);
  }

  [Fact]
  public async Task AccessToken_AfterPasswordReset_NoLongerAuthorizes()
  {
    var auth = await RegisterAsync(Factory.CreateClient());
    var resetToken = await Factory.GeneratePasswordResetTokenAsync(auth.Email);

    var resetResponse = await Factory.CreateClient().PostAsJsonAsync("/api/v1/auth/reset-password",
      new ResetPasswordDto { Email = auth.Email, Token = resetToken, NewPassword = "NewPassword123!" });
    Assert.Equal(HttpStatusCode.NoContent, resetResponse.StatusCode);

    var staleTokenResponse = await auth.Client.PutAsJsonAsync("/api/v1/auth/me", new UserForUpdateDto { Email = auth.Email });

    Assert.Equal(HttpStatusCode.Unauthorized, staleTokenResponse.StatusCode);
  }

  [Fact]
  public async Task AccessToken_ForDeletedUser_Returns401()
  {
    var (userId, client) = await CreateUserAndClientAsync(Roles.User);
    var adminClient = await CreateClientWithRoleAsync(Roles.Administrator);

    // Delete the user out from under their own still-valid, unexpired access token.
    var deleteResponse = await adminClient.DeleteAsync($"/api/v1/admin/users/{userId}");
    Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

    var response = await client.PutAsJsonAsync("/api/v1/auth/me", new UserForUpdateDto { Email = "irrelevant@test.com" });

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  // Helpers

  private static async Task<AuthResult> RegisterAsync(HttpClient client)
  {
    var email = $"test_{Guid.NewGuid():N}@test.com";
    var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register",
      new RegisterDto { Email = email, Password = Password });
    registerResponse.EnsureSuccessStatusCode();

    var tokenResponse = await PostTokenAsync(client, email, Password);
    tokenResponse.EnsureSuccessStatusCode();
    var token = (await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>())!;

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
    return new AuthResult(email, token.RefreshToken!, client);
  }

  private static Task<HttpResponseMessage> PostTokenAsync(HttpClient client, string email, string password) =>
    client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
    {
      ["grant_type"] = "password",
      ["client_id"] = OpenIddictClientSeeder.ClientId,
      ["username"] = email,
      ["password"] = password,
    }));

  private static Task<HttpResponseMessage> PostRefreshAsync(HttpClient client, string refreshToken) =>
    client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
    {
      ["grant_type"] = "refresh_token",
      ["client_id"] = OpenIddictClientSeeder.ClientId,
      ["refresh_token"] = refreshToken,
    }));

  private static Task<HttpResponseMessage> PostRevokeAsync(HttpClient client, string token) =>
    client.PostAsync("/connect/token/revoke", new FormUrlEncodedContent(new Dictionary<string, string>
    {
      ["token"] = token,
      ["client_id"] = OpenIddictClientSeeder.ClientId,
    }));

  private sealed record AuthResult(string Email, string RefreshToken, HttpClient Client);

  private sealed record TokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string? RefreshToken);
}
