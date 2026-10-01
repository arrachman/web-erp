/**
 * Seed data REALISTIS untuk attribute/classification item pabrik garment/tekstil Indonesia.
 * Run: npx ts-node --project tsconfig.json prisma/seed-erp-item-attrs.ts
 *
 * Idempotent: skip seluruh tabel jika sudah ada >= 1 row.
 *
 * Tabel yang di-seed:
 *   md_brands           (20 rows)
 *   md_materials        (25 rows)
 *   md_colors           (20 rows)
 *   md_sizes            (20 rows)
 *   md_item_models      (15 rows)
 *   md_sections         (15 rows)
 *   md_product_classes  (10 rows)
 *   md_price_categories  (8 rows)
 *   md_price_indices     (8 rows)
 *   md_commissions       (8 rows)
 *   md_classes          (10 rows)
 *   md_item_locations   (10 rows)
 */
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

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

// ── 1. md_brands ─────────────────────────────────────────────────────────────

async function seedBrands(): Promise<void> {
  await seedIfEmpty(
    'md_brands',
    () => prisma.erpBrand.count(),
    async () => {
      const rows = [
        // Brand lokal Indonesia
        { code: 'BRD-001', name: 'Cotton Ink',               isActive: true, legacyCode: null },
        { code: 'BRD-002', name: 'Erigo',                    isActive: true, legacyCode: null },
        { code: 'BRD-003', name: 'Damn! I Love Indonesia',   isActive: true, legacyCode: null },
        { code: 'BRD-004', name: 'Sage+Folk',                isActive: true, legacyCode: null },
        { code: 'BRD-005', name: 'Elhaus',                   isActive: true, legacyCode: null },
        { code: 'BRD-006', name: 'Buttonscarves',            isActive: true, legacyCode: null },
        { code: 'BRD-007', name: 'Cardinal',                 isActive: true, legacyCode: null },
        { code: 'BRD-008', name: 'Polo Ralph Lauren (OEM)',  isActive: true, legacyCode: null },
        { code: 'BRD-009', name: 'Hammer',                   isActive: true, legacyCode: null },
        { code: 'BRD-010', name: 'Nevada',                   isActive: true, legacyCode: null },
        // Brand internasional — OEM/CMT di Indonesia
        { code: 'BRD-011', name: 'H&M',                      isActive: true, legacyCode: null },
        { code: 'BRD-012', name: 'Zara',                     isActive: true, legacyCode: null },
        { code: 'BRD-013', name: 'Uniqlo',                   isActive: true, legacyCode: null },
        { code: 'BRD-014', name: 'GAP',                      isActive: true, legacyCode: null },
        { code: 'BRD-015', name: 'Nike',                     isActive: true, legacyCode: null },
        { code: 'BRD-016', name: 'Adidas',                   isActive: true, legacyCode: null },
        { code: 'BRD-017', name: 'Puma',                     isActive: true, legacyCode: null },
        { code: 'BRD-018', name: 'Decathlon',                isActive: true, legacyCode: null },
        { code: 'BRD-019', name: 'Next',                     isActive: true, legacyCode: null },
        { code: 'BRD-020', name: 'Primark',                  isActive: true, legacyCode: null },
      ];
      const r = await prisma.erpBrand.createMany({ data: rows, skipDuplicates: true });
      return r.count;
    },
  );
}

// ── 2. md_materials ───────────────────────────────────────────────────────────

