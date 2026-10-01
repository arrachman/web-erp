/**
 * Replace dummy md_items (and related) with 600 real items from legacy MyERP+.
 *
 * Actions:
 *   1. Hard-delete all DUMMY-* item_informations, items, categories, units.
 *   2. Upsert real units (from legacy + extras needed).
 *   3. Upsert real item categories (30 from legacy 0_barangbaru).
 *   4. Insert 600 real items parsed from legacy 0_barangbaru.
 *
 * Idempotent: skips items already inserted (skipDuplicates on code).
 * Run: npx ts-node prisma/seed-erp-items-real.ts
 */
import * as fs from 'fs';
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();
const LEGACY_SQL = '/home/rania/apps/myerpplus_serenity.sql';
const TARGET_ITEMS = 600;

// ─── Type mapping from legacy tipebarang ────────────────────────────────────
type ErpItemType = 'INVENTORY' | 'SERVICE' | 'CONSUMABLE' | 'ASSET' | 'NON_INVENTORY';
// Legacy tipebarang = the business-role/stage axis (kind), not system behavior.
// System `type` here reflects stock nature; assembly-ness is a separate fact (mfg_boms).
const TYPE_MAP: Record<string, ErpItemType> = {
  FG:  'INVENTORY',   // Finished Goods
  RM:  'INVENTORY',   // Raw Material
  WIP: 'INVENTORY',   // Work in Progress (was ASSEMBLY)
  MRO: 'INVENTORY',   // Maintenance, Repair, Operations
  FA:  'INVENTORY',   // Finished Assembly (was ASSEMBLY)
  ORM: 'INVENTORY',   // Other Raw Material
};

// ─── Real units to upsert ────────────────────────────────────────────────────
// Only those NOT already in the 21 non-dummy units seeded earlier.
const UNITS_TO_ADD: Array<{ code: string; name: string; notes?: string }> = [
  { code: 'BUKU',  name: 'Buku',         notes: 'Legacy unit' },
  { code: 'COLT',  name: 'Colet',        notes: 'Legacy unit' },
  { code: 'DUS',   name: 'Dus/Karton',   notes: 'Legacy unit' },
  { code: 'MTR',   name: 'Meter',        notes: 'Legacy unit' },
  { code: 'PSG',   name: 'Pasang',       notes: 'Legacy unit' },
  { code: 'RIM',   name: 'Rim',          notes: 'Legacy unit' },
  { code: 'ROLL',  name: 'Roll',         notes: 'Legacy unit' },
  { code: 'SET',   name: 'Set',          notes: 'Legacy unit' },
  { code: 'SLOP',  name: 'Slop',         notes: 'Legacy unit' },
];

