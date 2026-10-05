/**
 * Outstanding + tracking configs (Wave G5). For each document line:
 *   jml          = ordered/received/invoiced quantity
 *   jmlrealisasi = realized quantity from the downstream document
 *   sisa         = jml − jmlrealisasi
 *
 * Realization sources (ERP conventions, mirroring
 * pur-goods-receipt-outstanding.helpers; pur line tables carry no
 * deleted_at — lines live and die with their header):
 *   PO line  ← Σ pur_goods_receipt_lines.accepted_qty (by order_line_id)
 *   GRN line ← Σ pur_invoice_lines.quantity (by goods_receipt_line_id)
 *   RI line  ← Σ pur_return_lines.quantity (by invoice_line_id)
 *   PR line  ← Σ pur_order_lines.quantity (by source_line_id)
 *   RQ line  ← Σ pur_order_lines.quantity (by source_line_id)
 *   DNR line ← Σ pur_return_lines.quantity (by source_line_id)
 *
 * DEVIATION (documented in DECISIONS): the legacy RI-outstanding
 * template computes sisa = jml * jmlrealisasi (multiplication — a
 * legacy bug); here sisa = jml − jmlrealisasi like every other family.
 * pooutstanding filters to lines with sisa > 0; the *outstanding
 * document prints show all lines of their document family.
 */

import {
  docFilters,
  docJoins,
  hdrCols,
  lineCols,
  partnerExtraCols,
  statusLabel,
  type PurDatasetConfig,
  type PurReportConfig,
} from './pur-mrt-configs';

const receivedByPoLine = `(SELECT COALESCE(SUM(gl.accepted_qty), 0)
  FROM pur_goods_receipt_lines gl
  JOIN pur_goods_receipts g ON g.id = gl.goods_receipt_id
  WHERE gl.order_line_id = l.id AND g.deleted_at IS NULL)`;

const invoicedByGrnLine = `(SELECT COALESCE(SUM(il.quantity), 0)
  FROM pur_invoice_lines il
  WHERE il.goods_receipt_line_id = l.id)`;

const returnedByRiLine = `(SELECT COALESCE(SUM(rl.quantity), 0)
  FROM pur_return_lines rl
  WHERE rl.invoice_line_id = l.id)`;

const orderedBySourceLine = `(SELECT COALESCE(SUM(pl.quantity), 0)
  FROM pur_order_lines pl
  WHERE pl.source_line_id = l.id)`;

const settledByReturnLine = `(SELECT COALESCE(SUM(rl.quantity), 0)
  FROM pur_return_lines rl
  WHERE rl.source_line_id = l.id)`;

/** Shared realization columns given the per-line realization expression. */
function realizationCols(realisasi: string): Record<string, string> {
  return {
    jmlrealisasi: realisasi,
    realisasi,
    sisa: `(l.quantity - ${realisasi})`,
    totalrealisasi: `(${realisasi} * l.unit_price - COALESCE(l.discount_amount, 0) * ${realisasi} / NULLIF(l.quantity, 0))`,
    progress: `CASE WHEN l.quantity = 0 THEN 0 ELSE ${realisasi} * 100.0 / l.quantity END`,
  };
}

/* -------------------------------- PO -------------------------------- */

function poOutstanding(): PurDatasetConfig {
  return {
    from: `
      pur_orders t
      JOIN pur_order_lines l ON l.order_id = t.id
      JOIN md_items i ON i.id = l.item_id
      LEFT JOIN md_units u ON u.id = l.unit_id
      ${docJoins('t.supplier_id')}
    `,
    select: {
      ...hdrCols('po'),
      ...lineCols(),
      ...partnerExtraCols(),
      ...realizationCols(receivedByPoLine),
      poid: 't.id',
    },
    where: `(l.quantity - ${receivedByPoLine}) > 0`,
    deletedAlias: 't',
    paramFilters: docFilters('t.doc_date'),
    orderBy: 't.doc_date, t.doc_number, l.line_no',
  };
}

/* -------------------------------- GRN -------------------------------- */

function grnOutstanding(): PurDatasetConfig {
  return {
    from: `
      pur_goods_receipts t
      JOIN pur_goods_receipt_lines l ON l.goods_receipt_id = t.id
      JOIN md_items i ON i.id = l.item_id
      LEFT JOIN md_units u ON u.id = l.unit_id
      ${docJoins('t.supplier_id')}
      LEFT JOIN md_warehouses w ON w.id = t.warehouse_id
    `,
    select: {
      ...hdrCols('grn'),
      ...lineCols(),
      ...partnerExtraCols(),
      ...realizationCols(invoicedByGrnLine),
      gudang: 'w.name',
    },
    deletedAlias: 't',
    paramFilters: docFilters('t.doc_date'),
    orderBy: 't.doc_date, t.doc_number, l.line_no',
  };
}

/* -------------------------------- RI -------------------------------- */

function riOutstanding(): PurDatasetConfig {
  return {
    from: `
      pur_invoices t
      JOIN pur_invoice_lines l ON l.invoice_id = t.id
      JOIN md_items i ON i.id = l.item_id
      LEFT JOIN md_units u ON u.id = l.unit_id
      ${docJoins('t.supplier_id')}
    `,
    select: {
      ...hdrCols('ri'),
      ...lineCols(),
      ...partnerExtraCols(),
      ...realizationCols(returnedByRiLine),
    },
    deletedAlias: 't',
    paramFilters: docFilters('t.doc_date'),
    orderBy: 't.doc_date, t.doc_number, l.line_no',
  };
}

/* ------------------------------ PR / RQ ------------------------------ */

