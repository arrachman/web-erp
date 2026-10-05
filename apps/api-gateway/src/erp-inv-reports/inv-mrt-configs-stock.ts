/**
 * Stock position & aggregate configs (Wave G4): per item×warehouse
 * derived balances, below-minimum / negative-stock lists, booking
 * analysis, mutation recaps and inventory valuation snapshots.
 * All balances use the ERP derivation (POSTED movements signed +
 * POSTED openings); valuation uses the item's stored moving-average
 * cost (COALESCE average_cost → last_hpp → purchase_price).
 */

import {
  AVG_COST,
  EVENTS_FROM,
  EVENTS_SELECT,
  balanceExpr,
  partnerFilter,
  periodFilters,
  warehouseFilter,
  type InvDatasetConfig,
  type InvReportConfig,
} from './inv-mrt-configs';

const POSITION_FROM = `
  (SELECT DISTINCT item_id, wh_id FROM (${EVENTS_SELECT}) u0) pos
  JOIN md_items i ON i.id = pos.item_id
  LEFT JOIN md_warehouses w ON w.id = pos.wh_id
  LEFT JOIN md_item_categories ic ON ic.id = i.category_id
  LEFT JOIN md_units u ON u.id = i.base_unit_id
  LEFT JOIN md_divisions dv ON dv.id = i.division_id
  LEFT JOIN md_locations lc ON lc.id = i.default_location_id
`;

const WH_FILTER = warehouseFilter('w.id', 'w.code', 'w.name');

function positionConfig(extra: Record<string, string> = {}): InvDatasetConfig {
  return {
    from: POSITION_FROM,
    select: {
      idbarang: 'i.id',
      kgudang: 'pos.wh_id',
      stok: balanceExpr('i.id', 'pos.wh_id'),
      wnama: 'w.name',
      bkode: 'i.code',
      btipe: 'i.type::text',
      bnama: 'i.name',
      bsatuan: 'u.name',
      bstokminimal: 'i.min_stock',
      bstokmaksimal: 'i.max_stock',
      bdivisi: 'dv.name',
      blgkodelokasi: 'lc.code',
      blgidlokasi: 'i.default_location_id',
      knama: 'NULL',
      ...extra,
    },
    paramFilters: { ...WH_FILTER },
    orderBy: 'w.name, i.code',
  };
}

/** Outstanding purchase-order qty per item (ordered − received, open POs). */
const OPEN_PO_QTY = `(
  SELECT COALESCE(SUM(GREATEST(pol.base_quantity - COALESCE(gr.got, 0), 0)), 0)
  FROM pur_order_lines pol
  JOIN pur_orders po ON po.id = pol.order_id
  LEFT JOIN (
    SELECT order_line_id, SUM(base_quantity) AS got
    FROM pur_goods_receipt_lines GROUP BY order_line_id
  ) gr ON gr.order_line_id = pol.id
  WHERE po.deleted_at IS NULL AND po.status NOT IN ('VOID', 'CANCELLED')
    AND pol.item_id = i.id
)`;

/** Outstanding sales-order qty per item (ordered − delivered, open SOs). */
const OPEN_SO_QTY = `(
  SELECT COALESCE(SUM(GREATEST(sol.base_quantity - COALESCE(dl.got, 0), 0)), 0)
  FROM sls_order_lines sol
  JOIN sls_orders so ON so.id = sol.order_id
  LEFT JOIN (
    SELECT dol.source_line_id, SUM(dol.base_quantity) AS got
    FROM sls_delivery_order_lines dol
    JOIN sls_delivery_orders do2 ON do2.id = dol.delivery_order_id
    WHERE do2.deleted_at IS NULL AND do2.status = 'POSTED'
    GROUP BY dol.source_line_id
  ) dl ON dl.source_line_id = sol.id
  WHERE so.deleted_at IS NULL AND so.status NOT IN ('VOID', 'CANCELLED')
    AND sol.item_id = i.id
)`;

/** Global derived balance of the current item row (alias i). */
const GLOBAL_BALANCE = `(
  SELECT COALESCE(SUM(x.qty), 0) FROM (${EVENTS_SELECT}) x WHERE x.item_id = i.id
)`;

