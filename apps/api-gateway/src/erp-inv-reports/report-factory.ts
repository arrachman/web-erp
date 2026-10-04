/**
 * Declarative report factories for Inventory (M3). Two shapes cover most reports:
 *  - makeDocumentListReport: period-filtered, paginated document list + totals
 *  - makeOutstandingReport:  open-document aging OR recap by one dimension
 * Output is a plain ReportDef, so it slots into the existing registry Map.
 * Read-only, Decimal-safe (num()), soft-delete guarded (baseWhere), batch
 * ref resolution (no N+1).
 */

import { PrismaService } from '../prisma/prisma.service';
import {
  PartnerInfo,
  baseWhere,
  display,
  isoDate,
  num,
  paginate,
  resolvePartners,
} from '../erp-sls-reports/report-helpers';
import {
  ReportChart,
  ReportColType,
  ReportColumn,
  ReportDef,
  ReportGroup,
  ReportSummaryItem,
} from './report-types';

type Row = Record<string, unknown>;
type Numeric = Parameters<typeof num>[0];

const MS_PER_DAY = 86_400_000;
const MAX_AGING_ROWS = 10_000;
const TOP_CHART_ITEMS = 10;
const DEFAULT_BUCKET_DAYS = [30, 60, 90];
const NO_PARTNER_LABEL = '(Tanpa Partner)';
const NOT_PAID: Row = { settlementStatus: { not: 'PAID' } };

/* ------------------------------------------------------- dynamic delegate */

/** The only Prisma surface the factories touch; every model delegate has it. */
interface ModelDelegate {
  count(args: { where: Row }): Promise<number>;
  findMany(args: {
    where: Row;
    orderBy?: Row;
    skip?: number;
    take?: number;
    select?: Record<string, true>;
  }): Promise<Row[]>;
  aggregate(args: { where: Row; _sum: Record<string, true> }): Promise<{ _sum: Row }>;
  groupBy(args: {
    by: string[];
    where: Row;
    _count: { _all: true };
    _sum?: Record<string, true>;
  }): Promise<Row[]>;
}

/**
 * The ONE dynamic cast: Prisma delegates are looked up by model key string
 * (e.g. 'erpSlsInvoice'), which the generated client cannot type generically.
 */
function delegateOf(prisma: PrismaService, model: string): ModelDelegate {
  const delegate = (prisma as unknown as Record<string, ModelDelegate | undefined>)[model];
  if (!delegate) throw new Error(`Model Prisma '${model}' tidak ditemukan.`);
  return delegate;
}

/* ------------------------------------------------------------- shared bits */

function cell(value: unknown, type: ReportColType): unknown {
  switch (type) {
    case 'date':
      return value instanceof Date ? isoDate(value) : '';
    case 'money':
    case 'number':
    case 'qty':
    case 'percent':
      return num(value as Numeric);
    default:
      return value == null ? '' : String(value);
  }
}

function partnerLabel(partners: Map<string, PartnerInfo>, id: unknown): string {
  if (id == null) return NO_PARTNER_LABEL;
  return display(partners.get(String(id)) ?? null) || NO_PARTNER_LABEL;
}

function scopeWhere(
  filters: Parameters<typeof baseWhere>[1],
  dateField: string,
  partnerField?: string,
): Row {
  return {
    ...baseWhere(dateField, filters),
    ...(partnerField && filters.partnerId ? { [partnerField]: BigInt(filters.partnerId) } : {}),
  };
}

function topN(entries: [string, number][]): { labels: string[]; values: number[] } {
  const top = [...entries].sort((a, b) => b[1] - a[1]).slice(0, TOP_CHART_ITEMS);
  return { labels: top.map(([l]) => l), values: top.map(([, v]) => v) };
}

/* ----------------------------------------------------- A. document list */

export interface DocumentListConfig {
  key: string;
  title: string;
  group: ReportGroup;
  /** Prisma client delegate key, e.g. 'erpSlsInvoice'. */
  model: string;
  /** Real header date column used for range filter + ordering, e.g. 'docDate'. */
  dateField: string;
  /** FK column holding the partner id (customerId / supplierId / partnerId). */
  partnerField?: string;
  /** Output column key that receives the resolved partner display string. */
  partnerColumn?: string;
  /** Columns; every key except partnerColumn MUST be a real field of the model. */
  columns: ReportColumn[];
  /** Decimal column summed for 'Total Nilai' (omit for amount-less documents). */
  amountField?: string;
}

function selectFor(cfg: DocumentListConfig): Record<string, true> {
  const fields = new Set<string>(['id', 'status', cfg.dateField]);
  for (const c of cfg.columns) if (c.key !== cfg.partnerColumn) fields.add(c.key);
  if (cfg.partnerField) fields.add(cfg.partnerField);
  if (cfg.amountField) fields.add(cfg.amountField);
  return Object.fromEntries([...fields].map((f) => [f, true as const]));
}

