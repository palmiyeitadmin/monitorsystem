namespace ERAMonitor.Core.Entities;

public class HostDisk : BaseEntity
{
    public Guid HostId { get; set; }
    
    public string Name { get; set; } = string.Empty; // C:, /dev/sda1, etc.
    public string? MountPoint { get; set; } // C:\, /, /home
    public string? FileSystem { get; set; } // NTFS, ext4, xfs
    public string? Label { get; set; } // Volume label
    
    public decimal TotalGb { get; set; }
    public decimal UsedGb { get; set; }
    public decimal FreeGb => TotalGb - UsedGb;
    public decimal UsedPercent { get; set; }
    
    // Navigation
    public virtual Host Host { get; set; } = null!;
}
