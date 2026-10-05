/**
 * G3 — Daftar/register: giro, list kas/bank, list jurnal, dan config
 * honest-empty untuk dokumen yang belum ada padanannya di ERP
 * (anggaran, pembatalan giro, uang muka pembelian).
 */
import {
  FinDatasetConfig,
  docNoFilter,
  emptyConfig,
  partnerFilter,
  periodFilters,
  statusLabel,
} from './fin-doc-mrt-configs';

/* ------------------------------ giro ---------------------------------- */

interface GiroListOpts {
  giroType?: 'INCOMING' | 'OUTGOING';
  entryType?: 'INCOMING' | 'OUTGOING';
  /** bare: tanpa alias ` s` (untuk dibungkus CTE lain). */
  bare?: boolean;
}

const giroFrom = (o: GiroListOpts): string => `(
SELECT g.id AS gid, g.giro_number, g.amount, g.amount_fx, g.exchange_rate,
  g.due_date, g.cleared_date, g.status::text AS gstatus, g.bank_name,
  g.bank_account_no, g.line_no, g.notes, g.type::text AS gtype,
  e.id AS eid, COALESCE(e.doc_number, t.doc_number) AS doc_number,
  COALESCE(e.entry_date, t.transaction_date) AS entry_date, g.source,
  COALESCE(e.status::text, g.status::text) AS estatus,
  p.id AS id, p.code AS code, p.name AS name,
  ga.code AS gacode, ga.name AS ganame, ba.code AS bacode, ba.name AS baname,
  cur.code AS curcode
FROM fin_giros g
LEFT JOIN fin_giro_entries e ON e.id = g.giro_entry_id
LEFT JOIN fin_cash_bank_transactions t ON t.id = g.source_transaction_id
LEFT JOIN md_partners p ON p.id = g.partner_id
LEFT JOIN md_accounts ga ON ga.id = g.giro_account_id
LEFT JOIN md_accounts ba ON ba.id = g.bank_account_id
LEFT JOIN md_currencies cur ON cur.id = g.currency_id
WHERE g.deleted_at IS NULL AND (e.id IS NULL OR e.deleted_at IS NULL) AND (t.id IS NULL OR t.deleted_at IS NULL)${o.giroType ? ` AND g.type = '${o.giroType}'` : ''}${o.entryType ? ` AND e.kind = 'REGISTER' AND e.type = '${o.entryType}'` : ''})${o.bare ? '' : ' s'}`;

const GIRO_SELECT: Record<string, string> = {
  glid: 's.gid',
  glnogiro: 's.giro_number',
  glsumber: 's.source',
  glidtransaksi: 's.eid',
  glnotransaksi: 's.doc_number',
  glkontak: 's.id',
  glkontakkode: 's.code',
  glkontaknama: 's.name',
  glrekbank: 's.bacode',
  glrekbanknama: 's.baname',
  glrekgiro: 's.gacode',
  glrekgironama: 's.ganame',
  gljenis: `(CASE s.gtype WHEN 'INCOMING' THEN 'Masuk' WHEN 'OUTGOING' THEN 'Keluar' ELSE s.gtype END)::text`,
  glbank: 's.bank_name',
  glnoacbank: 's.bank_account_no',
  glmatauang: 's.curcode',
  glkurs: 's.exchange_rate',
  gljumlah: 's.amount',
  gljumlahvalas: 's.amount_fx',
  gltgljthtempo: 's.due_date',
  gltglcair: 's.cleared_date',
  glstatus: 's.gstatus',
  glstatusnama: statusLabel('s.gstatus'),
  glurutan: 's.line_no',
  gltgl: 's.entry_date',
  glcatatan: 's.notes',
  ttgl: 's.entry_date',
  status: statusLabel('s.estatus'),
  knama: 's.name',
  cnomor: 's.gacode',
  cnama: 's.ganame',
  bank: 's.bank_name',
  nogiro: 's.giro_number',
  jumlah: 's.amount',
  noacbank: 's.bank_account_no',
  rgmatauang: 's.curcode',
  sgmatauang: 's.curcode',
  tgljatuhtempo: 's.due_date',
};

const giroListConfig = (o: GiroListOpts): FinDatasetConfig => ({
  from: giroFrom(o),
  select: GIRO_SELECT,
  paramFilters: {
    ...periodFilters('s.entry_date'),
    ...partnerFilter('s.id', 's.code', 's.name'),
  },
  orderBy: 's.entry_date, s.giro_number',
});

/** Analisa umur giro: pita umur sama seperti AR/AP (label template). */
const GIRO_BUCKETS: Array<[string, string]> = [
  ['1', 'd < -60'],
  ['2', 'd >= -60 AND d <= -46'],
  ['3', 'd >= -45 AND d <= -31'],
  ['4', 'd >= -30 AND d <= -16'],
  ['5', 'd >= -15 AND d <= -1'],
  ['6', 'd >= 0 AND d <= 30'],
  ['7', 'd >= 31 AND d <= 60'],
  ['8', 'd >= 61'],
];

