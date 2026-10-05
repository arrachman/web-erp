/**
 * Tracking + backorder configs (Wave G5).
 *
 * potracking traces PO lines through GRN → RI → settlement
 * (totalinvoice / paid / outstanding per line). pokawatagrnri lists
 * the PO ↔ GRN ↔ RI document chain per received line.
 * stockbackorderreportglobal lists sales-order lines not yet fully
 * delivered against finished-goods stock (legacy warehouse code 'FG')
 * valued at DERIVED moving-average cost — the binding G4 convention
 * (mirror of ErpInvMovingAverageCostService; md_items.average_cost
 * is never read for money figures).
 */

import {
  docFilters,
  docJoins,
  lineCols,
  lineTotal,
  statusLabel,
  type PurDatasetConfig,
  type PurReportConfig,
} from './pur-mrt-configs';

/* ------------------------------ potracking ------------------------------ */

const riChain = (expr: string) => `(SELECT ${expr} FROM pur_goods_receipt_lines gl
  JOIN pur_invoice_lines il ON il.goods_receipt_line_id = gl.id
  JOIN pur_invoices inv ON inv.id = il.invoice_id
  WHERE gl.order_line_id = l.id ORDER BY inv.doc_date LIMIT 1)`;

function poTrackingConfig(): PurDatasetConfig {
  return {
    from: `
      pur_orders t
      JOIN pur_order_lines l ON l.order_id = t.id
      JOIN md_items i ON i.id = l.item_id
      LEFT JOIN md_units u ON u.id = l.unit_id
      ${docJoins('t.supplier_id')}
      LEFT JOIN pur_requisitions prq ON prq.id = t.requisition_id
    `,
    select: {
      ...lineCols(),
      idpodetail: 'l.id',
      ponotransaksi: 't.doc_number',
      potgl: 't.doc_date',
      potgljatuhtempo: 't.due_date',
      postatus: statusLabel('t.status'),
      pocatatan: 't.notes',
      prnotransaksi: 'prq.doc_number',
      prtgl: 'prq.doc_date',
      supplier: 'p.name',
      csimbol: 'cur.code',
      subtotal: lineTotal('l'),
      riid: riChain('inv.id'),
      rinotransaksi: riChain('inv.doc_number'),
      ritgl: riChain('inv.doc_date'),
      ritotaltransaksi: riChain('inv.grand_total'),
      urutanri: riChain('il.line_no'),
      totalinvoice: `(SELECT COALESCE(SUM(il.quantity * il.unit_price - COALESCE(il.discount_amount, 0)), 0)
        FROM pur_goods_receipt_lines gl
        JOIN pur_invoice_lines il ON il.goods_receipt_line_id = gl.id
        WHERE gl.order_line_id = l.id)`,
      paid: `(SELECT COALESCE(SUM(a.amount), 0) FROM fin_settlement_allocations a
        WHERE a.invoice_ref IN (
          SELECT inv.doc_number FROM pur_goods_receipt_lines gl
          JOIN pur_invoice_lines il ON il.goods_receipt_line_id = gl.id
          JOIN pur_invoices inv ON inv.id = il.invoice_id
          WHERE gl.order_line_id = l.id))`,
      outstanding: `(SELECT COALESCE(SUM(il.quantity * il.unit_price - COALESCE(il.discount_amount, 0)), 0)
        FROM pur_goods_receipt_lines gl
        JOIN pur_invoice_lines il ON il.goods_receipt_line_id = gl.id
        WHERE gl.order_line_id = l.id)
        - (SELECT COALESCE(SUM(a.amount), 0) FROM fin_settlement_allocations a
          WHERE a.invoice_ref IN (
            SELECT inv.doc_number FROM pur_goods_receipt_lines gl
            JOIN pur_invoice_lines il ON il.goods_receipt_line_id = gl.id
            JOIN pur_invoices inv ON inv.id = il.invoice_id
            WHERE gl.order_line_id = l.id))`,
    },
    deletedAlias: 't',
    paramFilters: docFilters('t.doc_date'),
    orderBy: 't.doc_date, t.doc_number, l.line_no',
  };
}

/* ---------------------------- pokawatagrnri ---------------------------- */

function poGrnRiConfig(): PurDatasetConfig {
  return {
    from: `
      pur_goods_receipt_lines l
      JOIN pur_goods_receipts t ON t.id = l.goods_receipt_id
      JOIN md_items i ON i.id = l.item_id
      LEFT JOIN pur_orders o ON o.id = t.order_id
      LEFT JOIN pur_invoice_lines il ON il.goods_receipt_line_id = l.id
      LEFT JOIN pur_invoices inv ON inv.id = il.invoice_id
      LEFT JOIN md_partners p ON p.id = t.supplier_id
    `,
    select: {
      bkode: 'i.code',
      bnama: 'i.name',
      jml: 'l.quantity',
      knama: 'p.name',
      grnnotransaksi: 't.doc_number',
      grnnoref: 't.reference_no',
      grntgl: 't.doc_date',
      ponotransaksi: 'o.doc_number',
      potgl: 'o.doc_date',
      rinotransaksi: 'inv.doc_number',
      rinoref: 'inv.reference_no',
      ritgl: 'inv.doc_date',
    },
    deletedAlias: 't',
    paramFilters: docFilters('t.doc_date'),
    orderBy: 't.doc_date, t.doc_number, l.line_no',
  };
}

