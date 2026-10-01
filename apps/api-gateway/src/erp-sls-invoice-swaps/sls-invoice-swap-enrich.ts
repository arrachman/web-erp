import { PrismaService } from '../prisma/prisma.service';

/**
 * Cross-domain FKs (customer/branch/currency + per-line fromInvoice/toInvoice)
 * are scalar BigInt with no Prisma @relation. Resolved in one batched pass for
 * list/detail responses.
 */
type Ref = { id: string; code: string; name: string } | null;
type InvoiceRef = { id: string; docNumber: string } | null;

type RawLine = {
  fromInvoiceId: bigint;
  toInvoiceId: bigint | null;
  [k: string]: unknown;
};

type RawInvoiceSwap = {
  customerId: bigint | null;
  branchId: bigint;
  currencyId: bigint;
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

async function invoiceMap(rows: Promise<{ id: bigint; docNumber: string }[]>) {
  const map = new Map<string, InvoiceRef>();
  for (const r of await rows) {
    map.set(r.id.toString(), { id: r.id.toString(), docNumber: r.docNumber });
  }
  return map;
}

const SELECT = { id: true, code: true, name: true } as const;
const INV_SELECT = { id: true, docNumber: true } as const;

export async function enrichInvoiceSwaps<T extends RawInvoiceSwap>(
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

  const invoiceIds = [
    ...new Set([...lineIds((l) => l.fromInvoiceId), ...lineIds((l) => l.toInvoiceId)]),
  ];

  const [customers, branches, currencies, invoices] = await Promise.all([
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
    invoiceMap(
      prisma.erpSlsInvoice.findMany({
        where: { id: { in: invoiceIds } },
        select: INV_SELECT,
      }),
    ),
  ]);

  const ref = (map: Map<string, Ref>, id: bigint | null) =>
    id != null ? map.get(id.toString()) ?? null : null;
  const invRef = (id: bigint | null) =>
    id != null ? invoices.get(id.toString()) ?? null : null;

  return items.map((t) => ({
    ...t,
    customer: ref(customers, t.customerId),
    branch: ref(branches, t.branchId),
    currency: ref(currencies, t.currencyId),
    lines: (t.lines ?? []).map((l) => ({
      ...l,
      fromInvoice: invRef(l.fromInvoiceId),
      toInvoice: invRef(l.toInvoiceId),
    })),
  }));
}
