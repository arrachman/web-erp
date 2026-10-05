/**
 * Purchase-return document configs (Wave G5): DNR (Delivery Note
 * Return, return_type = 'DEBIT_NOTE') and PRT (Purchase Return,
 * return_type = 'RETURN_TO_VENDOR') prints and lists, split from the
 * RI config file for the 400-line rule. Batch DS2 resolves the lot
 * through the source GRN line; serial DS2 resolves serials through
 * the return's source invoice GRN.
 */

import {
  docFilters,
  docJoins,
  hdrCols,
  lineCols,
  lineTotal,
  partnerExtraCols,
  type PurDatasetConfig,
  type PurReportConfig,
} from './pur-mrt-configs';

/* ----------------------------- Returns ----------------------------- */

function retFrom(extra = ''): string {
  return `
    pur_returns t
    JOIN pur_return_lines l ON l.return_id = t.id
    JOIN md_items i ON i.id = l.item_id
    LEFT JOIN md_units u ON u.id = l.unit_id
    ${docJoins('t.supplier_id')}
    LEFT JOIN md_warehouses w ON w.id = t.warehouse_id
    LEFT JOIN pur_invoices r ON r.id = t.invoice_id
    ${extra}
  `;
}

function retSelect(prefix: 'dnr' | 'prt'): Record<string, string> {
  return {
    ...hdrCols(prefix),
    ...lineCols(),
    ...partnerExtraCols(),
    ritermin: 'pt.name',
    rinotransaksi: 'r.doc_number',
    grnnotransaksi:
      '(SELECT g.doc_number FROM pur_goods_receipts g WHERE g.id = r.goods_receipt_id)',
    idbarang: 'i.id',
    bid: 'i.id',
    [`${prefix}gudang`]: 'w.name',
    gudang: 'w.name',
    prtnotransaksimix: 't.doc_number',
    kurs: 't.exchange_rate',
    totalrp: `(${lineTotal('l')} * t.exchange_rate)`,
    totalvalas: lineTotal('l'),
    'COUNT_x0028__x002A__x0029_':
      '(SELECT COUNT(*) FROM pur_return_lines x WHERE x.return_id = t.id)',
  };
}

function retFilters() {
  return docFilters('t.doc_date', {
    warehouse: { id: 'w.id', code: 'w.code', name: 'w.name' },
  });
}

function retDs1(prefix: 'dnr' | 'prt', typeFilter: string): PurDatasetConfig {
  return {
    from: retFrom(),
    select: retSelect(prefix),
    where: `t.return_type::text = '${typeFilter}'`,
    deletedAlias: 't',
    paramFilters: retFilters(),
    orderBy: 't.doc_date, t.doc_number, l.line_no',
  };
}

function retDs2Batch(prefix: 'dnr' | 'prt', typeFilter: string): PurDatasetConfig {
  return {
    from: retFrom('LEFT JOIN pur_goods_receipt_lines gl ON gl.id = l.goods_receipt_line_id'),
    select: { ...retSelect(prefix), nbtkode: 'gl.lot_number', nbtidbarang: 'l.item_id' },
    where: `t.return_type::text = '${typeFilter}'`,
    deletedAlias: 't',
    paramFilters: retFilters(),
    orderBy: 't.doc_date, t.doc_number, l.line_no',
  };
}