async function seedMaterials(): Promise<void> {
  await seedIfEmpty(
    'md_materials',
    () => prisma.erpMaterial.count(),
    async () => {
      const rows = [
        // Kain utama
        { code: 'MAT-001', name: 'Cotton 100%',              isActive: true, legacyCode: null },
        { code: 'MAT-002', name: 'Polyester 100%',           isActive: true, legacyCode: null },
        { code: 'MAT-003', name: 'CVC 60/40 (Cotton-Poly)',  isActive: true, legacyCode: null },
        { code: 'MAT-004', name: 'TC 65/35 (Poly-Cotton)',   isActive: true, legacyCode: null },
        { code: 'MAT-005', name: 'Rayon/Viscose',            isActive: true, legacyCode: null },
        { code: 'MAT-006', name: 'Linen',                    isActive: true, legacyCode: null },
        { code: 'MAT-007', name: 'Tencel (Lyocell)',         isActive: true, legacyCode: null },
        { code: 'MAT-008', name: 'Modal',                    isActive: true, legacyCode: null },
        { code: 'MAT-009', name: 'Spandex/Lycra',            isActive: true, legacyCode: null },
        { code: 'MAT-010', name: 'Jersey Knit',              isActive: true, legacyCode: null },
        { code: 'MAT-011', name: 'Fleece',                   isActive: true, legacyCode: null },
        { code: 'MAT-012', name: 'Denim',                    isActive: true, legacyCode: null },
        { code: 'MAT-013', name: 'Twill',                    isActive: true, legacyCode: null },
        { code: 'MAT-014', name: 'Canvas',                   isActive: true, legacyCode: null },
        { code: 'MAT-015', name: 'Satin',                    isActive: true, legacyCode: null },
        { code: 'MAT-016', name: 'Chiffon',                  isActive: true, legacyCode: null },
        { code: 'MAT-017', name: 'Interlock',                isActive: true, legacyCode: null },
        { code: 'MAT-018', name: 'Rib 1x1',                  isActive: true, legacyCode: null },
        { code: 'MAT-019', name: 'Waffle',                   isActive: true, legacyCode: null },
        { code: 'MAT-020', name: 'Dobby',                    isActive: true, legacyCode: null },
        // Aksesori / bahan pendukung
        { code: 'MAT-021', name: 'Benang Polyester',         isActive: true, legacyCode: null },
        { code: 'MAT-022', name: 'Benang Cotton',            isActive: true, legacyCode: null },
        { code: 'MAT-023', name: 'Karet Elastis',            isActive: true, legacyCode: null },
        { code: 'MAT-024', name: 'Interlining',              isActive: true, legacyCode: null },
        { code: 'MAT-025', name: 'Wadding/Padding',          isActive: true, legacyCode: null },
      ];
      const r = await prisma.erpMaterial.createMany({ data: rows, skipDuplicates: true });
      return r.count;
    },
  );
}

// ── 3. md_colors ──────────────────────────────────────────────────────────────

async function seedColors(): Promise<void> {
  await seedIfEmpty(
    'md_colors',
    () => prisma.erpColor.count(),
    async () => {
      const rows = [
        { code: 'CLR-001', name: 'Putih',       hexCode: '#FFFFFF', isActive: true, legacyCode: null },
        { code: 'CLR-002', name: 'Hitam',       hexCode: '#000000', isActive: true, legacyCode: null },
        { code: 'CLR-003', name: 'Abu-abu',     hexCode: '#808080', isActive: true, legacyCode: null },
        { code: 'CLR-004', name: 'Charcoal',    hexCode: '#36454F', isActive: true, legacyCode: null },
        { code: 'CLR-005', name: 'Navy',        hexCode: '#001F5B', isActive: true, legacyCode: null },
        { code: 'CLR-006', name: 'Biru',        hexCode: '#0066CC', isActive: true, legacyCode: null },
        { code: 'CLR-007', name: 'Biru Muda',   hexCode: '#87CEEB', isActive: true, legacyCode: null },
        { code: 'CLR-008', name: 'Tosca',       hexCode: '#008080', isActive: true, legacyCode: null },
        { code: 'CLR-009', name: 'Hijau',       hexCode: '#006400', isActive: true, legacyCode: null },
        { code: 'CLR-010', name: 'Hijau Muda',  hexCode: '#90EE90', isActive: true, legacyCode: null },
        { code: 'CLR-011', name: 'Merah',       hexCode: '#CC0000', isActive: true, legacyCode: null },
        { code: 'CLR-012', name: 'Maroon',      hexCode: '#800000', isActive: true, legacyCode: null },
        { code: 'CLR-013', name: 'Pink',        hexCode: '#FF69B4', isActive: true, legacyCode: null },
        { code: 'CLR-014', name: 'Salmon',      hexCode: '#FA8072', isActive: true, legacyCode: null },
        { code: 'CLR-015', name: 'Oranye',      hexCode: '#FF6600', isActive: true, legacyCode: null },
        { code: 'CLR-016', name: 'Kuning',      hexCode: '#FFD700', isActive: true, legacyCode: null },
        { code: 'CLR-017', name: 'Ungu',        hexCode: '#800080', isActive: true, legacyCode: null },
        { code: 'CLR-018', name: 'Cokelat',     hexCode: '#8B4513', isActive: true, legacyCode: null },
        { code: 'CLR-019', name: 'Khaki',       hexCode: '#C3B091', isActive: true, legacyCode: null },
        { code: 'CLR-020', name: 'Cream',       hexCode: '#FFFDD0', isActive: true, legacyCode: null },
      ];
      const r = await prisma.erpColor.createMany({ data: rows, skipDuplicates: true });
      return r.count;
    },
  );
}

