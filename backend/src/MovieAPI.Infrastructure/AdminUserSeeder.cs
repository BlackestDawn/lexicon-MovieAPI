using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MovieAPI.Domain.Constants;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Infrastructure;

public static class AdminUserSeeder
{
  public static async Task SeedAsync(IServiceProvider services)
  {
    using var scope = services.CreateScope();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    // Trimmed defensively - values sourced from a secret manager (e.g. piping
    // `openssl rand` straight into `gcloud secrets versions add`) routinely carry
    // an accidental trailing newline that becomes part of the raw env var value,
    // silently baking an unusable password into the seeded account.
    var email = configuration["Seed:AdminEmail"]?.Trim();
    var password = configuration["Seed:AdminPassword"]?.Trim();

    // Not configured means opted out, not misconfigured - Production shouldn't get a
    // default admin account with a known password unless it explicitly asks for one.
    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
    {
      return;
    }

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Never touch an account that already exists - it may have had its password
    // changed since, and this seeder isn't a password-reset mechanism.
    if (await userManager.FindByEmailAsync(email) is not null)
    {
      return;
    }

    var user = new ApplicationUser { UserName = email, Email = email, DisplayName = email.Split('@')[0] };
    var result = await userManager.CreateAsync(user, password);
    if (!result.Succeeded)
    {
      var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(AdminUserSeeder));
      logger.LogError("Failed to seed admin account '{Email}': {Errors}",
        email, string.Join("; ", result.Errors.Select(e => e.Description)));
      return;
    }

    await userManager.AddToRoleAsync(user, Roles.Administrator);
  }
}