function statusChart(groups: Row[]): ReportChart {
  return {
    kind: 'donut',
    title: 'Dokumen per Status',
    labels: groups.map((g) => String(g.status ?? '-')),
    values: groups.map((g) => (g._count as { _all: number })._all),
  };
}

export function makeDocumentListReport(prisma: PrismaService, cfg: DocumentListConfig): ReportDef {
  const delegate = delegateOf(prisma, cfg.model);
  return {
    key: cfg.key,
    title: cfg.title,
    group: cfg.group,
    columns: cfg.columns,
    resolve: async (filters) => {
      const { skip, take } = paginate(filters);
      const where = scopeWhere(filters, cfg.dateField, cfg.partnerField);
      const sumSelect: Record<string, true> | undefined = cfg.amountField
        ? { [cfg.amountField]: true }
        : undefined;
      const [total, docs, sums, byStatus] = await Promise.all([
        delegate.count({ where }),
        delegate.findMany({
          where,
          orderBy: { [cfg.dateField]: 'desc' },
          skip,
          take,
          select: selectFor(cfg),
        }),
        sumSelect
          ? delegate.aggregate({ where, _sum: sumSelect })
          : Promise.resolve({ _sum: {} as Row }),
        delegate.groupBy({ by: ['status'], where, _count: { _all: true }, ...(sumSelect ? { _sum: sumSelect } : {}) }),
      ]);

      const partners = cfg.partnerField
        ? await resolvePartners(
            prisma,
            docs.map((d) => (d[cfg.partnerField as string] as bigint | null) ?? null),
          )
        : new Map<string, PartnerInfo>();

      const rows = docs.map((doc) => {
        const row: Row = {};
        for (const col of cfg.columns) {
          row[col.key] =
            col.key === cfg.partnerColumn
              ? partnerLabel(partners, cfg.partnerField ? doc[cfg.partnerField] : null)
              : cell(doc[col.key], col.type);
        }
        return row;
      });

      const summary: ReportSummaryItem[] = [{ label: 'Total Dokumen', value: total, type: 'number' }];
      if (cfg.amountField) {
        summary.push({
          label: 'Total Nilai',
          value: num(sums._sum[cfg.amountField] as Numeric),
          type: 'money',
        });
      }
      return { rows, total, summary, charts: [statusChart(byStatus)] };
    },
  };
}

/* ------------------------------------------------------- B. outstanding */

export interface OutstandingConfig {
  key: string;
  title: string;
  group: ReportGroup;
  basis: 'aging' | 'recap';
  model: string;
  dateField: string;
  partnerField: string;
  /** Decimal column holding the document amount (e.g. 'grandTotal'). */
  amountField: string;
  /** Optional settled-amount column; outstanding = amount - paid. */
  paidField?: string;
  /** Aging age is measured from this date when set, else from dateField. */
  dueDateField?: string;
  /** Extra open-document predicate. Aging defaults to settlementStatus != PAID. */
  openWhere?: Row;
  /** Aging: upper bounds in days past due (default [30, 60, 90]). */
  bucketDays?: number[];
  /** Recap: dimension to group by (default 'partner'). */
  groupBy?: 'partner' | 'status';
}

function bucketLabels(days: number[]): string[] {
  const labels = ['Belum JT'];
  let from = 1;
  for (const d of days) {
    labels.push(`${from}-${d}`);
    from = d + 1;
  }
  labels.push(`>${days[days.length - 1]}`);
  return labels;
}

function bucketIndex(ageDays: number, days: number[]): number {
  if (ageDays <= 0) return 0;
  const hit = days.findIndex((d) => ageDays <= d);
  return hit === -1 ? days.length + 1 : hit + 1;
}

function agingColumns(labels: string[]): ReportColumn[] {
  return [
    { key: 'partner', header: 'Partner', type: 'text' },
    ...labels.map((l, i): ReportColumn => ({ key: `b${i}`, header: l, type: 'money' })),
    { key: 'total', header: 'Total', type: 'money' },
  ];
}

