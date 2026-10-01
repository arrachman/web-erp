/**
 * Shared builders for AR Card, AP Card, AR Aging, AP Aging reports.
 * Extracted to keep erp-fin-reports-ext.service.ts under 400 lines.
 */
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { ReportColumn, ReportDocument, ReportRow } from './report-types';
import { ZERO, metaCabang, num, printedMeta } from './report-helpers';

export async function partnerMap(
  prisma: PrismaService,
  ids: bigint[],
): Promise<Map<string, string>> {
  if (!ids.length) return new Map();
  const uniq = [...new Set(ids.map(String))];
  const partners = await prisma.erpPartner.findMany({
    where: { id: { in: uniq.map(BigInt) } },
    select: { id: true, code: true, name: true },
  });
  return new Map(partners.map((p) => [p.id.toString(), `${p.code} - ${p.name}`]));
}

export async function accountMap(
  prisma: PrismaService,
  ids: bigint[],
): Promise<Map<string, { code: string; name: string; type: string }>> {
  if (!ids.length) return new Map();
  const uniq = [...new Set(ids.map(String))];
  const accounts = await prisma.erpAccount.findMany({
    where: { id: { in: uniq.map(BigInt) } },
    select: { id: true, code: true, name: true, type: true },
  });
  return new Map(
    accounts.map((a) => [a.id.toString(), { code: a.code, name: a.name, type: a.type as string }]),
  );
}

const CARD_COLS: ReportColumn[] = [
  { key: 'No', label: 'No Dokumen', type: 'text', width: 16 },
  { key: 'Tanggal', label: 'Tanggal', type: 'date', width: 12 },
  { key: 'Partner', label: 'Partner', type: 'text', width: 30 },
  { key: 'Keterangan', label: 'Keterangan', type: 'text', width: 30 },
  { key: 'Jumlah', label: 'Jumlah', type: 'number', align: 'right', width: 16 },
  { key: 'Dialokasi', label: 'Dialokasi', type: 'number', align: 'right', width: 16 },
  { key: 'Outstanding', label: 'Outstanding', type: 'number', align: 'right', width: 16 },
];

const AGING_COLS: ReportColumn[] = [
  { key: 'No', label: 'No Dokumen', type: 'text', width: 16 },
  { key: 'Tanggal', label: 'Tanggal', type: 'date', width: 12 },
  { key: 'Partner', label: 'Partner', type: 'text', width: 30 },
  { key: 'Total', label: 'Total', type: 'number', align: 'right', width: 14 },
  { key: 'Outstanding', label: 'Outstanding', type: 'number', align: 'right', width: 14 },
  { key: 'Bucket', label: 'Umur (hari)', type: 'text', width: 16 },
];

const AGING_BUCKETS = ['0-30', '31-60', '61-90', '91-120', '120+'];

function ageBucket(days: number): string {
  if (days <= 30) return '0-30';
  if (days <= 60) return '31-60';
  if (days <= 90) return '61-90';
  if (days <= 120) return '91-120';
  return '120+';
}

interface CardItem {
  docNumber: string;
  transactionDate: Date;
  partnerId: bigint;
  description: string;
  amount: Prisma.Decimal;
  allocatedAmount: Prisma.Decimal;
}

export function buildCardRows(items: CardItem[], pMap: Map<string, string>): ReportRow[] {
  return items.map((r) => ({
    cells: {
      No: r.docNumber,
      Tanggal: r.transactionDate.toISOString().slice(0, 10),
      Partner: pMap.get(r.partnerId.toString()) ?? '',
      Keterangan: r.description,
      Jumlah: num(r.amount),
      Dialokasi: num(r.allocatedAmount),
      Outstanding: num(r.amount.sub(r.allocatedAmount)),
    },
  }));
}

