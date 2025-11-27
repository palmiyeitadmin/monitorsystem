using ERAMonitor.Core.Entities;

namespace ERAMonitor.Core.Interfaces.Repositories;

public interface IUserSessionRepository : IRepository<UserSession>
{
    Task<UserSession?> GetByRefreshTokenHashAsync(string tokenHash);
    Task<List<UserSession>> GetByUserIdAsync(Guid userId, bool includeRevoked = false);
    Task<int> GetActiveSessionCountAsync(Guid userId);
    Task RevokeAllUserSessionsAsync(Guid userId, string? reason = null);
    Task CleanupExpiredSessionsAsync();
}
