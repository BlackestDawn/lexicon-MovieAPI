using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MovieAPI.Domain.Constants;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure;
using Respawn;
using Respawn.Graph;
using Testcontainers.MsSql;

namespace MovieAPI.IntegrationTests.Infrastructure;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
  private readonly MsSqlContainer _dbContainer =
    new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04").Build();
  private Respawner _respawner = null!;
  private string _connectionString = null!;

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.UseEnvironment("Testing");

    // The app reads its connection string from configuration, so pointing that at the
    // Testcontainers instance lets the app's own DbContext registration wire itself up
    // instead of fighting EF Core's chained AddDbContext configuration delegates.
    builder.ConfigureAppConfiguration((_, configBuilder) =>
    {
      configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
      {
        ["ConnectionStrings:sqlserver"] = _connectionString,
      });
    });
  }

  public async Task InitializeAsync()
  {
    await _dbContainer.StartAsync();
    _connectionString = _dbContainer.GetConnectionString();

    using var scope = Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    // Roles/OpenIddict client are seeded here (after migration) rather than in
    // Program.cs, which skips seeding entirely in the "Testing" environment for
    // exactly this ordering reason.
    await RoleSeeder.SeedAsync(Services);
    await OpenIddictClientSeeder.SeedAsync(Services);

    using var respawnConnection = new SqlConnection(_connectionString);
    await respawnConnection.OpenAsync();
    _respawner = await Respawner.CreateAsync(respawnConnection, new RespawnerOptions
    {
      DbAdapter = DbAdapter.SqlServer,
      // AspNetRoles and OpenIddictApplications are reference data seeded once above,
      // not per-test fixture data - resetting them on every test would break role
      // lookups and the seeded OpenIddict client for the rest of the run.
      TablesToIgnore = [new Table("AspNetRoles"), new Table("OpenIddictApplications")],
    });
  }

  public async Task ResetDatabaseAsync()
  {
    using var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync();
    await _respawner.ResetAsync(connection);
  }

  // Creates a user directly via Identity (bypassing the HTTP /api/v1/auth/register
  // endpoint) and mints a token for it via a real POST /connect/token round trip, so
  // tests can get a token for a specific role without going through /register too.
  public async Task<string> CreateUserTokenAsync(string role = Roles.User)
  {
    var (_, token) = await CreateUserAsync(role);
    return token;
  }

  // Same as CreateUserTokenAsync, but also returns the new user's id - needed by
  // tests that act on "your own account" (e.g. admin self-delete/self-demote guards).
  public async Task<(Guid Id, string Token)> CreateUserAsync(string role = Roles.User)
  {
    using var scope = Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    var email = $"test_{Guid.NewGuid():N}@test.com";
    const string password = "Password123!";
    var user = new ApplicationUser { UserName = email, Email = email };

    var result = await userManager.CreateAsync(user, password);
    if (!result.Succeeded)
    {
      throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    await userManager.AddToRoleAsync(user, role);

    var token = await RequestAccessTokenAsync(email, password);
    return (user.Id, token);
  }

  private async Task<string> RequestAccessTokenAsync(string email, string password)
  {
    using var client = CreateClient();
    var response = await client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
    {
      ["grant_type"] = "password",
      ["client_id"] = OpenIddictClientSeeder.ClientId,
      ["username"] = email,
      ["password"] = password,
    }));

    if (!response.IsSuccessStatusCode)
    {
      var error = await response.Content.ReadAsStringAsync();
      throw new InvalidOperationException($"Token request for '{email}' failed ({response.StatusCode}): {error}");
    }

    var payload = await response.Content.ReadFromJsonAsync<TokenResponse>()
      ?? throw new InvalidOperationException("Token response was empty.");
    return payload.AccessToken;
  }

  private sealed record TokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken);

  // Email delivery is just a log line for now (see LoggingEmailSender), so there's no
  // HTTP-observable way to get the real reset token /api/v1/auth/forgot-password issued.
  // This generates one directly via Identity, the same way AuthService does internally.
  public async Task<string> GeneratePasswordResetTokenAsync(string email)
  {
    using var scope = Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    var user = await userManager.FindByEmailAsync(email)
      ?? throw new InvalidOperationException($"No user with email '{email}' exists.");

    return await userManager.GeneratePasswordResetTokenAsync(user);
  }

  async Task IAsyncLifetime.DisposeAsync()
  {
    await _dbContainer.DisposeAsync();
    await base.DisposeAsync();
  }
}
