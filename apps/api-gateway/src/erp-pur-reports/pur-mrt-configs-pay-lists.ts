/**
 * Vendor-payment list + advance configs (Wave G5), split from
 * pur-mrt-configs-pay.ts for the 400-line rule: vendor advances
 * (source 'AP') balances and lists, VP/VPP payment lists with
 * per-sumber allocation totals, and the purchase-return settlement
 * list. Shared helpers come from pur-mrt-configs-pay.ts.
 */

import { statusLabel, docFilters, type PurDatasetConfig, type PurReportConfig } from './pur-mrt-configs';
import { headerSelect, instrumentConfig, PAY_JOINS } from './pur-mrt-configs-pay';

/* ------------------------------ advances ------------------------------ */

const settledSum =
  '(SELECT COALESCE(SUM(a.amount), 0) FROM fin_settlement_allocations a WHERE a.invoice_ref = t.doc_number)';
const settledSumFx =
  '(SELECT COALESCE(SUM(a.amount_fx), 0) FROM fin_settlement_allocations a WHERE a.invoice_ref = t.doc_number)';

/** daftarum*: vendor advance (source AP) balances. */
function advanceConfig(detail: boolean): PurDatasetConfig {
  return {
    from: `
      fin_ap_payments t
      ${detail ? 'LEFT JOIN fin_settlement_allocations a ON a.invoice_ref = t.doc_number LEFT JOIN fin_ap_payments vp ON vp.id = a.ap_payment_id LEFT JOIN md_currencies vcur ON vcur.id = vp.currency_id' : ''}
      ${PAY_JOINS}
    `,
    select: {
      ...headerSelect('ap'),
      apkontak: 'p.name',
      apkontakkode: 'p.code',
      apkontaknama: 'p.name',
      aptgljatuhtempo: 't.settled_date',
      aptgllunas: 't.settled_date',
      apjumlah: 't.amount_fx',
      apjumlahvalas: 't.amount',
      apjumlahbayar: settledSumFx,
      apjumlahbayarvalas: settledSum,
      apsisa: `(t.amount_fx - ${settledSumFx})`,
      apsisavalas: `(t.amount - ${settledSum})`,
      apstatusbayar: `CASE WHEN ${settledSum} >= t.amount THEN 'Lunas'
        WHEN ${settledSum} > 0 THEN 'Sebagian' ELSE 'Belum Lunas' END`,
      ...(detail
        ? {
            vpid: 'vp.id',
            vpnotransaksi: 'vp.doc_number',
            vptgl: 'vp.transaction_date',
            vpsupplier: 'p.name',
            vpsupplierkode: 'p.code',
            vpsuppliernama: 'p.name',
            vpuraian: 'vp.description',
            vpcatatan: 'vp.notes',
            vpmatauang: 'vcur.code',
            vpkurs: 'vp.exchange_rate',
            jmlbayar: 'a.amount_fx',
            jmlbayarvalas: 'a.amount',
          }
        : {}),
    },
    where: "t.source = 'AP'",
    deletedAlias: 't',
    paramFilters: docFilters('t.transaction_date'),
    orderBy: 't.transaction_date, t.doc_number',
  };
}

