/**
 * Vendor-payment configs (Wave G5): VP (Vendor Payment), VPP (Vendor
 * Payment Plan) and AP (vendor advances) all live in fin_ap_payments
 * discriminated by `source` ('VP' | 'VPP' | 'AP' | 'PP'); settlements
 * in fin_settlement_allocations whose invoice_ref is the settled
 * document's numeric ID as text; instruments in
 * fin_payment_instruments.
 * fin_ap_payments dates are `transaction_date`; amounts are document
 * currency in `amount` and base currency in `amount_fx`.
 *
 * Allocation `sumber` derivation (legacy PaidTo semantics):
 *   invoice_ref matches pur_invoices   → 'RI'
 *   invoice_ref matches a VPP payment  → 'VPP'
 *   invoice_ref matches an AP payment  → 'AP'
 *   invoice_ref matches pur_returns    → 'PRT'
 *   otherwise                          → 'CA' (cash/bank account)
 * The daftar* templates negate jmlbayar for AP/PRT sources (money
 * flowing back); the vendorpayment documents keep amounts positive.
 */

import {
  docFilters,
  statusLabel,
  type PurDatasetConfig,
  type PurReportConfig,
} from './pur-mrt-configs';

export const PAY_JOINS = `
  LEFT JOIN md_partners p ON p.id = t.partner_id
  LEFT JOIN md_currencies cur ON cur.id = t.currency_id
  LEFT JOIN md_accounts acct ON acct.id = t.bank_account_id
  LEFT JOIN adm_users usr ON usr.id = t.created_by_id
  LEFT JOIN adm_users upd ON upd.id = t.updated_by_id
`;

/** Header columns for prefix x ('vp' | 'vpp' | 'ap'). */
export function headerSelect(x: string): Record<string, string> {
  return {
    [`${x}id`]: 't.id',
    [`${x}notransaksi`]: 't.doc_number',
    [`${x}tgl`]: 't.transaction_date',
    [`${x}supplier`]: 'p.name',
    [`${x}supplierkode`]: 'p.code',
    [`${x}suppliernama`]: 'p.name',
    [`${x}uraian`]: 't.description',
    [`${x}catatan`]: 't.notes',
    [`${x}status`]: statusLabel('t.status'),
    [`${x}statusnama`]: statusLabel('t.status'),
    [`${x}inputuser`]: 'usr.name',
    [`${x}inputtgl`]: 't.created_at',
    [`${x}modifikasiuser`]: 'upd.name',
    [`${x}modifikasitgl`]: 't.updated_at',
    [`${x}matauang`]: 'cur.code',
    [`${x}kurs`]: 't.exchange_rate',
    [`${x}totaltransaksi`]: 't.amount',
    [`${x}kodeakun`]: 'acct.code',
    [`${x}namaakun`]: 'acct.name',
    knama: 'p.name',
    kkode: 'p.code',
    cnobukti: 't.doc_number',
  };
}

function headerConfig(x: string, source: string): PurDatasetConfig {
  return {
    from: `fin_ap_payments t ${PAY_JOINS}`,
    select: headerSelect(x),
    where: `t.source = '${source}'`,
    deletedAlias: 't',
    paramFilters: docFilters('t.transaction_date'),
    orderBy: 't.transaction_date, t.doc_number',
  };
}

/* --------------------------- allocations --------------------------- */

// fin_settlement_allocations.invoice_ref stores the settled document's
// numeric ID as text (ap-payment-posting.service) — join on id::text.
const ALLOC_JOINS = `
  LEFT JOIN pur_invoices ri ON ri.id::text = a.invoice_ref
  LEFT JOIN fin_ap_payments vpp ON vpp.id::text = a.invoice_ref AND vpp.source = 'VPP'
  LEFT JOIN fin_ap_payments ap ON ap.id::text = a.invoice_ref AND ap.source = 'AP'
  LEFT JOIN pur_returns rt ON rt.id::text = a.invoice_ref
`;

const SUMBER = `CASE WHEN ri.id IS NOT NULL THEN 'RI'
  WHEN vpp.id IS NOT NULL THEN 'VPP'
  WHEN ap.id IS NOT NULL THEN 'AP'
  WHEN rt.id IS NOT NULL THEN 'PRT' ELSE 'CA' END`;

const DOC_TOTAL = 'COALESCE(ri.grand_total, vpp.amount, ap.amount, rt.grand_total)';
const DOC_DATE = 'COALESCE(ri.doc_date, vpp.transaction_date, ap.transaction_date, rt.doc_date)';
const DOC_RATE = 'COALESCE(ri.exchange_rate, vpp.exchange_rate, ap.exchange_rate, rt.exchange_rate)';
const DOC_DUE = 'COALESCE(ri.due_date, rt.due_date)';