function mutationRecapConfig(groupExtra: Record<string, string>): InvDatasetConfig {
  return {
    from: `
      ${EVENTS_FROM}
      JOIN md_items i ON i.id = ev.item_id
      LEFT JOIN md_warehouses w ON w.id = ev.wh_id
      LEFT JOIN md_item_categories ic ON ic.id = i.category_id
      LEFT JOIN md_units u ON u.id = i.base_unit_id
    `,
    select: {
      msgudangnama: 'w.name',
      mskodebarang: 'i.code',
      msnamabarang: 'i.name',
      mssatuanbarang: 'u.name',
      saldoawal: '0',
      masuk: 'COALESCE(SUM(CASE WHEN ev.qty > 0 THEN ev.qty ELSE 0 END), 0)',
      keluar: 'COALESCE(SUM(CASE WHEN ev.qty < 0 THEN -ev.qty ELSE 0 END), 0)',
      saldoakhir: 'COALESCE(SUM(ev.qty), 0)',
      bstatus: 'NULL',
      Shift: 'NULL',
      mscustomtext2: 'NULL',
      ...groupExtra,
    },
    groupBy: 'w.name, i.code, i.name, u.name, i.id, ic.name',
    orderBy: 'w.name, i.code',
    paramFilters: { ...WH_FILTER },
  };
}

