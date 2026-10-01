/**
 * Default Form Builder (sys_form_fields) header layouts for the M3 Warehouse &
 * Inventory transactions (mirrors the finance defaults in erp-form-fields.service.ts).
 * Seeded lazily by ErpFormFieldsService.getFields when an admin first opens a
 * transaction in Form Builder. Field keys bind to the inv_* header columns; LOOKUP
 * fields point at the shared lookup-source registry (warehouses/items/...).
 *
 * Layout follows web-erp/CLAUDE.md §2.36: LEFT = identity/description,
 * CENTER = dimensions (cabang/gudang/lokasi), RIGHT = tanggal/no/uang.
 *
 * DC (Daily Check) has no backing table yet → its fields are kind CUSTOM (stored in
 * a customFields json when a backend lands), so they never claim a real DB column.
 */

import type { Prisma } from '@prisma/client';

export interface InvFormFieldDefault {
  fieldKey: string;
  kind: 'STRUCTURAL' | 'CUSTOM';
  label: string;
  fieldType: 'TEXT' | 'NUMBER' | 'DATE' | 'PARTNER' | 'ACCOUNT' | 'BRANCH' | 'LOCATION' | 'CURRENCY' | 'LOOKUP';
  isRequired: boolean;
  isVisible: boolean;
  sortOrder: number;
  columnSlot: 'LEFT' | 'CENTER' | 'RIGHT';
  lookupSource?: string;
  lookupDefaultFilter?: Prisma.InputJsonValue;
}

type Slot = InvFormFieldDefault['columnSlot'];
type FieldType = InvFormFieldDefault['fieldType'];
interface Opt { req?: boolean; hidden?: boolean; kind?: 'STRUCTURAL' | 'CUSTOM'; source?: string; filter?: Prisma.InputJsonValue }

const WAREHOUSE_FILTER = { isActive: true };

function f(fieldKey: string, label: string, fieldType: FieldType, columnSlot: Slot, sortOrder: number, opt: Opt = {}): InvFormFieldDefault {
  return {
    fieldKey, kind: opt.kind ?? 'STRUCTURAL', label, fieldType, columnSlot, sortOrder,
    isRequired: opt.req ?? false, isVisible: !opt.hidden,
    ...(opt.source ? { lookupSource: opt.source } : {}),
    ...(opt.filter ? { lookupDefaultFilter: opt.filter } : {}),
  };
}

/** Warehouse lookup header field (fieldType LOOKUP → registry slug 'warehouses'). */
const wh = (key: string, label: string, slot: Slot, sort: number, req = false): InvFormFieldDefault =>
  f(key, label, 'LOOKUP', slot, sort, { req, source: 'warehouses', filter: WAREHOUSE_FILTER });

// ── Stock movement family (MR/TS/RS/RF) → inv_stock_movements header ──────────

const MR_DEFAULTS: InvFormFieldDefault[] = [
  f('description', 'Uraian', 'TEXT', 'LEFT', 0, { req: true }),
  f('requestedTo', 'Diminta Untuk', 'TEXT', 'LEFT', 1),
  f('branchId', 'Cabang', 'BRANCH', 'CENTER', 0, { req: true }),
  wh('destinationWarehouseId', 'Gudang Diminta', 'CENTER', 1, true),
  f('locationId', 'Lokasi', 'LOCATION', 'CENTER', 2),
  f('movementDate', 'Tanggal', 'DATE', 'RIGHT', 0, { req: true }),
  f('docNumber', 'No Transaksi', 'TEXT', 'RIGHT', 1),
  f('neededDate', 'Tgl Dibutuhkan', 'DATE', 'RIGHT', 2),
];

const TS_DEFAULTS: InvFormFieldDefault[] = [
  f('description', 'Uraian', 'TEXT', 'LEFT', 0, { req: true }),
  f('branchId', 'Cabang', 'BRANCH', 'CENTER', 0, { req: true }),
  wh('sourceWarehouseId', 'Gudang Asal', 'CENTER', 1, true),
  wh('destinationWarehouseId', 'Gudang Tujuan', 'CENTER', 2, true),
  f('movementDate', 'Tanggal', 'DATE', 'RIGHT', 0, { req: true }),
  f('docNumber', 'No Transaksi', 'TEXT', 'RIGHT', 1),
];

const RS_DEFAULTS: InvFormFieldDefault[] = [
  f('description', 'Uraian', 'TEXT', 'LEFT', 0, { req: true }),
  f('referenceNo', 'No Referensi (TS)', 'TEXT', 'LEFT', 1),
  f('branchId', 'Cabang', 'BRANCH', 'CENTER', 0, { req: true }),
  wh('sourceWarehouseId', 'Gudang Asal/Transit', 'CENTER', 1),
  wh('destinationWarehouseId', 'Gudang Tujuan', 'CENTER', 2, true),
  f('movementDate', 'Tanggal', 'DATE', 'RIGHT', 0, { req: true }),
  f('docNumber', 'No Transaksi', 'TEXT', 'RIGHT', 1),
  f('referenceDate', 'Tgl Referensi', 'DATE', 'RIGHT', 2),
];

const RF_DEFAULTS: InvFormFieldDefault[] = [
  f('description', 'Uraian', 'TEXT', 'LEFT', 0),
  f('branchId', 'Cabang', 'BRANCH', 'CENTER', 0, { req: true }),
  wh('destinationWarehouseId', 'Tangki/Lokasi', 'CENTER', 1),
  f('locationId', 'Lokasi', 'LOCATION', 'CENTER', 2),
  f('movementDate', 'Tanggal', 'DATE', 'RIGHT', 0, { req: true }),
  f('docNumber', 'No Transaksi', 'TEXT', 'RIGHT', 1),
];

