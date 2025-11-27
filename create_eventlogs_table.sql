-- Create EventLogs table
CREATE TABLE IF NOT EXISTS "EventLogs" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "HostId" uuid NOT NULL,
    "LogName" text NOT NULL,
    "EventId" integer NOT NULL,
    "Level" text NOT NULL,
    "Source" text NOT NULL,
    "Category" text NOT NULL,
    "Message" text NOT NULL,
    "TimeCreated" timestamp with time zone NOT NULL,
    "RecordedAt" timestamp with time zone NOT NULL DEFAULT now(),
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT now(),
    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT now(),
    CONSTRAINT "PK_EventLogs" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_EventLogs_Hosts_HostId" FOREIGN KEY ("HostId") REFERENCES "Hosts" ("Id") ON DELETE CASCADE
);

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS "IX_EventLogs_HostId" ON "EventLogs" ("HostId");
CREATE INDEX IF NOT EXISTS "IX_EventLogs_Category" ON "EventLogs" ("Category");
CREATE INDEX IF NOT EXISTS "IX_EventLogs_Level" ON "EventLogs" ("Level");
CREATE INDEX IF NOT EXISTS "IX_EventLogs_TimeCreated" ON "EventLogs" ("TimeCreated" DESC);
CREATE INDEX IF NOT EXISTS "IX_EventLogs_RecordedAt" ON "EventLogs" ("RecordedAt" DESC);
