/**
 * Seed proper locations & warehouses for md_locations + md_warehouses.
 * Run: npx ts-node -r tsconfig-paths/register prisma/seed-md-warehouses.ts
 *
 * Steps:
 *  1. Delete dummy warehouses (DUMMY-WH-*) and dummy locations (DUMMY-LOC-*)
 *  2. Upsert 17 real locations across 12 real branches
 *  3. Upsert 100 real warehouses
 */

import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

// ─── Location definitions (17 lokasi nyata) ──────────────────────────────────

interface LocationDef {
  code: string;
  name: string;
  branchCode: string;
  addressLine1?: string;
  city?: string;
  postalCode?: string;
  phone?: string;
  notes?: string;
}

const LOCATION_DEFS: LocationDef[] = [
  // HQ — 3 lokasi (pabrik terbesar)
  { code: 'LOC-HQ-01', name: 'Pabrik Utama Jakarta Timur', branchCode: 'HQ',
    addressLine1: 'Jl. Raya Bekasi KM 18, Cakung', city: 'Jakarta Timur', postalCode: '13920', phone: '021-48930001' },
  { code: 'LOC-HQ-02', name: 'Gudang Pusat Jakarta Utara', branchCode: 'HQ',
    addressLine1: 'Kawasan Industri Marunda Blok D2', city: 'Jakarta Utara', postalCode: '14150', phone: '021-44130002' },
  { code: 'LOC-HQ-03', name: 'Pabrik 2 Cikarang Barat', branchCode: 'HQ',
    addressLine1: 'EJIP Industrial Park Plot 4E', city: 'Cikarang Barat', postalCode: '17530', phone: '021-89830003' },
  // BDG — 2 lokasi
  { code: 'LOC-BDG-01', name: 'Pabrik Bandung Rancaekek', branchCode: 'BDG',
    addressLine1: 'Jl. Raya Rancaekek KM 25', city: 'Bandung', postalCode: '40614', phone: '022-77980001' },
  { code: 'LOC-BDG-02', name: 'Gudang Bandung Gedebage', branchCode: 'BDG',
    addressLine1: 'Kawasan Industri Gedebage Blok A3', city: 'Bandung', postalCode: '40293', phone: '022-87540002' },
  // SBY — 2 lokasi
  { code: 'LOC-SBY-01', name: 'Pabrik Surabaya SIER Rungkut', branchCode: 'SBY',
    addressLine1: 'SIER Rungkut Industri IX No. 5', city: 'Surabaya', postalCode: '60293', phone: '031-84310001' },
  { code: 'LOC-SBY-02', name: 'Gudang Surabaya Margomulyo', branchCode: 'SBY',
    addressLine1: 'Jl. Margomulyo Indah Blok D7', city: 'Surabaya', postalCode: '60183', phone: '031-74910002' },
  // MKS — 1 lokasi
  { code: 'LOC-MKS-01', name: 'Pabrik Makassar KIMA', branchCode: 'MKS',
    addressLine1: 'Kawasan Industri Makassar (KIMA) Blok M5', city: 'Makassar', postalCode: '90242', phone: '0411-51130001' },
  // MDN — 2 lokasi
  { code: 'LOC-MDN-01', name: 'Pabrik Medan KIM', branchCode: 'MDN',
    addressLine1: 'Kawasan Industri Medan (KIM) Blok G5', city: 'Medan', postalCode: '20242', phone: '061-68710001' },
  { code: 'LOC-MDN-02', name: 'Gudang Medan Belawan', branchCode: 'MDN',
    addressLine1: 'Jl. Pelabuhan Belawan KM 3', city: 'Medan', postalCode: '20411', phone: '061-69420002' },
  // SMG — 1 lokasi
  { code: 'LOC-SMG-01', name: 'Pabrik Semarang Kawasan Terboyo', branchCode: 'SMG',
    addressLine1: 'Kawasan Industri Terboyo Blok H12', city: 'Semarang', postalCode: '50196', phone: '024-65810001' },
  // YGY — 1 lokasi
  { code: 'LOC-YGY-01', name: 'Pabrik Yogyakarta Maguwoharjo', branchCode: 'YGY',
    addressLine1: 'Jl. Raya Solo KM 9, Maguwoharjo', city: 'Sleman', postalCode: '55282', phone: '0274-45620001' },
  // PLM — 1 lokasi
  { code: 'LOC-PLM-01', name: 'Pabrik Palembang Keramasan', branchCode: 'PLM',
    addressLine1: 'Jl. Keramasan Raya No. 12', city: 'Palembang', postalCode: '30154', phone: '0711-59210001' },
  // DPS — 1 lokasi
  { code: 'LOC-DPS-01', name: 'Pabrik Denpasar Mengwi', branchCode: 'DPS',
    addressLine1: 'Kawasan Industri Mengwi Blok C2', city: 'Badung', postalCode: '80351', phone: '0361-82320001' },
  // BPN — 1 lokasi
  { code: 'LOC-BPN-01', name: 'Pabrik Balikpapan Kariangau', branchCode: 'BPN',
    addressLine1: 'Kawasan Industri Kariangau No. 8', city: 'Balikpapan', postalCode: '76115', phone: '0542-77120001' },
  // PKU — 1 lokasi
  { code: 'LOC-PKU-01', name: 'Pabrik Pekanbaru Tenayan Raya', branchCode: 'PKU',
    addressLine1: 'Kawasan Industri Tenayan Raya Blok B3', city: 'Pekanbaru', postalCode: '28291', phone: '0761-88230001' },
  // PNK — 1 lokasi
  { code: 'LOC-PNK-01', name: 'Pabrik Pontianak Sungai Raya', branchCode: 'PNK',
    addressLine1: 'Jl. Trans Kalimantan KM 8, Sungai Raya', city: 'Kubu Raya', postalCode: '78391', phone: '0561-77430001' },
];

