/**
 * One-off: seed 100 dummy rows into empty md_* master data tables.
 * Run: npx ts-node prisma/seed-erp-md-dummy.ts
 *
 * Idempotent: skips entire table if it already has >=1 row.
 * Skipped because already populated:
 *   md_branches, md_partner_categories, md_currencies, md_accounts (snapshot 2026-05-20).
 *   md_projects — seeded via seed-erp-md-projects.ts with 104 real manufacturing projects.
 *
 * Seeded (target 100 rows each):
 *   md_locations, md_warehouses, md_units, md_item_categories, md_items,
 *   md_partners, md_taxes, md_payment_terms, md_divisions, md_subdivisions.
 *
 * Skipped (already populated with real data):
 *   md_cost_centers — seeded via seed-erp-md-legacy.ts with 43 real manufacturing cost centers.
 */
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();
const COUNT = 100;
const PREFIX = 'DUMMY';

const CITIES = [
  'Jakarta', 'Bandung', 'Surabaya', 'Medan', 'Semarang', 'Makassar', 'Palembang',
  'Tangerang', 'Depok', 'Bekasi', 'Bogor', 'Batam', 'Pekanbaru', 'Denpasar',
  'Yogyakarta', 'Malang', 'Solo', 'Padang', 'Manado', 'Pontianak',
];
const STREETS = [
  'Jl. Merdeka', 'Jl. Sudirman', 'Jl. Thamrin', 'Jl. Gatot Subroto',
  'Jl. Diponegoro', 'Jl. Ahmad Yani', 'Jl. Pahlawan', 'Jl. Veteran',
];
const ITEM_TYPES = ['INVENTORY', 'SERVICE', 'CONSUMABLE', 'ASSET', 'NON_INVENTORY'] as const;
const UNIT_NAMES = [
  ['PCS', 'Piece'], ['BOX', 'Box'], ['KG', 'Kilogram'], ['G', 'Gram'],
  ['L', 'Liter'], ['ML', 'Milliliter'], ['M', 'Meter'], ['CM', 'Centimeter'],
  ['PACK', 'Pack'], ['SET', 'Set'], ['ROLL', 'Roll'], ['DZ', 'Dozen'],
  ['UNIT', 'Unit'], ['BTL', 'Botol'], ['SAK', 'Sak'], ['DRUM', 'Drum'],
];
const CATEGORY_NAMES = [
  'Bahan Baku', 'Barang Jadi', 'Sparepart', 'Konsumsi', 'Elektronik',
  'Kimia', 'Kemasan', 'Alat Tulis', 'Perkakas', 'Komputer', 'Furnitur',
  'Tekstil', 'Makanan', 'Minuman', 'Logam',
];
const PARTNER_NAMES = [
  'Sentosa', 'Jaya Abadi', 'Maju Mundur', 'Cipta Karya', 'Sukses', 'Mandiri',
  'Berkah', 'Sinar', 'Bintang', 'Citra', 'Pratama', 'Sejahtera', 'Mulia',
  'Karya', 'Anugrah', 'Harapan', 'Lestari', 'Subur', 'Andalan', 'Tunggal',
];
const SUFFIXES = ['CV', 'PT', 'UD', 'Toko', 'Group'];

const rp = <T>(a: readonly T[]): T => a[Math.floor(Math.random() * a.length)];
const ri = (min: number, max: number) => min + Math.floor(Math.random() * (max - min + 1));
const pad = (n: number, w = 4) => String(n).padStart(w, '0');

async function seedIfEmpty(
  label: string,
  count: () => Promise<number>,
  insert: () => Promise<number>,
): Promise<void> {
  const n = await count();
  if (n > 0) {
    console.log(`- ${label}: already has ${n} rows, skip.`);
    return;
  }
  const created = await insert();
  console.log(`+ ${label}: inserted ${created} rows.`);
}

