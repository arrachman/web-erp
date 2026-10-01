import { Prisma } from '@prisma/client';
import { CreateSlsInvoiceDto } from './dto/create-sls-invoice.dto';
import { UpdateSlsInvoiceDto } from './dto/update-sls-invoice.dto';
import { SlsInvoiceLineDto } from './dto/create-sls-invoice.dto';
import { toBigInt, mapInvoiceLine } from './sls-invoice.helpers';

/** Invoice row returned by `findRaw` (include: lines ordered by lineNo asc). */
type InvoiceWithLines = Prisma.ErpSlsInvoiceGetPayload<{
  include: { lines: { orderBy: { lineNo: 'asc' } } };
}>;
type InvoiceLineRow = InvoiceWithLines['lines'][number];

/** Header context used by `mapInvoiceLine` for currency awareness. */
interface LineHeader {
  currencyId: string;
  exchangeRate: string;
}

type PriceMode = 'TAX_EXCLUSIVE' | 'TAX_INCLUSIVE';

interface CreateContext {
  docNumber: string;
  wantAuto: boolean;
  fiscalPeriodId: bigint;
  dueDate: Date | null;
  actor: bigint | null;
  priceMode: PriceMode;
  header: LineHeader;
  subtotal: Prisma.Decimal;
  grandTotal: Prisma.Decimal;
  discountAmount: Prisma.Decimal | null;
  otherCostAmount: Prisma.Decimal | null;
  computedLines: SlsInvoiceLineDto[];
}

/**
 * Build the `data` object for the Prisma `erpSlsInvoice.create` call.
 *
 * Pure: performs no Prisma calls and reads no injected deps. Period lookup,
 * document-number generation, tax loading, and the `create` transaction call
 * stay in the service — this only assembles the create payload.
 */
export function buildSlsInvoiceCreateData(
  dto: CreateSlsInvoiceDto,
  ctx: CreateContext,
): Prisma.ErpSlsInvoiceUncheckedCreateInput {
  const {
    docNumber,
    wantAuto,
    fiscalPeriodId,
    dueDate,
    actor,
    priceMode,
    header,
    subtotal,
    grandTotal,
    discountAmount,
    otherCostAmount,
    computedLines,
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
    orderId: toBigInt(dto.orderId),
    deliveryOrderId: toBigInt(dto.deliveryOrderId),
    advanceId: toBigInt(dto.advanceId),
    advanceAmount: dto.advanceAmount != null ? new Prisma.Decimal(dto.advanceAmount) : null,
    taxInvoiceNo: dto.taxInvoiceNo ?? null,
    channel: (dto.channel ?? 'STANDARD') as never,
    isOpeningBalance: dto.isOpeningBalance ?? false,
    settlementStatus: 'UNSETTLED' as never,
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
    createdById: actor,
    updatedById: actor,
    lines: computedLines.length
      ? { create: computedLines.map((l) => mapInvoiceLine(l, header)) }
      : undefined,
  };
}

/**
 * Build the synchronous update field assignments for the Prisma
 * `erpSlsInvoice.update` call.
 *
 * Pure: performs no Prisma calls. Field-by-field `!== undefined` gates and
 * `null`/`Decimal` semantics are preserved exactly from the original service.
 * Does NOT touch lines (line delete/recreate is gated separately in the
 * service). The `docDate`/`fiscalPeriodId` resolution is appended by the
 * service because it needs the transaction client.
 */
export function buildSlsInvoiceUpdatePatch(
  dto: UpdateSlsInvoiceDto,
  actor: bigint | null,
): Prisma.ErpSlsInvoiceUncheckedUpdateInput {
  const data: Prisma.ErpSlsInvoiceUncheckedUpdateInput = { updatedById: actor };
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
  if (dto.orderId !== undefined) data.orderId = toBigInt(dto.orderId);
  if (dto.deliveryOrderId !== undefined) data.deliveryOrderId = toBigInt(dto.deliveryOrderId);
  if (dto.advanceId !== undefined) data.advanceId = toBigInt(dto.advanceId);
  if (dto.advanceAmount !== undefined) {
    data.advanceAmount = dto.advanceAmount != null ? new Prisma.Decimal(dto.advanceAmount) : null;
  }
  if (dto.taxInvoiceNo !== undefined) data.taxInvoiceNo = dto.taxInvoiceNo;
  if (dto.channel !== undefined) data.channel = dto.channel as never;
  if (dto.isOpeningBalance !== undefined) data.isOpeningBalance = dto.isOpeningBalance;
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
    data.discountPercent = dto.discountPercent != null ? new Prisma.Decimal(dto.discountPercent) : null;
  }
  if (dto.legacyCode !== undefined) data.legacyCode = dto.legacyCode;
  if (dto.tax1Amount !== undefined) {
    data.tax1Amount = dto.tax1Amount != null ? new Prisma.Decimal(dto.tax1Amount) : null;
  }
  if (dto.tax2Amount !== undefined) {
    data.tax2Amount = dto.tax2Amount != null ? new Prisma.Decimal(dto.tax2Amount) : null;
  }
  return data;
}

/**
 * Normalize existing persisted invoice lines back into DTO-shaped lines so
 * `computeInvoiceTotals` can recompute from the merged set (dto-supplied +
 * persisted). Used by the update path before recompute.
 *
 * Pure: maps DB row values → string DTO fields, preserving nullability.
 */
export function mapExistingSlsInvoiceLines(lines: InvoiceLineRow[]): SlsInvoiceLineDto[] {
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

/**
 * Assemble the `header` argument for `computeInvoiceTotals` in the update
 * path. The four amount/percent fields fall back to the existing persisted
 * stringified values (decimal → string) when the dto omits them, so recalc
 * works for partial updates. `discountPercent` is passed straight from dto
 * (undefined if absent — `computeInvoiceTotals` treats it as absent).
 *
 * Pure: string normalization only, no Prisma calls.
 */
export function buildSlsInvoiceTotalsInput(
  dto: UpdateSlsInvoiceDto,
  existing: {
    discountAmount: Prisma.Decimal | null;
    tax1Amount: Prisma.Decimal | null;
    tax2Amount: Prisma.Decimal | null;
    otherCostAmount: Prisma.Decimal | null;
  },
): {
  discountAmount?: string;
  discountPercent?: string;
  tax1Amount?: string;
  tax2Amount?: string;
  otherCostAmount?: string;
} {
  const discountAmount =
    dto.discountAmount !== undefined ? dto.discountAmount : existing.discountAmount?.toString();
  const tax1Amount =
    dto.tax1Amount !== undefined ? dto.tax1Amount : existing.tax1Amount?.toString();
  const tax2Amount =
    dto.tax2Amount !== undefined ? dto.tax2Amount : existing.tax2Amount?.toString();
  const otherCostAmount =
    dto.otherCostAmount !== undefined
      ? dto.otherCostAmount
      : existing.otherCostAmount?.toString();
  return {
    discountAmount,
    discountPercent: dto.discountPercent,
    tax1Amount,
    tax2Amount,
    otherCostAmount,
  };
}

