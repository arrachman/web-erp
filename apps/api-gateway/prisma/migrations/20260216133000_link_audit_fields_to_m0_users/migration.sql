BEGIN;

ALTER TABLE public."m0_users"
  ADD COLUMN IF NOT EXISTS "id_text" TEXT GENERATED ALWAYS AS (("id")::text) STORED;

CREATE UNIQUE INDEX IF NOT EXISTS "ux_m0_users_id_text"
  ON public."m0_users"("id_text");

INSERT INTO public."m0_users" (
  "email",
  "username",
  "password_hash",
  "full_name",
  "is_active",
  "created_by",
  "updated_by"
)
VALUES (
  'system@local.internal',
  'system',
  'pbkdf2$v1$sha512$210000$AAAAAAAAAAAAAAAAAAAAAA==$AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==',
  'System Background Process',
  false,
  NULL,
  NULL
)
ON CONFLICT ("username")
DO UPDATE SET
  "email" = EXCLUDED."email",
  "full_name" = EXCLUDED."full_name",
  "is_active" = false,
  "deleted_at" = NULL,
  "deleted_by" = NULL,
  "updated_by" = NULL,
  "updated_at" = NOW();

DO $$
DECLARE
  r RECORD;
  v_system_id_text TEXT;
  v_conname TEXT;
BEGIN
  SELECT "id"::text
  INTO v_system_id_text
  FROM public."m0_users"
  WHERE "username" = 'system'
  LIMIT 1;

  IF v_system_id_text IS NULL THEN
    RAISE EXCEPTION 'System user not found after upsert';
  END IF;

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
               WHERE u."id"::text = btrim(t.%I)
          )',
      r.table_schema,
      r.table_name,
      r.column_name,
      v_system_id_text,
      r.column_name,
      r.column_name,
      r.column_name,
      r.column_name
    );
  END LOOP;

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
                   WHERE u."id"::text = btrim(t.deleted_by)
              )
         )',
      r.table_schema,
      r.table_name,
      v_system_id_text
    );
  END LOOP;

  FOR r IN
    SELECT c.table_schema, c.table_name, c.column_name
    FROM information_schema.columns c
    WHERE c.table_schema = 'public'
      AND c.column_name IN ('created_by', 'updated_by', 'deleted_by')
    ORDER BY c.table_name, c.column_name
  LOOP
    v_conname := 'fk_aud_' || substr(r.table_name, 1, 20) || '_' || substr(r.column_name, 1, 3) || '_' || substr(md5(r.table_name || ':' || r.column_name), 1, 8);

    IF NOT EXISTS (
      SELECT 1
      FROM pg_constraint con
      JOIN pg_class rel ON rel.oid = con.conrelid
      JOIN pg_namespace nsp ON nsp.oid = rel.relnamespace
      WHERE nsp.nspname = r.table_schema
        AND rel.relname = r.table_name
        AND con.conname = v_conname
    ) THEN
      EXECUTE format(
        'ALTER TABLE %I.%I
           ADD CONSTRAINT %I
           FOREIGN KEY (%I)
           REFERENCES public."m0_users"("id_text")
           ON UPDATE CASCADE
           ON DELETE SET NULL',
        r.table_schema,
        r.table_name,
        v_conname,
        r.column_name
      );
    END IF;
  END LOOP;
END $$;

COMMIT;