function retDs2Serial(prefix: 'dnr' | 'prt', typeFilter: string): PurDatasetConfig {
  return {
    from: `
      pur_returns t
      JOIN pur_invoices r ON r.id = t.invoice_id
      JOIN inv_serials s ON s.origin_goods_receipt_id = r.goods_receipt_id AND s.deleted_at IS NULL
      JOIN md_items i ON i.id = s.item_id
      LEFT JOIN md_units u ON u.id = i.base_unit_id
      ${docJoins('t.supplier_id')}
      LEFT JOIN md_warehouses w ON w.id = t.warehouse_id
    `,
    select: {
      ...retSelect(prefix),
      rinotransaksi: 'r.doc_number',
      nstkode: 's.serial_number',
      nstidbarang: 's.item_id',
      // No return-line alias in this FROM — one row per serial.
      jml: '1',
      satuan: 'u.name',
      diskon: 'NULL',
      jmldiskon: '0',
      harga: 'NULL',
      total: 'NULL',
      catatan: 'NULL',
      urutan: 'NULL',
      idbarang: 'i.id',
    },
    where: `t.return_type::text = '${typeFilter}'`,
    deletedAlias: 't',
    paramFilters: docFilters('t.doc_date'),
    orderBy: 't.doc_date, t.doc_number, s.serial_number',
  };
}

const retDoc = (
  prefix: 'dnr' | 'prt',
  typeFilter: string,
  ds2?: PurDatasetConfig,
): PurReportConfig => ({
  datasets: ds2
    ? { DS1: retDs1(prefix, typeFilter), DS2: ds2 }
    : { DS1: retDs1(prefix, typeFilter) },
});

const DNR = 'DEBIT_NOTE';
const PRT = 'RETURN_TO_VENDOR';

export const PUR_RET_CONFIGS: Record<string, PurReportConfig> = {
  /* DNR documents + lists (return_type = DEBIT_NOTE) */
  'pur.deliverynotereturndetail1': retDoc('dnr', DNR),
  'pur.deliverynotereturndetail2': retDoc('dnr', DNR),
  'pur.deliverynotereturndetail1serial': retDoc('dnr', DNR, retDs2Serial('dnr', DNR)),
  'pur.listdeliverynotereturn': retDoc('dnr', DNR),
  'pur.listdeliverynotereturn2': retDoc('dnr', DNR),
  'pur.listdeliverynotereturnperproduct': {
    datasets: { DS1: { ...retDs1('dnr', DNR), orderBy: 'i.code, t.doc_date, t.doc_number' } },
  },
  'pur.listdeliverynotereturnproduct': {
    datasets: { DS1: { ...retDs1('dnr', DNR), orderBy: 'i.code, t.doc_date, t.doc_number' } },
  },

  /* PRT documents + lists (return_type = RETURN_TO_VENDOR) */
  'pur.purchasereturndetail1': retDoc('prt', PRT),
  'pur.purchasereturndetail2': retDoc('prt', PRT),
  'pur.purchasereturndetail1batch': retDoc('prt', PRT, retDs2Batch('prt', PRT)),
  'pur.purchasereturndetail1batch2': retDoc('prt', PRT, retDs2Batch('prt', PRT)),
  'pur.purchasereturndetail1batch3': retDoc('prt', PRT, retDs2Batch('prt', PRT)),
  'pur.purchasereturndetail1batch4': retDoc('prt', PRT, retDs2Batch('prt', PRT)),
  'pur.purchasereturndetail1serial': retDoc('prt', PRT, retDs2Serial('prt', PRT)),
  'pur.purchasereturndetail1serial2': retDoc('prt', PRT, retDs2Serial('prt', PRT)),
  'pur.purchasereturndetail1serial3': retDoc('prt', PRT, retDs2Serial('prt', PRT)),
  'pur.purchasereturndetail1serial4': retDoc('prt', PRT, retDs2Serial('prt', PRT)),
  'pur.purchasereturndetail1serialud': retDoc('prt', PRT, retDs2Serial('prt', PRT)),
  'pur.buktibarangkeluarretur': retDoc('prt', PRT),
  'pur.listpurchasereturn': retDoc('prt', PRT),
  'pur.listpurchasereturn1': retDoc('prt', PRT),
  'pur.listpurchasereturnhppdetail': retDoc('prt', PRT),
  'pur.listpurchasereturnperproduct': {
    datasets: { DS1: { ...retDs1('prt', PRT), orderBy: 'i.code, t.doc_date, t.doc_number' } },
  },
};
