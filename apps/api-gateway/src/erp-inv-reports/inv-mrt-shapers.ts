/**
 * Row shapers for the replay engine (Wave G4): map one replayed stock
 * event onto the legacy staging column names the .mrt templates bind
 * (kartu stok `ks*`, mutasi stok `ms*`, persediaan detail `pd*`).
 * Declared template columns without a mapping stay NULL.
 */

import { fallbackCost, type StockEvent } from './inv-mrt-events';

function blankRow(cols: string[]): Record<string, unknown> {
  const row: Record<string, unknown> = {};
  for (const c of cols) row[c] = null;
  return row;
}

/** Shape one event into the legacy kartu stok (`ks*`) staging columns. */
export function toKsRow(ev: StockEvent, cols: string[], seq: number): Record<string, unknown> {
  const row = blankRow(cols);
  const fill: Record<string, unknown> = {
    ksnourut: seq,
    ksid: Number(ev.lineKey.slice(1)) || null,
    ksgudang: ev.whId,
    ksgudangnama: ev.whName,
    kskategoribarang: ev.categoryId,
    kskategoribarangnama: ev.categoryName,
    ksidbarang: ev.itemId,
    kskodebarang: ev.itemCode,
    kstipebarang: ev.itemType,
    ksnamabarang: ev.itemName,
    kssatuanbarang: ev.unitName,
    kstgl: ev.date,
    kssumber: ev.source ?? ev.type,
    ksnotransaksi: ev.docNumber,
    kskontak: ev.partnerId,
    kskontakkode: ev.partnerCode,
    kskontaknama: ev.partnerName,
    ksuraian: ev.description,
    kscatatan: ev.headerNotes,
    kscatatandetail: ev.lineNotes,
    ksmatauang: ev.currencyCode ?? 'IDR',
    kskurs: ev.exchangeRate ?? 1,
    ksharga: ev.price,
    ksdiskon: null,
    ksjmldiskon: null,
    ksjenismutasi: ev.type,
    ksjmlmasuk: ev.inQty,
    kshargamasuk: ev.inPrice,
    ksnilaimasuk: ev.inValue,
    ksjmlkeluar: ev.outQty,
    kshargakeluar: ev.outPrice,
    ksnilaikeluar: ev.outValue,
    kssaldojml: ev.balanceQty,
    kssaldohpp: ev.balanceAvg,
    kssaldonilai: ev.balanceValue,
    kspostingtgl: ev.postedAt,
    ksinputtgl: ev.createdAt,
    // inv.kartustokfifo2 subset columns
    customer: ev.partnerName,
    salesman: null,
    // fin.nilaipersediaangudangdetail summary columns (per item×warehouse window)
    kssaldoawal: ev.openingQty,
    kshargaawal: ev.openingQty !== 0 ? ev.openingValue / ev.openingQty : fallbackCost(ev),
    ksnilaiawal: ev.openingValue,
  };
  for (const [k, v] of Object.entries(fill)) if (k in row) row[k] = v;
  return row;
}

/** Shape one event into the legacy mutasi stok (`ms*`) staging columns. */
export function toMsRow(ev: StockEvent, cols: string[], seq: number): Record<string, unknown> {
  const row = blankRow(cols);
  const fill: Record<string, unknown> = {
    msnourut: seq,
    msid: Number(ev.lineKey.slice(1)) || null,
    msgudang: ev.whId,
    msgudangnama: ev.whName,
    msidbarang: ev.itemId,
    mskodebarang: ev.itemCode,
    mstipebarang: ev.itemType,
    msnamabarang: ev.itemName,
    mssatuanbarang: ev.unitName,
    mstgl: ev.date,
    mssumber: ev.source ?? ev.type,
    msnotransaksi: ev.docNumber,
    mskontak: ev.partnerId,
    mskontakkode: ev.partnerCode,
    mskontaknama: ev.partnerName,
    msuraian: ev.description,
    mscatatan: ev.headerNotes,
    mscatatandetail: ev.lineNotes,
    msjmlmasuk: ev.inQty,
    msjmlkeluar: ev.outQty,
    mssaldo: ev.balanceQty,
    msinputtgl: ev.createdAt,
    mscabang: ev.branchId,
    mscabangnama: ev.branchName,
    mslokasi: ev.locationId,
    mslokasinama: ev.locationName,
    mscostcenter: ev.costCenterId,
    mscostcenternama: ev.costCenterName,
    msdivisi: ev.divisionId,
    msdivisinama: ev.divisionName,
    msproyek: ev.projectId,
    msproyeknama: ev.projectName,
    mskategoribarang: ev.categoryId,
    mskategoribarangnama: ev.categoryName,
    mshargajual: null,
    customer: ev.partnerName,
    salesman: null,
    kkategori: null,
    cckode: null,
    ccnama: ev.costCenterName,
    sckode: null,
    scnama: null,
  };
  for (const [k, v] of Object.entries(fill)) if (k in row) row[k] = v;
  return row;
}

/** Shape one event into the persediaan detail (`pd*`) staging columns. */
export function toPdRow(ev: StockEvent, cols: string[]): Record<string, unknown> {
  const row = blankRow(cols);
  const fill: Record<string, unknown> = {
    pdid: Number(ev.lineKey.slice(1)) || null,
    pdidbarang: ev.itemId,
    pdkodebarang: ev.itemCode,
    pdtipebarang: ev.itemType,
    pdnamabarang: ev.itemName,
    pdsatuanbarang: ev.unitName,
    pdtgl: ev.date,
    pdsumber: ev.source ?? ev.type,
    pdnotransaksi: ev.docNumber,
    pduraian: ev.description,
    pdcatatan: ev.headerNotes,
    pdcatatandetail: ev.lineNotes,
    pdjenismutasi: ev.type,
    pdjmlmasuk: ev.inQty,
    pdhppmasuk: ev.inPrice,
    pdnilaimasuk: ev.inValue,
    pdjmlkeluar: ev.outQty,
    pdhppkeluar: ev.outPrice,
    pdnilaikeluar: ev.outValue,
    pdsaldoawal: ev.openingQty,
    pdinputtgl: ev.createdAt,
  };
  for (const [k, v] of Object.entries(fill)) if (k in row) row[k] = v;
  return row;
}
