using ERAMonitor.Core.Entities;

namespace ERAMonitor.Core.Interfaces.Repositories;

public interface IHostDiskRepository
{
    Task<List<HostDisk>> GetByHostAsync(Guid hostId);
    Task<HostDisk?> GetByHostAndNameAsync(Guid hostId, string name);
    
    Task<HostDisk> AddAsync(HostDisk disk);
    void Update(HostDisk disk);
    void Remove(HostDisk disk);
    
    Task UpsertDisksAsync(Guid hostId, List<HostDisk> disks);
    
    Task SaveChangesAsync();
}
