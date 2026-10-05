/**
 * Cash/bank document configs (Wave G2): CR / CD / RM / SM printed
 * vouchers + SIN flat variants + kasbon. Legacy printed one dataset
 * row per document LINE (header fields repeated); the header's own
 * cash/bank account is t.bank_account_id (ba), each line carries its
 * contra account (a). Legacy `sumber` ('CR'/'CD'/'RM'/'SM') maps to
 * ERP kind+direction on the shared fin_cash_bank_transactions table.
 */

import {
  docFilters,
  type FinDatasetConfig,
  type FinReportConfig,
} from './fin-doc-mrt-configs';

const LINE_FROM = `
  fin_cash_bank_transactions t
  JOIN fin_cash_bank_lines l ON l.cash_bank_transaction_id = t.id
  JOIN md_accounts a ON a.id = l.account_id
  JOIN md_accounts ba ON ba.id = t.bank_account_id
  LEFT JOIN md_banks bk ON bk.id = a.bank_id
  JOIN md_currencies lcur ON lcur.id = l.currency_id
  LEFT JOIN md_partners p ON p.id = t.partner_id
`;

const GIRO_FROM = `
  fin_giros g
  JOIN fin_cash_bank_transactions t ON t.id = g.source_transaction_id
  LEFT JOIN md_accounts ga ON ga.id = g.giro_account_id
  LEFT JOIN md_partners p ON p.id = t.partner_id
`;

const docWhere = (kind: string, direction: string) =>
  `t.kind = '${kind}' AND t.direction = '${direction}'`;

/** CR/CD/RM style: rows = lines, line amount aliased `kredit` (legacy naming). */
function lineDocConfig(
  prefix: 'cr' | 'cd' | 'rm',
  kind: string,
  direction: string,
): FinDatasetConfig {
  return {
    from: LINE_FROM,
    select: {
      urutan: 'l.line_no',
      ckodebank: 'bk.code',
      cnomor: 'a.code',
      cnama: 'a.name',
      norek: 'a.code',
      kredit: 'l.amount',
      kreditvalas: 'l.amount_fx',
      [`${prefix}notransaksi`]: 't.doc_number',
      [`${prefix}tgl`]: 't.transaction_date',
      catatan: 'l.notes',
      matauang: 'lcur.code',
      kurs: 'l.exchange_rate',
      cruraian: 't.description',
    },
    where: docWhere(kind, direction),
    orderBy: 't.transaction_date, t.doc_number, l.line_no',
    deletedAlias: 't',
    paramFilters: docFilters('t.transaction_date'),
    terbilang: {
      column: 'terbilang',
      mode: 'sum',
      amountColumn: 'kredit',
      groupByColumn: `${prefix}notransaksi`,
      currencyColumn: 'matauang',
    },
  };
}

/** Giro instruments attached to a cash/bank document (legacy DS2). */
function giroDs2Config(
  prefix: 'rm' | 'sm',
  giroType: 'INCOMING' | 'OUTGOING',
): FinDatasetConfig {
  return {
    from: GIRO_FROM,
    select: {
      glurutan: 'g.line_no',
      glbanknama: 'g.bank_name',
      glbank: 'g.bank_name',
      glnogiro: 'g.giro_number',
      glnoacbank: 'g.bank_account_no',
      gljumlah: 'g.amount',
      gltgljthtempo: 'g.due_date',
      cnama: 'ga.name',
      cnomor: 'ga.code',
      glrekbank: 'ga.code',
      glrekbanknama: 'ga.name',
      kredit: 'g.amount',
      debit: 't.amount',
      debit2: 'g.amount',
      jumlah: 'g.amount',
      nogiro: 'g.giro_number',
      rekbank: 'ga.code',
      noacbank: 'g.bank_account_no',
      bank: 'g.bank_name',
      tgljt: 'g.due_date',
      smid: 't.id',
      [`${prefix}notransaksi`]: 't.doc_number',
      [`${prefix}sumber`]: 't.source',
    },
    where: `g.type = '${giroType}' AND g.deleted_at IS NULL AND t.deleted_at IS NULL`,
    orderBy: 't.doc_number, g.line_no',
    paramFilters: docFilters('t.transaction_date', { partner: false }),
  };
}

/** SM detail (non-cb): rows repeat header values per line; `a` = bank account name. */
const SM_FROM = `
  fin_cash_bank_transactions t
  JOIN fin_cash_bank_lines l ON l.cash_bank_transaction_id = t.id
  JOIN md_accounts ba ON ba.id = t.bank_account_id
  LEFT JOIN md_banks bk ON bk.id = ba.bank_id
  JOIN md_currencies lcur ON lcur.id = l.currency_id
  LEFT JOIN md_partners p ON p.id = t.partner_id
`;

function spendMoneyDetail(): FinReportConfig {
  return {
    datasets: {
      DS1: {
        from: SM_FROM,
        select: {
          urutan: 'l.line_no',
          ckodebank: 'bk.code',
          cnomor: 'ba.code',
          cnama: 'ba.name',
          smnorek: 'ba.code',
          debit: 't.amount',
          debitvalas: 't.amount_fx',
          smnotransaksi: 't.doc_number',
          catatan: 'l.notes',
          matauang: 'lcur.code',
          smtgl: 't.transaction_date',
          a: 'ba.name',
        },
        where: docWhere('BANK', 'DISBURSEMENT'),
        orderBy: 't.doc_number, l.line_no',
        deletedAlias: 't',
        paramFilters: docFilters('t.transaction_date'),
      },
      DS2: giroDs2Config('sm', 'OUTGOING'),
    },
  };
}

