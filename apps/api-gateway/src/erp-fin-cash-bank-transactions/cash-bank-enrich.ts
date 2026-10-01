import { PrismaService } from '../prisma/prisma.service';

/**
 * Cross-domain FKs (partner/account/branch/location/currency) are scalar BigInt
 * with no Prisma @relation (fin ↔ md are decoupled by design). We resolve their
 * display code+name in one batched pass so list/detail responses carry names,
 * not just ids — keeps the frontend table simple (Terima Dari, Akun Kas, Uang…).
 */
type Ref = { id: string; code: string; name: string } | null;

type RawTxn = {
  partnerId: bigint | null;
  bankAccountId: bigint;
  branchId: bigint;
  locationId: bigint | null;
  currencyId: bigint;
  lines?: { accountId: bigint }[];
  [k: string]: unknown;
};

async function nameMap(
  rows: Promise<{ id: bigint; code: string; name: string }[]>,
) {
  const map = new Map<string, Ref>();
  for (const r of await rows) {
    map.set(r.id.toString(), { id: r.id.toString(), code: r.code, name: r.name });
  }
  return map;
}

export async function enrichTransactions<T extends RawTxn>(
  prisma: PrismaService,
  items: T[],
) {
  const ids = (sel: (t: T) => bigint | null) =>
    [...new Set(items.map(sel).filter((v): v is bigint => v != null).map((v) => v))];

  const accountIds = [
    ...new Set([
      ...items.map((t) => t.bankAccountId),
      ...items.flatMap((t) => (t.lines ?? []).map((l) => l.accountId)),
    ]),
  ];

  const [partners, accounts, branches, locations, currencies] = await Promise.all([
    nameMap(prisma.erpPartner.findMany({ where: { id: { in: ids((t) => t.partnerId) } }, select: { id: true, code: true, name: true } })),
    nameMap(prisma.erpAccount.findMany({ where: { id: { in: accountIds } }, select: { id: true, code: true, name: true } })),
    nameMap(prisma.erpBranch.findMany({ where: { id: { in: ids((t) => t.branchId) } }, select: { id: true, code: true, name: true } })),
    nameMap(prisma.erpLocation.findMany({ where: { id: { in: ids((t) => t.locationId) } }, select: { id: true, code: true, name: true } })),
    nameMap(prisma.erpCurrency.findMany({ where: { id: { in: ids((t) => t.currencyId) } }, select: { id: true, code: true, name: true } })),
  ]);

  return items.map((t) => ({
    ...t,
    partner: t.partnerId ? partners.get(t.partnerId.toString()) ?? null : null,
    bankAccount: accounts.get(t.bankAccountId.toString()) ?? null,
    branch: branches.get(t.branchId.toString()) ?? null,
    location: t.locationId ? locations.get(t.locationId.toString()) ?? null : null,
    currency: currencies.get(t.currencyId.toString()) ?? null,
    lines: (t.lines ?? []).map((l) => ({
      ...l,
      account: accounts.get(l.accountId.toString()) ?? null,
    })),
  }));
}