function allocConfig(
  x: 'vp' | 'vpp',
  source: string,
  negateAdvances: boolean,
): PurDatasetConfig {
  const jmlbayar = negateAdvances
    ? `(CASE WHEN ap.id IS NOT NULL OR rt.id IS NOT NULL THEN -a.amount ELSE a.amount END)`
    : 'a.amount';
  return {
    from: `
      fin_ap_payments t
      JOIN fin_settlement_allocations a ON a.ap_payment_id = t.id
      ${ALLOC_JOINS}
      ${PAY_JOINS}
    `,
    select: {
      ...headerSelect(x),
      idtransaksi: 'a.id',
      [`id${x}detail`]: 'a.id',
      sumber: SUMBER,
      notransaksi: 'COALESCE(ri.doc_number, vpp.doc_number, ap.doc_number, rt.doc_number, a.invoice_ref)',
      tgl: DOC_DATE,
      tgltransaksi: DOC_DATE,
      tgljt: DOC_DUE,
      matauang: 'cur.code',
      kurs: DOC_RATE,
      totaltransaksi: DOC_TOTAL,
      terbayar:
        '(SELECT COALESCE(SUM(a2.amount), 0) FROM fin_settlement_allocations a2 WHERE a2.invoice_ref = a.invoice_ref)',
      rencana: 'a.amount',
      jmlbayar,
      jmlbayarvalas: 'a.amount_fx',
      catatan: 't.notes',
      // daftarvp DS1 links the allocation back to its source VPP —
      // only for VP documents (for VPP documents these keys ARE the
      // header's own number/date and must not be overridden).
      ...(x === 'vp'
        ? {
            idvppdetail: 'a.id',
            vpptgl: 'vpp.transaction_date',
            vppnotransaksi: 'vpp.doc_number',
            nama: 'p.name',
          }
        : {}),
      tanggal: DOC_DATE,
      kjumlah: 't.amount',
      subt: 'a.amount',
    },
    where: `t.source = '${source}'`,
    deletedAlias: 't',
    paramFilters: docFilters('t.transaction_date'),
    orderBy: 't.transaction_date, t.doc_number, a.line_no',
    terbilang: [
      {
        column: 'terbilang',
        mode: 'sum',
        amountColumn: 'jmlbayar',
        groupByColumn: `${x}notransaksi`,
        currencyColumn: `${x}matauang`,
      },
    ],
  };
}

/* --------------------------- instruments --------------------------- */

export function instrumentConfig(x: 'vp' | 'vpp' | 'ap', source: string): PurDatasetConfig {
  return {
    from: `
      fin_ap_payments t
      JOIN fin_payment_instruments ins ON ins.ap_payment_id = t.id
      LEFT JOIN md_accounts ba ON ba.id = ins.bank_account_id
      LEFT JOIN md_accounts ga ON ga.id = ins.giro_account_id
      LEFT JOIN fin_giros gr ON gr.id = ins.giro_id
      ${PAY_JOINS}
    `,
    select: {
      [`${x}notransaksi`]: 't.doc_number',
      [`${x}tgl`]: 't.transaction_date',
      [`${x}supplier`]: 'p.name',
      [`${x}matauang`]: 'cur.code',
      [`${x}kurs`]: 't.exchange_rate',
      [`${x}uraian`]: 't.description',
      [`${x}totaltransaksi`]: 't.amount',
      idap: 't.id',
      urutan: 'ins.line_no',
      cara: `CASE ins.method::text WHEN 'CASH' THEN 0 WHEN 'TRANSFER' THEN 1
        WHEN 'GIRO' THEN 2 WHEN 'CHEQUE' THEN 2 WHEN 'CARD' THEN 3 ELSE 4 END`,
      carabayar: `CASE ins.method::text WHEN 'CASH' THEN 'Cash' WHEN 'TRANSFER' THEN 'Transfer'
        WHEN 'GIRO' THEN 'Check/Giro' WHEN 'CHEQUE' THEN 'Check/Giro' ELSE 'Other' END`,
      jumlah: 'ins.amount',
      jumlahvalas: 'ins.amount_fx',
      tgljt: 'ins.due_date',
      bank: 'ins.bank_name',
      nogiro: 'gr.giro_number',
      noacbank: 'ins.bank_account_no',
      cnama: `CASE WHEN ins.method::text IN ('GIRO', 'CHEQUE') THEN ga.name ELSE ba.name END`,
      cnomor: `CASE WHEN ins.method::text IN ('GIRO', 'CHEQUE') THEN ga.code ELSE ba.code END`,
      apjumlahbayar:
        '(SELECT COALESCE(SUM(a.amount), 0) FROM fin_settlement_allocations a WHERE a.invoice_ref = t.id::text)',
    },
    where: `t.source = '${source}'`,
    deletedAlias: 't',
    paramFilters: docFilters('t.transaction_date'),
    orderBy: 't.transaction_date, t.doc_number, ins.line_no',
  };
}

/** VP/VPP documents: DS1+DS2 allocations, DS3 instruments. */
const payDoc = (x: 'vp' | 'vpp', source: string, negate: boolean): PurReportConfig => ({
  datasets: {
    DS1: allocConfig(x, source, negate),
    DS2: allocConfig(x, source, negate),
    DS3: instrumentConfig(x, source),
  },
});

export const PUR_PAY_CONFIGS: Record<string, PurReportConfig> = {
  'pur.vendorpayment': payDoc('vp', 'VP', false),
  'pur.vendorpayment2newmakmur': payDoc('vp', 'VP', false),
  'pur.vendorpaymentplan1': payDoc('vpp', 'VPP', false),
  'pur.vendorpaymentplan2': payDoc('vpp', 'VPP', false),
  'pur.listvendorpaymentplandetail': payDoc('vpp', 'VPP', false),
  'pur.vpp': { datasets: { DS1: allocConfig('vpp', 'VPP', false) } },
  'pur.daftarvp': {
    datasets: {
      DS1: allocConfig('vp', 'VP', true),
      DS2: allocConfig('vp', 'VP', true),
      DS3: instrumentConfig('vp', 'VP'),
    },
  },
  'pur.pembayaranhutang': { datasets: { DS1: allocConfig('vp', 'VP', false) } },
  'pur.pelunasanharian': { datasets: { DS1: allocConfig('vp', 'VP', false) } },

  /* Payment lists */
};
