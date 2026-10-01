-- Drop foreign keys that currently point to uuid columns
ALTER TABLE "m0_users" DROP CONSTRAINT IF EXISTS "m0_users_warehouse_id_fkey";
ALTER TABLE "m0_menu" DROP CONSTRAINT IF EXISTS "m0_menu_parent_id_fkey";
ALTER TABLE "m0_role_menu" DROP CONSTRAINT IF EXISTS "m0_role_menu_role_id_fkey";
ALTER TABLE "m0_role_menu" DROP CONSTRAINT IF EXISTS "m0_role_menu_menu_id_fkey";
ALTER TABLE "m0_role_permission" DROP CONSTRAINT IF EXISTS "m0_role_permission_role_id_fkey";
ALTER TABLE "m0_role_permission" DROP CONSTRAINT IF EXISTS "m0_role_permission_permission_id_fkey";
ALTER TABLE "m0_department" DROP CONSTRAINT IF EXISTS "m0_department_parent_id_fkey";
ALTER TABLE "m0_user_role" DROP CONSTRAINT IF EXISTS "m0_user_role_user_id_fkey";
ALTER TABLE "m0_user_role" DROP CONSTRAINT IF EXISTS "m0_user_role_role_id_fkey";
ALTER TABLE "m0_user_department" DROP CONSTRAINT IF EXISTS "m0_user_department_user_id_fkey";
ALTER TABLE "m0_user_department" DROP CONSTRAINT IF EXISTS "m0_user_department_department_id_fkey";
ALTER TABLE "m1_city" DROP CONSTRAINT IF EXISTS "m1_city_province_id_fkey";
ALTER TABLE "m1_city_sla" DROP CONSTRAINT IF EXISTS "m1_city_sla_city_id_fkey";
ALTER TABLE "m1_warehouse" DROP CONSTRAINT IF EXISTS "m1_warehouse_city_id_fkey";
ALTER TABLE "m1_item" DROP CONSTRAINT IF EXISTS "m1_item_uom_id_fkey";
ALTER TABLE "m2_outbound" DROP CONSTRAINT IF EXISTS "m2_outbound_customer_id_fkey";
ALTER TABLE "m2_outbound" DROP CONSTRAINT IF EXISTS "m2_outbound_destination_city_id_fkey";
ALTER TABLE "m2_outbound_detail" DROP CONSTRAINT IF EXISTS "m2_outbound_detail_do_id_fkey";
ALTER TABLE "m2_outbound_detail" DROP CONSTRAINT IF EXISTS "m2_outbound_detail_item_id_fkey";
ALTER TABLE "m2_outbound_detail_batch" DROP CONSTRAINT IF EXISTS "m2_outbound_detail_batch_outbound_detail_id_fkey";
ALTER TABLE "m2_inbound" DROP CONSTRAINT IF EXISTS "m2_inbound_supplier_id_fkey";
ALTER TABLE "m2_inbound" DROP CONSTRAINT IF EXISTS "m2_inbound_warehouse_id_fkey";
ALTER TABLE "m2_inbound_detail" DROP CONSTRAINT IF EXISTS "m2_inbound_detail_inbound_id_fkey";
ALTER TABLE "m2_inbound_detail" DROP CONSTRAINT IF EXISTS "m2_inbound_detail_item_id_fkey";
ALTER TABLE "m2_inbound_detail_batch" DROP CONSTRAINT IF EXISTS "m2_inbound_detail_batch_inbound_detail_id_fkey";
ALTER TABLE "m0_session" DROP CONSTRAINT IF EXISTS "m0_session_user_id_fkey";
ALTER TABLE "m0_auditlog" DROP CONSTRAINT IF EXISTS "m0_auditlog_user_id_fkey";

