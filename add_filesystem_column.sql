-- Add missing FileSystem column to Hosts table
ALTER TABLE "Hosts" 
ADD COLUMN IF NOT EXISTS "FileSystem" VARCHAR(50);
