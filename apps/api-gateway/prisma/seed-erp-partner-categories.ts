/**
 * Seed 100 real partner categories for manufacturing ERP context.
 * Covers all 4 kinds: CUSTOMER (30), SUPPLIER (35), SALESMAN (15), GENERAL (20).
 * Idempotent via upsert on `code`.
 *
 * Run: npx ts-node prisma/seed-erp-partner-categories.ts
 */
import { PrismaClient, ErpPartnerCategoryKind } from '@prisma/client';

const prisma = new PrismaClient();

const CATEGORIES: Array<{
  code: string;
  name: string;
  kind: ErpPartnerCategoryKind;
  salesTier?: number;
  legacyCode?: string;
  isActive?: boolean;
}> = [
  // ── CUSTOMER (30) ────────────────────────────────────────────────────────────
  { code: 'C-RETAIL',    name: 'Pelanggan Ritel',                     kind: 'CUSTOMER', salesTier: 5 },
  { code: 'C-WHLSL',    name: 'Pelanggan Grosir',                     kind: 'CUSTOMER', salesTier: 3 },
  { code: 'C-DISTR',    name: 'Distributor',                          kind: 'CUSTOMER', salesTier: 1 },
  { code: 'C-RESELLR',  name: 'Reseller',                             kind: 'CUSTOMER', salesTier: 2 },
  { code: 'C-AGENT',    name: 'Agen Penjualan',                       kind: 'CUSTOMER', salesTier: 2 },
  { code: 'C-MODTRD',   name: 'Modern Trade (Supermarket/Hypermarket)', kind: 'CUSTOMER', salesTier: 1 },
  { code: 'C-MINIMT',   name: 'Minimarket',                           kind: 'CUSTOMER', salesTier: 4 },
  { code: 'C-ONLNMKT',  name: 'Online Marketplace',                   kind: 'CUSTOMER', salesTier: 3 },
  { code: 'C-EXPORT',   name: 'Pelanggan Ekspor',                     kind: 'CUSTOMER', salesTier: 1 },
  { code: 'C-GOVT',     name: 'Instansi Pemerintah',                  kind: 'CUSTOMER', salesTier: 2 },
  { code: 'C-BUMN',     name: 'BUMN / BUMD',                          kind: 'CUSTOMER', salesTier: 2 },
  { code: 'C-MILITARY', name: 'TNI / Polri',                          kind: 'CUSTOMER', salesTier: 2 },
  { code: 'C-HOSPITAL', name: 'Rumah Sakit & Klinik',                 kind: 'CUSTOMER', salesTier: 3 },
  { code: 'C-PHARMA',   name: 'Industri Farmasi',                     kind: 'CUSTOMER', salesTier: 2 },
  { code: 'C-TEXTILE',  name: 'Industri Tekstil & Garmen',            kind: 'CUSTOMER', salesTier: 2 },
  { code: 'C-FOODBEV',  name: 'Industri Makanan & Minuman',           kind: 'CUSTOMER', salesTier: 2 },
  { code: 'C-CHEMICAL', name: 'Industri Kimia & Petrokimia',          kind: 'CUSTOMER', salesTier: 2 },
  { code: 'C-AUTOMTV',  name: 'Industri Otomotif & Komponen',         kind: 'CUSTOMER', salesTier: 1 },
  { code: 'C-MINING',   name: 'Pertambangan & Energi',                kind: 'CUSTOMER', salesTier: 1 },
  { code: 'C-CONSTR',   name: 'Konstruksi & Properti',                kind: 'CUSTOMER', salesTier: 2 },
  { code: 'C-AGRI',     name: 'Pertanian & Perkebunan',               kind: 'CUSTOMER', salesTier: 3 },
  { code: 'C-FISHERY',  name: 'Perikanan & Kelautan',                 kind: 'CUSTOMER', salesTier: 4 },
  { code: 'C-HOTEL',    name: 'Perhotelan & Pariwisata',              kind: 'CUSTOMER', salesTier: 3 },
  { code: 'C-CATERING', name: 'Katering & Restoran',                  kind: 'CUSTOMER', salesTier: 4 },
  { code: 'C-EDU',      name: 'Lembaga Pendidikan',                   kind: 'CUSTOMER', salesTier: 4 },
  { code: 'C-OEM',      name: 'OEM Manufacturer',                     kind: 'CUSTOMER', salesTier: 1 },
  { code: 'C-WRKSHP',   name: 'Bengkel & Service Center',             kind: 'CUSTOMER', salesTier: 4 },
  { code: 'C-COOPERAT', name: 'Koperasi',                             kind: 'CUSTOMER', salesTier: 4 },
  { code: 'C-PERSONAL', name: 'Perorangan / Individual',              kind: 'CUSTOMER', salesTier: 5 },
  { code: 'C-STARTUP',  name: 'Startup & UKM Manufaktur',             kind: 'CUSTOMER', salesTier: 3 },

  // ── SUPPLIER (35) ────────────────────────────────────────────────────────────
  { code: 'S-RAW-MET',  name: 'Bahan Baku Logam & Besi',             kind: 'SUPPLIER' },
  { code: 'S-RAW-PLST', name: 'Bahan Baku Plastik & Polimer',        kind: 'SUPPLIER' },
  { code: 'S-RAW-CHEM', name: 'Bahan Baku Kimia & Additive',         kind: 'SUPPLIER' },
  { code: 'S-RAW-TXTL', name: 'Bahan Baku Tekstil & Serat',          kind: 'SUPPLIER' },
  { code: 'S-RAW-WOOD', name: 'Bahan Baku Kayu & Plywood',           kind: 'SUPPLIER' },
  { code: 'S-RAW-PAPER', name: 'Bahan Baku Kertas & Karton',         kind: 'SUPPLIER' },
  { code: 'S-RAW-RUBBR', name: 'Bahan Baku Karet & Elastomer',       kind: 'SUPPLIER' },
  { code: 'S-RAW-GLASS', name: 'Bahan Baku Kaca & Keramik',          kind: 'SUPPLIER' },
  { code: 'S-PKG-BOX',  name: 'Kemasan Karton & Box',                kind: 'SUPPLIER' },
  { code: 'S-PKG-FILM', name: 'Kemasan Film & Plastik Fleksibel',    kind: 'SUPPLIER' },
  { code: 'S-PKG-LABEL', name: 'Label, Stiker & Barcode',            kind: 'SUPPLIER' },
  { code: 'S-PKG-BAG',  name: 'Kemasan Kantong & Sachet',            kind: 'SUPPLIER' },
  { code: 'S-PKG-CAN',  name: 'Kemasan Kaleng, Drum & Botol',        kind: 'SUPPLIER' },
  { code: 'S-SP-MECH',  name: 'Suku Cadang Mekanik',                 kind: 'SUPPLIER' },
  { code: 'S-SP-ELEC',  name: 'Komponen Elektronik & Elektrik',      kind: 'SUPPLIER' },
  { code: 'S-SP-HYDR',  name: 'Komponen Hidrolik & Pneumatik',       kind: 'SUPPLIER' },
  { code: 'S-SP-BELT',  name: 'Belt, Chain & Coupling',              kind: 'SUPPLIER' },
  { code: 'S-SP-BRG',   name: 'Bearing, Seal & O-Ring',              kind: 'SUPPLIER' },
  { code: 'S-CONS-LUBE', name: 'Pelumas, Oli & Grease',              kind: 'SUPPLIER' },
  { code: 'S-CONS-WEL', name: 'Material Las, Potong & Abrasif',      kind: 'SUPPLIER' },
  { code: 'S-CONS-CHEM', name: 'Bahan Kimia Proses & Cleaning',      kind: 'SUPPLIER' },
  { code: 'S-CONS-SFTY', name: 'APD, Safety & K3',                   kind: 'SUPPLIER' },
  { code: 'S-CONS-ATK', name: 'Alat Tulis & Perlengkapan Kantor',    kind: 'SUPPLIER' },
  { code: 'S-TOOL-HND', name: 'Hand Tools & Power Tools',            kind: 'SUPPLIER' },
  { code: 'S-TOOL-MOLD', name: 'Mold, Cetakan & Jig Fixture',        kind: 'SUPPLIER' },
  { code: 'S-EQ-MACH',  name: 'Mesin Produksi & Equipment',          kind: 'SUPPLIER' },
  { code: 'S-EQ-HVAC',  name: 'HVAC, Refrigerasi & Kompresor',       kind: 'SUPPLIER' },
  { code: 'S-EQ-IT',    name: 'Hardware IT, Software & Lisensi',     kind: 'SUPPLIER' },
  { code: 'S-SVC-MNT',  name: 'Jasa Maintenance & Repair',           kind: 'SUPPLIER' },
  { code: 'S-SVC-TRNSP', name: 'Jasa Transportasi & Ekspedisi',      kind: 'SUPPLIER' },
  { code: 'S-SVC-CLEAN', name: 'Jasa Kebersihan & Housekeeping',     kind: 'SUPPLIER' },
  { code: 'S-SVC-SEC',  name: 'Jasa Keamanan & Satpam',              kind: 'SUPPLIER' },
  { code: 'S-SVC-CONS', name: 'Jasa Konsultan & Profesional',        kind: 'SUPPLIER' },
  { code: 'S-ENERGY',   name: 'Listrik, Gas, Air & Energi',          kind: 'SUPPLIER' },
  { code: 'S-IMPORT',   name: 'Pemasok Impor / Luar Negeri',         kind: 'SUPPLIER' },

  // ── SALESMAN (15) ────────────────────────────────────────────────────────────
  { code: 'SM-INT-HQ',  name: 'Sales Internal Kantor Pusat',         kind: 'SALESMAN' },
  { code: 'SM-INT-JKT', name: 'Sales Internal Wilayah Jakarta',      kind: 'SALESMAN' },
  { code: 'SM-INT-WJB', name: 'Sales Internal Jawa Barat',           kind: 'SALESMAN' },
  { code: 'SM-INT-WJT', name: 'Sales Internal Jawa Timur & Tengah',  kind: 'SALESMAN' },
  { code: 'SM-INT-BAL', name: 'Sales Internal Bali & Nusra',         kind: 'SALESMAN' },
  { code: 'SM-INT-SUM', name: 'Sales Internal Sumatera',             kind: 'SALESMAN' },
  { code: 'SM-INT-KAL', name: 'Sales Internal Kalimantan',           kind: 'SALESMAN' },
  { code: 'SM-INT-SUL', name: 'Sales Internal Sulawesi & Indonesia Timur', kind: 'SALESMAN' },
  { code: 'SM-EXT-AGN', name: 'Agen Penjualan Eksternal',            kind: 'SALESMAN' },
  { code: 'SM-EXT-BRK', name: 'Broker & Makelar',                    kind: 'SALESMAN' },
  { code: 'SM-EXT-RSL', name: 'Reseller Independen',                 kind: 'SALESMAN' },
  { code: 'SM-EXPORT',  name: 'Sales Ekspor & Internasional',        kind: 'SALESMAN' },
  { code: 'SM-KEYACC',  name: 'Key Account Manager',                 kind: 'SALESMAN' },
  { code: 'SM-ONLINE',  name: 'Tim Penjualan Online & Marketplace',  kind: 'SALESMAN' },
  { code: 'SM-TELE',    name: 'Telemarketing & Inbound Sales',       kind: 'SALESMAN' },

  // ── GENERAL (20) ─────────────────────────────────────────────────────────────
  { code: 'G-BANK',     name: 'Perbankan & Lembaga Keuangan',        kind: 'GENERAL' },
  { code: 'G-INS',      name: 'Perusahaan Asuransi',                 kind: 'GENERAL' },
  { code: 'G-LEASE',    name: 'Perusahaan Leasing & Multifinance',   kind: 'GENERAL' },
  { code: 'G-GOVT-REG', name: 'Instansi Regulasi & Perizinan',       kind: 'GENERAL' },
  { code: 'G-CUSTOMS',  name: 'Bea Cukai & Karantina',               kind: 'GENERAL' },
  { code: 'G-NOTARY',   name: 'Notaris & PPAT',                      kind: 'GENERAL' },
  { code: 'G-LAW',      name: 'Kantor Hukum & Advokat',              kind: 'GENERAL' },
  { code: 'G-KAP',      name: 'Kantor Akuntan Publik (KAP)',         kind: 'GENERAL' },
  { code: 'G-TAXPCONS', name: 'Konsultan Pajak',                     kind: 'GENERAL' },
  { code: 'G-FF',       name: 'Freight Forwarder & EMKL',            kind: 'GENERAL' },
  { code: 'G-SHIP',     name: 'Pelayaran & Kargo Udara',             kind: 'GENERAL' },
  { code: 'G-TRDASSOC', name: 'Asosiasi Industri & Perdagangan',     kind: 'GENERAL' },
  { code: 'G-CHAMBER',  name: 'Kamar Dagang (KADIN / APINDO)',       kind: 'GENERAL' },
  { code: 'G-MEDIA',    name: 'Media, Periklanan & Branding',        kind: 'GENERAL' },
  { code: 'G-NGO',      name: 'Yayasan & LSM',                       kind: 'GENERAL' },
  { code: 'G-UNIV',     name: 'Universitas & Lembaga Riset',         kind: 'GENERAL' },
  { code: 'G-JV',       name: 'Mitra Joint Venture & Konsorsium',    kind: 'GENERAL' },
  { code: 'G-INVESTOR', name: 'Investor & Pemegang Saham',           kind: 'GENERAL' },
  { code: 'G-EMBASSY',  name: 'Kedutaan Besar & Lembaga Internasional', kind: 'GENERAL' },
  { code: 'G-MISC',     name: 'Lain-lain (Umum)',                    kind: 'GENERAL' },
];

async function main(): Promise<void> {
  console.log(`Seeding ${CATEGORIES.length} partner categories...`);

  let upserted = 0;
  for (const cat of CATEGORIES) {
    await prisma.erpPartnerCategory.upsert({
      where: { code: cat.code },
      create: {
        code: cat.code,
        name: cat.name,
        kind: cat.kind,
        salesTier: cat.salesTier ?? null,
        legacyCode: cat.legacyCode ?? null,
        isActive: cat.isActive ?? true,
      },
      update: {
        name: cat.name,
        kind: cat.kind,
        salesTier: cat.salesTier ?? null,
        isActive: cat.isActive ?? true,
      },
    });
    upserted++;
  }

  console.log(`✓ md_partner_categories: ${upserted} upserted.`);
}

main()
  .catch((e) => { console.error(e); process.exit(1); })
  .finally(() => prisma.$disconnect());
