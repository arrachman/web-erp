-- A2 Order Hub: channel + external id + funding source on sales orders,
-- derived-stage audit log, and the Order Hub menu.
--
-- Orders gain: sales channel (SIPLah / sales / admin / portals), an
-- external order id (idempotency key for channel imports), BOS funding
-- source + budget year/stage, a custom-print production flag, and SIPLah
-- reconciliation fields (marketplace fee, disbursement reference).
-- `sls_order_status_logs` stores the observed transitions of the derived
-- hub stage (BARU -> DIKONFIRMASI -> DISIAPKAN -> DIKIRIM -> DITERIMA ->
-- DITAGIH -> LUNAS) as the status-change audit trail.

ALTER TYPE "ErpSalesChannel" ADD VALUE IF NOT EXISTS 'SIPLAH';
ALTER TYPE "ErpSalesChannel" ADD VALUE IF NOT EXISTS 'SALES';
ALTER TYPE "ErpSalesChannel" ADD VALUE IF NOT EXISTS 'ADMIN';
ALTER TYPE "ErpSalesChannel" ADD VALUE IF NOT EXISTS 'PORTAL_SEKOLAH';
ALTER TYPE "ErpSalesChannel" ADD VALUE IF NOT EXISTS 'PORTAL_ORANGTUA';

CREATE TYPE "ErpFundingSource" AS ENUM ('BOS', 'NON_BOS');

CREATE TYPE "ErpOrderHubStatus" AS ENUM (
  'BARU', 'DIKONFIRMASI', 'DISIAPKAN', 'DIKIRIM', 'DITERIMA', 'DITAGIH', 'LUNAS'
);

ALTER TABLE "sls_orders"
  ADD COLUMN "channel" "ErpSalesChannel" NOT NULL DEFAULT 'STANDARD',
  ADD COLUMN "external_order_id" TEXT,
  ADD COLUMN "funding_source" "ErpFundingSource",
  ADD COLUMN "budget_year" INTEGER,
  ADD COLUMN "budget_stage" INTEGER,
  ADD COLUMN "needs_production" BOOLEAN NOT NULL DEFAULT false,
  ADD COLUMN "marketplace_fee" DECIMAL(19, 4),
  ADD COLUMN "disbursement_ref" TEXT;

CREATE INDEX "sls_orders_channel_external_order_id_idx"
  ON "sls_orders" ("channel", "external_order_id");

CREATE UNIQUE INDEX "sls_orders_channel_external_order_id_key"
  ON "sls_orders" ("channel", "external_order_id")
  WHERE "external_order_id" IS NOT NULL AND "deleted_at" IS NULL;

CREATE TABLE "sls_order_status_logs" (
  "id" BIGSERIAL NOT NULL,
  "order_id" BIGINT NOT NULL,
  "hub_status" "ErpOrderHubStatus" NOT NULL,
  "source" TEXT NOT NULL DEFAULT 'DERIVED',
  "note" TEXT,
  "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "created_by_id" BIGINT,

  CONSTRAINT "sls_order_status_logs_pkey" PRIMARY KEY ("id")
);

CREATE INDEX "sls_order_status_logs_order_id_idx"
  ON "sls_order_status_logs" ("order_id");

ALTER TABLE "sls_order_status_logs"
  ADD CONSTRAINT "sls_order_status_logs_order_id_fkey"
  FOREIGN KEY ("order_id") REFERENCES "sls_orders" ("id")
  ON DELETE RESTRICT ON UPDATE CASCADE;

-- Menu: Sales -> Transactions -> Order Hub (queue across channels).
INSERT INTO "sys_menus"
  ("code", "title", "path", "icon", "type", "parent_id", "sort_order",
   "is_active", "created_at", "updated_at")
SELECT 'M5.TX.HUB', 'Order Hub', '/sales/order-hub', NULL, 'ITEM',
       (SELECT "id" FROM "sys_menus" WHERE "code" = 'M5.TX'),
       0, true, NOW(), NOW()
WHERE NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M5.TX.HUB');

-- Role grants: copy every grant on the Sales Order menu to the Order Hub menu.
INSERT INTO "adm_role_menus"
  ("role_id", "menu_id", "can_view", "can_create", "can_edit", "can_delete",
   "can_approve", "can_print", "can_export", "can_import", "is_favorite")
SELECT rm."role_id",
       (SELECT "id" FROM "sys_menus" WHERE "code" = 'M5.TX.HUB'),
       rm."can_view", rm."can_create", rm."can_edit", rm."can_delete",
       rm."can_approve", rm."can_print", rm."can_export", rm."can_import",
       rm."is_favorite"
FROM "adm_role_menus" rm
WHERE rm."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'M5.TX.SO')
  AND NOT EXISTS (
    SELECT 1 FROM "adm_role_menus" x
    WHERE x."role_id" = rm."role_id"
      AND x."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'M5.TX.HUB')
  );
