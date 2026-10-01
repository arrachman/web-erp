/* eslint-disable @typescript-eslint/no-explicit-any */
import { Prisma } from '@prisma/client';
import { CreateSlsPackingListDto, SlsPackingListLineDto } from './dto/create-sls-packing-list.dto';
import { UpdateSlsPackingListDto } from './dto/update-sls-packing-list.dto';
import { toBigInt, mapPackingListLine } from './sls-packing-list.helpers';

/** Existing DB row shape with lines included (output of findRaw). */
type ExistingPackingList = Prisma.ErpSlsPackingListGetPayload<{
  include: { lines: true };
}>;

export interface CreateSlsPackingListCtx {
  docNumber: string;
  wantAuto: boolean;
  fiscalPeriodId: bigint;
  dueDate: Date | null;
  priceMode: 'TAX_EXCLUSIVE' | 'TAX_INCLUSIVE';
  subtotal: Prisma.Decimal;
  discountAmount: Prisma.Decimal | null;
  otherCostAmount: Prisma.Decimal | null;
  grandTotal: Prisma.Decimal;
  computedLines: SlsPackingListLineDto[];
  header: { currencyId: string; exchangeRate: string };
  actor: bigint | null;
}

/**
 * Build the Prisma `data` literal for `erpSlsPackingList.create`.
 * Period, doc-number, tax, and the `create` transaction itself stay in the
 * service — this is a pure shape builder with no Prisma or injected deps.
 */
export function buildSlsPackingListCreateData(
  dto: CreateSlsPackingListDto,
  ctx: CreateSlsPackingListCtx,
): Prisma.ErpSlsPackingListUncheckedCreateInput {
  const {
    docNumber,
    wantAuto,
    fiscalPeriodId,
    dueDate,
    priceMode,
    subtotal,
    discountAmount,
    otherCostAmount,
    grandTotal,
    computedLines,
    header,
    actor,
  } = ctx;
  return {
    code: docNumber,
    docNumber,
    autoNumber: wantAuto ? docNumber : null,
    branchId: BigInt(dto.branchId),
    locationId: toBigInt(dto.locationId),
    warehouseId: toBigInt(dto.warehouseId),
    docDate: new Date(dto.docDate),
    fiscalPeriodId,
    customerId: toBigInt(dto.customerId),
    paymentTermId: toBigInt(dto.paymentTermId),
    dueDate,
    currencyId: BigInt(dto.currencyId),
    exchangeRate: new Prisma.Decimal(dto.exchangeRate),
    priceMode: priceMode as never,
    subtotal,
    discountPercent:
      dto.discountPercent != null ? new Prisma.Decimal(dto.discountPercent) : null,
    discountAmount,
    tax1Amount: dto.tax1Amount != null ? new Prisma.Decimal(dto.tax1Amount) : null,
    tax2Amount: dto.tax2Amount != null ? new Prisma.Decimal(dto.tax2Amount) : null,
    otherCostAmount,
    grandTotal,
    description: dto.description ?? null,
    notes: dto.notes ?? null,
    referenceNo: dto.referenceNo ?? null,
    referenceDate: dto.referenceDate ? new Date(dto.referenceDate) : null,
    receivableAccountId: toBigInt(dto.receivableAccountId),
    salesDeptId: toBigInt(dto.salesDeptId),
    quotationId: toBigInt(dto.quotationId),
    orderId: toBigInt(dto.orderId),
    proformaInvoiceId: toBigInt(dto.proformaInvoiceId),
    status: 'DRAFT',
    postingStatus: 'UNPOSTED',
    legacyCode: dto.legacyCode ?? null,
    createdById: actor,
    updatedById: actor,
    lines: computedLines.length
      ? { create: computedLines.map((l) => mapPackingListLine(l, header)) }
      : undefined,
  };
}

/**
 * Build the update field assignments (orig ~L229-265). Preserves the current
 * `any` update-data behavior — types are intentionally NOT tightened here.
 * NOTE: returns a partial object; the service adds fiscalPeriodId / totals
 * lines / line-recreate after this call.
 */