-- Snapshot mapping tables (uuid -> id) so conversion does not read tables being altered
CREATE TEMP TABLE tmp_m0_users AS SELECT uuid, id FROM "m0_users";
CREATE TEMP TABLE tmp_m0_role AS SELECT uuid, id FROM "m0_role";
CREATE TEMP TABLE tmp_m0_menu AS SELECT uuid, id FROM "m0_menu";
CREATE TEMP TABLE tmp_m0_permission AS SELECT uuid, id FROM "m0_permission";
CREATE TEMP TABLE tmp_m0_department AS SELECT uuid, id FROM "m0_department";
CREATE TEMP TABLE tmp_m1_contact AS SELECT uuid, id FROM "m1_contact";
CREATE TEMP TABLE tmp_m1_uom AS SELECT uuid, id FROM "m1_uom";
CREATE TEMP TABLE tmp_m1_province AS SELECT uuid, id FROM "m1_province";
CREATE TEMP TABLE tmp_m1_city AS SELECT uuid, id FROM "m1_city";
CREATE TEMP TABLE tmp_m1_warehouse AS SELECT uuid, id FROM "m1_warehouse";
CREATE TEMP TABLE tmp_m1_item AS SELECT uuid, id FROM "m1_item";
CREATE TEMP TABLE tmp_m2_outbound AS SELECT uuid, id FROM "m2_outbound";
CREATE TEMP TABLE tmp_m2_outbound_detail AS SELECT uuid, id FROM "m2_outbound_detail";
CREATE TEMP TABLE tmp_m2_inbound AS SELECT uuid, id FROM "m2_inbound";
CREATE TEMP TABLE tmp_m2_inbound_detail AS SELECT uuid, id FROM "m2_inbound_detail";

-- Helper function to map UUID values to integer IDs from snapshot mapping tables
CREATE OR REPLACE FUNCTION map_uuid_to_id(p_table regclass, p_uuid text)
RETURNS integer
LANGUAGE plpgsql
AS $$
DECLARE
  v_id integer;
BEGIN
  IF p_uuid IS NULL THEN
    RETURN NULL;
  END IF;

  EXECUTE format('SELECT id FROM %s WHERE uuid = $1', p_table)
    INTO v_id
    USING p_uuid;

  RETURN v_id;
END;
$$;

-- Convert FK columns from TEXT(uuid) to INTEGER(id)
ALTER TABLE "m0_users"
  ALTER COLUMN "warehouse_id" TYPE INTEGER
  USING map_uuid_to_id('tmp_m1_warehouse'::regclass, "warehouse_id");

ALTER TABLE "m0_menu"
  ALTER COLUMN "parent_id" TYPE INTEGER
  USING map_uuid_to_id('tmp_m0_menu'::regclass, "parent_id");

ALTER TABLE "m0_role_menu"
  ALTER COLUMN "role_id" TYPE INTEGER USING map_uuid_to_id('tmp_m0_role'::regclass, "role_id"),
  ALTER COLUMN "menu_id" TYPE INTEGER USING map_uuid_to_id('tmp_m0_menu'::regclass, "menu_id");

ALTER TABLE "m0_role_permission"
  ALTER COLUMN "role_id" TYPE INTEGER USING map_uuid_to_id('tmp_m0_role'::regclass, "role_id"),
  ALTER COLUMN "permission_id" TYPE INTEGER USING map_uuid_to_id('tmp_m0_permission'::regclass, "permission_id");

ALTER TABLE "m0_department"
  ALTER COLUMN "parent_id" TYPE INTEGER
  USING map_uuid_to_id('tmp_m0_department'::regclass, "parent_id");

ALTER TABLE "m0_user_role"
  ALTER COLUMN "user_id" TYPE INTEGER USING map_uuid_to_id('tmp_m0_users'::regclass, "user_id"),
  ALTER COLUMN "role_id" TYPE INTEGER USING map_uuid_to_id('tmp_m0_role'::regclass, "role_id");

ALTER TABLE "m0_user_department"
  ALTER COLUMN "user_id" TYPE INTEGER USING map_uuid_to_id('tmp_m0_users'::regclass, "user_id"),
  ALTER COLUMN "department_id" TYPE INTEGER USING map_uuid_to_id('tmp_m0_department'::regclass, "department_id");

ALTER TABLE "m1_city"
  ALTER COLUMN "province_id" TYPE INTEGER USING map_uuid_to_id('tmp_m1_province'::regclass, "province_id");

ALTER TABLE "m1_city_sla"
  ALTER COLUMN "city_id" TYPE INTEGER USING map_uuid_to_id('tmp_m1_city'::regclass, "city_id");

ALTER TABLE "m1_warehouse"
  ALTER COLUMN "city_id" TYPE INTEGER USING map_uuid_to_id('tmp_m1_city'::regclass, "city_id");

