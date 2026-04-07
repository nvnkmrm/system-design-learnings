-- PostgreSQL initialisation script
-- Runs once when the container is first created.

-- Enable UUID generation (used by gen_random_uuid() in EF migrations)
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Enable full-text search dictionary (English stemmer used in product search)
-- Already included in standard PostgreSQL, no extension needed.

-- Optional: pg_trgm extension for LIKE/ILIKE with GIN indexes
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- Log the setup
DO $$
BEGIN
    RAISE NOTICE 'Database extensions initialised successfully for ecom_db';
END $$;
