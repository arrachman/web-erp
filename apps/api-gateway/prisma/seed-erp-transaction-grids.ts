/**
 * Seed for Kustomisasi Grid: the transaction-type catalog (left module→transaction
 * tree) + default grid columns for the finance transaction families which the live
 * grid reads. Idempotent (upsert by code / by [grid, dataField]).
 *
 * Families covered (finance):
 * - cashbank     → fin_cash_bank_lines  (CR/CD/RM/SM)  — single "Total" contra lines
 * - journal      → fin_journal_lines    (GJ/AJ/JM/BB/RV) — Akun · Debit · Kredit
 * - giro         → fin_giros            (RG/SG)        — instrument: No Giro/Cek …
 * - giroClearing → fin_giros            (RGC/SGC)      — instrument + Tgl Cair · Akun Bank
 *
 * Sales (M5):
 * - salesItem    → sls_*_lines (per-txn override: SQ/SO/PI/PL/DO/DR/SI/RNR/SR) — item
 *   + qty/price/disc/tax + derived Total. Each sales doc owns its own line table.
 *
 * Purchasing (M4):
 * - purchaseItem    → pur_*_lines (PR/PO/PI/DNR/PRT) — item + qty/cost/disc/tax + Total.
 * - purchaseReceipt → pur_goods_receipt_lines (GRN) — item + QC delta (accepted/rejected/quarantine).
 * - purchaseRfq     → pur_rfq_suppliers (RFQ) — invited-supplier lines (not items).
 * - purchaseBid     → pur_bid_selection_lines (BS) — quotation comparison (rank/selected).
 *   Each purchasing doc owns its own line table (per-txn `lineTable` override).
 *
 * Column defaults exist so the Grid Customization screen (and a future config-driven
 * detail grid) opens with a sensible curated layout instead of an empty/lazy grid.
 * `update: {}` on columns keeps admin edits on re-seed (only missing defaults created).
 */

import { PrismaClient } from '@prisma/client';

interface ModuleDef { key: string; label: string; sortOrder: number }

const MODULES: ModuleDef[] = [
  { key: 'finance', label: 'Finance & Accounting', sortOrder: 1 },
  { key: 'inventory', label: 'Inventory & Warehouse', sortOrder: 2 },
  { key: 'purchasing', label: 'Purchasing', sortOrder: 3 },
  { key: 'sales', label: 'Sales & Distribution', sortOrder: 4 },
  { key: 'production', label: 'Production', sortOrder: 5 },
  { key: 'fixed-asset', label: 'Fixed Asset', sortOrder: 6 },
  { key: 'administrator', label: 'Administrator', sortOrder: 7 },
  { key: 'data-master', label: 'Data Master', sortOrder: 8 },
  { key: 'business-intelligence', label: 'Business Intelligence', sortOrder: 9 },
  { key: 'consolidation', label: 'Consolidation', sortOrder: 10 },
  { key: 'hrd-payroll', label: 'HRD & Payroll', sortOrder: 11 },
  { key: 'hospital', label: 'Hospital', sortOrder: 12 },
  { key: 'point-of-sales', label: 'Point of Sales', sortOrder: 13 },
  { key: 'academic', label: 'Academic', sortOrder: 14 },
  { key: 'cooperative', label: 'Cooperative', sortOrder: 15 },
];

// Grid families = the physical line table + default column set a transaction uses.
type GridFamily =
  | 'cashbank' | 'journal' | 'giro' | 'giroClearing'
  // Inventory (M3) — item-line families mapping to the inv_* line tables.
  | 'invMoveTransfer' | 'invMoveRequest' | 'invFuel'
  | 'invOpening' | 'invCount' | 'invAdjustment' | 'invCostRecalc'
  | 'invDailyCheck'
  // Sales (M5) — item-line family. Unlike finance/inventory families that share a
  // line table, each sales doc has its OWN line table → per-txn `lineTable` override.
  | 'salesItem'
  // Purchasing (M4) — like sales, each pur doc owns its own line table (per-txn
  // `lineTable` override). `purchaseItem` = standard item lines (PR/PO/PI/returns);
  // `purchaseReceipt` = GRN item lines + QC delta (accepted/rejected/quarantine);
  // `purchaseRfq` = invited-supplier lines (pur_rfq_suppliers, not items);
  // `purchaseBid` = bid-selection lines (rank/selected).
  | 'purchaseItem' | 'purchaseReceipt' | 'purchaseRfq' | 'purchaseBid';

const LINE_TABLE_BY_FAMILY: Record<GridFamily, string> = {
  cashbank: 'fin_cash_bank_lines',
  journal: 'fin_journal_lines',
  giro: 'fin_giros',
  giroClearing: 'fin_giros',
  invMoveTransfer: 'inv_stock_movement_lines',
  invMoveRequest: 'inv_stock_movement_lines',
  invFuel: 'inv_stock_movement_lines',
  invOpening: 'inv_opening_stock_lines',
  invCount: 'inv_stock_count_lines',
  invAdjustment: 'inv_stock_adjustment_lines',
  invCostRecalc: 'inv_cost_recalculation_lines',
  invDailyCheck: 'inv_daily_check_lines',
  // Fallback only — each salesItem txn ALWAYS sets an explicit `lineTable` override
  // (sales docs don't share a single line table).
  salesItem: 'sls_order_lines',
  // Fallback only — each purchasing txn sets an explicit `lineTable` override
  // (purchasing docs don't share a single line table).
  purchaseItem: 'pur_order_lines',
  purchaseReceipt: 'pur_goods_receipt_lines',
  purchaseRfq: 'pur_rfq_suppliers',
  purchaseBid: 'pur_bid_selection_lines',
};

