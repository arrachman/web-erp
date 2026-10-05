/**
 * Pre-PO document configs (Wave G5): Purchase Requisitions (PR),
 * Quotations (RQ, legacy m4_rq), RFQ invitations (Undangan Penawaran,
 * legacy m4_rfq) and Bid Sheets (BS, legacy m4_bs) — document prints
 * and their list families share one config per entity.
 */

import {
  docFilters,
  docJoins,
  hdrCols,
  lineCols,
  partnerExtraCols,
  statusLabel,
  stockBalance,
  type PurDatasetConfig,
  type PurReportConfig,
} from './pur-mrt-configs';

/* ------------------------------- PR ------------------------------- */

const PR_FROM = `
  pur_requisitions t
  JOIN pur_requisition_lines l ON l.requisition_id = t.id
  JOIN md_items i ON i.id = l.item_id
  LEFT JOIN md_units u ON u.id = l.unit_id
  ${docJoins('t.supplier_id')}
  LEFT JOIN adm_users req ON req.id = t.requested_by_id
  LEFT JOIN adm_users usr ON usr.id = t.created_by_id
  LEFT JOIN adm_users upd ON upd.id = t.updated_by_id
  LEFT JOIN md_warehouses w ON w.id = t.warehouse_id
`;

const PR_SELECT: Record<string, string> = {
  ...hdrCols('pr'),
  ...lineCols(),
  ...partnerExtraCols(),
  prdimintaoleh: 'req.name',
  diminta: 'req.name',
  prmintake: 't.requested_to',
  mintake: 't.requested_to',
  prtgldipakai: 't.needed_date',
  hargajual: 'l.unit_price',
  // purchaserequestdetail2 stock snapshot columns
  bstok: stockBalance('i'),
  bstokminimal: 'i.min_stock',
  bstokmaksimal: 'i.max_stock',
  bhargabeli: 'i.purchase_price',
  poinputuser: 'usr.name',
  prinputtgl: 't.created_at',
  pomodifikasiuser: 'upd.name',
  prmodifikasitgl: 't.updated_at',
  prtotaltransaksi: 't.grand_total',
  gudang: 'w.name',
  bid: 'i.id',
  // Realization progress of the PR line (ordered via POs carrying this
  // PR as requisition_id, matched per item), %.
  progress: `(CASE WHEN l.quantity = 0 THEN 0 ELSE
    (SELECT COALESCE(SUM(pl.quantity), 0) FROM pur_order_lines pl
     JOIN pur_orders o ON o.id = pl.order_id
     WHERE o.requisition_id = t.id AND pl.item_id = l.item_id AND o.deleted_at IS NULL)
    * 100.0 / l.quantity END)`,
};

function prConfig(): PurDatasetConfig {
  return {
    from: PR_FROM,
    select: PR_SELECT,
    deletedAlias: 't',
    paramFilters: docFilters('t.doc_date', {
      warehouse: { id: 'w.id', code: 'w.code', name: 'w.name' },
    }),
    orderBy: 't.doc_date, t.doc_number, l.line_no',
  };
}

/** permintaandana prints PR rows under RI aliases (legacy template quirk). */
function permintaanDanaConfig(): PurDatasetConfig {
  return {
    from: PR_FROM,
    select: {
      pruraian: 't.description',
      ritgl: 't.doc_date',
      risupplier: 'p.name',
      rinotransaksi: 't.doc_number',
      bkode: 'i.code',
      namabarang: 'i.name',
      jml: 'l.quantity',
      satuan: 'u.name',
      harga: 'l.unit_price',
      total: '(l.quantity * l.unit_price)',
      catatan: 'l.notes',
    },
    deletedAlias: 't',
    paramFilters: docFilters('t.doc_date'),
    orderBy: 't.doc_date, t.doc_number, l.line_no',
  };
}

const prDoc = (): PurReportConfig => ({ datasets: { DS1: prConfig() } });

/** prhistorykawata: one row per PR line × the PO line that realized it
 *  (PO header requisition_id link, matched per item). */
