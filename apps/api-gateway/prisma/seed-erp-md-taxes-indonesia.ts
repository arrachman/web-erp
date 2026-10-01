/**
 * Seed: Master Data Pajak Indonesia (md_taxes)
 * Mencakup PPN, PPh 21/22/23/26, PPh Final 4(2), PPh Badan, PPnBM, BPHTB, PBB.
 * Idempoten via upsert by code — aman dijalankan berkali-kali.
 *
 * Run:
 *   npx ts-node prisma/seed-erp-md-taxes-indonesia.ts
 *   # atau tambahkan ke seed-erp.ts batch
 */
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

const INDONESIAN_TAXES = [
  // ──────────────────────────────────────────────────────────────
  // PPN – Pajak Pertambahan Nilai
  // ──────────────────────────────────────────────────────────────
  {
    code: 'PPN-0',
    name: 'PPN 0% – Ekspor BKP/JKP & Barang Strategis Bebas PPN',
    rate: 0,
    legacyCode: null,
  },
  {
    code: 'PPN-11',
    name: 'PPN 11% – Pajak Pertambahan Nilai Standar',
    rate: 11,
    legacyCode: null,
  },
  {
    code: 'PPN-12',
    name: 'PPN 12% – Pajak Pertambahan Nilai (berlaku 1 Jan 2025)',
    rate: 12,
    legacyCode: null,
  },

  // ──────────────────────────────────────────────────────────────
  // PPh Pasal 21 – Pemotongan atas Penghasilan Orang Pribadi
  // Tarif berlapis UU HPP (2022): 5 / 15 / 25 / 30 / 35%
  // ──────────────────────────────────────────────────────────────
  {
    code: 'PPH21-5',
    name: 'PPh 21 Tarif 5% – Penghasilan s.d. Rp 60 Juta/tahun',
    rate: 5,
    legacyCode: null,
  },
  {
    code: 'PPH21-15',
    name: 'PPh 21 Tarif 15% – Penghasilan Rp 60–250 Juta/tahun',
    rate: 15,
    legacyCode: null,
  },
  {
    code: 'PPH21-25',
    name: 'PPh 21 Tarif 25% – Penghasilan Rp 250–500 Juta/tahun',
    rate: 25,
    legacyCode: null,
  },
  {
    code: 'PPH21-30',
    name: 'PPh 21 Tarif 30% – Penghasilan Rp 500 Juta – 5 Miliar/tahun',
    rate: 30,
    legacyCode: null,
  },
  {
    code: 'PPH21-35',
    name: 'PPh 21 Tarif 35% – Penghasilan di atas Rp 5 Miliar/tahun',
    rate: 35,
    legacyCode: null,
  },

  // ──────────────────────────────────────────────────────────────
  // PPh Pasal 22 – Pemotongan/Pemungutan atas Impor & Pembelian
  // ──────────────────────────────────────────────────────────────
  {
    code: 'PPH22-IMPOR-API',
    name: 'PPh 22 Impor – Dengan API (Angka Pengenal Importir)',
    rate: 2.5,
    legacyCode: null,
  },
  {
    code: 'PPH22-IMPOR-TANPA-API',
    name: 'PPh 22 Impor – Tanpa API (Angka Pengenal Importir)',
    rate: 7.5,
    legacyCode: null,
  },
  {
    code: 'PPH22-BUMN',
    name: 'PPh 22 Pembelian oleh BUMN / Bendahara Pemerintah',
    rate: 1.5,
    legacyCode: null,
  },
  {
    code: 'PPH22-SAWIT',
    name: 'PPh 22 Industri Kelapa Sawit – CPO & Produk Turunan',
    rate: 0.25,
    legacyCode: null,
  },
  {
    code: 'PPH22-MIGAS',
    name: 'PPh 22 Industri Migas, Tambang Batubara & Mineral',
    rate: 0.3,
    legacyCode: null,
  },
  {
    code: 'PPH22-KERTAS',
    name: 'PPh 22 Industri Kertas – Penjualan di Dalam Negeri',
    rate: 0.1,
    legacyCode: null,
  },
  {
    code: 'PPH22-BAJA',
    name: 'PPh 22 Industri Baja – Penjualan di Dalam Negeri',
    rate: 0.3,
    legacyCode: null,
  },
  {
    code: 'PPH22-OTOMOTIF',
    name: 'PPh 22 Industri Otomotif – Penjualan di Dalam Negeri',
    rate: 0.45,
    legacyCode: null,
  },

  // ──────────────────────────────────────────────────────────────
  // PPh Pasal 23 – Pemotongan atas Dividen, Bunga, Royalti, Jasa
  // ──────────────────────────────────────────────────────────────
  {
    code: 'PPH23-DIVIDEN',
    name: 'PPh 23 Dividen – dari Badan ke Badan (dalam negeri)',
    rate: 15,
    legacyCode: null,
  },
  {
    code: 'PPH23-BUNGA',
    name: 'PPh 23 Bunga – Termasuk Premium, Diskonto & Imbalan',
    rate: 15,
    legacyCode: null,
  },
  {
    code: 'PPH23-ROYALTI',
    name: 'PPh 23 Royalti – Penggunaan Hak / Hak Cipta',
    rate: 15,
    legacyCode: null,
  },
  {
    code: 'PPH23-HADIAH',
    name: 'PPh 23 Hadiah & Penghargaan (selain yang dipotong PPh 21)',
    rate: 15,
    legacyCode: null,
  },
  {
    code: 'PPH23-SEWA-NON-TB',
    name: 'PPh 23 Sewa Harta Non-Tanah/Bangunan',
    rate: 2,
    legacyCode: null,
  },
  {
    code: 'PPH23-JASA',
    name: 'PPh 23 Jasa Lainnya (sesuai PMK)',
    rate: 2,
    legacyCode: null,
  },
  {
    code: 'PPH23-JASA-TEKNIK',
    name: 'PPh 23 Jasa Teknik, Manajemen & Konsultan',
    rate: 2,
    legacyCode: null,
  },

  // ──────────────────────────────────────────────────────────────
  // PPh Pasal 26 – Pemotongan untuk Wajib Pajak Luar Negeri (WPLN)
  // Tanpa tax treaty; tarif berbeda jika ada P3B / tax treaty
  // ──────────────────────────────────────────────────────────────
  {
    code: 'PPH26-DIVIDEN',
    name: 'PPh 26 Dividen ke WPLN – Tanpa Tax Treaty',
    rate: 20,
    legacyCode: null,
  },
  {
    code: 'PPH26-BUNGA',
    name: 'PPh 26 Bunga ke WPLN – Tanpa Tax Treaty',
    rate: 20,
    legacyCode: null,
  },
  {
    code: 'PPH26-ROYALTI',
    name: 'PPh 26 Royalti ke WPLN – Tanpa Tax Treaty',
    rate: 20,
    legacyCode: null,
  },
  {
    code: 'PPH26-JASA',
    name: 'PPh 26 Imbalan Jasa ke WPLN – Tanpa Tax Treaty',
    rate: 20,
    legacyCode: null,
  },
  {
    code: 'PPH26-PENGHASILAN-LAIN',
    name: 'PPh 26 Penghasilan Lain ke WPLN – Tanpa Tax Treaty',
    rate: 20,
    legacyCode: null,
  },

  // ──────────────────────────────────────────────────────────────
  // PPh Final Pasal 4 Ayat (2)
  // ──────────────────────────────────────────────────────────────
  {
    code: 'PPH4-2-SEWA-TB',
    name: 'PPh Final 4(2) – Sewa Tanah & Bangunan',
    rate: 10,
    legacyCode: null,
  },
  {
    code: 'PPH4-2-KONSTRUKSI-KECIL',
    name: 'PPh Final 4(2) – Jasa Konstruksi Kualifikasi Kecil',
    rate: 1.75,
    legacyCode: null,
  },
  {
    code: 'PPH4-2-KONSTRUKSI-MENENGAH',
    name: 'PPh Final 4(2) – Jasa Konstruksi Kualifikasi Menengah/Besar',
    rate: 2.65,
    legacyCode: null,
  },
  {
    code: 'PPH4-2-KONSTRUKSI-TANPA',
    name: 'PPh Final 4(2) – Jasa Konstruksi Tanpa Kualifikasi',
    rate: 4,
    legacyCode: null,
  },
  {
    code: 'PPH4-2-PERENCANAAN',
    name: 'PPh Final 4(2) – Jasa Perencanaan/Pengawasan Konstruksi Berkualifikasi',
    rate: 3.5,
    legacyCode: null,
  },
  {
    code: 'PPH4-2-PERENCANAAN-TANPA',
    name: 'PPh Final 4(2) – Jasa Perencanaan/Pengawasan Konstruksi Tanpa Kualifikasi',
    rate: 6,
    legacyCode: null,
  },
  {
    code: 'PPH4-2-DEPOSITO',
    name: 'PPh Final 4(2) – Bunga Deposito, Tabungan & SBI',
    rate: 20,
    legacyCode: null,
  },
  {
    code: 'PPH4-2-SAHAM',
    name: 'PPh Final 4(2) – Penjualan Saham di Bursa Efek',
    rate: 0.1,
    legacyCode: null,
  },
  {
    code: 'PPH4-2-PENGALIHAN-TB',
    name: 'PPh Final 4(2) – Pengalihan Hak atas Tanah & Bangunan',
    rate: 2.5,
    legacyCode: null,
  },
  {
    code: 'PPH4-2-HADIAH-UNDIAN',
    name: 'PPh Final 4(2) – Hadiah Undian / Lotere',
    rate: 25,
    legacyCode: null,
  },
  {
    code: 'PPH4-2-DERIVATIF',
    name: 'PPh Final 4(2) – Transaksi Derivatif (Futures/Options di Bursa)',
    rate: 2.5,
    legacyCode: null,
  },

  // ──────────────────────────────────────────────────────────────
  // PPh Badan & UMKM
  // ──────────────────────────────────────────────────────────────
  {
    code: 'PPH-BADAN',
    name: 'PPh Badan 22% – Pajak Penghasilan Perusahaan Umum',
    rate: 22,
    legacyCode: null,
  },
  {
    code: 'PPH-BADAN-TBK',
    name: 'PPh Badan 19% – Perusahaan Tbk (Listed di Bursa Efek, diskon 3%)',
    rate: 19,
    legacyCode: null,
  },
  {
    code: 'PPH-UMKM',
    name: 'PPh Final UMKM 0,5% – PP 23/2018 (Omzet < Rp 4,8 Miliar/tahun)',
    rate: 0.5,
    legacyCode: null,
  },

  // ──────────────────────────────────────────────────────────────
  // PPnBM – Pajak Penjualan atas Barang Mewah
  // ──────────────────────────────────────────────────────────────
  {
    code: 'PPNBM-10',
    name: 'PPnBM 10% – Barang Mewah Golongan I',
    rate: 10,
    legacyCode: null,
  },
  {
    code: 'PPNBM-20',
    name: 'PPnBM 20% – Barang Mewah Golongan II',
    rate: 20,
    legacyCode: null,
  },
  {
    code: 'PPNBM-25',
    name: 'PPnBM 25% – Kendaraan Bermotor Tertentu',
    rate: 25,
    legacyCode: null,
  },
  {
    code: 'PPNBM-35',
    name: 'PPnBM 35% – Kendaraan Bermotor Tertentu',
    rate: 35,
    legacyCode: null,
  },
  {
    code: 'PPNBM-40',
    name: 'PPnBM 40% – Barang Mewah Golongan III',
    rate: 40,
    legacyCode: null,
  },
  {
    code: 'PPNBM-50',
    name: 'PPnBM 50% – Barang Mewah Golongan IV',
    rate: 50,
    legacyCode: null,
  },
  {
    code: 'PPNBM-60',
    name: 'PPnBM 60% – Barang Mewah Golongan V',
    rate: 60,
    legacyCode: null,
  },
  {
    code: 'PPNBM-75',
    name: 'PPnBM 75% – Barang Mewah Golongan VI',
    rate: 75,
    legacyCode: null,
  },
  {
    code: 'PPNBM-125',
    name: 'PPnBM 125% – Kendaraan Bermotor Mewah (CBU/CKD Tertentu)',
    rate: 125,
    legacyCode: null,
  },
  {
    code: 'PPNBM-150',
    name: 'PPnBM 150% – Kendaraan Bermotor Sangat Mewah',
    rate: 150,
    legacyCode: null,
  },
  {
    code: 'PPNBM-200',
    name: 'PPnBM 200% – Kendaraan Bermotor Khusus (Tertinggi)',
    rate: 200,
    legacyCode: null,
  },

  // ──────────────────────────────────────────────────────────────
  // BPHTB – Bea Perolehan Hak atas Tanah & Bangunan
  // ──────────────────────────────────────────────────────────────
  {
    code: 'BPHTB',
    name: 'BPHTB – Bea Perolehan Hak atas Tanah & Bangunan',
    rate: 5,
    legacyCode: null,
  },

  // ──────────────────────────────────────────────────────────────
  // PBB – Pajak Bumi dan Bangunan
  // ──────────────────────────────────────────────────────────────
  {
    code: 'PBB-P2',
    name: 'PBB Perdesaan & Perkotaan (P2) – Tarif Maksimum',
    rate: 0.5,
    legacyCode: null,
  },
  {
    code: 'PBB-P3',
    name: 'PBB Perkebunan, Perhutanan & Pertambangan (P3)',
    rate: 0.5,
    legacyCode: null,
  },
] as const;

async function main() {
  console.log('🌱  Seeding pajak Indonesia ke md_taxes…');

  let created = 0;
  let updated = 0;

  for (const tax of INDONESIAN_TAXES) {
    const result = await prisma.erpTax.upsert({
      where: { code: tax.code },
      create: {
        code: tax.code,
        name: tax.name,
        rate: tax.rate,
        legacyCode: tax.legacyCode,
        isActive: true,
      },
      update: {
        name: tax.name,
        rate: tax.rate,
        legacyCode: tax.legacyCode,
      },
    });

    if (result.createdAt.getTime() === result.updatedAt.getTime()) {
      created++;
    } else {
      updated++;
    }
  }

  console.log(`✅  Selesai: ${created} baru, ${updated} diperbarui (total ${INDONESIAN_TAXES.length} pajak).`);
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(() => prisma.$disconnect());
