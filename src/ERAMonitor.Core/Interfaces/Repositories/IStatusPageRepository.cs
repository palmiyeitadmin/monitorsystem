using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.StatusPages;
using ERAMonitor.Core.Entities;

namespace ERAMonitor.Core.Interfaces.Repositories;

public interface IStatusPageRepository : IRepository<StatusPage>
{
    Task<PagedResponse<StatusPageDto>> GetPagedAsync(Guid organizationId, PagedRequest request);
    Task<StatusPageDetailDto?> GetDetailAsync(Guid id, Guid organizationId);
    Task<StatusPage?> GetBySlugAsync(string slug);
    Task<bool> SlugExistsAsync(string slug);
}

public interface IStatusPageComponentRepository : IRepository<StatusPageComponent>
{
    Task<List<StatusPageComponent>> GetByStatusPageAsync(Guid statusPageId);
}

public interface IStatusPageComponentGroupRepository : IRepository<StatusPageComponentGroup>
{
    Task<List<StatusPageComponentGroup>> GetByStatusPageAsync(Guid statusPageId);
}

public interface IStatusPageSubscriberRepository : IRepository<StatusPageSubscriber>
{
    Task<List<StatusPageSubscriber>> GetByStatusPageAsync(Guid statusPageId);
    Task<StatusPageSubscriber?> GetByTokenAsync(string token);
    Task<StatusPageSubscriber?> GetByEmailAsync(Guid statusPageId, string email);
}
