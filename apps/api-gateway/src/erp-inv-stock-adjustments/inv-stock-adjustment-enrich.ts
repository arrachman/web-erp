import { PrismaService } from '../prisma/prisma.service';

/**
 * Cross-domain FKs (header branch/warehouse + per-line
 * item/unit/warehouse/inventoryAccount/contraAccount/costCenter) are scalar
 * BigInt with no Prisma @relation (inv ↔ md decoupled by design). Resolve
 * display code+name in one batched pass so list/detail responses carry names,
 * not just ids.
 */
type Ref = { id: string; code: string; name: string } | null;

type RawLine = {
  itemId: bigint;
  unitId: bigint;
  warehouseId: bigint;
  inventoryAccountId: bigint;
  contraAccountId: bigint;
  costCenterId: bigint | null;
  [k: string]: unknown;
};

type RawAdjustment = {
  branchId: bigint;
  warehouseId: bigint;
  lines?: RawLine[];
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

export async function enrichAdjustments<T extends RawAdjustment>(prisma: PrismaService, items: T[]) {
  const ids = (sel: (t: T) => bigint | null) => [
    ...new Set(items.map(sel).filter((v): v is bigint => v != null)),
  ];

  const lines = items.flatMap((t) => t.lines ?? []);
  const lineIds = (sel: (l: RawLine) => bigint | null) => [
    ...new Set(lines.map(sel).filter((v): v is bigint => v != null)),
  ];

  const warehouseIds = [
    ...new Set([...ids((t) => t.warehouseId), ...lineIds((l) => l.warehouseId)]),
  ];
  const accountIds = [
    ...new Set([
      ...lineIds((l) => l.inventoryAccountId),
      ...lineIds((l) => l.contraAccountId),
    ]),
  ];

  const [branches, warehouses, lineItems, units, accounts, costCenters] = await Promise.all([
    nameMap(prisma.erpBranch.findMany({ where: { id: { in: ids((t) => t.branchId) } }, select: SELECT })),
    nameMap(prisma.erpWarehouse.findMany({ where: { id: { in: warehouseIds } }, select: SELECT })),
    nameMap(prisma.erpItem.findMany({ where: { id: { in: lineIds((l) => l.itemId) } }, select: SELECT })),
    nameMap(prisma.erpUnit.findMany({ where: { id: { in: lineIds((l) => l.unitId) } }, select: SELECT })),
    nameMap(prisma.erpAccount.findMany({ where: { id: { in: accountIds } }, select: SELECT })),
    nameMap(prisma.erpCostCenter.findMany({ where: { id: { in: lineIds((l) => l.costCenterId) } }, select: SELECT })),
  ]);

  const ref = (map: Map<string, Ref>, id: bigint | null) =>
    id != null ? map.get(id.toString()) ?? null : null;

  return items.map((t) => ({
    ...t,
    branch: ref(branches, t.branchId),
    warehouse: ref(warehouses, t.warehouseId),
    lines: (t.lines ?? []).map((l) => ({
      ...l,
      item: ref(lineItems, l.itemId),
      unit: ref(units, l.unitId),
      warehouse: ref(warehouses, l.warehouseId),
      inventoryAccount: ref(accounts, l.inventoryAccountId),
      contraAccount: ref(accounts, l.contraAccountId),
      costCenter: ref(costCenters, l.costCenterId),
    })),
  }));
}