// `lineTable` overrides LINE_TABLE_BY_FAMILY[family] when present (sales docs each
// own their own line table — finance/inventory families share one per family).
interface TxnDef { code: string; name: string; moduleKey: string; group: string; grid?: GridFamily; lineTable?: string }

const TXNS: TxnDef[] = [
  // Finance — Transaction
  { code: 'FIN.CR', name: 'Cash Receipt (CR)', moduleKey: 'finance', group: 'Transaction', grid: 'cashbank' },
  { code: 'FIN.CD', name: 'Cash Disbursement (CD)', moduleKey: 'finance', group: 'Transaction', grid: 'cashbank' },
  { code: 'FIN.RM', name: 'Bank Receipt (RM)', moduleKey: 'finance', group: 'Transaction', grid: 'cashbank' },
  { code: 'FIN.SM', name: 'Bank Payment (SM)', moduleKey: 'finance', group: 'Transaction', grid: 'cashbank' },
  { code: 'FIN.GJ', name: 'General Journal (GJ)', moduleKey: 'finance', group: 'Transaction', grid: 'journal' },
  { code: 'FIN.AJ', name: 'Adjustment Journal (AJ)', moduleKey: 'finance', group: 'Transaction', grid: 'journal' },
  { code: 'FIN.JM', name: 'Journal Memorial (JM)', moduleKey: 'finance', group: 'Transaction', grid: 'journal' },
  { code: 'FIN.RG', name: 'Receipt Giro (RG)', moduleKey: 'finance', group: 'Transaction', grid: 'giro' },
  { code: 'FIN.SG', name: 'Send Giro (SG)', moduleKey: 'finance', group: 'Transaction', grid: 'giro' },
  { code: 'FIN.RGC', name: 'Receipt Giro Clearing (RGC)', moduleKey: 'finance', group: 'Transaction', grid: 'giroClearing' },
  { code: 'FIN.SGC', name: 'Send Giro Clearing (SGC)', moduleKey: 'finance', group: 'Transaction', grid: 'giroClearing' },
  { code: 'FIN.RV', name: 'Revaluasi Valas (RV)', moduleKey: 'finance', group: 'Transaction', grid: 'journal' },
  { code: 'FIN.BB', name: 'Opening Balance (CoA)', moduleKey: 'finance', group: 'Transaction', grid: 'journal' },
  // Inventory — M3 Warehouse & Inventory (10 transaksi, paritas menu M3.TX)
  { code: 'INV.MR', name: 'Material Request (MR)', moduleKey: 'inventory', group: 'Transaction', grid: 'invMoveRequest' },
  { code: 'INV.TS', name: 'Stock Transfer (TS)', moduleKey: 'inventory', group: 'Transaction', grid: 'invMoveTransfer' },
  { code: 'INV.RS', name: 'Transfer Receipt (RS)', moduleKey: 'inventory', group: 'Transaction', grid: 'invMoveTransfer' },
  { code: 'INV.SP', name: 'Stock Count (SP)', moduleKey: 'inventory', group: 'Transaction', grid: 'invCount' },
  { code: 'INV.SA', name: 'Stock Adjustment (SA)', moduleKey: 'inventory', group: 'Transaction', grid: 'invAdjustment' },
  { code: 'INV.PA', name: 'Price Adjustment (PA)', moduleKey: 'inventory', group: 'Transaction', grid: 'invCostRecalc' },
  { code: 'INV.IB', name: 'Opening Stock (IB)', moduleKey: 'inventory', group: 'Transaction', grid: 'invOpening' },
  { code: 'INV.RF', name: 'Fuel Refill (RF)', moduleKey: 'inventory', group: 'Transaction', grid: 'invFuel' },
  { code: 'INV.DC', name: 'Time Sheet/Daily Check (DC)', moduleKey: 'inventory', group: 'Transaction', grid: 'invDailyCheck' },
  { code: 'INV.RW', name: 'Receipt Weigher (RW)', moduleKey: 'inventory', group: 'Transaction' },
  // Purchasing (M4) — full 13-transaction set (paritas menu M4.TX). Item-based docs
  // (purchase* families) each own their line table via explicit `lineTable`;
  // payment/opening txns (AP/PP/VPP/VP/OB reuse the finance domain) omit a grid.
  { code: 'PUR.PR', name: 'Purchase Requisition (PR)', moduleKey: 'purchasing', group: 'Transaction', grid: 'purchaseItem', lineTable: 'pur_requisition_lines' },
  { code: 'PUR.RFQ', name: 'Request for Quotation (RFQ)', moduleKey: 'purchasing', group: 'Transaction', grid: 'purchaseRfq', lineTable: 'pur_rfq_suppliers' },
  { code: 'PUR.BS', name: 'Bid Comparison (BS)', moduleKey: 'purchasing', group: 'Transaction', grid: 'purchaseBid', lineTable: 'pur_bid_selection_lines' },
  { code: 'PUR.PO', name: 'Purchase Order (PO)', moduleKey: 'purchasing', group: 'Transaction', grid: 'purchaseItem', lineTable: 'pur_order_lines' },
  { code: 'PUR.AP', name: 'Vendor Advance (AP)', moduleKey: 'purchasing', group: 'Transaction' },
  { code: 'PUR.GRN', name: 'Goods Receipt (GRN)', moduleKey: 'purchasing', group: 'Transaction', grid: 'purchaseReceipt', lineTable: 'pur_goods_receipt_lines' },
  { code: 'PUR.PI', name: 'Purchase Invoice (PI)', moduleKey: 'purchasing', group: 'Transaction', grid: 'purchaseItem', lineTable: 'pur_invoice_lines' },
  { code: 'PUR.PP', name: 'Freight Payable (PP)', moduleKey: 'purchasing', group: 'Transaction' },
  { code: 'PUR.DNR', name: 'Return Shipment (DNR)', moduleKey: 'purchasing', group: 'Transaction', grid: 'purchaseItem', lineTable: 'pur_return_lines' },
  { code: 'PUR.PRT', name: 'Purchase Return (PRT)', moduleKey: 'purchasing', group: 'Transaction', grid: 'purchaseItem', lineTable: 'pur_return_lines' },
  { code: 'PUR.VPP', name: 'Payment Schedule (VPP)', moduleKey: 'purchasing', group: 'Transaction' },
  { code: 'PUR.VP', name: 'Vendor Payment (VP)', moduleKey: 'purchasing', group: 'Transaction' },
  { code: 'PUR.OB', name: 'Opening AP Balance', moduleKey: 'purchasing', group: 'Transaction' },
  // Sales (M5) — full 16-transaction set. Item-based docs (salesItem family) each
  // own their line table via explicit `lineTable`; document-only/payment txns omit grid.
  { code: 'SLS.SQ', name: 'Sales Quotation (SQ)', moduleKey: 'sales', group: 'Transaction', grid: 'salesItem', lineTable: 'sls_quotation_lines' },
  { code: 'SLS.SO', name: 'Sales Order (SO)', moduleKey: 'sales', group: 'Transaction', grid: 'salesItem', lineTable: 'sls_order_lines' },
  { code: 'SLS.AS', name: 'Customer Advance (AS)', moduleKey: 'sales', group: 'Transaction' },
  { code: 'SLS.IP', name: 'Payment Receipt (IP)', moduleKey: 'sales', group: 'Transaction' },
  { code: 'SLS.PI', name: 'Proforma Invoice (PI)', moduleKey: 'sales', group: 'Transaction', grid: 'salesItem', lineTable: 'sls_proforma_invoice_lines' },
  { code: 'SLS.PL', name: 'Packing List (PL)', moduleKey: 'sales', group: 'Transaction', grid: 'salesItem', lineTable: 'sls_packing_list_lines' },
  { code: 'SLS.DO', name: 'Delivery Order (DO)', moduleKey: 'sales', group: 'Transaction', grid: 'salesItem', lineTable: 'sls_delivery_order_lines' },
  { code: 'SLS.DR', name: 'Delivery Report (DR)', moduleKey: 'sales', group: 'Transaction', grid: 'salesItem', lineTable: 'sls_delivery_report_lines' },
  { code: 'SLS.SI', name: 'Sales Invoice (SI)', moduleKey: 'sales', group: 'Transaction', grid: 'salesItem', lineTable: 'sls_invoice_lines' },
  { code: 'SLS.RP', name: 'Freight Receivable (RP)', moduleKey: 'sales', group: 'Transaction' },
  { code: 'SLS.RNR', name: 'Return Receipt (RNR)', moduleKey: 'sales', group: 'Transaction', grid: 'salesItem', lineTable: 'sls_return_receipt_lines' },
  { code: 'SLS.SR', name: 'Sales Return (SR)', moduleKey: 'sales', group: 'Transaction', grid: 'salesItem', lineTable: 'sls_return_lines' },
  { code: 'SLS.IC', name: 'AR Collection (IC)', moduleKey: 'sales', group: 'Transaction' },
  { code: 'SLS.PV', name: 'AR Payment (PV)', moduleKey: 'sales', group: 'Transaction' },
  { code: 'SLS.SIE', name: 'Invoice Swap (SIE)', moduleKey: 'sales', group: 'Transaction' },
  { code: 'SLS.BB', name: 'Opening AR Balance', moduleKey: 'sales', group: 'Transaction' },
  // Production
  { code: 'MFG.WO', name: 'Work Order', moduleKey: 'production', group: 'Transaction' },
  { code: 'MFG.RES', name: 'Production Result', moduleKey: 'production', group: 'Transaction' },
  // Fixed Asset
  { code: 'FA.ACQ', name: 'Asset Acquisition', moduleKey: 'fixed-asset', group: 'Transaction' },
  { code: 'FA.DEP', name: 'Depreciation', moduleKey: 'fixed-asset', group: 'Transaction' },
  { code: 'FA.DIS', name: 'Asset Disposal', moduleKey: 'fixed-asset', group: 'Transaction' },
  // Point of Sales
  { code: 'POS.SALE', name: 'POS Sale', moduleKey: 'point-of-sales', group: 'Transaction' },
];