// ─── Real item categories (from legacy kategoribarang codes) ────────────────
// Code = legacyCode (no 'CAT-' prefix; redundant — entity scope is category).
const CATEGORIES: Array<{ code: string; name: string; legacyCode: string }> = [
  { code: 'AB',  name: 'Accessories / Bumper',      legacyCode: 'AB' },
  { code: 'AL',  name: 'Aluminium',                  legacyCode: 'AL' },
  { code: 'AP',  name: 'Auto Parts',                 legacyCode: 'AP' },
  { code: 'APD', name: 'APD / Safety Equipment',     legacyCode: 'APD' },
  { code: 'ATK', name: 'Alat Tulis Kantor',          legacyCode: 'ATK' },
  { code: 'BAR', name: 'Bar Stock',                  legacyCode: 'BAR' },
  { code: 'BT',  name: 'Bolt & Fastener',            legacyCode: 'BT' },
  { code: 'CH',  name: 'Chain',                      legacyCode: 'CH' },
  { code: 'CON', name: 'Consumable',                 legacyCode: 'CON' },
  { code: 'EL',  name: 'Electrical',                 legacyCode: 'EL' },
  { code: 'FA',  name: 'Finished Assembly',          legacyCode: 'FA' },
  { code: 'FL',  name: 'Flat / Wire',                legacyCode: 'FL' },
  { code: 'HL',  name: 'Hollow',                     legacyCode: 'HL' },
  { code: 'HX',  name: 'Hex / Hexagonal',            legacyCode: 'HX' },
  { code: 'IN',  name: 'Injection Part',             legacyCode: 'IN' },
  { code: 'JS',  name: 'Jasa / Service',             legacyCode: 'JS' },
  { code: 'KN',  name: 'Knob',                       legacyCode: 'KN' },
  { code: 'LM',  name: 'Laminasi',                   legacyCode: 'LM' },
  { code: 'MM',  name: 'Metal Miscellaneous',        legacyCode: 'MM' },
  { code: 'NC',  name: 'NC Parts / CNC',             legacyCode: 'NC' },
  { code: 'OBT', name: 'Obat / Chemical',            legacyCode: 'OBT' },
  { code: 'PAC', name: 'Packaging',                  legacyCode: 'PAC' },
  { code: 'PB',  name: 'Pipe / Tube (PB)',           legacyCode: 'PB' },
  { code: 'RD',  name: 'Rod',                        legacyCode: 'RD' },
  { code: 'RS',  name: 'Rod Special',                legacyCode: 'RS' },
  { code: 'ST',  name: 'Strip',                      legacyCode: 'ST' },
  { code: 'TB',  name: 'Tube',                       legacyCode: 'TB' },
  { code: 'TM',  name: 'Thermal / Heat Treatment',  legacyCode: 'TM' },
  { code: 'YZ',  name: 'Yamato Zipper',              legacyCode: 'YZ' },
  { code: 'ZN',  name: 'Zinc / Zamak',               legacyCode: 'ZN' },
];

// ─── Parser ──────────────────────────────────────────────────────────────────
interface LegacyItem {
  code: string;
  name: string;
  unit: string;
  type: string;
  cat: string;
}