// ── 4. md_sizes ───────────────────────────────────────────────────────────────

async function seedSizes(): Promise<void> {
  await seedIfEmpty(
    'md_sizes',
    () => prisma.erpSize.count(),
    async () => {
      const rows = [
        // Standar Asia
        { code: 'SZ-XS',   name: 'XS',   isActive: true, legacyCode: null },
        { code: 'SZ-S',    name: 'S',    isActive: true, legacyCode: null },
        { code: 'SZ-M',    name: 'M',    isActive: true, legacyCode: null },
        { code: 'SZ-L',    name: 'L',    isActive: true, legacyCode: null },
        { code: 'SZ-XL',   name: 'XL',   isActive: true, legacyCode: null },
        { code: 'SZ-XXL',  name: 'XXL',  isActive: true, legacyCode: null },
        { code: 'SZ-XXXL', name: 'XXXL', isActive: true, legacyCode: null },
        { code: 'SZ-4XL',  name: '4XL',  isActive: true, legacyCode: null },
        // Standar EU
        { code: 'SZ-EU36', name: 'EU 36', isActive: true, legacyCode: null },
        { code: 'SZ-EU38', name: 'EU 38', isActive: true, legacyCode: null },
        { code: 'SZ-EU40', name: 'EU 40', isActive: true, legacyCode: null },
        { code: 'SZ-EU42', name: 'EU 42', isActive: true, legacyCode: null },
        { code: 'SZ-EU44', name: 'EU 44', isActive: true, legacyCode: null },
        { code: 'SZ-EU46', name: 'EU 46', isActive: true, legacyCode: null },
        // Standar US
        { code: 'SZ-US4',  name: 'US 4',  isActive: true, legacyCode: null },
        { code: 'SZ-US6',  name: 'US 6',  isActive: true, legacyCode: null },
        { code: 'SZ-US8',  name: 'US 8',  isActive: true, legacyCode: null },
        // Anak
        { code: 'SZ-2Y',   name: '2Y',   isActive: true, legacyCode: null },
        { code: 'SZ-4Y',   name: '4Y',   isActive: true, legacyCode: null },
        { code: 'SZ-6Y',   name: '6Y',   isActive: true, legacyCode: null },
      ];
      const r = await prisma.erpSize.createMany({ data: rows, skipDuplicates: true });
      return r.count;
    },
  );
}

// ── 5. md_item_models ─────────────────────────────────────────────────────────