interface ColDef {
  field: string; header: string; width: number; type: 'TEXT' | 'NUMBER' | 'DATE' | 'LOOKUP';
  kind?: 'STANDARD' | 'CUSTOM'; lookup?: string; visible?: boolean; required?: boolean; editable?: boolean;
  lookupDefaultFilter?: Record<string, unknown>;
  skippable?: boolean;
  // Semantic render slots — null = derive from `type` via inferColumnType. Set explicitly
  // only where derivation is insufficient (e.g. rownum).
  columnType?: string; labelFormatter?: string; headerRenderer?: string; cellRenderer?: string; cellEditor?: string;
}

// Row-number column (semantic `rownum`): value = row position, read-only, centered.
const ROWNUM_COL: ColDef = {
  field: 'rowNo', header: 'No.', width: 56, type: 'NUMBER', editable: false,
  columnType: 'rownum', labelFormatter: 'NUMBER', headerRenderer: 'CENTER', cellRenderer: 'NUMERIC', cellEditor: 'ROWNUM',
};

// cashbank → fin_cash_bank_lines (single "Total" contra lines) + hidden custom slots.
const CASH_BANK_COLUMNS: ColDef[] = [
  { field: 'accountId', header: 'Akun (No · Nama)', width: 320, type: 'LOOKUP', lookup: 'accounts', required: true },
  { field: 'amount', header: 'Total', width: 160, type: 'NUMBER' },
  { field: 'amountFx', header: 'Total Valas', width: 140, type: 'NUMBER', visible: false },
  { field: 'notes', header: 'Catatan', width: 240, type: 'TEXT' },
  { field: 'costCenterId', header: 'Cost Center', width: 220, type: 'LOOKUP', lookup: 'cost-centers' },
  { field: 'divisionId', header: 'Divisi', width: 180, type: 'LOOKUP', lookup: 'divisions', visible: false },
  { field: 'subdivisionId', header: 'Sub Divisi', width: 180, type: 'LOOKUP', lookup: 'sub-divisions', visible: false },
  { field: 'projectId', header: 'Proyek', width: 180, type: 'LOOKUP', lookup: 'projects', visible: false },
  { field: 'customText1', header: 'Custom Text 1', width: 140, type: 'TEXT', kind: 'CUSTOM', visible: false },
  { field: 'customText2', header: 'Custom Text 2', width: 140, type: 'TEXT', kind: 'CUSTOM', visible: false },
  { field: 'customText3', header: 'Custom Text 3', width: 140, type: 'TEXT', kind: 'CUSTOM', visible: false },
  { field: 'customDouble1', header: 'Custom Double 1', width: 140, type: 'NUMBER', kind: 'CUSTOM', visible: false },
  { field: 'customDouble2', header: 'Custom Double 2', width: 140, type: 'NUMBER', kind: 'CUSTOM', visible: false },
  { field: 'customDate1', header: 'Custom Date 1', width: 140, type: 'DATE', kind: 'CUSTOM', visible: false },
  { field: 'customDate2', header: 'Custom Date 2', width: 140, type: 'DATE', kind: 'CUSTOM', visible: false },
];

