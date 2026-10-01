import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

type PairSource = {
  warehouseId: number;
  expiryDate: Date | null;
};

function pairKey(itemId: string, batchNumber: string) {
  return `${itemId}::${batchNumber.trim().toLowerCase()}`;
}

async function main() {
  console.log('[backfill] starting inventory batch + ledger rebuild...');

  await prisma.$transaction(async (tx) => {
    await tx.inventoryLedger.deleteMany({});
    await tx.inventoryBatch.deleteMany({});

    const inboundDocs = await tx.inbound.findMany({
      where: {
        deletedAt: null,
        status: 'POSTED',
      },
      orderBy: [{ transactionDate: 'asc' }, { createdAt: 'asc' }],
      select: {
        uuid: true,
        transactionNo: true,
        transactionDate: true,
        warehouse: { select: { id: true } },
        createdBy: true,
        details: {
          where: { deletedAt: null },
          orderBy: [{ lineNo: 'asc' }],
          select: {
            itemId: true,
            item: { select: { id: true, uom: { select: { id: true } } } },
            batches: {
              where: { deletedAt: null },
              orderBy: [{ lineNo: 'asc' }],
              select: {
                batchIn: true,
                qty: true,
                expiredDate: true,
              },
            },
          },
        },
      },
    });

    let inboundLedgerCount = 0;
    const sourceByPair = new Map<string, PairSource>();

    for (const inbound of inboundDocs) {
      for (const detail of inbound.details) {
        for (const batch of detail.batches) {
          const batchNumber = String(batch.batchIn ?? '').trim();
          if (!batchNumber) {
            continue;
          }

          const key = pairKey(detail.itemId, batchNumber);
          if (!sourceByPair.has(key)) {
            sourceByPair.set(key, {
              warehouseId: inbound.warehouse.id,
              expiryDate: batch.expiredDate ?? null,
            });
          }

          const inventoryBatch = await tx.inventoryBatch.upsert({
            where: {
              itemId_batchNumber: {
                itemId: detail.item.id,
                batchNumber,
              },
            },
            update: {
              expiryDate: batch.expiredDate ?? undefined,
              deletedAt: null,
              deletedBy: null,
            },
            create: {
              itemId: detail.item.id,
              batchNumber,
              expiryDate: batch.expiredDate ?? null,
              createdBy: inbound.createdBy ?? null,
              updatedBy: inbound.createdBy ?? null,
            },
            select: { id: true },
          });

          const createdByUser = inbound.createdBy
            ? await tx.user.findFirst({
                where: { uuid: inbound.createdBy, deletedAt: null },
                select: { id: true },
              })
            : null;

          await tx.inventoryLedger.create({
            data: {
              transactionDate: inbound.transactionDate,
              itemId: detail.item.id,
              warehouseId: inbound.warehouse.id,
              batchId: inventoryBatch.id,
              transactionType: 'INBOUND',
              referenceDocType: 'INBOUND',
              referenceDocId: inbound.uuid,
              referenceNumber: inbound.transactionNo,
              quantityPcs: batch.qty,
              quantityKg: 0,
              uomId: detail.item.uom.id,
              unitCost: null,
              totalValue: 0,
              userId: createdByUser?.id ?? null,
              createdBy: inbound.createdBy ?? null,
              updatedBy: inbound.createdBy ?? null,
            },
          });
          inboundLedgerCount += 1;
        }
      }
    }

    const outboundDocs = await tx.deliveryOrder.findMany({
      where: { deletedAt: null },
      orderBy: [{ doDate: 'asc' }, { createdAt: 'asc' }],
      select: {
        uuid: true,
        doNumber: true,
        doDate: true,
        createdBy: true,
        details: {
          where: { deletedAt: null },
          orderBy: [{ lineNo: 'asc' }],
          select: {
            itemId: true,
            item: { select: { id: true, uom: { select: { id: true } } } },
            batches: {
              where: { deletedAt: null },
              orderBy: [{ lineNo: 'asc' }],
              select: {
                batchOut: true,
                qtyPcs: true,
                qtyKg: true,
                expiredDate: true,
                notes: true,
              },
            },
          },
        },
      },
    });

    let outboundLedgerCount = 0;
    for (const outbound of outboundDocs) {
      for (const detail of outbound.details) {
        for (const batch of detail.batches) {
          const batchNumber = String(batch.batchOut ?? '').trim();
          if (!batchNumber) {
            continue;
          }

          const key = pairKey(detail.itemId, batchNumber);
          const source = sourceByPair.get(key);
          if (!source?.warehouseId) {
            throw new Error(
              `Cannot resolve source warehouse for item=${detail.itemId}, batch=${batchNumber}, do=${outbound.doNumber}`,
            );
          }

          const inventoryBatch = await tx.inventoryBatch.upsert({
            where: {
              itemId_batchNumber: {
                itemId: detail.item.id,
                batchNumber,
              },
            },
            update: {
              expiryDate: source.expiryDate ?? batch.expiredDate ?? undefined,
              deletedAt: null,
              deletedBy: null,
            },
            create: {
              itemId: detail.item.id,
              batchNumber,
              expiryDate: source.expiryDate ?? batch.expiredDate ?? null,
              createdBy: outbound.createdBy ?? null,
              updatedBy: outbound.createdBy ?? null,
            },
            select: { id: true },
          });

          const qtyPcs = Number(batch.qtyPcs ?? 0);
          const qtyKg = Number(batch.qtyKg ?? 0);
          const createdByUser = outbound.createdBy
            ? await tx.user.findFirst({
                where: { uuid: outbound.createdBy, deletedAt: null },
                select: { id: true },
              })
            : null;

          await tx.inventoryLedger.create({
            data: {
              transactionDate: outbound.doDate ?? new Date(),
              itemId: detail.item.id,
              warehouseId: source.warehouseId,
              batchId: inventoryBatch.id,
              transactionType: 'OUTBOUND',
              referenceDocType: 'OUTBOUND',
              referenceDocId: outbound.uuid,
              referenceNumber: outbound.doNumber,
              quantityPcs: -Math.abs(Number.isFinite(qtyPcs) ? qtyPcs : 0),
              quantityKg: -Math.abs(Number.isFinite(qtyKg) ? qtyKg : 0),
              uomId: detail.item.uom.id,
              unitCost: null,
              totalValue: 0,
              userId: createdByUser?.id ?? null,
              notes: batch.notes ?? null,
              createdBy: outbound.createdBy ?? null,
              updatedBy: outbound.createdBy ?? null,
            },
          });
          outboundLedgerCount += 1;
        }
      }
    }

    console.log(
      `[backfill] done. inbound ledger=${inboundLedgerCount}, outbound ledger=${outboundLedgerCount}`,
    );
  });
}

main()
  .then(async () => {
    await prisma.$disconnect();
  })
  .catch(async (error) => {
    console.error('[backfill] failed:', error);
    await prisma.$disconnect();
    process.exit(1);
  });
