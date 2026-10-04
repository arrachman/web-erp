/**
 * Declarative, read-only Finance document-report factory. Configurations may
 * only use model fields documented in erp-fin.prisma; dynamic delegate access
 * keeps the factory reusable without weakening the public HTTP contract.
 */

import { BadRequestException } from '@nestjs/common';
import { PrismaService } from '../prisma/prisma.service';
import { display, isoDate, num, paginate, resolvePartners } from '../erp-sls-reports/report-helpers';
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
const TOP_CHART_ITEMS = 10;
const NO_PARTNER_LABEL = '(Tanpa Partner)';

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

function delegateOf(prisma: PrismaService, model: string): ModelDelegate {
  const delegate = (prisma as unknown as Record<string, ModelDelegate | undefined>)[model];
  if (!delegate) throw new Error(`Model Prisma '${model}' tidak ditemukan.`);
  return delegate;
}

function readPartnerId(value?: string): bigint | undefined {
  if (!value) return undefined;
  try {
    return BigInt(value);
  } catch {
    throw new BadRequestException('partnerId tidak valid.');
  }
}

function baseWhere(dateField: string, filters: Parameters<ReportDef['resolve']>[0]): Row {
  const dateRange: Row = {};
  if (filters.dateFrom) dateRange.gte = new Date(filters.dateFrom);
  if (filters.dateTo) dateRange.lte = new Date(filters.dateTo);
  return {
    deletedAt: null,
    ...(Object.keys(dateRange).length ? { [dateField]: dateRange } : {}),
    ...(filters.status ? { status: filters.status } : {}),
    ...(filters.search ? { docNumber: { contains: filters.search, mode: 'insensitive' } } : {}),
  };
}

function cell(value: unknown, type: ReportColType): unknown {
  if (type === 'date') return value instanceof Date ? isoDate(value) : '';
  if (type === 'number' || type === 'money' || type === 'qty' || type === 'percent') {
    return num(value as Numeric);
  }
  return value == null ? '' : String(value);
}

function partnerLabel(
  partners: Map<string, { id: string; code: string; name: string }>,
  id: unknown,
): string {
  if (id == null) return NO_PARTNER_LABEL;
  return display(partners.get(String(id)) ?? null) || NO_PARTNER_LABEL;
}

function topChart(entries: Array<[string, number]>): { labels: string[]; values: number[] } {
  const top = [...entries].sort((a, b) => b[1] - a[1]).slice(0, TOP_CHART_ITEMS);
  return { labels: top.map(([label]) => label), values: top.map(([, value]) => value) };
}

export interface DocumentListConfig {
  key: string;
  title: string;
  group: ReportGroup;
  model: string;
  dateField: string;
  columns: ReportColumn[];
  amountField?: string;
  partnerField?: string;
  partnerColumn?: string;
  where?: Row;
}

function selectedFields(cfg: DocumentListConfig): Record<string, true> {
  const fields = new Set(['id', 'status', cfg.dateField]);
  cfg.columns.forEach((column) => {
    if (column.key !== cfg.partnerColumn) fields.add(column.key);
  });
  if (cfg.partnerField) fields.add(cfg.partnerField);
  if (cfg.amountField) fields.add(cfg.amountField);
  return Object.fromEntries([...fields].map((field) => [field, true as const]));
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
      const partnerId = readPartnerId(filters.partnerId);
      const where: Row = {
        ...baseWhere(cfg.dateField, filters),
        ...cfg.where,
        ...(cfg.partnerField && partnerId ? { [cfg.partnerField]: partnerId } : {}),
      };
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
          select: selectedFields(cfg),
        }),
        sumSelect ? delegate.aggregate({ where, _sum: sumSelect }) : Promise.resolve({ _sum: {} as Row }),
        delegate.groupBy({ by: ['status'], where, _count: { _all: true } }),
      ]);
      const partners = cfg.partnerField
        ? await resolvePartners(prisma, docs.map((doc) => (doc[cfg.partnerField!] as bigint | null) ?? null))
        : new Map();
      const rows = docs.map((doc) => Object.fromEntries(cfg.columns.map((column) => [
        column.key,
        column.key === cfg.partnerColumn
          ? partnerLabel(partners, cfg.partnerField ? doc[cfg.partnerField] : null)
          : cell(doc[column.key], column.type),
      ])));
      const summary: ReportSummaryItem[] = [{ label: 'Total Dokumen', value: total, type: 'number' }];
      if (cfg.amountField) {
        summary.push({
          label: 'Total Nilai',
          value: num(sums._sum[cfg.amountField] as Numeric),
          type: 'money',
        });
      }
      const charts: ReportChart[] = [{
        kind: 'donut',
        title: 'Dokumen per Status',
        labels: byStatus.map((entry) => String(entry.status ?? '-')),
        values: byStatus.map((entry) => (entry._count as { _all: number })._all),
      }];
      return { rows, total, summary, charts };
    },
  };
}

export interface StatusRecapConfig {
  key: string;
  title: string;
  group: ReportGroup;
  model: string;
  dateField: string;
  amountField?: string;
  partnerField?: string;
  where?: Row;
}

export function makeStatusRecapReport(prisma: PrismaService, cfg: StatusRecapConfig): ReportDef {
  const delegate = delegateOf(prisma, cfg.model);
  const columns: ReportColumn[] = [
    { key: 'status', header: 'Status', type: 'status' },
    { key: 'count', header: 'Jumlah Dokumen', type: 'number' },
    ...(cfg.amountField ? [{ key: 'amount', header: 'Total Nilai', type: 'money' as const }] : []),
  ];
  return {
    key: cfg.key,
    title: cfg.title,
    group: cfg.group,
    columns,
    resolve: async (filters) => {
      const partnerId = readPartnerId(filters.partnerId);
      const where: Row = {
        ...baseWhere(cfg.dateField, filters),
        ...cfg.where,
        ...(cfg.partnerField && partnerId ? { [cfg.partnerField]: partnerId } : {}),
      };
      const groups = await delegate.groupBy({
        by: ['status'],
        where,
        _count: { _all: true },
        ...(cfg.amountField ? { _sum: { [cfg.amountField]: true } } : {}),
      });
      const rows = groups
        .map((entry) => ({
          status: String(entry.status ?? '-'),
          count: (entry._count as { _all: number })._all,
          ...(cfg.amountField ? { amount: num((entry._sum as Row)[cfg.amountField] as Numeric) } : {}),
        }))
        .sort((a, b) => b.count - a.count);
      const summary: ReportSummaryItem[] = [
        { label: 'Total Dokumen', value: rows.reduce((sum, row) => sum + row.count, 0), type: 'number' },
      ];
      if (cfg.amountField) {
        summary.push({
          label: 'Total Nilai',
          value: rows.reduce((sum, row) => sum + (row.amount ?? 0), 0),
          type: 'money',
        });
      }
      const { labels, values } = topChart(rows.map((row) => [row.status, row.count]));
      return {
        rows,
        total: rows.length,
        summary,
        charts: [{ kind: 'donut', title: 'Dokumen per Status', labels, values }],
      };
    },
  };
}