// journal → fin_journal_lines (Akun · Debit · Kredit + GL dimensions; dims hidden by default).
const JOURNAL_COLUMNS: ColDef[] = [
  ROWNUM_COL,
  { field: 'accountId', header: 'Akun (No · Nama)', width: 320, type: 'LOOKUP', lookup: 'accounts', required: true },
  { field: 'debit', header: 'Debit', width: 160, type: 'NUMBER' },
  { field: 'credit', header: 'Kredit', width: 160, type: 'NUMBER' },
  { field: 'notes', header: 'Catatan', width: 240, type: 'TEXT' },
  { field: 'costCenterId', header: 'Cost Center', width: 220, type: 'LOOKUP', lookup: 'cost-centers' },
  { field: 'divisionId', header: 'Divisi', width: 180, type: 'LOOKUP', lookup: 'divisions', visible: false },
  { field: 'subdivisionId', header: 'Sub Divisi', width: 180, type: 'LOOKUP', lookup: 'sub-divisions', visible: false },
  { field: 'projectId', header: 'Proyek', width: 180, type: 'LOOKUP', lookup: 'projects', visible: false },
];

// giro → fin_giros (instrument capture: issue/receive a giro/cek).
const GIRO_COLUMNS: ColDef[] = [
  ROWNUM_COL,
  { field: 'giroNumber', header: 'No Giro/Cek', width: 200, type: 'TEXT', required: true },
  { field: 'bankName', header: 'Bank Penerbit', width: 200, type: 'TEXT' },
  { field: 'dueDate', header: 'Jatuh Tempo', width: 160, type: 'DATE' },
  { field: 'amount', header: 'Nominal', width: 180, type: 'NUMBER' },
  { field: 'notes', header: 'Catatan', width: 240, type: 'TEXT' },
];

// giroClearing → fin_giros (clear an outstanding giro: add Tgl Cair + Akun Bank tujuan).
const GIRO_CLEARING_COLUMNS: ColDef[] = [
  ROWNUM_COL,
  { field: 'giroNumber', header: 'No Giro/Cek', width: 200, type: 'TEXT', required: true },
  { field: 'dueDate', header: 'Jatuh Tempo', width: 160, type: 'DATE' },
  { field: 'amount', header: 'Nominal', width: 180, type: 'NUMBER' },
  { field: 'clearedDate', header: 'Tgl Cair', width: 160, type: 'DATE', required: true },
  { field: 'bankAccountId', header: 'Akun Bank', width: 280, type: 'LOOKUP', lookup: 'accounts' },
  { field: 'notes', header: 'Catatan', width: 240, type: 'TEXT' },
];

// ── Inventory (M3) item-line columns → inv_* line tables ─────────────────────

// Hidden custom slots shared by every inventory item-line family.
const INV_CUSTOM_SLOTS: ColDef[] = [
  { field: 'customText1', header: 'Custom Text 1', width: 140, type: 'TEXT', kind: 'CUSTOM', visible: false },
  { field: 'customText2', header: 'Custom Text 2', width: 140, type: 'TEXT', kind: 'CUSTOM', visible: false },
  { field: 'customDouble1', header: 'Custom Double 1', width: 140, type: 'NUMBER', kind: 'CUSTOM', visible: false },
  { field: 'customDate1', header: 'Custom Date 1', width: 140, type: 'DATE', kind: 'CUSTOM', visible: false },
];

