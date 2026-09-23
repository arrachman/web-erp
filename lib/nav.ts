/**
 * Sidebar navigation tree + per-route page metadata.
 * Ported from prototype `sidebar.jsx` (NAV) and the `pageMeta` helper.
 *
 * ERP_ROUTE_META extracted to lib/erp-route-meta.ts (400-line limit).
 */
import type { IconName } from '@/components/ui/icons';
import { ERP_ROUTE_META } from './erp-route-meta';
export { ERP_ROUTE_META };

export interface NavLeaf {
  id: string;
  label: string;
  code?: string;
}

export interface NavGroup {
  group: string;
  items: NavLeaf[];
}

export interface NavItem {
  id?: string;
  icon?: IconName;
  label?: string;
  children?: NavLeaf[] | NavGroup[];
  divider?: boolean;
}

export const NAV: NavItem[] = [
  { id: 'home', icon: 'home', label: 'Dashboard' },
  { id: 'statistik', icon: 'stats', label: 'Statistik' },
  {
    id: 'org',
    icon: 'database',
    label: 'Organization',
    children: [
      { id: '/org/branches', label: 'Branch', code: 'BRN' },
      { id: '/org/locations', label: 'Location', code: 'LOC' },
      { id: '/org/warehouses', label: 'Warehouse', code: 'WHS' },
      { id: '/org/divisions', label: 'Division', code: 'DIV' },
      { id: '/org/sub-divisions', label: 'Sub Division', code: 'SDIV' },
      { id: '/org/projects', label: 'Project', code: 'PRJ' },
      { id: '/org/cost-centers', label: 'Cost Center', code: 'CC' },
      { id: '/org/departments', label: 'Department', code: 'DEPT' },
      { id: '/org/sub-departments', label: 'Sub Department', code: 'SDEPT' },
    ],
  },
  {
    id: 'master',
    icon: 'database',
    label: 'Data Master',
    children: [
      { id: 'md-items', label: 'Item', code: 'ITM' },
      { id: 'md-partners', label: 'Partner', code: 'PTR' },
      { id: 'md-item-categories', label: 'Kategori Item', code: 'ICAT' },
      { id: 'md-units', label: 'Satuan', code: 'UOM' },
      { id: 'm-customer', label: 'Customer (lama)', code: 'CUS' },
      { id: 'm-supplier', label: 'Supplier (lama)', code: 'SUP' },
      { id: 'm-coa', label: 'Chart of Account', code: 'CoA' },
      { id: 'm-lokasi', label: 'Cabang & Lokasi', code: 'LOC' },
      { id: 'm-costcenter', label: 'Cost Center', code: 'CC' },
      { id: '/master/colors', label: 'Warna', code: 'CLR' },
      { id: '/master/nozzles', label: 'Nozzle', code: 'NZL' },
      { id: '/master/oems', label: 'OEM', code: 'OEM' },
      { id: '/master/price-indices', label: 'Price Index', code: 'PRX' },
      { id: '/master/item-info', label: 'Info Item', code: 'INF' },
    ],
  },
  {
    id: 'keuangan',
    icon: 'coins',
    label: 'Keuangan',
    children: [
      {
        group: 'Transaksi',
        items: [
          { id: 'kas-masuk', label: 'Kas Masuk', code: 'CR' },
          { id: 'kas-keluar', label: 'Kas Keluar', code: 'CD' },
          { id: 'bank-masuk', label: 'Bank Masuk', code: 'RM' },
          { id: 'bank-keluar', label: 'Bank Keluar', code: 'SM' },
          { id: 'jurnal-umum', label: 'Jurnal Umum', code: 'GJ' },
          { id: 'giro-masuk', label: 'Giro Masuk', code: 'RG' },
          { id: 'giro-keluar', label: 'Giro Keluar', code: 'SG' },
          { id: 'giro-masuk-batal', label: 'Giro Masuk Batal', code: 'RGC' },
          { id: 'giro-keluar-batal', label: 'Giro Keluar Batal', code: 'SGC' },
          { id: 'saldo-awal', label: 'Saldo Awal Coa', code: 'CB' },
          { id: 'buku-besar', label: 'Buku Besar', code: 'GL' },
        ],
      },
      {
        group: 'Laporan',
        items: [
          { id: 'rep-neraca', label: 'Neraca', code: 'BS' },
          { id: 'rep-labarugi', label: 'Laba Rugi', code: 'PL' },
          { id: 'rep-aruskas', label: 'Arus Kas', code: 'CF' },
          { id: 'rep-modal', label: 'Perubahan Modal', code: 'EQ' },
        ],
      },
    ],
  },
  {
    id: 'persediaan',
    icon: 'boxes',
    label: 'Persediaan',
    children: [
      { id: 'inv-opname', label: 'Stock Opname', code: 'SO' },
      { id: 'inv-mutasi', label: 'Mutasi Stok', code: 'MS' },
      { id: 'inv-adjust', label: 'Penyesuaian', code: 'AJ' },
      { id: 'inv-transfer', label: 'Transfer Gudang', code: 'TG' },
    ],
  },
  {
    id: 'pembelian',
    icon: 'cart',
    label: 'Pembelian',
    children: [
      { id: 'pur-po', label: 'PO Pembelian', code: 'PO' },
      { id: 'pur-receipt', label: 'Penerimaan Barang', code: 'PR' },
      { id: 'pur-invoice', label: 'Faktur Pembelian', code: 'PI' },
      { id: 'pur-return', label: 'Retur Pembelian', code: 'PRT' },
    ],
  },
  {
    id: 'sales',
    icon: 'tag',
    label: 'Sales',
    children: [
      { id: 'sales-order', label: 'SO Penjualan', code: 'SO' },
      { id: 'sal-do', label: 'Pengiriman', code: 'DO' },
      { id: 'sal-invoice', label: 'Faktur Penjualan', code: 'SI' },
      { id: 'sal-return', label: 'Retur Penjualan', code: 'SRT' },
    ],
  },
  {
    id: 'produksi',
    icon: 'factory',
    label: 'Produksi',
    children: [
      { id: 'prd-wo', label: 'Work Order', code: 'WO' },
      { id: 'prd-bom', label: 'BOM', code: 'BOM' },
      { id: 'prd-output', label: 'Output Produksi', code: 'OP' },
    ],
  },
  {
    id: 'fixed-asset',
    icon: 'layers',
    label: 'Fixed Asset',
    children: [
      { id: 'fa-list', label: 'Daftar Aset', code: 'FA' },
      { id: 'fa-deprec', label: 'Penyusutan', code: 'DEP' },
      { id: 'fa-disposal', label: 'Disposal', code: 'DSP' },
    ],
  },
  { divider: true },
  {
    id: 'setting',
    icon: 'gear',
    label: 'Setting',
    children: [
      { id: 'adm-users', label: 'Users', code: 'USR' },
      { id: 'adm-roles', label: 'Roles', code: 'ROL' },
      { id: 'adm-branches', label: 'Cabang', code: 'BRN' },
      { id: 'adm-settings', label: 'System Settings', code: 'SET' },
      { id: 'set-users', label: 'Users (lama)', code: 'U' },
      { id: 'set-roles', label: 'Roles (lama)', code: 'R' },
      { id: 'set-prefs', label: 'Preferensi', code: 'PR' },
      { id: 'set-appearance', label: 'Tampilan', code: 'UI' },
    ],
  },
];

