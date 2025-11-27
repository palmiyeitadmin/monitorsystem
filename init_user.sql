-- Create organization first
INSERT INTO "Organizations" ("Id", "Name", "Slug", "IsActive", "CreatedAt", "UpdatedAt")
VALUES (
  '11111111-1111-1111-1111-111111111111',
  'ERA Monitor',
  'era-monitor',
  true,
  NOW(),
  NOW()
)
ON CONFLICT ("Id") DO NOTHING;

-- Create SuperAdmin user
-- Password: Admin123! (BCrypt hash)
INSERT INTO "Users" (
  "Id",
  "Email",
  "FullName",
  "PasswordHash",
  "OrganizationId",
  "Role",
  "IsActive",
  "EmailVerified",
  "TwoFactorEnabled",
  "Timezone",
  "Locale",
  "Theme",
  "CreatedAt",
  "UpdatedAt"
)
VALUES (
  '22222222-2222-2222-2222-222222222222',
  'admin@eramonitor.local',
  'System Administrator',
  '$2a$11$rDXqKZBXZqKMPWQplZB0KeZ0DJXvYxVGOqQJz4s5pEJ8YPPWLxVam',
  '11111111-1111-1111-1111-111111111111',
  2,
  true,
  true,
  false,
  'Europe/Istanbul',
  'en',
  'dark',
  NOW(),
  NOW()
)
ON CONFLICT ("Id") DO NOTHING;
