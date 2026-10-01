/**
 * Seed 100 departemen untuk md_departments.
 * Mencakup departemen umum perusahaan manufaktur Indonesia.
 * Idempotent via upsert on `code`.
 */
import { PrismaClient } from '@prisma/client';
const prisma = new PrismaClient();

const departments = [
  // Manajemen (1–5)
  { code: 'DEPT-001', name: 'Direksi' },
  { code: 'DEPT-002', name: 'Komisaris' },
  { code: 'DEPT-003', name: 'Sekretaris Perusahaan' },
  { code: 'DEPT-004', name: 'Manajemen Risiko' },
  { code: 'DEPT-005', name: 'Hubungan Investor' },

  // Keuangan & Akuntansi (6–15)
  { code: 'DEPT-006', name: 'Keuangan Umum' },
  { code: 'DEPT-007', name: 'Akuntansi' },
  { code: 'DEPT-008', name: 'Perpajakan' },
  { code: 'DEPT-009', name: 'Treasury & Cash Management' },
  { code: 'DEPT-010', name: 'Audit Internal' },
  { code: 'DEPT-011', name: 'Penganggaran & Perencanaan Keuangan' },
  { code: 'DEPT-012', name: 'Akuntansi Biaya' },
  { code: 'DEPT-013', name: 'Piutang Usaha' },
  { code: 'DEPT-014', name: 'Hutang Usaha' },
  { code: 'DEPT-015', name: 'Pelaporan Keuangan' },

  // Sumber Daya Manusia (16–24)
  { code: 'DEPT-016', name: 'Rekrutmen & Seleksi' },
  { code: 'DEPT-017', name: 'Pengembangan SDM' },
  { code: 'DEPT-018', name: 'Payroll & Benefit' },
  { code: 'DEPT-019', name: 'Hubungan Industrial' },
  { code: 'DEPT-020', name: 'Keselamatan & Kesehatan Kerja' },
  { code: 'DEPT-021', name: 'Kompensasi & Benefit' },
  { code: 'DEPT-022', name: 'Pelatihan & Pengembangan' },
  { code: 'DEPT-023', name: 'Administrasi Kepegawaian' },
  { code: 'DEPT-024', name: 'Budaya Perusahaan' },

  // Teknologi Informasi (25–32)
  { code: 'DEPT-025', name: 'Infrastruktur IT' },
  { code: 'DEPT-026', name: 'Pengembangan Sistem & Aplikasi' },
  { code: 'DEPT-027', name: 'Keamanan Siber' },
  { code: 'DEPT-028', name: 'Helpdesk & IT Support' },
  { code: 'DEPT-029', name: 'Database & Integrasi Data' },
  { code: 'DEPT-030', name: 'ERP & Sistem Bisnis' },
  { code: 'DEPT-031', name: 'Jaringan & Telekomunikasi' },
  { code: 'DEPT-032', name: 'Otomasi & Digitalisasi' },

  // Produksi (33–42)
  { code: 'DEPT-033', name: 'Produksi Line A' },
  { code: 'DEPT-034', name: 'Produksi Line B' },
  { code: 'DEPT-035', name: 'Produksi Line C' },
  { code: 'DEPT-036', name: 'Perencanaan Produksi' },
  { code: 'DEPT-037', name: 'Proses & Kontrol Produksi' },
  { code: 'DEPT-038', name: 'Maintenance & Teknik' },
  { code: 'DEPT-039', name: 'Utilitas & Energi Pabrik' },
  { code: 'DEPT-040', name: 'Teknik Industri' },
  { code: 'DEPT-041', name: 'Manajemen Kapasitas' },
  { code: 'DEPT-042', name: 'Efisiensi Operasional' },

  // Quality Control & Assurance (43–49)
  { code: 'DEPT-043', name: 'QC Incoming' },
  { code: 'DEPT-044', name: 'QC In-Process' },
  { code: 'DEPT-045', name: 'QC Final' },
  { code: 'DEPT-046', name: 'Quality Assurance & Standar' },
  { code: 'DEPT-047', name: 'Laboratorium & Pengujian' },
  { code: 'DEPT-048', name: 'Kalibrasi & Instrumen' },
  { code: 'DEPT-049', name: 'Sistem Manajemen Mutu' },

  // Logistik & Pergudangan (50–57)
  { code: 'DEPT-050', name: 'Gudang Bahan Baku' },
  { code: 'DEPT-051', name: 'Gudang Produk Jadi' },
  { code: 'DEPT-052', name: 'Distribusi & Pengiriman' },
  { code: 'DEPT-053', name: 'Manajemen Inventori' },
  { code: 'DEPT-054', name: 'Ekspedisi & Kurir' },
  { code: 'DEPT-055', name: 'Impor & Ekspor' },
  { code: 'DEPT-056', name: 'Perencanaan Rantai Pasok' },
  { code: 'DEPT-057', name: 'Manajemen Transportasi' },

  // Pembelian (58–63)
  { code: 'DEPT-058', name: 'Pembelian Bahan Baku' },
  { code: 'DEPT-059', name: 'Pembelian Aset & Peralatan' },
  { code: 'DEPT-060', name: 'Pembelian Umum' },
  { code: 'DEPT-061', name: 'Evaluasi & Seleksi Vendor' },
  { code: 'DEPT-062', name: 'Negosiasi & Kontrak' },
  { code: 'DEPT-063', name: 'Strategic Sourcing' },

  // Penjualan (64–72)
  { code: 'DEPT-064', name: 'Sales Retail' },
  { code: 'DEPT-065', name: 'Sales Wholesale & Distributor' },
  { code: 'DEPT-066', name: 'Sales Ekspor' },
  { code: 'DEPT-067', name: 'Sales Regional Jawa' },
  { code: 'DEPT-068', name: 'Sales Regional Sumatra' },
  { code: 'DEPT-069', name: 'Sales Regional Kalimantan' },
  { code: 'DEPT-070', name: 'Sales Regional Sulawesi & Timur' },
  { code: 'DEPT-071', name: 'Key Account Management' },
  { code: 'DEPT-072', name: 'Inside Sales & Telesales' },

  // Pemasaran (73–79)
  { code: 'DEPT-073', name: 'Digital Marketing' },
  { code: 'DEPT-074', name: 'Brand Management' },
  { code: 'DEPT-075', name: 'Riset Pasar' },
  { code: 'DEPT-076', name: 'Promosi & Event' },
  { code: 'DEPT-077', name: 'Customer Relationship Management' },
  { code: 'DEPT-078', name: 'Konten & Komunikasi' },
  { code: 'DEPT-079', name: 'Trade Marketing' },

  // Riset & Pengembangan (80–85)
  { code: 'DEPT-080', name: 'Riset Produk' },
  { code: 'DEPT-081', name: 'Pengembangan Produk Baru' },
  { code: 'DEPT-082', name: 'Inovasi & Teknologi' },
  { code: 'DEPT-083', name: 'Desain Produk & Kemasan' },
  { code: 'DEPT-084', name: 'Formulasi & Proses Baru' },
  { code: 'DEPT-085', name: 'Kemitraan Riset' },

  // Customer Service (86–89)
  { code: 'DEPT-086', name: 'Customer Support' },
  { code: 'DEPT-087', name: 'After Sales Service' },
  { code: 'DEPT-088', name: 'Penanganan Komplain & Klaim' },
  { code: 'DEPT-089', name: 'Layanan Pelanggan Korporat' },

  // Hukum & Kepatuhan (90–93)
  { code: 'DEPT-090', name: 'Hukum & Perizinan' },
  { code: 'DEPT-091', name: 'Kepatuhan & Regulasi' },
  { code: 'DEPT-092', name: 'Hak Kekayaan Intelektual' },
  { code: 'DEPT-093', name: 'Manajemen Kontrak' },

  // Fasilitas & Umum (94–97)
  { code: 'DEPT-094', name: 'Fasilitas & Gedung' },
  { code: 'DEPT-095', name: 'Keamanan & Satpam' },
  { code: 'DEPT-096', name: 'Kebersihan & Lingkungan' },
  { code: 'DEPT-097', name: 'Transportasi Perusahaan' },

  // Teknologi Pabrik (98–100)
  { code: 'DEPT-098', name: 'Automasi & Robotik' },
  { code: 'DEPT-099', name: 'Mesin & Peralatan Produksi' },
  { code: 'DEPT-100', name: 'Keberlanjutan & Lingkungan Hidup' },
];

async function main() {
  let upserted = 0;
  for (const dept of departments) {
    await prisma.erpDepartment.upsert({
      where: { code: dept.code },
      update: { name: dept.name },
      create: { code: dept.code, name: dept.name, isActive: true },
    });
    upserted++;
  }
  console.log(`md-departments seed complete: ${upserted} departments upserted.`);
}

main().catch((e) => { console.error(e); process.exit(1); }).finally(() => prisma.$disconnect());