// invMoveTransfer → inv_stock_movement_lines (Stock Transfer / Transfer Receipt): item + source/dest warehouse.
const INV_MOVE_TRANSFER_COLUMNS: ColDef[] = [
  ROWNUM_COL,
  { field: 'itemId', header: 'Item (Kode · Nama)', width: 320, type: 'LOOKUP', lookup: 'items', required: true },
  { field: 'quantity', header: 'Qty', width: 120, type: 'NUMBER', required: true },
  { field: 'unitId', header: 'Satuan', width: 140, type: 'LOOKUP', lookup: 'units', required: true },
  { field: 'sourceWarehouseId', header: 'Gudang Asal', width: 200, type: 'LOOKUP', lookup: 'warehouses' },
  { field: 'destinationWarehouseId', header: 'Gudang Tujuan', width: 200, type: 'LOOKUP', lookup: 'warehouses' },
  { field: 'unitCost', header: 'Harga', width: 140, type: 'NUMBER', visible: false },
  { field: 'notes', header: 'Catatan', width: 240, type: 'TEXT' },
  { field: 'costCenterId', header: 'Cost Center', width: 200, type: 'LOOKUP', lookup: 'cost-centers', visible: false },
  { field: 'projectId', header: 'Proyek', width: 180, type: 'LOOKUP', lookup: 'projects', visible: false },
  ...INV_CUSTOM_SLOTS,
];

// invMoveRequest → inv_stock_movement_lines (Material Request): item demand, warehouse at header.
const INV_MOVE_REQUEST_COLUMNS: ColDef[] = [
  ROWNUM_COL,
  { field: 'itemId', header: 'Item (Kode · Nama)', width: 320, type: 'LOOKUP', lookup: 'items', required: true },
  { field: 'quantity', header: 'Qty', width: 120, type: 'NUMBER', required: true },
  { field: 'unitId', header: 'Satuan', width: 140, type: 'LOOKUP', lookup: 'units', required: true },
  { field: 'notes', header: 'Catatan', width: 280, type: 'TEXT' },
  { field: 'costCenterId', header: 'Cost Center', width: 200, type: 'LOOKUP', lookup: 'cost-centers', visible: false },
  { field: 'projectId', header: 'Proyek', width: 180, type: 'LOOKUP', lookup: 'projects', visible: false },
  ...INV_CUSTOM_SLOTS,
];

// invFuel → inv_stock_movement_lines (Fuel Refill): fuel item + price/liter visible.
const INV_FUEL_COLUMNS: ColDef[] = [
  ROWNUM_COL,
  { field: 'itemId', header: 'BBM (Kode · Nama)', width: 300, type: 'LOOKUP', lookup: 'items', required: true },
  { field: 'quantity', header: 'Qty (Liter)', width: 130, type: 'NUMBER', required: true },
  { field: 'unitId', header: 'Satuan', width: 140, type: 'LOOKUP', lookup: 'units', required: true },
  { field: 'unitCost', header: 'Harga/Liter', width: 150, type: 'NUMBER' },
  { field: 'destinationWarehouseId', header: 'Tangki/Lokasi', width: 200, type: 'LOOKUP', lookup: 'warehouses', visible: false },
  { field: 'notes', header: 'Catatan', width: 240, type: 'TEXT' },
  ...INV_CUSTOM_SLOTS,
];

// invOpening → inv_opening_stock_lines (Opening Stock): item + cost + warehouse.
const INV_OPENING_COLUMNS: ColDef[] = [
  ROWNUM_COL,
  { field: 'itemId', header: 'Item (Kode · Nama)', width: 320, type: 'LOOKUP', lookup: 'items', required: true },
  { field: 'quantity', header: 'Qty', width: 120, type: 'NUMBER', required: true },
  { field: 'unitId', header: 'Satuan', width: 140, type: 'LOOKUP', lookup: 'units', required: true },
  { field: 'unitCost', header: 'Harga Pokok', width: 150, type: 'NUMBER', required: true },
  { field: 'warehouseId', header: 'Gudang', width: 200, type: 'LOOKUP', lookup: 'warehouses', required: true },
  { field: 'inventoryAccountId', header: 'Akun Persediaan', width: 240, type: 'LOOKUP', lookup: 'accounts' },
  { field: 'notes', header: 'Catatan', width: 220, type: 'TEXT' },
  { field: 'costCenterId', header: 'Cost Center', width: 200, type: 'LOOKUP', lookup: 'cost-centers', visible: false },
  { field: 'projectId', header: 'Proyek', width: 180, type: 'LOOKUP', lookup: 'projects', visible: false },
  ...INV_CUSTOM_SLOTS,
];

// invCount → inv_stock_count_lines (Stock Count): system vs physical qty + variance.
const INV_COUNT_COLUMNS: ColDef[] = [
  ROWNUM_COL,
  { field: 'itemId', header: 'Item (Kode · Nama)', width: 320, type: 'LOOKUP', lookup: 'items', required: true },
  { field: 'unitId', header: 'Satuan', width: 140, type: 'LOOKUP', lookup: 'units', required: true },
  { field: 'warehouseId', header: 'Gudang', width: 200, type: 'LOOKUP', lookup: 'warehouses' },
  { field: 'systemQty', header: 'Qty Sistem', width: 130, type: 'NUMBER' },
  { field: 'physicalQty', header: 'Qty Fisik', width: 130, type: 'NUMBER', required: true },
  { field: 'goodQty', header: 'Qty Baik', width: 130, type: 'NUMBER', visible: false },
  { field: 'damagedQty', header: 'Qty Rusak', width: 130, type: 'NUMBER', visible: false },
  { field: 'varianceQty', header: 'Selisih', width: 130, type: 'NUMBER', visible: false },
  { field: 'notes', header: 'Catatan', width: 220, type: 'TEXT' },
  ...INV_CUSTOM_SLOTS,
];

