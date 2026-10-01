// From/to mapping for Goods Receipt (GRN) transaction form.
// Header = same shape as PurOrderFormData (reuse). Lines carry QC delta.

import type { PurOrderFormData } from './pur-order-form-model';
import type {
  CreatePurGoodsReceiptPayload, ErpPurGoodsReceipt, ErpPurGoodsReceiptLine,
} from '@/lib/api/pur-goods-receipts';

/** Thin from-mapper: GRN record → PurOrderFormData (header only; lines mapped inline). */
export function fromPurGoodsReceipt(r: ErpPurGoodsReceipt): PurOrderFormData {
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
    // GRN lines: map QC fields into the `customFields` bag on each line row,
    // since PurItemLineRow doesn't have QC-specific typed keys. The GRN grid
    // (via PUR.GRN Kustomisasi Grid config) surfaces them as dataFields from DB.
    lines: r.lines.map((l: ErpPurGoodsReceiptLine) => ({
      key: `gl-${l.id ?? l.lineNo}`,
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
      // QC delta persisted as customFields so the shared grid can display them.
      customFields: {
        acceptedQty: l.acceptedQty,
        rejectedQty: l.rejectedQty,
        quarantineQty: l.quarantineQty,
        qcStatus: l.qcStatus,
        unitCost: l.unitCost ?? undefined,
        accruedPayableAccountId: l.accruedPayableAccountId ?? undefined,
        orderLineId: l.orderLineId ?? undefined,
      },
    })),
  };
}

export function toPurGoodsReceiptPayload(d: PurOrderFormData): CreatePurGoodsReceiptPayload {
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
        unitCost: (l.customFields?.unitCost as string | undefined) || undefined,
        acceptedQty: (l.customFields?.acceptedQty as string | undefined) || undefined,
        rejectedQty: (l.customFields?.rejectedQty as string | undefined) || undefined,
        quarantineQty: (l.customFields?.quarantineQty as string | undefined) || undefined,
        qcStatus: (l.customFields?.qcStatus as string | undefined) as never || undefined,
        accruedPayableAccountId: (l.customFields?.accruedPayableAccountId as string | undefined) || undefined,
        orderLineId: (l.customFields?.orderLineId as string | undefined) || undefined,
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
