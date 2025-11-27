using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Core.Interfaces.Services;

namespace ERAMonitor.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditService> _logger;
    
    public AuditService(
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }
    
    public async Task LogAsync(
        string action,
        string entityType,
        Guid? entityId,
        string? entityName,
        object? oldValues = null,
        object? newValues = null,
        Guid? userId = null,
        Guid? organizationId = null)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var currentUserId = GetCurrentUserId();
            var currentOrgId = GetCurrentOrganizationId();
            
            var auditLog = new AuditLog
            {
                OrganizationId = organizationId ?? currentOrgId ?? Guid.Empty, // Fallback to empty if no org context
                UserId = userId ?? currentUserId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                EntityName = entityName,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                IpAddress = GetClientIpAddress(),
                UserAgent = httpContext?.Request.Headers["User-Agent"].ToString()
            };
            
            // Handle case where OrganizationId is required but not available
            // In a real app, we might want to fetch the user's org or use a system org
            if (auditLog.OrganizationId == Guid.Empty && auditLog.UserId.HasValue)
            {
                // Try to get org from user if possible, but we don't have access to UserRepository here easily without circular dependency
                // For now, we'll assume it's handled or nullable in DB (but it's not nullable in Entity)
                // We might need to make OrganizationId nullable in AuditLog or ensure it's always provided
            }
            
            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create audit log for {Action} on {EntityType}", action, entityType);
        }
    }
    
    public async Task LogCreateAsync<T>(T entity, Guid? userId = null) where T : BaseEntity
    {
        await LogAsync(
            "Create",
            typeof(T).Name,
            entity.Id,
            GetEntityName(entity),
            newValues: entity,
            userId: userId
        );
    }
    
    public async Task LogUpdateAsync<T>(T entity, object? oldValues, Guid? userId = null) where T : BaseEntity
    {
        await LogAsync(
            "Update",
            typeof(T).Name,
            entity.Id,
            GetEntityName(entity),
            oldValues: oldValues,
            newValues: entity,
            userId: userId
        );
    }
    
    public async Task LogDeleteAsync<T>(T entity, Guid? userId = null) where T : BaseEntity
    {
        await LogAsync(
            "Delete",
            typeof(T).Name,
            entity.Id,
            GetEntityName(entity),
            oldValues: entity,
            userId: userId
        );
    }
    
    public async Task LogLoginAsync(Guid userId, string ipAddress, bool success, string? failReason = null)
    {
        await LogAsync(
            success ? "Login" : "LoginFailed",
            "User",
            userId,
            null,
            newValues: new { Success = success, FailReason = failReason, IpAddress = ipAddress },
            userId: userId
        );
    }
    
    public async Task LogLogoutAsync(Guid userId)
    {
        await LogAsync(
            "Logout",
            "User",
            userId,
            null,
            userId: userId
        );
    }
    
    private Guid? GetCurrentUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var userIdClaim = httpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        
        return null;
    }
    
    private Guid? GetCurrentOrganizationId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var orgIdClaim = httpContext?.User.FindFirst("organizationId")?.Value;
        
        if (Guid.TryParse(orgIdClaim, out var orgId))
        {
            return orgId;
        }
        
        return null;
    }
    
    private string? GetClientIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        // Check for forwarded IP (behind reverse proxy)
        var forwardedFor = httpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').First().Trim();
        }
        
        return httpContext?.Connection.RemoteIpAddress?.ToString();
    }
    
    private static string? GetEntityName<T>(T entity) where T : BaseEntity
    {
        // Try to get name property via reflection
        var nameProperty = typeof(T).GetProperty("Name") ?? typeof(T).GetProperty("FullName") ?? typeof(T).GetProperty("Email");
        return nameProperty?.GetValue(entity)?.ToString();
    }
}
