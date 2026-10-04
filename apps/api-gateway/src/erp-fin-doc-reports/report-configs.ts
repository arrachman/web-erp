/**
 * Finance transaction-report catalogue. Every `where` predicate maps to a
 * documented enum discriminator in erp-fin.prisma/enums.prisma.
 */

import { PrismaService } from '../prisma/prisma.service';
import { ReportColumn, ReportDef } from './report-types';
import { makeDocumentListReport, makeStatusRecapReport } from './report-factory';

const CASH_BANK_COLUMNS: ReportColumn[] = [
  { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
  { key: 'transactionDate', header: 'Tanggal', type: 'date' },
  { key: 'partner', header: 'Partner', type: 'text' },
  { key: 'description', header: 'Keterangan', type: 'text' },
  { key: 'amount', header: 'Nilai', type: 'money' },
  { key: 'status', header: 'Status', type: 'status' },
];

const JOURNAL_COLUMNS: ReportColumn[] = [
  { key: 'docNumber', header: 'No. Jurnal', type: 'text' },
  { key: 'entryDate', header: 'Tanggal', type: 'date' },
  { key: 'partner', header: 'Partner', type: 'text' },
  { key: 'description', header: 'Keterangan', type: 'text' },
  { key: 'postingStatus', header: 'Status Posting', type: 'status' },
  { key: 'status', header: 'Status', type: 'status' },
];

const GIRO_COLUMNS: ReportColumn[] = [
  { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
  { key: 'entryDate', header: 'Tanggal', type: 'date' },
  { key: 'partner', header: 'Partner', type: 'text' },
  { key: 'description', header: 'Keterangan', type: 'text' },
  { key: 'postingStatus', header: 'Status Posting', type: 'status' },
  { key: 'status', header: 'Status', type: 'status' },
];

const REVALUATION_COLUMNS: ReportColumn[] = [
  { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
  { key: 'revaluationDate', header: 'Tanggal Revaluasi', type: 'date' },
  { key: 'totalGainLoss', header: 'Selisih Kurs', type: 'money' },
  { key: 'status', header: 'Status', type: 'status' },
];

const TAX_COLUMNS: ReportColumn[] = [
  { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
  { key: 'transactionDate', header: 'Tanggal', type: 'date' },
  { key: 'partnerName', header: 'Partner', type: 'text' },
  { key: 'taxEntryType', header: 'Jenis Pajak', type: 'text' },
  { key: 'dpp', header: 'DPP', type: 'money' },
  { key: 'taxAmount', header: 'Pajak', type: 'money' },
  { key: 'status', header: 'Status', type: 'status' },
];

interface PairConfig {
  key: string;
  title: string;
  model: string;
  dateField: string;
  columns: ReportColumn[];
  amountField?: string;
  partnerField?: string;
  partnerColumn?: string;
  where: Record<string, unknown>;
}

function pair(prisma: PrismaService, config: PairConfig): ReportDef[] {
  return [
    makeDocumentListReport(prisma, {
      ...config,
      group: 'transaction',
    }),
    makeStatusRecapReport(prisma, {
      key: `${config.key}-by-status`,
      title: `${config.title} per Status`,
      group: 'transaction',
      model: config.model,
      dateField: config.dateField,
      amountField: config.amountField,
      partnerField: config.partnerField,
      where: config.where,
    }),
  ];
}

export function buildFinDocumentReports(prisma: PrismaService): ReportDef[] {
  const configs: PairConfig[] = [
    {
      key: 'cash-receipts',
      title: 'Daftar Penerimaan Kas',
      model: 'erpFinCashBankTransaction',
      dateField: 'transactionDate',
      columns: CASH_BANK_COLUMNS,
      amountField: 'amount',
      partnerField: 'partnerId',
      partnerColumn: 'partner',
      where: { kind: 'CASH', direction: 'RECEIPT' },
    },
    {
      key: 'cash-disbursements',
      title: 'Daftar Pengeluaran Kas',
      model: 'erpFinCashBankTransaction',
      dateField: 'transactionDate',
      columns: CASH_BANK_COLUMNS,
      amountField: 'amount',
      partnerField: 'partnerId',
      partnerColumn: 'partner',
      where: { kind: 'CASH', direction: 'DISBURSEMENT' },
    },
    {
      key: 'bank-receipts',
      title: 'Daftar Penerimaan Bank',
      model: 'erpFinCashBankTransaction',
      dateField: 'transactionDate',
      columns: CASH_BANK_COLUMNS,
      amountField: 'amount',
      partnerField: 'partnerId',
      partnerColumn: 'partner',
      where: { kind: 'BANK', direction: 'RECEIPT' },
    },
    {
      key: 'bank-disbursements',
      title: 'Daftar Pengeluaran Bank',
      model: 'erpFinCashBankTransaction',
      dateField: 'transactionDate',
      columns: CASH_BANK_COLUMNS,
      amountField: 'amount',
      partnerField: 'partnerId',
      partnerColumn: 'partner',
      where: { kind: 'BANK', direction: 'DISBURSEMENT' },
    },
    {
      key: 'general-journals',
      title: 'Daftar Jurnal Umum',
      model: 'erpFinJournalEntry',
      dateField: 'entryDate',
      columns: JOURNAL_COLUMNS,
      partnerField: 'partnerId',
      partnerColumn: 'partner',
      where: { journalType: 'GENERAL' },
    },
    {
      key: 'adjustment-journals',
      title: 'Daftar Jurnal Penyesuaian',
      model: 'erpFinJournalEntry',
      dateField: 'entryDate',
      columns: JOURNAL_COLUMNS,
      partnerField: 'partnerId',
      partnerColumn: 'partner',
      where: { journalType: 'ADJUSTMENT' },
    },
    {
      key: 'memorial-journals',
      title: 'Daftar Jurnal Memorial',
      model: 'erpFinJournalEntry',
      dateField: 'entryDate',
      columns: JOURNAL_COLUMNS,
      partnerField: 'partnerId',
      partnerColumn: 'partner',
      where: { journalType: 'MEMORIAL' },
    },
    {
      key: 'opening-balances',
      title: 'Daftar Saldo Awal CoA',
      model: 'erpFinJournalEntry',
      dateField: 'entryDate',
      columns: JOURNAL_COLUMNS,
      partnerField: 'partnerId',
      partnerColumn: 'partner',
      where: { journalType: 'OPENING_BALANCE' },
    },
    {
      key: 'receipt-giros',
      title: 'Daftar Giro Masuk',
      model: 'erpFinGiroEntry',
      dateField: 'entryDate',
      columns: GIRO_COLUMNS,
      partnerField: 'partnerId',
      partnerColumn: 'partner',
      where: { kind: 'REGISTER', type: 'INCOMING' },
    },
    {
      key: 'send-giros',
      title: 'Daftar Giro Keluar',
      model: 'erpFinGiroEntry',
      dateField: 'entryDate',
      columns: GIRO_COLUMNS,
      partnerField: 'partnerId',
      partnerColumn: 'partner',
      where: { kind: 'REGISTER', type: 'OUTGOING' },
    },
    {
      key: 'receipt-giro-clearings',
      title: 'Daftar Kliring Giro Masuk',
      model: 'erpFinGiroEntry',
      dateField: 'entryDate',
      columns: GIRO_COLUMNS,
      partnerField: 'partnerId',
      partnerColumn: 'partner',
      where: { kind: 'CLEAR', type: 'INCOMING' },
    },
    {
      key: 'send-giro-clearings',
      title: 'Daftar Kliring Giro Keluar',
      model: 'erpFinGiroEntry',
      dateField: 'entryDate',
      columns: GIRO_COLUMNS,
      partnerField: 'partnerId',
      partnerColumn: 'partner',
      where: { kind: 'CLEAR', type: 'OUTGOING' },
    },
    {
      key: 'revaluations',
      title: 'Daftar Revaluasi Kurs',
      model: 'erpFinFxRevaluationRun',
      dateField: 'revaluationDate',
      columns: REVALUATION_COLUMNS,
      amountField: 'totalGainLoss',
      where: {},
    },
    {
      key: 'tax-subledger',
      title: 'Daftar Pajak Pengadaan',
      model: 'erpFinTaxEntry',
      dateField: 'transactionDate',
      columns: TAX_COLUMNS,
      amountField: 'taxAmount',
      where: { module: 'PUR' },
    },
  ];
  return configs.flatMap((config) => pair(prisma, config));
}
