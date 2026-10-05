/**
 * Receive Invoice + purchase-return configs (Wave G5).
 *
 * RI prints read pur_invoices × pur_invoice_lines. Batch DS2 resolves
 * the lot through the source GRN line (legacy nbt* columns). Returns
 * read pur_returns split by return_type: DNR (Delivery Note Return)
 * = 'DEBIT_NOTE', PRT = 'RETURN_TO_VENDOR' (the ERP's own numbering
 * convention). kwitansi1 is an RI receipt (legacy aliases RI as SI);
 * saldoawalhutang lists opening-balance RIs with their settled part;
 * laporanppn lists RIs carrying a tax invoice number (never invented).
 */

import {
  docFilters,
  docJoins,
  hdrCols,
  lineCols,
  partnerExtraCols,
  type PurDatasetConfig,
  type PurReportConfig,
  type PurTerbilangSpec,
} from './pur-mrt-configs';

/* -------------------------------- RI -------------------------------- */

const RI_FROM = `
  pur_invoices t
  JOIN pur_invoice_lines l ON l.invoice_id = t.id
  JOIN md_items i ON i.id = l.item_id
  LEFT JOIN md_units u ON u.id = l.unit_id
  LEFT JOIN md_item_categories ic ON ic.id = i.category_id
  LEFT JOIN md_item_types kd ON kd.id = i.kind_id
  ${docJoins('t.supplier_id')}
  LEFT JOIN md_warehouses w ON w.id = l.warehouse_id
  LEFT JOIN md_locations loc ON loc.id = t.location_id
  LEFT JOIN pur_goods_receipts g ON g.id = t.goods_receipt_id
`;

const RI_SELECT: Record<string, string> = {
  ...hdrCols('ri'),
  ...lineCols(),
  ...partnerExtraCols(),
  rigudang: 'w.name',
  gudang: 'w.name',
  gudanggrn: 'w.name',
  grnnotransaksi: 'g.doc_number',
  ponotransaksi: '(SELECT o.doc_number FROM pur_orders o WHERE o.id = g.order_id)',
  supplierkontakname: 'ctc.name',
  idbarang: 'i.id',
  bid: 'i.id',
  bnama: 'i.name',
  icnama: 'ic.name',
  bkategori: 'ic.name',
  grup: 'kd.name',
  // kwsg columns: bruto = jml × harga before discount; harganet = net unit price
  bruto: '(l.quantity * l.unit_price)',
  harganet: '((l.quantity * l.unit_price - COALESCE(l.discount_amount, 0)) / NULLIF(l.quantity, 0))',
  jmlpajak1: 'l.tax1_amount',
  jmlpajak2: 'l.tax2_amount',
  pajak1: 'l.tax1_amount',
  subtotal: '(l.quantity * l.unit_price - COALESCE(l.discount_amount, 0))',
};

const RI_TERBILANG: PurTerbilangSpec[] = [
  {
    column: 'terbilang',
    mode: 'first',
    amountColumn: 'ritotaltransaksi',
    groupByColumn: 'rinotransaksi',
    currencyColumn: 'rimatauang',
  },
];

function riFilters() {
  return docFilters('t.doc_date', {
    warehouse: { id: 'w.id', code: 'w.code', name: 'w.name' },
  });
}

function riDs1(orderByItem = false): PurDatasetConfig {
  return {
    from: RI_FROM,
    select: RI_SELECT,
    deletedAlias: 't',
    paramFilters: riFilters(),
    orderBy: orderByItem ? 'i.code, t.doc_date, t.doc_number' : 't.doc_date, t.doc_number, l.line_no',
    terbilang: RI_TERBILANG,
  };
}

function riDs2Batch(): PurDatasetConfig {
  return {
    from: `${RI_FROM} LEFT JOIN pur_goods_receipt_lines gl ON gl.id = l.goods_receipt_line_id`,
    select: {
      ...RI_SELECT,
      nbtkode: 'gl.lot_number',
      nbtidbarang: 'l.item_id',
      nbtjml: 'l.quantity',
      nbtsatuan: 'u.name',
    },
    deletedAlias: 't',
    paramFilters: riFilters(),
    orderBy: 't.doc_date, t.doc_number, l.line_no',
    terbilang: RI_TERBILANG,
  };
}

