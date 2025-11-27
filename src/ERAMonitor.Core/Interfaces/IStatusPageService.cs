using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.StatusPages;

namespace ERAMonitor.Core.Interfaces;

public interface IStatusPageService
{
    // Status Page Management
    Task<PagedResponse<StatusPageDto>> GetPagedAsync(Guid organizationId, PagedRequest request);
    Task<StatusPageDetailDto?> GetDetailAsync(Guid id, Guid organizationId);
    Task<StatusPageDetailDto?> GetBySlugAsync(string slug); // Public access doesn't require orgId/userId
    Task<StatusPageDetailDto> CreateAsync(Guid organizationId, CreateStatusPageDto dto);
    Task<StatusPageDetailDto?> UpdateAsync(Guid id, Guid organizationId, UpdateStatusPageDto dto);
    Task<bool> DeleteAsync(Guid id, Guid organizationId);
    
    // Component Management
    Task<StatusPageComponentDto> AddComponentAsync(Guid statusPageId, Guid organizationId, CreateStatusPageComponentDto dto);
    Task<StatusPageComponentDto?> UpdateComponentAsync(Guid componentId, Guid organizationId, UpdateStatusPageComponentDto dto);
    Task<bool> DeleteComponentAsync(Guid componentId, Guid organizationId);
    Task UpdateComponentStatusAsync(Guid componentId, Guid organizationId, UpdateComponentStatusDto dto);
    
    // Group Management
    Task<StatusPageComponentGroupDto> AddGroupAsync(Guid statusPageId, Guid organizationId, CreateStatusPageComponentGroupDto dto);
    Task<StatusPageComponentGroupDto?> UpdateGroupAsync(Guid groupId, Guid organizationId, UpdateStatusPageComponentGroupDto dto);
    Task<bool> DeleteGroupAsync(Guid groupId, Guid organizationId);
    
    // Subscriber Management
    Task<PagedResponse<StatusPageSubscriberDto>> GetSubscribersAsync(Guid statusPageId, Guid organizationId, PagedRequest request);
    Task<StatusPageSubscriberDto> AddSubscriberAsync(Guid statusPageId, CreateStatusPageSubscriberDto dto); // Public or Admin
    Task<bool> RemoveSubscriberAsync(Guid subscriberId, Guid statusPageId, Guid? organizationId = null); // OrgId null for public unsubscribe
    Task<bool> VerifySubscriberAsync(Guid subscriberId, string token);
}
