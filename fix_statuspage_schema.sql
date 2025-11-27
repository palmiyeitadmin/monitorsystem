ALTER TABLE "StatusPages" ADD COLUMN IF NOT EXISTS "Description" text NULL;
ALTER TABLE "StatusPages" ADD COLUMN IF NOT EXISTS "WebsiteUrl" text NULL;
ALTER TABLE "StatusPages" ADD COLUMN IF NOT EXISTS "SupportUrl" text NULL;
ALTER TABLE "StatusPages" ADD COLUMN IF NOT EXISTS "Theme" text NULL;
ALTER TABLE "StatusPages" ADD COLUMN IF NOT EXISTS "GoogleAnalyticsId" text NULL;

ALTER TABLE "StatusPageComponents" ADD COLUMN IF NOT EXISTS "Order" integer NOT NULL DEFAULT 0;
ALTER TABLE "StatusPageComponents" ADD COLUMN IF NOT EXISTS "Status" integer NOT NULL DEFAULT 0;

ALTER TABLE "StatusPageSubscribers" ADD COLUMN IF NOT EXISTS "EmailSubscribed" boolean NOT NULL DEFAULT false;
ALTER TABLE "StatusPageSubscribers" ADD COLUMN IF NOT EXISTS "SmsSubscribed" boolean NOT NULL DEFAULT false;
ALTER TABLE "StatusPageSubscribers" ADD COLUMN IF NOT EXISTS "IsVerified" boolean NOT NULL DEFAULT false;
ALTER TABLE "StatusPageSubscribers" ADD COLUMN IF NOT EXISTS "EmailVerified" boolean NOT NULL DEFAULT false;
ALTER TABLE "StatusPageSubscribers" ADD COLUMN IF NOT EXISTS "VerificationToken" text NULL;
ALTER TABLE "StatusPageSubscribers" ADD COLUMN IF NOT EXISTS "VerifiedAt" timestamp with time zone NULL;
ALTER TABLE "StatusPageSubscribers" ADD COLUMN IF NOT EXISTS "NotifyOnIncident" boolean NOT NULL DEFAULT true;
ALTER TABLE "StatusPageSubscribers" ADD COLUMN IF NOT EXISTS "NotifyOnMaintenance" boolean NOT NULL DEFAULT true;
ALTER TABLE "StatusPageSubscribers" ADD COLUMN IF NOT EXISTS "NotifyOnResolution" boolean NOT NULL DEFAULT true;
ALTER TABLE "StatusPageSubscribers" ADD COLUMN IF NOT EXISTS "IsActive" boolean NOT NULL DEFAULT true;
ALTER TABLE "StatusPageSubscribers" ADD COLUMN IF NOT EXISTS "UnsubscribedAt" timestamp with time zone NULL;