/* ------------------------- stock backorder (FG) ------------------------- */

const AVG_COST = `(SELECT CASE WHEN COALESCE(SUM(x.qty), 0) = 0 THEN 0
    ELSE SUM(x.qty * x.price) / SUM(x.qty) END
  FROM (
    SELECT ll.base_quantity AS qty, ll.unit_cost AS price
    FROM inv_stock_movement_lines ll
    JOIN inv_stock_movements m ON m.id = ll.stock_movement_id
    WHERE m.status = 'POSTED' AND m.deleted_at IS NULL AND ll.item_id = i.id
      AND m.movement_type IN ('TRANSFER_RECEIPT', 'RETURN')
    UNION ALL
    SELECT ol.quantity, ol.unit_cost
    FROM inv_opening_stock_lines ol
    JOIN inv_opening_stocks o ON o.id = ol.opening_stock_id
    WHERE o.status = 'POSTED' AND o.deleted_at IS NULL AND ol.item_id = i.id
  ) x)`;

const FG_STOCK = `(SELECT COALESCE(SUM(x.qty), 0) FROM (
    SELECT CASE m.movement_type
        WHEN 'TRANSFER_RECEIPT' THEN ll.base_quantity
        WHEN 'RETURN' THEN ll.base_quantity
        WHEN 'ISSUE' THEN -ll.base_quantity
        WHEN 'TRANSFER' THEN -ll.base_quantity ELSE 0 END AS qty
    FROM inv_stock_movement_lines ll
    JOIN inv_stock_movements m ON m.id = ll.stock_movement_id
    WHERE m.status = 'POSTED' AND m.deleted_at IS NULL AND ll.item_id = i.id
      AND COALESCE(ll.destination_warehouse_id, ll.source_warehouse_id) = (SELECT id FROM md_warehouses WHERE code = 'FG' LIMIT 1)
    UNION ALL
    SELECT ol.quantity
    FROM inv_opening_stock_lines ol
    JOIN inv_opening_stocks o ON o.id = ol.opening_stock_id
    WHERE o.status = 'POSTED' AND o.deleted_at IS NULL AND ol.item_id = i.id
      AND ol.warehouse_id = (SELECT id FROM md_warehouses WHERE code = 'FG' LIMIT 1)
  ) x)`;

const deliveredBySoLine = `(SELECT COALESCE(SUM(dl.quantity), 0)
  FROM sls_delivery_order_lines dl
  JOIN sls_delivery_orders d ON d.id = dl.delivery_order_id
  WHERE dl.source_line_id = l.id AND d.deleted_at IS NULL)`;

function backorderConfig(): PurDatasetConfig {
  return {
    from: `
      sls_order_lines l
      JOIN sls_orders t ON t.id = l.order_id
      JOIN md_items i ON i.id = l.item_id
      LEFT JOIN md_partners p ON p.id = t.customer_id
      LEFT JOIN md_partners sm ON sm.id = p.salesman_id
      LEFT JOIN md_divisions dv ON dv.id = t.sales_dept_id
    `,
    select: {
      kodecustomer: 'p.code',
      namacustomer: 'p.name',
      kodesalesman: 'sm.code',
      namasalesman: 'sm.name',
      bdivisi: 'dv.name',
      bkode: 'i.code',
      bnama: 'i.name',
      stok: FG_STOCK,
      bhppaverage: AVG_COST,
      amount: `((l.quantity - ${deliveredBySoLine}) * ${AVG_COST})`,
      wnama: `(SELECT name FROM md_warehouses WHERE code = 'FG' LIMIT 1)`,
    },
    where: `(l.quantity - ${deliveredBySoLine}) > 0`,
    deletedAlias: 't',
    paramFilters: docFilters('t.doc_date'),
    orderBy: 'p.name, t.doc_date, t.doc_number, l.line_no',
  };
}

export const PUR_ANALYTICS_CONFIGS: Record<string, PurReportConfig> = {
  'pur.potracking': { datasets: { DS1: poTrackingConfig() } },
  'pur.pokawatagrnri': { datasets: { DS1: poGrnRiConfig() } },
  'pur.stockbackorderreportglobal': { datasets: { DS1: backorderConfig() } },
};
