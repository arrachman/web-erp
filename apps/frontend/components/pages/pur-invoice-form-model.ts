// From/to mapping for the Purchase Invoice (PI) transaction form.
// The form-data shape is identical to Purchase Order, so we reuse
// `PurOrderFormData` verbatim — only the record↔form mapping is PI-specific.
// Mirrors fromPurOrder / toPurOrderPayload. settlementStatus is backend-derived
// (not submitted from the form).

import type { PurOrderFormData } from './pur-order-form-model';
import type {
  CreatePurInvoicePayload,
  ErpPurInvoice,
} from '@/lib/api/pur-invoices';

export function fromPurInvoice(r: ErpPurInvoice): PurOrderFormData {
  return {
    id: r.id,
    docNumber: r.docNumber,
    auto: !!r.autoNumber,
    docDate: r.docDate.slice(0, 10),
    dueDate: r.dueDate ? r.dueDate.slice(0, 10) : '',
    supplierId: r.supplierId ?? '',
    supplierLabel: r.supplier?.name,
    branchId: r.branchId,
    branchLabel: r.branch?.name,
    locationId: r.locationId ?? '',
    locationLabel: r.location?.name,
    warehouseId: r.warehouseId ?? '',
    warehouseLabel: r.warehouse?.name,
    payableAccountId: r.payableAccountId ?? '',
    payableAccountLabel: r.payableAccount?.name,
    paymentTermId: r.paymentTermId ?? '',
    paymentTermLabel: r.paymentTerm?.name,
    currencyId: r.currencyId,
    exchangeRate: r.exchangeRate,
    priceMode: r.priceMode,
    description: r.description ?? '',
    referenceNo: r.referenceNo ?? '',
    notes: r.notes ?? '',
    status: r.status,
    postedAt: r.postedAt,
    grandTotal: r.grandTotal,
    customFields: {},
    lines: r.lines.map((l) => ({
      key: `pl-${l.id ?? l.lineNo}`,
      itemId: l.itemId,
      itemLabel: l.item?.name,
      quantity: l.quantity,
      unitId: l.unitId,
      unitLabel: l.unit?.name,
      unitPrice: l.unitPrice,
      discountPercent: l.discountPercent ?? undefined,
      discountAmount: l.discountAmount ?? undefined,
      tax1Id: l.tax1Id ?? undefined,
      tax1Label: l.tax1?.name,
      tax2Id: l.tax2Id ?? undefined,
      warehouseId: l.warehouseId ?? undefined,
      warehouseLabel: l.warehouse?.name,
      notes: l.notes ?? undefined,
      costCenterId: l.costCenterId ?? undefined,
      divisionId: l.divisionId ?? undefined,
      subdivisionId: l.subdivisionId ?? undefined,
      projectId: l.projectId ?? undefined,
    })),
  };
}

export function toPurInvoicePayload(d: PurOrderFormData): CreatePurInvoicePayload {
  return {
    auto: d.auto,
    docNumber: d.auto ? undefined : d.docNumber || undefined,
    docDate: d.docDate,
    dueDate: d.dueDate || undefined,
    branchId: d.branchId,
    locationId: d.locationId || undefined,
    warehouseId: d.warehouseId || undefined,
    supplierId: d.supplierId || undefined,
    paymentTermId: d.paymentTermId || undefined,
    payableAccountId: d.payableAccountId || undefined,
    currencyId: d.currencyId,
    exchangeRate: d.exchangeRate || '1',
    priceMode: d.priceMode,
    description: d.description || undefined,
    referenceNo: d.referenceNo || undefined,
    notes: d.notes || undefined,
    lines: d.lines
      .filter((l) => l.itemId && Number(l.quantity) > 0)
      .map((l, i) => ({
        itemId: l.itemId,
        quantity: l.quantity,
        unitId: l.unitId,
        unitPrice: l.unitPrice || '0',
        discountPercent: l.discountPercent || undefined,
        discountAmount: l.discountAmount || undefined,
        tax1Id: l.tax1Id || undefined,
        tax2Id: l.tax2Id || undefined,
        warehouseId: l.warehouseId || undefined,
        costCenterId: l.costCenterId || undefined,
        divisionId: l.divisionId || undefined,
        subdivisionId: l.subdivisionId || undefined,
        projectId: l.projectId || undefined,
        notes: l.notes || undefined,
        lineNo: i + 1,
      })),
  };
}
