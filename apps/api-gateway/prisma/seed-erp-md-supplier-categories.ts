/**
 * Seed 100 kategori supplier realistis untuk manufaktur Indonesia.
 * Diorganisir berdasarkan asal geografis × jenis material/jasa.
 * Idempotent via upsert on (code, type).
 */
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

const SUPPLIER_CATEGORIES = [
  // ── Lokal (20) ──────────────────────────────────────────────────────────────
  { code: 'LOK-RM-01', name: 'Lokal - Bahan Baku Umum' },
  { code: 'LOK-RM-02', name: 'Lokal - Bahan Baku Kimia' },
  { code: 'LOK-RM-03', name: 'Lokal - Bahan Baku Logam' },
  { code: 'LOK-RM-04', name: 'Lokal - Bahan Baku Plastik' },
  { code: 'LOK-RM-05', name: 'Lokal - Bahan Baku Tekstil' },
  { code: 'LOK-SP-01', name: 'Lokal - Suku Cadang Mesin' },
  { code: 'LOK-SP-02', name: 'Lokal - Suku Cadang Kendaraan' },
  { code: 'LOK-SP-03', name: 'Lokal - Suku Cadang Elektronik' },
  { code: 'LOK-PKG-01', name: 'Lokal - Kemasan Karton & Kertas' },
  { code: 'LOK-PKG-02', name: 'Lokal - Kemasan Plastik & Fleksibel' },
  { code: 'LOK-PKG-03', name: 'Lokal - Kemasan Botol & Jar' },
  { code: 'LOK-KIM-01', name: 'Lokal - Bahan Kimia Industri' },
  { code: 'LOK-KIM-02', name: 'Lokal - Bahan Kimia Pembersih' },
  { code: 'LOK-MRO-01', name: 'Lokal - MRO Umum' },
  { code: 'LOK-MRO-02', name: 'Lokal - Alat Kerja & Perkakas' },
  { code: 'LOK-MRO-03', name: 'Lokal - Pelumas & Oli' },
  { code: 'LOK-JSA-01', name: 'Lokal - Jasa Kontraktor' },
  { code: 'LOK-JSA-02', name: 'Lokal - Jasa Maintenance' },
  { code: 'LOK-UTIL',   name: 'Lokal - Utilitas (Listrik/Air/Gas)' },
  { code: 'LOK-ATK',    name: 'Lokal - ATK & Perlengkapan Kantor' },

  // ── Impor Umum (10) ─────────────────────────────────────────────────────────
  { code: 'IMP-RM-01',  name: 'Impor - Bahan Baku Umum' },
  { code: 'IMP-RM-02',  name: 'Impor - Bahan Baku Kimia' },
  { code: 'IMP-RM-03',  name: 'Impor - Bahan Baku Logam & Baja' },
  { code: 'IMP-SP-01',  name: 'Impor - Suku Cadang Mesin' },
  { code: 'IMP-SP-02',  name: 'Impor - Suku Cadang Elektronik' },
  { code: 'IMP-PKG-01', name: 'Impor - Kemasan Khusus' },
  { code: 'IMP-KIM-01', name: 'Impor - Bahan Kimia Khusus' },
  { code: 'IMP-AST-01', name: 'Impor - Mesin & Peralatan Produksi' },
  { code: 'IMP-AST-02', name: 'Impor - Instrumen & Alat Ukur' },
  { code: 'IMP-IT',     name: 'Impor - Perangkat IT & Teknologi' },

  // ── China (12) ──────────────────────────────────────────────────────────────
  { code: 'CHN-RM-01',  name: 'China - Bahan Baku Logam' },
  { code: 'CHN-RM-02',  name: 'China - Bahan Baku Plastik' },
  { code: 'CHN-RM-03',  name: 'China - Bahan Baku Kimia' },
  { code: 'CHN-SP-01',  name: 'China - Suku Cadang Mesin' },
  { code: 'CHN-SP-02',  name: 'China - Komponen Elektronik' },
  { code: 'CHN-PKG-01', name: 'China - Kemasan & Label' },
  { code: 'CHN-AST-01', name: 'China - Mesin Produksi' },
  { code: 'CHN-AST-02', name: 'China - Tooling & Cetakan' },
  { code: 'CHN-MRO-01', name: 'China - MRO & Perkakas' },
  { code: 'CHN-TKS-01', name: 'China - Bahan Tekstil' },
  { code: 'CHN-ELK-01', name: 'China - Komponen Listrik' },
  { code: 'CHN-PLT-01', name: 'China - Bahan & Produk Plastik' },

  // ── Jepang (8) ──────────────────────────────────────────────────────────────
  { code: 'JPN-SP-01',  name: 'Jepang - Suku Cadang Mesin Presisi' },
  { code: 'JPN-ELK-01', name: 'Jepang - Komponen Elektronik' },
  { code: 'JPN-AST-01', name: 'Jepang - Mesin & Robot Industri' },
  { code: 'JPN-KIM-01', name: 'Jepang - Bahan Kimia Spesial' },
  { code: 'JPN-LGM-01', name: 'Jepang - Logam & Material Spesial' },
  { code: 'JPN-MRO-01', name: 'Jepang - MRO Premium' },
  { code: 'JPN-PLT-01', name: 'Jepang - Plastik Engineering Grade' },
  { code: 'JPN-RM-01',  name: 'Jepang - Bahan Baku Premium' },

  // ── Korea (6) ───────────────────────────────────────────────────────────────
  { code: 'KOR-ELK-01', name: 'Korea - Komponen Elektronik' },
  { code: 'KOR-LGM-01', name: 'Korea - Baja & Logam' },
  { code: 'KOR-KIM-01', name: 'Korea - Bahan Kimia' },
  { code: 'KOR-SP-01',  name: 'Korea - Suku Cadang' },
  { code: 'KOR-AST-01', name: 'Korea - Mesin & Peralatan' },
  { code: 'KOR-PLT-01', name: 'Korea - Plastik & Polimer' },

  // ── Eropa (8) ───────────────────────────────────────────────────────────────
  { code: 'ERP-SP-01',  name: 'Eropa - Suku Cadang Presisi' },
  { code: 'ERP-AST-01', name: 'Eropa - Mesin & Sistem Otomasi' },
  { code: 'ERP-KIM-01', name: 'Eropa - Bahan Kimia Industri' },
  { code: 'ERP-ELK-01', name: 'Eropa - Komponen Elektronik Premium' },
  { code: 'ERP-LGM-01', name: 'Eropa - Logam & Baja Spesial' },
  { code: 'ERP-MRO-01', name: 'Eropa - MRO & Alat Presisi' },
  { code: 'ERP-IT-01',  name: 'Eropa - Perangkat Lunak & IT' },
  { code: 'ERP-RM-01',  name: 'Eropa - Bahan Baku Premium' },

  // ── Amerika Serikat (6) ─────────────────────────────────────────────────────
  { code: 'USA-AST-01', name: 'Amerika - Mesin & Teknologi' },
  { code: 'USA-KIM-01', name: 'Amerika - Bahan Kimia' },
  { code: 'USA-ELK-01', name: 'Amerika - Komponen Elektronik' },
  { code: 'USA-SP-01',  name: 'Amerika - Suku Cadang' },
  { code: 'USA-IT-01',  name: 'Amerika - Perangkat Lunak & IT' },
  { code: 'USA-RM-01',  name: 'Amerika - Bahan Baku' },

  // ── India (5) ───────────────────────────────────────────────────────────────
  { code: 'IND-RM-01',  name: 'India - Bahan Baku Tekstil' },
  { code: 'IND-KIM-01', name: 'India - Bahan Kimia' },
  { code: 'IND-TKS-01', name: 'India - Bahan Tekstil' },
  { code: 'IND-LGM-01', name: 'India - Logam & Baja' },
  { code: 'IND-SP-01',  name: 'India - Suku Cadang' },

  // ── ASEAN (7) ───────────────────────────────────────────────────────────────
  { code: 'ASN-RM-01',  name: 'ASEAN - Bahan Baku Pertanian' },
  { code: 'ASN-TKS-01', name: 'ASEAN - Bahan Tekstil' },
  { code: 'ASN-KYU-01', name: 'ASEAN - Kayu & Material Alam' },
  { code: 'ASN-KIM-01', name: 'ASEAN - Bahan Kimia' },
  { code: 'ASN-LGM-01', name: 'ASEAN - Logam Non-Ferrous' },
  { code: 'ASN-PKG-01', name: 'ASEAN - Kemasan' },
  { code: 'ASN-PLT-01', name: 'ASEAN - Plastik' },

  // ── Timur Tengah (4) ────────────────────────────────────────────────────────
  { code: 'TMR-BHB-01', name: 'Timur Tengah - Energi & Bahan Bakar' },
  { code: 'TMR-KIM-01', name: 'Timur Tengah - Petrokimia' },
  { code: 'TMR-RM-01',  name: 'Timur Tengah - Bahan Baku Mineral' },
  { code: 'TMR-PLT-01', name: 'Timur Tengah - Plastik Petrochemical' },

  // ── Australia (3) ───────────────────────────────────────────────────────────
  { code: 'AUS-RM-01',  name: 'Australia - Bahan Baku Mineral' },
  { code: 'AUS-KIM-01', name: 'Australia - Bahan Kimia' },
  { code: 'AUS-AST-01', name: 'Australia - Peralatan Tambang' },

  // ── Kategori Khusus (11) ────────────────────────────────────────────────────
  { code: 'SPK-UMKM',   name: 'Pemasok UMKM Lokal' },
  { code: 'SPK-BUMN',   name: 'Pemasok BUMN/BUMD' },
  { code: 'SPK-PKP',    name: 'Pemasok PKP (Pengusaha Kena Pajak)' },
  { code: 'SPK-NPKP',   name: 'Pemasok Non-PKP' },
  { code: 'SPK-DIST',   name: 'Distributor & Agen Tunggal' },
  { code: 'SPK-MANF',   name: 'Produsen Langsung (Direct Manufacturer)' },
  { code: 'SPK-SUBS',   name: 'Subkontraktor Manufaktur' },
  { code: 'SPK-SERV',   name: 'Penyedia Jasa Teknik' },
  { code: 'SPK-CONS',   name: 'Konsultan & Profesional' },
  { code: 'SPK-TRANS',  name: 'Jasa Transportasi & Logistik' },
  { code: 'SPK-CERT',   name: 'Pemasok Bersertifikat (ISO/SNI)' },
];

async function main() {
  console.log(`Seeding ${SUPPLIER_CATEGORIES.length} supplier categories...`);
  let upserted = 0;

  for (const cat of SUPPLIER_CATEGORIES) {
    await prisma.erpPartnerSubCategory.upsert({
      where: { code_type: { code: cat.code, type: 'SUPPLIER' } },
      create: { code: cat.code, name: cat.name, type: 'SUPPLIER', isActive: true },
      update: { name: cat.name, isActive: true },
    });
    upserted++;
  }

  console.log(`Done. Upserted ${upserted} supplier categories.`);
}

main()
  .catch((e) => { console.error(e); process.exit(1); })
  .finally(() => prisma.$disconnect());
