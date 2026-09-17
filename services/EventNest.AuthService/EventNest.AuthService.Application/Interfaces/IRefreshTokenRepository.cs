using EventNest.AuthService.Domain.Entities;

namespace EventNest.AuthService.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
    Task AddAsync(RefreshToken refreshToken);
    Task RevokeAsync(string tokenHash, string? replacedByTokenHash = null);
    Task RevokeAllActiveByUserIdAsync(Guid userId);
    Task<int> DeleteExpiredAsync(DateTime utcNow, CancellationToken cancellationToken = default);
}
