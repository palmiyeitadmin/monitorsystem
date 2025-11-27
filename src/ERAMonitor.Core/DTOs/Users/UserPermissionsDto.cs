namespace ERAMonitor.Core.DTOs.Users;

public class UserPermissionsDto
{
    // Dashboard
    public bool ViewDashboard { get; set; } = true;
    
    // Hosts
    public bool ViewHosts { get; set; } = true;
    public bool ManageHosts { get; set; } = false;
    public bool DeleteHosts { get; set; } = false;
    
    // Services
    public bool ViewServices { get; set; } = true;
    public bool ManageServices { get; set; } = false;
    
    // Checks
    public bool ViewChecks { get; set; } = true;
    public bool ManageChecks { get; set; } = false;
    public bool DeleteChecks { get; set; } = false;
    
    // Incidents
    public bool ViewIncidents { get; set; } = true;
    public bool CreateIncidents { get; set; } = false;
    public bool ManageIncidents { get; set; } = false;
    public bool CloseIncidents { get; set; } = false;
    
    // Customers
    public bool ViewCustomers { get; set; } = false;
    public bool ManageCustomers { get; set; } = false;
    
    // Users
    public bool ViewUsers { get; set; } = false;
    public bool ManageUsers { get; set; } = false;
    
    // Reports
    public bool ViewReports { get; set; } = true;
    public bool GenerateReports { get; set; } = false;
    
    // Notifications
    public bool ManageNotifications { get; set; } = false;
    
    // Settings
    public bool ManageSettings { get; set; } = false;
    
    // Audit
    public bool ViewAuditLogs { get; set; } = false;
}

public class UpdateUserPermissionsRequest
{
    public UserPermissionsDto Permissions { get; set; } = new();
}
