/**
 * Seed 100 Salesman Categories — type = SALESMAN
 * Kategori berbasis wilayah, tier kinerja, saluran, dan spesialisasi produk
 * untuk konteks perusahaan manufaktur Indonesia.
 */
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

const SALESMAN_CATEGORIES = [
  // === WILAYAH — Pulau/Region (20) ===
  { code: 'SLCAT-WIL-JAV-W', name: 'Jawa Barat' },
  { code: 'SLCAT-WIL-JAV-C', name: 'Jawa Tengah' },
  { code: 'SLCAT-WIL-JAV-E', name: 'Jawa Timur' },
  { code: 'SLCAT-WIL-JAK', name: 'DKI Jakarta' },
  { code: 'SLCAT-WIL-BAL', name: 'Bali & Nusa Tenggara' },
  { code: 'SLCAT-WIL-SUM-N', name: 'Sumatera Utara' },
  { code: 'SLCAT-WIL-SUM-S', name: 'Sumatera Selatan' },
  { code: 'SLCAT-WIL-SUM-W', name: 'Sumatera Barat' },
  { code: 'SLCAT-WIL-RIU', name: 'Riau & Kepri' },
  { code: 'SLCAT-WIL-ACH', name: 'Aceh' },
  { code: 'SLCAT-WIL-KAL-W', name: 'Kalimantan Barat' },
  { code: 'SLCAT-WIL-KAL-E', name: 'Kalimantan Timur & Utara' },
  { code: 'SLCAT-WIL-KAL-S', name: 'Kalimantan Selatan & Tengah' },
  { code: 'SLCAT-WIL-SUL-N', name: 'Sulawesi Utara & Tengah' },
  { code: 'SLCAT-WIL-SUL-S', name: 'Sulawesi Selatan' },
  { code: 'SLCAT-WIL-SUL-SE', name: 'Sulawesi Tenggara' },
  { code: 'SLCAT-WIL-MAL', name: 'Maluku & Maluku Utara' },
  { code: 'SLCAT-WIL-PAP', name: 'Papua & Papua Barat' },
  { code: 'SLCAT-WIL-NTT', name: 'Nusa Tenggara Timur' },
  { code: 'SLCAT-WIL-BAN', name: 'Banten' },

  // === KOTA TIER-1 (10) ===
  { code: 'SLCAT-KOT-JKT', name: 'Kota Jakarta' },
  { code: 'SLCAT-KOT-SBY', name: 'Kota Surabaya' },
  { code: 'SLCAT-KOT-BDG', name: 'Kota Bandung' },
  { code: 'SLCAT-KOT-MDN', name: 'Kota Medan' },
  { code: 'SLCAT-KOT-SMG', name: 'Kota Semarang' },
  { code: 'SLCAT-KOT-MKS', name: 'Kota Makassar' },
  { code: 'SLCAT-KOT-PLG', name: 'Kota Palembang' },
  { code: 'SLCAT-KOT-PKB', name: 'Kota Pekanbaru' },
  { code: 'SLCAT-KOT-BTM', name: 'Kota Batam' },
  { code: 'SLCAT-KOT-BLI', name: 'Kota Denpasar & Badung' },

  // === TIER KINERJA (6) ===
  { code: 'SLCAT-TIR-PLT', name: 'Platinum' },
  { code: 'SLCAT-TIR-GLD', name: 'Gold' },
  { code: 'SLCAT-TIR-SLV', name: 'Silver' },
  { code: 'SLCAT-TIR-BRZ', name: 'Bronze' },
  { code: 'SLCAT-TIR-STD', name: 'Standard' },
  { code: 'SLCAT-TIR-NEW', name: 'New Salesman' },

  // === SALURAN PENJUALAN (10) ===
  { code: 'SLCAT-CHN-RTL', name: 'Retail Modern Trade' },
  { code: 'SLCAT-CHN-TRD', name: 'Traditional Trade' },
  { code: 'SLCAT-CHN-B2B', name: 'B2B / Industri' },
  { code: 'SLCAT-CHN-EXP', name: 'Ekspor' },
  { code: 'SLCAT-CHN-ONL', name: 'Online / E-Commerce' },
  { code: 'SLCAT-CHN-GVT', name: 'Pemerintah & Tender' },
  { code: 'SLCAT-CHN-DST', name: 'Distributor' },
  { code: 'SLCAT-CHN-SUB', name: 'Sub-Dealer' },
  { code: 'SLCAT-CHN-DIR', name: 'Direct Sales' },
  { code: 'SLCAT-CHN-PRJ', name: 'Project Sales' },

  // === SPESIALISASI PRODUK — Manufaktur (15) ===
  { code: 'SLCAT-PRD-FIN', name: 'Produk Jadi (Finished Goods)' },
  { code: 'SLCAT-PRD-RAW', name: 'Bahan Baku' },
  { code: 'SLCAT-PRD-PKG', name: 'Kemasan & Packaging' },
  { code: 'SLCAT-PRD-MRO', name: 'MRO & Suku Cadang' },
  { code: 'SLCAT-PRD-CAP', name: 'Barang Modal / Capital Goods' },
  { code: 'SLCAT-PRD-FMC', name: 'FMCG' },
  { code: 'SLCAT-PRD-CHM', name: 'Kimia & Bahan Pembantu' },
  { code: 'SLCAT-PRD-ELC', name: 'Elektronik & Komponen' },
  { code: 'SLCAT-PRD-TEX', name: 'Tekstil & Garmen' },
  { code: 'SLCAT-PRD-FRN', name: 'Furnitur & Kayu' },
  { code: 'SLCAT-PRD-MET', name: 'Logam & Baja' },
  { code: 'SLCAT-PRD-PLT', name: 'Plastik & Karet' },
  { code: 'SLCAT-PRD-FOD', name: 'Pangan & Minuman' },
  { code: 'SLCAT-PRD-PHM', name: 'Farmasi & Kosmetik' },
  { code: 'SLCAT-PRD-AGR', name: 'Agribisnis & Pupuk' },

  // === STATUS KETENAGAKERJAAN (5) ===
  { code: 'SLCAT-EMP-INT', name: 'Internal (Karyawan Tetap)' },
  { code: 'SLCAT-EMP-CNT', name: 'Kontrak' },
  { code: 'SLCAT-EMP-FRL', name: 'Freelance / Komisi' },
  { code: 'SLCAT-EMP-AGN', name: 'Agen Luar' },
  { code: 'SLCAT-EMP-BRK', name: 'Broker / Makelar' },

  // === TARGET PASAR (8) ===
  { code: 'SLCAT-MKT-UKM', name: 'UKM & UMKM' },
  { code: 'SLCAT-MKT-ENT', name: 'Enterprise / Korporat' },
  { code: 'SLCAT-MKT-SME', name: 'Perusahaan Menengah' },
  { code: 'SLCAT-MKT-SOE', name: 'BUMN & BUMD' },
  { code: 'SLCAT-MKT-HOS', name: 'Rumah Sakit & Klinik' },
  { code: 'SLCAT-MKT-SCH', name: 'Pendidikan & Institusi' },
  { code: 'SLCAT-MKT-HTL', name: 'Horeka (Hotel, Restoran, Kafe)' },
  { code: 'SLCAT-MKT-CON', name: 'Konstruksi & Properti' },

  // === PROGRAM KHUSUS (10) ===
  { code: 'SLCAT-PRG-KPI', name: 'KPI High Performer' },
  { code: 'SLCAT-PRG-TRN', name: 'In-Training / Magang' },
  { code: 'SLCAT-PRG-MNT', name: 'Mentor & Senior' },
  { code: 'SLCAT-PRG-INS', name: 'Insentif Khusus' },
  { code: 'SLCAT-PRG-NPA', name: 'New Product Ambassador' },
  { code: 'SLCAT-PRG-VIP', name: 'VIP Account Manager' },
  { code: 'SLCAT-PRG-REC', name: 'Recovery Akun Dorman' },
  { code: 'SLCAT-PRG-EXP', name: 'Ekspansi Wilayah Baru' },
  { code: 'SLCAT-PRG-CRS', name: 'Cross-Selling Specialist' },
  { code: 'SLCAT-PRG-AFT', name: 'After-Sales & Retensi' },

  // === KUOTA & TARGET (8) ===
  { code: 'SLCAT-QTA-A', name: 'Kuota Tier A (> Rp 1 M/bln)' },
  { code: 'SLCAT-QTA-B', name: 'Kuota Tier B (Rp 500 Jt–1 M/bln)' },
  { code: 'SLCAT-QTA-C', name: 'Kuota Tier C (Rp 100–500 Jt/bln)' },
  { code: 'SLCAT-QTA-D', name: 'Kuota Tier D (< Rp 100 Jt/bln)' },
  { code: 'SLCAT-QTA-VOL', name: 'Volume-Based Target' },
  { code: 'SLCAT-QTA-VAL', name: 'Value-Based Target' },
  { code: 'SLCAT-QTA-MIX', name: 'Mixed Target (Vol + Val)' },
  { code: 'SLCAT-QTA-SKU', name: 'SKU Expansion Target' },

  // === KOMISI / SKEMA KOMPENSASI (8) ===
  { code: 'SLCAT-KOM-FIX', name: 'Gaji Tetap (Tanpa Komisi)' },
  { code: 'SLCAT-KOM-BAS', name: 'Gaji Pokok + Komisi' },
  { code: 'SLCAT-KOM-COM', name: 'Full Komisi' },
  { code: 'SLCAT-KOM-BNS', name: 'Bonus Triwulan' },
  { code: 'SLCAT-KOM-SPF', name: 'Komisi Spesial Produk Baru' },
  { code: 'SLCAT-KOM-TEA', name: 'Komisi Tim' },
  { code: 'SLCAT-KOM-ACC', name: 'Komisi Akumulatif Tahunan' },
  { code: 'SLCAT-KOM-OVR', name: 'Overriding (Supervisor)' },
];

async function main() {
  console.log('Seeding 100 salesman categories...');

  let created = 0;
  let skipped = 0;

  for (const cat of SALESMAN_CATEGORIES) {
    const existing = await prisma.erpPartnerSubCategory.findFirst({
      where: { code: cat.code, type: 'SALESMAN' },
      select: { id: true },
    });

    if (existing) {
      skipped++;
      continue;
    }

    await prisma.erpPartnerSubCategory.create({
      data: {
        code: cat.code,
        name: cat.name,
        type: 'SALESMAN',
        isActive: true,
      },
    });
    created++;
  }

  console.log(`Done: ${created} created, ${skipped} skipped (already exist).`);
}

main()
  .catch((e) => { console.error(e); process.exit(1); })
  .finally(() => prisma.$disconnect());
