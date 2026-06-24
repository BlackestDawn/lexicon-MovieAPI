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
using MovieAPI.Infrastructure.Interfaces;
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
        ["Jwt:Issuer"] = "MovieAPI.Tests",
        ["Jwt:Audience"] = "MovieAPI.Tests",
        ["Jwt:Key"] = "test-signing-key-not-for-production-use-0123456789abcdef",
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

    // Roles are seeded here (after migration) rather than in Program.cs, which skips
    // seeding entirely in the "Testing" environment for exactly this ordering reason.
    await RoleSeeder.SeedAsync(Services);

    using var respawnConnection = new SqlConnection(_connectionString);
    await respawnConnection.OpenAsync();
    _respawner = await Respawner.CreateAsync(respawnConnection, new RespawnerOptions
    {
      DbAdapter = DbAdapter.SqlServer,
      // AspNetRoles is reference data seeded once above, not per-test fixture data -
      // resetting it on every test would break role lookups for the rest of the run.
      TablesToIgnore = [new Table("AspNetRoles")],
    });
  }

  public async Task ResetDatabaseAsync()
  {
    using var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync();
    await _respawner.ResetAsync(connection);
  }

  // Creates a user directly via Identity (bypassing the HTTP /api/auth/register
  // endpoint) and mints a token for it through the same ITokenService the app uses,
  // so tests can get a token for a specific role without an extra round trip.
  public async Task<string> CreateUserTokenAsync(string role = Roles.User)
  {
    using var scope = Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

    var email = $"test_{Guid.NewGuid():N}@test.com";
    var user = new ApplicationUser { UserName = email, Email = email };

    var result = await userManager.CreateAsync(user, "Password123!");
    if (!result.Succeeded)
    {
      throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    await userManager.AddToRoleAsync(user, role);

    var roles = await userManager.GetRolesAsync(user);
    var (token, _) = tokenService.GenerateToken(user, roles);
    return token;
  }

  async Task IAsyncLifetime.DisposeAsync()
  {
    await _dbContainer.DisposeAsync();
    await base.DisposeAsync();
  }
}
