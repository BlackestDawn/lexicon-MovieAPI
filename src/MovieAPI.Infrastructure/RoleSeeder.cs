using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MovieAPI.Domain.Constants;
using MovieAPI.Domain.Entities;

namespace MovieAPI.Infrastructure;

public static class RoleSeeder
{
  public static async Task SeedAsync(IServiceProvider services)
  {
    using var scope = services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

    foreach (var role in Roles.All)
    {
      if (!await roleManager.RoleExistsAsync(role))
      {
        await roleManager.CreateAsync(new ApplicationRole { Name = role });
      }
    }
  }
}