export const INV_STOCK_CONFIGS: Record<string, InvReportConfig> = {
  /* -------- stock position per item × warehouse -------- */
  'inv.stokpergudang': { datasets: { DS1: positionConfig() } },
  'inv.stokperbarangpergudang': { datasets: { DS1: positionConfig() } },
  'inv.laporanpersediaanbarangpergudang': { datasets: { DS1: positionConfig() } },
  'inv.laporanpersediaanbarangpergudangretur': { datasets: { DS1: positionConfig() } },
  'inv.komisilimoplast': { datasets: { DS1: positionConfig() } },

  /* -------- mutation recaps (opening/in/out/closing per item) -------- */
  'inv.mutasistokrekap': { datasets: { DS1: mutationRecapConfig({}) } },
  'inv.mutasistokrekappershift': { datasets: { DS1: mutationRecapConfig({}) } },
  'inv.inventoryhistorispergrup': {
    datasets: {
      DS1: mutationRecapConfig({
        mskategori: 'i.category_id::text',
        mskategorinama: 'ic.name',
      }),
    },
  },

  /* -------- fin.* per category × warehouse mutation recaps -------- */
  'fin.persediaanbarangpergudang': { datasets: { DS1: categoryRecapConfig() } },
  'fin.persediaanbarangpergudangperkategori': { datasets: { DS1: categoryRecapConfig() } },
  'fin.persediaanbarangperkategori': { datasets: { DS1: categoryRecapConfig() } },
  'fin.persediaanbarangperkategoripergudang': { datasets: { DS1: categoryRecapConfig() } },

  /* -------- below minimum stock (global balance < md_items.min_stock) -------- */
  'inv.barangbawahstokminim': {
    datasets: {
      DS1: {
        from: `
          md_items i
          LEFT JOIN md_item_categories ic ON ic.id = i.category_id
          LEFT JOIN md_divisions dv ON dv.id = i.division_id
        `,
        select: {
          bkode: 'i.code',
          bnama: 'i.name',
          bstokminimal: 'i.min_stock',
          bstok: GLOBAL_BALANCE,
          variasi: `${GLOBAL_BALANCE} - COALESCE(i.min_stock, 0)`,
          bookingpo: OPEN_PO_QTY,
          bsatuan: '(SELECT u.name FROM md_units u WHERE u.id = i.base_unit_id)',
          icnama: 'ic.name',
          dnama: 'dv.name',
        },
        where: `i.deleted_at IS NULL AND i.min_stock IS NOT NULL AND ${GLOBAL_BALANCE} < i.min_stock`,
        orderBy: 'i.code',
      },
    },
  },

  /* -------- stock analysis: on-hand + vendor/customer bookings -------- */
  'inv.laporananalisastok': {
    datasets: {
      DS1: {
        from: `
          md_items i
          LEFT JOIN md_item_categories ic ON ic.id = i.category_id
          LEFT JOIN md_units u ON u.id = i.base_unit_id
        `,
        select: {
          bkode: 'i.code',
          bnama: 'i.name',
          stok: GLOBAL_BALANCE,
          bookingvendor: OPEN_PO_QTY,
          bookingcustomer: OPEN_SO_QTY,
          bstokminimal: 'i.min_stock',
          bstokmaksimal: 'i.max_stock',
          total: `${GLOBAL_BALANCE} + ${OPEN_PO_QTY} - ${OPEN_SO_QTY}`,
          bkategori: 'ic.name',
        },
        where: 'i.deleted_at IS NULL',
        orderBy: 'i.code',
      },
    },
  },

  /* -------- simple stock list: balance + open sales orders -------- */
  'inv.lapstok': {
    datasets: {
      DS1: {
        from: 'md_items i',
        select: {
          bkode: 'i.code',
          bnama: 'i.name',
          bjmlorderjual: OPEN_SO_QTY,
          bstok: GLOBAL_BALANCE,
          tstok: `${GLOBAL_BALANCE} - ${OPEN_SO_QTY}`,
        },
        where: 'i.deleted_at IS NULL',
        orderBy: 'i.code',
      },
    },
  },

  /* -------- negative-stock moments (running balance < 0) -------- */
  'inv.stokminus': {
    datasets: {
      DS1: {
        from: `
          (
            SELECT e.*, SUM(e.qty) OVER (
              PARTITION BY e.item_id, e.wh_id ORDER BY e.evt_date, e.doc_number
            ) AS runbal
            FROM (${EVENTS_SELECT}) e
          ) neg
          JOIN md_items i ON i.id = neg.item_id
          LEFT JOIN md_units u ON u.id = i.base_unit_id
        `,
        select: {
          bkode: 'i.code',
          bnama: 'i.name',
          tgl: 'neg.evt_date',
          sumber: 'neg.doc_number',
          notransaksi: 'neg.doc_number',
          stok: 'neg.runbal',
          bsatuan: 'u.name',
        },
        where: 'neg.runbal < 0',
        orderBy: 'i.code, neg.evt_date',
      },
    },
  },

  /* -------- closing qty & HPP value per item -------- */
  'inv.saldojmlsaldohpp': {
    datasets: {
      DS1: {
        from: `
          ${EVENTS_FROM}
          JOIN md_items i ON i.id = ev.item_id
        `,
        select: {
          namabarang: 'i.name',
          tgl: 'MAX(ev.evt_date)',
          saldojml: 'COALESCE(SUM(ev.qty), 0)',
          saldohpp: `COALESCE(SUM(ev.qty), 0) * ${AVG_COST}`,
        },
        groupBy: 'i.id, i.name, i.average_cost, i.last_hpp, i.purchase_price',
        orderBy: 'i.name',
      },
    },
  },

  /* -------- cover month: stock vs trailing-30d outflow -------- */
  'fin.covermonth': {
    datasets: {
      DS1: {
        from: POSITION_FROM,
        select: {
          scidbarang: 'i.id',
          sckodebarang: 'i.code',
          scnamabarang: 'i.name',
          sctipebarang: 'i.type::text',
          sckategoribarang: 'i.category_id',
          sckategoribarangnama: 'ic.name',
          scgudang: 'pos.wh_id',
          scgudangnama: 'w.name',
          scstok: balanceExpr('i.id', 'pos.wh_id'),
          scsatuan: 'i.base_unit_id',
          scsatuannama: 'u.name',
          scsatuannilai: '1',
          scjmlkeluar: `(
            SELECT COALESCE(SUM(-x.qty), 0) FROM (${EVENTS_SELECT}) x
            WHERE x.item_id = i.id AND x.wh_id = pos.wh_id AND x.qty < 0
              AND x.evt_date >= CURRENT_DATE - 30
          )`,
          scnilai: `${balanceExpr('i.id', 'pos.wh_id')} * ${AVG_COST}`,
          sccovermonth: `CASE WHEN (
              SELECT COALESCE(SUM(-x.qty), 0) FROM (${EVENTS_SELECT}) x
              WHERE x.item_id = i.id AND x.wh_id = pos.wh_id AND x.qty < 0
                AND x.evt_date >= CURRENT_DATE - 30
            ) > 0 THEN ${balanceExpr('i.id', 'pos.wh_id')} / (
              SELECT SUM(-x.qty) FROM (${EVENTS_SELECT}) x
              WHERE x.item_id = i.id AND x.wh_id = pos.wh_id AND x.qty < 0
                AND x.evt_date >= CURRENT_DATE - 30
            ) ELSE NULL END`,
          statusmoving: `CASE WHEN (
              SELECT COALESCE(SUM(-x.qty), 0) FROM (${EVENTS_SELECT}) x
              WHERE x.item_id = i.id AND x.wh_id = pos.wh_id AND x.qty < 0
                AND x.evt_date >= CURRENT_DATE - 30
            ) = 0 THEN 'Non Moving' WHEN ${balanceExpr('i.id', 'pos.wh_id')} <= (
              SELECT SUM(-x.qty) FROM (${EVENTS_SELECT}) x
              WHERE x.item_id = i.id AND x.wh_id = pos.wh_id AND x.qty < 0
                AND x.evt_date >= CURRENT_DATE - 30
            ) THEN 'Fast Moving' ELSE 'Slow Moving' END`,
        },
        paramFilters: { ...WH_FILTER },
        orderBy: 'w.name, i.code',
      },
    },
  },

  /* -------- item transaction extract (fin.* + production/sales rows) -------- */
  'fin.itemtransaction': { datasets: { DS1: itemTransactionConfig() } },
  'inv.produksidanpenjualandlmton': {
    datasets: { DS1: itemTransactionConfig(), DS2: itemTransactionConfig() },
  },
};

