import { PrismaService } from '../prisma/prisma.service';

/**
 * Cross-domain FKs (branch/location/warehouses/requestedPartner + per-line
 * item/unit/warehouses/costCenter) are scalar BigInt with no Prisma @relation
 * (inv ↔ md decoupled by design). Resolve display code+name in one batched pass
 * so list/detail responses carry names, not just ids.
 */
type Ref = { id: string; code: string; name: string } | null;

type RawLine = {
  itemId: bigint;
  unitId: bigint;
  sourceWarehouseId: bigint | null;
  destinationWarehouseId: bigint | null;
  costCenterId: bigint | null;
  [k: string]: unknown;
};

type RawMovement = {
  branchId: bigint;
  locationId: bigint | null;
  sourceWarehouseId: bigint | null;
  transitWarehouseId: bigint | null;
  destinationWarehouseId: bigint | null;
  requestedPartnerId: bigint | null;
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

export async function enrichMovements<T extends RawMovement>(prisma: PrismaService, items: T[]) {
  const ids = (sel: (t: T) => bigint | null) => [
    ...new Set(items.map(sel).filter((v): v is bigint => v != null)),
  ];

  const lines = items.flatMap((t) => t.lines ?? []);
  const lineIds = (sel: (l: RawLine) => bigint | null) => [
    ...new Set(lines.map(sel).filter((v): v is bigint => v != null)),
  ];

  const warehouseIds = [
    ...new Set([
      ...ids((t) => t.sourceWarehouseId),
      ...ids((t) => t.transitWarehouseId),
      ...ids((t) => t.destinationWarehouseId),
      ...lineIds((l) => l.sourceWarehouseId),
      ...lineIds((l) => l.destinationWarehouseId),
    ]),
  ];

  const [branches, locations, warehouses, partners, lineItems, units, costCenters] = await Promise.all([
    nameMap(prisma.erpBranch.findMany({ where: { id: { in: ids((t) => t.branchId) } }, select: SELECT })),
    nameMap(prisma.erpLocation.findMany({ where: { id: { in: ids((t) => t.locationId) } }, select: SELECT })),
    nameMap(prisma.erpWarehouse.findMany({ where: { id: { in: warehouseIds } }, select: SELECT })),
    nameMap(prisma.erpPartner.findMany({ where: { id: { in: ids((t) => t.requestedPartnerId) } }, select: SELECT })),
    nameMap(prisma.erpItem.findMany({ where: { id: { in: lineIds((l) => l.itemId) } }, select: SELECT })),
    nameMap(prisma.erpUnit.findMany({ where: { id: { in: lineIds((l) => l.unitId) } }, select: SELECT })),
    nameMap(prisma.erpCostCenter.findMany({ where: { id: { in: lineIds((l) => l.costCenterId) } }, select: SELECT })),
  ]);

  const ref = (map: Map<string, Ref>, id: bigint | null) =>
    id != null ? map.get(id.toString()) ?? null : null;

  return items.map((t) => ({
    ...t,
    branch: ref(branches, t.branchId),
    location: ref(locations, t.locationId),
    sourceWarehouse: ref(warehouses, t.sourceWarehouseId),
    transitWarehouse: ref(warehouses, t.transitWarehouseId),
    destinationWarehouse: ref(warehouses, t.destinationWarehouseId),
    requestedPartner: ref(partners, t.requestedPartnerId),
    lines: (t.lines ?? []).map((l) => ({
      ...l,
      item: ref(lineItems, l.itemId),
      unit: ref(units, l.unitId),
      sourceWarehouse: ref(warehouses, l.sourceWarehouseId),
      destinationWarehouse: ref(warehouses, l.destinationWarehouseId),
      costCenter: ref(costCenters, l.costCenterId),
    })),
  }));
}