function prOutstanding(): PurDatasetConfig {
  return {
    from: `
      pur_requisitions t
      JOIN pur_requisition_lines l ON l.requisition_id = t.id
      JOIN md_items i ON i.id = l.item_id
      LEFT JOIN md_units u ON u.id = l.unit_id
      ${docJoins('t.supplier_id')}
      LEFT JOIN adm_users req ON req.id = t.requested_by_id
    `,
    select: {
      ...hdrCols('pr'),
      ...lineCols(),
      ...partnerExtraCols(),
      ...realizationCols(orderedBySourceLine),
      prdimintaoleh: 'req.name',
      prmintake: 't.requested_to',
    },
    deletedAlias: 't',
    paramFilters: docFilters('t.doc_date'),
    orderBy: 't.doc_date, t.doc_number, l.line_no',
  };
}

/** prtracking: PR lines traced to their realizing PO and first RI. */
function prTracking(): PurDatasetConfig {
  const poLine = `(SELECT pl.id FROM pur_order_lines pl
    WHERE pl.source_line_id = l.id ORDER BY pl.id LIMIT 1)`;
  const poDoc = `(SELECT o.doc_number FROM pur_order_lines pl
    JOIN pur_orders o ON o.id = pl.order_id
    WHERE pl.source_line_id = l.id ORDER BY o.doc_date LIMIT 1)`;
  const riDoc = `(SELECT inv.doc_number FROM pur_order_lines pl
    JOIN pur_goods_receipt_lines gl ON gl.order_line_id = pl.id
    JOIN pur_invoice_lines il ON il.goods_receipt_line_id = gl.id
    JOIN pur_invoices inv ON inv.id = il.invoice_id
    WHERE pl.source_line_id = l.id ORDER BY inv.doc_date LIMIT 1)`;
  const riDate = riDoc.replace('inv.doc_number', 'inv.doc_date');
  return {
    from: `
      pur_requisitions t
      JOIN pur_requisition_lines l ON l.requisition_id = t.id
      JOIN md_items i ON i.id = l.item_id
      LEFT JOIN md_units u ON u.id = l.unit_id
      LEFT JOIN md_partners p ON p.id = t.supplier_id
      LEFT JOIN adm_users req ON req.id = t.requested_by_id
    `,
    select: {
      bkode: 'i.code',
      namabarang: 'i.name',
      catatan: 'l.notes',
      jml: 'l.quantity',
      satuan: 'u.name',
      ...realizationCols(orderedBySourceLine),
      prnotransaksi: 't.doc_number',
      prtgl: 't.doc_date',
      prstatus: statusLabel('t.status'),
      prcatatan: 't.notes',
      pruraian: 't.description',
      prdimintaoleh: 'req.name',
      prmintake: 't.requested_to',
      supplier: 'p.name',
      idpodetail: poLine,
      ponotransaksi: poDoc,
      potgljatuhtempo: `(SELECT o.due_date FROM pur_order_lines pl
        JOIN pur_orders o ON o.id = pl.order_id
        WHERE pl.source_line_id = l.id ORDER BY o.doc_date LIMIT 1)`,
      rinotransaksi: riDoc,
      ritgl: riDate,
    },
    deletedAlias: 't',
    paramFilters: docFilters('t.doc_date'),
    orderBy: 't.doc_date, t.doc_number, l.line_no',
  };
}

function rqOutstanding(): PurDatasetConfig {
  return {
    from: `
      pur_quotations t
      JOIN pur_quotation_lines l ON l.quotation_id = t.id
      JOIN md_items i ON i.id = l.item_id
      LEFT JOIN md_units u ON u.id = l.unit_id
      ${docJoins('t.supplier_id')}
    `,
    select: {
      ...hdrCols('rq'),
      ...lineCols(),
      ...partnerExtraCols(),
      ...realizationCols(orderedBySourceLine),
      rqsupplier: 'p.name',
      rqid: 't.id',
    },
    deletedAlias: 't',
    paramFilters: docFilters('t.doc_date'),
    orderBy: 't.doc_date, t.doc_number, l.line_no',
  };
}

/* -------------------------------- DNR -------------------------------- */

function dnrOutstanding(): PurDatasetConfig {
  return {
    from: `
      pur_returns t
      JOIN pur_return_lines l ON l.return_id = t.id
      JOIN md_items i ON i.id = l.item_id
      LEFT JOIN md_units u ON u.id = l.unit_id
      ${docJoins('t.supplier_id')}
      LEFT JOIN md_warehouses w ON w.id = t.warehouse_id
    `,
    select: {
      ...hdrCols('dnr'),
      ...lineCols(),
      ...partnerExtraCols(),
      ...realizationCols(settledByReturnLine),
      dnrgudang: 'w.name',
    },
    where: "t.return_type::text = 'DEBIT_NOTE'",
    deletedAlias: 't',
    paramFilters: docFilters('t.doc_date'),
    orderBy: 't.doc_date, t.doc_number, l.line_no',
  };
}

export const PUR_OUTSTANDING_CONFIGS: Record<string, PurReportConfig> = {
  'pur.pooutstanding': { datasets: { DS1: poOutstanding() } },
  'pur.grnoutstanding': { datasets: { DS1: grnOutstanding() } },
  'pur.rioutstanding': { datasets: { DS1: riOutstanding() } },
  'pur.dnroutstanding': { datasets: { DS1: dnrOutstanding() } },
  'pur.proutstanding': { datasets: { DS1: prOutstanding() } },
  'pur.rqoutstanding': { datasets: { DS1: rqOutstanding() } },
  'pur.prtracking': { datasets: { DS1: prTracking() } },
};