function categoryRecapConfig(): InvDatasetConfig {
  return {
    from: `
      ${EVENTS_FROM}
      JOIN md_items i ON i.id = ev.item_id
      LEFT JOIN md_warehouses w ON w.id = ev.wh_id
      LEFT JOIN md_item_categories ic ON ic.id = i.category_id
    `,
    select: {
      pkategori: 'i.category_id',
      pkategorinama: 'ic.name',
      pgudang: 'ev.wh_id',
      pgudangnama: 'w.name',
      psaldoawal: '0',
      pmasuk: 'COALESCE(SUM(CASE WHEN ev.qty > 0 THEN ev.qty ELSE 0 END), 0)',
      pkeluar: 'COALESCE(SUM(CASE WHEN ev.qty < 0 THEN -ev.qty ELSE 0 END), 0)',
      psaldoakhir: 'COALESCE(SUM(ev.qty), 0)',
    },
    groupBy: 'i.category_id, ic.name, ev.wh_id, w.name',
    orderBy: 'ic.name, w.name',
    paramFilters: { ...WH_FILTER },
  };
}

function itemTransactionConfig(): InvDatasetConfig {
  return {
    from: `
      inv_stock_movement_lines l
      JOIN inv_stock_movements m ON m.id = l.stock_movement_id
      JOIN md_items i ON i.id = l.item_id
      LEFT JOIN md_units u ON u.id = l.base_unit_id
      LEFT JOIN md_warehouses w
        ON w.id = COALESCE(l.destination_warehouse_id, l.source_warehouse_id)
      LEFT JOIN md_partners p ON p.id = m.requested_partner_id
    `,
    select: {
      itnotransaksi: 'm.doc_number',
      notransaksi: 'm.doc_number',
      ittgl: 'm.movement_date',
      tgl: 'm.movement_date',
      namabarang: 'i.name',
      bkode: 'i.code',
      bnama: 'i.name',
      idbarang: 'i.id',
      jmlbarang: 'l.base_quantity',
      jmlmasuk: 'CASE WHEN m.movement_type IN (\'TRANSFER_RECEIPT\', \'RETURN\') THEN l.base_quantity ELSE 0 END',
      jmlkeluar: 'CASE WHEN m.movement_type IN (\'ISSUE\', \'TRANSFER\') THEN l.base_quantity ELSE 0 END',
      itkontakkode: 'p.code',
      itsatuanbarang: 'u.name',
      satuan: 'u.name',
      knama: 'p.name',
      wnama: 'w.name',
      itgudang: 'COALESCE(l.destination_warehouse_id, l.source_warehouse_id)',
      gudang: 'COALESCE(l.destination_warehouse_id, l.source_warehouse_id)',
      itkodebarang: 'i.code',
      harga: 'COALESCE(l.unit_cost, l.sale_price)',
      diskon: 'NULL',
      catatan: 'm.notes',
      ituraian: 'm.description',
      uraian: 'm.description',
      jenismutasi: 'm.movement_type::text',
      sumber: 'm.source',
      tahun: 'EXTRACT(YEAR FROM m.movement_date)',
      bulan: 'EXTRACT(MONTH FROM m.movement_date)',
    },
    where: "m.deleted_at IS NULL AND m.status = 'POSTED'",
    paramFilters: {
      ...periodFilters('m.movement_date'),
      ...warehouseFilter('w.id', 'w.code', 'w.name'),
      ...partnerFilter('m.requested_partner_id', 'p.code', 'p.name'),
    },
    orderBy: 'm.movement_date, m.doc_number, l.line_no',
  };
}
