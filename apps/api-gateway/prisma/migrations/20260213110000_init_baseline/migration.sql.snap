-- CreateTable
CREATE TABLE "m0_users" (
    "id" SERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "email" TEXT NOT NULL,
    "username" TEXT NOT NULL,
    "password_hash" TEXT NOT NULL,
    "full_name" TEXT,
    "avatar_url" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "last_login" TIMESTAMP(3),
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "user_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "m0_role" (
    "id" SERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "description" TEXT,
    "is_system" BOOLEAN NOT NULL DEFAULT false,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "m0_role_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "m0_menu" (
    "id" SERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "key" TEXT NOT NULL,
    "title" TEXT NOT NULL,
    "path" TEXT,
    "icon" TEXT,
    "type" TEXT NOT NULL DEFAULT 'ITEM',
    "parent_id" TEXT,
    "sort_order" INTEGER NOT NULL DEFAULT 0,
    "is_visible" BOOLEAN NOT NULL DEFAULT true,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "permission_name" TEXT,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "m0_menu_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "m0_role_menu" (
    "id" SERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "role_id" TEXT NOT NULL,
    "menu_id" TEXT NOT NULL,
    "can_view" BOOLEAN NOT NULL DEFAULT true,
    "assigned_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "m0_role_menu_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "m0_permission" (
    "id" SERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "description" TEXT,
    "module" TEXT NOT NULL,
    "action" TEXT NOT NULL,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "m0_permission_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "m0_role_permission" (
    "id" SERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "role_id" TEXT NOT NULL,
    "permission_id" TEXT NOT NULL,
    "assigned_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "m0_role_permission_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "m0_department" (
    "id" SERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "code" TEXT NOT NULL,
    "description" TEXT,
    "parent_id" TEXT,
    "manager_id" TEXT,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "m0_department_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "m0_user_role" (
    "id" SERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "user_id" TEXT NOT NULL,
    "role_id" TEXT NOT NULL,
    "assigned_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "m0_user_role_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "m0_user_department" (
    "id" SERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "user_id" TEXT NOT NULL,
    "department_id" TEXT NOT NULL,
    "joined_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "m0_user_department_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "m1_contact" (
    "id" SERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "tax" TEXT,
    "website" TEXT,
    "address" TEXT,
    "street" TEXT,
    "city" TEXT,
    "province" TEXT,
    "zip_code" TEXT,
    "type" TEXT NOT NULL,
    "contact_first_name" TEXT,
    "contact_email" TEXT,
    "contact_phone" TEXT,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "m1_contact_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "m1_uom" (
    "id" SERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "type" TEXT NOT NULL,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "m1_uom_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "m1_province" (
    "id" SERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "iso_code" TEXT NOT NULL,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "m1_province_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "m1_city" (
    "id" SERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "province_id" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "postal_code" TEXT NOT NULL,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "m1_city_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "m1_warehouse" (
    "id" SERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "city_id" TEXT NOT NULL,
    "location_name" TEXT,
    "address_detail" TEXT,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "m1_warehouse_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "m1_item" (
    "id" SERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "category" TEXT NOT NULL,
    "uom_id" TEXT NOT NULL,
    "item_type" TEXT NOT NULL,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "m1_item_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "m2_do" (
    "id" SERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "report_no" BIGSERIAL NOT NULL,
    "do_number" TEXT NOT NULL,
    "do_date" DATE NOT NULL,
    "do_received_date" DATE NOT NULL,
    "customer_id" TEXT NOT NULL,
    "destination_city_id" TEXT,
    "std_lead_time_days" INTEGER NOT NULL DEFAULT 0,
    "std_return_do_days" INTEGER NOT NULL DEFAULT 0,
    "shipping_date" DATE,
    "actual_received_date" DATE,
    "received_by" TEXT,
    "do_scan_return_date" DATE,
    "standard_received_date" DATE,
    "std_do_return_date" DATE,
    "kpi_delivery_status" TEXT,
    "kpi_do_return_status" TEXT,
    "total_item_types" INTEGER NOT NULL DEFAULT 0,
    "total_batches" INTEGER NOT NULL DEFAULT 0,
    "total_qty_pcs" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "total_kg" DECIMAL(18,3) NOT NULL DEFAULT 0,
    "bu" TEXT,
    "notes" TEXT,
    "status" TEXT NOT NULL DEFAULT 'DRAFT',
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "m2_do_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "m2_do_detail" (
    "id" SERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "do_id" TEXT NOT NULL,
    "line_no" INTEGER NOT NULL,
    "item_id" TEXT NOT NULL,
    "batch_number" TEXT NOT NULL,
    "qty_pcs" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "qty_kg" DECIMAL(18,3) NOT NULL,
    "item_code_snapshot" TEXT,
    "item_name_snapshot" TEXT,
    "uom_code_snapshot" TEXT,
    "notes" TEXT,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "m2_do_detail_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "m0_session" (
    "id" SERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "user_id" TEXT NOT NULL,
    "token" TEXT NOT NULL,
    "expires_at" TIMESTAMP(3) NOT NULL,
    "ip_address" TEXT,
    "user_agent" TEXT,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "m0_session_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "m0_auditlog" (
    "id" SERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "user_id" TEXT,
    "action" TEXT NOT NULL,
    "entity_type" TEXT NOT NULL,
    "entity_id" TEXT,
    "old_data" JSONB,
    "new_data" JSONB,
    "ip_address" TEXT,
    "user_agent" TEXT,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "m0_auditlog_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE UNIQUE INDEX "m0_users_uuid_key" ON "m0_users"("uuid");

-- CreateIndex
CREATE UNIQUE INDEX "m0_users_email_key" ON "m0_users"("email");

-- CreateIndex
CREATE UNIQUE INDEX "m0_users_username_key" ON "m0_users"("username");

-- CreateIndex
CREATE UNIQUE INDEX "m0_role_uuid_key" ON "m0_role"("uuid");

-- CreateIndex
CREATE UNIQUE INDEX "m0_role_name_key" ON "m0_role"("name");

-- CreateIndex
CREATE UNIQUE INDEX "m0_menu_uuid_key" ON "m0_menu"("uuid");

-- CreateIndex
CREATE UNIQUE INDEX "m0_menu_key_key" ON "m0_menu"("key");

-- CreateIndex
CREATE INDEX "m0_menu_parent_id_sort_order_idx" ON "m0_menu"("parent_id", "sort_order");

-- CreateIndex
CREATE UNIQUE INDEX "m0_role_menu_uuid_key" ON "m0_role_menu"("uuid");

-- CreateIndex
CREATE UNIQUE INDEX "m0_role_menu_role_id_menu_id_key" ON "m0_role_menu"("role_id", "menu_id");

-- CreateIndex
CREATE UNIQUE INDEX "m0_permission_uuid_key" ON "m0_permission"("uuid");

-- CreateIndex
CREATE UNIQUE INDEX "m0_permission_name_key" ON "m0_permission"("name");

-- CreateIndex
CREATE UNIQUE INDEX "m0_role_permission_uuid_key" ON "m0_role_permission"("uuid");

-- CreateIndex
CREATE UNIQUE INDEX "m0_role_permission_role_id_permission_id_key" ON "m0_role_permission"("role_id", "permission_id");

-- CreateIndex
CREATE UNIQUE INDEX "m0_department_uuid_key" ON "m0_department"("uuid");

-- CreateIndex
CREATE UNIQUE INDEX "m0_department_code_key" ON "m0_department"("code");

-- CreateIndex
CREATE UNIQUE INDEX "m0_user_role_uuid_key" ON "m0_user_role"("uuid");

-- CreateIndex
CREATE UNIQUE INDEX "m0_user_role_user_id_role_id_key" ON "m0_user_role"("user_id", "role_id");

-- CreateIndex
CREATE UNIQUE INDEX "m0_user_department_uuid_key" ON "m0_user_department"("uuid");

-- CreateIndex
CREATE UNIQUE INDEX "m0_user_department_user_id_department_id_key" ON "m0_user_department"("user_id", "department_id");

-- CreateIndex
CREATE UNIQUE INDEX "m1_contact_uuid_key" ON "m1_contact"("uuid");

-- CreateIndex
CREATE UNIQUE INDEX "m1_contact_code_key" ON "m1_contact"("code");

-- CreateIndex
CREATE UNIQUE INDEX "m1_uom_uuid_key" ON "m1_uom"("uuid");

-- CreateIndex
CREATE UNIQUE INDEX "m1_uom_code_key" ON "m1_uom"("code");

-- CreateIndex
CREATE UNIQUE INDEX "m1_province_uuid_key" ON "m1_province"("uuid");

-- CreateIndex
CREATE UNIQUE INDEX "m1_province_iso_code_key" ON "m1_province"("iso_code");

-- CreateIndex
CREATE UNIQUE INDEX "m1_city_uuid_key" ON "m1_city"("uuid");

-- CreateIndex
CREATE UNIQUE INDEX "m1_warehouse_uuid_key" ON "m1_warehouse"("uuid");

-- CreateIndex
CREATE UNIQUE INDEX "m1_item_uuid_key" ON "m1_item"("uuid");

-- CreateIndex
CREATE UNIQUE INDEX "m1_item_code_key" ON "m1_item"("code");

-- CreateIndex
CREATE UNIQUE INDEX "m2_do_uuid_key" ON "m2_do"("uuid");

-- CreateIndex
CREATE UNIQUE INDEX "m2_do_report_no_key" ON "m2_do"("report_no");

-- CreateIndex
CREATE INDEX "m2_do_customer_id_idx" ON "m2_do"("customer_id");

-- CreateIndex
CREATE INDEX "m2_do_shipping_date_idx" ON "m2_do"("shipping_date");

-- CreateIndex
CREATE INDEX "m2_do_do_received_date_idx" ON "m2_do"("do_received_date");

-- CreateIndex
CREATE UNIQUE INDEX "m2_do_detail_uuid_key" ON "m2_do_detail"("uuid");

-- CreateIndex
CREATE INDEX "m2_do_detail_do_id_idx" ON "m2_do_detail"("do_id");

-- CreateIndex
CREATE INDEX "m2_do_detail_item_id_idx" ON "m2_do_detail"("item_id");

-- CreateIndex
CREATE UNIQUE INDEX "m2_do_detail_do_id_line_no_key" ON "m2_do_detail"("do_id", "line_no");

-- CreateIndex
CREATE UNIQUE INDEX "m0_session_uuid_key" ON "m0_session"("uuid");

-- CreateIndex
CREATE UNIQUE INDEX "m0_session_token_key" ON "m0_session"("token");

-- CreateIndex
CREATE UNIQUE INDEX "m0_auditlog_uuid_key" ON "m0_auditlog"("uuid");

-- AddForeignKey
ALTER TABLE "m0_menu" ADD CONSTRAINT "m0_menu_parent_id_fkey" FOREIGN KEY ("parent_id") REFERENCES "m0_menu"("uuid") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m0_role_menu" ADD CONSTRAINT "m0_role_menu_role_id_fkey" FOREIGN KEY ("role_id") REFERENCES "m0_role"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m0_role_menu" ADD CONSTRAINT "m0_role_menu_menu_id_fkey" FOREIGN KEY ("menu_id") REFERENCES "m0_menu"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m0_role_permission" ADD CONSTRAINT "m0_role_permission_role_id_fkey" FOREIGN KEY ("role_id") REFERENCES "m0_role"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m0_role_permission" ADD CONSTRAINT "m0_role_permission_permission_id_fkey" FOREIGN KEY ("permission_id") REFERENCES "m0_permission"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m0_department" ADD CONSTRAINT "m0_department_parent_id_fkey" FOREIGN KEY ("parent_id") REFERENCES "m0_department"("uuid") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m0_user_role" ADD CONSTRAINT "m0_user_role_user_id_fkey" FOREIGN KEY ("user_id") REFERENCES "m0_users"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m0_user_role" ADD CONSTRAINT "m0_user_role_role_id_fkey" FOREIGN KEY ("role_id") REFERENCES "m0_role"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m0_user_department" ADD CONSTRAINT "m0_user_department_user_id_fkey" FOREIGN KEY ("user_id") REFERENCES "m0_users"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m0_user_department" ADD CONSTRAINT "m0_user_department_department_id_fkey" FOREIGN KEY ("department_id") REFERENCES "m0_department"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m1_city" ADD CONSTRAINT "m1_city_province_id_fkey" FOREIGN KEY ("province_id") REFERENCES "m1_province"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m1_warehouse" ADD CONSTRAINT "m1_warehouse_city_id_fkey" FOREIGN KEY ("city_id") REFERENCES "m1_city"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m1_item" ADD CONSTRAINT "m1_item_uom_id_fkey" FOREIGN KEY ("uom_id") REFERENCES "m1_uom"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m2_do" ADD CONSTRAINT "m2_do_customer_id_fkey" FOREIGN KEY ("customer_id") REFERENCES "m1_contact"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m2_do" ADD CONSTRAINT "m2_do_destination_city_id_fkey" FOREIGN KEY ("destination_city_id") REFERENCES "m1_city"("uuid") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m2_do_detail" ADD CONSTRAINT "m2_do_detail_do_id_fkey" FOREIGN KEY ("do_id") REFERENCES "m2_do"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m2_do_detail" ADD CONSTRAINT "m2_do_detail_item_id_fkey" FOREIGN KEY ("item_id") REFERENCES "m1_item"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m0_session" ADD CONSTRAINT "m0_session_user_id_fkey" FOREIGN KEY ("user_id") REFERENCES "m0_users"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m0_auditlog" ADD CONSTRAINT "m0_auditlog_user_id_fkey" FOREIGN KEY ("user_id") REFERENCES "m0_users"("uuid") ON DELETE SET NULL ON UPDATE CASCADE;


-- Manual constraints and behavior for Delivery Order reporting
CREATE UNIQUE INDEX IF NOT EXISTS "ux_m2_do_number_active"
  ON "m2_do"("do_number")
  WHERE "deleted_at" IS NULL;

CREATE UNIQUE INDEX IF NOT EXISTS "ux_m2_do_detail_do_item_batch_active"
  ON "m2_do_detail"("do_id", "item_id", "batch_number")
  WHERE "deleted_at" IS NULL;

ALTER TABLE "m2_do"
  ADD CONSTRAINT "chk_m2_do_std_lead_time_non_negative" CHECK ("std_lead_time_days" >= 0),
  ADD CONSTRAINT "chk_m2_do_std_return_non_negative" CHECK ("std_return_do_days" >= 0),
  ADD CONSTRAINT "chk_m2_do_status" CHECK ("status" IN ('DRAFT', 'SHIPPED', 'RECEIVED', 'CLOSED', 'CANCELLED')),
  ADD CONSTRAINT "chk_m2_do_kpi_delivery" CHECK ("kpi_delivery_status" IS NULL OR "kpi_delivery_status" IN ('ONTIME', 'LATE')),
  ADD CONSTRAINT "chk_m2_do_kpi_return" CHECK ("kpi_do_return_status" IS NULL OR "kpi_do_return_status" IN ('ONTIME', 'LATE'));

ALTER TABLE "m2_do_detail"
  ADD CONSTRAINT "chk_m2_do_detail_qty_kg_positive" CHECK ("qty_kg" > 0),
  ADD CONSTRAINT "chk_m2_do_detail_qty_pcs_non_negative" CHECK ("qty_pcs" >= 0);

CREATE OR REPLACE FUNCTION fn_m2_do_derive_fields()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
  IF NEW.shipping_date IS NOT NULL THEN
    NEW.standard_received_date := NEW.shipping_date + NEW.std_lead_time_days;
    NEW.std_do_return_date := NEW.shipping_date + NEW.std_return_do_days;
  ELSE
    NEW.standard_received_date := NULL;
    NEW.std_do_return_date := NULL;
  END IF;

  IF NEW.actual_received_date IS NULL OR NEW.standard_received_date IS NULL THEN
    NEW.kpi_delivery_status := NULL;
  ELSIF NEW.actual_received_date <= NEW.standard_received_date THEN
    NEW.kpi_delivery_status := 'ONTIME';
  ELSE
    NEW.kpi_delivery_status := 'LATE';
  END IF;

  IF NEW.do_scan_return_date IS NULL OR NEW.std_do_return_date IS NULL THEN
    NEW.kpi_do_return_status := NULL;
  ELSIF NEW.do_scan_return_date <= NEW.std_do_return_date THEN
    NEW.kpi_do_return_status := 'ONTIME';
  ELSE
    NEW.kpi_do_return_status := 'LATE';
  END IF;

  NEW.updated_at := CURRENT_TIMESTAMP;
  RETURN NEW;
END;
$$;

CREATE TRIGGER trg_m2_do_derive_fields
BEFORE INSERT OR UPDATE ON "m2_do"
FOR EACH ROW
EXECUTE FUNCTION fn_m2_do_derive_fields();

CREATE OR REPLACE FUNCTION fn_m2_do_recalc_totals(p_do_id TEXT)
RETURNS VOID
LANGUAGE plpgsql
AS $$
DECLARE
  v_total_item_types INTEGER := 0;
  v_total_batches INTEGER := 0;
  v_total_qty_pcs NUMERIC(18,2) := 0;
  v_total_kg NUMERIC(18,3) := 0;
BEGIN
  SELECT
    COALESCE(COUNT(DISTINCT d.item_id), 0),
    COALESCE(COUNT(*), 0),
    COALESCE(SUM(d.qty_pcs), 0),
    COALESCE(SUM(d.qty_kg), 0)
  INTO v_total_item_types, v_total_batches, v_total_qty_pcs, v_total_kg
  FROM "m2_do_detail" d
  WHERE d.do_id = p_do_id
    AND d.deleted_at IS NULL;

  UPDATE "m2_do"
  SET
    total_item_types = v_total_item_types,
    total_batches = v_total_batches,
    total_qty_pcs = v_total_qty_pcs,
    total_kg = v_total_kg,
    updated_at = CURRENT_TIMESTAMP
  WHERE uuid = p_do_id;
END;
$$;

CREATE OR REPLACE FUNCTION trg_m2_do_detail_recalc()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
  IF TG_OP = 'UPDATE' THEN
    IF NEW.do_id IS DISTINCT FROM OLD.do_id THEN
      PERFORM fn_m2_do_recalc_totals(OLD.do_id);
      PERFORM fn_m2_do_recalc_totals(NEW.do_id);
    ELSE
      PERFORM fn_m2_do_recalc_totals(NEW.do_id);
    END IF;
  ELSIF TG_OP = 'DELETE' THEN
    PERFORM fn_m2_do_recalc_totals(OLD.do_id);
  ELSE
    PERFORM fn_m2_do_recalc_totals(NEW.do_id);
  END IF;

  RETURN COALESCE(NEW, OLD);
END;
$$;

CREATE TRIGGER trg_m2_do_detail_aiud_recalc
AFTER INSERT OR UPDATE OR DELETE ON "m2_do_detail"
FOR EACH ROW
EXECUTE FUNCTION trg_m2_do_detail_recalc();