async function seedItemModels(): Promise<void> {
  await seedIfEmpty(
    'md_item_models',
    () => prisma.erpItemModel.count(),
    async () => {
      const rows = [
        // Kemeja
        { code: 'MDL-001', name: 'Slim Fit',     isActive: true, legacyCode: null },
        { code: 'MDL-002', name: 'Regular Fit',  isActive: true, legacyCode: null },
        { code: 'MDL-003', name: 'Oversized',    isActive: true, legacyCode: null },
        { code: 'MDL-004', name: 'Henley',       isActive: true, legacyCode: null },
        { code: 'MDL-005', name: 'Oxford',       isActive: true, legacyCode: null },
        // Celana
        { code: 'MDL-006', name: 'Chino',        isActive: true, legacyCode: null },
        { code: 'MDL-007', name: 'Jogger',       isActive: true, legacyCode: null },
        { code: 'MDL-008', name: 'Cargo',        isActive: true, legacyCode: null },
        { code: 'MDL-009', name: 'Slim Straight',isActive: true, legacyCode: null },
        { code: 'MDL-010', name: 'Wide Leg',     isActive: true, legacyCode: null },
        // Dress
        { code: 'MDL-011', name: 'A-Line',       isActive: true, legacyCode: null },
        { code: 'MDL-012', name: 'Wrap',         isActive: true, legacyCode: null },
        // Kaos
        { code: 'MDL-013', name: 'V-Neck',       isActive: true, legacyCode: null },
        { code: 'MDL-014', name: 'O-Neck',       isActive: true, legacyCode: null },
        { code: 'MDL-015', name: 'Raglan',       isActive: true, legacyCode: null },
      ];
      const r = await prisma.erpItemModel.createMany({ data: rows, skipDuplicates: true });
      return r.count;
    },
  );
}

// ── 6. md_sections ────────────────────────────────────────────────────────────

async function seedSections(): Promise<void> {
  await seedIfEmpty(
    'md_sections',
    () => prisma.erpSection.count(),
    async () => {
      const rows = [
        // Area produksi
        { code: 'SEC-001', name: 'Cutting',        isActive: true, legacyCode: null },
        { code: 'SEC-002', name: 'Sewing Line A',  isActive: true, legacyCode: null },
        { code: 'SEC-003', name: 'Sewing Line B',  isActive: true, legacyCode: null },
        { code: 'SEC-004', name: 'Sewing Line C',  isActive: true, legacyCode: null },
        { code: 'SEC-005', name: 'Embroidery',     isActive: true, legacyCode: null },
        { code: 'SEC-006', name: 'Printing',       isActive: true, legacyCode: null },
        { code: 'SEC-007', name: 'Washing',        isActive: true, legacyCode: null },
        { code: 'SEC-008', name: 'Finishing',      isActive: true, legacyCode: null },
        { code: 'SEC-009', name: 'QC/Inspection',  isActive: true, legacyCode: null },
        // Gudang
        { code: 'SEC-010', name: 'Gudang Bahan Baku',   isActive: true, legacyCode: null },
        { code: 'SEC-011', name: 'Gudang Barang Jadi',  isActive: true, legacyCode: null },
        { code: 'SEC-012', name: 'Gudang Aksesori',     isActive: true, legacyCode: null },
        { code: 'SEC-013', name: 'In-Process Store',    isActive: true, legacyCode: null },
        // Lain
        { code: 'SEC-014', name: 'Packaging',           isActive: true, legacyCode: null },
        { code: 'SEC-015', name: 'Shipping/Ekspedisi',  isActive: true, legacyCode: null },
      ];
      const r = await prisma.erpSection.createMany({ data: rows, skipDuplicates: true });
      return r.count;
    },
  );
}

// ── 7. md_product_classes ─────────────────────────────────────────────────────

async function seedProductClasses(): Promise<void> {
  await seedIfEmpty(
    'md_product_classes',
    () => prisma.erpProductClass.count(),
    async () => {
      const rows = [
        { code: 'PC-001', name: 'Tops',                    isActive: true, legacyCode: null },
        { code: 'PC-002', name: 'Bottoms',                 isActive: true, legacyCode: null },
        { code: 'PC-003', name: 'Outerwear',               isActive: true, legacyCode: null },
        { code: 'PC-004', name: 'Dresses & Skirts',        isActive: true, legacyCode: null },
        { code: 'PC-005', name: 'Sportswear',              isActive: true, legacyCode: null },
        { code: 'PC-006', name: 'Kids',                    isActive: true, legacyCode: null },
        { code: 'PC-007', name: 'Underwear & Innerwear',   isActive: true, legacyCode: null },
        { code: 'PC-008', name: 'Accessories',             isActive: true, legacyCode: null },
        { code: 'PC-009', name: 'Uniforms',                isActive: true, legacyCode: null },
        { code: 'PC-010', name: 'Swimwear',                isActive: true, legacyCode: null },
      ];
      const r = await prisma.erpProductClass.createMany({ data: rows, skipDuplicates: true });
      return r.count;
    },
  );
}