const giroAgingConfig = (giroType: 'INCOMING' | 'OUTGOING'): FinDatasetConfig => ({
  from: `(
WITH prm AS (SELECT COALESCE(?::date, CURRENT_DATE) AS pend),
base AS ${giroFrom({ giroType, bare: true })}
SELECT b.*, prm.pend - b.due_date AS age
FROM base b, prm) s`,
  select: {
    ...GIRO_SELECT,
    glsisa: 's.amount',
    ...Object.fromEntries(
      GIRO_BUCKETS.map(([n, cond]) => [
        `glumur${n}`,
        `(CASE WHEN ${cond.split('d ').join('s.age ')} THEN s.amount ELSE 0 END)`,
      ]),
    ),
    glumur_1: '0',
    glumur9: '0',
    glumur10: '0',
    glumur_10: '0',
  },
  bindParams: [{ name: 'period_end', kind: 'date' }],
  paramFilters: { ...partnerFilter('s.id', 's.code', 's.name') },
  orderBy: 's.code, s.due_date, s.giro_number',
});

/* --------------------------- list kas/bank ----------------------------- */

const cbListConfig = (
  kind: 'CASH' | 'BANK',
  direction: 'RECEIPT' | 'DISBURSEMENT',
  prefix: string,
): FinDatasetConfig => ({
  from:
    `(SELECT * FROM fin_cash_bank_transactions WHERE kind = '${kind}' ` +
    `AND direction = '${direction}' AND posting_status = 'POSTED') t ` +
    'JOIN fin_cash_bank_lines l ON l.cash_bank_transaction_id = t.id ' +
    'JOIN md_accounts a ON a.id = l.account_id ' +
    'LEFT JOIN md_partners p ON p.id = t.partner_id ' +
    'LEFT JOIN md_currencies cur ON cur.id = t.currency_id',
  deletedAlias: 't',
  select: {
    [`${prefix}notransaksi`]: 't.doc_number',
    [`${prefix}tgl`]: 't.transaction_date',
    [`${prefix}uraian`]: 't.description',
    catatan: 'l.notes',
    knama: 'p.name',
    norek: 'a.code',
    namarekening: 'a.name',
    kredit: 'l.amount',
    jmlvalas: 'l.amount_fx',
    kurs: 't.exchange_rate',
    matauang: 'cur.code',
    [`status${prefix}`]: statusLabel('t.status::text'),
  },
  paramFilters: {
    ...docNoFilter('t.doc_number'),
    ...periodFilters('t.transaction_date'),
    ...partnerFilter('t.partner_id', 'p.code', 'p.name'),
    division: { sql: 'l.division_id = ?', kind: 'number' },
    cost_center: { sql: 'l.cost_center_id = ?', kind: 'number' },
    project: { sql: 'l.project_id = ?', kind: 'number' },
  },
  orderBy: 't.transaction_date, t.id, l.line_no',
});

/* ----------------------------- list jurnal ----------------------------- */

const journalListConfig = (type: string, prefix: string): FinDatasetConfig => ({
  from:
    `(SELECT * FROM fin_journal_entries WHERE journal_type = '${type}') e ` +
    'JOIN fin_journal_lines l ON l.journal_entry_id = e.id ' +
    'JOIN md_accounts a ON a.id = l.account_id ' +
    'LEFT JOIN md_partners p ON p.id = e.partner_id ' +
    'LEFT JOIN md_currencies cur ON cur.id = e.currency_id',
  deletedAlias: 'e',
  select: {
    [`${prefix}notransaksi`]: 'e.doc_number',
    [`${prefix}tgl`]: 'e.entry_date',
    [`${prefix}uraian`]: 'e.description',
    [`${prefix}matauang`]: 'cur.code',
    norek: 'a.code',
    cnama: 'a.name',
    knama: 'p.name',
    kontak: 'e.partner_id',
    debit: 'l.debit',
    kredit: 'l.credit',
    debitvalas: 'l.debit_fx',
    kreditvalas: 'l.credit_fx',
    kurs: 'e.exchange_rate',
    [`status${prefix}`]: statusLabel('e.status::text'),
  },
  paramFilters: {
    ...docNoFilter('e.doc_number'),
    ...periodFilters('e.entry_date'),
    ...partnerFilter('e.partner_id', 'p.code', 'p.name'),
  },
  orderBy: 'e.entry_date, e.id, l.line_no',
});

/* ---------------------------- honest-empty ----------------------------- */

const ANGGARAN_LIST_NOTE =
  'Dokumen anggaran legacy (m2_bd) belum ada padanannya di ERP: yang tersedia ' +
  'hanya agregat realisasi per periode di fin_budget_realizations (lihat laporan ' +
  'anggaran & realisasi), bukan dokumen anggaran.';