ALTER TABLE "m1_item"
  ALTER COLUMN "uom_id" TYPE INTEGER USING map_uuid_to_id('tmp_m1_uom'::regclass, "uom_id");

ALTER TABLE "m2_outbound"
  ALTER COLUMN "customer_id" TYPE INTEGER USING map_uuid_to_id('tmp_m1_contact'::regclass, "customer_id"),
  ALTER COLUMN "destination_city_id" TYPE INTEGER USING map_uuid_to_id('tmp_m1_city'::regclass, "destination_city_id");

ALTER TABLE "m2_outbound_detail"
  ALTER COLUMN "do_id" TYPE INTEGER USING map_uuid_to_id('tmp_m2_outbound'::regclass, "do_id"),
  ALTER COLUMN "item_id" TYPE INTEGER USING map_uuid_to_id('tmp_m1_item'::regclass, "item_id");

ALTER TABLE "m2_outbound_detail_batch"
  ALTER COLUMN "outbound_detail_id" TYPE INTEGER
  USING map_uuid_to_id('tmp_m2_outbound_detail'::regclass, "outbound_detail_id");

ALTER TABLE "m2_inbound"
  ALTER COLUMN "supplier_id" TYPE INTEGER USING map_uuid_to_id('tmp_m1_contact'::regclass, "supplier_id"),
  ALTER COLUMN "warehouse_id" TYPE INTEGER USING map_uuid_to_id('tmp_m1_warehouse'::regclass, "warehouse_id");

ALTER TABLE "m2_inbound_detail"
  ALTER COLUMN "inbound_id" TYPE INTEGER USING map_uuid_to_id('tmp_m2_inbound'::regclass, "inbound_id"),
  ALTER COLUMN "item_id" TYPE INTEGER USING map_uuid_to_id('tmp_m1_item'::regclass, "item_id");

ALTER TABLE "m2_inbound_detail_batch"
  ALTER COLUMN "inbound_detail_id" TYPE INTEGER
  USING map_uuid_to_id('tmp_m2_inbound_detail'::regclass, "inbound_detail_id");

ALTER TABLE "m0_session"
  ALTER COLUMN "user_id" TYPE INTEGER USING map_uuid_to_id('tmp_m0_users'::regclass, "user_id");

ALTER TABLE "m0_auditlog"
  ALTER COLUMN "user_id" TYPE INTEGER USING map_uuid_to_id('tmp_m0_users'::regclass, "user_id");

DROP FUNCTION IF EXISTS map_uuid_to_id(regclass, text);

