import { ErpTaxEntryType } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';

/**
 * A4 — Build PPN subledger entries from posted invoices.
 *
 * Sales invoices produce PPN_KELUARAN entries, purchase invoices
 * PPN_MASUKAN; any line-level PPh tax becomes a PPH_* entry on the same
 * invoice (informational — the bendahara withholding expectation itself
 * is recorded on the Bukti Potong tab). Amounts are aggregated per
 * (invoice, tax) from the invoice lines, exactly as posted.
 * Idempotent on (module, sourceDocType, sourceId, taxId).
 */

interface TaxAgg {
  taxId: bigint;
  dpp: number;
  taxAmount: number;
}

function aggregateLines(lines: any[]): Map<string, TaxAgg> {
  const byTax = new Map<string, TaxAgg>();
  const add = (taxId: bigint | null, taxAmount: any, base: number) => {
    if (!taxId) return;
    const amt = Number(taxAmount?.toString() ?? 0);
    if (!amt) return;
    const key = taxId.toString();
    const cur = byTax.get(key) ?? { taxId, dpp: 0, taxAmount: 0 };
    cur.dpp += base;
    cur.taxAmount += amt;
    byTax.set(key, cur);
  };
  for (const l of lines) {
    const base = Number(l.quantity.toString()) * Number(l.unitPrice.toString()) - Number(l.discountAmount?.toString() ?? 0);
    add(l.tax1Id, l.tax1Amount, base);
    add(l.tax2Id, l.tax2Amount, base);
  }
  return byTax;
}

export function entryTypeForTaxCode(code: string, module: 'SLS' | 'PUR'): ErpTaxEntryType {
  const c = code.toUpperCase();
  if (c.startsWith('PPN')) return module === 'SLS' ? ErpTaxEntryType.PPN_KELUARAN : ErpTaxEntryType.PPN_MASUKAN;
  if (c.startsWith('PPH22')) return ErpTaxEntryType.PPH_22;
  if (c.startsWith('PPH23')) return ErpTaxEntryType.PPH_23;
  if (c.startsWith('PPH21')) return ErpTaxEntryType.PPH_21;
  if (c.startsWith('PPH42') || c.startsWith('PPH4')) return ErpTaxEntryType.PPH_4_2;
  if (c.startsWith('PPH26')) return ErpTaxEntryType.PPH_26;
  if (c.startsWith('PPH25')) return ErpTaxEntryType.PPH_25;
  return ErpTaxEntryType.OTHER;
}

export async function syncTaxEntries(prisma: PrismaService, actorId?: bigint) {
  let created = 0;
  let skipped = 0;

  const taxes = await prisma.erpTax.findMany({ select: { id: true, code: true, rate: true } });
  const taxById = new Map(taxes.map((t) => [t.id.toString(), t]));

  const salesInvoices = await prisma.erpSlsInvoice.findMany({
    where: { postingStatus: 'POSTED', deletedAt: null },
    include: { lines: true },
  });
  const purInvoices = await prisma.erpPurInvoice.findMany({
    where: { postingStatus: 'POSTED', deletedAt: null },
    include: { lines: true },
  });

  const partnerCache = new Map<string, { name: string; taxNumber: string | null } | null>();
  const partnerOf = async (id: bigint | null) => {
    if (!id) return null;
    const key = id.toString();
    if (!partnerCache.has(key)) {
      const p = await prisma.erpPartner.findUnique({
        where: { id },
        select: { name: true, taxNumber: true },
      });
      partnerCache.set(key, p);
    }
    return partnerCache.get(key) ?? null;
  };

  const process = async (
    inv: any,
    module: 'SLS' | 'PUR',
    sourceDocType: string,
    partnerId: bigint | null,
  ) => {
    const agg = aggregateLines(inv.lines ?? []);
    for (const a of agg.values()) {
      const existing = await prisma.erpFinTaxEntry.findFirst({
        where: { module, sourceDocType, sourceId: inv.id, taxId: a.taxId, deletedAt: null },
        select: { id: true },
      });
      if (existing) {
        skipped += 1;
        continue;
      }
      const tax = taxById.get(a.taxId.toString());
      if (!tax) {
        skipped += 1;
        continue;
      }
      const partner = await partnerOf(partnerId);
      await prisma.erpFinTaxEntry.create({
        data: {
          module,
          sourceDocType,
          sourceId: inv.id,
          docNumber: inv.docNumber,
          transactionDate: inv.docDate,
          fiscalPeriodId: inv.fiscalPeriodId,
          partnerId,
          partnerNpwp: partner?.taxNumber ?? null,
          partnerName: partner?.name ?? null,
          taxId: a.taxId,
          taxEntryType: entryTypeForTaxCode(tax.code, module),
          dpp: String(a.dpp),
          taxRate: tax.rate.toString(),
          taxAmount: String(a.taxAmount),
          fakturNumber: inv.taxInvoiceNo ?? null,
          status: 'DRAFT',
          currencyId: inv.currencyId,
          exchangeRate: inv.exchangeRate.toString(),
          createdById: actorId ?? null,
        },
      });
      created += 1;
    }
  };

  for (const inv of salesInvoices) {
    await process(inv, 'SLS', 'sls_invoices', inv.customerId ?? null);
  }
  for (const inv of purInvoices) {
    await process(inv, 'PUR', 'pur_invoices', inv.supplierId ?? null);
  }
  return { created, skipped, salesInvoices: salesInvoices.length, purchaseInvoices: purInvoices.length };
}
