using MovieAPI.Domain.Entities;

namespace MovieAPI.Infrastructure.Interfaces;

public interface IRefreshTokenRepository : IRepositoryBase<RefreshToken>
{
  Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken token);
  Task RevokeAllActiveForUserAsync(Guid userId, CancellationToken token);
}
