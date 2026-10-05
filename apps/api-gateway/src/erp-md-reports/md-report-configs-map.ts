/**
 * The full M1 report-config map (report_key → datasets), part of Wave G1.
 * Simple masters live here directly; item/contact configs compose the
 * shared fragments from md-report-configs.ts.
 */

import {
  ITEM_FROM,
  ITEM_PARAM_FILTERS,
  ITEM_SELECT,
  ITEM_STOCK_SQL,
  emptyConfig,
  simpleMaster,
  type MdReportConfig,
} from './md-report-configs';

const CONTACT_FROM = `
  md_partners p
  LEFT JOIN md_partner_categories pc ON pc.id = p.category_id
  LEFT JOIN md_partners sm ON sm.id = p.salesman_id
  LEFT JOIN LATERAL (
    SELECT * FROM md_partner_addresses a0
    WHERE a0.partner_id = p.id AND a0.deleted_at IS NULL
    ORDER BY a0.is_default DESC, a0.id LIMIT 1
  ) a ON true
  LEFT JOIN md_cities ct ON ct.id = a.city_id
  LEFT JOIN md_accounts ra ON ra.id = p.receivable_account_id
  LEFT JOIN md_accounts pa ON pa.id = p.payable_account_id
  LEFT JOIN md_payment_terms st ON st.id = p.sale_term_id
  LEFT JOIN md_payment_terms bt ON bt.id = p.purchase_term_id
`;

const CONTACT_PERSON = (field: string) =>
  `(SELECT c.${field} FROM md_partner_contacts c WHERE c.partner_id = p.id AND c.deleted_at IS NULL ORDER BY c.is_default DESC, c.id LIMIT 1)`;

const PARTNER_FILTER = {
  partner: {
    sql: `(p.id = CASE WHEN ? ~ '^[0-9]+$' THEN ?::bigint ELSE -1 END OR p.code ILIKE '%' || ? || '%' OR p.name ILIKE '%' || ? || '%')`,
    kind: 'text' as const,
  },
};

const itemReport = (extra?: {
  where?: string;
  select?: Record<string, string>;
  from?: string;
}): MdReportConfig => ({
  datasets: {
    DS1: {
      from: extra?.from ?? ITEM_FROM,
      select: { ...ITEM_SELECT, ...extra?.select },
      where: extra?.where,
      orderBy: 'i.code',
      deletedAlias: 'i',
      paramFilters: ITEM_PARAM_FILTERS,
    },
  },
});

const partnerCategory = (prefix: 'cc' | 'sc', kind?: string): MdReportConfig => ({
  datasets: {
    DS1: simpleMaster('md_partner_categories', prefix, {
      notes: null,
      where: kind ? `t.kind = '${kind}'` : undefined,
    }),
  },
});

