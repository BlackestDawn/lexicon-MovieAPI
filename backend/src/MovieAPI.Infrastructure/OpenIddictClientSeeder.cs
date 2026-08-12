using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace MovieAPI.Infrastructure;

public static class OpenIddictClientSeeder
{
  public const string ClientId = "movieapi-client";

  public static async Task SeedAsync(IServiceProvider services)
  {
    using var scope = services.CreateScope();
    var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

    if (await manager.FindByClientIdAsync(ClientId) is not null)
    {
      return;
    }

    await manager.CreateAsync(new OpenIddictApplicationDescriptor
    {
      ClientId = ClientId,
      ClientType = ClientTypes.Public,
      Permissions =
      {
        Permissions.Endpoints.Token,
        Permissions.Endpoints.Revocation,
        Permissions.GrantTypes.Password,
        Permissions.GrantTypes.RefreshToken,
        Permissions.Scopes.Email,
        Permissions.Scopes.Profile,
        Permissions.Scopes.Roles,
        // Without this, OpenIddict silently issues an access token only - no refresh
        // token - since offline_access is what actually signals "this client wants a
        // refresh token", independent of AllowRefreshTokenFlow being enabled server-side.
        Permissions.Prefixes.Scope + Scopes.OfflineAccess,
      },
    });
  }
}