function prHistoryConfig(): PurDatasetConfig {
  return {
    from: `
      pur_requisition_lines l
      JOIN pur_requisitions t ON t.id = l.requisition_id
      JOIN pur_orders po ON po.requisition_id = t.id AND po.deleted_at IS NULL
      JOIN pur_order_lines pl ON pl.order_id = po.id AND pl.item_id = l.item_id
      JOIN md_items i ON i.id = l.item_id
      LEFT JOIN md_partners p ON p.id = po.supplier_id
    `,
    select: {
      prid: 't.id',
      idprdetail: 'l.id',
      prnotransaksi: 't.doc_number',
      prtgl: 't.doc_date',
      prstatus: statusLabel('t.status'),
      prstatusnama: statusLabel('t.status'),
      poid: 'po.id',
      ponotransaksi: 'po.doc_number',
      potgl: 'po.doc_date',
      posupplier: 'p.name',
      kkode: 'p.code',
      knama: 'p.name',
      bkode: 'i.code',
      namabarang: 'i.name',
      idbarang: 'i.id',
      jml: 'l.quantity',
      jmlorder: 'pl.quantity',
    },
    deletedAlias: 't',
    paramFilters: docFilters('t.doc_date'),
    orderBy: 't.doc_date, t.doc_number, l.line_no',
  };
}

/* ------------------------------- RQ ------------------------------- */

const RQ_FROM = `
  pur_quotations t
  JOIN pur_quotation_lines l ON l.quotation_id = t.id
  JOIN md_items i ON i.id = l.item_id
  LEFT JOIN md_units u ON u.id = l.unit_id
  ${docJoins('t.supplier_id')}
`;

const RQ_SELECT: Record<string, string> = {
  ...hdrCols('rq'),
  ...lineCols(),
  ...partnerExtraCols(),
  supplier: 'p.name',
  rqsupplier: 'p.name',
  rqnogrup: 't.group_no',
  rqdiskon: 'COALESCE(t.discount_amount, 0)',
  tipebarang: 'i.type::text',
};

function rqConfig(orderByItem = false): PurDatasetConfig {
  return {
    from: RQ_FROM,
    select: RQ_SELECT,
    deletedAlias: 't',
    paramFilters: docFilters('t.doc_date'),
    orderBy: orderByItem ? 'i.code, t.doc_date, t.doc_number' : 't.doc_date, t.doc_number, l.line_no',
  };
}

const rqDoc = (): PurReportConfig => ({ datasets: { DS1: rqConfig() } });

/* ---------------------------- Undangan ---------------------------- */

const RFQ_FROM = `
  pur_rfqs t
  JOIN pur_requisitions prq ON prq.id = t.requisition_id
  JOIN pur_requisition_lines l ON l.requisition_id = prq.id
  JOIN md_items i ON i.id = l.item_id
  LEFT JOIN md_units u ON u.id = l.unit_id
  LEFT JOIN md_partners p ON p.id = t.supplier_id
  LEFT JOIN md_currencies pcur ON pcur.id = prq.currency_id
`;

const RFQ_SELECT: Record<string, string> = {
  ...lineCols(),
  rfqid: 't.id',
  rfqidpr: 't.requisition_id',
  rfqnotransaksi: 't.doc_number',
  rfqtgl: 't.doc_date',
  rfquraian: 't.description',
  prnotransaksi: 'prq.doc_number',
  prmatauang: 'pcur.code',
  prkurs: 'prq.exchange_rate',
  prjmldiskon: 'COALESCE(prq.discount_amount, 0)',
  prtotalpajak1detail: 'COALESCE(prq.tax1_amount, 0)',
  prbiayalain: 'COALESCE(prq.other_cost_amount, 0)',
  prtotaltransaksi: 'prq.grand_total',
  kkode: 'p.code',
  knama: 'p.name',
  nama: 'p.name',
  idrfqdetail: 'l.id',
  bnama: 'i.name',
};

function rfqConfig(): PurDatasetConfig {
  return {
    from: RFQ_FROM,
    select: RFQ_SELECT,
    deletedAlias: 't',
    paramFilters: docFilters('t.doc_date'),
    orderBy: 't.doc_date, t.doc_number, l.line_no',
  };
}

