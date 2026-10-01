import { PrismaService } from '../prisma/prisma.service';

/**
 * Cross-domain FKs (item/warehouse) are scalar BigInt with no Prisma @relation.
 * Resolve display code+name in one batched pass so list/detail responses carry
 * names, not just ids.
 */
type Ref = { id: string; code: string; name: string } | null;

type RawPriceAdj = {
  itemId: bigint | null;
  warehouseId: bigint | null;
  [k: string]: unknown;
};

async function nameMap(rows: Promise<{ id: bigint; code: string; name: string }[]>) {
  const map = new Map<string, Ref>();
  for (const r of await rows) {
    map.set(r.id.toString(), { id: r.id.toString(), code: r.code, name: r.name });
  }
  return map;
}

const SELECT = { id: true, code: true, name: true } as const;

export async function enrichPriceAdjustments<T extends RawPriceAdj>(
  prisma: PrismaService,
  items: T[],
) {
  const ids = (sel: (t: T) => bigint | null) => [
    ...new Set(items.map(sel).filter((v): v is bigint => v != null)),
  ];

  const [itemsMap, warehousesMap] = await Promise.all([
    nameMap(prisma.erpItem.findMany({ where: { id: { in: ids((t) => t.itemId) } }, select: SELECT })),
    nameMap(
      prisma.erpWarehouse.findMany({
        where: { id: { in: ids((t) => t.warehouseId) } },
        select: SELECT,
      }),
    ),
  ]);

  const ref = (map: Map<string, Ref>, id: bigint | null) =>
    id != null ? map.get(id.toString()) ?? null : null;

  return items.map((t) => ({
    ...t,
    item: ref(itemsMap, t.itemId),
    warehouse: ref(warehousesMap, t.warehouseId),
  }));
}
