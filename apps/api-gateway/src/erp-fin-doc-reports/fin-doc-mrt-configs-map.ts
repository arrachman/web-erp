/**
 * Giro documents, the sales-invoice document filed under m2, the two
 * tax registers, and the honest-empty document configs (Wave G2).
 */

import {
  docFilters,
  emptyConfig,
  partnerFilter,
  periodFilters,
  statusLabel,
  docNoFilter,
  type FinDatasetConfig,
  type FinReportConfig,
} from './fin-doc-mrt-configs';
import { CASHBANK_DOC_CONFIGS } from './fin-doc-mrt-configs-cashbank';
import { JOURNAL_DOC_CONFIGS } from './fin-doc-mrt-configs-journal';
import { FIN_G3_STATEMENT_CONFIGS } from './fin-doc-mrt-configs-statements';
import { FIN_G3_STATEMENT2_CONFIGS } from './fin-doc-mrt-configs-statements2';
import { FIN_G3_GL_CONFIGS } from './fin-doc-mrt-configs-gl';
import { FIN_G3_DAILY_CONFIGS } from './fin-doc-mrt-configs-daily';
import { FIN_G3_ARAP_CONFIGS } from './fin-doc-mrt-configs-arap';
import { FIN_G3_ARAP2_CONFIGS } from './fin-doc-mrt-configs-arap2';
import { FIN_G3_ARAP3_CONFIGS } from './fin-doc-mrt-configs-arap3';
import { FIN_G3_LIST_CONFIGS } from './fin-doc-mrt-configs-lists';

/** Wave G3 files key datasets directly; adapt to the FinReportConfig shape. */
const wrapG3 = (
  m: Record<string, Record<string, FinDatasetConfig>>,
): Record<string, FinReportConfig> =>
  Object.fromEntries(
    Object.entries(m).map(([key, datasets]) => [key, { datasets }]),
  );

export const FIN_G3_MRT_CONFIGS: Record<string, FinReportConfig> = {
  ...wrapG3(FIN_G3_STATEMENT_CONFIGS),
  ...wrapG3(FIN_G3_STATEMENT2_CONFIGS),
  ...wrapG3(FIN_G3_GL_CONFIGS),
  ...wrapG3(FIN_G3_DAILY_CONFIGS),
  ...wrapG3(FIN_G3_ARAP_CONFIGS),
  ...wrapG3(FIN_G3_ARAP2_CONFIGS),
  ...wrapG3(FIN_G3_ARAP3_CONFIGS),
  ...wrapG3(FIN_G3_LIST_CONFIGS),
};

/* ---------------- giro register documents (RG / SG) ---------------- */

function giroDocConfig(prefix: 'rg' | 'sg', giroType: string): FinDatasetConfig {
  return {
    from: `
      fin_giro_entries t
      JOIN fin_giros g ON g.giro_entry_id = t.id
      LEFT JOIN md_accounts a ON a.id = g.giro_account_id
      JOIN md_currencies cur ON cur.id = t.currency_id
      LEFT JOIN md_partners p ON p.id = t.partner_id
    `,
    select: {
      jumlah: 'g.amount',
      [`${prefix}catatan`]: 't.notes',
      [`${prefix}notransaksi`]: 't.doc_number',
      [`${prefix}tgl`]: 't.entry_date',
      nogiro: 'g.giro_number',
      bnama: 'g.bank_name',
      bank: 'g.bank_name',
      tgljatuhtempo: 'g.due_date',
      cnomor: 'a.code',
      cnama: 'a.name',
      noacbank: 'g.bank_account_no',
      rekbank: 'a.code',
      matauang: 'cur.code',
      [`status${prefix}`]: statusLabel('t.status::text'),
    },
    where: `t.kind = 'REGISTER' AND t.type = '${giroType}' AND g.deleted_at IS NULL`,
    orderBy: 't.entry_date, t.doc_number, g.line_no',
    deletedAlias: 't',
    paramFilters: docFilters('t.entry_date'),
  };
}

/* ---------------- sales invoice document (m5_si filed in m2) ---------------- */

const SI_STATUS = statusLabel('t.status::text');

