INSERT INTO "EventLogs" ("Id", "HostId", "LogName", "EventId", "Level", "Source", "Category", "Message", "TimeCreated", "RecordedAt", "CreatedAt", "UpdatedAt") 
VALUES (gen_random_uuid(), '6cfe4c29-12bf-434c-9d36-2c7812117dc7', 'TestLog', 100, 'Info', 'TestSource', 'TestCategory', 'Test Message', now(), now(), now(), now());
