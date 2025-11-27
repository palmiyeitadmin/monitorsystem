ALTER TABLE "Customers" ADD COLUMN IF NOT EXISTS "EmergencyContactName" text NULL;
ALTER TABLE "Customers" ADD COLUMN IF NOT EXISTS "EmergencyContactPhone" text NULL;