-- Recreate foreign keys to numeric primary keys
ALTER TABLE "m0_users"
  ADD CONSTRAINT "m0_users_warehouse_id_fkey"
  FOREIGN KEY ("warehouse_id") REFERENCES "m1_warehouse"("id") ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE "m0_menu"
  ADD CONSTRAINT "m0_menu_parent_id_fkey"
  FOREIGN KEY ("parent_id") REFERENCES "m0_menu"("id") ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE "m0_role_menu"
  ADD CONSTRAINT "m0_role_menu_role_id_fkey"
  FOREIGN KEY ("role_id") REFERENCES "m0_role"("id") ON DELETE RESTRICT ON UPDATE CASCADE,
  ADD CONSTRAINT "m0_role_menu_menu_id_fkey"
  FOREIGN KEY ("menu_id") REFERENCES "m0_menu"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "m0_role_permission"
  ADD CONSTRAINT "m0_role_permission_role_id_fkey"
  FOREIGN KEY ("role_id") REFERENCES "m0_role"("id") ON DELETE RESTRICT ON UPDATE CASCADE,
  ADD CONSTRAINT "m0_role_permission_permission_id_fkey"
  FOREIGN KEY ("permission_id") REFERENCES "m0_permission"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "m0_department"
  ADD CONSTRAINT "m0_department_parent_id_fkey"
  FOREIGN KEY ("parent_id") REFERENCES "m0_department"("id") ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE "m0_user_role"
  ADD CONSTRAINT "m0_user_role_user_id_fkey"
  FOREIGN KEY ("user_id") REFERENCES "m0_users"("id") ON DELETE RESTRICT ON UPDATE CASCADE,
  ADD CONSTRAINT "m0_user_role_role_id_fkey"
  FOREIGN KEY ("role_id") REFERENCES "m0_role"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "m0_user_department"
  ADD CONSTRAINT "m0_user_department_user_id_fkey"
  FOREIGN KEY ("user_id") REFERENCES "m0_users"("id") ON DELETE RESTRICT ON UPDATE CASCADE,
  ADD CONSTRAINT "m0_user_department_department_id_fkey"
  FOREIGN KEY ("department_id") REFERENCES "m0_department"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "m1_city"
  ADD CONSTRAINT "m1_city_province_id_fkey"
  FOREIGN KEY ("province_id") REFERENCES "m1_province"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "m1_city_sla"
  ADD CONSTRAINT "m1_city_sla_city_id_fkey"
  FOREIGN KEY ("city_id") REFERENCES "m1_city"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "m1_warehouse"
  ADD CONSTRAINT "m1_warehouse_city_id_fkey"
  FOREIGN KEY ("city_id") REFERENCES "m1_city"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "m1_item"
  ADD CONSTRAINT "m1_item_uom_id_fkey"
  FOREIGN KEY ("uom_id") REFERENCES "m1_uom"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "m2_outbound"
  ADD CONSTRAINT "m2_outbound_customer_id_fkey"
  FOREIGN KEY ("customer_id") REFERENCES "m1_contact"("id") ON DELETE RESTRICT ON UPDATE CASCADE,
  ADD CONSTRAINT "m2_outbound_destination_city_id_fkey"
  FOREIGN KEY ("destination_city_id") REFERENCES "m1_city"("id") ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE "m2_outbound_detail"
  ADD CONSTRAINT "m2_outbound_detail_do_id_fkey"
  FOREIGN KEY ("do_id") REFERENCES "m2_outbound"("id") ON DELETE RESTRICT ON UPDATE CASCADE,
  ADD CONSTRAINT "m2_outbound_detail_item_id_fkey"
  FOREIGN KEY ("item_id") REFERENCES "m1_item"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "m2_outbound_detail_batch"
  ADD CONSTRAINT "m2_outbound_detail_batch_outbound_detail_id_fkey"
  FOREIGN KEY ("outbound_detail_id") REFERENCES "m2_outbound_detail"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "m2_inbound"
  ADD CONSTRAINT "m2_inbound_supplier_id_fkey"
  FOREIGN KEY ("supplier_id") REFERENCES "m1_contact"("id") ON DELETE RESTRICT ON UPDATE CASCADE,
  ADD CONSTRAINT "m2_inbound_warehouse_id_fkey"
  FOREIGN KEY ("warehouse_id") REFERENCES "m1_warehouse"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "m2_inbound_detail"
  ADD CONSTRAINT "m2_inbound_detail_inbound_id_fkey"
  FOREIGN KEY ("inbound_id") REFERENCES "m2_inbound"("id") ON DELETE RESTRICT ON UPDATE CASCADE,
  ADD CONSTRAINT "m2_inbound_detail_item_id_fkey"
  FOREIGN KEY ("item_id") REFERENCES "m1_item"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "m2_inbound_detail_batch"
  ADD CONSTRAINT "m2_inbound_detail_batch_inbound_detail_id_fkey"
  FOREIGN KEY ("inbound_detail_id") REFERENCES "m2_inbound_detail"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "m0_session"
  ADD CONSTRAINT "m0_session_user_id_fkey"
  FOREIGN KEY ("user_id") REFERENCES "m0_users"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "m0_auditlog"
  ADD CONSTRAINT "m0_auditlog_user_id_fkey"
  FOREIGN KEY ("user_id") REFERENCES "m0_users"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- Drop uuid columns from all tables
ALTER TABLE "m0_users" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m0_role" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m0_menu" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m0_role_menu" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m0_permission" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m0_role_permission" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m0_department" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m0_user_role" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m0_user_department" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m1_contact" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m1_uom" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m1_division" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m1_province" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m1_city" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m1_city_sla" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m1_warehouse" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m1_item" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m2_outbound" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m2_outbound_detail" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m2_outbound_detail_batch" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m2_inbound" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m2_inbound_detail" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m2_inbound_detail_batch" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m2_inventory_batch" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m2_inventory_ledger" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m0_session" DROP COLUMN IF EXISTS "uuid";
ALTER TABLE "m0_auditlog" DROP COLUMN IF EXISTS "uuid";
