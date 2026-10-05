/**
 * G3 — Buku kas/bank harian (staging legacy m2r_kas_harian, kolom kbh*).
 *
 * Baris = transaksi kas/bank POSTED × baris lawannya. kbhnorek = akun
 * kas/bank transaksi; debit/kredit dari sisi akun kas (RECEIPT = debit);
 * kolom *lawan adalah sisi akun kontra. Saldo awal per akun kas =
 * opening_balance + mutasi ledger sisi normal sebelum periode; saldo
 * berjalan diakumulasi per akun kas.
 */
import { FinDatasetConfig, periodFilters } from './fin-doc-mrt-configs';

interface KbhOpts {
  kind?: 'CASH' | 'BANK';
}

const kbhConfig = (o: KbhOpts): FinDatasetConfig => {
  const kindCond = o.kind ? ` AND t.kind = '${o.kind}'` : '';
  return {
    from: `(
WITH prm AS (SELECT COALESCE(?::date, DATE '1900-01-01') AS pstart,
    COALESCE(?::date, CURRENT_DATE) AS pend),
acc AS (
  SELECT DISTINCT bank_account_id AS aid FROM fin_cash_bank_transactions
  WHERE deleted_at IS NULL AND bank_account_id IS NOT NULL),
open AS (
  SELECT a.id AS aid, a.opening_balance + COALESCE(SUM(
    CASE WHEN l.entry_date < (SELECT pstart FROM prm)
      THEN (CASE WHEN a.normal_balance = 'DEBIT' THEN l.debit - l.credit ELSE l.credit - l.debit END)
      ELSE 0 END), 0) AS saldoawal
  FROM md_accounts a
  LEFT JOIN fin_ledger_entries l ON l.account_id = a.id AND l.deleted_at IS NULL
  WHERE a.id IN (SELECT aid FROM acc)
  GROUP BY a.id, a.opening_balance),
tx AS (
  SELECT t.id, t.doc_number, t.transaction_date, t.direction, t.description,
    COALESCE(l.notes, t.notes) AS catatan, t.bank_account_id AS baid,
    l.account_id AS contra, l.amount, l.amount_fx, l.line_no,
    (CASE WHEN t.kind = 'CASH' AND t.direction = 'RECEIPT' THEN 'CR'
          WHEN t.kind = 'CASH' THEN 'CD'
          WHEN t.direction = 'RECEIPT' THEN 'RM' ELSE 'SM' END) AS sumber,
    (CASE WHEN t.direction = 'RECEIPT' THEN l.amount ELSE -l.amount END) AS delta
  FROM fin_cash_bank_transactions t
  JOIN fin_cash_bank_lines l ON l.cash_bank_transaction_id = t.id, prm
  WHERE t.deleted_at IS NULL AND t.posting_status = 'POSTED'
    AND t.transaction_date >= prm.pstart AND t.transaction_date <= prm.pend${kindCond})
SELECT x.*, ba.code AS bacode, ba.name AS baname, ca.code AS cacode, ca.name AS caname,
  COALESCE(o.saldoawal, 0) AS saldoawal,
  SUM(x.delta) OVER (PARTITION BY x.baid ORDER BY x.transaction_date, x.id, x.line_no) AS saldomutasi
FROM tx x
JOIN md_accounts ba ON ba.id = x.baid
JOIN md_accounts ca ON ca.id = x.contra
LEFT JOIN open o ON o.aid = x.baid) s`,
    select: {
      kbhtgl: 's.transaction_date',
      kbhnotransaksi: 's.doc_number',
      kbhnorek: 's.bacode',
      kbhnoreknama: 's.baname',
      kbhdebit: "(CASE WHEN s.direction = 'RECEIPT' THEN s.amount ELSE 0 END)",
      kbhkredit: "(CASE WHEN s.direction = 'DISBURSEMENT' THEN s.amount ELSE 0 END)",
      kbhnoreklawan: 's.cacode',
      kbhnoreklawannama: 's.caname',
      kbhdebitlawan: "(CASE WHEN s.direction = 'DISBURSEMENT' THEN s.amount ELSE 0 END)",
      kbhkreditlawan: "(CASE WHEN s.direction = 'RECEIPT' THEN s.amount ELSE 0 END)",
      kbhsumber: 's.sumber',
      kbhuraian: 's.description',
      kbhcatatan: 's.catatan',
      kbhsaldoawal: 's.saldoawal',
      kbhsaldomutasi: 's.saldomutasi',
      kbhsaldoakhir: '(s.saldoawal + s.saldomutasi)',
      kbhcustomdbl1: 's.amount_fx',
    },
    bindParams: [
      { name: 'period_start', kind: 'date' },
      { name: 'period_end', kind: 'date' },
    ],
    orderBy: 's.bacode, s.transaction_date, s.id, s.line_no',
  };
};

/** DS2 bankharianakunlawan: daftar giro terkait (kolom polos template). */
const kbhGiroDs2Config = (): FinDatasetConfig => ({
  from: `(
SELECT COALESCE(e.entry_date, t.transaction_date) AS tgl,
  COALESCE(e.doc_number, t.doc_number) AS notransaksi, g.giro_number AS nogiro,
  g.amount AS jumlah, g.due_date AS tgljatuhtempo, g.bank_name AS bank,
  cur.code AS matauang, g.source AS sumber
FROM fin_giros g
LEFT JOIN fin_giro_entries e ON e.id = g.giro_entry_id
LEFT JOIN fin_cash_bank_transactions t ON t.id = g.source_transaction_id
LEFT JOIN md_currencies cur ON cur.id = g.currency_id
WHERE g.deleted_at IS NULL AND (e.id IS NULL OR e.deleted_at IS NULL)) s`,
  select: {
    notransaksi: 's.notransaksi',
    nogiro: 's.nogiro',
    jumlah: 's.jumlah',
    tgljatuhtempo: 's.tgljatuhtempo',
    bank: 's.bank',
    matauang: 's.matauang',
    sumber: 's.sumber',
  },
  paramFilters: periodFilters('s.tgl'),
  orderBy: 's.tgl, s.nogiro',
});

export const FIN_G3_DAILY_CONFIGS: Record<string, Record<string, FinDatasetConfig>> = {
  'fin.kasharianakunlawan': { DS1: kbhConfig({ kind: 'CASH' }), DS2: kbhConfig({ kind: 'CASH' }) },
  'fin.kasharianakunlawandetail': { DS1: kbhConfig({ kind: 'CASH' }) },
  'fin.kasharianakunlawanglobal': { DS1: kbhConfig({ kind: 'CASH' }) },
  'fin.bankharianakunlawan': { DS1: kbhConfig({ kind: 'BANK' }), DS2: kbhGiroDs2Config() },
  'fin.bukubesarakunlawan': { DS1: kbhConfig({}), DS2: kbhConfig({}) },
  'fin.cashflow': { DS1: kbhConfig({}) },
};
