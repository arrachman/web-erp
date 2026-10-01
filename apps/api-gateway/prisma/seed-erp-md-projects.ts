/**
 * Seed real manufacturing project data into md_projects.
 * Replaces dummy DUMMY-PRJ-* rows with 105 realistic project entries.
 *
 * Run: npx ts-node prisma/seed-erp-md-projects.ts
 * Safe to re-run: deletes DUMMY-PRJ-* rows first, then upserts by code.
 *
 * Categories:
 *   PROD = Produksi & Manufaktur        (30 entries)
 *   RD   = Riset & Pengembangan         (15 entries)
 *   QC   = Quality Control & Sertifikasi(15 entries)
 *   FAC  = Fasilitas & Infrastruktur    (15 entries)
 *   IT   = Teknologi Informasi          (10 entries)
 *   EXP  = Ekspor & Pemasaran           (10 entries)
 *   ENV  = Lingkungan, K3 & CSR         (10 entries)
 */
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

type ProjectSeed = {
  code: string;
  name: string;
  startDate: Date;
  endDate: Date;
  isActive: boolean;
};

const d = (y: number, m: number, day: number) => new Date(y, m - 1, day);

const PROJECTS: ProjectSeed[] = [
  // ─── PROD: Produksi & Manufaktur ───────────────────────────────────────────
  { code: 'PROD-2022-001', name: 'Produksi Seragam Karyawan Nasional 2022',         startDate: d(2022,  1,  3), endDate: d(2022,  6, 30), isActive: false },
  { code: 'PROD-2022-002', name: 'Produksi Kemeja Korporat Batch I 2022',           startDate: d(2022,  3,  1), endDate: d(2022,  5, 31), isActive: false },
  { code: 'PROD-2022-003', name: 'Produksi Seragam Rumah Sakit Batch A',            startDate: d(2022,  4,  1), endDate: d(2022,  8, 31), isActive: false },
  { code: 'PROD-2023-001', name: 'Produksi Seragam Karyawan Nasional 2023',         startDate: d(2023,  1,  2), endDate: d(2023,  6, 30), isActive: false },
  { code: 'PROD-2023-002', name: 'Produksi Batik Korporat Edisi Khusus 2023',       startDate: d(2023,  2,  1), endDate: d(2023,  4, 28), isActive: false },
  { code: 'PROD-2023-003', name: 'Produksi Jaket Musim Hujan 2023',                 startDate: d(2023,  6,  1), endDate: d(2023,  9, 30), isActive: false },
  { code: 'PROD-2023-004', name: 'Produksi Seragam Sekolah Ajaran 2023/2024',       startDate: d(2023,  5,  1), endDate: d(2023,  7, 31), isActive: false },
  { code: 'PROD-2023-005', name: 'Produksi Pakaian Olahraga & Atribut Porseni',     startDate: d(2023,  7,  1), endDate: d(2023, 10, 31), isActive: false },
  { code: 'PROD-2023-006', name: 'Produksi Rompi Safety Industri Batch I',          startDate: d(2023,  3,  1), endDate: d(2023,  5, 31), isActive: false },
  { code: 'PROD-2023-007', name: 'Produksi Seragam Hotel & Hospitality 2023',       startDate: d(2023,  8,  1), endDate: d(2023, 11, 30), isActive: false },
  { code: 'PROD-2024-001', name: 'Produksi Seragam Karyawan Nasional 2024',         startDate: d(2024,  1,  2), endDate: d(2024,  6, 28), isActive: true  },
  { code: 'PROD-2024-002', name: 'Produksi Kemeja Korporat Q1 2024',                startDate: d(2024,  1, 15), endDate: d(2024,  3, 31), isActive: false },
  { code: 'PROD-2024-003', name: 'Produksi Kemeja Korporat Q2 2024',                startDate: d(2024,  4,  1), endDate: d(2024,  6, 30), isActive: false },
  { code: 'PROD-2024-004', name: 'Produksi Kemeja Korporat Q3 2024',                startDate: d(2024,  7,  1), endDate: d(2024,  9, 30), isActive: false },
  { code: 'PROD-2024-005', name: 'Produksi Kemeja Korporat Q4 2024',                startDate: d(2024, 10,  1), endDate: d(2024, 12, 31), isActive: false },
  { code: 'PROD-2024-006', name: 'Produksi Batik Korporat Hari Batik 2024',         startDate: d(2024,  7,  1), endDate: d(2024, 10,  2), isActive: false },
  { code: 'PROD-2024-007', name: 'Produksi Seragam Sekolah Ajaran 2024/2025',       startDate: d(2024,  4,  1), endDate: d(2024,  7, 15), isActive: false },
  { code: 'PROD-2024-008', name: 'Produksi Jas & Blazer Eksekutif 2024',            startDate: d(2024,  3,  1), endDate: d(2024,  8, 31), isActive: true  },
  { code: 'PROD-2024-009', name: 'Produksi Pakaian Olahraga & Atribut 2024',        startDate: d(2024,  5,  1), endDate: d(2024,  9, 30), isActive: true  },
  { code: 'PROD-2024-010', name: 'Produksi Seragam Rumah Sakit Batch B 2024',       startDate: d(2024,  2,  1), endDate: d(2024,  7, 31), isActive: false },
  { code: 'PROD-2024-011', name: 'Produksi Rompi Safety Industri Batch II 2024',    startDate: d(2024,  1,  1), endDate: d(2024,  4, 30), isActive: false },
  { code: 'PROD-2024-012', name: 'Produksi Seragam Hotel & Resort 2024',            startDate: d(2024,  6,  1), endDate: d(2024, 12, 31), isActive: true  },
  { code: 'PROD-2024-013', name: 'Produksi Kaos Promosi & Merchandise 2024',        startDate: d(2024,  8,  1), endDate: d(2024, 11, 30), isActive: true  },
  { code: 'PROD-2024-014', name: 'Produksi Celana Kerja & Bib Overall 2024',        startDate: d(2024,  4,  1), endDate: d(2024, 10, 31), isActive: true  },
  { code: 'PROD-2025-001', name: 'Produksi Seragam Karyawan Nasional 2025',         startDate: d(2025,  1,  2), endDate: d(2025,  6, 30), isActive: true  },
  { code: 'PROD-2025-002', name: 'Produksi Kemeja Korporat Q1 2025',                startDate: d(2025,  1, 15), endDate: d(2025,  3, 31), isActive: false },
  { code: 'PROD-2025-003', name: 'Produksi Kemeja Korporat Q2 2025',                startDate: d(2025,  4,  1), endDate: d(2025,  6, 30), isActive: true  },
  { code: 'PROD-2025-004', name: 'Produksi Seragam Perbankan 2025',                 startDate: d(2025,  2,  1), endDate: d(2025,  8, 31), isActive: true  },
  { code: 'PROD-2025-005', name: 'Produksi Pakaian Pilot & Kru Kabin 2025',         startDate: d(2025,  3,  1), endDate: d(2025,  9, 30), isActive: true  },
  { code: 'PROD-2025-006', name: 'Produksi Seragam BUMN Batch 2025',                startDate: d(2025,  1,  1), endDate: d(2025, 12, 31), isActive: true  },

  // ─── RD: Riset & Pengembangan ──────────────────────────────────────────────
  { code: 'RD-2022-001', name: 'Riset Material Kain Anti-Air Generasi 2',           startDate: d(2022,  3,  1), endDate: d(2023,  2, 28), isActive: false },
  { code: 'RD-2022-002', name: 'Pengembangan Desain Seragam Ergonomis',             startDate: d(2022,  6,  1), endDate: d(2023,  5, 31), isActive: false },
  { code: 'RD-2023-001', name: 'Riset Bahan Baku Ramah Lingkungan (Eco-Fabric)',     startDate: d(2023,  1,  1), endDate: d(2024,  6, 30), isActive: false },
  { code: 'RD-2023-002', name: 'Pengembangan Lini Produk Smart Uniform',            startDate: d(2023,  7,  1), endDate: d(2025,  6, 30), isActive: true  },
  { code: 'RD-2023-003', name: 'Riset Pewarnaan Tekstil Berbasis Air (Water-Based)', startDate: d(2023,  4,  1), endDate: d(2024,  3, 31), isActive: false },
  { code: 'RD-2024-001', name: 'Pengembangan Standar Pola Potong Otomatis (CAD)',   startDate: d(2024,  1,  1), endDate: d(2024, 12, 31), isActive: true  },
  { code: 'RD-2024-002', name: 'Riset Bahan Antibakteri untuk Seragam Medis',       startDate: d(2024,  3,  1), endDate: d(2025,  2, 28), isActive: true  },
  { code: 'RD-2024-003', name: 'Pengembangan Koleksi Batik Kontemporer 2025',       startDate: d(2024,  6,  1), endDate: d(2025,  3, 31), isActive: true  },
  { code: 'RD-2024-004', name: 'Riset Otomasi Mesin Jahit Berbasis AI',             startDate: d(2024,  9,  1), endDate: d(2026,  8, 31), isActive: true  },
  { code: 'RD-2025-001', name: 'Pengembangan Lini Recycled Polyester Uniform',      startDate: d(2025,  1,  1), endDate: d(2026, 12, 31), isActive: true  },
  { code: 'RD-2025-002', name: 'Riset Teknologi Laser Cutting untuk Pola Tekstil',  startDate: d(2025,  2,  1), endDate: d(2026,  1, 31), isActive: true  },
  { code: 'RD-2025-003', name: 'Pengembangan Produk Custom Embroidery Digital',     startDate: d(2025,  3,  1), endDate: d(2025, 12, 31), isActive: true  },
  { code: 'RD-2025-004', name: 'Riset Rantai Pasok Lokal (Local Content) ≥70%',     startDate: d(2025,  1,  1), endDate: d(2026,  6, 30), isActive: true  },
  { code: 'RD-2025-005', name: 'Pengembangan Lini Pakaian Kesehatan & Wellness',    startDate: d(2025,  4,  1), endDate: d(2026,  3, 31), isActive: true  },
  { code: 'RD-2025-006', name: 'Riset Standarisasi Ukuran Seragam Nasional',        startDate: d(2025,  5,  1), endDate: d(2026,  4, 30), isActive: true  },

  // ─── QC: Quality Control & Sertifikasi ────────────────────────────────────
  { code: 'QC-2022-001', name: 'Sertifikasi ISO 9001:2015 Sistem Manajemen Mutu',   startDate: d(2022,  1,  1), endDate: d(2022, 12, 31), isActive: false },
  { code: 'QC-2023-001', name: 'Audit Internal Mutu Produksi Semester I 2023',      startDate: d(2023,  1,  1), endDate: d(2023,  6, 30), isActive: false },
  { code: 'QC-2023-002', name: 'Audit Internal Mutu Produksi Semester II 2023',     startDate: d(2023,  7,  1), endDate: d(2023, 12, 31), isActive: false },
  { code: 'QC-2023-003', name: 'Program Zero Defect Lini Jahit 2023',               startDate: d(2023,  3,  1), endDate: d(2023, 11, 30), isActive: false },
  { code: 'QC-2023-004', name: 'Sertifikasi OEKO-TEX Standard 100',                 startDate: d(2023,  6,  1), endDate: d(2024,  5, 31), isActive: false },
  { code: 'QC-2024-001', name: 'Audit Internal Mutu Produksi Semester I 2024',      startDate: d(2024,  1,  1), endDate: d(2024,  6, 30), isActive: false },
  { code: 'QC-2024-002', name: 'Audit Internal Mutu Produksi Semester II 2024',     startDate: d(2024,  7,  1), endDate: d(2024, 12, 31), isActive: false },
  { code: 'QC-2024-003', name: 'Program Peningkatan Mutu First Pass Yield ≥98%',    startDate: d(2024,  2,  1), endDate: d(2024, 12, 31), isActive: true  },
  { code: 'QC-2024-004', name: 'Sertifikasi ISO 14001:2015 Manajemen Lingkungan',   startDate: d(2024,  4,  1), endDate: d(2025,  3, 31), isActive: true  },
  { code: 'QC-2024-005', name: 'Validasi Proses Pencelupan & Finishing',             startDate: d(2024,  5,  1), endDate: d(2024, 10, 31), isActive: false },
  { code: 'QC-2025-001', name: 'Audit Internal Mutu Produksi Semester I 2025',      startDate: d(2025,  1,  1), endDate: d(2025,  6, 30), isActive: true  },
  { code: 'QC-2025-002', name: 'Recertification ISO 9001:2015 Siklus 3 Tahun',      startDate: d(2025,  1,  1), endDate: d(2025,  9, 30), isActive: true  },
  { code: 'QC-2025-003', name: 'Program Kalibrasi Alat Ukur & Mesin Produksi',      startDate: d(2025,  2,  1), endDate: d(2025, 12, 31), isActive: true  },
  { code: 'QC-2025-004', name: 'Implementasi Sistem AQL 2.5 pada Inspeksi Akhir',   startDate: d(2025,  3,  1), endDate: d(2025,  9, 30), isActive: true  },
  { code: 'QC-2025-005', name: 'Sertifikasi SA8000 Tanggung Jawab Sosial',          startDate: d(2025,  4,  1), endDate: d(2026,  3, 31), isActive: true  },

  // ─── FAC: Fasilitas & Infrastruktur ───────────────────────────────────────
  { code: 'FAC-2022-001', name: 'Renovasi Gedung Produksi Lantai 2',                startDate: d(2022,  1,  1), endDate: d(2022,  8, 31), isActive: false },
  { code: 'FAC-2022-002', name: 'Instalasi Sistem HVAC Gudang Bahan Baku',          startDate: d(2022,  6,  1), endDate: d(2022, 10, 31), isActive: false },
  { code: 'FAC-2023-001', name: 'Perluasan Area Produksi Lini 4 & 5',               startDate: d(2023,  1,  1), endDate: d(2023, 12, 31), isActive: false },
  { code: 'FAC-2023-002', name: 'Pengadaan & Instalasi Mesin Jahit Lockstitch 60 Unit', startDate: d(2023,  4,  1), endDate: d(2023,  8, 31), isActive: false },
  { code: 'FAC-2023-003', name: 'Pembangunan Musholla & Area Istirahat Karyawan',   startDate: d(2023,  5,  1), endDate: d(2023,  9, 30), isActive: false },
  { code: 'FAC-2024-001', name: 'Instalasi Panel Surya 200 kWp Atap Pabrik',        startDate: d(2024,  1,  1), endDate: d(2024,  6, 30), isActive: false },
  { code: 'FAC-2024-002', name: 'Upgrade Sistem Kelistrikan & Daya Cadangan',       startDate: d(2024,  3,  1), endDate: d(2024,  8, 31), isActive: false },
  { code: 'FAC-2024-003', name: 'Pembangunan Gudang Barang Jadi Kapasitas 5000 m²', startDate: d(2024,  6,  1), endDate: d(2025,  3, 31), isActive: true  },
  { code: 'FAC-2024-004', name: 'Instalasi Conveyor System Lini Produksi',          startDate: d(2024,  7,  1), endDate: d(2024, 12, 31), isActive: true  },
  { code: 'FAC-2025-001', name: 'Renovasi Total Kantor HO & Showroom',              startDate: d(2025,  1,  1), endDate: d(2025,  8, 31), isActive: true  },
  { code: 'FAC-2025-002', name: 'Pengadaan Mesin Bordir Digital 20 Unit',           startDate: d(2025,  2,  1), endDate: d(2025,  5, 31), isActive: true  },
  { code: 'FAC-2025-003', name: 'Pembangunan Laboratorium Uji Kualitas Tekstil',    startDate: d(2025,  3,  1), endDate: d(2025, 12, 31), isActive: true  },
  { code: 'FAC-2025-004', name: 'Instalasi Sistem Fire Suppression Gudang',         startDate: d(2025,  1, 15), endDate: d(2025,  4, 30), isActive: false },
  { code: 'FAC-2025-005', name: 'Modernisasi Lini Potong dengan Mesin Gerber',      startDate: d(2025,  4,  1), endDate: d(2025, 10, 31), isActive: true  },

  // ─── IT: Teknologi Informasi ───────────────────────────────────────────────
  { code: 'IT-2023-001', name: 'Implementasi Sistem ERP Senti — Fase I Master Data', startDate: d(2023,  6,  1), endDate: d(2024,  5, 31), isActive: false },
  { code: 'IT-2023-002', name: 'Migrasi Data MyERP+ ke Senti ERP',                  startDate: d(2023,  9,  1), endDate: d(2024,  6, 30), isActive: false },
  { code: 'IT-2024-001', name: 'Implementasi Sistem ERP Senti — Fase II Transaksi', startDate: d(2024,  1,  1), endDate: d(2024, 12, 31), isActive: true  },
  { code: 'IT-2024-002', name: 'Upgrade Infrastruktur Server & Jaringan LAN',       startDate: d(2024,  3,  1), endDate: d(2024,  8, 31), isActive: false },
  { code: 'IT-2024-003', name: 'Implementasi CCTV & Kontrol Akses Digital',         startDate: d(2024,  5,  1), endDate: d(2024,  9, 30), isActive: false },
  { code: 'IT-2024-004', name: 'Pengembangan Aplikasi Absensi Mobile Karyawan',     startDate: d(2024,  7,  1), endDate: d(2024, 12, 31), isActive: true  },
  { code: 'IT-2025-001', name: 'Implementasi Sistem ERP Senti — Fase III Laporan',  startDate: d(2025,  1,  1), endDate: d(2025, 12, 31), isActive: true  },
  { code: 'IT-2025-002', name: 'Pengembangan Dashboard BI & Analitik Produksi',     startDate: d(2025,  2,  1), endDate: d(2025, 10, 31), isActive: true  },
  { code: 'IT-2025-003', name: 'Integrasi E-Commerce & Marketplace Platform',       startDate: d(2025,  3,  1), endDate: d(2025, 12, 31), isActive: true  },
  { code: 'IT-2025-004', name: 'Implementasi Sistem WMS (Warehouse Management)',    startDate: d(2025,  4,  1), endDate: d(2026,  3, 31), isActive: true  },

  // ─── EXP: Ekspor & Pemasaran ───────────────────────────────────────────────
  { code: 'EXP-2022-001', name: 'Ekspansi Pasar Malaysia & Brunei 2022',            startDate: d(2022,  1,  1), endDate: d(2022, 12, 31), isActive: false },
  { code: 'EXP-2023-001', name: 'Ekspor Seragam Korporat ke Singapura 2023',        startDate: d(2023,  1,  1), endDate: d(2023, 12, 31), isActive: false },
  { code: 'EXP-2023-002', name: 'Partisipasi Pameran Apparel India 2023',           startDate: d(2023,  8,  1), endDate: d(2023, 10, 31), isActive: false },
  { code: 'EXP-2024-001', name: 'Ekspor Batik & Tenun ke Pasar Eropa 2024',         startDate: d(2024,  2,  1), endDate: d(2024, 12, 31), isActive: true  },
  { code: 'EXP-2024-002', name: 'Pengembangan Brand Global "Senti Wear"',           startDate: d(2024,  5,  1), endDate: d(2025, 12, 31), isActive: true  },
  { code: 'EXP-2024-003', name: 'Riset & Penetrasi Pasar Timur Tengah 2024',        startDate: d(2024,  7,  1), endDate: d(2025,  6, 30), isActive: true  },
  { code: 'EXP-2025-001', name: 'Ekspor Seragam Hospitality ke Singapura 2025',     startDate: d(2025,  1,  1), endDate: d(2025, 12, 31), isActive: true  },
  { code: 'EXP-2025-002', name: 'Partisipasi Pameran Texworld Paris 2025',          startDate: d(2025,  3,  1), endDate: d(2025,  5, 31), isActive: true  },
  { code: 'EXP-2025-003', name: 'Ekspansi OEM Pakaian ke Pembeli Australia',        startDate: d(2025,  2,  1), endDate: d(2026,  1, 31), isActive: true  },
  { code: 'EXP-2025-004', name: 'Program Sertifikasi USDOL FIST untuk Ekspor AS',   startDate: d(2025,  4,  1), endDate: d(2025, 12, 31), isActive: true  },

  // ─── ENV: Lingkungan, K3 & CSR ────────────────────────────────────────────
  { code: 'ENV-2022-001', name: 'Program Pengolahan Limbah Cair Tekstil (IPAL)',     startDate: d(2022,  6,  1), endDate: d(2023,  5, 31), isActive: false },
  { code: 'ENV-2023-001', name: 'Program K3 — Zero Accident 2023',                  startDate: d(2023,  1,  1), endDate: d(2023, 12, 31), isActive: false },
  { code: 'ENV-2023-002', name: 'Penghijauan & Taman Lingkungan Pabrik',            startDate: d(2023,  4,  1), endDate: d(2023,  9, 30), isActive: false },
  { code: 'ENV-2024-001', name: 'Program K3 — Zero Accident 2024',                  startDate: d(2024,  1,  1), endDate: d(2024, 12, 31), isActive: false },
  { code: 'ENV-2024-002', name: 'Reduksi Emisi Karbon 15% Target 2024',             startDate: d(2024,  1,  1), endDate: d(2024, 12, 31), isActive: true  },
  { code: 'ENV-2024-003', name: 'CSR Beasiswa SMK Tekstil & Garmen 2024',           startDate: d(2024,  5,  1), endDate: d(2025,  4, 30), isActive: true  },
  { code: 'ENV-2025-001', name: 'Program K3 — Zero Accident 2025',                  startDate: d(2025,  1,  1), endDate: d(2025, 12, 31), isActive: true  },
  { code: 'ENV-2025-002', name: 'Sertifikasi Higg Index (FEM) Fasilitas Pabrik',    startDate: d(2025,  2,  1), endDate: d(2025, 11, 30), isActive: true  },
  { code: 'ENV-2025-003', name: 'Program Daur Ulang Sisa Kain (Fabric Waste)',      startDate: d(2025,  1,  1), endDate: d(2025, 12, 31), isActive: true  },
  { code: 'ENV-2025-004', name: 'CSR Pemberdayaan Penjahit Rumahan Sekitar Pabrik', startDate: d(2025,  3,  1), endDate: d(2026,  2, 28), isActive: true  },
];

async function main(): Promise<void> {
  // 1. Hapus semua baris dummy
  const deleted = await prisma.erpProject.deleteMany({
    where: { code: { startsWith: 'DUMMY-PRJ-' } },
  });
  console.log(`- md_projects: deleted ${deleted.count} dummy rows.`);

  // 2. Upsert data real (idempoten per code)
  let upserted = 0;
  for (const p of PROJECTS) {
    await prisma.erpProject.upsert({
      where: { code: p.code },
      update: { name: p.name, startDate: p.startDate, endDate: p.endDate, isActive: p.isActive },
      create: { code: p.code, name: p.name, startDate: p.startDate, endDate: p.endDate, isActive: p.isActive },
    });
    upserted++;
  }
  console.log(`+ md_projects: upserted ${upserted} real project rows.`);
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(async () => {
    await prisma.$disconnect();
  });
