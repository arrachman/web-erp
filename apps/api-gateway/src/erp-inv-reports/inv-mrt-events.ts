/**
 * Stock-event replay engine for the .mrt analytics datasets (Wave G4).
 *
 * Kartu stok / mutasi / nilai persediaan templates bind the legacy
 * staging schemas (m2r_kartu_stok `ks*`, m2r_mutasi_stok `ms*`,
 * persediaan detail `pd*`) whose rows carry RUNNING balances — not
 * expressible as one flat SQL config. This module replays the ERP's
 * derived event stream (POSTED movement lines signed by type, UNION
 * POSTED opening lines — the erp-inv-reports convention) in
 * chronological order per item×warehouse, applying the ERP's MOVING
 * AVERAGE costing (InvMovingAverageCostService): inbound valued at
 * line cost (fallback item average_cost → last_hpp → purchase_price),
 * outbound valued at the running average. By construction the final
 * running qty/value per item×warehouse equals the derived balance.
 */

import { Prisma } from '@prisma/client';
import type { PrismaService } from '../prisma/prisma.service';
import { normalizeValue } from '../erp-report-registry/report-data-provider';

export interface StockEvent {
  lineKey: string;
  date: Date;
  docNumber: string;
  type: string;
  source: string | null;
  description: string | null;
  headerNotes: string | null;
  lineNotes: string | null;
  itemId: number;
  itemCode: string;
  itemName: string;
  itemType: string | null;
  categoryId: number | null;
  categoryName: string | null;
  unitName: string | null;
  whId: number | null;
  whCode: string | null;
  whName: string | null;
  partnerId: number | null;
  partnerCode: string | null;
  partnerName: string | null;
  currencyCode: string | null;
  exchangeRate: number | null;
  price: number | null;
  signedQty: number;
  unitCost: number | null;
  itemAvgCost: number | null;
  itemLastHpp: number | null;
  itemPurchasePrice: number | null;
  postedAt: Date | null;
  createdAt: Date | null;
  costCenterId: number | null;
  costCenterName: string | null;
  divisionId: number | null;
  divisionName: string | null;
  projectId: number | null;
  projectName: string | null;
  branchId: number | null;
  branchName: string | null;
  locationId: number | null;
  locationName: string | null;
  // filled by replay()
  inQty: number;
  outQty: number;
  inPrice: number | null;
  outPrice: number | null;
  inValue: number;
  outValue: number;
  openingQty: number;
  openingValue: number;
  balanceQty: number;
  balanceValue: number;
  balanceAvg: number;
}

