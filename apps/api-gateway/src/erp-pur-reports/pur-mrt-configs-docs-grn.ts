/**
 * GRN document configs (Wave G5): Good Receipt Note prints (Detail1
 * + batch/serial variants, Detail2/3/4, Dili) and the GRN list family.
 *
 * Batch variants carry DS2 = the same lines with the lot number
 * (pur_goods_receipt_lines.lot_number — the ERP stores the batch on
 * the receipt line itself). Serial variants carry DS2 = inv_serials
 * rows whose origin is this GRN (one row per serial number).
 */

import {
  docFilters,
  docJoins,
  hdrCols,
  lineCols,
  partnerExtraCols,
  type PurDatasetConfig,
  type PurReportConfig,
} from './pur-mrt-configs';

const GRN_JOINS = `
  JOIN md_items i ON i.id = l.item_id
  LEFT JOIN md_units u ON u.id = l.unit_id
  ${docJoins('t.supplier_id')}
  LEFT JOIN md_warehouses w ON w.id = t.warehouse_id
  LEFT JOIN md_locations loc ON loc.id = t.location_id
  LEFT JOIN pur_orders o ON o.id = t.order_id
`;

const GRN_SELECT: Record<string, string> = {
  ...hdrCols('grn'),
  ...lineCols(),
  ...partnerExtraCols(),
  grngudang: 'w.name',
  gudang: 'w.name',
  ponotransaksi: 'o.doc_number',
  blgkodelokasi: 'loc.code',
  idbarang: 'i.id',
  bid: 'i.id',
  subtotal: '(l.quantity * l.unit_price - COALESCE(l.discount_amount, 0))',
  'COUNT_x0028__x002A__x0029_':
    '(SELECT COUNT(*) FROM pur_goods_receipt_lines x WHERE x.goods_receipt_id = t.id)',
};

const FILTERS = () =>
  docFilters('t.doc_date', {
    warehouse: { id: 'w.id', code: 'w.code', name: 'w.name' },
  });

function grnDs1(orderByItem = false): PurDatasetConfig {
  return {
    from: `pur_goods_receipts t JOIN pur_goods_receipt_lines l ON l.goods_receipt_id = t.id ${GRN_JOINS}`,
    select: GRN_SELECT,
    deletedAlias: 't',
    paramFilters: FILTERS(),
    orderBy: orderByItem ? 'i.code, t.doc_date, t.doc_number' : 't.doc_date, t.doc_number, l.line_no',
  };
}

/** DS2 batch: receipt lines with their lot numbers. */
function grnDs2Batch(): PurDatasetConfig {
  return {
    from: `pur_goods_receipts t JOIN pur_goods_receipt_lines l ON l.goods_receipt_id = t.id ${GRN_JOINS}`,
    select: { ...GRN_SELECT, nbtkode: 'l.lot_number', nbtidbarang: 'l.item_id' },
    deletedAlias: 't',
    paramFilters: FILTERS(),
    orderBy: 't.doc_date, t.doc_number, l.line_no',
  };
}

/** DS2 serial: one row per serial number received on this GRN. */
function grnDs2Serial(): PurDatasetConfig {
  return {
    from: `
      pur_goods_receipts t
      JOIN inv_serials s ON s.origin_goods_receipt_id = t.id AND s.deleted_at IS NULL
      JOIN md_items i ON i.id = s.item_id
      LEFT JOIN md_units u ON u.id = i.base_unit_id
      ${docJoins('t.supplier_id')}
      LEFT JOIN md_warehouses w ON w.id = t.warehouse_id
      LEFT JOIN md_locations loc ON loc.id = t.location_id
      LEFT JOIN pur_orders o ON o.id = t.order_id
    `,
    select: {
      ...GRN_SELECT,
      nstkode: 's.serial_number',
      nstidbarang: 's.item_id',
      // No receipt-line alias in this FROM — one row per serial.
      jml: '1',
      satuan: 'u.name',
      diskon: 'NULL',
      jmldiskon: '0',
      harga: 'NULL',
      total: 'NULL',
      subtotal: 'NULL',
      catatan: 'NULL',
      urutan: 'NULL',
    },
    deletedAlias: 't',
    paramFilters: FILTERS(),
    orderBy: 't.doc_date, t.doc_number, s.serial_number',
  };
}

const grnDoc = (ds2?: PurDatasetConfig): PurReportConfig => ({
  datasets: ds2 ? { DS1: grnDs1(), DS2: ds2 } : { DS1: grnDs1() },
});

export const PUR_GRN_CONFIGS: Record<string, PurReportConfig> = {
  'pur.goodreceiptnotedetail1': grnDoc(),
  'pur.goodreceiptnotedetail1batch': grnDoc(grnDs2Batch()),
  'pur.goodreceiptnotedetail1batch2': grnDoc(grnDs2Batch()),
  'pur.goodreceiptnotedetail1batch3': grnDoc(grnDs2Batch()),
  'pur.goodreceiptnotedetail1batch4': grnDoc(grnDs2Batch()),
  'pur.goodreceiptnotedetail1serial': grnDoc(grnDs2Serial()),
  'pur.goodreceiptnotedetail1serial2': grnDoc(grnDs2Serial()),
  'pur.goodreceiptnotedetail1serial3': grnDoc(grnDs2Serial()),
  'pur.goodreceiptnotedetail1serial4': grnDoc(grnDs2Serial()),
  'pur.goodreceiptnotedetail1serialud': grnDoc(grnDs2Serial()),
  'pur.goodreceiptnotedetail2': grnDoc(),
  'pur.goodreceiptnotedetail3': grnDoc(),
  'pur.goodreceiptnotedetail4': grnDoc(),
  'pur.grndili1': grnDoc(),

  /* GRN lists */
  'pur.listgoodreceiptnote': grnDoc(),
  'pur.listgoodreceiptnote2': grnDoc(),
  'pur.listgoodreceiptnoteproduct': { datasets: { DS1: grnDs1(true) } },
};
