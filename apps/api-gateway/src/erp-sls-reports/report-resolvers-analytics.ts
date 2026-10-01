/**
 * 10 analytics resolvers for Sales reports using Prisma GROUP BY and $queryRaw.
 * Column definitions live in report-resolvers-analytics-cols.ts.
 */

import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { num, resolvePartners, resolveRefs, SOFT_DELETE } from './report-helpers';
import { ReportDef, ReportFilters, ReportSummaryItem } from './report-types';
import {
  SUMMARY_COLS, BY_CUSTOMER_COLS, BY_SALESMAN_COLS, BY_ITEM_COLS,
  BY_PROJECT_COLS, BY_DIVISION_COLS, BY_CC_COLS, BY_ITEM_CAT_COLS,
  REV_COLLECTION_COLS, BY_GROUP_COLS,
} from './report-resolvers-analytics-cols';

/* ---------------------------------------------------------------- helpers */

function buildDateFilter(filters: ReportFilters): { gte?: Date; lte?: Date } | undefined {
  const range: { gte?: Date; lte?: Date } = {};
  if (filters.dateFrom) range.gte = new Date(filters.dateFrom);
  if (filters.dateTo) range.lte = new Date(filters.dateTo);
  return Object.keys(range).length ? range : undefined;
}

function dateWhereSql(df: ReturnType<typeof buildDateFilter>, field: Prisma.Sql) {
  if (!df) return Prisma.sql``;
  const from = df.gte ?? new Date('2000-01-01');
  const to = df.lte ?? new Date('2100-01-01');
  return Prisma.sql`AND ${field} >= ${from} AND ${field} <= ${to}`;
}

function totalSummary(rows: Record<string, unknown>[], key: string, label: string): ReportSummaryItem {
  return { label, value: rows.reduce((s, r) => s + num(r[key] as number), 0), type: 'money' };
}

/* ---------------------------------------------------------------- resolvers */

