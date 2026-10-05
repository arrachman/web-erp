/**
 * Journal document configs (Wave G2): general / memorial / adjustment
 * journal vouchers + opening-balance (saldo awal) documents. Legacy
 * kept four tables (m2_gj / m2_jm / m2_aj / m2_cb); ERP unifies them
 * in fin_journal_entries discriminated by journal_type.
 */

import {
  docFilters,
  statusLabel,
  type FinDatasetConfig,
  type FinReportConfig,
} from './fin-doc-mrt-configs';

const JOURNAL_FROM = `
  fin_journal_entries t
  JOIN fin_journal_lines l ON l.journal_entry_id = t.id
  JOIN md_accounts a ON a.id = l.account_id
  LEFT JOIN md_partners p ON p.id = t.partner_id
  JOIN md_currencies hcur ON hcur.id = t.currency_id
  JOIN md_currencies lcur ON lcur.id = l.currency_id
`;

function journalConfig(
  prefix: 'gj' | 'jm' | 'aj',
  journalType: string,
): FinDatasetConfig {
  return {
    from: JOURNAL_FROM,
    select: {
      [`${prefix}notransaksi`]: 't.doc_number',
      [`${prefix}tgl`]: 't.entry_date',
      [`${prefix}uraian`]: 't.description',
      kkode: 'p.code',
      knama: 'p.name',
      norek: 'a.code',
      cnama: 'a.name',
      [`${prefix}matauang`]: 'hcur.code',
      matauang: 'lcur.code',
      [`${prefix}kurs`]: 't.exchange_rate',
      debit: 'l.debit',
      debitvalas: 'l.debit_fx',
      kredit: 'l.credit',
      kreditvalas: 'l.credit_fx',
      urutan: 'l.line_no',
      catatan: 'l.notes',
    },
    where: `t.journal_type = '${journalType}'`,
    orderBy: 't.doc_number, l.line_no',
    deletedAlias: 't',
    paramFilters: docFilters('t.entry_date'),
  };
}

/** Opening balance: DS1/DS2 are the same line set (two printed copies);
 *  header totals are window sums over the document's lines; DS3 lists
 *  giro instruments attached to the opening document. */
function openingBalanceConfig(): FinReportConfig {
  const lines: FinDatasetConfig = {
    from: JOURNAL_FROM,
    select: {
      cbnotransaksi: 't.doc_number',
      cbtgl: 't.entry_date',
      cnomor: 'a.code',
      cnama: 'a.name',
      catatan: 'l.notes',
      statuscb: statusLabel('t.status::text'),
      kontak: 'p.name',
      cbmatauang: 'hcur.code',
      cbkurs: 't.exchange_rate',
      cburaian: 't.description',
      cbkredit: 'SUM(l.credit) OVER (PARTITION BY t.id)',
      cbdebit: 'SUM(l.debit) OVER (PARTITION BY t.id)',
      cbdebitvalas: 'SUM(COALESCE(l.debit_fx, 0)) OVER (PARTITION BY t.id)',
      cbkreditvalas: 'SUM(COALESCE(l.credit_fx, 0)) OVER (PARTITION BY t.id)',
      kredit: 'l.credit',
      kreditvalas: 'l.credit_fx',
      debit: 'l.debit',
      debitvalas: 'l.debit_fx',
      cbid: 't.id',
      norek: 'a.code',
    },
    where: `t.journal_type = 'OPENING_BALANCE'`,
    orderBy: 't.doc_number, l.line_no',
    deletedAlias: 't',
    paramFilters: docFilters('t.entry_date'),
  };
  return {
    datasets: {
      DS1: lines,
      DS2: lines,
      DS3: {
        from: `
          fin_giros g
          JOIN fin_journal_entries t ON t.id = g.source_transaction_id
          LEFT JOIN md_accounts a ON a.id = g.giro_account_id
          LEFT JOIN md_partners p ON p.id = t.partner_id
          JOIN md_currencies hcur ON hcur.id = t.currency_id
        `,
        select: {
          cbnotransaksi: 't.doc_number',
          cbtgl: 't.entry_date',
          cnomor: 'a.code',
          cnama: 'a.name',
          jenisgiro: `CASE g.type WHEN 'INCOMING' THEN 'Masuk' ELSE 'Keluar' END`,
          jumlah: 'g.amount',
          tgljt: 'g.due_date',
          noacbank: 'g.bank_account_no',
          bank: 'g.bank_name',
          nogiro: 'g.giro_number',
          knama: 'p.name',
          cbmatauang: 'hcur.code',
          cbkurs: 't.exchange_rate',
          cbid: 't.id',
        },
        where: 'g.deleted_at IS NULL AND t.deleted_at IS NULL',
        orderBy: 't.doc_number, g.giro_number',
        paramFilters: docFilters('t.entry_date'),
      },
    },
  };
}

export const JOURNAL_DOC_CONFIGS: Record<string, FinReportConfig> = {
  'fin.generaljournaldetail': { datasets: { DS1: journalConfig('gj', 'GENERAL') } },
  'fin.generaljournaldetail1': { datasets: { DS1: journalConfig('gj', 'GENERAL') } },
  'fin.generaljournaldetail2': { datasets: { DS1: journalConfig('gj', 'GENERAL') } },
  'fin.jurnalmemorialdetail': { datasets: { DS1: journalConfig('jm', 'MEMORIAL') } },
  'fin.jurnalmemorialdetail2': { datasets: { DS1: journalConfig('jm', 'MEMORIAL') } },
  'fin.adjustmentjournaldetail': { datasets: { DS1: journalConfig('aj', 'ADJUSTMENT') } },
  'fin.adjustmentjournaldetail2': { datasets: { DS1: journalConfig('aj', 'ADJUSTMENT') } },
  'fin.saldoawalcoa': openingBalanceConfig(),
  'fin.saldoawalcoa2': openingBalanceConfig(),
};
