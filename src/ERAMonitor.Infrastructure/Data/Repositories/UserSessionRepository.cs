using Microsoft.EntityFrameworkCore;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Interfaces.Repositories;

namespace ERAMonitor.Infrastructure.Data.Repositories;

public class UserSessionRepository : Repository<UserSession>, IUserSessionRepository
{
    public UserSessionRepository(ApplicationDbContext context) : base(context)
    {
    }
    
    public async Task<UserSession?> GetByRefreshTokenHashAsync(string tokenHash)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.TokenHash == tokenHash);
    }
    
    public async Task<List<UserSession>> GetByUserIdAsync(Guid userId, bool includeRevoked = false)
    {
        var query = _dbSet.Where(s => s.UserId == userId);
        
        if (!includeRevoked)
        {
            query = query.Where(s => !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow);
        }
        
        return await query.OrderByDescending(s => s.LastActiveAt).ToListAsync();
    }
    
    public async Task<int> GetActiveSessionCountAsync(Guid userId)
    {
        return await _dbSet.CountAsync(s => s.UserId == userId && !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow);
    }
    
    public async Task RevokeAllUserSessionsAsync(Guid userId, string? reason = null)
    {
        var activeSessions = await _dbSet
            .Where(s => s.UserId == userId && !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();
            
        foreach (var session in activeSessions)
        {
            session.IsRevoked = true;
            session.RevokedAt = DateTime.UtcNow;
            session.RevokedReason = reason ?? "Revoked all sessions";
        }
    }
    
    public async Task CleanupExpiredSessionsAsync()
    {
        var expiredSessions = await _dbSet
            .Where(s => s.ExpiresAt < DateTime.UtcNow.AddDays(-30))
            .ToListAsync();
            
        _dbSet.RemoveRange(expiredSessions);
    }
}