/** listadvancepurchase*: AP payments with their first instrument + terbilang. */
function advanceListConfig(withTerbilang: boolean): PurDatasetConfig {
  return {
    from: `
      fin_ap_payments t
      LEFT JOIN LATERAL (
        SELECT method, bank_account_id, giro_account_id FROM fin_payment_instruments
        WHERE ap_payment_id = t.id ORDER BY line_no LIMIT 1) ins ON TRUE
      LEFT JOIN md_accounts ba ON ba.id = ins.bank_account_id
      LEFT JOIN md_accounts ga ON ga.id = ins.giro_account_id
      ${PAY_JOINS}
    `,
    select: {
      ...headerSelect('ap'),
      apjenis: 't.source',
      apnorek: 'acct.code',
      carabayar: `CASE ins.method::text WHEN 'CASH' THEN 'Cash' WHEN 'TRANSFER' THEN 'Transfer'
        WHEN 'GIRO' THEN 'Check/Giro' WHEN 'CHEQUE' THEN 'Check/Giro' ELSE '' END`,
      noakun: `CASE WHEN ins.method::text IN ('GIRO', 'CHEQUE') THEN ga.code ELSE ba.code END`,
      namaakun: `CASE WHEN ins.method::text IN ('GIRO', 'CHEQUE') THEN ga.name ELSE ba.name END`,
      matauang: 'cur.code',
      jumlah: 't.amount_fx',
      jumlahvalas: 't.amount',
      snilai: 't.amount_fx',
    },
    where: "t.source = 'AP'",
    deletedAlias: 't',
    paramFilters: docFilters('t.transaction_date'),
    orderBy: 't.transaction_date, t.doc_number',
    ...(withTerbilang
      ? {
          terbilang: [
            {
              column: 'terbilangJumlah',
              mode: 'first' as const,
              amountColumn: 'jumlah',
              groupByColumn: 'apnotransaksi',
              currencyColumn: 'apmatauang',
            },
            {
              column: 'terbilangvalas',
              mode: 'first' as const,
              amountColumn: 'jumlahvalas',
              groupByColumn: 'apnotransaksi',
              currencyColumn: 'apmatauang',
            },
          ],
        }
      : {}),
  };
}

/* --------------------------- payment lists --------------------------- */

const sumBySumber = (matchSql: string) =>
  `(SELECT COALESCE(SUM(a.amount), 0) FROM fin_settlement_allocations a WHERE a.ap_payment_id = t.id AND ${matchSql})`;
const RI_MATCH = `EXISTS (SELECT 1 FROM pur_invoices d WHERE d.doc_number = a.invoice_ref)`;
const AP_MATCH = `EXISTS (SELECT 1 FROM fin_ap_payments d WHERE d.doc_number = a.invoice_ref AND d.source = 'AP')`;
const PRT_MATCH = `EXISTS (SELECT 1 FROM pur_returns d WHERE d.doc_number = a.invoice_ref)`;
const CA_MATCH = `NOT EXISTS (SELECT 1 FROM pur_invoices d WHERE d.doc_number = a.invoice_ref)
  AND NOT EXISTS (SELECT 1 FROM fin_ap_payments d WHERE d.doc_number = a.invoice_ref)
  AND NOT EXISTS (SELECT 1 FROM pur_returns d WHERE d.doc_number = a.invoice_ref)`;

function vpListConfig(): PurDatasetConfig {
  return {
    from: `fin_ap_payments t ${PAY_JOINS}`,
    select: {
      ...headerSelect('vp'),
      jumlahnayar: '(SELECT COALESCE(SUM(a.amount), 0) FROM fin_settlement_allocations a WHERE a.ap_payment_id = t.id)',
      jumlahbayar: '(SELECT COALESCE(SUM(a.amount), 0) FROM fin_settlement_allocations a WHERE a.ap_payment_id = t.id)',
      vptotalap: sumBySumber(AP_MATCH),
      vptotalar: sumBySumber(RI_MATCH),
      vpptotalap: sumBySumber(AP_MATCH),
      vpptotalar: sumBySumber(RI_MATCH),
      totalRI: sumBySumber(RI_MATCH),
      totalAP: sumBySumber(AP_MATCH),
      totalPRT: sumBySumber(PRT_MATCH),
      totalCA: sumBySumber(CA_MATCH),
      jmlRI: `(SELECT COUNT(*) FROM fin_settlement_allocations a WHERE a.ap_payment_id = t.id AND ${RI_MATCH})`,
      jmlbayar: '(SELECT COALESCE(SUM(a.amount), 0) FROM fin_settlement_allocations a WHERE a.ap_payment_id = t.id)',
      jmlbayar1: '(SELECT COALESCE(SUM(a.amount), 0) FROM fin_settlement_allocations a WHERE a.ap_payment_id = t.id)',
      vppselisihkurs: 't.fx_gain_loss_amount',
    },
    where: "t.source = 'VP'",
    deletedAlias: 't',
    paramFilters: docFilters('t.transaction_date'),
    orderBy: 't.transaction_date, t.doc_number',
  };
}

