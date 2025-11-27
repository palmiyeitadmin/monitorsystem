-- List all hosts with details
SELECT 
    "Id",
    "Name",
    "Hostname",
    "CurrentStatus",
    "MonitoringEnabled",
    "OsType",
    "OrganizationId",
    "CreatedAt"
FROM "Hosts" 
ORDER BY "CreatedAt" DESC;