async function main(): Promise<void> {
  // ---- md_units (no FK)
  await seedIfEmpty('md_units', () => prisma.erpUnit.count(), async () => {
    const rows = Array.from({ length: COUNT }, (_, i) => {
      const [base, baseName] = UNIT_NAMES[i % UNIT_NAMES.length];
      return {
        code: `${PREFIX}-UNIT-${pad(i + 1)}`,
        name: `${baseName} ${pad(i + 1)}`,
        conversionFactor: 1,
        notes: `Dummy unit ${base}`,
        isActive: Math.random() > 0.05,
      };
    });
    const r = await prisma.erpUnit.createMany({ data: rows, skipDuplicates: true });
    return r.count;
  });

  // ---- md_taxes (no required FK)
  await seedIfEmpty('md_taxes', () => prisma.erpTax.count(), async () => {
    const rows = Array.from({ length: COUNT }, (_, i) => ({
      code: `${PREFIX}-TAX-${pad(i + 1)}`,
      name: `Pajak Dummy ${pad(i + 1)}`,
      rate: [0, 1, 2, 5, 10, 11][i % 6],
      isActive: Math.random() > 0.05,
    }));
    const r = await prisma.erpTax.createMany({ data: rows, skipDuplicates: true });
    return r.count;
  });

  // ---- md_payment_terms (no FK)
  await seedIfEmpty('md_payment_terms', () => prisma.erpPaymentTerm.count(), async () => {
    const rows = Array.from({ length: COUNT }, (_, i) => {
      const net = [7, 14, 30, 45, 60, 90][i % 6];
      return {
        code: `${PREFIX}-TOP-${pad(i + 1)}`,
        name: `Net ${net} Dummy ${pad(i + 1)}`,
        netDays: net,
        isActive: Math.random() > 0.05,
      };
    });
    const r = await prisma.erpPaymentTerm.createMany({ data: rows, skipDuplicates: true });
    return r.count;
  });

  // ---- md_item_categories (self-FK optional)
  await seedIfEmpty('md_item_categories', () => prisma.erpItemCategory.count(), async () => {
    const rows = Array.from({ length: COUNT }, (_, i) => ({
      code: `${PREFIX}-CAT-${pad(i + 1)}`,
      name: `${rp(CATEGORY_NAMES)} ${pad(i + 1)}`,
      isActive: Math.random() > 0.05,
    }));
    const r = await prisma.erpItemCategory.createMany({ data: rows, skipDuplicates: true });
    return r.count;
  });

  // md_cost_centers — skipped, real manufacturing data seeded via seed-erp-md-legacy.ts

  // ---- md_divisions
  await seedIfEmpty('md_divisions', () => prisma.erpDivision.count(), async () => {
    const rows = Array.from({ length: COUNT }, (_, i) => ({
      code: `${PREFIX}-DIV-${pad(i + 1)}`,
      name: `Divisi Dummy ${pad(i + 1)}`,
      isActive: Math.random() > 0.05,
    }));
    const r = await prisma.erpDivision.createMany({ data: rows, skipDuplicates: true });
    return r.count;
  });

  // ---- md_subdivisions (needs divisionId)
  await seedIfEmpty('md_subdivisions', () => prisma.erpSubdivision.count(), async () => {
    const divs = await prisma.erpDivision.findMany({ select: { id: true }, take: 200 });
    if (divs.length === 0) throw new Error('Need md_divisions to seed subdivisions');
    const rows = Array.from({ length: COUNT }, (_, i) => ({
      code: `${PREFIX}-SUB-${pad(i + 1)}`,
      name: `Subdivisi Dummy ${pad(i + 1)}`,
      divisionId: divs[i % divs.length].id,
      isActive: Math.random() > 0.05,
    }));
    const r = await prisma.erpSubdivision.createMany({ data: rows, skipDuplicates: true });
    return r.count;
  });

  // ---- md_locations (needs branchId)
  await seedIfEmpty('md_locations', () => prisma.erpLocation.count(), async () => {
    const branches = await prisma.erpBranch.findMany({ select: { id: true }, take: 500 });
    if (branches.length === 0) throw new Error('Need md_branches to seed locations');
    const rows = Array.from({ length: COUNT }, (_, i) => {
      const city = rp(CITIES);
      return {
        code: `${PREFIX}-LOC-${pad(i + 1)}`,
        name: `Lokasi ${city} ${pad(i + 1)}`,
        branchId: branches[i % branches.length].id,
        addressLine1: `${rp(STREETS)} No. ${ri(1, 999)}`,
        city,
        postalCode: String(ri(10000, 99999)),
        phone: `021-${ri(1000000, 9999999)}`,
        isActive: Math.random() > 0.05,
      };
    });
    const r = await prisma.erpLocation.createMany({ data: rows, skipDuplicates: true });
    return r.count;
  });

  // ---- md_warehouses (needs locationId)
  await seedIfEmpty('md_warehouses', () => prisma.erpWarehouse.count(), async () => {
    const locs = await prisma.erpLocation.findMany({ select: { id: true }, take: 500 });
    if (locs.length === 0) throw new Error('Need md_locations to seed warehouses');
    const rows = Array.from({ length: COUNT }, (_, i) => ({
      code: `${PREFIX}-WH-${pad(i + 1)}`,
      name: `Gudang Dummy ${pad(i + 1)}`,
      locationId: locs[i % locs.length].id,
      allowNegativeStock: Math.random() < 0.1,
      notes: 'Dummy warehouse',
      isActive: Math.random() > 0.05,
    }));
    const r = await prisma.erpWarehouse.createMany({ data: rows, skipDuplicates: true });
    return r.count;
  });

  // ---- md_items (needs categoryId + baseUnitId + type)
  await seedIfEmpty('md_items', () => prisma.erpItem.count(), async () => {
    const cats = await prisma.erpItemCategory.findMany({ select: { id: true }, take: 500 });
    const units = await prisma.erpUnit.findMany({ select: { id: true }, take: 500 });
    if (cats.length === 0 || units.length === 0) {
      throw new Error('Need md_item_categories and md_units to seed items');
    }
    const rows = Array.from({ length: COUNT }, (_, i) => {
      const cost = ri(1000, 1000000);
      return {
        code: `${PREFIX}-ITM-${pad(i + 1)}`,
        name: `Barang Dummy ${pad(i + 1)}`,
        barcode: String(ri(1000000000000, 9999999999999)),
        type: ITEM_TYPES[i % ITEM_TYPES.length],
        categoryId: cats[i % cats.length].id,
        baseUnitId: units[i % units.length].id,
        standardCost: cost,
        averageCost: cost,
        purchasePrice: cost,
        salePrice: Math.round(cost * 1.3),
        minStock: ri(0, 50),
        maxStock: ri(100, 1000),
        reorderQty: ri(10, 100),
        isActive: Math.random() > 0.05,
      };
    });
    const r = await prisma.erpItem.createMany({ data: rows, skipDuplicates: true });
    return r.count;
  });

  // ---- md_partners (FKs all optional)
  await seedIfEmpty('md_partners', () => prisma.erpPartner.count(), async () => {
    const cats = await prisma.erpPartnerCategory.findMany({ select: { id: true }, take: 200 });
    const currencies = await prisma.erpCurrency.findMany({ select: { id: true }, take: 50 });
    const [customerType, supplierType] = await Promise.all([
      prisma.erpPartnerType.findUnique({ where: { code: 'CUST' } }),
      prisma.erpPartnerType.findUnique({ where: { code: 'SUP' } }),
    ]);
    if (!customerType || !supplierType) throw new Error('Missing md_partner_types CUST/SUP seed');
    const rows = Array.from({ length: COUNT }, (_, i) => {
      const isCust = Math.random() < 0.6;
      const isSup = Math.random() < 0.5;
      return {
        code: `${PREFIX}-PRT-${pad(i + 1)}`,
        name: `${rp(SUFFIXES)} ${rp(PARTNER_NAMES)} ${rp(PARTNER_NAMES)} ${pad(i + 1)}`,
        partnerTypeId: isSup && !isCust ? supplierType.id : customerType.id,
        categoryId: cats.length ? cats[i % cats.length].id : null,
        currencyId: currencies.length ? currencies[i % currencies.length].id : null,
        taxNumber: `${ri(10, 99)}.${ri(100, 999)}.${ri(100, 999)}.${ri(1, 9)}-${ri(100, 999)}.${ri(100, 999)}`,
        isTaxable: Math.random() < 0.7,
        isActive: Math.random() > 0.05,
      };
    });
    const r = await prisma.erpPartner.createMany({ data: rows, skipDuplicates: true });
    return r.count;
  });

  // md_projects — skipped, real data seeded via seed-erp-md-projects.ts
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(async () => {
    await prisma.$disconnect();
  });
