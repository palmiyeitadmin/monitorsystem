using System.Security.Claims;
using ERAMonitor.Core.Entities;

namespace ERAMonitor.Core.Interfaces.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string HashToken(string token);
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
