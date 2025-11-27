using System.Security.Claims;

namespace ERAMonitor.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetOrganizationId(this ClaimsPrincipal principal)
    {
        var orgIdClaim = principal.FindFirst("OrganizationId");
        if (orgIdClaim == null)
        {
            throw new UnauthorizedAccessException("Organization ID not found in token");
        }

        if (Guid.TryParse(orgIdClaim.Value, out var orgId))
        {
            return orgId;
        }

        throw new UnauthorizedAccessException("Invalid Organization ID in token");
    }

    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }

        if (Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        throw new UnauthorizedAccessException("Invalid User ID in token");
    }
}