const riDoc = (ds2?: PurDatasetConfig): PurReportConfig => ({
  datasets: ds2 ? { DS1: riDs1(), DS2: ds2 } : { DS1: riDs1() },
});

/** RI header-only rows (lists without line detail, faktur list, PIB summary). */
function riHeaderConfig(where?: string): PurDatasetConfig {
  return {
    from: `pur_invoices t ${docJoins('t.supplier_id')}`,
    select: {
      ...hdrCols('ri'),
      ...partnerExtraCols(),
      kid: 't.supplier_id',
      rinofakturpajak: 't.tax_invoice_no',
      risupplierkode: 'p.code',
      risuppliernama: 'p.name',
      rigudangnama:
        '(SELECT w.name FROM pur_invoice_lines x JOIN md_warehouses w ON w.id = x.warehouse_id WHERE x.invoice_id = t.id ORDER BY x.line_no LIMIT 1)',
      silokasi:
        '(SELECT w.name FROM pur_invoice_lines x JOIN md_warehouses w ON w.id = x.warehouse_id WHERE x.invoice_id = t.id ORDER BY x.line_no LIMIT 1)',
      rigudang:
        '(SELECT w.name FROM pur_invoice_lines x JOIN md_warehouses w ON w.id = x.warehouse_id WHERE x.invoice_id = t.id ORDER BY x.line_no LIMIT 1)',
      risupplierkontak: 'ctc.name',
      dppamount: '(t.grand_total - COALESCE(t.tax1_amount, 0) - COALESCE(t.tax2_amount, 0))',
      pembelian: 't.grand_total',
      jml: '(SELECT COUNT(*) FROM pur_invoice_lines x WHERE x.invoice_id = t.id)',
    },
    ...(where ? { where } : {}),
    deletedAlias: 't',
    paramFilters: docFilters('t.doc_date'),
    orderBy: 't.doc_date, t.doc_number',
  };
}

/** kwsg DS2: per-item price/margin reference (PI = price information). */
function kwsgMarginConfig(): PurDatasetConfig {
  return {
    from: `
      pur_invoice_lines l
      JOIN pur_invoices t ON t.id = l.invoice_id
      JOIN md_items i ON i.id = l.item_id
      LEFT JOIN md_item_categories ic ON ic.id = i.category_id
    `,
    select: {
      riid: 'MAX(t.id)',
      idbarang: 'i.id',
      piidbarang: 'i.id',
      bkode: 'i.code',
      bnama: 'i.name',
      pikategori: 'ic.name',
      pihargajual1: 'i.sale_price',
      bhargabeli: 'i.purchase_price',
      margin: '(i.sale_price - i.purchase_price)',
    },
    deletedAlias: 't',
    paramFilters: docFilters('t.doc_date', { partner: false }),
    groupBy: 'i.id, i.code, i.name, ic.name, i.sale_price, i.purchase_price',
    orderBy: 'i.code',
  };
}

/* ------------------------- kwitansi / saldo / ppn ------------------------- */

function kwitansiConfig(): PurDatasetConfig {
  return {
    from: `pur_invoices t ${docJoins('t.supplier_id')}`,
    select: {
      sicustomer: 'p.name',
      siuraian: 't.description',
      sicatatan: 't.notes',
      sinotransaksi: 't.doc_number',
      sitotaltransaksi: 't.grand_total',
    },
    deletedAlias: 't',
    paramFilters: docFilters('t.doc_date'),
    orderBy: 't.doc_date, t.doc_number',
    terbilang: [
      {
        column: 'terbilang',
        mode: 'first',
        amountColumn: 'sitotaltransaksi',
        groupByColumn: 'sinotransaksi',
      },
    ],
  };
}