function parseLegacyItems(): LegacyItem[] {
  const sql = fs.readFileSync(LEGACY_SQL, 'latin1');
  const re = /INSERT INTO 0_barangbaru VALUES \('([^']*)', '([^']*)', '([^']*)', '([^']*)', '([^']*)', '([^']*)'/g;
  const items: LegacyItem[] = [];
  let m: RegExpExecArray | null;
  while ((m = re.exec(sql)) !== null && items.length < TARGET_ITEMS) {
    const code = m[2].trim();
    const name = m[3].trim();
    const unit = m[4].trim();
    const type = m[5].trim();
    const cat  = m[6].trim();
    if (code && name && code.length <= 100 && name.length <= 255) {
      items.push({ code, name, unit, type, cat });
    }
  }
  return items;
}

// ─── Main ────────────────────────────────────────────────────────────────────
async function main(): Promise<void> {

  // ── 1. Delete dummy item_informations ───────────────────────────────────
  const dummyItems = await prisma.erpItem.findMany({
    where: { code: { startsWith: 'DUMMY' } },
    select: { id: true },
  });
  const dummyIds = dummyItems.map(i => i.id);
  if (dummyIds.length > 0) {
    const del1 = await prisma.erpItemInformation.deleteMany({
      where: { itemId: { in: dummyIds } },
    });
    console.log(`- Deleted ${del1.count} dummy item_informations`);

    const del2 = await prisma.erpItem.deleteMany({
      where: { id: { in: dummyIds } },
    });
    console.log(`- Deleted ${del2.count} dummy md_items`);
  } else {
    console.log('- No dummy items found, skip delete');
  }

  // ── 2. Delete dummy categories ───────────────────────────────────────────
  const del3 = await prisma.erpItemCategory.deleteMany({
    where: { code: { startsWith: 'DUMMY' } },
  });
  console.log(`- Deleted ${del3.count} dummy md_item_categories`);

  // ── 3. Delete dummy units ────────────────────────────────────────────────
  const del4 = await prisma.erpUnit.deleteMany({
    where: { code: { startsWith: 'DUMMY' } },
  });
  console.log(`- Deleted ${del4.count} dummy md_units`);

  // ── 4. Upsert additional real units ─────────────────────────────────────
  let unitsAdded = 0;
  for (const u of UNITS_TO_ADD) {
    await prisma.erpUnit.upsert({
      where: { code: u.code },
      create: { code: u.code, name: u.name, conversionFactor: 1, notes: u.notes },
      update: {},
    });
    unitsAdded++;
  }
  console.log(`+ Upserted ${unitsAdded} real units`);

  // ── 5. Upsert real categories ────────────────────────────────────────────
  let catsAdded = 0;
  for (const c of CATEGORIES) {
    await prisma.erpItemCategory.upsert({
      where: { code: c.code },
      create: { code: c.code, name: c.name, legacyCode: c.legacyCode },
      update: {},
    });
    catsAdded++;
  }
  console.log(`+ Upserted ${catsAdded} real item categories`);

  // ── 6. Load unit & category lookup maps ─────────────────────────────────
  const allUnits = await prisma.erpUnit.findMany({
    where: { deletedAt: null },
    select: { id: true, code: true },
  });
  const unitMap = new Map<string, bigint>(allUnits.map(u => [u.code, u.id]));

  const allCats = await prisma.erpItemCategory.findMany({
    where: { deletedAt: null },
    select: { id: true, legacyCode: true },
  });
  const catMap = new Map<string, bigint>(
    allCats.filter(c => c.legacyCode).map(c => [c.legacyCode!, c.id]),
  );

  // Fallback unit (KG — always exists)
  const fallbackUnitId = unitMap.get('KG') ?? allUnits[0]?.id;
  // Fallback category (MM — metal misc)
  const fallbackCatId = catMap.get('MM') ?? allCats[0]?.id;

  if (!fallbackUnitId || !fallbackCatId) {
    throw new Error('Missing fallback unit or category — cannot proceed');
  }

  // ── 7. Parse legacy items ────────────────────────────────────────────────
  console.log(`Parsing legacy items from ${LEGACY_SQL} …`);
  const legacyItems = parseLegacyItems();
  console.log(`  Found ${legacyItems.length} items in legacy SQL`);

  // ── 8. Insert real items ─────────────────────────────────────────────────
  // Unit codes in legacy match our md_units codes directly for: KG, LBR, LTR,
  // PACK, PCS, ROLL, SET, MTR, DUS, BUKU, PSG, RIM, SLOP, COLT, UNIT.
  const rows = legacyItems.map(item => {
    const unitId = unitMap.get(item.unit) ?? fallbackUnitId;
    const catId  = catMap.get(item.cat) ?? fallbackCatId;
    const erpType: ErpItemType = TYPE_MAP[item.type] ?? 'INVENTORY';
    return {
      code:       item.code,
      name:       item.name,
      type:       erpType,
      categoryId: catId,
      baseUnitId: unitId,
      legacyCode: item.code,
    };
  });

  const result = await prisma.erpItem.createMany({
    data: rows,
    skipDuplicates: true,
  });
  console.log(`+ Inserted ${result.count}/${rows.length} real md_items`);

  // ── 9. Final summary ─────────────────────────────────────────────────────
  const total = await prisma.erpItem.count({ where: { deletedAt: null } });
  const totalCats = await prisma.erpItemCategory.count({ where: { deletedAt: null } });
  const totalUnits = await prisma.erpUnit.count({ where: { deletedAt: null } });
  console.log(`\nSummary: ${total} items | ${totalCats} categories | ${totalUnits} units`);
}

main()
  .catch(e => {
    console.error(e);
    process.exit(1);
  })
  .finally(() => prisma.$disconnect());
