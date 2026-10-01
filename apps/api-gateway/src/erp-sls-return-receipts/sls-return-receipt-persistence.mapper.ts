import { Prisma } from '@prisma/client';
import { CreateSlsReturnReceiptDto } from './dto/create-sls-return-receipt.dto';
import { UpdateSlsReturnReceiptDto } from './dto/update-sls-return-receipt.dto';
import { toBigInt, mapReturnReceiptLine, computeReturnReceiptTotals } from './sls-return-receipt.helpers';

/** Context values resolved inside the create transaction, passed into the data builder. */
export interface SlsReturnReceiptCreateContext {
  docNumber: string;
  wantAuto: boolean;
  fiscalPeriodId: bigint;
  dueDate: Date | null;
  actor: bigint | null;
  priceMode: 'TAX_EXCLUSIVE' | 'TAX_INCLUSIVE';
  rateById: Map<string, Prisma.Decimal>;
  header: { currencyId: string; exchangeRate: string };
}

/**
 * Build the Prisma `data` literal for create (excluding the `lines` create + `create` tx call,
 * which stay in the service to preserve the transaction boundary).
 */
export function buildSlsReturnReceiptCreateData(
  dto: CreateSlsReturnReceiptDto,
  ctx: SlsReturnReceiptCreateContext,
): Prisma.ErpSlsReturnReceiptCreateInput {
  const { subtotal, grandTotal, lines: computedLines, discountAmount, otherCostAmount } =
    computeReturnReceiptTotals(dto.lines, dto, ctx.rateById, ctx.priceMode);
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const data: any = {
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
    subtotal,
    discountPercent: dto.discountPercent != null ? new Prisma.Decimal(dto.discountPercent) : null,
    discountAmount: discountAmount,
    tax1Amount: dto.tax1Amount != null ? new Prisma.Decimal(dto.tax1Amount) : null,
    tax2Amount: dto.tax2Amount != null ? new Prisma.Decimal(dto.tax2Amount) : null,
    otherCostAmount: otherCostAmount,
    grandTotal,
    description: dto.description ?? null,
    notes: dto.notes ?? null,
    referenceNo: dto.referenceNo ?? null,
    referenceDate: dto.referenceDate ? new Date(dto.referenceDate) : null,
    receivableAccountId: toBigInt(dto.receivableAccountId),
    salesDeptId: toBigInt(dto.salesDeptId),
    invoiceId: toBigInt(dto.invoiceId),
    returnId: toBigInt(dto.returnId),
    settlementStatus: (dto.settlementStatus ?? 'OPEN') as never,
    status: 'DRAFT',
    postingStatus: 'UNPOSTED',
    legacyCode: dto.legacyCode ?? null,
    createdById: ctx.actor,
    updatedById: ctx.actor,
  };
  if (computedLines.length) {
    data.lines = {
      create: computedLines.map((l) => mapReturnReceiptLine(l, ctx.header)),
    };
  }
  return data as Prisma.ErpSlsReturnReceiptCreateInput;
}

/**
 * Build the update field-assignment patch (excluding totals, docDate/fiscalPeriod,
 * dueDate, and lines — those are resolved in-service to preserve transaction + gate semantics).
 * PRESERVES `invoice` / `return` relation `connect`/`disconnect` — NOT scalar FK.
 */
export function buildSlsReturnReceiptUpdatePatch(
  dto: UpdateSlsReturnReceiptDto,
  actor: bigint | null,
): Prisma.ErpSlsReturnReceiptUpdateInput {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
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
  if (dto.invoiceId !== undefined) {
    data.invoice = dto.invoiceId ? { connect: { id: BigInt(dto.invoiceId) } } : { disconnect: true };
  }
  if (dto.returnId !== undefined) {
    data.return = dto.returnId ? { connect: { id: BigInt(dto.returnId) } } : { disconnect: true };
  }
  if (dto.settlementStatus !== undefined) data.settlementStatus = dto.settlementStatus as never;
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
  return data as Prisma.ErpSlsReturnReceiptUpdateInput;
}

/** Normalize existing persisted lines back into the DTO-line shape (for recompute merge). */
export function mapExistingSlsReturnReceiptLines(
  lines: ReadonlyArray<{
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
  }>,
) {
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
 * Build the input passed to `computeReturnReceiptTotals` during update — merging dto overrides
 * with existing persisted scalar values for discount/tax/other-cost.
 */
export function buildSlsReturnReceiptTotalsInput(
  dto: UpdateSlsReturnReceiptDto,
  existing: {
    discountAmount: Prisma.Decimal | null;
    tax1Amount: Prisma.Decimal | null;
    tax2Amount: Prisma.Decimal | null;
    otherCostAmount: Prisma.Decimal | null;
  },
) {
  return {
    discountAmount:
      dto.discountAmount !== undefined ? dto.discountAmount : existing.discountAmount?.toString(),
    discountPercent: dto.discountPercent,
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