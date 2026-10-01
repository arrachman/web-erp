import { PrismaService } from '../prisma/prisma.service';

type AnyBom = {
  branchId: bigint;
  sourceWarehouseId?: bigint | null;
  productionWarehouseId?: bigint | null;
  destinationWarehouseId?: bigint | null;
  [key: string]: unknown;
};

type EnrichedBom<T extends AnyBom> = T & {
  branch?: { id: string; code: string; name: string } | null;
  sourceWarehouse?: { id: string; code: string; name: string } | null;
  productionWarehouse?: { id: string; code: string; name: string } | null;
  destinationWarehouse?: { id: string; code: string; name: string } | null;
};

/**
 * Enrich BOMs with branch and warehouse lookups.
 * Performs two batch queries regardless of result set size.
 */
export async function enrichBoms<T extends AnyBom>(
  prisma: PrismaService,
  boms: T[],
): Promise<EnrichedBom<T>[]> {
  if (!boms.length) return [];

  // Collect unique branch ids
  const branchIds = [...new Set(boms.map((b) => b.branchId))];

  // Collect unique warehouse ids
  const warehouseIdSet = new Set<bigint>();
  for (const b of boms) {
    if (b.sourceWarehouseId) warehouseIdSet.add(b.sourceWarehouseId);
    if (b.productionWarehouseId) warehouseIdSet.add(b.productionWarehouseId);
    if (b.destinationWarehouseId) warehouseIdSet.add(b.destinationWarehouseId);
  }
  const warehouseIds = [...warehouseIdSet];

  const [branches, warehouses] = await Promise.all([
    prisma.erpBranch.findMany({
      where: { id: { in: branchIds } },
      select: { id: true, code: true, name: true },
    }),
    warehouseIds.length
      ? prisma.erpWarehouse.findMany({
          where: { id: { in: warehouseIds } },
          select: { id: true, code: true, name: true },
        })
      : Promise.resolve([]),
  ]);

  const branchMap = new Map(branches.map((b) => [b.id.toString(), b]));
  const whMap = new Map(warehouses.map((w) => [w.id.toString(), w]));

  const toRef = (id?: bigint | null) => {
    if (!id) return null;
    const r = whMap.get(id.toString());
    return r ? { id: r.id.toString(), code: r.code, name: r.name } : null;
  };

  return boms.map((bom) => {
    const br = branchMap.get(bom.branchId.toString());
    return {
      ...bom,
      branch: br ? { id: br.id.toString(), code: br.code, name: br.name } : null,
      sourceWarehouse: toRef(bom.sourceWarehouseId),
      productionWarehouse: toRef(bom.productionWarehouseId),
      destinationWarehouse: toRef(bom.destinationWarehouseId),
    } as EnrichedBom<T>;
  });
}
