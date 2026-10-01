import { BadRequestException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { InvStockAdjustmentLineDto } from './dto/create-inv-stock-adjustment.dto';
import { QueryInvStockAdjustmentsDto } from './dto/query-inv-stock-adjustments.dto';
import { InvStockAdjustmentTransitionAction as A } from './dto/transition-inv-stock-adjustment.dto';

export function toBigInt(v?: string | null): bigint | null {
  if (v === undefined || v === null || v === '') return null;
  return BigInt(v);
}

const dec = (v?: string | null) => new Prisma.Decimal(v ?? 0);

/** Statuses where header/lines may still be edited (§2.7 state machine). */
export const EDITABLE = new Set(['DRAFT', 'NEED_APPROVE', 'REJECTED']);

/** valid (status, action) → next status. POST/REOPEN handled separately. */
export const NEXT: Record<string, Partial<Record<A, string>>> = {
  DRAFT: { [A.SUBMIT]: 'NEED_APPROVE' },
  REJECTED: { [A.SUBMIT]: 'NEED_APPROVE' },
  NEED_APPROVE: { [A.APPROVE]: 'APPROVED', [A.REJECT]: 'REJECTED' },
  APPROVED: { [A.POST]: 'POSTED', [A.REOPEN]: 'DRAFT' },
  POSTED: { [A.REOPEN]: 'DRAFT' },
};

/** Document-numbering code for stock adjustments (own running number). */
export const DOC_CODE = 'SA';

export function buildInvAdjustmentWhere(
  query: QueryInvStockAdjustmentsDto,
): Prisma.ErpInvStockAdjustmentWhereInput {
  const where: Prisma.ErpInvStockAdjustmentWhereInput = { deletedAt: null };

  if (query.status) where.status = query.status as never;
  if (query.branchId) where.branchId = BigInt(query.branchId);
  if (query.warehouseId) where.warehouseId = BigInt(query.warehouseId);
  if (query.createdById) where.createdById = BigInt(query.createdById);
  if (query.dateFrom || query.dateTo) {
    where.adjustmentDate = {
      ...(query.dateFrom ? { gte: new Date(query.dateFrom) } : {}),
      ...(query.dateTo ? { lte: new Date(query.dateTo) } : {}),
    };
  }
  if (query.search?.trim()) {
    const q = query.search.trim();
    where.OR = [
      { docNumber: { contains: q, mode: 'insensitive' } },
      { description: { contains: q, mode: 'insensitive' } },
    ];
  }
  return where;
}

/**
 * Resolved per-line GL accounts (inventory + contra) — both NOT NULL on the line.
 * Resolution precedence (per §inventory accounts):
 *   inventoryAccountId = line ?? item.inventoryAccountId ?? Setting.defaultInventoryAccountId ?? throw
 *   contraAccountId    = line ?? Setting.defaultAdjustmentContraAccountId ?? throw
 */
export type ResolvedAccounts = {
  inventoryAccountId: bigint;
  contraAccountId: bigint;
  itemUnitCost: Prisma.Decimal | null;
};

async function readSetting(
  prisma: PrismaService,
  key: string,
): Promise<bigint | null> {
  const row = await prisma.erpSetting.findUnique({
    where: { module_group_key: { module: 'inventory', group: 'accounts', key } },
    select: { value: true },
  });
  if (!row?.value) return null;
  try {
    return BigInt(row.value);
  } catch {
    return null;
  }
}

/**
 * Resolve inventory + contra accounts for each distinct itemId used in the lines.
 * Fetches items (md_items) and inventory settings once, then returns a per-item
 * resolver closure. Throws BadRequestException with a localised message when a
 * required account cannot be resolved from any source.
 */
export async function resolveLineAccounts(
  prisma: PrismaService,
  lines: InvStockAdjustmentLineDto[],
): Promise<(line: InvStockAdjustmentLineDto) => ResolvedAccounts> {
  const itemIds = [...new Set(lines.map((l) => BigInt(l.itemId)))];
  const items = await prisma.erpItem.findMany({
    where: { id: { in: itemIds } },
    select: { id: true, code: true, inventoryAccountId: true, averageCost: true },
  });
  const itemMap = new Map(items.map((i) => [i.id.toString(), i]));

  const [defaultInventoryAccountId, defaultContraAccountId] = await Promise.all([
    readSetting(prisma, 'defaultInventoryAccountId'),
    readSetting(prisma, 'defaultAdjustmentContraAccountId'),
  ]);

  return (line: InvStockAdjustmentLineDto): ResolvedAccounts => {
    const item = itemMap.get(BigInt(line.itemId).toString());
    const inventoryAccountId =
      toBigInt(line.inventoryAccountId) ??
      item?.inventoryAccountId ??
      defaultInventoryAccountId;
    if (inventoryAccountId == null) {
      throw new BadRequestException(
        `Akun persediaan belum diset untuk item ${item?.code ?? line.itemId} — set di master item atau Setting inventory.`,
      );
    }
    const contraAccountId = toBigInt(line.contraAccountId) ?? defaultContraAccountId;
    if (contraAccountId == null) {
      throw new BadRequestException(
        'Akun lawan penyesuaian belum diset di Setting inventory.',
      );
    }
    return { inventoryAccountId, contraAccountId, itemUnitCost: item?.averageCost ?? null };
  };
}

/**
 * Map a line DTO → Prisma create input for ErpInvStockAdjustmentLine. Populates
 * ALL NOT NULL columns: quantity/baseQuantity, unitId/baseUnitId, unitCost,
 * inventoryAccountId/contraAccountId, warehouseId, direction. unitCost defaults
 * to the provided line value, else the item averageCost, else 0. No UoM
 * conversion table is wired yet, so baseQuantity = quantity and baseUnitId =
 * unitId (admin enters in base unit). warehouseId falls back to the header.
 */
export function mapAdjustmentLine(
  line: InvStockAdjustmentLineDto,
  accounts: ResolvedAccounts,
  headerWarehouseId: bigint,
): Prisma.ErpInvStockAdjustmentLineCreateWithoutStockAdjustmentInput {
  const unitCost =
    line.unitCost != null
      ? new Prisma.Decimal(line.unitCost)
      : accounts.itemUnitCost ?? new Prisma.Decimal(0);
  return {
    itemId: BigInt(line.itemId),
    direction: line.direction as never,
    quantity: dec(line.quantity),
    baseQuantity: dec(line.quantity),
    unitId: BigInt(line.unitId),
    baseUnitId: BigInt(line.unitId),
    unitCost,
    inventoryAccountId: accounts.inventoryAccountId,
    contraAccountId: accounts.contraAccountId,
    warehouseId: toBigInt(line.warehouseId) ?? headerWarehouseId,
    costCenterId: toBigInt(line.costCenterId),
    divisionId: toBigInt(line.divisionId),
    subdivisionId: toBigInt(line.subdivisionId),
    projectId: toBigInt(line.projectId),
    notes: line.notes ?? null,
    lineNo: line.lineNo,
  };
}