// ─── Warehouse definitions (100 gudang nyata) ─────────────────────────────────

interface WarehouseDef {
  code: string;
  name: string;
  locationCode: string;
  allowNegativeStock?: boolean;
  notes?: string;
  legacyCode?: string;
}

// Tipe gudang yang umum di perusahaan manufaktur Indonesia:
//   BB = Bahan Baku       | BJ = Barang Jadi       | BSJ = Barang Setengah Jadi
//   TR = Transit          | SP = Spare Part        | KM = Kemasan
//   B3 = Bahan B3         | RJ = Reject / Return   | EXP = Ekspor
//   AT = Alat & Mesin     | OFC = Kantor / Supplies

const WAREHOUSE_DEFS: WarehouseDef[] = [
  // ── HQ Pabrik Utama Jakarta Timur (LOC-HQ-01) — 8 gudang ─────────────────
  { code: 'GD-HQ-BB-01', name: 'Gudang Bahan Baku 1 — HQ',         locationCode: 'LOC-HQ-01', notes: 'Bahan baku utama lini produksi 1-4' },
  { code: 'GD-HQ-BB-02', name: 'Gudang Bahan Baku 2 — HQ',         locationCode: 'LOC-HQ-01', notes: 'Bahan baku lini produksi 5-8' },
  { code: 'GD-HQ-BSJ-01', name: 'Gudang Barang Setengah Jadi 1 — HQ', locationCode: 'LOC-HQ-01', notes: 'WIP lini 1-4' },
  { code: 'GD-HQ-BSJ-02', name: 'Gudang Barang Setengah Jadi 2 — HQ', locationCode: 'LOC-HQ-01', notes: 'WIP lini 5-8' },
  { code: 'GD-HQ-BJ-01',  name: 'Gudang Barang Jadi A — HQ',        locationCode: 'LOC-HQ-01', notes: 'FG SKU reguler' },
  { code: 'GD-HQ-KM-01',  name: 'Gudang Kemasan — HQ',              locationCode: 'LOC-HQ-01', notes: 'Kardus, plastik, label, inner box' },
  { code: 'GD-HQ-B3-01',  name: 'Gudang Bahan B3 — HQ',             locationCode: 'LOC-HQ-01', notes: 'Solvent, cat, bahan kimia berbahaya', legacyCode: 'B3-HQ' },
  { code: 'GD-HQ-SP-01',  name: 'Gudang Spare Part — HQ',           locationCode: 'LOC-HQ-01', notes: 'Suku cadang mesin produksi' },

  // ── HQ Gudang Pusat Jakarta Utara (LOC-HQ-02) — 6 gudang ─────────────────
  { code: 'GD-HQ-BJ-02',  name: 'Gudang Barang Jadi B — HQ',        locationCode: 'LOC-HQ-02', notes: 'FG SKU ekspor & distribusi nasional' },
  { code: 'GD-HQ-TR-01',  name: 'Gudang Transit Tanjung Priok',      locationCode: 'LOC-HQ-02', allowNegativeStock: true, notes: 'Buffer sebelum pengiriman ke pelabuhan' },
  { code: 'GD-HQ-EXP-01', name: 'Gudang Ekspor — HQ',               locationCode: 'LOC-HQ-02', notes: 'Staging area produk ekspor' },
  { code: 'GD-HQ-RJ-01',  name: 'Gudang Reject & Retur — HQ',       locationCode: 'LOC-HQ-02', allowNegativeStock: true, notes: 'Barang retur customer + barang reject QC' },
  { code: 'GD-HQ-OFC-01', name: 'Gudang ATK & Perlengkapan Kantor',  locationCode: 'LOC-HQ-02', notes: 'Alat tulis kantor, supplies' },
  { code: 'GD-HQ-AT-01',  name: 'Gudang Alat & Mesin — HQ',         locationCode: 'LOC-HQ-02', notes: 'Peralatan maintenance & tooling' },

  // ── HQ Pabrik 2 Cikarang (LOC-HQ-03) — 6 gudang ──────────────────────────
  { code: 'GD-CKR-BB-01', name: 'Gudang Bahan Baku — Cikarang',     locationCode: 'LOC-HQ-03', notes: 'BB khusus lini otomotif' },
  { code: 'GD-CKR-BSJ-01', name: 'Gudang WIP — Cikarang',           locationCode: 'LOC-HQ-03', notes: 'Komponen setengah jadi lini otomotif' },
  { code: 'GD-CKR-BJ-01', name: 'Gudang Barang Jadi — Cikarang',    locationCode: 'LOC-HQ-03', notes: 'FG lini otomotif Cikarang' },
  { code: 'GD-CKR-KM-01', name: 'Gudang Kemasan — Cikarang',        locationCode: 'LOC-HQ-03', notes: 'Kemasan khusus komponen otomotif' },
  { code: 'GD-CKR-SP-01', name: 'Gudang Spare Part — Cikarang',     locationCode: 'LOC-HQ-03', notes: 'Suku cadang mesin lini Cikarang' },
  { code: 'GD-CKR-TR-01', name: 'Gudang Transit — Cikarang',        locationCode: 'LOC-HQ-03', allowNegativeStock: true, notes: 'Transfer antar pabrik HQ-CKR' },

  // ── Bandung Rancaekek (LOC-BDG-01) — 6 gudang ────────────────────────────
  { code: 'GD-BDG-BB-01', name: 'Gudang Bahan Baku 1 — Bandung',    locationCode: 'LOC-BDG-01', notes: 'BB tekstil & garmen' },
  { code: 'GD-BDG-BB-02', name: 'Gudang Bahan Baku 2 — Bandung',    locationCode: 'LOC-BDG-01', notes: 'BB bahan kimia tekstil' },
  { code: 'GD-BDG-BSJ-01', name: 'Gudang WIP — Bandung',            locationCode: 'LOC-BDG-01', notes: 'Kain setengah jadi, benang' },
  { code: 'GD-BDG-BJ-01', name: 'Gudang Barang Jadi — Bandung',     locationCode: 'LOC-BDG-01', notes: 'Produk jadi garmen ekspor' },
  { code: 'GD-BDG-KM-01', name: 'Gudang Kemasan — Bandung',         locationCode: 'LOC-BDG-01', notes: 'Polybag, hang tag, label garmen' },
  { code: 'GD-BDG-SP-01', name: 'Gudang Spare Part — Bandung',      locationCode: 'LOC-BDG-01', notes: 'Suku cadang mesin jahit & tenun' },

  // ── Bandung Gedebage (LOC-BDG-02) — 4 gudang ─────────────────────────────
  { code: 'GD-BDG-TR-01', name: 'Gudang Transit Bandung',           locationCode: 'LOC-BDG-02', allowNegativeStock: true, notes: 'Buffer distribusi Jabodetabek & Jabar' },
  { code: 'GD-BDG-RJ-01', name: 'Gudang Reject — Bandung',         locationCode: 'LOC-BDG-02', notes: 'Garmen reject QC' },
  { code: 'GD-BDG-EXP-01', name: 'Gudang Ekspor — Bandung',        locationCode: 'LOC-BDG-02', notes: 'Staging ekspor via Tanjung Priok' },
  { code: 'GD-BDG-AT-01', name: 'Gudang Alat — Bandung',           locationCode: 'LOC-BDG-02', notes: 'Peralatan & tooling pabrik Bandung' },

  // ── Surabaya SIER (LOC-SBY-01) — 7 gudang ────────────────────────────────
  { code: 'GD-SBY-BB-01', name: 'Gudang Bahan Baku 1 — Surabaya',  locationCode: 'LOC-SBY-01', notes: 'BB plastik & petrokimia' },
  { code: 'GD-SBY-BB-02', name: 'Gudang Bahan Baku 2 — Surabaya',  locationCode: 'LOC-SBY-01', notes: 'BB logam & baja' },
  { code: 'GD-SBY-BSJ-01', name: 'Gudang WIP — Surabaya',          locationCode: 'LOC-SBY-01', notes: 'Komponen setengah jadi lini plastik' },
  { code: 'GD-SBY-BJ-01', name: 'Gudang Barang Jadi A — Surabaya', locationCode: 'LOC-SBY-01', notes: 'FG distribusi Jatim & Jateng' },
  { code: 'GD-SBY-KM-01', name: 'Gudang Kemasan — Surabaya',       locationCode: 'LOC-SBY-01', notes: 'Kemasan plastik & karton' },
  { code: 'GD-SBY-B3-01', name: 'Gudang B3 — Surabaya',            locationCode: 'LOC-SBY-01', notes: 'Cairan kimia proses produksi', legacyCode: 'B3-SBY' },
  { code: 'GD-SBY-SP-01', name: 'Gudang Spare Part — Surabaya',    locationCode: 'LOC-SBY-01', notes: 'Suku cadang mesin injection' },

  // ── Surabaya Margomulyo (LOC-SBY-02) — 4 gudang ──────────────────────────
  { code: 'GD-SBY-TR-01', name: 'Gudang Transit Surabaya',         locationCode: 'LOC-SBY-02', allowNegativeStock: true, notes: 'Buffer distribusi Jatim & Indonesia Timur' },
  { code: 'GD-SBY-BJ-02', name: 'Gudang Barang Jadi B — Surabaya', locationCode: 'LOC-SBY-02', notes: 'FG ekspor via Tanjung Perak' },
  { code: 'GD-SBY-EXP-01', name: 'Gudang Ekspor — Surabaya',       locationCode: 'LOC-SBY-02', notes: 'Staging ekspor Tanjung Perak' },
  { code: 'GD-SBY-RJ-01', name: 'Gudang Reject — Surabaya',        locationCode: 'LOC-SBY-02', notes: 'Return & reject wilayah Jatim' },

  // ── Makassar KIMA (LOC-MKS-01) — 6 gudang ────────────────────────────────
  { code: 'GD-MKS-BB-01', name: 'Gudang Bahan Baku — Makassar',    locationCode: 'LOC-MKS-01', notes: 'BB hasil perkebunan & agro-industri' },
  { code: 'GD-MKS-BSJ-01', name: 'Gudang WIP — Makassar',          locationCode: 'LOC-MKS-01', notes: 'Produk olahan setengah jadi' },
  { code: 'GD-MKS-BJ-01', name: 'Gudang Barang Jadi — Makassar',   locationCode: 'LOC-MKS-01', notes: 'FG distribusi KTI (Kawasan Timur Indonesia)' },
  { code: 'GD-MKS-KM-01', name: 'Gudang Kemasan — Makassar',       locationCode: 'LOC-MKS-01', notes: 'Kemasan produk agro-industri' },
  { code: 'GD-MKS-TR-01', name: 'Gudang Transit Makassar',         locationCode: 'LOC-MKS-01', allowNegativeStock: true, notes: 'Hub distribusi Indonesia Timur' },
  { code: 'GD-MKS-SP-01', name: 'Gudang Spare Part — Makassar',    locationCode: 'LOC-MKS-01', notes: 'Suku cadang mesin pengolahan' },

  // ── Medan KIM (LOC-MDN-01) — 5 gudang ────────────────────────────────────
  { code: 'GD-MDN-BB-01', name: 'Gudang Bahan Baku — Medan',       locationCode: 'LOC-MDN-01', notes: 'BB karet, kelapa sawit, pulp' },
  { code: 'GD-MDN-BSJ-01', name: 'Gudang WIP — Medan',             locationCode: 'LOC-MDN-01', notes: 'Produk olahan setengah jadi Sumatera' },
  { code: 'GD-MDN-BJ-01', name: 'Gudang Barang Jadi — Medan',      locationCode: 'LOC-MDN-01', notes: 'FG distribusi Sumatera Utara & Aceh' },
  { code: 'GD-MDN-KM-01', name: 'Gudang Kemasan — Medan',          locationCode: 'LOC-MDN-01', notes: 'Kemasan produk Sumatera' },
  { code: 'GD-MDN-SP-01', name: 'Gudang Spare Part — Medan',       locationCode: 'LOC-MDN-01', notes: 'Spare part mesin pengolahan Sumatera' },

  // ── Medan Belawan (LOC-MDN-02) — 4 gudang ────────────────────────────────
  { code: 'GD-MDN-TR-01', name: 'Gudang Transit Belawan',          locationCode: 'LOC-MDN-02', allowNegativeStock: true, notes: 'Hub distribusi & ekspor via Belawan' },
  { code: 'GD-MDN-EXP-01', name: 'Gudang Ekspor — Medan',          locationCode: 'LOC-MDN-02', notes: 'Staging ekspor via Pelabuhan Belawan' },
  { code: 'GD-MDN-RJ-01', name: 'Gudang Reject — Medan',           locationCode: 'LOC-MDN-02', notes: 'Retur & reject Sumatera' },
  { code: 'GD-MDN-OFC-01', name: 'Gudang ATK — Medan',             locationCode: 'LOC-MDN-02', notes: 'Perlengkapan kantor wilayah Sumatera' },

  // ── Semarang Terboyo (LOC-SMG-01) — 6 gudang ─────────────────────────────
  { code: 'GD-SMG-BB-01', name: 'Gudang Bahan Baku — Semarang',    locationCode: 'LOC-SMG-01', notes: 'BB industri kayu & furniture' },
  { code: 'GD-SMG-BSJ-01', name: 'Gudang WIP — Semarang',          locationCode: 'LOC-SMG-01', notes: 'Komponen furniture setengah jadi' },
  { code: 'GD-SMG-BJ-01', name: 'Gudang Barang Jadi — Semarang',   locationCode: 'LOC-SMG-01', notes: 'FG furniture distribusi Jateng & DIY' },
  { code: 'GD-SMG-KM-01', name: 'Gudang Kemasan — Semarang',       locationCode: 'LOC-SMG-01', notes: 'Kemasan produk furniture' },
  { code: 'GD-SMG-TR-01', name: 'Gudang Transit Semarang',         locationCode: 'LOC-SMG-01', allowNegativeStock: true, notes: 'Buffer distribusi Jateng' },
  { code: 'GD-SMG-SP-01', name: 'Gudang Spare Part — Semarang',    locationCode: 'LOC-SMG-01', notes: 'Spare part mesin CNC & woodworking' },

  // ── Yogyakarta Maguwoharjo (LOC-YGY-01) — 5 gudang ──────────────────────
  { code: 'GD-YGY-BB-01', name: 'Gudang Bahan Baku — Yogyakarta',  locationCode: 'LOC-YGY-01', notes: 'BB kerajinan perak & batik' },
  { code: 'GD-YGY-BSJ-01', name: 'Gudang WIP — Yogyakarta',        locationCode: 'LOC-YGY-01', notes: 'Produk kerajinan setengah jadi' },
  { code: 'GD-YGY-BJ-01', name: 'Gudang Barang Jadi — Yogyakarta', locationCode: 'LOC-YGY-01', notes: 'FG kerajinan & batik ekspor + lokal' },
  { code: 'GD-YGY-KM-01', name: 'Gudang Kemasan — Yogyakarta',     locationCode: 'LOC-YGY-01', notes: 'Kemasan gift box premium' },
  { code: 'GD-YGY-TR-01', name: 'Gudang Transit — Yogyakarta',     locationCode: 'LOC-YGY-01', allowNegativeStock: true, notes: 'Buffer distribusi DIY & Jateng Selatan' },

  // ── Palembang Keramasan (LOC-PLM-01) — 5 gudang ──────────────────────────
  { code: 'GD-PLM-BB-01', name: 'Gudang Bahan Baku — Palembang',   locationCode: 'LOC-PLM-01', notes: 'BB karet alam & petrokimia' },
  { code: 'GD-PLM-BSJ-01', name: 'Gudang WIP — Palembang',         locationCode: 'LOC-PLM-01', notes: 'Produk karet setengah jadi' },
  { code: 'GD-PLM-BJ-01', name: 'Gudang Barang Jadi — Palembang',  locationCode: 'LOC-PLM-01', notes: 'FG distribusi Sumatera Selatan & Babel' },
  { code: 'GD-PLM-TR-01', name: 'Gudang Transit Palembang',        locationCode: 'LOC-PLM-01', allowNegativeStock: true, notes: 'Buffer distribusi Sumsel & ekspor' },
  { code: 'GD-PLM-SP-01', name: 'Gudang Spare Part — Palembang',   locationCode: 'LOC-PLM-01', notes: 'Spare part mesin karet' },

  // ── Denpasar Mengwi (LOC-DPS-01) — 5 gudang ──────────────────────────────
  { code: 'GD-DPS-BB-01', name: 'Gudang Bahan Baku — Denpasar',    locationCode: 'LOC-DPS-01', notes: 'BB handicraft & produk kreatif Bali' },
  { code: 'GD-DPS-BSJ-01', name: 'Gudang WIP — Denpasar',          locationCode: 'LOC-DPS-01', notes: 'Produk handicraft setengah jadi' },
  { code: 'GD-DPS-BJ-01', name: 'Gudang Barang Jadi — Denpasar',   locationCode: 'LOC-DPS-01', notes: 'FG handicraft + home decor ekspor' },
  { code: 'GD-DPS-EXP-01', name: 'Gudang Ekspor — Denpasar',       locationCode: 'LOC-DPS-01', notes: 'Staging ekspor via Ngurah Rai' },
  { code: 'GD-DPS-TR-01', name: 'Gudang Transit Denpasar',         locationCode: 'LOC-DPS-01', allowNegativeStock: true, notes: 'Buffer distribusi Bali & Nusa Tenggara' },

  // ── Balikpapan Kariangau (LOC-BPN-01) — 5 gudang ─────────────────────────
  { code: 'GD-BPN-BB-01', name: 'Gudang Bahan Baku — Balikpapan',  locationCode: 'LOC-BPN-01', notes: 'BB industri tambang & energi' },
  { code: 'GD-BPN-BSJ-01', name: 'Gudang WIP — Balikpapan',        locationCode: 'LOC-BPN-01', notes: 'Komponen setengah jadi energi' },
  { code: 'GD-BPN-BJ-01', name: 'Gudang Barang Jadi — Balikpapan', locationCode: 'LOC-BPN-01', notes: 'FG distribusi Kaltim & Kaltara' },
  { code: 'GD-BPN-TR-01', name: 'Gudang Transit Balikpapan',       locationCode: 'LOC-BPN-01', allowNegativeStock: true, notes: 'Hub distribusi Kalimantan Timur' },
  { code: 'GD-BPN-SP-01', name: 'Gudang Spare Part — Balikpapan',  locationCode: 'LOC-BPN-01', notes: 'Spare part alat berat & energi' },

  // ── Pekanbaru Tenayan (LOC-PKU-01) — 4 gudang ────────────────────────────
  { code: 'GD-PKU-BB-01', name: 'Gudang Bahan Baku — Pekanbaru',   locationCode: 'LOC-PKU-01', notes: 'BB industri kertas & pulp' },
  { code: 'GD-PKU-BJ-01', name: 'Gudang Barang Jadi — Pekanbaru',  locationCode: 'LOC-PKU-01', notes: 'FG distribusi Riau & Kepri' },
  { code: 'GD-PKU-TR-01', name: 'Gudang Transit Pekanbaru',        locationCode: 'LOC-PKU-01', allowNegativeStock: true, notes: 'Buffer distribusi Sumatera Tengah' },
  { code: 'GD-PKU-SP-01', name: 'Gudang Spare Part — Pekanbaru',   locationCode: 'LOC-PKU-01', notes: 'Spare part mesin kertas & pulp' },

  // ── Pontianak Sungai Raya (LOC-PNK-01) — 4 gudang ────────────────────────
  { code: 'GD-PNK-BB-01', name: 'Gudang Bahan Baku — Pontianak',   locationCode: 'LOC-PNK-01', notes: 'BB industri pengolahan kayu Kalimantan' },
  { code: 'GD-PNK-BJ-01', name: 'Gudang Barang Jadi — Pontianak',  locationCode: 'LOC-PNK-01', notes: 'FG distribusi Kalbar & ekspor via Pontianak' },
  { code: 'GD-PNK-TR-01', name: 'Gudang Transit Pontianak',        locationCode: 'LOC-PNK-01', allowNegativeStock: true, notes: 'Hub distribusi Kalimantan Barat' },
  { code: 'GD-PNK-SP-01', name: 'Gudang Spare Part — Pontianak',   locationCode: 'LOC-PNK-01', notes: 'Spare part mesin pengolahan kayu' },

  // ── Tambahan untuk genap 100 ──────────────────────────────────────────────
  { code: 'GD-HQ-BB-03',  name: 'Gudang Bahan Baku 3 — HQ',        locationCode: 'LOC-HQ-01', notes: 'BB lini elektronik & PCB' },
  { code: 'GD-HQ-AT-02',  name: 'Gudang Alat & Tooling 2 — HQ',    locationCode: 'LOC-HQ-01', notes: 'Cetakan, jig, fixture lini 5-8' },
  { code: 'GD-CKR-RJ-01', name: 'Gudang Reject — Cikarang',        locationCode: 'LOC-HQ-03', notes: 'Komponen NG & retur lini otomotif' },
  { code: 'GD-SBY-BB-03', name: 'Gudang Bahan Baku 3 — Surabaya',  locationCode: 'LOC-SBY-01', notes: 'BB kabel & komponen elektrikal' },
  { code: 'GD-BDG-B3-01', name: 'Gudang B3 — Bandung',             locationCode: 'LOC-BDG-01', notes: 'Zat pewarna & kimia tekstil', legacyCode: 'B3-BDG' },
  { code: 'GD-MDN-B3-01', name: 'Gudang B3 — Medan',               locationCode: 'LOC-MDN-01', notes: 'Bahan kimia proses karet & pulp', legacyCode: 'B3-MDN' },
  { code: 'GD-PKU-BSJ-01', name: 'Gudang WIP — Pekanbaru',         locationCode: 'LOC-PKU-01', notes: 'Pulp setengah jadi sebelum finishing' },
  { code: 'GD-PKU-KM-01', name: 'Gudang Kemasan — Pekanbaru',      locationCode: 'LOC-PKU-01', notes: 'Kemasan kertas & wrapping' },
  { code: 'GD-PNK-BSJ-01', name: 'Gudang WIP — Pontianak',         locationCode: 'LOC-PNK-01', notes: 'Kayu olahan setengah jadi' },
  { code: 'GD-PNK-KM-01', name: 'Gudang Kemasan — Pontianak',      locationCode: 'LOC-PNK-01', notes: 'Kemasan produk kayu & plywood' },
];