// invAdjustment → inv_stock_adjustment_lines (Stock Adjustment): +/- qty with GL accounts.
const INV_ADJUSTMENT_COLUMNS: ColDef[] = [
  ROWNUM_COL,
  { field: 'itemId', header: 'Item (Kode · Nama)', width: 320, type: 'LOOKUP', lookup: 'items', required: true },
  { field: 'direction', header: 'Arah (+/-)', width: 120, type: 'TEXT' },
  { field: 'quantity', header: 'Qty', width: 120, type: 'NUMBER', required: true },
  { field: 'unitId', header: 'Satuan', width: 140, type: 'LOOKUP', lookup: 'units', required: true },
  { field: 'unitCost', header: 'Harga', width: 140, type: 'NUMBER' },
  { field: 'warehouseId', header: 'Gudang', width: 200, type: 'LOOKUP', lookup: 'warehouses' },
  { field: 'inventoryAccountId', header: 'Akun Persediaan', width: 220, type: 'LOOKUP', lookup: 'accounts' },
  { field: 'contraAccountId', header: 'Akun Lawan', width: 220, type: 'LOOKUP', lookup: 'accounts' },
  { field: 'notes', header: 'Catatan', width: 220, type: 'TEXT' },
  ...INV_CUSTOM_SLOTS,
];

// invCostRecalc → inv_cost_recalculation_lines (Price Adjustment): old vs new unit cost.
const INV_COST_RECALC_COLUMNS: ColDef[] = [
  ROWNUM_COL,
  { field: 'itemId', header: 'Item (Kode · Nama)', width: 320, type: 'LOOKUP', lookup: 'items', required: true },
  { field: 'warehouseId', header: 'Gudang', width: 200, type: 'LOOKUP', lookup: 'warehouses' },
  { field: 'oldUnitCost', header: 'Harga Lama', width: 150, type: 'NUMBER' },
  { field: 'newUnitCost', header: 'Harga Baru', width: 150, type: 'NUMBER', required: true },
  { field: 'affectedQty', header: 'Qty Terdampak', width: 150, type: 'NUMBER' },
  { field: 'deltaAmount', header: 'Selisih Nilai', width: 160, type: 'NUMBER', visible: false },
  ...INV_CUSTOM_SLOTS,
];

// invDailyCheck → inv_daily_check_lines (Time Sheet/Daily Check): qty-based count
// per item/warehouse. Mirrors defaultInvDailyCheckCols() in the frontend line-model.
const INV_DAILY_CHECK_COLUMNS: ColDef[] = [
  ROWNUM_COL,
  { field: 'itemId', header: 'Item (Kode · Nama)', width: 320, type: 'LOOKUP', lookup: 'items', required: true },
  { field: 'quantity', header: 'Qty', width: 120, type: 'NUMBER', required: true },
  { field: 'unitId', header: 'Satuan', width: 140, type: 'LOOKUP', lookup: 'units', required: true },
  { field: 'warehouseId', header: 'Gudang', width: 200, type: 'LOOKUP', lookup: 'warehouses' },
  { field: 'costCenterId', header: 'Cost Center', width: 200, type: 'LOOKUP', lookup: 'cost-centers', visible: false },
  { field: 'machineRef', header: 'Mesin', width: 180, type: 'TEXT' },
  { field: 'workHours', header: 'Jam Kerja', width: 120, type: 'NUMBER' },
  { field: 'notes', header: 'Catatan', width: 220, type: 'TEXT' },
  ...INV_CUSTOM_SLOTS,
];

// ── Sales (M5) item-line columns → sls_*_lines tables ────────────────────────

// salesItem → per-txn sls_*_lines (item + qty/price/disc/tax + derived total).
// dataFields are a FIXED CONTRACT — the frontend binds to them exactly; do not rename.
const SALES_ITEM_COLUMNS: ColDef[] = [
  ROWNUM_COL,
  { field: 'itemId', header: 'Item', width: 300, type: 'LOOKUP', lookup: 'items', required: true },
  {
    field: 'quantity', header: 'Qty', width: 110, type: 'NUMBER', required: true,
    columnType: 'stepper', cellEditor: 'STEPPER', labelFormatter: 'NUMBER', cellRenderer: 'NUMERIC',
  },
  { field: 'unitId', header: 'Satuan', width: 140, type: 'LOOKUP', lookup: 'units', required: true },
  {
    field: 'unitPrice', header: 'Harga', width: 150, type: 'NUMBER',
    columnType: 'currency', cellEditor: 'NUMBER', labelFormatter: 'CURRENCY', cellRenderer: 'CURRENCY',
  },
  {
    field: 'discountPercent', header: 'Disc %', width: 100, type: 'NUMBER',
    columnType: 'discount', cellEditor: 'DISCOUNT', labelFormatter: 'PERCENT', cellRenderer: 'NUMERIC',
  },
  { field: 'discountAmount', header: 'Disc Rp', width: 130, type: 'NUMBER', visible: false },
  { field: 'tax1Id', header: 'Pajak', width: 150, type: 'LOOKUP', lookup: 'taxes' },
  {
    field: 'lineTotal', header: 'Total', width: 160, type: 'NUMBER', editable: false, skippable: true,
    columnType: 'currency', cellEditor: 'NUMBER', labelFormatter: 'CURRENCY', cellRenderer: 'CURRENCY',
  },
  { field: 'warehouseId', header: 'Gudang', width: 180, type: 'LOOKUP', lookup: 'warehouses', visible: false },
  { field: 'notes', header: 'Catatan', width: 220, type: 'TEXT' },
  { field: 'costCenterId', header: 'Cost Center', width: 200, type: 'LOOKUP', lookup: 'cost-centers', visible: false },
  { field: 'divisionId', header: 'Divisi', width: 160, type: 'LOOKUP', lookup: 'divisions', visible: false },
  { field: 'subdivisionId', header: 'Sub Divisi', width: 160, type: 'LOOKUP', lookup: 'sub-divisions', visible: false },
  { field: 'projectId', header: 'Proyek', width: 160, type: 'LOOKUP', lookup: 'projects', visible: false },
];

