import { BadRequestException } from '@nestjs/common';
import { Prisma } from '@prisma/client';

const ZERO = new Prisma.Decimal(0);

type Side = 'DEBIT' | 'CREDIT';
type SubledgerTotals = Map<string, { total: Prisma.Decimal; side: Side; label: string }>;

function addTo(map: SubledgerTotals, accountId: bigint | null, amount: Prisma.Decimal, side: Side, label: string) {
  if (!accountId) return;
  const key = accountId.toString();
  const cur = map.get(key);
  map.set(key, { total: (cur?.total ?? ZERO).add(amount), side, label });
}

/** Saldo awal subledger per akun kontrol (AR/AP/Persediaan) dari dokumen opening yang sudah POSTED. */
async function loadOpeningSubledgers(tx: Prisma.TransactionClient): Promise<SubledgerTotals> {
  const map: SubledgerTotals = new Map();
  const live = { deletedAt: null, postingStatus: 'POSTED' as const, isOpeningBalance: true };

  const ar = await tx.erpSlsInvoice.findMany({ where: live, select: { grandTotal: true, receivableAccountId: true } });
  ar.forEach((i) => addTo(map, i.receivableAccountId, new Prisma.Decimal(i.grandTotal), 'DEBIT', 'Opening AR Balance'));

  const ap = await tx.erpPurInvoice.findMany({ where: live, select: { grandTotal: true, payableAccountId: true } });
  ap.forEach((i) => addTo(map, i.payableAccountId, new Prisma.Decimal(i.grandTotal), 'CREDIT', 'Opening AP Balance'));

  const stock = await tx.erpInvOpeningStockLine.findMany({
    where: { openingStock: { deletedAt: null, postingStatus: 'POSTED' } },
    select: { quantity: true, unitCost: true, inventoryAccountId: true },
  });
  stock.forEach((l) =>
    addTo(map, l.inventoryAccountId, new Prisma.Decimal(l.quantity).mul(l.unitCost), 'DEBIT', 'Opening Stock (IB)'),
  );
  return map;
}

/**
 * FR-FIN-06: Opening Balance (CoA) harus seimbang dengan Opening AR/AP/Stock.
 * Bila jurnal OPENING_BALANCE menyentuh akun kontrol yang punya saldo awal
 * subledger, saldo akun itu (jurnal ini + jurnal opening lain yang sudah POSTED)
 * wajib sama dengan Σ saldo awal subledger. Akun non-kontrol tidak diperiksa.
 */
export async function assertOpeningControlBalanced(
  tx: Prisma.TransactionClient,
  entry: { id: bigint; lines: { accountId: bigint; debit: Prisma.Decimal; credit: Prisma.Decimal }[] },
): Promise<void> {
  const subledgers = await loadOpeningSubledgers(tx);
  const touched = [...new Set(entry.lines.map((l) => l.accountId.toString()))].filter((id) => subledgers.has(id));

  for (const accountId of touched) {
    const sub = subledgers.get(accountId)!;
    const net = (d: Prisma.Decimal, c: Prisma.Decimal) => (sub.side === 'DEBIT' ? d.sub(c) : c.sub(d));
    const here = entry.lines
      .filter((l) => l.accountId.toString() === accountId)
      .reduce((s, l) => s.add(net(new Prisma.Decimal(l.debit), new Prisma.Decimal(l.credit))), ZERO);
    const prior = await tx.erpFinLedgerEntry.aggregate({
      where: {
        accountId: BigInt(accountId),
        isOpeningBalance: true,
        source: 'JOURNAL',
        NOT: { sourceDocType: 'fin_journal_entries', sourceId: entry.id },
        deletedAt: null,
      },
      _sum: { debit: true, credit: true },
    });
    const priorNet = net(new Prisma.Decimal(prior._sum.debit ?? 0), new Prisma.Decimal(prior._sum.credit ?? 0));
    const coa = here.add(priorNet);
    if (!coa.sub(sub.total).abs().lte('0.01')) {
      throw new BadRequestException(
        `Opening Balance (CoA) tidak seimbang dengan ${sub.label} di akun kontrol (id ${accountId}): ` +
          `CoA ${coa.toFixed(2)} ≠ subledger ${sub.total.toFixed(2)}.`,
      );
    }
  }
}
