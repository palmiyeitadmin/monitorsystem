using Microsoft.EntityFrameworkCore;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Infrastructure.Data;

namespace ERAMonitor.Infrastructure.Repositories;

public class HostDiskRepository : IHostDiskRepository
{
    private readonly ApplicationDbContext _context;

    public HostDiskRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<HostDisk>> GetByHostAsync(Guid hostId)
    {
        return await _context.HostDisks
            .Where(d => d.HostId == hostId)
            .ToListAsync();
    }

    public async Task<HostDisk?> GetByHostAndNameAsync(Guid hostId, string name)
    {
        return await _context.HostDisks
            .FirstOrDefaultAsync(d => d.HostId == hostId && d.Name == name);
    }

    public async Task<HostDisk> AddAsync(HostDisk disk)
    {
        await _context.HostDisks.AddAsync(disk);
        return disk;
    }

    public void Update(HostDisk disk)
    {
        _context.HostDisks.Update(disk);
    }

    public void Remove(HostDisk disk)
    {
        _context.HostDisks.Remove(disk);
    }

    public async Task UpsertDisksAsync(Guid hostId, List<HostDisk> disks)
    {
        // This is a simplified implementation. 
        // In a real scenario, we might want to be more careful about tracking changes.
        
        var existingDisks = await GetByHostAsync(hostId);
        var existingMap = existingDisks.ToDictionary(d => d.Name);
        
        foreach (var disk in disks)
        {
            if (existingMap.TryGetValue(disk.Name, out var existing))
            {
                // Update
                existing.MountPoint = disk.MountPoint;
                existing.FileSystem = disk.FileSystem;
                existing.TotalGb = disk.TotalGb;
                existing.UsedGb = disk.UsedGb;
                existing.UsedPercent = disk.UsedPercent;
                existing.UpdatedAt = DateTime.UtcNow;
                
                Update(existing);
            }
            else
            {
                // Add
                disk.HostId = hostId;
                await AddAsync(disk);
            }
        }
        
        // Optional: Remove disks that are no longer present?
        // Usually we keep them but mark as missing, or just leave them until cleanup.
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