function saldoAwalConfig(): PurDatasetConfig {
  return {
    from: `pur_invoices t ${docJoins('t.supplier_id')}`,
    select: {
      notransaksi: 't.doc_number',
      tgl: 't.doc_date',
      kodekontak: 'p.code',
      namakontak: 'p.name',
      uraian: 't.description',
      total: 't.grand_total',
      totaltransaksi: 't.grand_total',
      diskonnominal: 'COALESCE(t.discount_amount, 0)',
      diskonpersen: 't.discount_percent',
      biayalainnominal: 'COALESCE(t.other_cost_amount, 0)',
      biayalainpersen: 't.other_cost_percent',
      pajak1: 'COALESCE(t.tax1_amount, 0)',
      pajak2: 'COALESCE(t.tax2_amount, 0)',
      saldoawal:
        't.grand_total - (SELECT COALESCE(SUM(a.amount), 0) FROM fin_settlement_allocations a WHERE a.invoice_ref = t.id::text)',
    },
    where: 't.is_opening_balance',
    deletedAlias: 't',
    paramFilters: docFilters('t.doc_date'),
    orderBy: 't.doc_date, t.doc_number',
  };
}

export const PUR_RI_CONFIGS: Record<string, PurReportConfig> = {
  /* RI documents */
  'pur.receiveinvoicedetail1': riDoc(),
  'pur.receiveinvoicedetail1batch1': riDoc(riDs2Batch()),
  'pur.receiveinvoicedetail1batch2': riDoc(riDs2Batch()),
  'pur.receiveinvoicedetailbatch3': riDoc(riDs2Batch()),
  'pur.receiveinvoicedetailbatch4': riDoc(riDs2Batch()),
  'pur.receiveinvoicedetail1serial': riDoc(),
  // serial3's DS2 is batch-shaped in the converted template (nbtkode).
  'pur.receiveinvoicedetailserial3': riDoc(riDs2Batch()),
  'pur.receiveinvoicedetail2': riDoc(),
  'pur.receiveinvoicedetail3': riDoc(),
  'pur.ridili1': riDoc(),
  'pur.buktibarangkeluarmakmur': riDoc(),
  'pur.rikawata': riDoc(),
  'pur.risadita': riDoc(),
  'pur.kwitansi1': { datasets: { DS1: kwitansiConfig() } },
  'pur.saldoawalhutang': { datasets: { DS1: saldoAwalConfig() } },
  'pur.saldoawalhutang2': { datasets: { DS1: saldoAwalConfig() } },
  'pur.ri': { datasets: { DS1: riHeaderConfig() } },
  'pur.ripibsummary': { datasets: { DS1: riHeaderConfig() } },
  'pur.laporanppn': {
    datasets: { DS1: riHeaderConfig('t.tax_invoice_no IS NOT NULL') },
  },

  /* RI lists */
  'pur.listreceiveinvoice': riDoc(),
  'pur.listreceiveinvoice2': riDoc(),
  'pur.listreceiveinvoice2pay': riDoc(),
  'pur.listreceiveinvoiceproduct': { datasets: { DS1: riDs1(true) } },
  'pur.listreceiveinvoice1': { datasets: { DS1: riHeaderConfig() } },
  'pur.listreceiveinvoicesa': { datasets: { DS1: riHeaderConfig() } },
  'pur.listreceiveinvoice1kwsg': riDoc(),
  'pur.listreceiveinvoicekwsg': riDoc(),
  'pur.listreceiveinvoicekwsg2': riDoc(),
  'pur.listreceiveinvoicekwsgperjenis': riDoc(),
  'pur.listreceiveinvoicekwsgperkategori': riDoc(),
  'pur.listreceiveinvoice2kwsg': {
    datasets: { DS1: riDs1(), DS2: kwsgMarginConfig() },
  },
  'pur.listreceiveinvoice3kwsg': {
    datasets: { DS1: riDs1(), DS2: kwsgMarginConfig() },
  },

};
