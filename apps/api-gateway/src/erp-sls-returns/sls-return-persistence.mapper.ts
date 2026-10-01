import { Prisma } from '@prisma/client';
import { toBigInt, mapReturnLine } from './sls-return.helpers';
import { CreateSlsReturnDto, SlsReturnLineDto } from './dto/create-sls-return.dto';
import { UpdateSlsReturnDto } from './dto/update-sls-return.dto';

/**
 * Persistence mapping helpers for ErpSlsReturnsService.
 *
 * Pure functions only — no Prisma client access, no injected deps.
 * Transaction boundaries, doc-number, period resolution, and total
 * computation stay in the service; this module only shapes create/update
 * payloads and normalizes existing lines.
 */

interface CreateContext {
  docNumber: string;
  wantAuto: boolean;
  fiscalPeriodId: bigint;
  dueDate: Date | null;
  priceMode: 'TAX_EXCLUSIVE' | 'TAX_INCLUSIVE';
  subtotal: Prisma.Decimal;
  discountAmount: Prisma.Decimal | null;
  otherCostAmount: Prisma.Decimal | null;
  grandTotal: Prisma.Decimal;
  actor: bigint | null;
  computedLines: SlsReturnLineDto[];
  header: { currencyId: string; exchangeRate: string };
}

/**
 * Build the `data` literal for `tx.erpSlsReturn.create({ data })`.
 * Period/doc-number/tax/`create` tx remain in the service.
 */
export function buildSlsReturnCreateData(
  dto: CreateSlsReturnDto,
  ctx: CreateContext,
): Prisma.ErpSlsReturnUncheckedCreateInput {
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
    subtotal: ctx.subtotal,
    discountPercent: dto.discountPercent != null ? new Prisma.Decimal(dto.discountPercent) : null,
    discountAmount: ctx.discountAmount,
    tax1Amount: dto.tax1Amount != null ? new Prisma.Decimal(dto.tax1Amount) : null,
    tax2Amount: dto.tax2Amount != null ? new Prisma.Decimal(dto.tax2Amount) : null,
    otherCostAmount: ctx.otherCostAmount,
    grandTotal: ctx.grandTotal,
    description: dto.description ?? null,
    notes: dto.notes ?? null,
    referenceNo: dto.referenceNo ?? null,
    referenceDate: dto.referenceDate ? new Date(dto.referenceDate) : null,
    receivableAccountId: toBigInt(dto.receivableAccountId),
    salesDeptId: toBigInt(dto.salesDeptId),
    invoiceId: toBigInt(dto.invoiceId),
    remainingAccountId: toBigInt(dto.remainingAccountId),
    settlementStatus: (dto.settlementStatus ?? 'OPEN') as never,
    status: 'DRAFT',
    postingStatus: 'UNPOSTED',
    legacyCode: dto.legacyCode ?? null,
    createdById: ctx.actor,
    updatedById: ctx.actor,
    lines: ctx.computedLines.length
      ? { create: ctx.computedLines.map((l) => mapReturnLine(l, ctx.header)) }
      : undefined,
  };
}

/** Existing line shape from `include: { lines }` (Prisma row). */
interface ExistingLine {
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
}

/**
 * Normalize existing Prisma lines → DTO-shape for merge with incoming `dto.lines`.
 * Mirrors the inline `existing.lines.map(...)` block in the original update method.
 */
export function mapExistingSlsReturnLines(lines: ExistingLine[]): SlsReturnLineDto[] {
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

export interface ReturnTotalsInput {
  discountAmount: string | undefined;
  discountPercent: string | undefined;
  tax1Amount: string | undefined;
  tax2Amount: string | undefined;
  otherCostAmount: string | undefined;
}

/**
 * Build the `computeReturnTotals` header input for the update path, merging
 * dto values with existing fallbacks. Non-trivial because it reconciles four
 * amount fields with `undefined`/value semantics. Preserves the original's
 * `undefined` fallback (not `'0'`) so `computeReturnTotals` handles nulls.
 */
export function buildSlsReturnTotalsInput(
  dto: UpdateSlsReturnDto,
  existing: {
    discountAmount: Prisma.Decimal | null;
    tax1Amount: Prisma.Decimal | null;
    tax2Amount: Prisma.Decimal | null;
    otherCostAmount: Prisma.Decimal | null;
  },
): ReturnTotalsInput {
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

/**
 * Build the update field-assignments for `tx.erpSlsReturn.update({ data })`.
 * PRESERVES the `invoice` relation connect/disconnect AND the
 * `settlementStatus` enum handling — scalar FK conversion is intentionally
 * avoided. `any` types are NOT tightened (parity with original).
 */
export function buildSlsReturnUpdatePatch(dto: UpdateSlsReturnDto, actor: bigint | null): any {
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
  if (dto.remainingAccountId !== undefined) data.remainingAccountId = toBigInt(dto.remainingAccountId);
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
  return data;
}