function spendMoneyDetailCb(): FinReportConfig {
  return {
    datasets: {
      DS1: {
        from: LINE_FROM,
        select: {
          urutan: 'l.line_no',
          ckodebank: 'bk.code',
          cnomor: 'a.code',
          cnama: 'a.name',
          smnorek: 'ba.code',
          norek: 'a.code',
          debit: 't.amount',
          debitvalas: 't.amount_fx',
          debit2: 'l.amount',
          debitvalas2: 'l.amount_fx',
          smnotransaksi: 't.doc_number',
          catatan: 'l.notes',
          matauang: 'lcur.code',
          kurs: 'l.exchange_rate',
          smtgl: 't.transaction_date',
          smid: 't.id',
          a: 'ba.name',
        },
        where: docWhere('BANK', 'DISBURSEMENT'),
        orderBy: 't.doc_number, l.line_no',
        deletedAlias: 't',
        paramFilters: docFilters('t.transaction_date'),
        terbilang: {
          column: 'terbilang',
          mode: 'first',
          amountColumn: 'debit',
          groupByColumn: 'smnotransaksi',
          currencyColumn: 'matauang',
        },
      },
      DS2: giroDs2Config('sm', 'OUTGOING'),
    },
  };
}

/** SIN flat layout: header account + partner printed per line row. */
function sinDocConfig(
  kind: string,
  direction: string,
  uraianColumn: 'rmuraian' | 'cduraian',
): FinReportConfig {
  return {
    datasets: {
      DS1: {
        from: LINE_FROM,
        select: {
          cdnotransaksi: 't.doc_number',
          cdtgl: 't.transaction_date',
          cdnorek: 'ba.code',
          cnama: 'ba.name',
          knama: 'p.name',
          cdmatauang: 'lcur.code',
          cdjumlah: 't.amount',
          jumlah: 'l.amount',
          norek: 'a.code',
          nama: 'a.name',
          [uraianColumn]: 't.description',
        },
        where: docWhere(kind, direction),
        orderBy: 't.transaction_date, t.doc_number, l.line_no',
        deletedAlias: 't',
        paramFilters: docFilters('t.transaction_date'),
        terbilang: {
          column: 'terbilang',
          mode: 'first',
          amountColumn: 'cdjumlah',
          groupByColumn: 'cdnotransaksi',
          currencyColumn: 'cdmatauang',
        },
      },
    },
  };
}

export const CASHBANK_DOC_CONFIGS: Record<string, FinReportConfig> = {
  'fin.cashreceiptdetail': { datasets: { DS1: lineDocConfig('cr', 'CASH', 'RECEIPT') } },
  'fin.cashreceiptdetailcb': { datasets: { DS1: lineDocConfig('cr', 'CASH', 'RECEIPT') } },
  // bank.mrt prints the same cash-receipt voucher on bank paper.
  'fin.bank': { datasets: { DS1: lineDocConfig('cr', 'CASH', 'RECEIPT') } },
  'fin.cashdisbursementsdetail': { datasets: { DS1: lineDocConfig('cd', 'CASH', 'DISBURSEMENT') } },
  'fin.receivemoneydetail': {
    datasets: {
      DS1: lineDocConfig('rm', 'BANK', 'RECEIPT'),
      DS2: giroDs2Config('rm', 'INCOMING'),
    },
  },
  'fin.receivemoneydetailcb': {
    datasets: {
      DS1: lineDocConfig('rm', 'BANK', 'RECEIPT'),
      DS2: giroDs2Config('rm', 'INCOMING'),
    },
  },
  'fin.spendmoneydetail': spendMoneyDetail(),
  'fin.spendmoneydetailcb': spendMoneyDetailCb(),
  'fin.bankmasuksin': sinDocConfig('BANK', 'RECEIPT', 'rmuraian'),
  // Legacy bankkeluar_SIN.mrt SQL literally duplicates the RM query
  // (copy-paste in legacy); mapped to spend-money so the document is
  // the bank-OUT voucher its name promises. Noted in DECISIONS.
  'fin.bankkeluarsin': sinDocConfig('BANK', 'DISBURSEMENT', 'rmuraian'),
  'fin.kaskeluarsin': sinDocConfig('CASH', 'DISBURSEMENT', 'cduraian'),
  'fin.kaskeluarkasbon': {
    datasets: {
      DS1: {
        from: `
          fin_cash_bank_transactions t
          JOIN md_currencies hcur ON hcur.id = t.currency_id
        `,
        select: {
          cdtgl: 't.transaction_date',
          cdnotransaksi: 't.doc_number',
          cduraian: 't.description',
          cdmatauang: 'hcur.code',
          cdjumlah: 't.amount',
          nama: `CASE t.status::text WHEN 'DRAFT' THEN 'Draft' WHEN 'NEED_APPROVE' THEN 'Need Approve' WHEN 'APPROVED' THEN 'Approved' WHEN 'REJECTED' THEN 'Rejected' WHEN 'POSTED' THEN 'Posted' WHEN 'VOID' THEN 'Void' WHEN 'CANCELLED' THEN 'Cancelled' ELSE t.status::text END`,
        },
        where: docWhere('CASH', 'DISBURSEMENT'),
        orderBy: 't.transaction_date, t.id',
        deletedAlias: 't',
        paramFilters: docFilters('t.transaction_date', { partner: false }),
        terbilang: {
          column: 'terbilang',
          mode: 'first',
          amountColumn: 'cdjumlah',
          groupByColumn: 'cdnotransaksi',
          currencyColumn: 'cdmatauang',
        },
      },
    },
  },
};
