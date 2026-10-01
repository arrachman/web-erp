/**
 * Bulk seed: isi Supplier & Salesman categories sampai 1000 baris per tipe.
 *
 * Konteks: tabel md_partner_sub_categories (type = SUPPLIER | SALESMAN) sudah
 * berisi 100 baris kurasi per tipe (seed-erp-md-supplier-categories.ts &
 * seed-erp-salesman-categories.ts). Script ini MENAMBAH 900 baris realistis
 * per tipe — dibentuk kombinatorial dari taksonomi nyata:
 *   - SUPPLIER : Asal/Origin (30) × Komoditas/Material (30) = 900
 *   - SALESMAN : Wilayah/Provinsi (30) × Segmen Saluran×Tier (30) = 900
 *
 * Namespace kode baru (SUPX- / SLMX-) tidak bertabrakan dengan kode kurasi
 * (LOK-/IMP-/CHN-/SLCAT-...), jadi total final = 1000 per tipe.
 *
 * Idempotent: upsert on (code, type) → aman dijalankan berulang.
 *
 * Run:  npx ts-node prisma/seed-erp-partner-categories-bulk.ts
 */
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

const TARGET_PER_TYPE = 1000;

// ── SUPPLIER: 30 asal × 30 komoditas ────────────────────────────────────────
const SUPPLIER_ORIGINS = [
  'Lokal Jabodetabek',
  'Lokal Jawa Barat',
  'Lokal Jawa Tengah',
  'Lokal Jawa Timur',
  'Lokal Sumatera',
  'Lokal Kalimantan',
  'Lokal Sulawesi',
  'Lokal Bali & Nusa Tenggara',
  'China',
  'Hong Kong',
  'Taiwan',
  'Jepang',
  'Korea Selatan',
  'India',
  'Vietnam',
  'Thailand',
  'Malaysia',
  'Singapura',
  'Filipina',
  'Jerman',
  'Italia',
  'Perancis',
  'Inggris',
  'Belanda',
  'Spanyol',
  'Amerika Serikat',
  'Kanada',
  'Australia',
  'Uni Emirat Arab',
  'Turki',
];

const SUPPLIER_COMMODITIES = [
  'Bahan Baku Logam',
  'Bahan Baku Plastik',
  'Bahan Baku Kimia',
  'Bahan Baku Tekstil',
  'Bahan Baku Kertas',
  'Komponen Elektronik',
  'Komponen Mekanik',
  'Suku Cadang Mesin',
  'Suku Cadang Kendaraan',
  'Kemasan Karton',
  'Kemasan Fleksibel',
  'Kemasan Botol & Jar',
  'Pelumas & Oli',
  'Bahan Kimia Industri',
  'Bahan Kimia Pembersih',
  'MRO & Perkakas',
  'Alat Ukur & Instrumen',
  'Mesin Produksi',
  'Tooling & Cetakan',
  'Bahan Bangunan',
  'Logam Non-Ferrous',
  'Karet & Elastomer',
  'Adhesive & Sealant',
  'Cat & Coating',
  'Label & Printing',
  'Energi & Bahan Bakar',
  'Produk Pertanian',
  'Kayu & Material Alam',
  'Tekstil & Garmen',
  'Perangkat IT & Jaringan',
];

// ── SALESMAN: 30 wilayah × 30 segmen ────────────────────────────────────────
const SALESMAN_REGIONS = [
  'Aceh',
  'Sumatera Utara',
  'Sumatera Barat',
  'Riau',
  'Kepulauan Riau',
  'Jambi',
  'Bengkulu',
  'Sumatera Selatan',
  'Bangka Belitung',
  'Lampung',
  'DKI Jakarta',
  'Banten',
  'Jawa Barat',
  'Jawa Tengah',
  'DI Yogyakarta',
  'Jawa Timur',
  'Bali',
  'Nusa Tenggara Barat',
  'Nusa Tenggara Timur',
  'Kalimantan Barat',
  'Kalimantan Tengah',
  'Kalimantan Selatan',
  'Kalimantan Timur',
  'Kalimantan Utara',
  'Sulawesi Utara',
  'Sulawesi Tengah',
  'Sulawesi Selatan',
  'Sulawesi Tenggara',
  'Maluku',
  'Papua',
];

const SALESMAN_SEGMENTS = [
  'Modern Trade - Platinum',
  'Modern Trade - Gold',
  'Modern Trade - Silver',
  'Traditional Trade - Gold',
  'Traditional Trade - Silver',
  'Traditional Trade - Bronze',
  'B2B Industri - Platinum',
  'B2B Industri - Gold',
  'B2B Industri - Silver',
  'Distributor - Gold',
  'Distributor - Silver',
  'Sub-Dealer - Bronze',
  'E-Commerce - Gold',
  'E-Commerce - Silver',
  'Project Sales - Platinum',
  'Project Sales - Gold',
  'Pemerintah & Tender - Gold',
  'Pemerintah & Tender - Silver',
  'Horeka - Gold',
  'Horeka - Silver',
  'Ekspor - Platinum',
  'Ekspor - Gold',
  'Direct Sales - Gold',
  'Direct Sales - Silver',
  'Korporat - Platinum',
  'Korporat - Gold',
  'UKM - Silver',
  'UKM - Bronze',
  'Farmasi & Rumah Sakit - Gold',
  'Konstruksi & Properti - Gold',
];

type Row = { code: string; name: string };

function buildCombos(prefix: string, rows: string[], cols: string[]): Row[] {
  const out: Row[] = [];
  rows.forEach((r, ri) => {
    cols.forEach((c, ci) => {
      const code = `${prefix}-${String(ri + 1).padStart(2, '0')}-${String(ci + 1).padStart(2, '0')}`;
      out.push({ code, name: `${r} — ${c}` });
    });
  });
  return out;
}

async function seedType(type: 'SUPPLIER' | 'SALESMAN', rows: Row[]): Promise<void> {
  const existing = await prisma.erpPartnerSubCategory.count({
    where: { type, deletedAt: null },
  });
  const capacity = Math.max(0, TARGET_PER_TYPE - existing);
  const slice = rows.slice(0, capacity);

  console.log(
    `[${type}] existing=${existing}, generating up to ${capacity} new (combos available=${rows.length})...`,
  );

  let upserted = 0;
  for (const row of slice) {
    await prisma.erpPartnerSubCategory.upsert({
      where: { code_type: { code: row.code, type } },
      create: { code: row.code, name: row.name, type, isActive: true },
      update: { name: row.name, isActive: true },
    });
    upserted++;
  }

  const total = await prisma.erpPartnerSubCategory.count({ where: { type, deletedAt: null } });
  console.log(`[${type}] upserted=${upserted}, total now=${total}`);
}

async function main() {
  const supplierCombos = buildCombos('SUPX', SUPPLIER_ORIGINS, SUPPLIER_COMMODITIES);
  const salesmanCombos = buildCombos('SLMX', SALESMAN_REGIONS, SALESMAN_SEGMENTS);

  await seedType('SUPPLIER', supplierCombos);
  await seedType('SALESMAN', salesmanCombos);

  console.log('Done.');
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(() => prisma.$disconnect());
