import { BadRequestException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { InvOpeningStockLineDto } from './dto/create-inv-opening-stock.dto';
import { QueryInvOpeningStocksDto } from './dto/query-inv-opening-stocks.dto';
import { InvOpeningStockTransitionAction as A } from './dto/transition-inv-opening-stock.dto';

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

/** Document-numbering code for opening stock (Initial Balance). */
export const DOC_CODE = 'IB';

/**
 * Resolve the NOT NULL inventory account for a line, in priority order:
 *   1. line.inventoryAccountId (explicit override)
 *   2. item.inventoryAccountId (master item default)
 *   3. Setting inventory/accounts/defaultInventoryAccountId
 * Throws BadRequest if none resolve — the column cannot be null.
 */
export async function resolveInventoryAccount(
  prisma: PrismaService,
  line: InvOpeningStockLineDto,
): Promise<bigint> {
  if (line.inventoryAccountId) return BigInt(line.inventoryAccountId);

  const item = await prisma.erpItem.findUnique({
    where: { id: BigInt(line.itemId) },
    select: { inventoryAccountId: true },
  });
  if (item?.inventoryAccountId != null) return item.inventoryAccountId;

  const setting = await prisma.erpSetting.findUnique({
    where: {
      module_group_key: {
        module: 'inventory',
        group: 'accounts',
        key: 'defaultInventoryAccountId',
      },
    },
    select: { value: true },
  });
  if (setting?.value) return BigInt(setting.value);

  throw new BadRequestException(
    'Akun persediaan belum diset untuk item — set di master item atau Setting inventory.',
  );
}

export function buildInvOpeningStockWhere(
  query: QueryInvOpeningStocksDto,
): Prisma.ErpInvOpeningStockWhereInput {
  const where: Prisma.ErpInvOpeningStockWhereInput = { deletedAt: null };

  if (query.status) where.status = query.status as never;
  if (query.branchId) where.branchId = BigInt(query.branchId);
  if (query.warehouseId) where.warehouseId = BigInt(query.warehouseId);
  if (query.dateFrom || query.dateTo) {
    where.openingDate = {
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
 * Map a line DTO → Prisma create input for ErpInvOpeningStockLine. Populates ALL
 * NOT NULL columns. `inventoryAccountId` and `warehouseId` are resolved upstream
 * (inventory account via {@link resolveInventoryAccount}, warehouse via header
 * fallback). `baseUnitId` mirrors `unitId` — no UoM conversion table wired yet.
 */
export function mapOpeningStockLine(
  line: InvOpeningStockLineDto,
  resolved: { inventoryAccountId: bigint; warehouseId: bigint },
): Prisma.ErpInvOpeningStockLineCreateWithoutOpeningStockInput {
  return {
    itemId: BigInt(line.itemId),
    quantity: dec(line.quantity),
    unitId: BigInt(line.unitId),
    baseUnitId: BigInt(line.unitId),
    unitCost: dec(line.unitCost),
    inventoryAccountId: resolved.inventoryAccountId,
    warehouseId: resolved.warehouseId,
    costCenterId: toBigInt(line.costCenterId),
    divisionId: toBigInt(line.divisionId),
    subdivisionId: toBigInt(line.subdivisionId),
    projectId: toBigInt(line.projectId),
    notes: line.notes ?? null,
    lineNo: line.lineNo,
  };
}