export function isNavGroupArray(
  children: NavLeaf[] | NavGroup[] | undefined,
): children is NavGroup[] {
  return Array.isArray(children) && !!children[0] && 'group' in children[0];
}

export interface Crumb {
  label: string;
  onClick?: () => void;
}

export interface PageMeta {
  title: string;
  icon: IconName;
  code?: string;
  crumbs: Crumb[];
}

/**
 * Title/breadcrumb for ERP routes keyed by the seeded `sys_menus.path`
 * (canonical id). The full object lives in `lib/erp-route-meta.ts`;
 * re-exported here for backward compatibility.
 */

/** Resolve a route id to its title/icon/breadcrumb — mirrors prototype `pageMeta`. */
export function pageMeta(route: string, t: (k: string) => string): PageMeta {
  const erpMeta = ERP_ROUTE_META[route];
  if (erpMeta) {
    return {
      title: t(erpMeta.title),
      icon: erpMeta.icon,
      crumbs: [{ label: t(erpMeta.group) }, { label: t(erpMeta.title) }],
    };
  }
  if (route === 'home') {
    return { title: t('Dashboard'), icon: 'home', crumbs: [{ label: t('Dashboard') }] };
  }
  if (route === 'statistik') {
    return { title: t('Statistik'), icon: 'stats', crumbs: [{ label: t('Statistik') }] };
  }
  for (const top of NAV) {
    if (top.divider || !top.children) continue;
    const leaves: NavLeaf[] = isNavGroupArray(top.children)
      ? top.children.flatMap((g) => g.items)
      : top.children;
    const leaf = leaves.find((l) => l.id === route);
    if (leaf) {
      return {
        title: t(leaf.label),
        icon: top.icon ?? 'file',
        code: leaf.code,
        crumbs: [{ label: t(top.label ?? '') }, { label: t(leaf.label) }],
      };
    }
  }
  // Transaction sub-routes (<base>/new, <base>/:id) inherit the base meta
  // plus a form crumb — keeps breadcrumb/tab label clean for /new & /:id.
  const slash = route.lastIndexOf('/');
  if (slash > 0) {
    const baseMeta = ERP_ROUTE_META[route.slice(0, slash)];
    if (baseMeta) {
      const subLabel = route.slice(slash + 1) === 'new' ? t('Baru') : t('Edit');
      return {
        title: `${t(baseMeta.title)} · ${subLabel}`,
        icon: baseMeta.icon,
        crumbs: [
          { label: t(baseMeta.group) },
          { label: t(baseMeta.title) },
          { label: subLabel },
        ],
      };
    }
  }
  return { title: route, icon: 'file', crumbs: [{ label: route }] };
}
