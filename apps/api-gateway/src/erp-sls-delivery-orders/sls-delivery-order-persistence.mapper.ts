import { Prisma } from '@prisma/client';
import { CreateSlsDeliveryOrderDto } from './dto/create-sls-delivery-order.dto';
import { UpdateSlsDeliveryOrderDto } from './dto/update-sls-delivery-order.dto';
import { SlsDeliveryOrderLineDto } from './dto/create-sls-delivery-order.dto';
import { toBigInt, mapDeliveryOrderLine } from './sls-delivery-order.helpers';

/** Delivery-order row returned by `findRaw` (include: lines ordered by lineNo asc). */
type DeliveryOrderWithLines = Prisma.ErpSlsDeliveryOrderGetPayload<{
  include: { lines: { orderBy: { lineNo: 'asc' } } };
}>;
type DeliveryOrderLineRow = DeliveryOrderWithLines['lines'][number];

/** Header context used by `mapDeliveryOrderLine` for currency awareness. */
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
  computedLines: SlsDeliveryOrderLineDto[];
}

/**
 * Build the `data` object for the Prisma `erpSlsDeliveryOrder.create` call.
 *
 * Pure: performs no Prisma calls and reads no injected deps. Period lookup,
 * document-number generation, tax loading, and the `create` transaction call
 * stay in the service — this only assembles the create payload.
 */
export function buildSlsDeliveryOrderCreateData(
  dto: CreateSlsDeliveryOrderDto,
  ctx: CreateContext,
): Prisma.ErpSlsDeliveryOrderUncheckedCreateInput {
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
    shippingDeptId: toBigInt(dto.shippingDeptId),
    orderId: toBigInt(dto.orderId),
    packingListId: toBigInt(dto.packingListId),
    proformaInvoiceId: toBigInt(dto.proformaInvoiceId),
    status: 'DRAFT',
    postingStatus: 'UNPOSTED',
    legacyCode: dto.legacyCode ?? null,
    createdById: actor,
    updatedById: actor,
    lines: computedLines.length
      ? { create: computedLines.map((l) => mapDeliveryOrderLine(l, header)) }
      : undefined,
  };
}

/**
 * Build the synchronous update field assignments for the Prisma
 * `erpSlsDeliveryOrder.update` call.
 *
 * Pure: performs no Prisma calls. Field-by-field `!== undefined` gates and
 * `null`/`Decimal` semantics are preserved exactly from the original service.
 * Does NOT touch lines (line delete/recreate is gated separately in the
 * service). The `docDate`/`fiscalPeriodId` resolution is appended by the
 * service because it needs the transaction client.
 *
 * Note: the original service declared this object as `any`; that loose typing
 * is intentionally preserved here — tightening it is outside the scope of this
 * pure restructure.
 */
export function buildSlsDeliveryOrderUpdatePatch(
  dto: UpdateSlsDeliveryOrderDto,
  actor: bigint | null,
): any {
  const data: any = { updatedById: actor };
  if (dto.docNumber !== undefined) {
    data.docNumber = dto.docNumber;
    data.code = dto.docNumber;
  }
  if (dto.branchId !== undefined) data.branchId = BigInt(dto.branchId);
  if (dto.locationId !== undefined) data.locationId = toBigInt(dto.locationId);
  if (dto.warehouseId !== undefined) data.warehouseId = toBigInt(dto.warehouseId);
  if (dto.customerId !== undefined) data.customerId = BigInt(dto.customerId);
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
  if (dto.shippingDeptId !== undefined) data.shippingDeptId = toBigInt(dto.shippingDeptId);
  if (dto.discountPercent !== undefined) {
    data.discountPercent = dto.discountPercent != null ? new Prisma.Decimal(dto.discountPercent) : null;
  }
  if (dto.legacyCode !== undefined) data.legacyCode = dto.legacyCode;
  if (dto.orderId !== undefined) data.orderId = toBigInt(dto.orderId);
  if (dto.packingListId !== undefined) data.packingListId = toBigInt(dto.packingListId);
  if (dto.proformaInvoiceId !== undefined) data.proformaInvoiceId = toBigInt(dto.proformaInvoiceId);
  if (dto.tax1Amount !== undefined) {
    data.tax1Amount = dto.tax1Amount != null ? new Prisma.Decimal(dto.tax1Amount) : null;
  }
  if (dto.tax2Amount !== undefined) {
    data.tax2Amount = dto.tax2Amount != null ? new Prisma.Decimal(dto.tax2Amount) : null;
  }
  return data;
}

/**
 * Normalize existing persisted delivery-order lines back into DTO-shaped lines
 * so `computeOrderTotals` can recompute from the merged set (dto-supplied +
 * persisted). Used by the update path before recompute.
 *
 * Pure: maps DB row values → string DTO fields, preserving nullability. The
 * `customFields: undefined` sentinel is preserved verbatim from the original
 * service so downstream spread semantics stay identical.
 */
export function mapExistingSlsDeliveryOrderLines(
  lines: DeliveryOrderLineRow[],
): SlsDeliveryOrderLineDto[] {
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
    customFields: undefined,
  }));
}

/**
 * Assemble the `header` argument for `computeOrderTotals` in the update path.
 * The four amount/percent fields fall back to the existing persisted stringified
 * values (decimal → string) when the dto omits them, so recalc works for
 * partial updates. `discountPercent` is passed straight from dto (undefined if
 * absent — `computeOrderTotals` treats it as absent).
 *
 * Pure: string normalization only, no Prisma calls.
 */
export function buildSlsDeliveryOrderTotalsInput(
  dto: UpdateSlsDeliveryOrderDto,
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