const rfqDoc = (): PurReportConfig => ({ datasets: { DS1: rfqConfig() } });

/* ------------------------------- BS ------------------------------- */

const BS_FROM = `
  pur_bid_selections t
  JOIN pur_bid_selection_lines bl ON bl.bid_selection_id = t.id
  JOIN pur_quotation_lines ql ON ql.id = bl.quotation_line_id
  JOIN pur_quotations q ON q.id = ql.quotation_id
  JOIN md_items i ON i.id = ql.item_id
  LEFT JOIN md_units u ON u.id = ql.unit_id
  LEFT JOIN md_partners p ON p.id = q.supplier_id
  LEFT JOIN md_currencies cur ON cur.id = t.currency_id
`;

const BS_SELECT: Record<string, string> = {
  bsnotransaksi: 't.doc_number',
  bstgl: 't.doc_date',
  bsstatus: statusLabel('t.status'),
  bsbagianperbandingan: 't.description',
  bsuraian: 't.description',
  bsmatauang: 'cur.code',
  bsnogrup: 'q.group_no',
  rqnotransaksi: 'q.doc_number',
  rqtgl: 'q.doc_date',
  rqsupplier: 'p.name',
  rqcatatan: 'q.notes',
  rquraian: 'q.description',
  bkode: 'i.code',
  namabarang: 'i.name',
  jml: 'ql.quantity',
  satuan: 'u.name',
  harga: 'ql.unit_price',
};

function bsConfig(orderByItem = false): PurDatasetConfig {
  return {
    from: BS_FROM,
    select: BS_SELECT,
    deletedAlias: 't',
    paramFilters: docFilters('t.doc_date'),
    orderBy: orderByItem ? 'i.code, t.doc_date, t.doc_number' : 't.doc_date, t.doc_number, bl.line_no',
  };
}

const bsDoc = (): PurReportConfig => ({ datasets: { DS1: bsConfig() } });

export const PUR_PRE_DOC_CONFIGS: Record<string, PurReportConfig> = {
  /* PR documents + lists */
  'pur.purchaserequestdetail': prDoc(),
  'pur.purchaserequestdetail2': prDoc(),
  'pur.purchaserequestdetailttd': prDoc(),
  'pur.buktipermintaanpembelian': prDoc(),
  'pur.prkawata': prDoc(),
  'pur.listpurchaserequest': prDoc(),
  'pur.listpurchaserequest2': prDoc(),
  'pur.listpurchaserequestproduct': { datasets: { DS1: { ...prConfig(), orderBy: 'i.code, t.doc_date, t.doc_number' } } },
  'pur.permintaandana': { datasets: { DS1: permintaanDanaConfig() } },
  'pur.prhistorykawata': { datasets: { DS1: prHistoryConfig() } },

  /* RQ documents + lists */
  'pur.requestforquotationdetail': rqDoc(),
  'pur.requestforquotationdetail1': rqDoc(),
  'pur.requestforquotationdetail2': rqDoc(),
  'pur.requestforquotationdetail2ud2': rqDoc(),
  'pur.requestforquotationdetailth': rqDoc(),
  'pur.requestforquotationdetailth2': rqDoc(),
  'pur.listrequestforquotation': rqDoc(),
  'pur.listrequestforquotation2': rqDoc(),
  'pur.listrequestforquotationproduct': { datasets: { DS1: rqConfig(true) } },

  /* RFQ invitations */
  'pur.undanganpenawarandetail': rfqDoc(),
  'pur.undanganpenawarandetail2': rfqDoc(),
  'pur.undanganpenawarandetailth': rfqDoc(),
  'pur.undanganpenawarandetailth2': rfqDoc(),

  /* Bid sheets */
  'pur.biddingsheetsdetail1': bsDoc(),
  'pur.biddingsheetsdetail2': bsDoc(),
  'pur.listbiddingsheets': bsDoc(),
  'pur.listbiddingsheets2': bsDoc(),
  'pur.listbiddingsheetsproduct': { datasets: { DS1: bsConfig(true) } },
};