// ── 8. md_price_categories ────────────────────────────────────────────────────

async function seedPriceCategories(): Promise<void> {
  await seedIfEmpty(
    'md_price_categories',
    () => prisma.erpPriceCategory.count(),
    async () => {
      const rows = [
        { code: 'PRCAT-001', name: 'Retail',       isActive: true, legacyCode: null },
        { code: 'PRCAT-002', name: 'Wholesale',    isActive: true, legacyCode: null },
        { code: 'PRCAT-003', name: 'Distributor',  isActive: true, legacyCode: null },
        { code: 'PRCAT-004', name: 'Reseller',     isActive: true, legacyCode: null },
        { code: 'PRCAT-005', name: 'OEM/Buyer',    isActive: true, legacyCode: null },
        { code: 'PRCAT-006', name: 'Internal',     isActive: true, legacyCode: null },
        { code: 'PRCAT-007', name: 'Export',       isActive: true, legacyCode: null },
        { code: 'PRCAT-008', name: 'Special',      isActive: true, legacyCode: null },
      ];
      const r = await prisma.erpPriceCategory.createMany({ data: rows, skipDuplicates: true });
      return r.count;
    },
  );
}

// ── 9. md_price_indices ───────────────────────────────────────────────────────

async function seedPriceIndices(): Promise<void> {
  await seedIfEmpty(
    'md_price_indices',
    () => prisma.erpPriceIndex.count(),
    async () => {
      const rows = [
        { code: 'PI-001', name: 'Base',         margin: 0,     notes: 'Harga dasar HPP',                isActive: true, legacyCode: null },
        { code: 'PI-002', name: 'Standard',     margin: 0.20,  notes: 'Markup 20% dari harga dasar',    isActive: true, legacyCode: null },
        { code: 'PI-003', name: 'Premium',      margin: 0.35,  notes: 'Markup 35% untuk produk premium',isActive: true, legacyCode: null },
        { code: 'PI-004', name: 'Economy',      margin: -0.10, notes: 'Diskon 10% untuk program hemat', isActive: true, legacyCode: null },
        { code: 'PI-005', name: 'Export',       margin: 0.15,  notes: 'Markup 15% untuk order ekspor',  isActive: true, legacyCode: null },
        { code: 'PI-006', name: 'Wholesale',    margin: -0.15, notes: 'Diskon 15% untuk grosir',        isActive: true, legacyCode: null },
        { code: 'PI-007', name: 'VIP',          margin: 0.05,  notes: 'Markup 5% untuk pelanggan VIP',  isActive: true, legacyCode: null },
        { code: 'PI-008', name: 'Distributor',  margin: -0.20, notes: 'Diskon 20% untuk distributor',   isActive: true, legacyCode: null },
      ];
      const r = await prisma.erpPriceIndex.createMany({ data: rows, skipDuplicates: true });
      return r.count;
    },
  );
}

// ── 10. md_commissions ────────────────────────────────────────────────────────

async function seedCommissions(): Promise<void> {
  await seedIfEmpty(
    'md_commissions',
    () => prisma.erpCommission.count(),
    async () => {
      const rows = [
        { code: 'COM-001', name: 'Regular',   amount: 0.02,  isActive: true, legacyCode: null },
        { code: 'COM-002', name: 'Silver',    amount: 0.03,  isActive: true, legacyCode: null },
        { code: 'COM-003', name: 'Gold',      amount: 0.04,  isActive: true, legacyCode: null },
        { code: 'COM-004', name: 'Platinum',  amount: 0.05,  isActive: true, legacyCode: null },
        { code: 'COM-005', name: 'OEM',       amount: 0.01,  isActive: true, legacyCode: null },
        { code: 'COM-006', name: 'Export',    amount: 0.015, isActive: true, legacyCode: null },
        { code: 'COM-007', name: 'Special',   amount: 0,     isActive: true, legacyCode: null },
        { code: 'COM-008', name: 'Manager',   amount: 0.025, isActive: true, legacyCode: null },
      ];
      const r = await prisma.erpCommission.createMany({ data: rows, skipDuplicates: true });
      return r.count;
    },
  );
}

