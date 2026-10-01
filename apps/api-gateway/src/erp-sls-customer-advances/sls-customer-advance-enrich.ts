import { PrismaService } from '../prisma/prisma.service';

/**
 * Cross-domain FKs (customer/branch/currency/paymentTerm/costCenter/division/
 * project) are scalar BigInt with no Prisma @relation (sls ↔ md are decoupled
 * by design). Resolved in one batched pass for list/detail responses.
 */
type Ref = { id: string; code: string; name: string } | null;

type RawCustomerAdvance = {
  customerId: bigint | null;
  branchId: bigint;
  currencyId: bigint;
  paymentTermId: bigint | null;
  costCenterId: bigint | null;
  divisionId: bigint | null;
  projectId: bigint | null;
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

export async function enrichCustomerAdvances<T extends RawCustomerAdvance>(
  prisma: PrismaService,
  items: T[],
) {
  const ids = (sel: (t: T) => bigint | null) => [
    ...new Set(items.map(sel).filter((v): v is bigint => v != null)),
  ];

  const [customers, branches, currencies, paymentTerms, costCenters, divisions, projects] =
    await Promise.all([
      nameMap(
        prisma.erpPartner.findMany({
          where: { id: { in: ids((t) => t.customerId) } },
          select: SELECT,
        }),
      ),
      nameMap(
        prisma.erpBranch.findMany({
          where: { id: { in: ids((t) => t.branchId) } },
          select: SELECT,
        }),
      ),
      nameMap(
        prisma.erpCurrency.findMany({
          where: { id: { in: ids((t) => t.currencyId) } },
          select: SELECT,
        }),
      ),
      nameMap(
        prisma.erpPaymentTerm.findMany({
          where: { id: { in: ids((t) => t.paymentTermId) } },
          select: SELECT,
        }),
      ),
      nameMap(
        prisma.erpCostCenter.findMany({
          where: { id: { in: ids((t) => t.costCenterId) } },
          select: SELECT,
        }),
      ),
      nameMap(
        prisma.erpDivision.findMany({
          where: { id: { in: ids((t) => t.divisionId) } },
          select: SELECT,
        }),
      ),
      nameMap(
        prisma.erpProject.findMany({
          where: { id: { in: ids((t) => t.projectId) } },
          select: SELECT,
        }),
      ),
    ]);

  const ref = (map: Map<string, Ref>, id: bigint | null) =>
    id != null ? map.get(id.toString()) ?? null : null;

  return items.map((t) => ({
    ...t,
    customer: ref(customers, t.customerId),
    branch: ref(branches, t.branchId),
    currency: ref(currencies, t.currencyId),
    paymentTerm: ref(paymentTerms, t.paymentTermId),
    costCenter: ref(costCenters, t.costCenterId),
    division: ref(divisions, t.divisionId),
    project: ref(projects, t.projectId),
  }));
}
