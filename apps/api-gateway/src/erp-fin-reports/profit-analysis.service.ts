import { Injectable } from '@nestjs/common';
import { PrismaService } from '../prisma/prisma.service';

export type ProfitDimension = 'SCHOOL' | 'PRODUCT' | 'REGION';

const round2 = (v: number) => Math.round(v * 100) / 100;

interface Bucket {
  key: string;
  label: string;
  revenue: number;
  cogs: number;
  docIds: Set<string>;
}

/**
 * E1 — gross profit per school (customer), product, or region, computed
 * from POSTED sales invoices minus POSTED sales returns in the period.
 * Revenue is net of line discounts; COGS uses each line's unitCost.
 */
@Injectable()
export class ProfitAnalysisService {
  constructor(private readonly prisma: PrismaService) {}

  async build(from?: string, to?: string, dimension: ProfitDimension = 'SCHOOL') {
    const now = new Date();
    const fromDate = from ? new Date(from) : new Date(Date.UTC(now.getUTCFullYear(), 0, 1));
    const toDate = to ? new Date(to) : now;

    const lineSelect = {
      itemId: true, quantity: true, unitPrice: true, discountAmount: true,
      baseQuantity: true, unitCost: true,
    } as const;
    const docWhere = {
      deletedAt: null, postingStatus: 'POSTED' as const,
      docDate: { gte: fromDate, lte: toDate },
    };
    const [invoices, returns] = await Promise.all([
      this.prisma.erpSlsInvoice.findMany({
        where: docWhere,
        select: { id: true, customerId: true, lines: { select: lineSelect } },
      }),
      this.prisma.erpSlsReturn.findMany({
        where: docWhere,
        select: { id: true, customerId: true, lines: { select: lineSelect } },
      }),
    ]);

    // Label resolution: partners (+ region via first address), items.
    const customerIds = [
      ...new Set(
        [...invoices, ...returns]
          .map((d) => d.customerId)
          .filter((v): v is bigint => v != null),
      ),
    ];
    const itemIds = [
      ...new Set(
        [...invoices, ...returns].flatMap((d) => d.lines.map((l) => l.itemId)),
      ),
    ];
    const [partners, items, addresses] = await Promise.all([
      customerIds.length
        ? this.prisma.erpPartner.findMany({
            where: { id: { in: customerIds } },
            select: { id: true, code: true, name: true },
          })
        : Promise.resolve([]),
      itemIds.length
        ? this.prisma.erpItem.findMany({
            where: { id: { in: itemIds } },
            select: { id: true, code: true, name: true },
          })
        : Promise.resolve([]),
      dimension === 'REGION' && customerIds.length
        ? this.prisma.erpPartnerAddress.findMany({
            where: { partnerId: { in: customerIds }, deletedAt: null },
            select: {
              partnerId: true,
              subArea: { select: { name: true } },
              area: { select: { name: true } },
            },
            orderBy: { id: 'asc' },
          })
        : Promise.resolve([]),
    ]);
    const partnerLabel = new Map(
      partners.map((p) => [p.id.toString(), `${p.code ? `${p.code} — ` : ''}${p.name}`]),
    );
    const itemLabel = new Map(
      items.map((i) => [i.id.toString(), `${i.code} — ${i.name}`]),
    );
    const regionOf = new Map<string, string>();
    for (const a of addresses) {
      const key = a.partnerId.toString();
      if (!regionOf.has(key)) {
        regionOf.set(key, a.subArea?.name ?? a.area?.name ?? '—');
      }
    }

    const buckets = new Map<string, Bucket>();
    const add = (key: string, label: string, revenue: number, cogs: number, docId: bigint) => {
      let b = buckets.get(key);
      if (!b) {
        b = { key, label, revenue: 0, cogs: 0, docIds: new Set() };
        buckets.set(key, b);
      }
      b.revenue += revenue;
      b.cogs += cogs;
      b.docIds.add(docId.toString());
    };

    const process = (
      docs: typeof invoices, sign: 1 | -1,
    ) => {
      for (const doc of docs) {
        const custKey = doc.customerId ? doc.customerId.toString() : 'none';
        for (const l of doc.lines) {
          const revenue = sign * (Number(l.quantity) * Number(l.unitPrice) - Number(l.discountAmount));
          const cogs = sign * (Number(l.baseQuantity) * Number(l.unitCost));
          if (dimension === 'PRODUCT') {
            const k = l.itemId.toString();
            add(k, itemLabel.get(k) ?? k, revenue, cogs, doc.id);
          } else if (dimension === 'REGION') {
            const label = doc.customerId ? regionOf.get(custKey) ?? '—' : '—';
            add(label, label, revenue, cogs, doc.id);
          } else {
            add(
              custKey,
              doc.customerId ? partnerLabel.get(custKey) ?? custKey : 'Tanpa Customer',
              revenue, cogs, doc.id,
            );
          }
        }
      }
    };
    process(invoices, 1);
    process(returns, -1);

    const rows = [...buckets.values()]
      .map((b) => {
        const grossProfit = b.revenue - b.cogs;
        return {
          key: b.key,
          label: b.label,
          revenue: round2(b.revenue),
          cogs: round2(b.cogs),
          grossProfit: round2(grossProfit),
          marginPercent: b.revenue > 0 ? round2((grossProfit / b.revenue) * 100) : 0,
          docCount: b.docIds.size,
        };
      })
      .sort((a, b) => b.grossProfit - a.grossProfit);

    const totalRevenue = round2(rows.reduce((s, r) => s + r.revenue, 0));
    const totalCogs = round2(rows.reduce((s, r) => s + r.cogs, 0));
    const totalGross = round2(totalRevenue - totalCogs);
    return {
      dimension,
      from: fromDate.toISOString().slice(0, 10),
      to: toDate.toISOString().slice(0, 10),
      rows,
      totals: {
        revenue: totalRevenue,
        cogs: totalCogs,
        grossProfit: totalGross,
        marginPercent: totalRevenue > 0 ? round2((totalGross / totalRevenue) * 100) : 0,
        docCount: rows.reduce((s, r) => s + r.docCount, 0),
      },
    };
  }
}