// ── Opening / Count / Adjustment / Price ─────────────────────────────────────

const IB_DEFAULTS: InvFormFieldDefault[] = [
  f('description', 'Uraian', 'TEXT', 'LEFT', 0),
  f('branchId', 'Cabang', 'BRANCH', 'CENTER', 0, { req: true }),
  wh('warehouseId', 'Gudang', 'CENTER', 1, true),
  f('locationId', 'Lokasi', 'LOCATION', 'CENTER', 2),
  f('openingDate', 'Tanggal', 'DATE', 'RIGHT', 0, { req: true }),
  f('docNumber', 'No Transaksi', 'TEXT', 'RIGHT', 1),
  f('currencyId', 'Uang', 'CURRENCY', 'RIGHT', 2, { req: true }),
];

const SP_DEFAULTS: InvFormFieldDefault[] = [
  f('description', 'Uraian', 'TEXT', 'LEFT', 0),
  f('branchId', 'Cabang', 'BRANCH', 'CENTER', 0, { req: true }),
  wh('warehouseId', 'Gudang', 'CENTER', 1, true),
  f('countDate', 'Tanggal Hitung', 'DATE', 'RIGHT', 0, { req: true }),
  f('docNumber', 'No Transaksi', 'TEXT', 'RIGHT', 1),
];

const SA_DEFAULTS: InvFormFieldDefault[] = [
  f('description', 'Uraian', 'TEXT', 'LEFT', 0),
  f('branchId', 'Cabang', 'BRANCH', 'CENTER', 0, { req: true }),
  wh('warehouseId', 'Gudang', 'CENTER', 1, true),
  f('adjustmentDate', 'Tanggal', 'DATE', 'RIGHT', 0, { req: true }),
  f('docNumber', 'No Transaksi', 'TEXT', 'RIGHT', 1),
];

const PA_DEFAULTS: InvFormFieldDefault[] = [
  f('notes', 'Catatan', 'TEXT', 'LEFT', 0),
  wh('warehouseId', 'Gudang', 'CENTER', 0),
  f('fromDate', 'Dari Tanggal', 'DATE', 'RIGHT', 0, { req: true }),
  f('docNumber', 'No Transaksi', 'TEXT', 'RIGHT', 1),
  f('toDate', 'Sampai Tanggal', 'DATE', 'RIGHT', 2),
];

// ── Receipt Weigher (RW) → inv_weighbridge_tickets (header-heavy, no item grid) ──

const RW_DEFAULTS: InvFormFieldDefault[] = [
  f('partnerId', 'Partner', 'PARTNER', 'LEFT', 0),
  f('vehiclePlate', 'No Polisi', 'TEXT', 'LEFT', 1),
  f('driverName', 'Sopir', 'TEXT', 'LEFT', 2),
  f('itemId', 'Barang', 'LOOKUP', 'LEFT', 3, { source: 'items' }),
  f('branchId', 'Cabang', 'BRANCH', 'CENTER', 0, { req: true }),
  f('locationId', 'Lokasi', 'LOCATION', 'CENTER', 1),
  f('grossWeight', 'Bruto (kg)', 'NUMBER', 'CENTER', 2),
  f('tareWeight', 'Tara (kg)', 'NUMBER', 'CENTER', 3),
  f('netWeight', 'Netto (kg)', 'NUMBER', 'CENTER', 4),
  f('ticketDate', 'Tanggal', 'DATE', 'RIGHT', 0, { req: true }),
  f('docNumber', 'No Transaksi', 'TEXT', 'RIGHT', 1),
  f('unitPrice', 'Harga/kg', 'NUMBER', 'RIGHT', 2),
];

// ── Time Sheet/Daily Check (DC) — no backing table yet → all fields CUSTOM ────

const DC_DEFAULTS: InvFormFieldDefault[] = [
  f('description', 'Uraian', 'TEXT', 'LEFT', 0, { kind: 'CUSTOM' }),
  f('machine', 'Mesin/Unit', 'TEXT', 'LEFT', 1, { kind: 'CUSTOM' }),
  f('branchId', 'Cabang', 'BRANCH', 'CENTER', 0, { kind: 'CUSTOM', req: true }),
  f('locationId', 'Lokasi', 'LOCATION', 'CENTER', 1, { kind: 'CUSTOM' }),
  f('checkDate', 'Tanggal', 'DATE', 'RIGHT', 0, { kind: 'CUSTOM', req: true }),
  f('docNumber', 'No Transaksi', 'TEXT', 'RIGHT', 1, { kind: 'CUSTOM' }),
];

export const INV_DEFAULTS_BY_CODE: Record<string, InvFormFieldDefault[]> = {
  'INV.MR': MR_DEFAULTS,
  'INV.TS': TS_DEFAULTS,
  'INV.RS': RS_DEFAULTS,
  'INV.RF': RF_DEFAULTS,
  'INV.IB': IB_DEFAULTS,
  'INV.SP': SP_DEFAULTS,
  'INV.SA': SA_DEFAULTS,
  'INV.PA': PA_DEFAULTS,
  'INV.RW': RW_DEFAULTS,
  'INV.DC': DC_DEFAULTS,
};
