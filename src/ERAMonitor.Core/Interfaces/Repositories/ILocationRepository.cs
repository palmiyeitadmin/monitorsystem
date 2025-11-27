using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Locations;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Interfaces.Repositories;

public interface ILocationRepository : IRepository<Location>
{
    Task<PagedResponse<LocationDto>> GetPagedAsync(
        Guid organizationId, 
        PagedRequest request, 
        LocationCategory? category = null,
        bool? isActive = null);
    Task<LocationDetailDto?> GetDetailAsync(Guid id);
    Task<List<Location>> GetByOrganizationAsync(Guid organizationId);
    Task<List<LocationDto>> GetForMapAsync(Guid organizationId);
}
