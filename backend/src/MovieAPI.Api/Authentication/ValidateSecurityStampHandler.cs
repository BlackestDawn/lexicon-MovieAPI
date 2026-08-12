using Microsoft.AspNetCore.Identity;
using MovieAPI.Domain.Constants;
using MovieAPI.Domain.Entities;
using OpenIddict.Abstractions;
using OpenIddict.Validation;
using static OpenIddict.Validation.OpenIddictValidationEvents;

namespace MovieAPI.Api.Authentication;

// Mirrors what the old JwtBearerEvents.OnTokenValidated hook did: a token's security-stamp
// claim is fixed at issuance, so comparing it against the user's current stamp on every
// authenticated request means anything that bumps the stamp (password change/reset)
// invalidates every access token issued before that point, without needing a token
// blacklist. Deliberately a real per-request DB read, not cached - the whole point is no
// staleness window.
public class ValidateSecurityStampHandler(UserManager<ApplicationUser> userManager)
  : IOpenIddictValidationHandler<ProcessAuthenticationContext>
{
  public async ValueTask HandleAsync(ProcessAuthenticationContext context)
  {
    var principal = context.AccessTokenPrincipal;
    if (principal is null)
    {
      return;
    }

    var userId = principal.GetClaim(OpenIddictConstants.Claims.Subject);
    var tokenStamp = principal.GetClaim(CustomClaimTypes.SecurityStamp);

    if (userId is null || tokenStamp is null)
    {
      context.Reject(
        error: OpenIddictConstants.Errors.InvalidToken,
        description: "The token is missing required claims.");
      return;
    }

    var user = await userManager.FindByIdAsync(userId);
    if (user is null || !string.Equals(user.SecurityStamp, tokenStamp, StringComparison.Ordinal))
    {
      context.Reject(
        error: OpenIddictConstants.Errors.InvalidToken,
        description: "The token is no longer valid.");
    }
  }
}
