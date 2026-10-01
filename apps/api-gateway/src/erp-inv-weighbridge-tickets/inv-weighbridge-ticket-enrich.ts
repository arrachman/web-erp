import { PrismaService } from '../prisma/prisma.service';

/**
 * Cross-domain FKs (branch/location/partner/item) are scalar BigInt with no
 * Prisma @relation. Resolve display code+name in one batched pass.
 */
type Ref = { id: string; code: string; name: string } | null;

type RawTicket = {
  branchId: bigint;
  locationId: bigint | null;
  partnerId: bigint | null;
  itemId: bigint | null;
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

export async function enrichWeighbridgeTickets<T extends RawTicket>(
  prisma: PrismaService,
  items: T[],
) {
  const ids = (sel: (t: T) => bigint | null) => [
    ...new Set(items.map(sel).filter((v): v is bigint => v != null)),
  ];

  const [branches, locations, partners, itemsMap] = await Promise.all([
    nameMap(prisma.erpBranch.findMany({ where: { id: { in: ids((t) => t.branchId) } }, select: SELECT })),
    nameMap(
      prisma.erpLocation.findMany({ where: { id: { in: ids((t) => t.locationId) } }, select: SELECT }),
    ),
    nameMap(
      prisma.erpPartner.findMany({ where: { id: { in: ids((t) => t.partnerId) } }, select: SELECT }),
    ),
    nameMap(prisma.erpItem.findMany({ where: { id: { in: ids((t) => t.itemId) } }, select: SELECT })),
  ]);

  const ref = (map: Map<string, Ref>, id: bigint | null) =>
    id != null ? map.get(id.toString()) ?? null : null;

  return items.map((t) => ({
    ...t,
    branch: ref(branches, t.branchId),
    location: ref(locations, t.locationId),
    partner: ref(partners, t.partnerId),
    item: ref(itemsMap, t.itemId),
  }));
}