const SI_FROM = `
  sls_invoices t
  JOIN sls_invoice_lines l ON l.invoice_id = t.id
  JOIN md_items i ON i.id = l.item_id
  JOIN md_partners cust ON cust.id = t.customer_id
  LEFT JOIN md_divisions dv ON dv.id = t.sales_dept_id
  LEFT JOIN md_payment_terms pt ON pt.id = t.payment_term_id
  JOIN md_currencies cur ON cur.id = t.currency_id
  LEFT JOIN md_units u ON u.id = l.unit_id
  LEFT JOIN LATERAL (
    SELECT ad.address_line1 FROM md_partner_addresses ad
    WHERE ad.partner_id = t.customer_id AND ad.deleted_at IS NULL
    ORDER BY ad.is_default DESC, ad.id LIMIT 1
  ) addr ON true
`;

const SI_HEADER_SELECT: Record<string, string> = {
  siid: 't.id',
  sitgl: 't.doc_date',
  sinotransaksi: 't.doc_number',
  sicustomer: 'cust.name',
  sibagianpenjualan: 'dv.name',
  sistatus: SI_STATUS,
  siuraian: 't.description',
  simatauang: 'cur.code',
  sikurs: 't.exchange_rate',
  sidiskonpersen: 't.discount_percent',
  sijmldiskon: 't.discount_amount',
  sibiayalain: 't.other_cost_amount',
  sitotalpajak1detail: 't.tax1_amount',
  sijmluangmuka: 't.advance_amount',
  sitermin: 'pt.name',
  sicustomdbl2: `(t.metadata->>'sicustomdbl2')::numeric`,
  sicatatan: 't.notes',
};

const SI_FILTERS = {
  ...docNoFilter('t.doc_number'),
  ...periodFilters('t.doc_date'),
  ...partnerFilter('t.customer_id', 'cust.code', 'cust.name'),
};

function salesInvoiceConfig(): FinReportConfig {
  return {
    datasets: {
      // DS1 aggregates lines per item+price (legacy GROUP BY).
      DS1: {
        from: SI_FROM,
        select: {
          'COUNT_x0028__x002A__x0029_': 'COUNT(*)',
          idbarang: 'l.item_id',
          ...SI_HEADER_SELECT,
          bkode: 'i.code',
          namabarang: 'i.name',
          catatan: 'l.notes',
          jml: 'SUM(l.quantity)',
          satuan: 'u.name',
          harga: 'l.unit_price',
          diskon: 'SUM(l.discount_amount)',
          total: 'SUM(l.quantity * l.unit_price) - COALESCE(SUM(l.discount_amount), 0)',
          si1alamat1: 'addr.address_line1',
        },
        where: 't.is_opening_balance = false',
        groupBy:
          't.id, l.item_id, i.code, i.name, l.notes, u.name, l.unit_price, ' +
          `cust.name, dv.name, pt.name, addr.address_line1, cur.code, ${SI_STATUS}`,
        orderBy: 't.doc_date, t.id, t.doc_number, l.item_id',
        deletedAlias: 't',
        paramFilters: SI_FILTERS,
      },
      // DS2: raw lines + batch/lot of the linked delivery line, when
      // the invoice line points at one (source_line_id); else NULL.
      DS2: {
        from: `${SI_FROM}
          LEFT JOIN sls_delivery_order_lines dl ON dl.id = l.source_line_id
          LEFT JOIN inv_lots lot ON lot.id = dl.lot_id
          LEFT JOIN md_units du ON du.id = dl.unit_id
        `,
        select: {
          ...SI_HEADER_SELECT,
          bkode: 'i.code',
          namabarang: 'i.name',
          catatan: 'l.notes',
          jml: 'l.quantity',
          satuan: 'u.name',
          harga: 'l.unit_price',
          diskon: 'l.discount_amount',
          total: '(l.quantity * l.unit_price) - COALESCE(l.discount_amount, 0)',
          nbtidbarang: 'l.item_id',
          nbtkode: 'lot.lot_number',
          nbtjml: 'dl.base_quantity',
          nbtsatuan: 'du.name',
        },
        where: 't.is_opening_balance = false',
        orderBy: 't.doc_date, t.id, l.line_no',
        deletedAlias: 't',
        paramFilters: SI_FILTERS,
      },
    },
  };
}

/* ---------------- tax registers (pajak keluaran / masukan) ---------------- */

