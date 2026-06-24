using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MovieAPI.Infrastructure;
using Respawn;
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

    using var respawnConnection = new SqlConnection(_connectionString);
    await respawnConnection.OpenAsync();
    _respawner = await Respawner.CreateAsync(respawnConnection, new RespawnerOptions
    {
      DbAdapter = DbAdapter.SqlServer,
    });
  }

  public async Task ResetDatabaseAsync()
  {
    using var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync();
    await _respawner.ResetAsync(connection);
  }

  async Task IAsyncLifetime.DisposeAsync()
  {
    await _dbContainer.DisposeAsync();
    await base.DisposeAsync();
  }
}
