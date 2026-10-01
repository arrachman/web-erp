/**
 * Seed data untuk konteks pabrik garment/tekstil Indonesia.
 * Run: npx ts-node prisma/seed-erp-manufacturing.ts
 *
 * Idempotent: skip seluruh tabel jika sudah ada >= 1 row.
 *
 * Tabel yang di-seed:
 *   md_production_categories  (8 rows)
 *   md_point_categories        (5 rows)
 *   md_miscellaneous          (10 rows)
 *   md_sub_classes            (10 rows)
 *   md_work_estimates          (8 rows)
 *   md_labors                 (20 rows)
 *   md_machines               (15 rows)
 *   md_designers               (8 rows)
 *   md_production_activities  (12 rows)
 *   md_production_routes       (8 rows)
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

// ── 1. md_production_categories ──────────────────────────────────────────────

async function seedProductionCategories(): Promise<void> {
  await seedIfEmpty(
    'md_production_categories',
    () => prisma.erpProductionCategory.count(),
    async () => {
      const rows = [
        { code: 'PRODCAT-001', name: 'Cutting',    isActive: true, legacyCode: null },
        { code: 'PRODCAT-002', name: 'Sewing',     isActive: true, legacyCode: null },
        { code: 'PRODCAT-003', name: 'Finishing',  isActive: true, legacyCode: null },
        { code: 'PRODCAT-004', name: 'Embroidery', isActive: true, legacyCode: null },
        { code: 'PRODCAT-005', name: 'Printing',   isActive: true, legacyCode: null },
        { code: 'PRODCAT-006', name: 'Washing',    isActive: true, legacyCode: null },
        { code: 'PRODCAT-007', name: 'Packing',    isActive: true, legacyCode: null },
        { code: 'PRODCAT-008', name: 'QC',         isActive: true, legacyCode: null },
      ];
      const r = await prisma.erpProductionCategory.createMany({ data: rows, skipDuplicates: true });
      return r.count;
    },
  );
}

// ── 2. md_point_categories ────────────────────────────────────────────────────

async function seedPointCategories(): Promise<void> {
  await seedIfEmpty(
    'md_point_categories',
    () => prisma.erpPointCategory.count(),
    async () => {
      const rows = [
        { code: 'PTCAT-001', name: 'Quality',    isActive: true, legacyCode: null },
        { code: 'PTCAT-002', name: 'Efficiency', isActive: true, legacyCode: null },
        { code: 'PTCAT-003', name: 'Attendance', isActive: true, legacyCode: null },
        { code: 'PTCAT-004', name: 'Skill',      isActive: true, legacyCode: null },
        { code: 'PTCAT-005', name: 'Innovation', isActive: true, legacyCode: null },
      ];
      const r = await prisma.erpPointCategory.createMany({ data: rows, skipDuplicates: true });
      return r.count;
    },
  );
}

// ── 3. md_miscellaneous ───────────────────────────────────────────────────────

async function seedMiscellaneous(): Promise<void> {
  await seedIfEmpty(
    'md_miscellaneous',
    () => prisma.erpMiscellaneous.count(),
    async () => {
      const rows = [
        { code: 'MISC-001', name: 'Benang Jahit',   isActive: true, legacyCode: null },
        { code: 'MISC-002', name: 'Jarum Jahit',    isActive: true, legacyCode: null },
        { code: 'MISC-003', name: 'Kancing',        isActive: true, legacyCode: null },
        { code: 'MISC-004', name: 'Resleting',      isActive: true, legacyCode: null },
        { code: 'MISC-005', name: 'Tali Celana',    isActive: true, legacyCode: null },
        { code: 'MISC-006', name: 'Label Merek',    isActive: true, legacyCode: null },
        { code: 'MISC-007', name: 'Tag Harga',      isActive: true, legacyCode: null },
        { code: 'MISC-008', name: 'Plastik Pak',    isActive: true, legacyCode: null },
        { code: 'MISC-009', name: 'Stiker Barcode', isActive: true, legacyCode: null },
        { code: 'MISC-010', name: 'Kardus Karton',  isActive: true, legacyCode: null },
      ];
      const r = await prisma.erpMiscellaneous.createMany({ data: rows, skipDuplicates: true });
      return r.count;
    },
  );
}

// ── 4. md_sub_classes ─────────────────────────────────────────────────────────

async function seedSubClasses(): Promise<void> {
  await seedIfEmpty(
    'md_sub_classes',
    () => prisma.erpSubClass.count(),
    async () => {
      const rows = [
        { code: 'SUBCLS-001', name: 'Kemeja Pria',    isActive: true, legacyCode: null },
        { code: 'SUBCLS-002', name: 'Kemeja Wanita',  isActive: true, legacyCode: null },
        { code: 'SUBCLS-003', name: 'Celana Pria',    isActive: true, legacyCode: null },
        { code: 'SUBCLS-004', name: 'Celana Wanita',  isActive: true, legacyCode: null },
        { code: 'SUBCLS-005', name: 'Gaun',           isActive: true, legacyCode: null },
        { code: 'SUBCLS-006', name: 'Rok',            isActive: true, legacyCode: null },
        { code: 'SUBCLS-007', name: 'Jaket',          isActive: true, legacyCode: null },
        { code: 'SUBCLS-008', name: 'Kaos',           isActive: true, legacyCode: null },
        { code: 'SUBCLS-009', name: 'Cardigan',       isActive: true, legacyCode: null },
        { code: 'SUBCLS-010', name: 'Dress Anak',     isActive: true, legacyCode: null },
      ];
      const r = await prisma.erpSubClass.createMany({ data: rows, skipDuplicates: true });
      return r.count;
    },
  );
}

// ── 5. md_work_estimates ──────────────────────────────────────────────────────

async function seedWorkEstimates(): Promise<void> {
  await seedIfEmpty(
    'md_work_estimates',
    () => prisma.erpWorkEstimate.count(),
    async () => {
      const rows = [
        { code: 'WE-001', name: 'CUTTING-STD',   isActive: true, legacyCode: null },
        { code: 'WE-002', name: 'SEWING-STD',    isActive: true, legacyCode: null },
        { code: 'WE-003', name: 'FINISHING-STD', isActive: true, legacyCode: null },
        { code: 'WE-004', name: 'EMBROID-STD',   isActive: true, legacyCode: null },
        { code: 'WE-005', name: 'PRINT-STD',     isActive: true, legacyCode: null },
        { code: 'WE-006', name: 'WASH-STD',      isActive: true, legacyCode: null },
        { code: 'WE-007', name: 'PACK-STD',      isActive: true, legacyCode: null },
        { code: 'WE-008', name: 'QC-STD',        isActive: true, legacyCode: null },
      ];
      const r = await prisma.erpWorkEstimate.createMany({ data: rows, skipDuplicates: true });
      return r.count;
    },
  );
}

// ── 6. md_labors ──────────────────────────────────────────────────────────────

async function seedLabors(): Promise<void> {
  await seedIfEmpty(
    'md_labors',
    () => prisma.erpLabor.count(),
    async () => {
      const rows = [
        { code: 'LBR-001', name: 'Operator Mesin Jahit',      isActive: true, legacyCode: null },
        { code: 'LBR-002', name: 'Operator Cutting',          isActive: true, legacyCode: null },
        { code: 'LBR-003', name: 'Operator Obras',            isActive: true, legacyCode: null },
        { code: 'LBR-004', name: 'Operator Bordir',           isActive: true, legacyCode: null },
        { code: 'LBR-005', name: 'Operator Print',            isActive: true, legacyCode: null },
        { code: 'LBR-006', name: 'Operator Washing',          isActive: true, legacyCode: null },
        { code: 'LBR-007', name: 'Quality Control Inspector', isActive: true, legacyCode: null },
        { code: 'LBR-008', name: 'Packer',                    isActive: true, legacyCode: null },
        { code: 'LBR-009', name: 'Supervisor Produksi',       isActive: true, legacyCode: null },
        { code: 'LBR-010', name: 'Helper',                    isActive: true, legacyCode: null },
        { code: 'LBR-011', name: 'Operator Overdeck',         isActive: true, legacyCode: null },
        { code: 'LBR-012', name: 'Operator Bartack',          isActive: true, legacyCode: null },
        { code: 'LBR-013', name: 'Operator Kancing',          isActive: true, legacyCode: null },
        { code: 'LBR-014', name: 'Operator Pasang Label',     isActive: true, legacyCode: null },
        { code: 'LBR-015', name: 'Operator Setrika',          isActive: true, legacyCode: null },
        { code: 'LBR-016', name: 'Folder/Nder',               isActive: true, legacyCode: null },
        { code: 'LBR-017', name: 'Operator Finishing',        isActive: true, legacyCode: null },
        { code: 'LBR-018', name: 'Koordinator Line',          isActive: true, legacyCode: null },
        { code: 'LBR-019', name: 'Mekanik Mesin',             isActive: true, legacyCode: null },
        { code: 'LBR-020', name: 'Admin Produksi',            isActive: true, legacyCode: null },
      ];
      const r = await prisma.erpLabor.createMany({ data: rows, skipDuplicates: true });
      return r.count;
    },
  );
}

// ── 7. md_machines ────────────────────────────────────────────────────────────

async function seedMachines(): Promise<void> {
  await seedIfEmpty(
    'md_machines',
    () => prisma.erpMachine.count(),
    async () => {
      const rows = [
        { code: 'MCH-001', name: 'Mesin Jahit Single Needle',  isActive: true, legacyCode: null },
        { code: 'MCH-002', name: 'Mesin Obras 3 Benang',       isActive: true, legacyCode: null },
        { code: 'MCH-003', name: 'Mesin Obras 5 Benang',       isActive: true, legacyCode: null },
        { code: 'MCH-004', name: 'Mesin Bartack',              isActive: true, legacyCode: null },
        { code: 'MCH-005', name: 'Mesin Kancing Lubang',       isActive: true, legacyCode: null },
        { code: 'MCH-006', name: 'Mesin Kancing Pasang',       isActive: true, legacyCode: null },
        { code: 'MCH-007', name: 'Mesin Overdeck',             isActive: true, legacyCode: null },
        { code: 'MCH-008', name: 'Mesin Bordir',               isActive: true, legacyCode: null },
        { code: 'MCH-009', name: 'Mesin Setrika Steam',        isActive: true, legacyCode: null },
        { code: 'MCH-010', name: 'Mesin Press',                isActive: true, legacyCode: null },
        { code: 'MCH-011', name: 'Mesin Cutting Straight',     isActive: true, legacyCode: null },
        { code: 'MCH-012', name: 'Mesin Cutting Round',        isActive: true, legacyCode: null },
        { code: 'MCH-013', name: 'Mesin Sablon Karusel',       isActive: true, legacyCode: null },
        { code: 'MCH-014', name: 'Mesin Digital Print',        isActive: true, legacyCode: null },
        { code: 'MCH-015', name: 'Mesin Washing',              isActive: true, legacyCode: null },
      ];
      const r = await prisma.erpMachine.createMany({ data: rows, skipDuplicates: true });
      return r.count;
    },
  );
}

// ── 8. md_designers ───────────────────────────────────────────────────────────

async function seedDesigners(): Promise<void> {
  await seedIfEmpty(
    'md_designers',
    () => prisma.erpDesigner.count(),
    async () => {
      const rows = [
        { code: 'DSG-001', name: 'Tech Pack - Womenswear',  isActive: true, legacyCode: null },
        { code: 'DSG-002', name: 'Tech Pack - Menswear',    isActive: true, legacyCode: null },
        { code: 'DSG-003', name: 'Pattern Maker',           isActive: true, legacyCode: null },
        { code: 'DSG-004', name: 'Grader',                  isActive: true, legacyCode: null },
        { code: 'DSG-005', name: 'Fashion Illustrator',     isActive: true, legacyCode: null },
        { code: 'DSG-006', name: 'Textile Designer',        isActive: true, legacyCode: null },
        { code: 'DSG-007', name: 'CAD Operator',            isActive: true, legacyCode: null },
        { code: 'DSG-008', name: 'Sample Maker',            isActive: true, legacyCode: null },
      ];
      const r = await prisma.erpDesigner.createMany({ data: rows, skipDuplicates: true });
      return r.count;
    },
  );
}

// ── 9. md_production_activities ───────────────────────────────────────────────

async function seedProductionActivities(): Promise<void> {
  await seedIfEmpty(
    'md_production_activities',
    () => prisma.erpProductionActivity.count(),
    async () => {
      const rows = [
        { code: 'ACT-001', name: 'Material Inspection', isActive: true, legacyCode: null },
        { code: 'ACT-002', name: 'Marker Making',       isActive: true, legacyCode: null },
        { code: 'ACT-003', name: 'Spreading',           isActive: true, legacyCode: null },
        { code: 'ACT-004', name: 'Cutting',             isActive: true, legacyCode: null },
        { code: 'ACT-005', name: 'Bundling',            isActive: true, legacyCode: null },
        { code: 'ACT-006', name: 'Stitching',           isActive: true, legacyCode: null },
        { code: 'ACT-007', name: 'Pressing',            isActive: true, legacyCode: null },
        { code: 'ACT-008', name: 'Quality Inspection',  isActive: true, legacyCode: null },
        { code: 'ACT-009', name: 'Trimming',            isActive: true, legacyCode: null },
        { code: 'ACT-010', name: 'Folding',             isActive: true, legacyCode: null },
        { code: 'ACT-011', name: 'Tagging',             isActive: true, legacyCode: null },
        { code: 'ACT-012', name: 'Packing',             isActive: true, legacyCode: null },
      ];
      const r = await prisma.erpProductionActivity.createMany({ data: rows, skipDuplicates: true });
      return r.count;
    },
  );
}

// ── 10. md_production_routes ──────────────────────────────────────────────────

async function seedProductionRoutes(): Promise<void> {
  await seedIfEmpty(
    'md_production_routes',
    () => prisma.erpProductionRoute.count(),
    async () => {
      const rows = [
        { code: 'ROUTE-001', name: 'Standard Woven',  isActive: true, legacyCode: null },
        { code: 'ROUTE-002', name: 'Standard Knit',   isActive: true, legacyCode: null },
        { code: 'ROUTE-003', name: 'Express Basic',   isActive: true, legacyCode: null },
        { code: 'ROUTE-004', name: 'Premium Woven',   isActive: true, legacyCode: null },
        { code: 'ROUTE-005', name: 'Casual Knit',     isActive: true, legacyCode: null },
        { code: 'ROUTE-006', name: 'Formal Woven',    isActive: true, legacyCode: null },
        { code: 'ROUTE-007', name: 'Kids Basic',      isActive: true, legacyCode: null },
        { code: 'ROUTE-008', name: 'Outerwear',       isActive: true, legacyCode: null },
      ];
      const r = await prisma.erpProductionRoute.createMany({ data: rows, skipDuplicates: true });
      return r.count;
    },
  );
}

// ── main ──────────────────────────────────────────────────────────────────────

async function main(): Promise<void> {
  console.log('Seeding garment manufacturing master data...');

  await seedProductionCategories();
  await seedPointCategories();
  await seedMiscellaneous();
  await seedSubClasses();
  await seedWorkEstimates();
  await seedLabors();
  await seedMachines();
  await seedDesigners();
  await seedProductionActivities();
  await seedProductionRoutes();

  console.log('Done.');
  await prisma.$disconnect();
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
