using Microsoft.EntityFrameworkCore;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Interfaces;

namespace MovieAPI.Infrastructure.Services;

public class RefreshTokenRepository(AppDbContext context) : RepositoryBase<RefreshToken>(context), IRefreshTokenRepository
{
  protected override DbSet<RefreshToken> Set => Context.RefreshTokens;

  public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken token)
  {
    return Context.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == tokenHash, token);
  }

  public async Task RevokeAllActiveForUserAsync(Guid userId, CancellationToken token)
  {
    var activeTokens = await Context.RefreshTokens
      .Where(r => r.UserId == userId && r.RevokedAtUtc == null)
      .ToListAsync(token);

    var now = DateTime.UtcNow;
    foreach (var refreshToken in activeTokens)
    {
      refreshToken.RevokedAtUtc = now;
    }
  }
}
