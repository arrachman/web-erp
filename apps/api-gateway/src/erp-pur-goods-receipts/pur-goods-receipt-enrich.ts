import { PrismaService } from '../prisma/prisma.service';

type Ref = { id: string; code: string; name: string } | null;
type RawLine = { itemId: bigint; unitId: bigint; tax1Id: bigint | null; tax2Id: bigint | null; warehouseId: bigint | null; [k: string]: unknown };
type RawGrn = { supplierId: bigint | null; branchId: bigint; locationId: bigint | null; warehouseId: bigint | null; currencyId: bigint; paymentTermId: bigint | null; payableAccountId: bigint | null; lines?: RawLine[]; [k: string]: unknown };

async function nameMap(rows: Promise<{ id: bigint; code: string; name: string }[]>) {
  const map = new Map<string, Ref>();
  for (const r of await rows) map.set(r.id.toString(), { id: r.id.toString(), code: r.code, name: r.name });
  return map;
}

const SELECT = { id: true, code: true, name: true } as const;

export async function enrichGoodsReceipts<T extends RawGrn>(prisma: PrismaService, items: T[]) {
  const ids = (sel: (t: T) => bigint | null) => [...new Set(items.map(sel).filter((v): v is bigint => v != null))];
  const lines = items.flatMap((t) => t.lines ?? []);
  const lineIds = (sel: (l: RawLine) => bigint | null) => [...new Set(lines.map(sel).filter((v): v is bigint => v != null))];
  const warehouseIds = [...new Set([...ids((t) => t.warehouseId), ...lineIds((l) => l.warehouseId)])];
  const taxIds = [...new Set([...lineIds((l) => l.tax1Id), ...lineIds((l) => l.tax2Id)])];

  const [suppliers, branches, locations, warehouses, currencies, paymentTerms, payableAccounts, lineItems, units, taxes] = await Promise.all([
    nameMap(prisma.erpPartner.findMany({ where: { id: { in: ids((t) => t.supplierId) } }, select: SELECT })),
    nameMap(prisma.erpBranch.findMany({ where: { id: { in: ids((t) => t.branchId) } }, select: SELECT })),
    nameMap(prisma.erpLocation.findMany({ where: { id: { in: ids((t) => t.locationId) } }, select: SELECT })),
    nameMap(prisma.erpWarehouse.findMany({ where: { id: { in: warehouseIds } }, select: SELECT })),
    nameMap(prisma.erpCurrency.findMany({ where: { id: { in: ids((t) => t.currencyId) } }, select: SELECT })),
    nameMap(prisma.erpPaymentTerm.findMany({ where: { id: { in: ids((t) => t.paymentTermId) } }, select: SELECT })),
    nameMap(prisma.erpAccount.findMany({ where: { id: { in: ids((t) => t.payableAccountId) } }, select: SELECT })),
    nameMap(prisma.erpItem.findMany({ where: { id: { in: lineIds((l) => l.itemId) } }, select: SELECT })),
    nameMap(prisma.erpUnit.findMany({ where: { id: { in: lineIds((l) => l.unitId) } }, select: SELECT })),
    nameMap(prisma.erpTax.findMany({ where: { id: { in: taxIds } }, select: SELECT })),
  ]);

  const ref = (map: Map<string, Ref>, id: bigint | null) => id != null ? map.get(id.toString()) ?? null : null;
  return items.map((t) => ({
    ...t,
    supplier: ref(suppliers, t.supplierId), branch: ref(branches, t.branchId),
    location: ref(locations, t.locationId), warehouse: ref(warehouses, t.warehouseId),
    currency: ref(currencies, t.currencyId), paymentTerm: ref(paymentTerms, t.paymentTermId),
    payableAccount: ref(payableAccounts, t.payableAccountId),
    lines: (t.lines ?? []).map((l) => ({
      ...l, item: ref(lineItems, l.itemId), unit: ref(units, l.unitId),
      tax1: ref(taxes, l.tax1Id), tax2: ref(taxes, l.tax2Id), warehouse: ref(warehouses, l.warehouseId),
    })),
  }));
}
