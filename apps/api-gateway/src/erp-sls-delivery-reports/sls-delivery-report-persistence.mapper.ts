import { Prisma } from '@prisma/client';
import {
  CreateSlsDeliveryReportDto,
  SlsDeliveryReportLineDto,
} from './dto/create-sls-delivery-report.dto';
import { UpdateSlsDeliveryReportDto } from './dto/update-sls-delivery-report.dto';
import { toBigInt, mapDeliveryReportLine } from './sls-delivery-report.helpers';

/**
 * Persistence-layer mapping helpers extracted from ErpSlsDeliveryReportsService.
 * Pure transforms — no Prisma calls, no injected deps.
 *
 * Anything touching the tx (resolvePeriod, genDocNumber, the create/update tx
 * call itself) stays in the service to preserve transaction boundaries.
 */

export interface DeliveryReportCreateContext {
  docNumber: string;
  wantAuto: boolean;
  fiscalPeriodId: bigint;
  dueDate: Date | null;
  subtotal: Prisma.Decimal;
  discountAmount: Prisma.Decimal | null;
  otherCostAmount: Prisma.Decimal | null;
  grandTotal: Prisma.Decimal;
  computedLines: SlsDeliveryReportLineDto[];
  header: { currencyId: string; exchangeRate: string };
}

/**
 * Build the `data` literal for `tx.erpSlsDeliveryReport.create({ data })`.
 * Caller retains the create tx call, period resolution, doc-number gen,
 * and tax/total computation.
 */
export function buildSlsDeliveryReportCreateData(
  dto: CreateSlsDeliveryReportDto,
  ctx: DeliveryReportCreateContext,
  actor: bigint | null,
): Prisma.ErpSlsDeliveryReportUncheckedCreateInput {
  const {
    docNumber,
    wantAuto,
    fiscalPeriodId,
    dueDate,
    subtotal,
    discountAmount,
    otherCostAmount,
    grandTotal,
    computedLines,
    header,
  } = ctx;
  const priceMode = (dto.priceMode ?? 'TAX_EXCLUSIVE') as 'TAX_EXCLUSIVE' | 'TAX_INCLUSIVE';
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
    deliveryOrderId: toBigInt(dto.deliveryOrderId),
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
      ? { create: computedLines.map((l) => mapDeliveryReportLine(l, header)) }
      : undefined,
  };
}

/**
 * Apply update field mappings onto the unchecked update-input object.
 * Caller retains the `data` object, the docDate/fiscalPeriodId block, totals
 * assignment, lines write, and the update tx call.
 * `deliveryOrderId` scalar behavior is preserved — no relation conversion.
 */
export function buildSlsDeliveryReportUpdatePatch(
  dto: UpdateSlsDeliveryReportDto,
  data: Prisma.ErpSlsDeliveryReportUncheckedUpdateInput,
): void {
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
  if (dto.deliveryOrderId !== undefined) data.deliveryOrderId = toBigInt(dto.deliveryOrderId);
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
  if (dto.tax1Amount !== undefined) {
    data.tax1Amount = dto.tax1Amount != null ? new Prisma.Decimal(dto.tax1Amount) : null;
  }
  if (dto.tax2Amount !== undefined) {
    data.tax2Amount = dto.tax2Amount != null ? new Prisma.Decimal(dto.tax2Amount) : null;
  }
}

/**
 * Normalize existing DB-tier lines → the same shape as `dto.lines` so they can
 * be re-fed into `computeDeliveryReportTotals` when no explicit lines patch
 * was supplied.
 */
export function mapExistingSlsDeliveryReportLines(
  lines: Array<{
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
): SlsDeliveryReportLineDto[] {
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