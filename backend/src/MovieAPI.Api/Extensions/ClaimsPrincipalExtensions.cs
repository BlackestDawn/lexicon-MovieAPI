using System.Security.Claims;

namespace MovieAPI.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
  public static Guid GetUserId(this ClaimsPrincipal principal) =>
    Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
