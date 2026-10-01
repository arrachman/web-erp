/**
 * One-off: seed 100+ realistic item types into md_item_types.
 * Run: npx ts-node --project tsconfig.json prisma/seed-erp-item-types.ts
 *
 * Idempotent: skips if table already has >=1 row.
 * Data covers manufacturing ERP context: bahan baku, jasa, kemasan, aset, dll.
 */
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

const ITEM_TYPES: { code: string; name: string }[] = [
  // --- Bahan Baku (Raw Materials)
  { code: 'BB-01', name: 'Bahan Baku Logam' },
  { code: 'BB-02', name: 'Bahan Baku Plastik' },
  { code: 'BB-03', name: 'Bahan Baku Karet' },
  { code: 'BB-04', name: 'Bahan Baku Kayu' },
  { code: 'BB-05', name: 'Bahan Baku Kain / Tekstil' },
  { code: 'BB-06', name: 'Bahan Baku Kimia' },
  { code: 'BB-07', name: 'Bahan Baku Kertas / Karton' },
  { code: 'BB-08', name: 'Bahan Baku Kaca' },
  { code: 'BB-09', name: 'Bahan Baku Keramik' },
  { code: 'BB-10', name: 'Bahan Baku Komposit' },
  { code: 'BB-11', name: 'Bahan Baku Elektronik (Komponen Dasar)' },
  { code: 'BB-12', name: 'Bahan Baku Pangan (Tepung, Gula, dll)' },
  { code: 'BB-13', name: 'Bahan Baku Minyak & Pelumas' },
  { code: 'BB-14', name: 'Bahan Baku Cat & Pelapisan' },
  { code: 'BB-15', name: 'Bahan Baku Serat & Filamen' },

  // --- Bahan Penolong (Auxiliary / Indirect Materials)
  { code: 'BP-01', name: 'Bahan Penolong Proses Produksi' },
  { code: 'BP-02', name: 'Bahan Penolong Finishing' },
  { code: 'BP-03', name: 'Bahan Penolong Pembersih' },
  { code: 'BP-04', name: 'Bahan Penolong Pelumas & Coolant' },
  { code: 'BP-05', name: 'Bahan Penolong Kimia Tambahan' },
  { code: 'BP-06', name: 'Bahan Penolong Energi (Gas Industri)' },
  { code: 'BP-07', name: 'Bahan Penolong Proteksi' },

  // --- Barang Dalam Proses / WIP
  { code: 'WIP-01', name: 'WIP — Tahap Cutting / Potong' },
  { code: 'WIP-02', name: 'WIP — Tahap Forming / Pembentukan' },
  { code: 'WIP-03', name: 'WIP — Tahap Machining / Permesinan' },
  { code: 'WIP-04', name: 'WIP — Tahap Welding / Pengelasan' },
  { code: 'WIP-05', name: 'WIP — Tahap Assembly / Perakitan' },
  { code: 'WIP-06', name: 'WIP — Tahap Painting / Pengecatan' },
  { code: 'WIP-07', name: 'WIP — Tahap Quality Control' },
  { code: 'WIP-08', name: 'WIP — Tahap Packing Awal' },

  // --- Barang Setengah Jadi (Semi-Finished Goods)
  { code: 'SFG-01', name: 'Barang Setengah Jadi — Sub-assembly A' },
  { code: 'SFG-02', name: 'Barang Setengah Jadi — Sub-assembly B' },
  { code: 'SFG-03', name: 'Barang Setengah Jadi — Komponen Intermediate' },
  { code: 'SFG-04', name: 'Barang Setengah Jadi — Blank / Preform' },
  { code: 'SFG-05', name: 'Barang Setengah Jadi — Modul Elektronik' },
  { code: 'SFG-06', name: 'Barang Setengah Jadi — Rangkaian Mekanik' },
  { code: 'SFG-07', name: 'Barang Setengah Jadi — Komponen Karet / Seal' },
  { code: 'SFG-08', name: 'Barang Setengah Jadi — Panel / Plat Olahan' },

  // --- Barang Jadi (Finished Goods)
  { code: 'FG-01', name: 'Barang Jadi — Produk Standar' },
  { code: 'FG-02', name: 'Barang Jadi — Produk Custom Order' },
  { code: 'FG-03', name: 'Barang Jadi — Produk Private Label' },
  { code: 'FG-04', name: 'Barang Jadi — Produk OEM' },
  { code: 'FG-05', name: 'Barang Jadi — Produk Ekspor' },
  { code: 'FG-06', name: 'Barang Jadi — Produk Lokal' },
  { code: 'FG-07', name: 'Barang Jadi — Produk Bundling / Set' },
  { code: 'FG-08', name: 'Barang Jadi — Produk Replacement / Spare' },

  // --- Barang Dagang (Trading Goods — beli-jual tanpa proses)
  { code: 'TD-01', name: 'Barang Dagang — Resale Langsung' },
  { code: 'TD-02', name: 'Barang Dagang — Distribusi' },
  { code: 'TD-03', name: 'Barang Dagang — Impor Resale' },
  { code: 'TD-04', name: 'Barang Dagang — Konsinyasi' },
  { code: 'TD-05', name: 'Barang Dagang — Sample / Demo' },

  // --- Suku Cadang (Spare Parts)
  { code: 'SP-01', name: 'Suku Cadang Mesin Produksi' },
  { code: 'SP-02', name: 'Suku Cadang Kendaraan Operasional' },
  { code: 'SP-03', name: 'Suku Cadang Alat Berat' },
  { code: 'SP-04', name: 'Suku Cadang Sistem Pneumatik' },
  { code: 'SP-05', name: 'Suku Cadang Sistem Hidrolik' },
  { code: 'SP-06', name: 'Suku Cadang Sistem Listrik' },
  { code: 'SP-07', name: 'Suku Cadang Instrumen / Sensor' },
  { code: 'SP-08', name: 'Suku Cadang Konveyor & Material Handling' },
  { code: 'SP-09', name: 'Suku Cadang Kompresor & Vakum' },
  { code: 'SP-10', name: 'Suku Cadang Forklift & Palet Jack' },

  // --- Bahan Kemasan (Packaging Materials)
  { code: 'PKG-01', name: 'Kemasan Primer — Botol / Kaleng' },
  { code: 'PKG-02', name: 'Kemasan Primer — Sachet / Pouch' },
  { code: 'PKG-03', name: 'Kemasan Primer — Blister Pack' },
  { code: 'PKG-04', name: 'Kemasan Sekunder — Karton Box' },
  { code: 'PKG-05', name: 'Kemasan Sekunder — Wrapper / Shrink' },
  { code: 'PKG-06', name: 'Kemasan Tersier — Palet / Crate' },
  { code: 'PKG-07', name: 'Label & Stiker Produk' },
  { code: 'PKG-08', name: 'Pita Rekat / Strapping Band' },
  { code: 'PKG-09', name: 'Foam / Bubble Wrap / Busa Pelindung' },
  { code: 'PKG-10', name: 'Kantong Plastik / Poly Bag' },

  // --- Alat & Perkakas (Tools & Equipment — tidak disusutkan per operasi)
  { code: 'TL-01', name: 'Perkakas Tangan (Hand Tools)' },
  { code: 'TL-02', name: 'Perkakas Presisi (Jig, Fixture, Gauge)' },
  { code: 'TL-03', name: 'Alat Potong (Cutting Tools — Insert, Drill, dll)' },
  { code: 'TL-04', name: 'Alat Ukur & Kalibrasi' },
  { code: 'TL-05', name: 'Alat Proteksi Diri (APD/PPE)' },
  { code: 'TL-06', name: 'Mold / Dies / Cetakan' },
  { code: 'TL-07', name: 'Electrode & Kawat Las' },
  { code: 'TL-08', name: 'Selang & Konektor Industri' },

  // --- Jasa (Services — non-physical)
  { code: 'SVC-01', name: 'Jasa Makloon / Toll Manufacturing' },
  { code: 'SVC-02', name: 'Jasa Perawatan & Perbaikan (MRO)' },
  { code: 'SVC-03', name: 'Jasa Kalibrasi & Inspeksi' },
  { code: 'SVC-04', name: 'Jasa Transportasi & Pengiriman' },
  { code: 'SVC-05', name: 'Jasa Konsultasi Teknis' },
  { code: 'SVC-06', name: 'Jasa Instalasi & Commissioning' },
  { code: 'SVC-07', name: 'Jasa Pelatihan & Training' },
  { code: 'SVC-08', name: 'Jasa Kebersihan & Sanitasi' },
  { code: 'SVC-09', name: 'Jasa Keamanan (Security)' },
  { code: 'SVC-10', name: 'Jasa Outsourcing Tenaga Kerja' },
  { code: 'SVC-11', name: 'Jasa IT & Software' },
  { code: 'SVC-12', name: 'Jasa Uji Laboratorium' },

  // --- Aset & Inventaris Tetap
  { code: 'ASSET-01', name: 'Aset Mesin Produksi' },
  { code: 'ASSET-02', name: 'Aset Kendaraan' },
  { code: 'ASSET-03', name: 'Aset Peralatan Kantor' },
  { code: 'ASSET-04', name: 'Aset IT & Komputer' },
  { code: 'ASSET-05', name: 'Aset Furnitur & Inventaris Gedung' },
  { code: 'ASSET-06', name: 'Aset Alat Berat & Material Handling' },

  // --- Barang Konsumsi (Consumables — habis dipakai, bukan stok jual)
  { code: 'CONS-01', name: 'Konsumsi ATK (Alat Tulis Kantor)' },
  { code: 'CONS-02', name: 'Konsumsi Kebersihan (Sabun, MOP, dll)' },
  { code: 'CONS-03', name: 'Konsumsi Listrik & Utilitas' },
  { code: 'CONS-04', name: 'Konsumsi Lab (Reagen, Pipette, dll)' },
  { code: 'CONS-05', name: 'Konsumsi Kantin / Konsumsi Rapat' },
  { code: 'CONS-06', name: 'Konsumsi Seragam & APD Habis Pakai' },

  // --- Voucher & Non-Fisik
  { code: 'VCH-01', name: 'Voucher Diskon / Promo' },
  { code: 'VCH-02', name: 'Voucher Gift / Hadiah' },
  { code: 'VCH-03', name: 'Subscription / Langganan' },
  { code: 'VCH-04', name: 'Lisensi Software' },
  { code: 'VCH-05', name: 'Hak Paten / Royalti' },

  // --- Scrap & Retur
  { code: 'SCRAP-01', name: 'Scrap Produksi (Sisa Potong, Reject)' },
  { code: 'SCRAP-02', name: 'Retur dari Pelanggan' },
  { code: 'SCRAP-03', name: 'Barang Rusak / Cacat Stok' },
  { code: 'SCRAP-04', name: 'Scrap Logam Bekas' },
];

async function main(): Promise<void> {
  const existing = await prisma.erpItemKind.count();
  if (existing > 0) {
    console.log(`md_item_types already has ${existing} rows — skipping.`);
    return;
  }

  const created = await prisma.erpItemKind.createMany({
    data: ITEM_TYPES.map(({ code, name }) => ({ code, name, isActive: true })),
    skipDuplicates: true,
  });
  console.log(`+ md_item_types: inserted ${created.count} rows.`);
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(() => prisma.$disconnect());
