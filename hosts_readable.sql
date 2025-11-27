SELECT 
    ROW_NUMBER() OVER (ORDER BY "Name") as "#",
    "Name",
    "Hostname",
    CASE "CurrentStatus"
        WHEN 0 THEN 'Unknown'
        WHEN 1 THEN 'Up'
        WHEN 2 THEN 'Down'
        WHEN 3 THEN 'Warning'
        WHEN 4 THEN 'Degraded'
        ELSE 'Unknown'
    END as "Status",
    CASE "MonitoringEnabled"
        WHEN true THEN 'Yes'
        WHEN false THEN 'No'
    END as "Monitoring",
    CASE "OsType"
        WHEN 0 THEN 'Linux'
        WHEN 1 THEN 'Windows'
        WHEN 2 THEN 'macOS'
        ELSE 'Other'
    END as "OS"
FROM "Hosts" 
ORDER BY "Name";
