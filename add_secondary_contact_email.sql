-- Add missing SecondaryContactEmail column to Customers table
ALTER TABLE "Customers" 
ADD COLUMN IF NOT EXISTS "SecondaryContactEmail" VARCHAR(255);
