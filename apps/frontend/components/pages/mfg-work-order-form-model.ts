/**
 * Manufacturing Work Order form — data model, factories, and payload builders.
 * Kept in a separate file so the React component stays under 400 lines.
 */

import type {
  ErpMfgWorkOrder,
  ErpMfgWorkOrderLine,
  CreateMfgWorkOrderPayload,
  MfgWorkOrderLinePayload,
} from '@/lib/api/mfg-work-orders';

// ─── Form state types ─────────────────────────────────────────────────────────

export interface MfgWorkOrderLineFormData {
  id?: string;
  lineType: 'INPUT' | 'OUTPUT';
  itemId: string;
  itemName?: string;
  quantity: string;
  unitId: string;
  unitName?: string;
  warehouseId?: string;
  warehouseName?: string;
  costCenterId?: string;
  divisionId?: string;
  notes?: string;
  lineNo: number;
}

export interface MfgWorkOrderFormData {
  id?: string;
  docNumber?: string;
  auto: boolean;
  docDate: string;
  fiscalPeriodId?: string;
  branchId: string;
  branchName?: string;
  warehouseId?: string;
  warehouseName?: string;
  bomId?: string;
  description?: string;
  notes?: string;
  referenceNo?: string;
  legacyCode?: string;
  lines: MfgWorkOrderLineFormData[];
}

// ─── Factories ────────────────────────────────────────────────────────────────

function today(): string {
  return new Date().toISOString().slice(0, 10);
}

export function defaultMfgWorkOrderForm(): MfgWorkOrderFormData {
  return {
    auto: true,
    docDate: today(),
    branchId: '',
    warehouseId: '',
    bomId: '',
    description: '',
    notes: '',
    referenceNo: '',
    lines: [],
  };
}

function lineFromApi(l: ErpMfgWorkOrderLine, idx: number): MfgWorkOrderLineFormData {
  return {
    id: l.id,
    lineType: l.lineType,
    itemId: l.itemId,
    itemName: l.item?.name,
    quantity: l.quantity,
    unitId: l.unitId,
    unitName: l.unit?.name,
    warehouseId: l.warehouseId ?? '',
    warehouseName: l.warehouse?.name,
    costCenterId: l.costCenterId ?? '',
    divisionId: l.divisionId ?? '',
    notes: l.notes ?? '',
    lineNo: l.lineNo ?? idx + 1,
  };
}

export function fromMfgWorkOrder(r: ErpMfgWorkOrder): MfgWorkOrderFormData {
  return {
    id: r.id,
    docNumber: r.docNumber,
    auto: false,
    docDate: r.docDate.slice(0, 10),
    fiscalPeriodId: r.fiscalPeriodId,
    branchId: r.branchId,
    branchName: r.branch?.name,
    warehouseId: r.warehouseId ?? '',
    warehouseName: r.warehouse?.name,
    bomId: r.bomId ?? '',
    description: r.description ?? '',
    notes: r.notes ?? '',
    referenceNo: r.referenceNo ?? '',
    legacyCode: r.legacyCode ?? '',
    lines: r.lines.map(lineFromApi),
  };
}

function lineToPayload(l: MfgWorkOrderLineFormData, idx: number): MfgWorkOrderLinePayload {
  return {
    lineType: l.lineType,
    itemId: l.itemId,
    quantity: l.quantity,
    unitId: l.unitId,
    warehouseId: l.warehouseId || undefined,
    costCenterId: l.costCenterId || undefined,
    divisionId: l.divisionId || undefined,
    notes: l.notes || undefined,
    lineNo: idx + 1,
  };
}

export function toMfgWorkOrderPayload(f: MfgWorkOrderFormData): CreateMfgWorkOrderPayload {
  return {
    docNumber: f.auto ? undefined : (f.docNumber || undefined),
    auto: f.auto,
    docDate: f.docDate,
    fiscalPeriodId: f.fiscalPeriodId || undefined,
    branchId: f.branchId,
    warehouseId: f.warehouseId || undefined,
    bomId: f.bomId || undefined,
    description: f.description || undefined,
    notes: f.notes || undefined,
    referenceNo: f.referenceNo || undefined,
    legacyCode: f.legacyCode || undefined,
    lines: f.lines.map(lineToPayload),
  };
}
