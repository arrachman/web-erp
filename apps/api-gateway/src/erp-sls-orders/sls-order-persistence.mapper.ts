import { Prisma } from '@prisma/client';
import { toBigInt, mapOrderLine } from './sls-order.helpers';
import { CreateSlsOrderDto, SlsOrderLineDto } from './dto/create-sls-order.dto';
import { UpdateSlsOrderDto } from './dto/update-sls-order.dto';

/** Context the service resolves (period/doc-number/totals/tx) before building the create literal. */
export interface SlsOrderCreateContext {
  actor: bigint | null;
  priceMode: 'TAX_EXCLUSIVE' | 'TAX_INCLUSIVE';
  fiscalPeriodId: bigint;
  docNumber: string;
  wantAuto: boolean;
  dueDate: Date | null;
  subtotal: Prisma.Decimal;
  grandTotal: Prisma.Decimal;
  /** Resolved header discount amount (explicit, or derived from percent). Null = not set. */
  discountAmount: Prisma.Decimal | null;
  /** Resolved header other-cost amount (explicit, or derived from percent). Null = not set. */
  otherCostAmount: Prisma.Decimal | null;
  computedLines: SlsOrderLineDto[];
  header: { currencyId: string; exchangeRate: string };
}

/**
 * Build the Prisma `data` literal for `erpSlsOrder.create`. Pure — period
 * resolution, doc-number generation, totals computation, and the transaction
 * itself stay in the service. Header customFields mirrors the inline literal:
 * truthy → InputJsonValue, falsy → undefined (no write). Line customFields is
 * applied per-line via `mapOrderLine` (undefined = no write).
 */
export function buildSlsOrderCreateData(
  dto: CreateSlsOrderDto,
  ctx: SlsOrderCreateContext,
): Prisma.ErpSlsOrderUncheckedCreateInput {
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
    status: 'DRAFT',
    postingStatus: 'UNPOSTED',
    legacyCode: dto.legacyCode ?? null,
    customFields: dto.customFields ? (dto.customFields as Prisma.InputJsonValue) : undefined,
    createdById: ctx.actor,
    updatedById: ctx.actor,
    lines: ctx.computedLines.length
      ? { create: ctx.computedLines.map((l) => mapOrderLine(l, ctx.header)) }
      : undefined,
  };
}

/** Subset of a persisted ErpSlsOrderLine row needed to normalize back into a line DTO. */
export interface ExistingSlsOrderLine {
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
  customFields: unknown;
}

/**
 * Normalize persisted order-line rows back into the line-DTO shape so the
 * update path can merge caller-supplied lines with existing ones and recompute
 * totals uniformly. Line customFields preserves the original convention:
 * null/missing → undefined (no change downstream), truthy object → kept.
 */
export function mapExistingSlsOrderLines(lines: ExistingSlsOrderLine[]): SlsOrderLineDto[] {
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
    customFields: (l.customFields as Record<string, unknown> | null) ?? undefined,
  }));
}

/** Existing header money fields (decimal | null) used to recompute totals. */
export interface ExistingSlsOrderTotals {
  discountAmount: Prisma.Decimal | null;
  tax1Amount: Prisma.Decimal | null;
  tax2Amount: Prisma.Decimal | null;
  otherCostAmount: Prisma.Decimal | null;
}

/**
 * Merge the update DTO's header money fields with the existing row so
 * `computeOrderTotals` recomputes from the same effective values the original
 * inline block used. Each field is `string | undefined` — undefined (not '0') so
 * computeOrderTotals treats a missing value as null/percent-fallback, matching
 * the prior inline behavior exactly. Pure.
 */
export function buildSlsOrderTotalsInput(
  dto: UpdateSlsOrderDto,
  existing: ExistingSlsOrderTotals,
): {
  discountAmount: string | undefined;
  discountPercent: string | undefined;
  tax1Amount: string | undefined;
  tax2Amount: string | undefined;
  otherCostAmount: string | undefined;
} {
  return {
    discountAmount:
      dto.discountAmount !== undefined ? dto.discountAmount : existing.discountAmount?.toString(),
    discountPercent: dto.discountPercent,
    tax1Amount:
      dto.tax1Amount !== undefined ? dto.tax1Amount : existing.tax1Amount?.toString(),
    tax2Amount:
      dto.tax2Amount !== undefined ? dto.tax2Amount : existing.tax2Amount?.toString(),
    otherCostAmount:
      dto.otherCostAmount !== undefined ? dto.otherCostAmount : existing.otherCostAmount?.toString(),
  };
}

/**
 * Build the scalar/field-assignment portion of the update `data` object for
 * `erpSlsOrder.update`. Pure — docDate/fiscalPeriodId resolution (needs the
 * transaction client), totals assignment, dueDate recompute, and line
 * delete+create stay in the service. Header customFields preserves the exact
 * semantics: undefined = no change (skipped entirely), truthy → InputJsonValue,
 * explicit null/falsey → Prisma.DbNull (writes null into the JSONB column).
 * `any`-typed branches from the original inline block are intentionally NOT
 * tightened here.
 */
export function buildSlsOrderUpdatePatch(
  dto: UpdateSlsOrderDto,
  actor: bigint | null,
): Prisma.ErpSlsOrderUpdateInput {
  const data: Prisma.ErpSlsOrderUpdateInput = { updatedById: actor };
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
    data.discountPercent = dto.discountPercent != null ? new Prisma.Decimal(dto.discountPercent) : null;
  }
  if (dto.legacyCode !== undefined) data.legacyCode = dto.legacyCode;
  if (dto.tax1Amount !== undefined) {
    data.tax1Amount = dto.tax1Amount != null ? new Prisma.Decimal(dto.tax1Amount) : null;
  }
  if (dto.tax2Amount !== undefined) {
    data.tax2Amount = dto.tax2Amount != null ? new Prisma.Decimal(dto.tax2Amount) : null;
  }
  if (dto.customFields !== undefined) {
    data.customFields = dto.customFields ? (dto.customFields as Prisma.InputJsonValue) : Prisma.DbNull;
  }
  return data;
}