const GIRO_CANCEL_LIST_NOTE =
  'Dokumen pembatalan giro legacy (m2_rgc/m2_sgc) belum ada padanannya di ERP: ' +
  'pembatalan adalah status per giro di fin_giros, bukan dokumen tersendiri.';

const UM_PEMBELIAN_NOTE =
  'Dokumen uang muka pembelian legacy (m4_ap) belum ada padanannya di ERP: ' +
  'tidak ada tabel advance pembelian; uang muka penjualan memakai sls_customer_advances.';

export const FIN_G3_LIST_CONFIGS: Record<string, Record<string, FinDatasetConfig>> = {
  'fin.daftargiromasuk': { DS1: giroListConfig({ giroType: 'INCOMING' }) },
  'fin.daftargirokeluar': { DS1: giroListConfig({ giroType: 'OUTGOING' }) },
  'fin.datagiromasuk': { DS1: giroListConfig({ giroType: 'INCOMING' }) },
  'fin.datagirokeluar': { DS1: giroListConfig({ giroType: 'OUTGOING' }) },
  'fin.giromasukpertanggal': { DS1: giroListConfig({ giroType: 'INCOMING' }) },
  'fin.girokeluarpertanggal': { DS1: giroListConfig({ giroType: 'OUTGOING' }) },
  'fin.receivegirolist': { DS1: giroListConfig({ entryType: 'INCOMING' }) },
  'fin.receivegirolist2': { DS1: giroListConfig({ entryType: 'INCOMING' }) },
  'fin.spendgirolist': { DS1: giroListConfig({ entryType: 'OUTGOING' }) },
  'fin.spendgirolist2': { DS1: giroListConfig({ entryType: 'OUTGOING' }) },
  'fin.analisaumurgiromasuk': { DS1: giroAgingConfig('INCOMING') },
  'fin.analisaumurgiromasukdetail': { DS1: giroAgingConfig('INCOMING') },
  'fin.analisaumurgirokeluar': { DS1: giroAgingConfig('OUTGOING') },
  'fin.analisaumurgirokeluardetail': { DS1: giroAgingConfig('OUTGOING') },
  'fin.listcashreceipt': { DS1: cbListConfig('CASH', 'RECEIPT', 'cr') },
  'fin.listcashreceipt2': { DS1: cbListConfig('CASH', 'RECEIPT', 'cr') },
  'fin.listcashdisbursements': { DS1: cbListConfig('CASH', 'DISBURSEMENT', 'cd') },
  'fin.listcashdisbursements2': { DS1: cbListConfig('CASH', 'DISBURSEMENT', 'cd') },
  'fin.listreceivemoney': { DS1: cbListConfig('BANK', 'RECEIPT', 'rm') },
  'fin.listreceivemoney2': { DS1: cbListConfig('BANK', 'RECEIPT', 'rm') },
  'fin.listspendmoney': { DS1: cbListConfig('BANK', 'DISBURSEMENT', 'sm') },
  'fin.listspendmoney2': { DS1: cbListConfig('BANK', 'DISBURSEMENT', 'sm') },
  'fin.listgeneraljournal': { DS1: journalListConfig('GENERAL', 'gj') },
  'fin.listgeneraljournal2': { DS1: journalListConfig('GENERAL', 'gj') },
  'fin.listadjustmentjournal': { DS1: journalListConfig('ADJUSTMENT', 'aj') },
  'fin.daftarjurnalmemorial': { DS1: journalListConfig('MEMORIAL', 'jm') },
  'fin.daftarsaldoawalcoa': { DS1: journalListConfig('OPENING_BALANCE', 'cb') },
  'fin.daftarsaldoawalcoa2': { DS1: journalListConfig('OPENING_BALANCE', 'cb') },
  'fin.daftaranggaran': { DS1: emptyConfig(ANGGARAN_LIST_NOTE) },
  'fin.daftaranggaran2': { DS1: emptyConfig(ANGGARAN_LIST_NOTE) },
  'fin.receivegirocancellist': { DS1: emptyConfig(GIRO_CANCEL_LIST_NOTE) },
  'fin.receivegirocancellist2': { DS1: emptyConfig(GIRO_CANCEL_LIST_NOTE) },
  'fin.spendgirocancellist': { DS1: emptyConfig(GIRO_CANCEL_LIST_NOTE) },
  'fin.spendgirocancellist2': { DS1: emptyConfig(GIRO_CANCEL_LIST_NOTE) },
  'fin.daftarumpembelian': { DS1: emptyConfig(UM_PEMBELIAN_NOTE) },
  'fin.daftarumpembeliandetail': { DS1: emptyConfig(UM_PEMBELIAN_NOTE) },
  'fin.kartuumpembelian': { DS1: emptyConfig(UM_PEMBELIAN_NOTE) },
  'fin.rekapumpembelian': { DS1: emptyConfig(UM_PEMBELIAN_NOTE) },
  'fin.voucherumpembelian': { DS1: emptyConfig(UM_PEMBELIAN_NOTE) },
};