// ─── Main ─────────────────────────────────────────────────────────────────────

async function main() {
  console.log('=== Seed md_locations + md_warehouses ===\n');

  // 1. Delete dummy warehouses (FK dependency first)
  const deletedWh = await prisma.erpWarehouse.deleteMany({
    where: { code: { startsWith: 'DUMMY-WH-' } },
  });
  console.log(`🗑  Deleted ${deletedWh.count} dummy warehouses`);

  const deletedLoc = await prisma.erpLocation.deleteMany({
    where: { code: { startsWith: 'DUMMY-LOC-' } },
  });
  console.log(`🗑  Deleted ${deletedLoc.count} dummy locations\n`);

  // 2. Build branch code → id map
  const branches = await prisma.erpBranch.findMany({
    where: { deletedAt: null },
    select: { id: true, code: true },
  });
  const branchMap = new Map<string, bigint>(branches.map((b) => [b.code, b.id]));

  // 3. Upsert locations
  let locCreated = 0;
  let locUpdated = 0;
  const locationMap = new Map<string, bigint>(); // code → id

  for (const def of LOCATION_DEFS) {
    const branchId = branchMap.get(def.branchCode);
    if (!branchId) {
      console.warn(`  ⚠ Branch ${def.branchCode} not found — skip location ${def.code}`);
      continue;
    }
    const payload = {
      name: def.name,
      branchId,
      addressLine1: def.addressLine1 ?? null,
      city: def.city ?? null,
      postalCode: def.postalCode ?? null,
      phone: def.phone ?? null,
      notes: def.notes ?? null,
      isActive: true,
    };
    const existing = await prisma.erpLocation.findUnique({ where: { code: def.code } });
    if (existing) {
      await prisma.erpLocation.update({ where: { id: existing.id }, data: payload });
      locationMap.set(def.code, existing.id);
      locUpdated++;
    } else {
      const created = await prisma.erpLocation.create({ data: { code: def.code, ...payload } });
      locationMap.set(def.code, created.id);
      locCreated++;
    }
  }
  console.log(`✅ Locations: ${locCreated} created, ${locUpdated} updated\n`);

  // 4. Upsert warehouses
  let whCreated = 0;
  let whUpdated = 0;

  for (const def of WAREHOUSE_DEFS) {
    const locationId = locationMap.get(def.locationCode);
    if (!locationId) {
      console.warn(`  ⚠ Location ${def.locationCode} not found — skip warehouse ${def.code}`);
      continue;
    }
    const payload = {
      name: def.name,
      locationId,
      allowNegativeStock: def.allowNegativeStock ?? false,
      notes: def.notes ?? null,
      legacyCode: def.legacyCode ?? null,
      isActive: true,
    };
    const existing = await prisma.erpWarehouse.findUnique({ where: { code: def.code } });
    if (existing) {
      await prisma.erpWarehouse.update({ where: { id: existing.id }, data: payload });
      whUpdated++;
    } else {
      await prisma.erpWarehouse.create({ data: { code: def.code, ...payload } });
      whCreated++;
    }
  }
  console.log(`✅ Warehouses: ${whCreated} created, ${whUpdated} updated`);
  console.log(`\nTotal warehouses: ${WAREHOUSE_DEFS.length}`);
  console.log(`Total locations: ${LOCATION_DEFS.length}`);
}

main()
  .catch((e) => { console.error(e); process.exit(1); })
  .finally(() => prisma.$disconnect());
