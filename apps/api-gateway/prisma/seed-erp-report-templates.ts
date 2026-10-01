/**
 * Seed default report templates for Finance reports:
 * - RPT.FIN.GL     → Buku Besar (General Ledger)
 * - RPT.FIN.TB     → Neraca Saldo (Trial Balance)
 * - RPT.FIN.BS     → Neraca (Balance Sheet)
 *
 * Run: npx ts-node prisma/seed-erp-report-templates.ts
 * Idempotent — uses upsert on `code`.
 */

import { Prisma, PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

// ── shared line style helpers ─────────────────────────────────────────────────
const hline = (id: string, y: number, w: number) => ({
  id,
  type: 'line',
  name: id,
  x: 0,
  y,
  width: w,
  height: 1,
  style: { color: '#333333', width: 1, style: 'solid' },
});

const hrule = (id: string, y: number, w: number) => ({
  id,
  type: 'line',
  name: id,
  x: 0,
  y,
  width: w,
  height: 1,
  style: { color: '#cccccc', width: 1, style: 'solid' },
});

const txt = (
  id: string,
  name: string,
  x: number,
  y: number,
  w: number,
  h: number,
  expression: string,
  style: Record<string, unknown> = {},
) => ({ id, type: 'text', name, x, y, width: w, height: h, expression, style });

// ── Buku Besar ────────────────────────────────────────────────────────────────
// Landscape A4 — content width 257mm
// Cols: Tanggal(25) | No.Bukti(35) | Keterangan(100) | Debit(32) | Kredit(32) | Saldo(33)

const W_GL = 257;

const templateBukuBesar: Prisma.InputJsonValue = {
  name: 'Buku Besar',
  module: 'fin',
  pageSize: 'A4',
  orientation: 'landscape',
  margins: { top: 20, right: 20, bottom: 20, left: 20 },
  params: [
    { name: 'date_from', type: 'date', label: 'Tanggal Dari', required: true },
    { name: 'date_to', type: 'date', label: 'Tanggal Sampai', required: true },
  ],
  dataSources: [
    {
      id: 'ds_gl',
      alias: 'ledger',
      name: 'Data Buku Besar',
      sql: [
        'SELECT',
        '  le.id,',
        '  le.entry_date::text AS entry_date,',
        '  le.doc_number,',
        '  COALESCE(le.description, \'\') AS description,',
        '  a.code AS account_code,',
        '  a.name AS account_name,',
        '  le.debit::float AS debit,',
        '  le.credit::float AS credit,',
        '  SUM(le.debit - le.credit) OVER (',
        '    PARTITION BY le.account_id',
        '    ORDER BY le.entry_date, le.id',
        '  )::float AS running_balance',
        'FROM fin_ledger_entries le',
        'JOIN md_accounts a ON a.id = le.account_id',
        'WHERE le.deleted_at IS NULL',
        '  AND le.entry_date >= :date_from::date',
        '  AND le.entry_date <= :date_to::date',
        'ORDER BY a.code, le.entry_date, le.id',
      ].join('\n'),
      params: [
        { name: 'date_from', type: 'date', label: 'Tanggal Dari', required: true },
        { name: 'date_to', type: 'date', label: 'Tanggal Sampai', required: true },
      ],
    },
  ],
  bands: [
    {
      id: 'ph',
      type: 'pageHeader',
      height: 22,
      printOnAllPages: true,
      components: [
        txt('ph_title', 'title', 0, 0, W_GL, 10, 'BUKU BESAR', { fontSize: 14, bold: true, align: 'center' }),
        txt('ph_period', 'period', 0, 11, W_GL, 7, 'Periode: {date_from} s/d {date_to}', { fontSize: 9, align: 'center' }),
        hline('ph_hline', 20, W_GL),
      ],
    },
    // groupHeader — shows account name + column headers
    {
      id: 'gh_acct',
      type: 'groupHeader',
      level: 1,
      height: 14,
      groupBy: 'account_code',
      components: [
        txt('gh_acct_lbl', 'acctLabel', 0, 0, W_GL, 7,
          '{account_code}  {account_name}',
          { fontSize: 9, bold: true, background: '#e8e8e8' }),
        txt('gh_col_date',  'colDate',  0,   7, 25, 7, 'Tanggal',     { fontSize: 8, bold: true, background: '#f5f5f5', border: { sides: ['bottom'], style: 'solid', width: 1, color: '#aaaaaa' } }),
        txt('gh_col_doc',   'colDoc',   25,  7, 35, 7, 'No. Bukti',   { fontSize: 8, bold: true, background: '#f5f5f5', border: { sides: ['bottom'], style: 'solid', width: 1, color: '#aaaaaa' } }),
        txt('gh_col_desc',  'colDesc',  60,  7, 100, 7, 'Keterangan', { fontSize: 8, bold: true, background: '#f5f5f5', border: { sides: ['bottom'], style: 'solid', width: 1, color: '#aaaaaa' } }),
        txt('gh_col_deb',   'colDebit', 160, 7, 32, 7, 'Debit',       { fontSize: 8, bold: true, align: 'right', background: '#f5f5f5', border: { sides: ['bottom'], style: 'solid', width: 1, color: '#aaaaaa' } }),
        txt('gh_col_cred',  'colCredit',192, 7, 32, 7, 'Kredit',      { fontSize: 8, bold: true, align: 'right', background: '#f5f5f5', border: { sides: ['bottom'], style: 'solid', width: 1, color: '#aaaaaa' } }),
        txt('gh_col_bal',   'colBal',   224, 7, 33, 7, 'Saldo',       { fontSize: 8, bold: true, align: 'right', background: '#f5f5f5', border: { sides: ['bottom'], style: 'solid', width: 1, color: '#aaaaaa' } }),
      ],
    },
    // detail row
    {
      id: 'data',
      type: 'data',
      height: 6,
      canGrow: true,
      components: [
        txt('d_date',  'date',    0,   0, 25,  6, '{entry_date}',     { fontSize: 8 }),
        txt('d_doc',   'doc',     25,  0, 35,  6, '{doc_number}',     { fontSize: 8 }),
        txt('d_desc',  'desc',    60,  0, 100, 6, '{description}',    { fontSize: 8 }, ),
        txt('d_deb',   'debit',   160, 0, 32,  6, '{debit}',          { fontSize: 8, align: 'right' }),
        txt('d_cred',  'credit',  192, 0, 32,  6, '{credit}',         { fontSize: 8, align: 'right' }),
        txt('d_bal',   'balance', 224, 0, 33,  6, '{running_balance}',{ fontSize: 8, align: 'right' }),
      ],
    },
    // groupFooter — subtotal per account
    {
      id: 'gf_acct',
      type: 'groupFooter',
      level: 1,
      height: 8,
      components: [
        hrule('gf_line', 0, W_GL),
        txt('gf_lbl',  'totlbl',   60,  2, 100, 6, 'Total {account_code}', { fontSize: 8, bold: true }),
        txt('gf_deb',  'totdebit', 160, 2, 32,  6, '{{SUM(debit)}}',       { fontSize: 8, bold: true, align: 'right' }),
        txt('gf_cred', 'totcredit',192, 2, 32,  6, '{{SUM(credit)}}',      { fontSize: 8, bold: true, align: 'right' }),
      ],
    },
    // pageFooter
    {
      id: 'pf',
      type: 'pageFooter',
      height: 8,
      components: [
        hrule('pf_line', 0, W_GL),
        txt('pf_pg', 'pageNum', 0, 2, W_GL, 6,
          'Halaman {#pageNo} dari {#totalPages}',
          { fontSize: 7, align: 'right', color: '#666666' }),
      ],
    },
  ],
} as Prisma.InputJsonValue;

// ── Neraca Saldo (Trial Balance) ──────────────────────────────────────────────
// Portrait A4 — content width 170mm
// Cols: Kode(25) | Nama Akun(85) | Debit(30) | Kredit(30)

const W_TB = 170;

const templateNshldSaldo: Prisma.InputJsonValue = {
  name: 'Neraca Saldo',
  module: 'fin',
  pageSize: 'A4',
  orientation: 'portrait',
  margins: { top: 20, right: 20, bottom: 20, left: 20 },
  params: [
    { name: 'date_from', type: 'date', label: 'Tanggal Dari', required: true },
    { name: 'date_to', type: 'date', label: 'Tanggal Sampai', required: true },
  ],
  dataSources: [
    {
      id: 'ds_tb',
      alias: 'tb',
      name: 'Neraca Saldo',
      sql: [
        'SELECT',
        '  a.code,',
        '  a.name,',
        '  a.type AS account_type,',
        '  COALESCE(SUM(le.debit), 0)::float AS total_debit,',
        '  COALESCE(SUM(le.credit), 0)::float AS total_credit',
        'FROM md_accounts a',
        'LEFT JOIN fin_ledger_entries le',
        '  ON le.account_id = a.id',
        '  AND le.deleted_at IS NULL',
        '  AND le.entry_date >= :date_from::date',
        '  AND le.entry_date <= :date_to::date',
        'WHERE a.deleted_at IS NULL AND a.is_active = true',
        'GROUP BY a.id, a.code, a.name, a.type',
        'HAVING COALESCE(SUM(le.debit), 0) != 0 OR COALESCE(SUM(le.credit), 0) != 0',
        'ORDER BY a.code',
      ].join('\n'),
      params: [
        { name: 'date_from', type: 'date', label: 'Tanggal Dari', required: true },
        { name: 'date_to', type: 'date', label: 'Tanggal Sampai', required: true },
      ],
    },
  ],
  bands: [
    {
      id: 'ph',
      type: 'pageHeader',
      height: 32,
      printOnAllPages: true,
      components: [
        txt('ph_title',  'title',    0,  0,  W_TB, 10, 'NERACA SALDO',             { fontSize: 14, bold: true, align: 'center' }),
        txt('ph_period', 'period',   0, 11,  W_TB,  7, 'Periode: {date_from} s/d {date_to}', { fontSize: 9, align: 'center' }),
        hline('ph_hline', 20, W_TB),
        // column headers
        txt('ph_code',   'colCode',   0, 22, 25,  7, 'Kode',      { fontSize: 9, bold: true, background: '#f0f0f0' }),
        txt('ph_name',   'colName',  25, 22, 85,  7, 'Nama Akun', { fontSize: 9, bold: true, background: '#f0f0f0' }),
        txt('ph_debit',  'colDebit', 110, 22, 30, 7, 'Debit',     { fontSize: 9, bold: true, align: 'right', background: '#f0f0f0' }),
        txt('ph_credit', 'colCredit',140, 22, 30, 7, 'Kredit',    { fontSize: 9, bold: true, align: 'right', background: '#f0f0f0' }),
      ],
    },
    {
      id: 'data',
      type: 'data',
      height: 6,
      components: [
        txt('d_code',   'code',   0,   0, 25, 6, '{code}',        { fontSize: 8 }),
        txt('d_name',   'name',   25,  0, 85, 6, '{name}',        { fontSize: 8 }),
        txt('d_debit',  'debit',  110, 0, 30, 6, '{total_debit}', { fontSize: 8, align: 'right' }),
        txt('d_credit', 'credit', 140, 0, 30, 6, '{total_credit}',{ fontSize: 8, align: 'right' }),
      ],
    },
    // summary row
    {
      id: 'gf_total',
      type: 'groupFooter',
      level: 1,
      height: 10,
      components: [
        hline('gf_line', 0, W_TB),
        txt('gf_lbl',    'totlbl',   0,   2, 110, 7, 'TOTAL',             { fontSize: 9, bold: true }),
        txt('gf_debit',  'totdebit', 110, 2, 30,  7, '{{SUM(total_debit)}}',  { fontSize: 9, bold: true, align: 'right' }),
        txt('gf_credit', 'totcredit',140, 2, 30,  7, '{{SUM(total_credit)}}', { fontSize: 9, bold: true, align: 'right' }),
      ],
    },
    {
      id: 'pf',
      type: 'pageFooter',
      height: 8,
      components: [
        hrule('pf_line', 0, W_TB),
        txt('pf_pg', 'pageNum', 0, 2, W_TB, 6,
          'Halaman {#pageNo} dari {#totalPages}',
          { fontSize: 7, align: 'right', color: '#666666' }),
      ],
    },
  ],
} as Prisma.InputJsonValue;

// ── Neraca (Balance Sheet) ────────────────────────────────────────────────────
// Portrait A4 — content width 170mm
// Grouped by account_type (ASSET / LIABILITY / EQUITY)
// Cols: Kode(25) | Nama Akun(105) | Saldo(40)

const W_BS = 170;

const templateNeraca: Prisma.InputJsonValue = {
  name: 'Neraca',
  module: 'fin',
  pageSize: 'A4',
  orientation: 'portrait',
  margins: { top: 20, right: 20, bottom: 20, left: 20 },
  params: [
    { name: 'as_of_date', type: 'date', label: 'Per Tanggal', required: true },
  ],
  dataSources: [
    {
      id: 'ds_bs',
      alias: 'bs',
      name: 'Neraca',
      sql: [
        'SELECT',
        '  a.type AS account_type,',
        '  CASE a.type',
        "    WHEN 'ASSET'     THEN '1. Aset'",
        "    WHEN 'LIABILITY' THEN '2. Kewajiban'",
        "    WHEN 'EQUITY'    THEN '3. Ekuitas'",
        '    ELSE a.type::text',
        '  END AS type_label,',
        '  a.code,',
        '  a.name,',
        '  CASE',
        "    WHEN a.normal_balance = 'DEBIT'",
        '      THEN COALESCE(SUM(le.debit) - SUM(le.credit), 0)',
        '    ELSE COALESCE(SUM(le.credit) - SUM(le.debit), 0)',
        '  END::float AS balance',
        'FROM md_accounts a',
        'LEFT JOIN fin_ledger_entries le',
        '  ON le.account_id = a.id',
        '  AND le.deleted_at IS NULL',
        '  AND le.entry_date <= :as_of_date::date',
        'WHERE a.deleted_at IS NULL',
        '  AND a.is_active = true',
        "  AND a.type IN ('ASSET', 'LIABILITY', 'EQUITY')",
        'GROUP BY a.id, a.type, a.code, a.name, a.normal_balance',
        'HAVING',
        '  CASE',
        "    WHEN a.normal_balance = 'DEBIT'",
        '      THEN COALESCE(SUM(le.debit) - SUM(le.credit), 0)',
        '    ELSE COALESCE(SUM(le.credit) - SUM(le.debit), 0)',
        '  END != 0',
        'ORDER BY a.type, a.code',
      ].join('\n'),
      params: [
        { name: 'as_of_date', type: 'date', label: 'Per Tanggal', required: true },
      ],
    },
  ],
  bands: [
    {
      id: 'ph',
      type: 'pageHeader',
      height: 22,
      printOnAllPages: true,
      components: [
        txt('ph_title', 'title', 0, 0, W_BS, 10, 'NERACA',              { fontSize: 14, bold: true, align: 'center' }),
        txt('ph_asof',  'asof',  0, 11, W_BS, 7, 'Per Tanggal: {as_of_date}', { fontSize: 9, align: 'center' }),
        hline('ph_hline', 20, W_BS),
      ],
    },
    // group by account type
    {
      id: 'gh_type',
      type: 'groupHeader',
      level: 1,
      height: 14,
      groupBy: 'account_type',
      components: [
        txt('gh_type_lbl', 'typeLabel', 0, 0, W_BS, 7, '{type_label}',
          { fontSize: 10, bold: true, background: '#e8e8e8' }),
        txt('gh_col_code', 'colCode',   0,  7, 25,  7, 'Kode',      { fontSize: 8, bold: true, background: '#f5f5f5' }),
        txt('gh_col_name', 'colName',  25,  7, 105, 7, 'Nama Akun', { fontSize: 8, bold: true, background: '#f5f5f5' }),
        txt('gh_col_bal',  'colBal',  130,  7, 40,  7, 'Saldo',     { fontSize: 8, bold: true, align: 'right', background: '#f5f5f5' }),
      ],
    },
    {
      id: 'data',
      type: 'data',
      height: 6,
      components: [
        txt('d_code',    'code',    0,   0, 25,  6, '{code}',    { fontSize: 8 }),
        txt('d_name',    'name',    25,  0, 105, 6, '{name}',    { fontSize: 8 }),
        txt('d_balance', 'balance', 130, 0, 40,  6, '{balance}', { fontSize: 8, align: 'right' }),
      ],
    },
    // subtotal per type
    {
      id: 'gf_type',
      type: 'groupFooter',
      level: 1,
      height: 10,
      components: [
        hrule('gf_line', 0, W_BS),
        txt('gf_lbl', 'totlbl', 0,   2, 130, 7, 'Total {type_label}',  { fontSize: 8, bold: true }),
        txt('gf_tot', 'totbal', 130, 2, 40,  7, '{{SUM(balance)}}',    { fontSize: 8, bold: true, align: 'right' }),
      ],
    },
    {
      id: 'pf',
      type: 'pageFooter',
      height: 8,
      components: [
        hrule('pf_line', 0, W_BS),
        txt('pf_pg', 'pageNum', 0, 2, W_BS, 6,
          'Halaman {#pageNo} dari {#totalPages}',
          { fontSize: 7, align: 'right', color: '#666666' }),
      ],
    },
  ],
} as Prisma.InputJsonValue;

// ── seed ──────────────────────────────────────────────────────────────────────

const TEMPLATES = [
  {
    code: 'RPT.FIN.GL',
    name: 'Buku Besar',
    module: 'fin',
    description: 'Buku Besar per akun dengan saldo berjalan',
    templateJson: templateBukuBesar,
  },
  {
    code: 'RPT.FIN.TB',
    name: 'Neraca Saldo',
    module: 'fin',
    description: 'Trial Balance — total debit & kredit per akun dalam periode',
    templateJson: templateNshldSaldo,
  },
  {
    code: 'RPT.FIN.BS',
    name: 'Neraca',
    module: 'fin',
    description: 'Balance Sheet — Aset, Kewajiban, Ekuitas per tanggal tertentu',
    templateJson: templateNeraca,
  },
];

async function main() {
  for (const tpl of TEMPLATES) {
    const result = await prisma.erpRptTemplate.upsert({
      where: { code: tpl.code },
      update: {
        name: tpl.name,
        description: tpl.description,
        templateJson: tpl.templateJson,
        isActive: true,
      },
      create: {
        code: tpl.code,
        name: tpl.name,
        module: tpl.module,
        description: tpl.description,
        templateJson: tpl.templateJson,
        isActive: true,
      },
    });
    console.log(`✓ ${result.code} — ${result.name}`);
  }
}

main()
  .then(() => { console.log('Done.'); })
  .catch(err => { console.error(err); process.exit(1); })
  .finally(() => prisma.$disconnect());
