import { Prisma } from '@prisma/client';
import { CreateSlsProformaInvoiceDto } from './dto/create-sls-proforma-invoice.dto';
import { UpdateSlsProformaInvoiceDto } from './dto/update-sls-proforma-invoice.dto';
import { SlsProformaInvoiceLineDto } from './dto/create-sls-proforma-invoice.dto';
import {
  toBigInt,
  mapProformaInvoiceLine,
  OrderTotalsResult,
} from './sls-proforma-invoice.helpers';

/** Context object handed to the create builder (assembled by the service). */
export interface SlsProformaInvoiceCreateContext {
  docNumber: string;
  wantAuto: boolean;
  fiscalPeriodId: bigint;
  actor: bigint | null;
  priceMode: 'TAX_EXCLUSIVE' | 'TAX_INCLUSIVE';
  dueDate: Date | null;
  header: { currencyId: string; exchangeRate: string };
  totals: OrderTotalsResult;
}

/**
 * Build the Prisma `data` literal for `erpSlsProformaInvoice.create`.
 * Pure synchronous transform — no Prisma calls, no injected deps.
 * Period lookup, doc-number gen, tax loading, AND the `create` transaction
 * call stay in the service; this only returns the data object.
 *
 * Field assignments mirror the original create literal exactly (scalar FKs via
 * `toBigInt` / `BigInt`; relations are NOT used here — connect/disconnect only
 * appears in the update patch builder).
 */
export function buildSlsProformaInvoiceCreateData(
  dto: CreateSlsProformaInvoiceDto,
  ctx: SlsProformaInvoiceCreateContext,
): Prisma.ErpSlsProformaInvoiceUncheckedCreateInput {
  const { subtotal, grandTotal, lines: computedLines, discountAmount, otherCostAmount } =
    ctx.totals;
  return {
    code: ctx.docNumber,
    docNumber: ctx.docNumber,
    autoNumber: ctx.wantAuto ? ctx.docNumber : null,
    branchId: BigInt(dto.branchId),
    locationId: toBigInt(dto.locationId),
    warehouseId: toBigInt(dto.warehouseId),
    docDate: new Date(dto.docDate),
    fiscalPeriodId: ctx.fiscalPeriodId,
    customerId: toBigInt(dto.customerId),
    paymentTermId: toBigInt(dto.paymentTermId),
    dueDate: ctx.dueDate,
    currencyId: BigInt(dto.currencyId),
    exchangeRate: new Prisma.Decimal(dto.exchangeRate),
    priceMode: ctx.priceMode as never,
    quotationId: toBigInt(dto.quotationId),
    orderId: toBigInt(dto.orderId),
    subtotal,
    discountPercent: dto.discountPercent != null ? new Prisma.Decimal(dto.discountPercent) : null,
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
    status: 'DRAFT',
    postingStatus: 'UNPOSTED',
    legacyCode: dto.legacyCode ?? null,
    createdById: ctx.actor,
    updatedById: ctx.actor,
    lines: computedLines.length
      ? { create: computedLines.map((l) => mapProformaInvoiceLine(l, ctx.header)) }
      : undefined,
  };
}

/**
 * Build the synchronous update field assignments for
 * `erpSlsProformaInvoice.update`. Pure transform — no Prisma calls, no async.
 *
 * Mirrors the original update literal field-for-field: most FK columns stay as
 * scalar assignments (`branchId`/`locationId`/…) using `BigInt` / `toBigInt`.
 * CRITICAL: only `quotation` / `order` use relation `connect` / `disconnect`
 * (NOT scalar FK) — preserved exactly; do NOT convert them to scalar FKs.
 * Field-by-field `!== undefined` gates are preserved; `any` types not tightened.
 */
export function buildSlsProformaInvoiceUpdatePatch(
  dto: UpdateSlsProformaInvoiceDto,
  actor: bigint | null,
): Prisma.ErpSlsProformaInvoiceUpdateInput {
  const data: Prisma.ErpSlsProformaInvoiceUpdateInput = { updatedById: actor };
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
  if (dto.quotationId !== undefined) {
    data.quotation = dto.quotationId
      ? { connect: { id: BigInt(dto.quotationId) } }
      : { disconnect: true };
  }
  if (dto.orderId !== undefined) {
    data.order = dto.orderId
      ? { connect: { id: BigInt(dto.orderId) } }
      : { disconnect: true };
  }
  if (dto.discountPercent !== undefined) {
    data.discountPercent = dto.discountPercent != null ? new Prisma.Decimal(dto.discountPercent) : null;
  }
  if (dto.legacyCode !== undefined) data.legacyCode = dto.legacyCode;
  return data;
}

/** Shape of an existing proforma-invoice line as read by `findRaw` (Prisma payload). */
type ExistingLine = {
  itemId: bigint;
  quantity: Prisma.Decimal;
  unitId: bigint;
  unitPrice: Prisma.Decimal;
  discountPercent: Prisma.Decimal | null;
  discountAmount: Prisma.Decimal | null;
  tax1Id: bigint | null;
  tax2Id: bigint | null;
  tax1Amount: Prisma.Decimal | null;
  tax2Amount: Prisma.Decimal | null;
  lineNo: number;
};

/**
 * Normalize existing persisted lines back into the DTO shape used by
 * `computeOrderTotals` for totals recompute in the update path.
 * Pure synchronous transform — no Prisma calls.
 */
export function mapExistingSlsProformaInvoiceLines(
  lines: ExistingLine[],
): SlsProformaInvoiceLineDto[] {
  return lines.map((l) => ({
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
  }));
}

/** Shape of the existing header fields read by `findRaw` needed for totals fallback. */
export interface SlsProformaInvoiceExistingHeader {
  discountAmount: Prisma.Decimal | null;
  tax1Amount: Prisma.Decimal | null;
  tax2Amount: Prisma.Decimal | null;
  otherCostAmount: Prisma.Decimal | null;
}

/**
 * Assemble the header argument for `computeOrderTotals` in the update path:
 * each amount field falls back to the existing value stringified when the DTO
 * omits it. Pure synchronous transform — no Prisma calls.
 */
export function buildSlsProformaInvoiceTotalsInput(
  dto: UpdateSlsProformaInvoiceDto,
  existing: SlsProformaInvoiceExistingHeader,
): {
  discountAmount: string | undefined;
  tax1Amount: string | undefined;
  tax2Amount: string | undefined;
  otherCostAmount: string | undefined;
} {
  return {
    discountAmount:
      dto.discountAmount !== undefined ? dto.discountAmount : existing.discountAmount?.toString(),
    tax1Amount:
      dto.tax1Amount !== undefined ? dto.tax1Amount : existing.tax1Amount?.toString(),
    tax2Amount:
      dto.tax2Amount !== undefined ? dto.tax2Amount : existing.tax2Amount?.toString(),
    otherCostAmount:
      dto.otherCostAmount !== undefined
        ? dto.otherCostAmount
        : existing.otherCostAmount?.toString(),
  };
}