function pajakKluaranConfig(): FinReportConfig {
  return {
    datasets: {
      DS1: {
        from: `
          sls_invoice_lines l
          JOIN sls_invoices t ON t.id = l.invoice_id
          LEFT JOIN md_taxes tx ON tx.id = l.tax1_id
          LEFT JOIN md_divisions dv ON dv.id = t.sales_dept_id
          LEFT JOIN md_partners cust ON cust.id = t.customer_id
        `,
        select: {
          tkode: 'tx.code',
          tnama: 'tx.name',
          sitgl: 't.doc_date',
          siuraian: 't.description',
          sinotransaksi: 't.doc_number',
          sinofakturpajak: 't.tax_invoice_no',
          // Legacy joins the sales-section contact (m1_contact via
          // sibagianpenjualan); ERP equivalent is the sales division.
          knama: 'dv.name',
          sikurs: 't.exchange_rate',
          sitotalpajak1detail: 'SUM(l.tax1_amount)',
        },
        where: 'l.tax1_id IS NOT NULL AND t.is_opening_balance = false AND t.deleted_at IS NULL',
        groupBy: 'tx.code, tx.name, t.id, dv.name',
        orderBy: 'tx.code, t.doc_date, t.doc_number',
        paramFilters: {
          ...periodFilters('t.doc_date'),
          ...partnerFilter('t.customer_id', 'cust.code', 'cust.name'),
        },
      },
    },
  };
}

function pajakMasukanConfig(): FinReportConfig {
  return {
    datasets: {
      DS1: {
        from: `
          pur_invoice_lines l
          JOIN pur_invoices t ON t.id = l.invoice_id
          JOIN md_taxes tx ON tx.id = l.tax1_id
          LEFT JOIN md_partners sup ON sup.id = t.supplier_id
        `,
        select: {
          tkode: 'tx.code',
          tnama: 'tx.name',
          ritgl: 't.doc_date',
          riuraian: 't.description',
          rinotransaksi: 't.doc_number',
          rinofakturpajak: 't.tax_invoice_no',
          knama: 'sup.name',
          rikurs: 't.exchange_rate',
          // Legacy takes the header tax total per invoice.
          ritotalpajak1detail: 't.tax1_amount',
        },
        where: 't.deleted_at IS NULL',
        groupBy: 'tx.code, tx.name, t.id, sup.name',
        orderBy: 'tx.code, t.doc_date, t.doc_number',
        paramFilters: {
          ...periodFilters('t.doc_date'),
          ...partnerFilter('t.supplier_id', 'sup.code', 'sup.name'),
        },
      },
    },
  };
}

/* ---------------- merged G2 document map ---------------- */

const GIRO_CANCEL_NOTE =
  'Dokumen pembatalan giro legacy (m2_rgc/m2_sgc) belum ada padanannya di ERP: ' +
  'pembatalan adalah status per giro di fin_giros, bukan dokumen tersendiri.';

const ANGGARAN_NOTE =
  'Dokumen anggaran legacy (m2_bd) belum ada padanannya di ERP: yang tersedia ' +
  'hanya agregat realisasi per periode di fin_budget_realizations, bukan dokumen anggaran.';

export const FIN_DOC_MRT_CONFIGS: Record<string, FinReportConfig> = {
  ...CASHBANK_DOC_CONFIGS,
  ...JOURNAL_DOC_CONFIGS,
  ...FIN_G3_MRT_CONFIGS,
  'fin.receivegirodetail1': { datasets: { DS1: giroDocConfig('rg', 'INCOMING') } },
  'fin.receivegirodetail2': { datasets: { DS1: giroDocConfig('rg', 'INCOMING') } },
  'fin.spendgirodetail1': { datasets: { DS1: giroDocConfig('sg', 'OUTGOING') } },
  'fin.spendgirodetail2': { datasets: { DS1: giroDocConfig('sg', 'OUTGOING') } },
  'fin.receivegirocanceldetail1': { datasets: { DS1: emptyConfig(GIRO_CANCEL_NOTE) } },
  'fin.receivegirocanceldetail2': { datasets: { DS1: emptyConfig(GIRO_CANCEL_NOTE) } },
  'fin.spendgirocanceldetail1': { datasets: { DS1: emptyConfig(GIRO_CANCEL_NOTE) } },
  'fin.spendgirocanceldetail2': { datasets: { DS1: emptyConfig(GIRO_CANCEL_NOTE) } },
  'fin.anggarandetail': { datasets: { DS1: emptyConfig(ANGGARAN_NOTE) } },
  'fin.anggarandetail2': { datasets: { DS1: emptyConfig(ANGGARAN_NOTE) } },
  'fin.salesinvoicedetail1newbatch': salesInvoiceConfig(),
  'fin.pajakkluaran': pajakKluaranConfig(),
  'fin.pajakmasukan': pajakMasukanConfig(),
};
