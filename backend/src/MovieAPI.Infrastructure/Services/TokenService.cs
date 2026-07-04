using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MovieAPI.Domain.Constants;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Interfaces;

namespace MovieAPI.Infrastructure.Services;

public class TokenService(IConfiguration configuration) : ITokenService
{
  public (string Token, DateTime ExpiresAtUtc) GenerateToken(ApplicationUser user, IEnumerable<string> roles)
  {
    var issuer = configuration["Jwt:Issuer"]
      ?? throw new InvalidOperationException("Configuration value 'Jwt:Issuer' is not configured.");
    var audience = configuration["Jwt:Audience"]
      ?? throw new InvalidOperationException("Configuration value 'Jwt:Audience' is not configured.");
    var key = configuration["Jwt:Key"]
      ?? throw new InvalidOperationException("Configuration value 'Jwt:Key' is not configured.");
    var expiryMinutes = configuration.GetValue("Jwt:ExpiryMinutes", 60);

    var expiresAtUtc = DateTime.UtcNow.AddMinutes(expiryMinutes);

    var claims = new List<Claim>
    {
      new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
      new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
      new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
      new(CustomClaimTypes.SecurityStamp, user.SecurityStamp ?? string.Empty),
    };
    claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

    var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
      issuer: issuer,
      audience: audience,
      claims: claims,
      expires: expiresAtUtc,
      signingCredentials: credentials);

    return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
  }

  public (string Token, DateTime ExpiresAtUtc) GenerateRefreshToken()
  {
    var expiryDays = configuration.GetValue("Jwt:RefreshTokenExpiryDays", 7);
    var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    return (token, DateTime.UtcNow.AddDays(expiryDays));
  }

  public string HashToken(string token)
  {
    var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
    return Convert.ToBase64String(hash);
  }
}
