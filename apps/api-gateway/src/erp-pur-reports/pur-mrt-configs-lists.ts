/**
 * Pembelian (purchase) report configs (Wave G5): header-level
 * purchase lists (Pembelian / Pembelian Payment), the per-supplier /
 * per-product / per-category line lists, the per-item×category
 * aggregate, and the per-project purchase lists. All read Receive
 * Invoice data (pur_invoices × pur_invoice_lines) per their legacy
 * template bindings; line-level variants reuse the RI list config.
 */

import {
  docFilters,
  lineTotal,
  type PurDatasetConfig,
  type PurReportConfig,
} from './pur-mrt-configs';
import { PUR_RI_CONFIGS } from './pur-mrt-configs-docs-ri';

const riListDs1 = PUR_RI_CONFIGS['pur.listreceiveinvoice'].datasets.DS1;
const riHeaderDs1 = PUR_RI_CONFIGS['pur.ri'].datasets.DS1;

/** Per item × item-category aggregate over RI lines. */
function perProdukPerKategoriConfig(): PurDatasetConfig {
  return {
    from: `
      pur_invoice_lines l
      JOIN pur_invoices t ON t.id = l.invoice_id
      JOIN md_items i ON i.id = l.item_id
      LEFT JOIN md_units u ON u.id = i.base_unit_id
      LEFT JOIN md_item_categories ic ON ic.id = i.category_id
    `,
    select: {
      kodebarang: 'i.code',
      namabarang: 'i.name',
      idbarang: 'i.id',
      kategori: 'ic.code',
      kategorinama: 'ic.name',
      ritgl: 'MAX(t.doc_date)',
      satuan: 'u.name',
      satuanbarang: 'u.name',
      jml: 'SUM(l.quantity)',
      jmlbarang: 'SUM(l.quantity)',
      jmldiskon: 'SUM(COALESCE(l.discount_amount, 0))',
      jmlpajak1: 'SUM(COALESCE(l.tax1_amount, 0))',
      jmlpajak2: 'SUM(COALESCE(l.tax2_amount, 0))',
      harga: 'AVG(l.unit_price)',
      subtotal: `SUM(${lineTotal('l')})`,
      total: `SUM(${lineTotal('l')})`,
    },
    deletedAlias: 't',
    paramFilters: docFilters('t.doc_date', { partner: false }),
    groupBy: 'i.id, i.code, i.name, ic.code, ic.name, u.name',
    orderBy: 'ic.name, i.code',
  };
}

/** RI lines enriched with their project + originating PO line. */
function perProyekConfig(): PurDatasetConfig {
  return {
    from: `
      pur_invoice_lines l
      JOIN pur_invoices t ON t.id = l.invoice_id
      JOIN md_items i ON i.id = l.item_id
      LEFT JOIN md_units u ON u.id = l.unit_id
      LEFT JOIN md_projects prj ON prj.id = l.project_id
      LEFT JOIN pur_order_lines pol ON pol.id = l.order_line_id
      LEFT JOIN pur_orders po ON po.id = pol.order_id
      LEFT JOIN md_units pu ON pu.id = pol.unit_id
      LEFT JOIN md_partners p ON p.id = t.supplier_id
      LEFT JOIN md_currencies cur ON cur.id = t.currency_id
    `,
    select: {
      bkode: 'i.code',
      namabarang: 'i.name',
      idbarang: 'i.id',
      jml: 'l.quantity',
      satuan: 'u.name',
      harga: 'l.unit_price',
      total: lineTotal('l'),
      jmldiskon: 'COALESCE(l.discount_amount, 0)',
      jmlpajak1: 'l.tax1_amount',
      pajak1: 'l.tax1_amount',
      rinotransaksi: 't.doc_number',
      ritgl: 't.doc_date',
      risupplier: 'p.name',
      kkode: 'p.code',
      knama: 'p.name',
      proyek: 'prj.name',
      pnama: 'prj.name',
      ponotransaksi: 'po.doc_number',
      jmlpo: 'pol.quantity',
      satuanpo: 'pu.name',
      rihargatermasukpajak: `CASE t.price_mode::text WHEN 'TAX_INCLUSIVE' THEN 1 ELSE 0 END`,
      rihargatermasukpajaknama: `CASE t.price_mode::text WHEN 'TAX_INCLUSIVE' THEN 'Sudah Termasuk Pajak' ELSE 'Belum Termasuk Pajak' END`,
    },
    deletedAlias: 't',
    paramFilters: docFilters('t.doc_date'),
    orderBy: 'prj.name, t.doc_date, t.doc_number, l.line_no',
  };
}

export const PUR_LIST_CONFIGS: Record<string, PurReportConfig> = {
  /* Header-level purchase lists */
  'pur.pembelian': { datasets: { DS1: riHeaderDs1 } },
  'pur.pembelianpayment': { datasets: { DS1: riHeaderDs1 } },

  /* Line-level purchase lists (RI lines; grouping is presentational) */
  'pur.pembelianpersupplier': { datasets: { DS1: riListDs1 } },
  'pur.pembelianperproduk': { datasets: { DS1: riListDs1 } },
  'pur.pembelianperkategoribarang': { datasets: { DS1: riListDs1 } },

  /* Aggregates + project lists */
  'pur.pembelianperprodukperkategoribarang': {
    datasets: { DS1: perProdukPerKategoriConfig() },
  },
  'pur.pembelianperproyek': { datasets: { DS1: perProyekConfig() } },
  'pur.pembelianperproyekperproduk': { datasets: { DS1: perProyekConfig() } },
};