/** PRT documents × the VP allocations that settled them. */
function prtPaymentConfig(): PurDatasetConfig {
  return {
    from: `
      pur_returns t
      LEFT JOIN fin_settlement_allocations a ON a.invoice_ref = t.doc_number
      LEFT JOIN fin_ap_payments vp ON vp.id = a.ap_payment_id
      LEFT JOIN md_partners p ON p.id = t.supplier_id
      LEFT JOIN md_currencies cur ON cur.id = t.currency_id
      LEFT JOIN md_currencies vcur ON vcur.id = vp.currency_id
    `,
    select: {
      prtnotransaksi: 't.doc_number',
      prttgl: 't.doc_date',
      prturaian: 't.description',
      prtmatauang: 'cur.code',
      prtkurs: 't.exchange_rate',
      knama: 'p.name',
      matauang: 'vcur.code',
      kurs: 'vp.exchange_rate',
      jmlbayar: 'a.amount',
      jmlbayarvalas: 'a.amount_fx',
      terbayar:
        '(SELECT COALESCE(SUM(a2.amount), 0) FROM fin_settlement_allocations a2 WHERE a2.invoice_ref = t.doc_number)',
      vpnotransaksi: 'vp.doc_number',
      vptgl: 'vp.transaction_date',
      vpcatatan: 'vp.notes',
    },
    where: "t.return_type::text = 'RETURN_TO_VENDOR'",
    deletedAlias: 't',
    paramFilters: docFilters('t.doc_date'),
    orderBy: 't.doc_date, t.doc_number',
  };
}

export const PUR_PAY_LIST_CONFIGS: Record<string, PurReportConfig> = {
  'pur.listvendorpayment': { datasets: { DS1: vpListConfig() } },
  'pur.listvendorpayment2': { datasets: { DS1: vpListConfig() } },
  'pur.listvendorpaymentplan': {
    datasets: {
      DS1: {
        ...vpListConfig(),
        where: "t.source = 'VPP'",
        select: {
          vppnotransaksi: 't.doc_number',
          vpptgl: 't.transaction_date',
          vppsupplier: 'p.name',
          vppstatus: statusLabel('t.status'),
          vppuraian: 't.description',
          vppmatauang: 'cur.code',
          vppkurs: 't.exchange_rate',
          jumlahbayar:
            '(SELECT COALESCE(SUM(a.amount), 0) FROM fin_settlement_allocations a WHERE a.ap_payment_id = t.id)',
          vpptotalap: sumBySumber(AP_MATCH),
          vpptotalar: sumBySumber(RI_MATCH),
        },
      },
    },
  },

  /* Vendor advances (source AP) */
  'pur.daftarumpembelian': { datasets: { DS1: advanceConfig(false) } },
  'pur.daftarumpembeliandetail': { datasets: { DS1: advanceConfig(true) } },
  'pur.listadvancepurchase': {
    datasets: {
      DS1: advanceListConfig(true),
      DS2: advanceListConfig(true),
      DS3: instrumentConfig('ap', 'AP'),
    },
  },
  'pur.listadvancepurchasettd': {
    datasets: {
      DS1: advanceListConfig(true),
      DS2: advanceListConfig(true),
      DS3: instrumentConfig('ap', 'AP'),
    },
  },
  'pur.listadvancepurchaseglobal': {
    datasets: { DS1: advanceListConfig(false), DS2: instrumentConfig('ap', 'AP') },
  },
  'pur.listadvancepurchaseoutstandingpembayaran': {
    datasets: { DS1: advanceConfig(true) },
  },

  /* Purchase-return settlement list */
  'pur.listpurchasereturnoutstandingpayment': {
    datasets: { DS1: prtPaymentConfig() },
  },
};
