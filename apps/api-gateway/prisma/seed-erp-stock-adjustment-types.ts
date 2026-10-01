/**
 * Seed 100 realistic stock adjustment types into md_stock_adjustment_types.
 * Run: npm run db:seed:stock-adjustment-types
 * Idempotent: skips if table already has >=1 row.
 */
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

type TxnType = { code: string; name: string; direction: 'IN' | 'OUT' | 'TRANSFER'; legacyCode?: string };

const ITEM_TXN_TYPES: TxnType[] = [
  // ── IN: Penerimaan Bahan Baku ──────────────────────────────────────────────
  { code: 'GR-PO',     name: 'Goods Receipt from Purchase Order',    direction: 'IN',  legacyCode: 'GR' },
  { code: 'GR-RETURN', name: 'Goods Receipt from Supplier Return',   direction: 'IN',  legacyCode: 'GRR' },
  { code: 'GR-CONSIGN',name: 'Goods Receipt Consignment',            direction: 'IN',  legacyCode: 'GRC' },
  { code: 'ADJ-IN',    name: 'Stock Adjustment — Surplus',           direction: 'IN',  legacyCode: 'ADJ+' },
  { code: 'PROD-OUTPUT',name: 'Production Output / Finished Goods',  direction: 'IN',  legacyCode: 'PRD' },
  { code: 'MFG-RETURN', name: 'Manufacturing Return to Warehouse',   direction: 'IN',  legacyCode: 'MFR' },
  { code: 'REPROCESS',  name: 'Re-processed Goods Receipt',          direction: 'IN',  legacyCode: 'REP' },
  { code: 'SAMPLING-RETURN', name: 'Sample Return to Stock',         direction: 'IN',  legacyCode: 'SMP' },
  { code: 'WH-RETURN',  name: 'Warehouse Return Receipt',            direction: 'IN',  legacyCode: 'WHR' },
  { code: 'IMPORT-GR',  name: 'Import Goods Receipt',                direction: 'IN',  legacyCode: 'IGR' },
  { code: 'DONATION-IN',name: 'Donation Received',                   direction: 'IN' },
  { code: 'OPENING-STK',name: 'Opening Stock Entry',                 direction: 'IN',  legacyCode: 'OS' },
  { code: 'FOUND',      name: 'Goods Found (Unrecorded)',            direction: 'IN',  legacyCode: 'FND' },
  { code: 'ASSEMBLY-IN',name: 'Assembly Completed — Receipt',        direction: 'IN',  legacyCode: 'ASM' },
  { code: 'SUBCON-GR',  name: 'Sub-contract Goods Receipt',          direction: 'IN',  legacyCode: 'SGR' },
  { code: 'REPAIR-IN',  name: 'Repaired Goods Return',               direction: 'IN',  legacyCode: 'RPR' },
  { code: 'POS-RETURN', name: 'POS Sales Return to Stock',           direction: 'IN',  legacyCode: 'PSR' },
  { code: 'CUST-RETURN',name: 'Customer Return',                     direction: 'IN',  legacyCode: 'CR' },
  { code: 'INTER-CO-IN',name: 'Inter-company Receipt',               direction: 'IN' },
  { code: 'PACKING-IN', name: 'Packing Material Receipt',            direction: 'IN' },
  { code: 'RECYCLE-IN', name: 'Recycled Material Receipt',           direction: 'IN' },
  { code: 'BY-PRODUCT',  name: 'By-product from Production',         direction: 'IN' },
  { code: 'REWORK-IN',   name: 'Rework Goods Received Back',         direction: 'IN' },
  { code: 'GR-MANUAL',   name: 'Manual Goods Receipt',               direction: 'IN',  legacyCode: 'MGR' },
  { code: 'PHYSICAL-SURPLUS', name: 'Physical Count Surplus',        direction: 'IN',  legacyCode: 'PCS' },

  // ── OUT: Pengeluaran ───────────────────────────────────────────────────────
  { code: 'DO-SALES',   name: 'Delivery Order — Sales',              direction: 'OUT', legacyCode: 'DO' },
  { code: 'DO-SAMPLE',  name: 'Sample Delivery',                     direction: 'OUT', legacyCode: 'DOS' },
  { code: 'DO-CONSIGN', name: 'Consignment Delivery',                direction: 'OUT', legacyCode: 'DOC' },
  { code: 'ADJ-OUT',    name: 'Stock Adjustment — Deficit',          direction: 'OUT', legacyCode: 'ADJ-' },
  { code: 'PROD-ISSUE', name: 'Production Issue — Raw Material',     direction: 'OUT', legacyCode: 'ISS' },
  { code: 'SCRAP',      name: 'Scrap / Scrapping',                   direction: 'OUT', legacyCode: 'SCR' },
  { code: 'DAMAGED',    name: 'Goods Damaged Write-off',             direction: 'OUT', legacyCode: 'DMG' },
  { code: 'EXPIRED',    name: 'Expired Goods Write-off',             direction: 'OUT', legacyCode: 'EXP' },
  { code: 'THEFT',      name: 'Goods Lost / Theft Write-off',        direction: 'OUT', legacyCode: 'LST' },
  { code: 'SUPP-RETURN',name: 'Return to Supplier',                  direction: 'OUT', legacyCode: 'RT' },
  { code: 'LOAN-OUT',   name: 'Goods on Loan to External Party',     direction: 'OUT' },
  { code: 'DEMO-OUT',   name: 'Goods for Demo / Exhibition',         direction: 'OUT' },
  { code: 'DONATION-OUT',name: 'Goods Donated',                      direction: 'OUT' },
  { code: 'DESTROY',    name: 'Goods Destroyed / Incinerated',       direction: 'OUT' },
  { code: 'TEST-ISSUE', name: 'Goods Issued for Testing',            direction: 'OUT', legacyCode: 'TST' },
  { code: 'ASSET-XFER', name: 'Goods Transferred to Fixed Assets',   direction: 'OUT' },
  { code: 'MAINT-ISSUE',name: 'Goods Issued for Maintenance',        direction: 'OUT', legacyCode: 'MNT' },
  { code: 'POS-SALE',   name: 'POS Direct Sale',                     direction: 'OUT', legacyCode: 'POS' },
  { code: 'EXPORT-DO',  name: 'Export Delivery',                     direction: 'OUT', legacyCode: 'EXD' },
  { code: 'SUBCON-ISS', name: 'Issue to Sub-contractor',             direction: 'OUT', legacyCode: 'SCI' },
  { code: 'PACK-ISSUE', name: 'Packaging Material Issued',           direction: 'OUT' },
  { code: 'INTER-CO-OUT',name: 'Inter-company Delivery',             direction: 'OUT' },
  { code: 'PHYSICAL-DEFICIT', name: 'Physical Count Deficit',        direction: 'OUT', legacyCode: 'PCD' },
  { code: 'RECYCLE-OUT',name: 'Goods Sent for Recycling',            direction: 'OUT' },
  { code: 'QUARANTINE', name: 'Goods Quarantined / Blocked',         direction: 'OUT', legacyCode: 'QTN' },

  // ── TRANSFER: Mutasi Antar Lokasi / Gudang ───────────────────────��─────────
  { code: 'STR-WH',    name: 'Stock Transfer Between Warehouses',    direction: 'TRANSFER', legacyCode: 'STW' },
  { code: 'STR-LOC',   name: 'Stock Transfer Between Locations',     direction: 'TRANSFER', legacyCode: 'STL' },
  { code: 'STR-BRANCH',name: 'Stock Transfer Between Branches',      direction: 'TRANSFER', legacyCode: 'STB' },
  { code: 'STR-FLOOR', name: 'Transfer to Production Floor',         direction: 'TRANSFER', legacyCode: 'STF' },
  { code: 'STR-FG',    name: 'Transfer to Finished Goods Area',      direction: 'TRANSFER', legacyCode: 'TFG' },
  { code: 'STR-HOLD',  name: 'Transfer to Quality Hold Area',        direction: 'TRANSFER', legacyCode: 'QHL' },
  { code: 'STR-RELEASE',name: 'Release from Quality Hold',           direction: 'TRANSFER', legacyCode: 'QRL' },
  { code: 'STR-DAMAGE',name: 'Transfer to Damage Area',              direction: 'TRANSFER', legacyCode: 'DMV' },
  { code: 'STR-REPAIR',name: 'Transfer to Repair Area',              direction: 'TRANSFER', legacyCode: 'RPV' },
  { code: 'STR-STAGE', name: 'Transfer to Staging Area',             direction: 'TRANSFER', legacyCode: 'STG' },
  { code: 'STR-BIN',   name: 'Bin-to-Bin Transfer',                  direction: 'TRANSFER', legacyCode: 'BIN' },
  { code: 'STR-PACK',  name: 'Transfer to Packing Station',          direction: 'TRANSFER' },
  { code: 'STR-COLD',  name: 'Transfer to Cold Storage',             direction: 'TRANSFER' },
  { code: 'STR-BULK',  name: 'Bulk to Unit Conversion Transfer',     direction: 'TRANSFER' },
  { code: 'STR-REPACK',name: 'Repacking Transfer',                   direction: 'TRANSFER' },
  { code: 'STR-CONSIGN-BACK', name: 'Consignment Stock Return Transfer', direction: 'TRANSFER' },
  { code: 'STR-3PL',   name: 'Transfer to 3PL Warehouse',            direction: 'TRANSFER' },
  { code: 'STR-HAZMAT',name: 'Transfer to Hazmat Storage',           direction: 'TRANSFER' },
  { code: 'STR-SLOW',  name: 'Transfer Slow-moving to Archive',      direction: 'TRANSFER' },
  { code: 'STR-URGENT',name: 'Emergency Transfer for Urgent Order',  direction: 'TRANSFER' },
  { code: 'STR-SHOW',  name: 'Transfer to Showroom / Display',       direction: 'TRANSFER' },
  { code: 'STR-RECYCLE-CTR', name: 'Transfer to Recycling Center',   direction: 'TRANSFER' },
  { code: 'STR-SUBCON',name: 'Transfer to Sub-contractor Site',      direction: 'TRANSFER', legacyCode: 'TSC' },
  { code: 'STR-KIOSK', name: 'Transfer to Kiosk / POS Point',        direction: 'TRANSFER' },
  { code: 'STR-BONDED',name: 'Transfer to Bonded Warehouse',         direction: 'TRANSFER', legacyCode: 'TBW' },

  // ── Campuran / Khusus ───────────────────────────────��──────────────────────
  { code: 'REVALUE',   name: 'Stock Revaluation Adjustment',         direction: 'IN',  legacyCode: 'RVL' },
  { code: 'CYCLE-CNT', name: 'Cycle Count Correction',               direction: 'IN',  legacyCode: 'CYC' },
  { code: 'KITTING',   name: 'Kit Assembly Issue',                   direction: 'OUT', legacyCode: 'KIT' },
  { code: 'DEKIT',     name: 'Kit Disassembly Return',               direction: 'IN',  legacyCode: 'DKT' },
  { code: 'MERGE',     name: 'Lot Merge',                            direction: 'TRANSFER' },
  { code: 'SPLIT',     name: 'Lot Split',                            direction: 'TRANSFER' },
];

async function main() {
  const count = await prisma.erpStockAdjustmentType.count({ where: { deletedAt: null } });
  if (count > 0) {
    console.log(`md_stock_adjustment_types already has ${count} rows — skip.`);
    return;
  }
  console.log(`Inserting ${ITEM_TXN_TYPES.length} stock adjustment types…`);
  const result = await prisma.erpStockAdjustmentType.createMany({
    data: ITEM_TXN_TYPES.map(t => ({
      code: t.code,
      name: t.name,
      direction: t.direction,
      legacyCode: t.legacyCode ?? null,
      isActive: true,
    })),
    skipDuplicates: true,
  });
  console.log(`Done. Inserted ${result.count} rows.`);
}

main()
  .catch(err => { console.error(err); process.exit(1); })
  .finally(() => prisma.$disconnect());
