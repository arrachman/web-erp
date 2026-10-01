import { Prisma } from '@prisma/client';
import { QueryInvPriceAdjustmentsDto } from './dto/query-inv-price-adjustments.dto';

export function toBigInt(v?: string | null): bigint | null {
  if (v === undefined || v === null || v === '') return null;
  return BigInt(v);
}

export function buildPriceAdjWhere(
  query: QueryInvPriceAdjustmentsDto,
): Prisma.ErpInvCostRecalculationWhereInput {
  const where: Prisma.ErpInvCostRecalculationWhereInput = { deletedAt: null };

  if (query.status) where.status = query.status as never;
  if (query.itemId) where.itemId = BigInt(query.itemId);
  if (query.warehouseId) where.warehouseId = BigInt(query.warehouseId);
  if (query.createdById) where.createdById = BigInt(query.createdById);

  if (query.dateFrom || query.dateTo) {
    where.fromDate = {
      ...(query.dateFrom ? { gte: new Date(query.dateFrom) } : {}),
      ...(query.dateTo ? { lte: new Date(query.dateTo) } : {}),
    };
  }

  return where;
}

export async function genDocNumber(
  tx: Prisma.TransactionClient,
  docCode: string,
): Promise<string> {
  const numbering = await tx.erpDocumentNumbering.findFirst({
    where: { documentCode: docCode, deletedAt: null },
  });
  if (numbering) {
    // Atomic increment: row lock serializes concurrent saves (no duplicate / skipped numbers).
    const bumped = await tx.erpDocumentNumbering.update({
      where: { id: numbering.id },
      data: { nextNumber: { increment: 1 } },
      select: { nextNumber: true },
    });
    const seq = bumped.nextNumber - 1;
    return `${numbering.prefix}${String(seq).padStart(numbering.digitCount, '0')}`;
  }
  const count = await tx.erpInvCostRecalculation.count();
  return `${docCode}${String(count + 1).padStart(6, '0')}`;
}
