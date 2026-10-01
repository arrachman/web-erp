/**
 * Seed 8 item kinds dari legacy MyERP+ (md_item_types).
 * Run: npx ts-node --project tsconfig.json prisma/seed-erp-item-kinds-legacy.ts
 *
 * Idempotent: pakai createMany skipDuplicates — aman dijalankan
 * berulang dan tidak menghapus data existing.
 */
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

const LEGACY_ITEM_KINDS = [
  { code: 'FA',  name: 'Fixed Asset',                                   legacyCode: 'FA'  },
  { code: 'FG',  name: 'Finish Goods',                                  legacyCode: 'FG'  },
  { code: 'MRO', name: 'Maintenance, Repair, Operation',                legacyCode: 'MRO' },
  { code: 'ORM', name: 'Overhead, Repair, Maintenance to Sparepart Mesin', legacyCode: 'ORM' },
  { code: 'RM',  name: 'Raw Material',                                  legacyCode: 'RM'  },
  { code: 'UM',  name: 'Umum',                                          legacyCode: 'UM'  },
  { code: 'WIP', name: 'Work in Process',                               legacyCode: 'WIP' },
  { code: 'WO',  name: 'Asset in Progress',                             legacyCode: 'WO'  },
];

async function main(): Promise<void> {
  const result = await prisma.erpItemKind.createMany({
    data: LEGACY_ITEM_KINDS.map((r) => ({ ...r, isActive: true })),
    skipDuplicates: true,
  });
  console.log(`+ md_item_types (legacy): inserted ${result.count} rows (skipped duplicates).`);
}

main()
  .catch((e) => { console.error(e); process.exit(1); })
  .finally(() => prisma.$disconnect());