function makeAging(prisma: PrismaService, cfg: OutstandingConfig): ReportDef {
  const delegate = delegateOf(prisma, cfg.model);
  const days = cfg.bucketDays ?? DEFAULT_BUCKET_DAYS;
  const labels = bucketLabels(days);
  const ageField = cfg.dueDateField ?? cfg.dateField;
  const select: Record<string, true> = { id: true, [cfg.partnerField]: true, [cfg.amountField]: true, [ageField]: true };
  if (cfg.paidField) select[cfg.paidField] = true;

  return {
    key: cfg.key,
    title: cfg.title,
    group: cfg.group,
    columns: agingColumns(labels),
    resolve: async (filters) => {
      const where = { ...scopeWhere(filters, cfg.dateField, cfg.partnerField), ...(cfg.openWhere ?? NOT_PAID) };
      const docs = await delegate.findMany({ where, take: MAX_AGING_ROWS, select });
      const partners = await resolvePartners(prisma, docs.map((d) => (d[cfg.partnerField] as bigint | null) ?? null));
      const refTime = (filters.asOfDate ? new Date(filters.asOfDate) : new Date()).getTime();

      const byPartner = new Map<string, number[]>();
      for (const doc of docs) {
        const open = num(doc[cfg.amountField] as Numeric) - (cfg.paidField ? num(doc[cfg.paidField] as Numeric) : 0);
        if (open <= 0) continue;
        const due = doc[ageField] instanceof Date ? (doc[ageField] as Date).getTime() : refTime;
        const idx = bucketIndex(Math.floor((refTime - due) / MS_PER_DAY), days);
        const label = partnerLabel(partners, doc[cfg.partnerField]);
        const sums = byPartner.get(label) ?? labels.map(() => 0);
        sums[idx] += open;
        byPartner.set(label, sums);
      }

      const rows = [...byPartner.entries()]
        .map(([partner, sums]) => ({
          partner,
          ...Object.fromEntries(sums.map((v, i) => [`b${i}`, v])),
          total: sums.reduce((s, v) => s + v, 0),
        }))
        .sort((a, b) => b.total - a.total);

      const bucketTotals = labels.map((_, i) => rows.reduce((s, r) => s + (r as Row as Record<string, number>)[`b${i}`], 0));
      const summary: ReportSummaryItem[] = [
        { label: 'Total Outstanding', value: bucketTotals.reduce((s, v) => s + v, 0), type: 'money' },
        { label: 'Jumlah Partner', value: rows.length, type: 'number' },
      ];
      const charts: ReportChart[] = [{ kind: 'bar', title: 'Outstanding per Umur (hari)', labels, values: bucketTotals }];
      return { rows, total: rows.length, summary, charts };
    },
  };
}

const RECAP_COLUMNS = (dimension: string): ReportColumn[] => [
  { key: 'label', header: dimension, type: 'text' },
  { key: 'count', header: 'Jumlah Dok.', type: 'number' },
  { key: 'amount', header: 'Total Nilai', type: 'money' },
];

function makeRecap(prisma: PrismaService, cfg: OutstandingConfig): ReportDef {
  const delegate = delegateOf(prisma, cfg.model);
  const dimension = cfg.groupBy ?? 'partner';
  const by = dimension === 'status' ? 'status' : cfg.partnerField;

  return {
    key: cfg.key,
    title: cfg.title,
    group: cfg.group,
    columns: RECAP_COLUMNS(dimension === 'status' ? 'Status' : 'Partner'),
    resolve: async (filters) => {
      const where = { ...scopeWhere(filters, cfg.dateField, cfg.partnerField), ...(cfg.openWhere ?? {}) };
      const groups = await delegate.groupBy({ by: [by], where, _count: { _all: true }, _sum: { [cfg.amountField]: true } });
      const partners =
        dimension === 'partner'
          ? await resolvePartners(prisma, groups.map((g) => (g[by] as bigint | null) ?? null))
          : new Map<string, PartnerInfo>();

      const rows = groups
        .map((g) => ({
          label: dimension === 'status' ? String(g.status ?? '-') : partnerLabel(partners, g[by]),
          count: (g._count as { _all: number })._all,
          amount: num((g._sum as Row)[cfg.amountField] as Numeric),
        }))
        .sort((a, b) => b.amount - a.amount);

      const summary: ReportSummaryItem[] = [
        { label: 'Total Dokumen', value: rows.reduce((s, r) => s + r.count, 0), type: 'number' },
        { label: 'Total Nilai', value: rows.reduce((s, r) => s + r.amount, 0), type: 'money' },
      ];
      const { labels, values } = topN(rows.map((r) => [r.label, r.amount]));
      const charts: ReportChart[] = [
        { kind: dimension === 'status' ? 'donut' : 'bar', title: `Nilai per ${dimension === 'status' ? 'Status' : 'Partner'}`, labels, values },
      ];
      return { rows, total: rows.length, summary, charts };
    },
  };
}

export function makeOutstandingReport(prisma: PrismaService, cfg: OutstandingConfig): ReportDef {
  return cfg.basis === 'aging' ? makeAging(prisma, cfg) : makeRecap(prisma, cfg);
}
