-- ErpItemType — switch value set from composition-focused to stock/asset-nature.
-- Old: INVENTORY/SERVICE/VOUCHER/ASSEMBLY  ->  New: INVENTORY/SERVICE/CONSUMABLE/ASSET/NON_INVENTORY
-- Rationale: 3-axis item classification (type=system behavior, kind=business role,
-- category=material). "Assembly/BOM" is a separate fact (mfg_boms), "voucher" = NON_INVENTORY.
-- Postgres cannot drop enum values in place, so the type is recreated.
-- Backfill of existing rows: VOUCHER -> NON_INVENTORY, ASSEMBLY -> INVENTORY.
-- Only column md_items.type uses this enum (no DEFAULT to preserve).

ALTER TYPE "ErpItemType" RENAME TO "ErpItemType_old";

CREATE TYPE "ErpItemType" AS ENUM ('INVENTORY', 'SERVICE', 'CONSUMABLE', 'ASSET', 'NON_INVENTORY');

ALTER TABLE "md_items"
  ALTER COLUMN "type" TYPE "ErpItemType"
  USING (
    CASE "type"::text
      WHEN 'VOUCHER' THEN 'NON_INVENTORY'
      WHEN 'ASSEMBLY' THEN 'INVENTORY'
      ELSE "type"::text
    END::"ErpItemType"
  );

DROP TYPE "ErpItemType_old";