const EVENT_SQL = `
  SELECT * FROM (
    SELECT
      ('m' || l.id::text) AS line_key,
      m.movement_date AS evt_date, m.doc_number AS doc_number,
      m.movement_type::text AS typ, m.source AS source,
      m.description AS description, m.notes AS header_notes, l.notes AS line_notes,
      l.item_id AS item_id, i.code AS item_code, i.name AS item_name,
      i.type::text AS item_type, i.category_id AS category_id, ic.name AS category_name,
      u.name AS unit_name,
      COALESCE(l.destination_warehouse_id, l.source_warehouse_id) AS wh_id,
      w.code AS wh_code, w.name AS wh_name,
      m.requested_partner_id AS partner_id, p.code AS partner_code, p.name AS partner_name,
      cur.code AS currency_code, l.exchange_rate AS exchange_rate,
      COALESCE(l.sale_price, l.unit_cost) AS price,
      (CASE m.movement_type
        WHEN 'TRANSFER_RECEIPT' THEN l.base_quantity
        WHEN 'RETURN' THEN l.base_quantity
        WHEN 'ISSUE' THEN -l.base_quantity
        WHEN 'TRANSFER' THEN -l.base_quantity
        ELSE 0 END) AS signed_qty,
      l.unit_cost AS unit_cost,
      i.average_cost AS item_avg_cost, i.last_hpp AS item_last_hpp,
      i.purchase_price AS item_purchase_price,
      m.posted_at AS posted_at, m.created_at AS created_at,
      l.cost_center_id AS cc_id, cc.name AS cc_name,
      COALESCE(l.division_id, i.division_id) AS div_id, dv.name AS div_name,
      l.project_id AS proj_id, pj.name AS proj_name,
      m.branch_id AS branch_id, br.name AS branch_name,
      m.location_id AS loc_id, lc.name AS loc_name,
      1 AS kind_sort, l.id AS sort_id
    FROM inv_stock_movement_lines l
    JOIN inv_stock_movements m ON m.id = l.stock_movement_id
    JOIN md_items i ON i.id = l.item_id
    LEFT JOIN md_item_categories ic ON ic.id = i.category_id
    LEFT JOIN md_units u ON u.id = l.base_unit_id
    LEFT JOIN md_warehouses w
      ON w.id = COALESCE(l.destination_warehouse_id, l.source_warehouse_id)
    LEFT JOIN md_partners p ON p.id = m.requested_partner_id
    LEFT JOIN md_currencies cur ON cur.id = l.currency_id
    LEFT JOIN md_cost_centers cc ON cc.id = l.cost_center_id
    LEFT JOIN md_divisions dv ON dv.id = COALESCE(l.division_id, i.division_id)
    LEFT JOIN md_projects pj ON pj.id = l.project_id
    LEFT JOIN md_branches br ON br.id = m.branch_id
    LEFT JOIN md_locations lc ON lc.id = m.location_id
    WHERE m.status = 'POSTED' AND m.deleted_at IS NULL AND m.movement_type <> 'REQUEST'
    UNION ALL
    SELECT
      ('o' || ol.id::text), o.opening_date, o.doc_number,
      'OPENING', 'OPENING',
      o.description, o.notes, ol.notes,
      ol.item_id, i.code, i.name,
      i.type::text, i.category_id, ic.name,
      u.name,
      ol.warehouse_id, w.code, w.name,
      NULL, NULL, NULL,
      NULL, NULL,
      ol.unit_cost,
      ol.quantity,
      ol.unit_cost,
      i.average_cost, i.last_hpp, i.purchase_price,
      o.posted_at, o.created_at,
      ol.cost_center_id, cc.name,
      COALESCE(ol.division_id, i.division_id), dv.name,
      ol.project_id, pj.name,
      o.branch_id, br.name,
      o.location_id, lc.name,
      0, ol.id
    FROM inv_opening_stock_lines ol
    JOIN inv_opening_stocks o ON o.id = ol.opening_stock_id
    JOIN md_items i ON i.id = ol.item_id
    LEFT JOIN md_item_categories ic ON ic.id = i.category_id
    LEFT JOIN md_units u ON u.id = ol.base_unit_id
    LEFT JOIN md_warehouses w ON w.id = ol.warehouse_id
    LEFT JOIN md_cost_centers cc ON cc.id = ol.cost_center_id
    LEFT JOIN md_divisions dv ON dv.id = COALESCE(ol.division_id, i.division_id)
    LEFT JOIN md_projects pj ON pj.id = ol.project_id
    LEFT JOIN md_branches br ON br.id = o.branch_id
    LEFT JOIN md_locations lc ON lc.id = o.location_id
    WHERE o.status = 'POSTED' AND o.deleted_at IS NULL
  ) ev
  ORDER BY ev.item_id, ev.wh_id NULLS FIRST, ev.evt_date, ev.kind_sort, ev.doc_number, ev.sort_id
`;

function num(v: unknown): number | null {
  const n = normalizeValue(v);
  if (n === null || n === undefined) return null;
  const x = Number(n);
  return Number.isNaN(x) ? null : x;
}

function str(v: unknown): string | null {
  const n = normalizeValue(v);
  return n === null || n === undefined ? null : String(n);
}

