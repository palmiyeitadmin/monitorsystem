using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class OnCallSchedule : BaseEntityWithOrganization
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Timezone { get; set; } = "Europe/Istanbul";
    
    // Rotation settings
    public OnCallRotationType RotationType { get; set; } = OnCallRotationType.Weekly;
    public int RotationLength { get; set; } = 1; // 1 week, 1 day, etc.
    public DateTime RotationStartDate { get; set; }
    public string? RotationStartTime { get; set; } = "09:00"; // When rotation changes
    
    // Current state
    public Guid? CurrentOnCallUserId { get; set; }
    public DateTime? CurrentRotationStart { get; set; }
    public DateTime? CurrentRotationEnd { get; set; }
    
    // Members in rotation order
    public virtual ICollection<OnCallScheduleMember> Members { get; set; } = new List<OnCallScheduleMember>();
    public virtual ICollection<OnCallOverride> Overrides { get; set; } = new List<OnCallOverride>();
    
    // Settings
    public bool IsActive { get; set; } = true;
    public bool NotifyOnRotation { get; set; } = true;
    
    // Navigation
    public virtual User? CurrentOnCallUser { get; set; }
}

public class OnCallScheduleMember : BaseEntity
{
    public Guid OnCallScheduleId { get; set; }
    public Guid UserId { get; set; }
    public int Order { get; set; } // Position in rotation
    
    // Navigation
    public virtual OnCallSchedule OnCallSchedule { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

public class OnCallOverride : BaseEntity
{
    public Guid OnCallScheduleId { get; set; }
    public Guid UserId { get; set; }
    
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string? Reason { get; set; }
    
    // Who created the override
    public Guid CreatedByUserId { get; set; }
    
    // Navigation
    public virtual OnCallSchedule OnCallSchedule { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual User CreatedByUser { get; set; } = null!;
}