// ── Purchasing (M4) item-line columns → pur_*_lines tables ───────────────────

// purchaseItem → per-txn pur_*_lines (item + qty/cost/disc/tax + derived total).
// Mirrors SALES_ITEM_COLUMNS; dataFields are a FIXED CONTRACT — frontend binds exactly.
const PURCHASE_ITEM_COLUMNS: ColDef[] = [
  ROWNUM_COL,
  { field: 'itemId', header: 'Item', width: 300, type: 'LOOKUP', lookup: 'items', required: true },
  {
    field: 'quantity', header: 'Qty', width: 110, type: 'NUMBER', required: true,
    columnType: 'stepper', cellEditor: 'STEPPER', labelFormatter: 'NUMBER', cellRenderer: 'NUMERIC',
  },
  { field: 'unitId', header: 'Satuan', width: 140, type: 'LOOKUP', lookup: 'units', required: true },
  {
    field: 'unitPrice', header: 'Harga', width: 150, type: 'NUMBER',
    columnType: 'currency', cellEditor: 'NUMBER', labelFormatter: 'CURRENCY', cellRenderer: 'CURRENCY',
  },
  {
    field: 'discountPercent', header: 'Disc %', width: 100, type: 'NUMBER',
    columnType: 'discount', cellEditor: 'DISCOUNT', labelFormatter: 'PERCENT', cellRenderer: 'NUMERIC',
  },
  { field: 'discountAmount', header: 'Disc Rp', width: 130, type: 'NUMBER', visible: false },
  { field: 'tax1Id', header: 'Pajak', width: 150, type: 'LOOKUP', lookup: 'taxes' },
  {
    field: 'lineTotal', header: 'Total', width: 160, type: 'NUMBER', editable: false, skippable: true,
    columnType: 'currency', cellEditor: 'NUMBER', labelFormatter: 'CURRENCY', cellRenderer: 'CURRENCY',
  },
  { field: 'warehouseId', header: 'Gudang', width: 180, type: 'LOOKUP', lookup: 'warehouses', visible: false },
  { field: 'notes', header: 'Catatan', width: 220, type: 'TEXT' },
  { field: 'costCenterId', header: 'Cost Center', width: 200, type: 'LOOKUP', lookup: 'cost-centers', visible: false },
  { field: 'divisionId', header: 'Divisi', width: 160, type: 'LOOKUP', lookup: 'divisions', visible: false },
  { field: 'subdivisionId', header: 'Sub Divisi', width: 160, type: 'LOOKUP', lookup: 'sub-divisions', visible: false },
  { field: 'projectId', header: 'Proyek', width: 160, type: 'LOOKUP', lookup: 'projects', visible: false },
];

// purchaseReceipt → pur_goods_receipt_lines (item + QC delta: accepted/rejected/quarantine).
// Stock only increases by acceptedQty; rejectedQty seeds a return (resolved §8 #25).
const PURCHASE_RECEIPT_COLUMNS: ColDef[] = [
  ROWNUM_COL,
  { field: 'itemId', header: 'Item', width: 300, type: 'LOOKUP', lookup: 'items', required: true },
  {
    field: 'quantity', header: 'Qty Kirim', width: 120, type: 'NUMBER', required: true,
    columnType: 'stepper', cellEditor: 'STEPPER', labelFormatter: 'NUMBER', cellRenderer: 'NUMERIC',
  },
  { field: 'unitId', header: 'Satuan', width: 130, type: 'LOOKUP', lookup: 'units', required: true },
  { field: 'acceptedQty', header: 'Diterima', width: 120, type: 'NUMBER' },
  { field: 'rejectedQty', header: 'Ditolak', width: 120, type: 'NUMBER', visible: false },
  { field: 'quarantineQty', header: 'Karantina', width: 120, type: 'NUMBER', visible: false },
  {
    field: 'unitCost', header: 'Harga Pokok', width: 150, type: 'NUMBER', visible: false,
    columnType: 'currency', cellEditor: 'NUMBER', labelFormatter: 'CURRENCY', cellRenderer: 'CURRENCY',
  },
  { field: 'warehouseId', header: 'Gudang', width: 180, type: 'LOOKUP', lookup: 'warehouses' },
  { field: 'notes', header: 'Catatan', width: 220, type: 'TEXT' },
  { field: 'costCenterId', header: 'Cost Center', width: 200, type: 'LOOKUP', lookup: 'cost-centers', visible: false },
  { field: 'projectId', header: 'Proyek', width: 160, type: 'LOOKUP', lookup: 'projects', visible: false },
];

// purchaseRfq → pur_rfq_suppliers (RFQ "lines" = invited suppliers, NOT items).
const PURCHASE_RFQ_COLUMNS: ColDef[] = [
  ROWNUM_COL,
  {
    field: 'supplierId', header: 'Supplier', width: 360, type: 'LOOKUP', lookup: 'partners', required: true,
    lookupDefaultFilter: { typeKind: 'SUPPLIER', isActive: true },
  },
  { field: 'notes', header: 'Catatan', width: 320, type: 'TEXT' },
];

