/**
 * Seed 100 sub-departemen untuk md_sub_departments.
 * Sub-divisi nyata perusahaan manufaktur garmen Indonesia.
 * Idempotent via upsert on `code`.
 *
 * Mapping departmentId mengacu pada urutan insert seed-md-departments.ts:
 * DEPT-001=1 (Direksi), DEPT-006=6 (Keuangan Umum), dst.
 */
import { PrismaClient } from '@prisma/client';
const prisma = new PrismaClient();

// [departmentCode, subCode, subName]
const subDepts: [string, string, string][] = [
  // Direksi (DEPT-001)
  ['DEPT-001', 'SUBDEPT-001', 'Staf Ahli Direksi'],
  ['DEPT-001', 'SUBDEPT-002', 'Sekretaris Direksi'],

  // Keuangan Umum (DEPT-006)
  ['DEPT-006', 'SUBDEPT-003', 'Penganggaran & Forecasting'],
  ['DEPT-006', 'SUBDEPT-004', 'Kas & Bank'],
  ['DEPT-006', 'SUBDEPT-005', 'Pelaporan Manajemen'],

  // Akuntansi (DEPT-007)
  ['DEPT-007', 'SUBDEPT-006', 'Akuntansi Umum (GL)'],
  ['DEPT-007', 'SUBDEPT-007', 'Akuntansi Aset Tetap'],
  ['DEPT-007', 'SUBDEPT-008', 'Rekonsiliasi & Tutup Buku'],

  // Perpajakan (DEPT-008)
  ['DEPT-008', 'SUBDEPT-009', 'PPh Badan & Pasal 21'],
  ['DEPT-008', 'SUBDEPT-010', 'PPN & Faktur Pajak'],
  ['DEPT-008', 'SUBDEPT-011', 'Perencanaan Pajak'],

  // Treasury (DEPT-009)
  ['DEPT-009', 'SUBDEPT-012', 'Cash Pooling & Likuiditas'],
  ['DEPT-009', 'SUBDEPT-013', 'FX & Hedging'],

  // Audit Internal (DEPT-010)
  ['DEPT-010', 'SUBDEPT-014', 'Audit Operasional'],
  ['DEPT-010', 'SUBDEPT-015', 'Audit IT & Sistem'],
  ['DEPT-010', 'SUBDEPT-016', 'Audit Kepatuhan'],

  // Rekrutmen & Seleksi (DEPT-016)
  ['DEPT-016', 'SUBDEPT-017', 'Rekrutmen Staff & Operator'],
  ['DEPT-016', 'SUBDEPT-018', 'Rekrutmen Manajerial & Eksekutif'],

  // Pengembangan SDM (DEPT-017)
  ['DEPT-017', 'SUBDEPT-019', 'Training Teknis & Vokasional'],
  ['DEPT-017', 'SUBDEPT-020', 'Leadership & Soft Skills'],
  ['DEPT-017', 'SUBDEPT-021', 'Talent Management & Suksesi'],

  // Payroll & Benefit (DEPT-018)
  ['DEPT-018', 'SUBDEPT-022', 'Penggajian Karyawan Tetap'],
  ['DEPT-018', 'SUBDEPT-023', 'Penggajian Kontrak & Harian'],
  ['DEPT-018', 'SUBDEPT-024', 'Benefit & Tunjangan'],

  // Keselamatan & Kesehatan Kerja (DEPT-020)
  ['DEPT-020', 'SUBDEPT-025', 'K3 Pabrik & Lapangan'],
  ['DEPT-020', 'SUBDEPT-026', 'Klinik & Kesehatan Karyawan'],
  ['DEPT-020', 'SUBDEPT-027', 'P3K & Tanggap Darurat'],

  // Infrastruktur IT (DEPT-025)
  ['DEPT-025', 'SUBDEPT-028', 'Server & Storage'],
  ['DEPT-025', 'SUBDEPT-029', 'Jaringan & Telekomunikasi'],
  ['DEPT-025', 'SUBDEPT-030', 'Cloud & Virtualisasi'],

  // Pengembangan Sistem & Aplikasi (DEPT-026)
  ['DEPT-026', 'SUBDEPT-031', 'Backend & API Development'],
  ['DEPT-026', 'SUBDEPT-032', 'Frontend & UI/UX'],
  ['DEPT-026', 'SUBDEPT-033', 'QA & Testing Sistem'],

  // Keamanan Siber (DEPT-027)
  ['DEPT-027', 'SUBDEPT-034', 'SOC & Monitoring Ancaman'],
  ['DEPT-027', 'SUBDEPT-035', 'Penetration Testing & Vulnerability'],
  ['DEPT-027', 'SUBDEPT-036', 'GRC & Kebijakan Keamanan'],

  // ERP & Sistem Bisnis (DEPT-030)
  ['DEPT-030', 'SUBDEPT-037', 'Implementasi & Konfigurasi ERP'],
  ['DEPT-030', 'SUBDEPT-038', 'Dukungan Pengguna ERP'],
  ['DEPT-030', 'SUBDEPT-039', 'Integrasi & Pengembangan Add-on'],

  // Produksi Line A (DEPT-033)
  ['DEPT-033', 'SUBDEPT-040', 'Cutting Line A'],
  ['DEPT-033', 'SUBDEPT-041', 'Sewing Line A'],
  ['DEPT-033', 'SUBDEPT-042', 'Finishing & Packing Line A'],

  // Produksi Line B (DEPT-034)
  ['DEPT-034', 'SUBDEPT-043', 'Cutting Line B'],
  ['DEPT-034', 'SUBDEPT-044', 'Sewing Line B'],
  ['DEPT-034', 'SUBDEPT-045', 'Finishing & Packing Line B'],

  // Produksi Line C (DEPT-035)
  ['DEPT-035', 'SUBDEPT-046', 'Embroidery & Bordir'],
  ['DEPT-035', 'SUBDEPT-047', 'Washing & Laundry'],
  ['DEPT-035', 'SUBDEPT-048', 'Printing & Sablon'],

  // Perencanaan Produksi (DEPT-036)
  ['DEPT-036', 'SUBDEPT-049', 'PPIC – Perencanaan Kapasitas'],
  ['DEPT-036', 'SUBDEPT-050', 'PPIC – Penjadwalan Produksi'],
  ['DEPT-036', 'SUBDEPT-051', 'PPIC – Material Requirement Planning'],

  // Maintenance & Teknik (DEPT-038)
  ['DEPT-038', 'SUBDEPT-052', 'Preventive Maintenance Mesin Jahit'],
  ['DEPT-038', 'SUBDEPT-053', 'Corrective Maintenance & Breakdown'],
  ['DEPT-038', 'SUBDEPT-054', 'Teknik Sipil & Fasilitas Pabrik'],

  // QC Incoming (DEPT-043)
  ['DEPT-043', 'SUBDEPT-055', 'Inspeksi Bahan Baku Kain'],
  ['DEPT-043', 'SUBDEPT-056', 'Inspeksi Aksesori & Benang'],

  // QC In-Process (DEPT-044)
  ['DEPT-044', 'SUBDEPT-057', 'QC Inline Line A'],
  ['DEPT-044', 'SUBDEPT-058', 'QC Inline Line B'],
  ['DEPT-044', 'SUBDEPT-059', 'QC Inline Line C'],

  // QC Final (DEPT-045)
  ['DEPT-045', 'SUBDEPT-060', 'Final Inspection Lokal'],
  ['DEPT-045', 'SUBDEPT-061', 'Final Inspection Ekspor (AQL)'],

  // Sistem Manajemen Mutu (DEPT-049)
  ['DEPT-049', 'SUBDEPT-062', 'Dokumentasi & Prosedur ISO'],
  ['DEPT-049', 'SUBDEPT-063', 'Audit Mutu Internal'],
  ['DEPT-049', 'SUBDEPT-064', 'Sertifikasi & Akreditasi'],

  // Gudang Bahan Baku (DEPT-050)
  ['DEPT-050', 'SUBDEPT-065', 'Penerimaan & Verifikasi Bahan Baku'],
  ['DEPT-050', 'SUBDEPT-066', 'Penyimpanan & Bin Management'],
  ['DEPT-050', 'SUBDEPT-067', 'Pengeluaran Bahan ke Produksi'],

  // Gudang Produk Jadi (DEPT-051)
  ['DEPT-051', 'SUBDEPT-068', 'Penerimaan dari Finishing'],
  ['DEPT-051', 'SUBDEPT-069', 'Penyimpanan & Carton Management'],
  ['DEPT-051', 'SUBDEPT-070', 'Pengiriman ke Pelanggan'],

  // Distribusi & Pengiriman (DEPT-052)
  ['DEPT-052', 'SUBDEPT-071', 'Ekspedisi Domestik'],
  ['DEPT-052', 'SUBDEPT-072', 'Ekspedisi Ekspor & Forwarder'],

  // Pembelian Bahan Baku (DEPT-058)
  ['DEPT-058', 'SUBDEPT-073', 'Pembelian Kain & Tekstil Lokal'],
  ['DEPT-058', 'SUBDEPT-074', 'Pembelian Bahan Impor'],
  ['DEPT-058', 'SUBDEPT-075', 'Pembelian Aksesori & Trims'],

  // Pembelian Aset & Peralatan (DEPT-059)
  ['DEPT-059', 'SUBDEPT-076', 'Pembelian Mesin Produksi'],
  ['DEPT-059', 'SUBDEPT-077', 'Pembelian IT & Infrastruktur'],

  // Evaluasi & Seleksi Vendor (DEPT-061)
  ['DEPT-061', 'SUBDEPT-078', 'Kualifikasi Vendor Baru'],
  ['DEPT-061', 'SUBDEPT-079', 'Evaluasi Kinerja Vendor'],

  // Sales Retail (DEPT-064)
  ['DEPT-064', 'SUBDEPT-080', 'B2C Direct Store'],
  ['DEPT-064', 'SUBDEPT-081', 'E-commerce & Marketplace'],

  // Sales Wholesale & Distributor (DEPT-065)
  ['DEPT-065', 'SUBDEPT-082', 'Distribusi Modern Trade'],
  ['DEPT-065', 'SUBDEPT-083', 'Distribusi Traditional Trade'],

  // Sales Regional Jawa (DEPT-067)
  ['DEPT-067', 'SUBDEPT-084', 'Sales Jabodetabek'],
  ['DEPT-067', 'SUBDEPT-085', 'Sales Jawa Tengah & DIY'],
  ['DEPT-067', 'SUBDEPT-086', 'Sales Jawa Timur'],

  // Digital Marketing (DEPT-073)
  ['DEPT-073', 'SUBDEPT-087', 'SEO & SEM'],
  ['DEPT-073', 'SUBDEPT-088', 'Social Media Management'],
  ['DEPT-073', 'SUBDEPT-089', 'Email & CRM Marketing'],
  ['DEPT-073', 'SUBDEPT-090', 'Analytics & Conversion'],

  // Brand Management (DEPT-074)
  ['DEPT-074', 'SUBDEPT-091', 'Brand Identity & Guideline'],
  ['DEPT-074', 'SUBDEPT-092', 'Brand Activation & Event'],

  // Riset Produk (DEPT-080)
  ['DEPT-080', 'SUBDEPT-093', 'Tren & Consumer Insight'],
  ['DEPT-080', 'SUBDEPT-094', 'Analisis Kompetitor'],

  // Pengembangan Produk Baru (DEPT-081)
  ['DEPT-081', 'SUBDEPT-095', 'Desain & Sampling'],
  ['DEPT-081', 'SUBDEPT-096', 'Proto & Pre-Production'],

  // Customer Support (DEPT-086)
  ['DEPT-086', 'SUBDEPT-097', 'CS Chat & Telepon'],
  ['DEPT-086', 'SUBDEPT-098', 'CS Digital & Media Sosial'],

  // Penanganan Komplain & Klaim (DEPT-088)
  ['DEPT-088', 'SUBDEPT-099', 'Klaim Produk & Retur'],
  ['DEPT-088', 'SUBDEPT-100', 'Investigasi & Root Cause Komplain'],
];

async function main() {
  // Build lookup map: deptCode → BigInt id
  const allDepts = await prisma.erpDepartment.findMany({
    select: { id: true, code: true },
  });
  const deptMap = new Map(allDepts.map((d) => [d.code, d.id]));

  let upserted = 0;
  let skipped = 0;
  for (const [deptCode, subCode, subName] of subDepts) {
    const departmentId = deptMap.get(deptCode);
    if (!departmentId) {
      console.warn(`  SKIP ${subCode}: departemen ${deptCode} tidak ditemukan`);
      skipped++;
      continue;
    }
    await prisma.erpSubDepartment.upsert({
      where: { code: subCode },
      update: { name: subName, departmentId },
      create: { code: subCode, name: subName, departmentId, isActive: true },
    });
    upserted++;
  }
  console.log(`md-sub-departments seed selesai: ${upserted} upserted, ${skipped} dilewati.`);
}

main()
  .catch((e) => { console.error(e); process.exit(1); })
  .finally(() => prisma.$disconnect());
