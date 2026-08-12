using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MovieAPI.Domain.Constants;
using MovieAPI.Domain.Entities;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace MovieAPI.Api.Controllers;

/// <summary>
/// OAuth2 token endpoint (password + refresh_token grants), backed by ASP.NET Identity.
/// </summary>
public class AuthorizationController(
  UserManager<ApplicationUser> userManager,
  SignInManager<ApplicationUser> signInManager) : ControllerBase
{
  [HttpPost("~/connect/token")]
  [IgnoreAntiforgeryToken]
  [Produces("application/json")]
  public async Task<IActionResult> Exchange()
  {
    var request = HttpContext.GetOpenIddictServerRequest()
      ?? throw new InvalidOperationException("The OpenIddict server request cannot be retrieved.");

    if (request.IsPasswordGrantType())
    {
      var user = await userManager.FindByEmailAsync(request.Username ?? string.Empty);
      if (user is not null)
      {
        var passwordResult = await signInManager.CheckPasswordSignInAsync(user, request.Password ?? string.Empty, lockoutOnFailure: true);
        if (passwordResult.Succeeded)
        {
          return SignIn(await CreatePrincipalAsync(user), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
      }

      return InvalidGrant("Invalid email or password.");
    }

    if (request.IsRefreshTokenGrantType())
    {
      var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
      var userId = result.Principal?.GetClaim(Claims.Subject);
      var user = userId is null ? null : await userManager.FindByIdAsync(userId);

      // A missing user or one that can no longer sign in (locked out, deleted) invalidates
      // the refresh token just like the security-stamp check invalidates access tokens -
      // there's no separate revocation list to consult, the live user state is the source
      // of truth.
      if (user is null || !await signInManager.CanSignInAsync(user))
      {
        return InvalidGrant("The refresh token is no longer valid.");
      }

      return SignIn(await CreatePrincipalAsync(user), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    throw new InvalidOperationException("The specified grant type is not supported.");
  }

  private async Task<ClaimsPrincipal> CreatePrincipalAsync(ApplicationUser user)
  {
    // Gives us ClaimTypes.NameIdentifier (read by ClaimsPrincipalExtensions.GetUserId) and
    // one ClaimTypes.Role claim per role.
    var principal = await signInManager.CreateUserPrincipalAsync(user);

    // "sub" is what OpenIddict itself keys authorizations/tokens on internally - distinct
    // from the ClaimTypes.NameIdentifier URI claim above, which is what the app's own code
    // reads. Security stamp is re-embedded (rather than relying on Identity's own stamp
    // claim) to match the constant already used by ValidateSecurityStampHandler.
    principal.SetClaim(Claims.Subject, user.Id.ToString());
    principal.SetClaim(CustomClaimTypes.SecurityStamp, user.SecurityStamp);

    // [Authorize(Roles = ...)] calls User.IsInRole, which checks claims typed with the
    // *current* ClaimsIdentity.RoleClaimType - and the identity OpenIddict's validation
    // handler builds when it later reads this token back uses its own short "role" claim
    // type (Claims.Role), not ASP.NET Identity's long ClaimTypes.Role URI. Without this,
    // every [Authorize(Roles = ...)] check would fail despite the role claim being present.
    var identity = (ClaimsIdentity)principal.Identity!;
    foreach (var role in principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList())
    {
      identity.AddClaim(new Claim(Claims.Role, role));
    }

    // offline_access is what actually tells OpenIddict to issue a refresh token
    // alongside the access token - AllowRefreshTokenFlow() alone only permits the
    // grant type, it doesn't cause one to be issued.
    principal.SetScopes(Scopes.Email, Scopes.Profile, Scopes.Roles, Scopes.OfflineAccess);

    // No "openid" scope is ever requested (this is plain OAuth2, not OIDC), so no identity
    // token is issued - every claim only needs to land in the access token.
    foreach (var claim in principal.Claims)
    {
      claim.SetDestinations(Destinations.AccessToken);
    }

    return principal;
  }

  private ForbidResult InvalidGrant(string description) =>
    Forbid(
      new AuthenticationProperties(new Dictionary<string, string?>
      {
        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description,
      }),
      OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
}
