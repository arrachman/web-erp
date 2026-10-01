BEGIN;

DO $$
DECLARE
  r RECORD;
  v_system_id INTEGER;
  v_conname TEXT;
BEGIN
  SELECT id
  INTO v_system_id
  FROM public."m0_users"
  WHERE username = 'system'
  ORDER BY id ASC
  LIMIT 1;

  IF v_system_id IS NULL THEN
    RAISE EXCEPTION 'System user not found in m0_users';
  END IF;

  -- Drop previous TEXT-based audit FKs
  FOR r IN
    SELECT conname, nsp.nspname AS schema_name, rel.relname AS table_name
    FROM pg_constraint con
    JOIN pg_class rel ON rel.oid = con.conrelid
    JOIN pg_namespace nsp ON nsp.oid = rel.relnamespace
    WHERE nsp.nspname = 'public'
      AND con.conname LIKE 'fk_aud_%'
  LOOP
    EXECUTE format('ALTER TABLE %I.%I DROP CONSTRAINT %I', r.schema_name, r.table_name, r.conname);
  END LOOP;

  -- created_by and updated_by: replace invalid/empty/null with system id, then cast to integer
  FOR r IN
    SELECT c.table_schema, c.table_name, c.column_name
    FROM information_schema.columns c
    WHERE c.table_schema = 'public'
      AND c.column_name IN ('created_by', 'updated_by')
    ORDER BY c.table_name, c.column_name
  LOOP
    EXECUTE format(
      'UPDATE %I.%I t
       SET %I = %L
       WHERE %I IS NULL
          OR btrim(%I) = ''''
          OR %I !~ ''^[0-9]+$''
          OR NOT EXISTS (
               SELECT 1
               FROM public."m0_users" u
               WHERE u.id = nullif(btrim(t.%I), '''')::integer
          )',
      r.table_schema,
      r.table_name,
      r.column_name,
      v_system_id::text,
      r.column_name,
      r.column_name,
      r.column_name,
      r.column_name
    );

    EXECUTE format(
      'ALTER TABLE %I.%I
         ALTER COLUMN %I TYPE INTEGER
         USING nullif(btrim(%I), '''')::integer',
      r.table_schema,
      r.table_name,
      r.column_name,
      r.column_name
    );
  END LOOP;

  -- deleted_by: keep NULL as NULL, replace invalid non-null with system id, then cast to integer
  FOR r IN
    SELECT c.table_schema, c.table_name
    FROM information_schema.columns c
    WHERE c.table_schema = 'public'
      AND c.column_name = 'deleted_by'
    ORDER BY c.table_name
  LOOP
    EXECUTE format(
      'UPDATE %I.%I t
       SET deleted_by = %L
       WHERE deleted_by IS NOT NULL
         AND (
              btrim(deleted_by) = ''''
              OR deleted_by !~ ''^[0-9]+$''
              OR NOT EXISTS (
                   SELECT 1
                   FROM public."m0_users" u
                   WHERE u.id = nullif(btrim(t.deleted_by), '''')::integer
              )
         )',
      r.table_schema,
      r.table_name,
      v_system_id::text
    );

    EXECUTE format(
      'ALTER TABLE %I.%I
         ALTER COLUMN deleted_by TYPE INTEGER
         USING nullif(btrim(deleted_by), '''')::integer',
      r.table_schema,
      r.table_name
    );
  END LOOP;

  -- Re-add audit FK directly to m0_users(id)
  FOR r IN
    SELECT c.table_schema, c.table_name, c.column_name
    FROM information_schema.columns c
    WHERE c.table_schema = 'public'
      AND c.column_name IN ('created_by', 'updated_by', 'deleted_by')
    ORDER BY c.table_name, c.column_name
  LOOP
    v_conname := 'fk_aud_' || substr(r.table_name, 1, 20) || '_' || substr(r.column_name, 1, 3) || '_' || substr(md5(r.table_name || ':' || r.column_name), 1, 8);

    EXECUTE format(
      'ALTER TABLE %I.%I
         ADD CONSTRAINT %I
         FOREIGN KEY (%I)
         REFERENCES public."m0_users"(id)
         ON UPDATE CASCADE
         ON DELETE SET NULL',
      r.table_schema,
      r.table_name,
      v_conname,
      r.column_name
    );
  END LOOP;
END $$;

DROP INDEX IF EXISTS public."ux_m0_users_id_text";
ALTER TABLE public."m0_users" DROP COLUMN IF EXISTS "id_text";

COMMIT;