export const MD_REPORT_CONFIGS: Record<string, MdReportConfig> = {
  'md.area': { datasets: { DS1: simpleMaster('md_areas', 'a', { notes: null }) } },
  'md.bank': { datasets: { DS1: simpleMaster('md_banks', 'b', { notes: null }) } },
  'md.branch': {
    datasets: {
      DS1: {
        ...simpleMaster('md_branches', 'b', { notes: 'notes' }),
        paramFilters: { branch: { sql: 't.id = ?', kind: 'number' } },
      },
    },
  },
  'md.city': { datasets: { DS1: simpleMaster('md_cities', 'c', { notes: null }) } },
  'md.country': { datasets: { DS1: simpleMaster('md_countries', 'c', { notes: null }) } },
  'md.province': { datasets: { DS1: simpleMaster('md_provinces', 'p', { notes: null }) } },
  'md.currency': {
    datasets: {
      DS1: {
        from: 'md_currencies t',
        select: {
          ckode: 't.code',
          cnama: 't.name',
          csimbol: 't.symbol',
          ckurs: `(SELECT r.rate FROM md_currency_rates r WHERE r.currency_id = t.id ORDER BY r.rate_date DESC, r.id DESC LIMIT 1)`,
        },
        orderBy: 't.code',
        deletedAlias: 't',
      },
    },
  },
  'md.coa': {
    datasets: {
      DS1: {
        from: 'md_accounts ac LEFT JOIN md_currencies cu ON cu.id = ac.currency_id',
        select: { cnomor: 'ac.code', cnama: 'ac.name', cnamaalias1: 'ac.alias', cmatauang: 'cu.code' },
        orderBy: 'ac.code',
        deletedAlias: 'ac',
      },
    },
  },
  'md.coa2': {
    datasets: {
      DS1: {
        from: 'md_accounts ac LEFT JOIN md_currencies cu ON cu.id = ac.currency_id',
        select: {
          cnomor: 'ac.code',
          cnama: 'ac.name',
          cnamaalias1: 'ac.alias',
          cmatauang: 'cu.code',
          clevel: 'ac.level',
        },
        orderBy: 'ac.code',
        deletedAlias: 'ac',
      },
    },
  },
  'md.itemcategory': { datasets: { DS1: simpleMaster('md_item_categories', 'ic', { notes: null }) } },
  'md.itemtype': { datasets: { DS1: simpleMaster('md_item_types', 'it', { notes: null }) } },
  'md.unit': {
    datasets: {
      DS1: simpleMaster('md_units', 'u', {
        notes: null,
        extra: { unilai: 't.conversion_factor', uketerangan: 't.notes' },
      }),
    },
  },
  'md.tax': {
    datasets: {
      DS1: simpleMaster('md_taxes', 't', {
        notes: null,
        extra: { tnilai: 't.rate' },
      }),
    },
  },
  'md.terms': {
    datasets: {
      DS1: simpleMaster('md_payment_terms', 'tr', {
        notes: null,
        extra: {
          trdiskon1: 't.discount_percent1',
          trharidiskon1: 't.discount_days1',
          trdiskon2: 't.discount_percent2',
          trharidiskon2: 't.discount_days2',
          trdenda: 't.penalty_percent',
          trharijatuhtempo: 't.net_days',
          traktif: 't.is_active',
          trinputtgl: 't.created_at',
          trmodifikasitgl: 't.updated_at',
        },
      }),
    },
  },
  'md.warehouse': {
    datasets: {
      DS1: {
        from: 'md_warehouses w LEFT JOIN md_locations l ON l.id = w.location_id',
        select: {
          wkode: 'w.code',
          wnama: 'w.name',
          walamat1: 'l.address_line1',
          wketerangan: 'w.notes',
        },
        orderBy: 'w.code',
        deletedAlias: 'w',
      },
    },
  },
  'md.location': {
    datasets: {
      DS1: {
        from: 'md_locations l LEFT JOIN md_branches b ON b.id = l.branch_id',
        select: { lkode: 'l.code', lnama: 'l.name', lcabang: 'b.code', cabang: 'b.name', lcatatan: 'l.notes' },
        orderBy: 'l.code',
        deletedAlias: 'l',
      },
    },
  },
  'md.itemlocation': {
    datasets: {
      DS1: {
        from: 'md_storage_bins sb JOIN md_warehouses w ON w.id = sb.warehouse_id',
        select: { ilkode: 'sb.code', ilnama: 'sb.name', wnama: 'w.name' },
        orderBy: 'w.code, sb.code',
        deletedAlias: 'sb',
      },
    },
  },
  'md.contactcategory': partnerCategory('cc'),
  'md.customercategory': partnerCategory('cc', 'CUSTOMER'),
  'md.kategoripemasok': partnerCategory('sc', 'SUPPLIER'),
  'md.salesmancategory': {
    datasets: {
      DS1: simpleMaster('md_partner_categories', 'sc', { notes: null, where: `t.kind = 'SALESMAN'` }),
    },
  },
  'md.costcenter': { datasets: { DS1: simpleMaster('md_cost_centers', 'cc', { notes: null }) } },
  'md.division': {
    datasets: { DS1: simpleMaster('md_divisions', 'd', { notes: 'note' }) },
  },
  'md.subdivision': {
    datasets: {
      DS1: {
        from: 'md_subdivisions s JOIN md_divisions dv ON dv.id = s.division_id',
        select: { sdkode: 's.code', divisi: 'dv.name', sdnama: 's.name' },
        orderBy: 'dv.code, s.code',
        deletedAlias: 's',
      },
    },
  },
  'md.departemen': { datasets: { DS1: simpleMaster('md_departments', 'dp', { notes: null }) } },
  'md.subdepartemen': {
    datasets: {
      DS1: {
        from: 'md_sub_departments sd JOIN md_departments d ON d.id = sd.department_id',
        select: { sdpkode: 'sd.code', sdpnama: 'sd.name', dpnama: 'd.name' },
        orderBy: 'd.code, sd.code',
        deletedAlias: 'sd',
      },
    },
  },
  'md.project': { datasets: { DS1: simpleMaster('md_projects', 'p', { notes: null }) } },
  'md.estimasikerja': {
    datasets: {
      DS1: simpleMaster('md_work_estimates', 'we', {
        notes: null,
        extra: { weaktif: 't.is_active', weinputtgl: 't.created_at', wemodifikasitgl: 't.updated_at' },
      }),
    },
  },
  'md.expedition': { datasets: { DS1: simpleMaster('md_expeditions', 'e', { notes: null }) } },
  'md.komisi': {
    datasets: {
      DS1: simpleMaster('md_commissions', 'km', {
        notes: null,
        extra: { kmdnilai: 't.amount' },
      }),
    },
  },
  'md.other': { datasets: { DS1: simpleMaster('md_miscellaneous', 'o', { notes: null }) } },
  'md.othercost': {
    datasets: {
      DS1: {
        from: 'md_other_costs oc LEFT JOIN md_accounts da ON da.id = oc.debit_account_id LEFT JOIN md_accounts ca ON ca.id = oc.credit_account_id',
        select: { ockode: 'oc.code', ocnama: 'oc.name', ocrekbeli: 'da.code', ocrekjual: 'ca.code' },
        orderBy: 'oc.code',
        deletedAlias: 'oc',
      },
    },
  },
  'md.stocksadjustmenttype': {
    datasets: {
      DS1: {
        from: 'md_stock_adjustment_types sat LEFT JOIN md_accounts ac ON ac.id = sat.account_id',
        select: { tsakode: 'sat.code', tsanama: 'sat.name', cnama: 'ac.name' },
        orderBy: 'sat.code',
        deletedAlias: 'sat',
      },
    },
  },
  'md.transactionnote': {
    datasets: {
      DS1: {
        from: 'md_transaction_notes t',
        select: { tnkode: 't.code', tncatatan: 't.name' },
        orderBy: 't.code',
        deletedAlias: 't',
      },
    },
  },
  'md.transactionnotedetail': {
    datasets: {
      DS1: {
        from: 'md_transaction_note_details d JOIN md_transaction_notes n ON n.id = d.transaction_note_id',
        select: { tndkode: 'n.code', tndcatatan: 'd.content' },
        orderBy: 'n.code, d.sort_order',
        deletedAlias: 'd',
      },
    },
  },
  /* ---------------- items & partners ---------------- */
  'md.item': itemReport(),
  'md.barangdetail': itemReport(),
  'md.daftarbarang': itemReport({ select: { kategoribarang: 'ic.code' } }),
  'md.itemdetail': itemReport({ select: { kategoribarang: 'ic.code' } }),
  'md.itemdetaildivisi': itemReport({
    from: `${ITEM_FROM}
      LEFT JOIN md_divisions dv ON dv.id = i.division_id
      LEFT JOIN md_departments dp ON dp.id = i.department_id
      LEFT JOIN md_sub_departments sdp ON sdp.id = i.sub_department_id`,
    select: {
      kategoribarang: 'ic.code',
      bdivisi: 'dv.name',
      bdepartemen: 'dp.name',
      bsubdepartemen: 'sdp.name',
    },
  }),
  'md.daftarhargaalamindo': itemReport({
    from: `${ITEM_FROM}
      LEFT JOIN md_departments dp ON dp.id = i.department_id
      LEFT JOIN md_sub_departments sdp ON sdp.id = i.sub_department_id`,
    select: { bdepartemen: 'dp.name', bsubdepartemen: 'sdp.name' },
  }),
  'md.label': itemReport(),
  'md.laporanpoinsalesman': itemReport(),
  'md.barangbawahstokminim': itemReport({
    where: `i.min_stock > 0 AND ${ITEM_STOCK_SQL} < i.min_stock`,
    select: { bbooking: '0' },
  }),
  'md.hargajualdibawahmargin': itemReport({
    where: 'i.purchase_price > 0 AND i.sale_price < i.purchase_price',
    select: { kategoribarang: 'ic.code', kategoribarangnama: 'ic.name' },
  }),
  'md.contact': {
    datasets: {
      DS1: {
        from: CONTACT_FROM,
        select: {
          kkode: 'p.code',
          namacontact: 'p.name',
          k1notelp1: 'a.phone',
          k1alamat1: 'a.address_line1',
          k1alamat2: 'a.address_line2',
          k1kota: 'ct.name',
          k1kontaknohp: CONTACT_PERSON('phone'),
          ccnama: 'pc.name',
          salesmankode: 'sm.code',
          salesmannama: 'sm.name',
        },
        orderBy: 'p.code',
        deletedAlias: 'p',
        paramFilters: PARTNER_FILTER,
      },
    },
  },
  'md.contactdetail': {
    datasets: {
      DS1: {
        from: CONTACT_FROM,
        select: {
          kid: 'p.id',
          kkode: 'p.code',
          knama: 'p.name',
          namacontact: 'p.name',
          ccnama: 'pc.name',
          kaktiftgl: 'p.created_at',
          k1alamat1: 'a.address_line1',
          k1notelp1: 'a.phone',
          k1nofax: 'a.fax',
          k1email: 'a.email',
          k1website: 'a.website',
          k1kontaknohp: CONTACT_PERSON('phone'),
          k1kontakemail: CONTACT_PERSON('email'),
          kanama: CONTACT_PERSON('name'),
          kajabatan: CONTACT_PERSON('title'),
          kanotelp: CONTACT_PERSON('phone'),
          kanohp: CONTACT_PERSON('phone'),
          kaemail: CONTACT_PERSON('email'),
          kbatashutang: 'p.ap_credit_limit',
          kbataspiutang: 'p.ar_credit_limit',
          kterminbeli: 'bt.name',
          kterminjual: 'st.name',
          krekhutang: 'pa.code',
          krekpiutang: 'ra.code',
          knorekening: `(SELECT ba.account_number FROM md_partner_bank_accounts ba WHERE ba.partner_id = p.id AND ba.deleted_at IS NULL ORDER BY ba.is_default DESC, ba.id LIMIT 1)`,
          kbank: `(SELECT ba.bank_name FROM md_partner_bank_accounts ba WHERE ba.partner_id = p.id AND ba.deleted_at IS NULL ORDER BY ba.is_default DESC, ba.id LIMIT 1)`,
        },
        orderBy: 'p.code',
        deletedAlias: 'p',
        paramFilters: PARTNER_FILTER,
      },
    },
  },
  /* -------- no ERP equivalent yet: honest empty datasets -------- */
  'md.kategoripengecekan': {
    datasets: { DS1: emptyConfig('Kategori pengecekan belum ada padanannya di ERP (tidak ada tabel md_*)') },
  },
  'md.kategoriproduksi': {
    datasets: { DS1: emptyConfig('Kategori produksi (m1_pricekategori produksi) belum ada padanannya di ERP') },
  },
  'md.baranghauling': {
    datasets: { DS1: emptyConfig('Barang hauling/hourmeter belum ada padanannya di md_items') },
  },
  'md.barangkhusus': {
    datasets: { DS1: emptyConfig('Laporan barang khusus berbasis mutasi stok — builder transaksi menyusul di gelombang inventory (G4)') },
  },
  'md.sellingpoint': {
    datasets: { DS1: emptyConfig('Selling point belum ada padanannya di ERP') },
  },
  'md.poinpelanggan': {
    datasets: { DS1: emptyConfig('Poin pelanggan belum ada padanannya di ERP (program poin belum dibangun)') },
  },
  'md.labelpci2': {
    datasets: { DS1: emptyConfig('Label PCI berbasis dokumen penerimaan (m4) — builder lintas modul menyusul di gelombang purchasing (G5)') },
  },
  'md.pricekategori': {
    datasets: {
      DS1: {
        from: 'md_item_categories t',
        select: { pckode: 't.code', pcnama: 't.name' },
        orderBy: 't.code',
        deletedAlias: 't',
        note: 'Kategori harga legacy (m1_pricekategori) belum ada tabelnya; ditampilkan daftar kategori barang, kolom harga per kategori kosong',
      },
    },
  },
};
