import { PrismaService } from '../prisma/prisma.service';

/**
 * Cross-domain FKs resolved in one batched pass so list/detail responses carry
 * names, not just ids — keeps the frontend grid/form simple.
 */
type Ref = { id: string; code: string; name: string } | null;

type RawLine = {
  itemId: bigint;
  unitId: bigint;
  tax1Id: bigint | null;
  tax2Id: bigint | null;
  warehouseId: bigint | null;
  [k: string]: unknown;
};

type RawDeliveryReport = {
  customerId: bigint | null;
  branchId: bigint;
  locationId: bigint | null;
  warehouseId: bigint | null;
  currencyId: bigint;
  paymentTermId: bigint | null;
  salesDeptId: bigint | null;
  receivableAccountId: bigint | null;
  deliveryOrderId: bigint | null;
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

export async function enrichDeliveryReports<T extends RawDeliveryReport>(
  prisma: PrismaService,
  items: T[],
) {
  const ids = (sel: (t: T) => bigint | null) => [
    ...new Set(items.map(sel).filter((v): v is bigint => v != null)),
  ];

  const lines = items.flatMap((t) => t.lines ?? []);
  const lineIds = (sel: (l: RawLine) => bigint | null) => [
    ...new Set(lines.map(sel).filter((v): v is bigint => v != null)),
  ];

  const warehouseIds = [
    ...new Set([
      ...ids((t) => t.warehouseId),
      ...lineIds((l) => l.warehouseId),
    ]),
  ];
  const taxIds = [
    ...new Set([...lineIds((l) => l.tax1Id), ...lineIds((l) => l.tax2Id)]),
  ];

  // Resolve deliveryOrder codes — use docNumber as both code and name for display
  const doIds = [...new Set(items.map((t) => t.deliveryOrderId).filter((v): v is bigint => v != null))];
  const deliveryOrderRows = doIds.length
    ? await prisma.erpSlsDeliveryOrder.findMany({
        where: { id: { in: doIds } },
        select: { id: true, docNumber: true },
      })
    : [];
  const deliveryOrders = new Map<string, Ref>();
  for (const r of deliveryOrderRows) {
    deliveryOrders.set(r.id.toString(), {
      id: r.id.toString(),
      code: r.docNumber,
      name: r.docNumber,
    });
  }

  const [
    customers,
    branches,
    locations,
    warehouses,
    currencies,
    paymentTerms,
    salesDepts,
    receivableAccounts,
    lineItems,
    units,
    taxes,
  ] = await Promise.all([
    nameMap(prisma.erpPartner.findMany({ where: { id: { in: ids((t) => t.customerId) } }, select: SELECT })),
    nameMap(prisma.erpBranch.findMany({ where: { id: { in: ids((t) => t.branchId) } }, select: SELECT })),
    nameMap(prisma.erpLocation.findMany({ where: { id: { in: ids((t) => t.locationId) } }, select: SELECT })),
    nameMap(prisma.erpWarehouse.findMany({ where: { id: { in: warehouseIds } }, select: SELECT })),
    nameMap(prisma.erpCurrency.findMany({ where: { id: { in: ids((t) => t.currencyId) } }, select: SELECT })),
    nameMap(prisma.erpPaymentTerm.findMany({ where: { id: { in: ids((t) => t.paymentTermId) } }, select: SELECT })),
    nameMap(prisma.erpDivision.findMany({ where: { id: { in: ids((t) => t.salesDeptId) } }, select: SELECT })),
    nameMap(prisma.erpAccount.findMany({ where: { id: { in: ids((t) => t.receivableAccountId) } }, select: SELECT })),
    nameMap(prisma.erpItem.findMany({ where: { id: { in: lineIds((l) => l.itemId) } }, select: SELECT })),
    nameMap(prisma.erpUnit.findMany({ where: { id: { in: lineIds((l) => l.unitId) } }, select: SELECT })),
    nameMap(prisma.erpTax.findMany({ where: { id: { in: taxIds } }, select: SELECT })),
  ]);

  const ref = (map: Map<string, Ref>, id: bigint | null) =>
    id != null ? map.get(id.toString()) ?? null : null;

  return items.map((t) => ({
    ...t,
    customer: ref(customers, t.customerId),
    branch: ref(branches, t.branchId),
    location: ref(locations, t.locationId),
    warehouse: ref(warehouses, t.warehouseId),
    currency: ref(currencies, t.currencyId),
    paymentTerm: ref(paymentTerms, t.paymentTermId),
    salesDept: ref(salesDepts, t.salesDeptId),
    receivableAccount: ref(receivableAccounts, t.receivableAccountId),
    deliveryOrder: t.deliveryOrderId != null
      ? deliveryOrders.get(t.deliveryOrderId.toString()) ?? null
      : null,
    lines: (t.lines ?? []).map((l) => ({
      ...l,
      item: ref(lineItems, l.itemId),
      unit: ref(units, l.unitId),
      tax1: ref(taxes, l.tax1Id),
      tax2: ref(taxes, l.tax2Id),
      warehouse: ref(warehouses, l.warehouseId),
    })),
  }));
}
