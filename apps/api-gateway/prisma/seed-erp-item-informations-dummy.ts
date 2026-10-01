/**
 * Seed 100 dummy rows into md_item_informations (1:1 extension of md_items).
 * Run: npx ts-node prisma/seed-erp-item-informations-dummy.ts
 *
 * Idempotent: skips itemIds that already have an information record.
 * Requires md_items to be populated (see seed-erp-md-dummy.ts).
 */
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();
const TARGET_COUNT = 100;

// ─── Reference data — tuned for a realistic manufacturing catalog ────────────

const MANUFACTURERS = [
  'Sentient Manufacturing', 'PT Cakra Industri', 'CV Maju Bersama',
  'PT Sinar Logam Jaya', 'CV Karya Mandiri', 'PT Inti Komponen Nusantara',
  'PT Bumi Tekstil', 'CV Andalan Plastik', 'PT Mega Elektronik',
  'PT Cipta Presisi', 'Toko Sumber Rejeki', 'PT Wira Kemasan',
  'CV Berkah Teknik', 'PT Lestari Furniture', 'PT Andalas Kimia',
];

const COUNTRIES = [
  'Indonesia', 'China', 'Japan', 'Germany', 'South Korea', 'Taiwan',
  'Vietnam', 'Thailand', 'India', 'Malaysia', 'United States', 'Italy',
];

const SPEC_TEMPLATES = [
  'Material: stainless steel SS304; Dimensi: 100×50×25 mm; Berat: 250 g',
  'Spec: tegangan 220V/50Hz; daya 1500W; sertifikasi SNI',
  'Komposisi: 80% katun, 20% polyester; lebar 150 cm; gramatur 180 gsm',
  'Resin grade ABS; warna natural; MFI 18 g/10 min; suhu kerja -40~80°C',
  'Toleransi ±0.05 mm; finishing anodized; kekuatan tarik 350 MPa',
  'Kapasitas 50 kg; akurasi 0.01 kg; tampilan LCD backlight',
  'Diameter 10 mm; panjang 1.2 m; bahan kuningan; ulir M10×1.5',
  'Kandungan utama: NaOH 99%; kemasan drum 50 L; pH > 13',
];

const NOTE_TEMPLATES = [
  'Stok cepat habis di musim panen; minimal order 50 unit.',
  'Sertifikat halal & SNI tersedia atas permintaan.',
  'Pengiriman ex-works; lead time 5-7 hari kerja.',
  'Wajib pakai APD saat handling (sarung tangan + masker).',
  'Bonus 1 pcs cadangan tiap 12 unit pembelian.',
  'Cocok untuk produksi batch < 1000 unit/hari.',
  'Disimpan di area kering, suhu < 30°C.',
  'Berat netto sudah termasuk kemasan dalam.',
];

const TAG_POOL = [
  'electronics', 'imported', 'fragile', 'flammable', 'food-grade',
  'hazardous', 'oversize', 'recycled', 'oem', 'premium', 'bulk',
  'export', 'local', 'fast-moving', 'seasonal', 'discontinued',
];

const LONG_DESC_TEMPLATES = [
  'Produk unggulan dengan kualitas premium yang sudah teruji di pasar industri Indonesia selama lebih dari 10 tahun. Cocok untuk aplikasi produksi massal maupun custom dengan tingkat presisi tinggi.',
  'Komponen kritis yang dirancang khusus untuk lingkungan manufaktur dengan beban kerja berat. Tahan terhadap getaran, suhu ekstrem, dan paparan bahan kimia industri ringan.',
  'Bahan baku berstandar internasional dengan sertifikasi mutu lengkap. Telah lulus uji laboratorium independen dan memenuhi spesifikasi yang ditetapkan oleh regulator industri.',
  'Item multi-fungsi yang banyak digunakan lintas departemen produksi, perawatan, dan logistik. Stok harus dijaga di atas safety level karena tingkat konsumsi yang tinggi.',
  'Spare part penting untuk mesin produksi utama. Disarankan untuk selalu memiliki cadangan minimal 2 unit per lokasi guna mengantisipasi downtime tak terduga.',
];

// ─── Random helpers ──────────────────────────────────────────────────────────

function pick<T>(arr: readonly T[], i: number): T {
  return arr[i % arr.length];
}

function pickRandom<T>(arr: readonly T[]): T {
  return arr[Math.floor(Math.random() * arr.length)];
}

function pickTags(count: number): string {
  const shuffled = [...TAG_POOL].sort(() => Math.random() - 0.5);
  return shuffled.slice(0, count).join(',');
}

// ─── Main ────────────────────────────────────────────────────────────────────

async function main(): Promise<void> {
  // Find items that don't yet have an information record.
  const itemsWithoutInfo = await prisma.erpItem.findMany({
    where: { deletedAt: null, information: { is: null } },
    select: { id: true, code: true, name: true },
    orderBy: { id: 'asc' },
    take: TARGET_COUNT,
  });

  if (itemsWithoutInfo.length === 0) {
    const totalInfo = await prisma.erpItemInformation.count({ where: { deletedAt: null } });
    const totalItems = await prisma.erpItem.count({ where: { deletedAt: null } });
    console.log(
      `[seed-erp-item-informations] no items without info found. ` +
      `Already have ${totalInfo} info records across ${totalItems} items.`,
    );
    return;
  }

  console.log(
    `[seed-erp-item-informations] found ${itemsWithoutInfo.length} items without info. ` +
    `Target=${TARGET_COUNT}. Seeding…`,
  );

  const rows = itemsWithoutInfo.map((it, i) => ({
    itemId: it.id,
    longDescription: pick(LONG_DESC_TEMPLATES, i),
    specifications: pick(SPEC_TEMPLATES, i),
    manufacturer: pick(MANUFACTURERS, i),
    countryOfOrigin: pick(COUNTRIES, i),
    warrantyPeriodMonths: [0, 3, 6, 12, 24, 36][i % 6],
    tags: pickTags(2 + (i % 3)),
    notes: pickRandom(NOTE_TEMPLATES),
  }));

  // `itemId` is @unique → createMany with skipDuplicates is safe even on re-run.
  const result = await prisma.erpItemInformation.createMany({
    data: rows,
    skipDuplicates: true,
  });

  console.log(
    `[seed-erp-item-informations] inserted ${result.count}/${rows.length} records.`,
  );
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(async () => {
    await prisma.$disconnect();
  });
