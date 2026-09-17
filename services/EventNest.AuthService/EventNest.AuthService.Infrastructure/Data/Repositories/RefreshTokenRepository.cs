using EventNest.AuthService.Application.Interfaces;
using EventNest.AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventNest.AuthService.Infrastructure.Data.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AuthDbContext _context;

    public RefreshTokenRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
    }

    public async Task AddAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeAsync(string tokenHash, string? replacedByTokenHash = null)
    {
        var token = await GetByTokenHashAsync(tokenHash);
        if (token is not null && !token.IsRevoked)
        {
            token.Revoke(replacedByTokenHash);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RevokeAllActiveByUserIdAsync(Guid userId)
    {
        await _context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, DateTime.UtcNow));
    }

    public async Task<int> DeleteExpiredAsync(DateTime utcNow, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Where(t => t.ExpiresAt <= utcNow)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