export async function fetchStockEvents(prisma: PrismaService): Promise<StockEvent[]> {
  const rows = await prisma.$queryRaw<Array<Record<string, unknown>>>(Prisma.raw(EVENT_SQL));
  return rows.map((r) => ({
    lineKey: String(r.line_key),
    date: r.evt_date as Date,
    docNumber: String(r.doc_number ?? ''),
    type: String(r.typ ?? ''),
    source: str(r.source),
    description: str(r.description),
    headerNotes: str(r.header_notes),
    lineNotes: str(r.line_notes),
    itemId: Number(r.item_id),
    itemCode: String(r.item_code ?? ''),
    itemName: String(r.item_name ?? ''),
    itemType: str(r.item_type),
    categoryId: num(r.category_id),
    categoryName: str(r.category_name),
    unitName: str(r.unit_name),
    whId: num(r.wh_id),
    whCode: str(r.wh_code),
    whName: str(r.wh_name),
    partnerId: num(r.partner_id),
    partnerCode: str(r.partner_code),
    partnerName: str(r.partner_name),
    currencyCode: str(r.currency_code),
    exchangeRate: num(r.exchange_rate),
    price: num(r.price),
    signedQty: num(r.signed_qty) ?? 0,
    unitCost: num(r.unit_cost),
    itemAvgCost: num(r.item_avg_cost),
    itemLastHpp: num(r.item_last_hpp),
    itemPurchasePrice: num(r.item_purchase_price),
    postedAt: (r.posted_at as Date) ?? null,
    createdAt: (r.created_at as Date) ?? null,
    costCenterId: num(r.cc_id),
    costCenterName: str(r.cc_name),
    divisionId: num(r.div_id),
    divisionName: str(r.div_name),
    projectId: num(r.proj_id),
    projectName: str(r.proj_name),
    branchId: num(r.branch_id),
    branchName: str(r.branch_name),
    locationId: num(r.loc_id),
    locationName: str(r.loc_name),
    inQty: 0,
    outQty: 0,
    inPrice: null,
    outPrice: null,
    inValue: 0,
    outValue: 0,
    openingQty: 0,
    openingValue: 0,
    balanceQty: 0,
    balanceValue: 0,
    balanceAvg: 0,
  }));
}

export function fallbackCost(ev: StockEvent): number {
  return ev.itemAvgCost ?? ev.itemLastHpp ?? ev.itemPurchasePrice ?? 0;
}

/** Replay events (already ordered) computing running qty/value per item×warehouse. */
export function replay(events: StockEvent[]): StockEvent[] {
  const state = new Map<string, { qty: number; value: number }>();
  for (const ev of events) {
    const key = `${ev.itemId}|${ev.whId ?? 0}`;
    const st = state.get(key) ?? { qty: 0, value: 0 };
    ev.openingQty = st.qty;
    ev.openingValue = st.value;
    if (ev.signedQty > 0) {
      const cost = ev.unitCost ?? fallbackCost(ev);
      ev.inQty = ev.signedQty;
      ev.inPrice = cost;
      ev.inValue = ev.signedQty * cost;
      st.value += ev.inValue;
      st.qty += ev.signedQty;
    } else if (ev.signedQty < 0) {
      const avg = st.qty !== 0 ? st.value / st.qty : fallbackCost(ev);
      ev.outQty = -ev.signedQty;
      ev.outPrice = avg;
      ev.outValue = ev.outQty * avg;
      st.value -= ev.outValue;
      st.qty += ev.signedQty;
      if (st.qty === 0) st.value = 0;
    }
    ev.balanceQty = st.qty;
    ev.balanceValue = st.value;
    ev.balanceAvg = st.qty !== 0 ? st.value / st.qty : ev.outPrice ?? fallbackCost(ev);
    state.set(key, st);
  }
  return events;
}

export interface ReplayFilter {
  periodStart?: string;
  periodEnd?: string;
  warehouse?: string;
  partner?: string;
  division?: string;
  costCenter?: string;
  project?: string;
}

function matchText(param: string | undefined, id: number | null, code: string | null, name: string | null): boolean {
  if (param === undefined || param === '') return true;
  const p = param.toLowerCase();
  if (id !== null && String(id) === param) return true;
  if (code !== null && code.toLowerCase() === p) return true;
  return name !== null && name.toLowerCase().includes(p);
}

/** Emit window: running balances are computed over ALL events; only rows in the window are emitted. */
export function filterEvents(events: StockEvent[], f: ReplayFilter): StockEvent[] {
  return events.filter((ev) => {
    const d = ev.date instanceof Date ? ev.date.toISOString().slice(0, 10) : String(ev.date).slice(0, 10);
    if (f.periodStart && d < f.periodStart) return false;
    if (f.periodEnd && d > f.periodEnd) return false;
    if (!matchText(f.warehouse, ev.whId, ev.whCode, ev.whName)) return false;
    if (!matchText(f.partner, ev.partnerId, ev.partnerCode, ev.partnerName)) return false;
    if (!matchText(f.division, ev.divisionId, null, ev.divisionName)) return false;
    if (!matchText(f.costCenter, ev.costCenterId, null, ev.costCenterName)) return false;
    if (!matchText(f.project, ev.projectId, null, ev.projectName)) return false;
    return true;
  });
}