export function buildAnalyticsReports(prisma: PrismaService): ReportDef[] {
  return [
    {
      key: 'summary', title: 'Ringkasan Penjualan', group: 'analytics', columns: SUMMARY_COLS,
      resolve: async (filters: ReportFilters) => {
        const df = buildDateFilter(filters);
        const where: Prisma.ErpSlsInvoiceWhereInput = { ...SOFT_DELETE, status: { in: ['POSTED', 'APPROVED'] as any }, ...(df ? { docDate: df } : {}) };
        const grouped = await prisma.erpSlsInvoice.groupBy({ by: ['docDate'], where, _count: { id: true }, _sum: { subtotal: true, tax1Amount: true, grandTotal: true }, orderBy: { docDate: 'asc' } });
        const monthMap = new Map<string, { count: number; subtotal: number; tax: number; grandTotal: number }>();
        for (const g of grouped) {
          const d = g.docDate;
          const period = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
          const prev = monthMap.get(period) ?? { count: 0, subtotal: 0, tax: 0, grandTotal: 0 };
          monthMap.set(period, { count: prev.count + (g._count.id ?? 0), subtotal: prev.subtotal + num(g._sum.subtotal), tax: prev.tax + num(g._sum.tax1Amount), grandTotal: prev.grandTotal + num(g._sum.grandTotal) });
        }
        const rows = [...monthMap.entries()].map(([period, v]) => ({ period, count: v.count, subtotal: v.subtotal, tax: v.tax, grandTotal: v.grandTotal }));
        return { rows, total: rows.length, summary: [totalSummary(rows, 'grandTotal', 'Total Grand Total')] };
      },
    },
    {
      key: 'by-customer', title: 'Penjualan per Pelanggan', group: 'analytics', columns: BY_CUSTOMER_COLS,
      resolve: async (filters: ReportFilters) => {
        const df = buildDateFilter(filters);
        const where: Prisma.ErpSlsInvoiceWhereInput = { ...SOFT_DELETE, ...(df ? { docDate: df } : {}), ...(filters.status ? { status: filters.status as any } : {}) };
        const grouped = await prisma.erpSlsInvoice.groupBy({ by: ['customerId'], where, _count: { id: true }, _sum: { subtotal: true, tax1Amount: true, grandTotal: true }, orderBy: { _sum: { grandTotal: 'desc' } } });
        const partners = await resolvePartners(prisma, grouped.map((g) => g.customerId));
        const rows = grouped.map((g) => {
          const p = g.customerId ? partners.get(g.customerId.toString()) : null;
          return { customerCode: p?.code ?? '', customerName: p?.name ?? '', invoiceCount: g._count.id, subtotal: num(g._sum.subtotal), tax: num(g._sum.tax1Amount), grandTotal: num(g._sum.grandTotal) };
        });
        return { rows, total: rows.length, summary: [totalSummary(rows, 'grandTotal', 'Total Grand Total')] };
      },
    },
    {
      key: 'by-salesman', title: 'Penjualan per Salesman', group: 'analytics', columns: BY_SALESMAN_COLS,
      resolve: async (filters: ReportFilters) => {
        const df = buildDateFilter(filters);
        const dw = dateWhereSql(df, Prisma.sql`i.doc_date`);
        type RawRow = { salesman_id: bigint | null; salesman_code: string | null; salesman_name: string | null; invoice_count: bigint; grand_total: number };
        const raw: RawRow[] = await prisma.$queryRaw`
          SELECT c.salesman_id, s.code AS salesman_code, s.name AS salesman_name,
                 COUNT(i.id) AS invoice_count,
                 COALESCE(SUM(i.grand_total), 0) AS grand_total
          FROM sls_invoices i JOIN md_partners c ON c.id = i.customer_id
          LEFT JOIN md_partners s ON s.id = c.salesman_id
          WHERE i.deleted_at IS NULL ${dw}
          GROUP BY c.salesman_id, s.code, s.name ORDER BY grand_total DESC`;
        const rows = raw.map((r) => ({ salesmanCode: r.salesman_code ?? '(Tanpa Sales)', salesmanName: r.salesman_name ?? '', invoiceCount: Number(r.invoice_count), grandTotal: num(r.grand_total) }));
        return { rows, total: rows.length, summary: [totalSummary(rows, 'grandTotal', 'Total Grand Total')] };
      },
    },
    {
      key: 'by-item', title: 'Penjualan per Item', group: 'analytics', columns: BY_ITEM_COLS,
      resolve: async (filters: ReportFilters) => {
        const df = buildDateFilter(filters);
        const dw = dateWhereSql(df, Prisma.sql`i.doc_date`);
        type RawRow = { item_id: bigint; item_code: string; item_name: string; qty: number; subtotal: number };
        const raw: RawRow[] = await prisma.$queryRaw`
          SELECT l.item_id, m.code AS item_code, m.name AS item_name,
                 SUM(l.base_quantity) AS qty, SUM(l.unit_price * l.base_quantity) AS subtotal
          FROM sls_invoice_lines l JOIN sls_invoices i ON i.id = l.invoice_id
          JOIN md_items m ON m.id = l.item_id
          WHERE i.deleted_at IS NULL ${dw}
          GROUP BY l.item_id, m.code, m.name ORDER BY subtotal DESC`;
        const rows = raw.map((r) => ({ itemCode: r.item_code, itemName: r.item_name, qty: num(r.qty), subtotal: num(r.subtotal) }));
        return { rows, total: rows.length, summary: [totalSummary(rows, 'subtotal', 'Total Subtotal')] };
      },
    },
    {
      key: 'by-project', title: 'Penjualan per Proyek', group: 'analytics', columns: BY_PROJECT_COLS,
      resolve: async (filters: ReportFilters) => {
        const df = buildDateFilter(filters);
        const dw = dateWhereSql(df, Prisma.sql`i.doc_date`);
        type RawRow = { project_id: bigint | null; project_code: string | null; project_name: string | null; qty: number; subtotal: number };
        const raw: RawRow[] = await prisma.$queryRaw`
          SELECT l.project_id, p.code AS project_code, p.name AS project_name,
                 SUM(l.base_quantity) AS qty, SUM(l.unit_price * l.base_quantity) AS subtotal
          FROM sls_invoice_lines l JOIN sls_invoices i ON i.id = l.invoice_id
          LEFT JOIN md_projects p ON p.id = l.project_id
          WHERE i.deleted_at IS NULL AND l.project_id IS NOT NULL ${dw}
          GROUP BY l.project_id, p.code, p.name ORDER BY subtotal DESC`;
        const rows = raw.map((r) => ({ projectCode: r.project_code ?? '', projectName: r.project_name ?? '', qty: num(r.qty), subtotal: num(r.subtotal) }));
        return { rows, total: rows.length, summary: [totalSummary(rows, 'subtotal', 'Total Subtotal')] };
      },
    },
    {
      key: 'by-division', title: 'Penjualan per Divisi', group: 'analytics', columns: BY_DIVISION_COLS,
      resolve: async (filters: ReportFilters) => {
        const df = buildDateFilter(filters);
        const where: Prisma.ErpSlsInvoiceWhereInput = { ...SOFT_DELETE, ...(df ? { docDate: df } : {}), ...(filters.status ? { status: filters.status as any } : {}) };
        const grouped = await prisma.erpSlsInvoice.groupBy({ by: ['salesDeptId'], where, _count: { id: true }, _sum: { grandTotal: true }, orderBy: { _sum: { grandTotal: 'desc' } } });
        const divIds = grouped.map((g) => g.salesDeptId).filter((id): id is bigint => id != null);
        const divMap = await resolveRefs(prisma.erpDivision.findMany({ where: { id: { in: divIds } }, select: { id: true, code: true, name: true } }));
        const rows = grouped.map((g) => {
          const d = g.salesDeptId ? divMap.get(g.salesDeptId.toString()) : null;
          return { divisionCode: d?.code ?? '(Tanpa Divisi)', divisionName: d?.name ?? '', invoiceCount: g._count.id, grandTotal: num(g._sum.grandTotal) };
        });
        return { rows, total: rows.length, summary: [totalSummary(rows, 'grandTotal', 'Total Grand Total')] };
      },
    },
    {
      key: 'by-cost-center', title: 'Penjualan per Cost Center', group: 'analytics', columns: BY_CC_COLS,
      resolve: async (filters: ReportFilters) => {
        const df = buildDateFilter(filters);
        const dw = dateWhereSql(df, Prisma.sql`i.doc_date`);
        type RawRow = { cc_id: bigint | null; cc_code: string | null; cc_name: string | null; qty: number; subtotal: number };
        const raw: RawRow[] = await prisma.$queryRaw`
          SELECT l.cost_center_id AS cc_id, cc.code AS cc_code, cc.name AS cc_name,
                 SUM(l.base_quantity) AS qty, SUM(l.unit_price * l.base_quantity) AS subtotal
          FROM sls_invoice_lines l JOIN sls_invoices i ON i.id = l.invoice_id
          LEFT JOIN md_cost_centers cc ON cc.id = l.cost_center_id
          WHERE i.deleted_at IS NULL AND l.cost_center_id IS NOT NULL ${dw}
          GROUP BY l.cost_center_id, cc.code, cc.name ORDER BY subtotal DESC`;
        const rows = raw.map((r) => ({ ccCode: r.cc_code ?? '', ccName: r.cc_name ?? '', qty: num(r.qty), subtotal: num(r.subtotal) }));
        return { rows, total: rows.length, summary: [totalSummary(rows, 'subtotal', 'Total Subtotal')] };
      },
    },
    {
      key: 'by-item-category', title: 'Penjualan per Kategori Item', group: 'analytics', columns: BY_ITEM_CAT_COLS,
      resolve: async (filters: ReportFilters) => {
        const df = buildDateFilter(filters);
        const dw = dateWhereSql(df, Prisma.sql`i.doc_date`);
        type RawRow = { category_id: bigint | null; category_code: string | null; category_name: string | null; qty: number; subtotal: number };
        const raw: RawRow[] = await prisma.$queryRaw`
          SELECT m.category_id, ic.code AS category_code, ic.name AS category_name,
                 SUM(l.base_quantity) AS qty, SUM(l.unit_price * l.base_quantity) AS subtotal
          FROM sls_invoice_lines l JOIN sls_invoices i ON i.id = l.invoice_id
          JOIN md_items m ON m.id = l.item_id
          LEFT JOIN md_item_categories ic ON ic.id = m.category_id
          WHERE i.deleted_at IS NULL ${dw}
          GROUP BY m.category_id, ic.code, ic.name ORDER BY subtotal DESC`;
        const rows = raw.map((r) => ({ categoryCode: r.category_code ?? '(Tanpa Kategori)', categoryName: r.category_name ?? '', qty: num(r.qty), subtotal: num(r.subtotal) }));
        return { rows, total: rows.length, summary: [totalSummary(rows, 'subtotal', 'Total Subtotal')] };
      },
    },
    {
      key: 'revenue-collection', title: 'Pendapatan vs Penerimaan', group: 'analytics', columns: REV_COLLECTION_COLS,
      resolve: async (filters: ReportFilters) => {
        const df = buildDateFilter(filters);
        const invDw = dateWhereSql(df, Prisma.sql`i.doc_date`);
        const rcpDw = dateWhereSql(df, Prisma.sql`r.transaction_date`);
        type InvRow = { period: string; invoiced: number };
        type RcpRow = { period: string; collected: number };
        const [invRows, rcpRows] = await Promise.all([
          prisma.$queryRaw<InvRow[]>`SELECT TO_CHAR(i.doc_date,'YYYY-MM') AS period, COALESCE(SUM(i.grand_total),0) AS invoiced FROM sls_invoices i WHERE i.deleted_at IS NULL AND i.status IN ('POSTED','APPROVED') ${invDw} GROUP BY period ORDER BY period`,
          prisma.$queryRaw<RcpRow[]>`SELECT TO_CHAR(r.transaction_date,'YYYY-MM') AS period, COALESCE(SUM(r.amount),0) AS collected FROM fin_ar_receipts r WHERE r.deleted_at IS NULL AND r.status IN ('POSTED','APPROVED') ${rcpDw} GROUP BY period ORDER BY period`,
        ]);
        const periods = new Set([...invRows.map((r) => r.period), ...rcpRows.map((r) => r.period)]);
        const invMap = new Map(invRows.map((r) => [r.period, num(r.invoiced)]));
        const rcpMap = new Map(rcpRows.map((r) => [r.period, num(r.collected)]));
        const rows = [...periods].sort().map((period) => {
          const invoiced = invMap.get(period) ?? 0;
          const collected = rcpMap.get(period) ?? 0;
          return { period, invoiced, collected, outstanding: invoiced - collected };
        });
        return { rows, total: rows.length, summary: [{ label: 'Total Ditagih', value: rows.reduce((s, r) => s + r.invoiced, 0), type: 'money' as const }, { label: 'Total Diterima', value: rows.reduce((s, r) => s + r.collected, 0), type: 'money' as const }, { label: 'Outstanding', value: rows.reduce((s, r) => s + r.outstanding, 0), type: 'money' as const }] };
      },
    },
    {
      key: 'by-group', title: 'Penjualan per Grup Pelanggan', group: 'analytics', columns: BY_GROUP_COLS,
      resolve: async (filters: ReportFilters) => {
        const df = buildDateFilter(filters);
        const dw = dateWhereSql(df, Prisma.sql`i.doc_date`);
        type RawRow = { group_id: bigint | null; group_code: string | null; group_name: string | null; customer_count: bigint; invoice_count: bigint; grand_total: number };
        const raw: RawRow[] = await prisma.$queryRaw`
          SELECT c.category_id AS group_id, pc.code AS group_code, pc.name AS group_name,
                 COUNT(DISTINCT i.customer_id) AS customer_count,
                 COUNT(i.id) AS invoice_count,
                 COALESCE(SUM(i.grand_total),0) AS grand_total
          FROM sls_invoices i JOIN md_partners c ON c.id = i.customer_id
          LEFT JOIN md_partner_categories pc ON pc.id = c.category_id
          WHERE i.deleted_at IS NULL ${dw}
          GROUP BY c.category_id, pc.code, pc.name ORDER BY grand_total DESC`;
        const rows = raw.map((r) => ({ groupCode: r.group_code ?? '(Tanpa Grup)', groupName: r.group_name ?? '', customerCount: Number(r.customer_count), invoiceCount: Number(r.invoice_count), grandTotal: num(r.grand_total) }));
        return { rows, total: rows.length, summary: [totalSummary(rows, 'grandTotal', 'Total Grand Total')] };
      },
    },
  ];
}