// purchaseBid → pur_bid_selection_lines (compare quotations: rank + winner flag).
const PURCHASE_BID_COLUMNS: ColDef[] = [
  ROWNUM_COL,
  { field: 'priceRank', header: 'Peringkat Harga', width: 150, type: 'NUMBER' },
  { field: 'selected', header: 'Terpilih', width: 120, type: 'TEXT' },
  { field: 'notes', header: 'Catatan', width: 360, type: 'TEXT' },
];

const COLUMNS_BY_FAMILY: Record<GridFamily, ColDef[]> = {
  cashbank: CASH_BANK_COLUMNS,
  journal: JOURNAL_COLUMNS,
  giro: GIRO_COLUMNS,
  giroClearing: GIRO_CLEARING_COLUMNS,
  invMoveTransfer: INV_MOVE_TRANSFER_COLUMNS,
  invMoveRequest: INV_MOVE_REQUEST_COLUMNS,
  invFuel: INV_FUEL_COLUMNS,
  invOpening: INV_OPENING_COLUMNS,
  invCount: INV_COUNT_COLUMNS,
  invAdjustment: INV_ADJUSTMENT_COLUMNS,
  invCostRecalc: INV_COST_RECALC_COLUMNS,
  invDailyCheck: INV_DAILY_CHECK_COLUMNS,
  salesItem: SALES_ITEM_COLUMNS,
  purchaseItem: PURCHASE_ITEM_COLUMNS,
  purchaseReceipt: PURCHASE_RECEIPT_COLUMNS,
  purchaseRfq: PURCHASE_RFQ_COLUMNS,
  purchaseBid: PURCHASE_BID_COLUMNS,
};

// Generic inventory placeholders superseded by the M3 transaction codes above.
const OBSOLETE_CODES = ['INV.IN', 'INV.OUT', 'INV.TRF', 'INV.OPN'];

export async function seedTransactionGrids(prisma: PrismaClient): Promise<void> {
  const moduleLabel = new Map(MODULES.map((m) => [m.key, m.label]));
  const moduleOrder = new Map(MODULES.map((m) => [m.key, m.sortOrder]));

  // Drop superseded placeholder types (no transactions reference them yet);
  // their grids/columns cascade away with the type row.
  await prisma.erpTransactionType.deleteMany({ where: { code: { in: OBSOLETE_CODES } } });

  // Prune obsolete sales codes replaced by SLS.SI / SLS.SR. Grids + columns
  // cascade via onDelete: Cascade (verified). SLS.SO / SLS.DO are KEPT (same codes).
  await prisma.erpTransactionType.deleteMany({ where: { code: { in: ['SLS.INV', 'SLS.RET'] } } });

  // Prune obsolete purchasing stubs replaced by the canonical 13 menu codes:
  // PUR.GR → PUR.GRN, PUR.INV → PUR.PI, PUR.RET → PUR.DNR/PUR.PRT. PUR.PO is KEPT
  // (same code). Grids + columns cascade via onDelete: Cascade.
  await prisma.erpTransactionType.deleteMany({ where: { code: { in: ['PUR.GR', 'PUR.INV', 'PUR.RET'] } } });

  for (const [i, t] of TXNS.entries()) {
    // Per-txn `lineTable` override wins; else fall back to the family's shared table.
    const lineTable = t.lineTable ?? (t.grid ? LINE_TABLE_BY_FAMILY[t.grid] : null);
    await prisma.erpTransactionType.upsert({
      where: { code: t.code },
      create: {
        code: t.code, name: t.name, moduleKey: t.moduleKey,
        moduleLabel: moduleLabel.get(t.moduleKey) ?? t.moduleKey,
        groupLabel: t.group, lineTable,
        sortOrder: (moduleOrder.get(t.moduleKey) ?? 0) * 100 + i,
      },
      update: {
        name: t.name, moduleLabel: moduleLabel.get(t.moduleKey) ?? t.moduleKey,
        groupLabel: t.group, lineTable,
      },
    });
  }

  // Primary "main" grid (tab) + default columns per finance transaction family.
  for (const t of TXNS) {
    if (!t.grid) continue;
    const type = await prisma.erpTransactionType.findUnique({ where: { code: t.code } });
    if (!type) continue;
    const grid = await prisma.erpTransactionGrid.upsert({
      where: { transactionTypeId_key: { transactionTypeId: type.id, key: 'main' } },
      create: {
        transactionTypeId: type.id, key: 'main', label: 'Utama', sortOrder: 0,
        lineTable: type.lineTable, isPrimary: true,
      },
      update: { lineTable: type.lineTable, isPrimary: true },
    });
    const columns = COLUMNS_BY_FAMILY[t.grid];
    for (const [i, c] of columns.entries()) {
      await prisma.erpTransactionGridColumn.upsert({
        where: { gridId_dataField: { gridId: grid.id, dataField: c.field } },
        create: {
          gridId: grid.id, sortOrder: i, headerText: c.header, dataField: c.field,
          width: c.width, dataType: c.type, kind: c.kind ?? 'STANDARD', lookupSource: c.lookup ?? null,
          isVisible: c.visible ?? true, isRequired: c.required ?? false, isEditable: c.editable ?? true,
          isSkippable: c.skippable ?? false,
          lookupDefaultFilter: c.lookupDefaultFilter ?? undefined,
          columnType: c.columnType ?? null, labelFormatter: c.labelFormatter ?? null,
          headerRenderer: c.headerRenderer ?? null, cellRenderer: c.cellRenderer ?? null, cellEditor: c.cellEditor ?? null,
        },
        update: {}, // keep user edits on re-seed; only create missing defaults
      });
    }
  }

  const gridded = TXNS.filter((t) => t.grid).length;
  console.log(`✓ sys_transaction_types (${TXNS.length}) + primary grid + default columns (${gridded} fin/inv/sales/pur txns)`);
}
