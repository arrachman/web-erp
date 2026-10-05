/**
 * PO document + list configs (Wave G5). One base config serves every
 * Purchase Order print variant (Detail1/2/3 + per-customer letterhead
 * variants, FORM PO, SPK forms, Order Pembelian, FONT) and the PO list
 * family — the templates differ in layout, not in dataset semantics:
 * header (pur_orders) × lines (pur_order_lines).
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

const PO_FROM = `
  pur_orders t
  JOIN pur_order_lines l ON l.order_id = t.id
  JOIN md_items i ON i.id = l.item_id
  LEFT JOIN md_units u ON u.id = l.unit_id
  ${docJoins('t.supplier_id')}
  LEFT JOIN md_warehouses w ON w.id = t.warehouse_id
  LEFT JOIN md_locations loc ON loc.id = t.location_id
  LEFT JOIN adm_users usr ON usr.id = t.created_by_id
  LEFT JOIN adm_users upd ON upd.id = t.updated_by_id
  LEFT JOIN pur_requisitions prq ON prq.id = t.requisition_id
`;

const PO_SELECT: Record<string, string> = {
  ...hdrCols('po'),
  ...lineCols(),
  ...partnerExtraCols(),
  posupplierkontak: 'ctc.name',
  kepada: 'ctc.name',
  alamatkepada: 'addr.address_line1',
  po1alamat1: 'addr.address_line1',
  po2alamat1: 'addr.address_line2',
  notlp: 'addr.phone',
  lnama: 'loc.name',
  alamatkirim: 'loc.address_line1',
  poinputuser: 'usr.name',
  unama: 'usr.name',
  poinputtgl: 't.created_at',
  pomodifikasiuser: 'upd.name',
  pomodifikasitgl: 't.updated_at',
  prnotransaksi: 'prq.doc_number',
  potgldipenuhi: 't.fulfil_date',
  totaltransaksi: 't.grand_total',
  matauang: 'cur.code',
  pajak1: 'COALESCE(t.tax1_amount, 0)',
  gudang: 'w.name',
  sckode: 'i.legacy_code',
  pnama: 'p.name',
  bid: 'i.id',
};

function poDocConfig(orderByItem = false): PurDatasetConfig {
  return {
    from: PO_FROM,
    select: PO_SELECT,
    deletedAlias: 't',
    paramFilters: docFilters('t.doc_date', {
      warehouse: { id: 'w.id', code: 'w.code', name: 'w.name' },
    }),
    orderBy: orderByItem ? 'i.code, t.doc_date, t.doc_number' : 't.doc_date, t.doc_number, l.line_no',
    terbilang: [
      {
        column: 'terbilang',
        mode: 'first',
        amountColumn: 'pototaltransaksi',
        groupByColumn: 'ponotransaksi',
        currencyColumn: 'pomatauang',
      },
    ],
  };
}

const poDoc = (): PurReportConfig => ({ datasets: { DS1: poDocConfig() } });
const poListByItem = (): PurReportConfig => ({ datasets: { DS1: poDocConfig(true) } });

export const PUR_PO_CONFIGS: Record<string, PurReportConfig> = {
  /* -------- PO document prints -------- */
  'pur.purchaseorderdetail1': poDoc(),
  'pur.purchaseorderdetail12': poDoc(),
  'pur.purchaseorderdetail13': poDoc(),
  'pur.purchaseorderdetail1ud2': poDoc(),
  'pur.purchaseorderdetail2': poDoc(),
  'pur.purchaseorderdetail3': poDoc(),
  'pur.purchaseorderdetail3baja': poDoc(),
  'pur.purchaseorderdetail3bam': poDoc(),
  'pur.purchaseorderdetail3mahameru': poDoc(),
  'pur.purchaseorderdetail3medipro': poDoc(),
  'pur.purchaseorderdetail3putra': poDoc(),
  'pur.purchaseorderdetail3putralogo': poDoc(),
  'pur.purchaseorderdetail3wulancipta': poDoc(),
  'pur.po': poDoc(),
  'pur.podili1': poDoc(),
  'pur.pokawata': poDoc(),
  'pur.formpo': poDoc(),
  'pur.formspksubkont': poDoc(),
  'pur.formspkupah': poDoc(),
  'pur.orderpembelian': poDoc(),
  'pur.orderpembeliansadita': poDoc(),
  'pur.font': poDoc(),

  /* -------- PO lists -------- */
  'pur.listpurchaseorder': poDoc(),
  'pur.listpurchaseorder2': poDoc(),
  'pur.listpurchaseorderproduct': poListByItem(),
};
