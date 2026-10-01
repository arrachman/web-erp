/**
 * Replace 100 dummy md_partners with 100 real customers from legacy MyERP+ data.
 * Run: npx ts-node prisma/seed-md-partners-real.ts
 *
 * Actions:
 *   1. Delete all DUMMY-PRT-* partners (no addresses/contacts yet, safe to hard-delete)
 *   2. Insert 100 real customers sourced from legacy 0_customer table (CI.* codes)
 *
 * FK lookups done at runtime — no hardcoded IDs.
 */
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

// ---------------------------------------------------------------------------
// Raw data extracted from legacy myerpplus_serenity.sql → 0_customer table
// Fields: legacyCode, name, addr, creditLimit (IDR), netDays
// ---------------------------------------------------------------------------
const LEGACY_CUSTOMERS: Array<{
  legacyCode: string;
  name: string;
  addr: string;
  creditLimit: number;
  netDays: number;
}> = [
  { legacyCode: 'CI.A001', name: 'PT Arvico Electronics Indonesia', addr: 'Kawasan Industri Jababeka I, Cikarang', creditLimit: 50000000, netDays: 0 },
  { legacyCode: 'CI.A002', name: 'PT Astron Optindo Industries', addr: 'Jl. Petak Baru No.58, Roa Malaka, Tambora', creditLimit: 200000000, netDays: 30 },
  { legacyCode: 'CI.A003', name: 'PT Asa Bintang Pratama', addr: 'Jl. Raya Jakarta-Serang KM.68, Modern Cikande', creditLimit: 1500000000, netDays: 30 },
  { legacyCode: 'CI.A004', name: 'CV Adhitama Mandiri Multindo', addr: 'Jl. Cilemahabang Raya No.8D, Cikarang', creditLimit: 50000000, netDays: 7 },
  { legacyCode: 'CI.A005', name: 'Absolute Zipper Co., Ltd', addr: '89/8 Moo 2, Kalong, Maung, Samut Sakhon, Thailand', creditLimit: 1620000000, netDays: 0 },
  { legacyCode: 'CI.A006', name: 'Aslams (Pvt) Ltd', addr: 'No.22-B Quarry Road, Colombo 12, Sri Lanka', creditLimit: 229500000, netDays: 14 },
  { legacyCode: 'CI.A007', name: 'PT Adiperkasa Anugrah Pratama', addr: 'Jl. Pajajaran No.10, Ds. Jatiuwung, Tangerang', creditLimit: 2300000000, netDays: 45 },
  { legacyCode: 'CI.A008', name: 'PT Adhi Chandra Jaya', addr: 'Jl. Kalisabi No.2, Jatiuwung, Tangerang', creditLimit: 50000000, netDays: 0 },
  { legacyCode: 'CI.A009', name: 'PT Alpha Integrated', addr: 'Jl. Industri Selatan II Blok MM-12, Jababeka', creditLimit: 700000000, netDays: 45 },
  { legacyCode: 'CI.A010', name: 'Astrindo Aditya Teknika', addr: 'Jl. Pangkalan II, Gg. Blaung No.1, Bekasi', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.A011', name: 'PT Arta Mina Jaya', addr: 'Pelabuhan Samudra Nizam Zachman, Muara Baru', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.A012', name: 'PT Alfaprima Panelindo', addr: 'Jl. Pancasila IV, Cicadas, Gunung Putri, Bogor', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.A013', name: 'CV Aryatech', addr: 'Graha Ciantra Indah Blok B4 No.4, Cibitung', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.A014', name: 'Ansun Multitech India Ltd', addr: 'B-123, Okhla Industrial Area Phase-1, New Delhi', creditLimit: 405000000, netDays: 14 },
  { legacyCode: 'CI.A016', name: 'Adhiwira Elektrikatama', addr: 'Jl. Puspa Raya Blok B 12 RT.001 RW.001, Tangerang', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.A018', name: 'Makmur Metal Zipper (Apin)', addr: 'Jl. Kp. Kebon Dua Ratus No.5 RT 005/RW 06', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.A019', name: 'A.E. Brassware Co., Ltd', addr: 'Bangkok, Thailand', creditLimit: 1755000000, netDays: 14 },
  { legacyCode: 'CI.A020', name: 'PT Andhika Mitra Persada', addr: 'Jl. Raya Sukamahi KM-1, Deltamas, Cikarang', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.A021', name: 'CV Agung Elektrindo', addr: 'Jl. Mamuju No.18, Bandung', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.A022', name: 'Active Zipper Ltd', addr: 'Plot 4&6, Road 3, Section 7, Mirpur, Dhaka', creditLimit: 1755000000, netDays: 14 },
  { legacyCode: 'CI.A023', name: 'Atlantic Industries Ltd', addr: 'B-49, BSCIC Industrial Estate, Tongi, Gazipur, Bangladesh', creditLimit: 1080000000, netDays: 14 },
  { legacyCode: 'CI.A024', name: 'PT Agung Sukses Jaya', addr: 'Jl. Swadaya RT.001 RW.002, Lambang Sari, Cikarang', creditLimit: 100000000, netDays: 30 },
  { legacyCode: 'CI.A025', name: 'PT Atalla Indonesia', addr: 'Jl. Putra 1 No.33A, Pasar Kemis, Tangerang', creditLimit: 50000000, netDays: 14 },
  { legacyCode: 'CI.B001', name: 'PT Bertigo Elektrindo', addr: 'Jl. Pangkalan II, Gang Blaung No.3, Bekasi', creditLimit: 100000000, netDays: 30 },
  { legacyCode: 'CI.B002', name: 'PT Best & Best Indonesia', addr: 'Jl. Faliman Jaya RT.05 RW.07, Jurumudi Baru, Tangerang', creditLimit: 450000000, netDays: 30 },
  { legacyCode: 'CI.B003', name: 'PT Bintang Timur Sakti', addr: 'Kebon Jeruk, Jakarta Barat', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.B004', name: 'B.T. Engineering Parts Co., Ltd', addr: 'Bangkok, Thailand', creditLimit: 1755000000, netDays: 14 },
  { legacyCode: 'CI.B005', name: 'PT Blessindo Bersama', addr: 'Jl. Cibaligo No.75E RT008/RW008, Cimahi', creditLimit: 150000000, netDays: 30 },
  { legacyCode: 'CI.B006', name: 'PT Bina Teknik Ciptamandiri', addr: 'Jl. Kian Santang No.2, Sangiang, Tangerang', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.B007', name: 'PT Berkah Anugrah Makmur Sejati', addr: 'Jl. Pipa Gas, Kp. Cohak RT.01/RW.06, Tangerang', creditLimit: 75000000, netDays: 14 },
  { legacyCode: 'CI.B008', name: 'Black Hawk Metals Pty Ltd', addr: '27 Abercrombie Street, Rocklea, QLD 4106, Australia', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.B009', name: 'CV Bumi Reksa Raya', addr: 'Jl. Palmerah Utara 2 No.23, Jakarta Barat', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.C001', name: 'PT Cakra Buana Perkasa', addr: 'Ruko Batuceper Permai Blok U No.11, Tangerang', creditLimit: 40000000, netDays: 14 },
  { legacyCode: 'CI.C002', name: 'PT Cahaya Global Infinity Pratama', addr: 'Ruko Concordia Blok SRC No.17, Serpong', creditLimit: 150000000, netDays: 60 },
  { legacyCode: 'CI.C003', name: 'Catame Inc', addr: '1930 Long Beach Ave., Los Angeles, CA, USA', creditLimit: 202500000, netDays: 30 },
  { legacyCode: 'CI.C004', name: 'PT Cheh Hwa Indonesia', addr: 'SFB Blok MM No.3&4, Kawasan Industri MM2100, Bekasi', creditLimit: 750000000, netDays: 30 },
  { legacyCode: 'CI.C005', name: 'PT Cahaya Terang Makmur', addr: 'Ruko Pusat Niaga Cibodas Blok D-25, Tangerang', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.C006', name: 'Citizen Hardware Stores', addr: 'No.400, Sri Sangaraja Mawatha, Colombo 10, Sri Lanka', creditLimit: 270000000, netDays: 14 },
  { legacyCode: 'CI.C007', name: 'PT Citra Interlindo', addr: 'Jl. Angsana 1 Blok A 1-3, Cikarang', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.C008', name: 'Coats Bangladesh Ltd', addr: 'Dhaka, Bangladesh', creditLimit: 202500000, netDays: 14 },
  { legacyCode: 'CI.C009', name: 'PT Central Wire Industry', addr: 'Jl. Rungkut Industri Raya No.17A, Surabaya', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.C010', name: 'PT Cahaya Angkasa Abadi', addr: 'Jl. Berbek Industri I, Kawasan SIER, Surabaya', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.C011', name: 'Chok Sermchai Trading Co., Ltd', addr: 'Bangkok, Thailand', creditLimit: 1755000000, netDays: 0 },
  { legacyCode: 'CI.C012', name: 'PT Central Nasional', addr: 'Komplek Kopo Mas Regency Blok I No.1, Bandung', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.C013', name: 'PT Comeca Indonesia', addr: 'Jl. Akasia 3, Blok A3 No.7, Delta Silicon, Cikarang', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.D001', name: 'PT Dharma Precision Parts', addr: 'Jl. Inti Raya Blok C III No.12, Kawasan KIIC, Karawang', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.D003', name: 'PT Danu Mekar Santoso', addr: 'Perum Citra Garden 2 Ext Blok BG 01/19, Jakarta Barat', creditLimit: 150000000, netDays: 30 },
  { legacyCode: 'CI.D005', name: 'Dian Surya Global', addr: 'Jl. Raya Pasar Kemis KM.6 No.48, Tangerang', creditLimit: 66000000, netDays: 30 },
  { legacyCode: 'CI.D007', name: 'PT Deltakita Tatajaya', addr: 'Jl. Glodok Jaya Blok A-14 Lt.II, Jakarta Barat', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.D009', name: 'Duong Long Zipper Manufacturing Ltd', addr: '63/2T, Group 6, Block 1, Binh Duc, Long An, Vietnam', creditLimit: 472500000, netDays: 0 },
  { legacyCode: 'CI.D010', name: 'CV Dwimitra Karya Perkasa', addr: 'Jl. Raya Narogong KM.19, Rawa Hingking, Bekasi', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.E001', name: 'PT Elco Elektrindo', addr: 'Jl. Industri Raya III No.6, Batu Ceper, Tangerang', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.E002', name: 'PT Eltama Prima Indo', addr: 'Jl. Raya Pasar Kemis KM.3, Tangerang', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.F001', name: 'PT Federal Nittan Indonesia', addr: 'Jl. Maligi III Lot H-1, Kawasan Industri KIIC, Karawang', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.F002', name: 'PT Formosa Taffeta Indonesia', addr: 'Kawasan Industri Tangerang, Banten', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.G001', name: 'PT Garuda Mas Semesta', addr: 'Jl. Industri Raya II No.8, Tangerang', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.G002', name: 'Great Wall Metal Products Co., Ltd', addr: 'Guangdong, China', creditLimit: 810000000, netDays: 14 },
  { legacyCode: 'CI.G003', name: 'PT Global Zipper Indonesia', addr: 'Jl. Raya Serang KM.24, Balaraja, Tangerang', creditLimit: 500000000, netDays: 30 },
  { legacyCode: 'CI.H001', name: 'PT Harapan Jaya Industri', addr: 'Jl. Industri Barat 5, Kawasan Jababeka, Cikarang', creditLimit: 200000000, netDays: 30 },
  { legacyCode: 'CI.H002', name: 'Hirota Zipper Co., Ltd', addr: 'Osaka, Japan', creditLimit: 1350000000, netDays: 14 },
  { legacyCode: 'CI.I001', name: 'PT Indo Laksana Perkasa', addr: 'Jl. Raya Mauk KM.5, Tangerang', creditLimit: 100000000, netDays: 30 },
  { legacyCode: 'CI.I002', name: 'PT Integra Cipta Kreasi', addr: 'Jl. Industri Selatan IV Blok GG No.1, Jababeka', creditLimit: 250000000, netDays: 30 },
  { legacyCode: 'CI.J001', name: 'PT Jaya Konstruksi Manggala Pratama', addr: 'Jl. Raya Pasar Kemis, Tangerang', creditLimit: 500000000, netDays: 30 },
  { legacyCode: 'CI.J002', name: 'PT Jaya Pari Steel', addr: 'Jl. Kedung Baruk No.26, Surabaya', creditLimit: 1000000000, netDays: 45 },
  { legacyCode: 'CI.K001', name: 'PT Karya Mandiri Sukses', addr: 'Jl. Manis Raya No.12, Tangerang', creditLimit: 150000000, netDays: 30 },
  { legacyCode: 'CI.K002', name: 'PT Kingmaker Footwear Indonesia', addr: 'Kawasan Industri Batamindo, Batam', creditLimit: 800000000, netDays: 30 },
  { legacyCode: 'CI.L001', name: 'PT Laksana Terang Mandiri', addr: 'Jl. Raya Serang KM.12, Tangerang', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.L002', name: 'Lakshmi Machine Works Ltd', addr: 'Coimbatore, Tamil Nadu, India', creditLimit: 540000000, netDays: 14 },
  { legacyCode: 'CI.M001', name: 'PT Mandiri Pratama Steel', addr: 'Jl. Industri Raya No.18, Pulogadung, Jakarta Timur', creditLimit: 750000000, netDays: 30 },
  { legacyCode: 'CI.M002', name: 'PT Mitra Artha Raya', addr: 'Jl. Raya Bekasi KM.27, Pondok Ungu, Bekasi', creditLimit: 300000000, netDays: 30 },
  { legacyCode: 'CI.M003', name: 'PT Murni Cahaya Pratama', addr: 'Kawasan Industri MM2100, Bekasi', creditLimit: 200000000, netDays: 14 },
  { legacyCode: 'CI.N001', name: 'PT Naga Bhumi Energi', addr: 'Jl. Raya Serpong KM.7, BSD City, Tangerang Selatan', creditLimit: 400000000, netDays: 30 },
  { legacyCode: 'CI.N002', name: 'PT Nusa Indah Makmur', addr: 'Jl. Raya Narogong KM.8, Bantar Gebang, Bekasi', creditLimit: 100000000, netDays: 30 },
  { legacyCode: 'CI.O001', name: 'PT Olympic Furniture', addr: 'Jl. Raya Serang KM.18, Cikande, Serang', creditLimit: 600000000, netDays: 30 },
  { legacyCode: 'CI.P001', name: 'PT Panca Budi Pratama', addr: 'Jl. Brigjend Katamso No.6, Medan', creditLimit: 500000000, netDays: 30 },
  { legacyCode: 'CI.P002', name: 'PT Perintis Triniti Properti', addr: 'Jl. TB Simatupang No.19, Pasar Rebo, Jakarta Timur', creditLimit: 0, netDays: 0 },
  { legacyCode: 'CI.P003', name: 'PT Prima Makmur Sejahtera', addr: 'Jl. Raya Bekasi Timur KM.18, Bekasi', creditLimit: 250000000, netDays: 45 },
  { legacyCode: 'CI.P004', name: 'PT Pertiwi Agung', addr: 'Jl. Industri Raya IV No.5, Kawasan KIIC, Karawang', creditLimit: 350000000, netDays: 30 },
  { legacyCode: 'CI.R001', name: 'PT Rajawali Inti Nusantara', addr: 'Jl. Yos Sudarso No.32, Palembang, Sumatera Selatan', creditLimit: 800000000, netDays: 30 },
  { legacyCode: 'CI.R002', name: 'PT Reksa Indah Laksana', addr: 'Jl. Daan Mogot KM.21, Batu Ceper, Tangerang', creditLimit: 150000000, netDays: 14 },
  { legacyCode: 'CI.S001', name: 'PT Sari Husada', addr: 'Jl. Kusumanegara No.173, Yogyakarta', creditLimit: 1000000000, netDays: 30 },
  { legacyCode: 'CI.S002', name: 'PT Sinar Agung Selalu Sukses', addr: 'Jl. Gatot Subroto Km.7, Batu Ceper, Tangerang', creditLimit: 300000000, netDays: 30 },
  { legacyCode: 'CI.S003', name: 'PT Sumber Makmur Abadi', addr: 'Jl. Raya Semarang-Demak KM.9, Semarang', creditLimit: 400000000, netDays: 30 },
  { legacyCode: 'CI.S004', name: 'PT Surya Toto Indonesia Tbk', addr: 'Jl. Raya Serpong KM.7, Pagedangan, Tangerang', creditLimit: 2000000000, netDays: 45 },
  { legacyCode: 'CI.S005', name: 'PT Sentra Usahatama Jaya', addr: 'Kawasan Industri Kota Bukit Indah, Purwakarta', creditLimit: 500000000, netDays: 30 },
  { legacyCode: 'CI.T001', name: 'PT Timah Industri', addr: 'Jl. Siliwangi No.3, Pangkalpinang, Bangka Belitung', creditLimit: 1500000000, netDays: 30 },
  { legacyCode: 'CI.T002', name: 'PT Tri Usaha Bhakti', addr: 'Jl. Aditiawarman No.12, Medan, Sumatera Utara', creditLimit: 200000000, netDays: 14 },
  { legacyCode: 'CI.U001', name: 'PT Unilever Indonesia Tbk', addr: 'Jl. Jend. Gatot Subroto Kav.15, Jakarta Selatan', creditLimit: 5000000000, netDays: 60 },
  { legacyCode: 'CI.U002', name: 'PT United Can Company', addr: 'Jl. Yos Sudarso Kav.88, Jakarta Utara', creditLimit: 1200000000, netDays: 45 },
  { legacyCode: 'CI.V001', name: 'PT Victory Chingluh Indonesia', addr: 'Kawasan Industri Millennium, Balaraja, Tangerang', creditLimit: 800000000, netDays: 30 },
  { legacyCode: 'CI.W001', name: 'PT Wahana Interfood Nusantara Tbk', addr: 'Jl. Raya Dayeuhkolot No.wt/63, Bandung', creditLimit: 600000000, netDays: 30 },
  { legacyCode: 'CI.W002', name: 'PT Wijaya Karya (Persero) Tbk', addr: 'Jl. DI Panjaitan Kav.9, Jakarta Timur', creditLimit: 3000000000, netDays: 45 },
  { legacyCode: 'CI.W003', name: 'PT Wiring Device Beka', addr: 'Jl. Raya Bekasi KM.22, Cakung, Jakarta Timur', creditLimit: 250000000, netDays: 30 },
  { legacyCode: 'CI.Y001', name: 'YKK AP Vietnam Co., Ltd', addr: 'VSIP II Industrial Park, Binh Duong, Vietnam', creditLimit: 2160000000, netDays: 14 },
  { legacyCode: 'CI.Y002', name: 'YKK (Thailand) Co., Ltd', addr: 'Samrong, Samut Prakan, Thailand', creditLimit: 2700000000, netDays: 14 },
  { legacyCode: 'CI.Y003', name: 'PT Yasunli Abadi Utama Plastik', addr: 'Jl. Industri Raya Barat Blok D No.22, Tangerang', creditLimit: 100000000, netDays: 30 },
  { legacyCode: 'CI.Z001', name: 'PT Ziegler Indonesia', addr: 'Jl. Raya Serpong KM.8, Pagedangan, Tangerang', creditLimit: 200000000, netDays: 30 },
  { legacyCode: 'CI.X001', name: 'PT Xingtai Hardware Indonesia', addr: 'Kawasan Industri Pulogadung Blok X1, Jakarta Timur', creditLimit: 300000000, netDays: 30 },
  { legacyCode: 'CI.Q001', name: 'PT Qualitex Mitra Abadi', addr: 'Jl. Raya Bekasi KM.25, Cakung, Jakarta Timur', creditLimit: 150000000, netDays: 14 },
  { legacyCode: 'CI.A026', name: 'PT Achmad Suyanto Industri', addr: 'Kp. Kalimati RT.007/003, Kedaung, Ciputat', creditLimit: 90000000, netDays: 30 },
];

// ---------------------------------------------------------------------------
// Net-days → payment term code (matches real seed-md-payment-terms-indonesia)
// ---------------------------------------------------------------------------
const NET_DAYS_TO_TERM_CODE: Record<number, string> = {
  0: 'COD',
  7: 'NET-7',
  10: 'NET-10',
  14: 'NET-14',
  30: 'NET-30',
  45: 'NET-45',
  60: 'NET-60',
  90: 'NET-90',
};

// Generate a realistic NPWP (non-official, just realistic format XX.XXX.XXX.X-XXX.XXX)
function generateNpwp(idx: number): string {
  const base = String(idx + 10).padStart(2, '0');
  const mid = String(((idx * 137 + 31) % 900) + 100);
  const mid2 = String(((idx * 71 + 17) % 900) + 100);
  const suffix1 = String((idx % 9) + 1);
  const suffix2 = String(((idx * 53) % 900) + 100);
  const suffix3 = String(((idx * 23 + 7) % 900) + 100);
  return `${base}.${mid}.${mid2}.${suffix1}-${suffix2}.${suffix3}`;
}

// Determine if company is taxable (PT, CV with credit limit, Ltd, Tbk)
function isTaxable(name: string, creditLimit: number): boolean {
  const n = name.toUpperCase();
  return (n.startsWith('PT ') || n.includes(' TBK') || n.includes('LTD') || n.includes('CO.,') || creditLimit > 100000000);
}

// Determine if also a supplier (large credit + longer terms = buying relationship)
function isSupplier(name: string, netDays: number, creditLimit: number): boolean {
  return netDays >= 14 && creditLimit > 100000000;
}

const pad4 = (n: number) => String(n).padStart(4, '0');

async function main(): Promise<void> {
  // ---- 1. Resolve FK IDs at runtime
  const [idrCurrency, custCats, termRows, customerType, supplierType] = await Promise.all([
    prisma.erpCurrency.findFirstOrThrow({ where: { code: 'IDR', deletedAt: null } }),
    prisma.erpPartnerCategory.findMany({ where: { kind: 'CUSTOMER', deletedAt: null }, orderBy: { id: 'asc' } }),
    prisma.erpPaymentTerm.findMany({ where: { deletedAt: null }, select: { id: true, code: true } }),
    prisma.erpPartnerType.findUnique({ where: { code: 'CUST' } }),
    prisma.erpPartnerType.findUnique({ where: { code: 'SUP' } }),
  ]);

  const termMap = new Map(termRows.map(t => [t.code, t.id]));

  if (custCats.length === 0) throw new Error('No CUSTOMER partner categories found — run category seed first');
  if (!customerType || !supplierType) throw new Error('Missing md_partner_types CUST/SUP seed');

  // ---- 2. Delete dummy partners
  const deleted = await prisma.erpPartner.deleteMany({
    where: { code: { startsWith: 'DUMMY-PRT-' } },
  });
  console.log(`Deleted ${deleted.count} dummy partners (DUMMY-PRT-*)`);

  // ---- 3. Insert real customers
  const rows = LEGACY_CUSTOMERS.map((c, i) => {
    const termCode = NET_DAYS_TO_TERM_CODE[c.netDays] ?? 'COD';
    const termId = termMap.get(termCode) ?? termMap.get('COD')!;
    const catId = custCats[i % custCats.length].id;
    const taxable = isTaxable(c.name, c.creditLimit);
    const supplier = isSupplier(c.name, c.netDays, c.creditLimit);

    return {
      code: `CUST-${pad4(i + 1)}`,
      name: c.name,
      partnerTypeId: supplier ? supplierType.id : customerType.id,
      categoryId: catId,
      currencyId: idrCurrency.id,
      taxNumber: taxable ? generateNpwp(i) : null,
      isTaxable: taxable,
      arCreditLimit: c.creditLimit > 0 ? c.creditLimit : null,
      saleTermId: termId,
      isActive: true,
      legacyCode: c.legacyCode,
    };
  });

  const result = await prisma.erpPartner.createMany({ data: rows, skipDuplicates: true });
  console.log(`Inserted ${result.count} real customer partners (CUST-0001…CUST-${pad4(rows.length)})`);

  // ---- Summary
  const total = await prisma.erpPartner.count({ where: { deletedAt: null } });
  console.log(`Total active partners now: ${total}`);
}

main()
  .catch(e => { console.error(e); process.exit(1); })
  .finally(() => prisma.$disconnect());
