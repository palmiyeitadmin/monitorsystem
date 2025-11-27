ALTER TABLE "Customers" ADD COLUMN IF NOT EXISTS "SecondaryContactName" text NULL;
ALTER TABLE "Customers" ADD COLUMN IF NOT EXISTS "SecondaryContactPhone" text NULL;
ALTER TABLE "Customers" ADD COLUMN IF NOT EXISTS "ContactMobile" text NULL;
ALTER TABLE "Customers" ADD COLUMN IF NOT EXISTS "ContactJobTitle" text NULL;
ALTER TABLE "Customers" ADD COLUMN IF NOT EXISTS "ApiEnabled" boolean NOT NULL DEFAULT false;
ALTER TABLE "Customers" ADD COLUMN IF NOT EXISTS "ApiKey" text NULL;
ALTER TABLE "Customers" ADD COLUMN IF NOT EXISTS "ApiRateLimit" integer NOT NULL DEFAULT 1000;
ALTER TABLE "Customers" ADD COLUMN IF NOT EXISTS "AssignedAdminId" uuid NULL;
