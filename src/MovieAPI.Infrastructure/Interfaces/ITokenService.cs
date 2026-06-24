using MovieAPI.Domain.Entities;

namespace MovieAPI.Infrastructure.Interfaces;

public interface ITokenService
{
  (string Token, DateTime ExpiresAtUtc) GenerateToken(ApplicationUser user, IEnumerable<string> roles);
}