export function buildSlsPackingListUpdatePatch(
  dto: UpdateSlsPackingListDto,
  actor: bigint | null,
): Record<string, unknown> {
  const data: any = { updatedById: actor };
  if (dto.docNumber !== undefined) {
    data.docNumber = dto.docNumber;
    data.code = dto.docNumber;
  }
  if (dto.branchId !== undefined) data.branchId = BigInt(dto.branchId);
  if (dto.locationId !== undefined) data.locationId = toBigInt(dto.locationId);
  if (dto.warehouseId !== undefined) data.warehouseId = toBigInt(dto.warehouseId);
  if (dto.customerId !== undefined) data.customerId = toBigInt(dto.customerId);
  if (dto.paymentTermId !== undefined) data.paymentTermId = toBigInt(dto.paymentTermId);
  if (dto.dueDate !== undefined) data.dueDate = dto.dueDate ? new Date(dto.dueDate) : null;
  if (dto.currencyId !== undefined) data.currencyId = BigInt(dto.currencyId);
  if (dto.exchangeRate !== undefined) data.exchangeRate = new Prisma.Decimal(dto.exchangeRate);
  if (dto.priceMode !== undefined) data.priceMode = dto.priceMode as never;
  if (dto.description !== undefined) data.description = dto.description;
  if (dto.notes !== undefined) data.notes = dto.notes;
  if (dto.referenceNo !== undefined) data.referenceNo = dto.referenceNo;
  if (dto.referenceDate !== undefined) {
    data.referenceDate = dto.referenceDate ? new Date(dto.referenceDate) : null;
  }
  if (dto.receivableAccountId !== undefined) {
    data.receivableAccountId = toBigInt(dto.receivableAccountId);
  }
  if (dto.salesDeptId !== undefined) data.salesDeptId = toBigInt(dto.salesDeptId);
  if (dto.discountPercent !== undefined) {
    data.discountPercent =
      dto.discountPercent != null ? new Prisma.Decimal(dto.discountPercent) : null;
  }
  if (dto.legacyCode !== undefined) data.legacyCode = dto.legacyCode;
  if (dto.quotationId !== undefined) data.quotationId = toBigInt(dto.quotationId);
  if (dto.orderId !== undefined) data.orderId = toBigInt(dto.orderId);
  if (dto.proformaInvoiceId !== undefined)
    data.proformaInvoiceId = toBigInt(dto.proformaInvoiceId);
  return data;
}

/**
 * Existing-line -> DTO normalization (orig ~L285-298) so the merged line set
 * can be fed back into `computeOrderTotals`. Preserves the exact
 * `customFields: undefined` normalization line present in the original.
 */
export function mapExistingSlsPackingListLines(
  existing: ExistingPackingList,
): SlsPackingListLineDto[] {
  return existing.lines.map((l) => ({
    itemId: l.itemId.toString(),
    quantity: l.quantity.toString(),
    unitId: l.unitId.toString(),
    unitPrice: l.unitPrice.toString(),
    discountPercent: l.discountPercent?.toString(),
    discountAmount: l.discountAmount?.toString(),
    tax1Id: (l.tax1Id as bigint | null)?.toString(),
    tax2Id: (l.tax2Id as bigint | null)?.toString(),
    tax1Amount: l.tax1Amount?.toString(),
    tax2Amount: l.tax2Amount?.toString(),
    lineNo: l.lineNo,
    customFields: undefined,
  }));
}

/**
 * Build the header input for `computeOrderTotals` during update, merging DTO
 * header money fields with the existing row. Preserves the exact
 * `undefined` fallback of the original (null existing -> undefined, NOT '0').
 * Pure — no Prisma calls, no injected deps.
 */
export function buildSlsPackingListTotalsInput(
  dto: UpdateSlsPackingListDto,
  existing: ExistingPackingList,
): {
  discountAmount: string | undefined;
  tax1Amount: string | undefined;
  tax2Amount: string | undefined;
  otherCostAmount: string | undefined;
  discountPercent: string | undefined;
} {
  return {
    discountAmount:
      dto.discountAmount !== undefined
        ? dto.discountAmount
        : existing.discountAmount?.toString(),
    tax1Amount:
      dto.tax1Amount !== undefined ? dto.tax1Amount : existing.tax1Amount?.toString(),
    tax2Amount:
      dto.tax2Amount !== undefined ? dto.tax2Amount : existing.tax2Amount?.toString(),
    otherCostAmount:
      dto.otherCostAmount !== undefined
        ? dto.otherCostAmount
        : existing.otherCostAmount?.toString(),
    discountPercent: dto.discountPercent,
  };
}