// ── 11. md_classes ────────────────────────────────────────────────────────────

async function seedClasses(): Promise<void> {
  await seedIfEmpty(
    'md_classes',
    () => prisma.erpClass.count(),
    async () => {
      const rows = [
        { code: 'CLS-001', name: 'Woven',              isActive: true, legacyCode: null },
        { code: 'CLS-002', name: 'Knit',               isActive: true, legacyCode: null },
        { code: 'CLS-003', name: 'Denim',              isActive: true, legacyCode: null },
        { code: 'CLS-004', name: 'Non-Woven',          isActive: true, legacyCode: null },
        { code: 'CLS-005', name: 'Leather',            isActive: true, legacyCode: null },
        { code: 'CLS-006', name: 'Technical Fabric',   isActive: true, legacyCode: null },
        { code: 'CLS-007', name: 'Accessories',        isActive: true, legacyCode: null },
        { code: 'CLS-008', name: 'Packaging Material', isActive: true, legacyCode: null },
        { code: 'CLS-009', name: 'Consumable',         isActive: true, legacyCode: null },
        { code: 'CLS-010', name: 'Raw Material',       isActive: true, legacyCode: null },
      ];
      const r = await prisma.erpClass.createMany({ data: rows, skipDuplicates: true });
      return r.count;
    },
  );
}

// ── 12. md_item_locations ─────────────────────────────────────────────────────

async function seedItemLocations(): Promise<void> {
  await seedIfEmpty(
    'md_item_locations',
    () => prisma.erpItemLocation.count(),
    async () => {
      const rows = [
        { code: 'LOC-RAK-A1',  name: 'Rak A-1 (Bahan Baku)',       warehouseId: null, isActive: true, legacyCode: null },
        { code: 'LOC-RAK-A2',  name: 'Rak A-2 (Bahan Baku)',       warehouseId: null, isActive: true, legacyCode: null },
        { code: 'LOC-RAK-A3',  name: 'Rak A-3 (Bahan Baku)',       warehouseId: null, isActive: true, legacyCode: null },
        { code: 'LOC-RAK-A4',  name: 'Rak A-4 (Aksesori)',         warehouseId: null, isActive: true, legacyCode: null },
        { code: 'LOC-RAK-A5',  name: 'Rak A-5 (Aksesori)',         warehouseId: null, isActive: true, legacyCode: null },
        { code: 'LOC-RAK-B1',  name: 'Rak B-1 (Barang Jadi)',      warehouseId: null, isActive: true, legacyCode: null },
        { code: 'LOC-RAK-B2',  name: 'Rak B-2 (Barang Jadi)',      warehouseId: null, isActive: true, legacyCode: null },
        { code: 'LOC-RAK-B3',  name: 'Rak B-3 (In-Process)',       warehouseId: null, isActive: true, legacyCode: null },
        { code: 'LOC-BULK-01', name: 'Area Bulk 01 (Kain Roll)',    warehouseId: null, isActive: true, legacyCode: null },
        { code: 'LOC-FLOOR-01',name: 'Floor Area 01 (Cutting)',     warehouseId: null, isActive: true, legacyCode: null },
      ];
      const r = await prisma.erpItemLocation.createMany({ data: rows, skipDuplicates: true });
      return r.count;
    },
  );
}

// ── main ──────────────────────────────────────────────────────────────────────

async function main(): Promise<void> {
  console.log('Seeding garment item attribute master data...');

  await seedBrands();
  await seedMaterials();
  await seedColors();
  await seedSizes();
  await seedItemModels();
  await seedSections();
  await seedProductClasses();
  await seedPriceCategories();
  await seedPriceIndices();
  await seedCommissions();
  await seedClasses();
  await seedItemLocations();

  console.log('Done.');
  await prisma.$disconnect();
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