export function buildAgingSections(
  items: CardItem[],
  pMap: Map<string, string>,
  refDate: Date,
): import('./report-types').ReportSection[] {
  const byBucket: Record<string, ReportRow[]> = Object.fromEntries(AGING_BUCKETS.map((b) => [b, []]));
  for (const r of items) {
    const outstanding = r.amount.sub(r.allocatedAmount);
    if (outstanding.lte(ZERO)) continue;
    const days = Math.floor((refDate.getTime() - r.transactionDate.getTime()) / 86400000);
    const bucket = ageBucket(days);
    byBucket[bucket].push({
      cells: {
        No: r.docNumber,
        Tanggal: r.transactionDate.toISOString().slice(0, 10),
        Partner: pMap.get(r.partnerId.toString()) ?? '',
        Total: num(r.amount),
        Outstanding: num(outstanding),
        Bucket: bucket,
      },
    });
  }
  return AGING_BUCKETS.map((b) => ({ heading: `${b} hari`, rows: byBucket[b] }));
}

export async function buildArCard(
  prisma: PrismaService,
  from: string,
  to: string,
  partnerId?: string,
  branchId?: string,
): Promise<ReportDocument> {
  const where: Prisma.ErpFinArReceiptWhereInput = {
    deletedAt: null,
    postingStatus: 'POSTED',
    transactionDate: { gte: new Date(from), lte: new Date(to) },
  };
  if (branchId) where.branchId = BigInt(branchId);
  if (partnerId) where.partnerId = BigInt(partnerId);
  const rows = await prisma.erpFinArReceipt.findMany({ where, orderBy: { transactionDate: 'asc' } });
  const pMap = await partnerMap(prisma, rows.map((r) => r.partnerId));
  return {
    key: 'ar-card',
    title: 'Kartu Piutang (AR Card)',
    subtitle: `Periode ${from} s/d ${to}`,
    meta: [{ label: 'Periode', value: `${from} s/d ${to}` }, ...metaCabang(branchId), printedMeta()],
    columns: CARD_COLS,
    sections: [{ rows: buildCardRows(rows, pMap) }],
  };
}

export async function buildArAging(prisma: PrismaService, asOf?: string): Promise<ReportDocument> {
  const refDate = asOf ? new Date(asOf) : new Date();
  const refStr = refDate.toISOString().slice(0, 10);
  const rows = await prisma.erpFinArReceipt.findMany({ where: { deletedAt: null, postingStatus: 'POSTED' } });
  const pMap = await partnerMap(prisma, rows.map((r) => r.partnerId));
  return {
    key: 'ar-aging',
    title: 'Analisis Umur Piutang (AR Aging)',
    subtitle: `Per tanggal ${refStr}`,
    meta: [{ label: 'Per Tanggal', value: refStr }, printedMeta()],
    columns: AGING_COLS,
    sections: buildAgingSections(rows, pMap, refDate),
  };
}

export async function buildApCard(
  prisma: PrismaService,
  from: string,
  to: string,
  partnerId?: string,
  branchId?: string,
): Promise<ReportDocument> {
  const where: Prisma.ErpFinApPaymentWhereInput = {
    deletedAt: null,
    postingStatus: 'POSTED',
    transactionDate: { gte: new Date(from), lte: new Date(to) },
  };
  if (branchId) where.branchId = BigInt(branchId);
  if (partnerId) where.partnerId = BigInt(partnerId);
  const rows = await prisma.erpFinApPayment.findMany({ where, orderBy: { transactionDate: 'asc' } });
  const pMap = await partnerMap(prisma, rows.map((r) => r.partnerId));
  return {
    key: 'ap-card',
    title: 'Kartu Utang (AP Card)',
    subtitle: `Periode ${from} s/d ${to}`,
    meta: [{ label: 'Periode', value: `${from} s/d ${to}` }, ...metaCabang(branchId), printedMeta()],
    columns: CARD_COLS,
    sections: [{ rows: buildCardRows(rows, pMap) }],
  };
}

export async function buildApAging(prisma: PrismaService, asOf?: string): Promise<ReportDocument> {
  const refDate = asOf ? new Date(asOf) : new Date();
  const refStr = refDate.toISOString().slice(0, 10);
  const rows = await prisma.erpFinApPayment.findMany({ where: { deletedAt: null, postingStatus: 'POSTED' } });
  const pMap = await partnerMap(prisma, rows.map((r) => r.partnerId));
  return {
    key: 'ap-aging',
    title: 'Analisis Umur Utang (AP Aging)',
    subtitle: `Per tanggal ${refStr}`,
    meta: [{ label: 'Per Tanggal', value: refStr }, printedMeta()],
    columns: AGING_COLS,
    sections: buildAgingSections(rows, pMap, refDate),
  };
}
