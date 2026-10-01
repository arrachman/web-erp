/**
 * One-off: add 1000 dummy branches to md_branches.
 * Run: npm run db:seed:erp:branches:dummy --workspace=apps/api-gateway
 * Or:  npx ts-node prisma/seed-erp-branches-dummy.ts
 *
 * Idempotent: skips codes that already exist (code is UNIQUE).
 */
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

const CITIES = [
  'Jakarta', 'Bandung', 'Surabaya', 'Medan', 'Semarang', 'Makassar', 'Palembang',
  'Tangerang', 'Depok', 'Bekasi', 'Bogor', 'Batam', 'Pekanbaru', 'Denpasar',
  'Yogyakarta', 'Malang', 'Solo', 'Padang', 'Manado', 'Pontianak', 'Banjarmasin',
  'Balikpapan', 'Samarinda', 'Cirebon', 'Tasikmalaya', 'Serang', 'Cilegon',
  'Sukabumi', 'Magelang', 'Salatiga', 'Tegal', 'Pekalongan', 'Kediri', 'Madiun',
  'Probolinggo', 'Pasuruan', 'Mojokerto', 'Banyuwangi', 'Jember', 'Lampung',
  'Jambi', 'Bengkulu', 'Mataram', 'Kupang', 'Ambon', 'Jayapura', 'Sorong',
  'Ternate', 'Gorontalo', 'Palu', 'Kendari', 'Tarakan',
];

const STREETS = [
  'Jl. Merdeka', 'Jl. Sudirman', 'Jl. Thamrin', 'Jl. Gatot Subroto',
  'Jl. Diponegoro', 'Jl. Ahmad Yani', 'Jl. Pahlawan', 'Jl. Veteran',
  'Jl. Imam Bonjol', 'Jl. Hayam Wuruk', 'Jl. Asia Afrika', 'Jl. Cendana',
  'Jl. Mawar', 'Jl. Melati', 'Jl. Anggrek', 'Jl. Kenanga',
];

function randPick<T>(arr: T[]): T {
  return arr[Math.floor(Math.random() * arr.length)];
}

function randPostal(): string {
  return String(10000 + Math.floor(Math.random() * 89999));
}

function randPhone(): string {
  const area = ['021', '022', '024', '031', '0274', '0341', '061', '0411', '0711'];
  const a = randPick(area);
  return `${a}-${1000000 + Math.floor(Math.random() * 8999999)}`;
}

async function main() {
  const COUNT = 1000;
  const PREFIX = 'DUMMY';

  const existing = await prisma.erpBranch.findMany({
    where: { code: { startsWith: PREFIX } },
    select: { code: true },
  });
  const existingSet = new Set(existing.map((b) => b.code));

  const rows: Array<{
    code: string;
    name: string;
    addressLine1: string;
    city: string;
    postalCode: string;
    phone: string;
    notes: string;
    isActive: boolean;
  }> = [];

  for (let i = 1; i <= COUNT; i++) {
    const code = `${PREFIX}${String(i).padStart(4, '0')}`;
    if (existingSet.has(code)) continue;
    const city = randPick(CITIES);
    rows.push({
      code,
      name: `Cabang ${city} ${String(i).padStart(4, '0')}`,
      addressLine1: `${randPick(STREETS)} No. ${1 + Math.floor(Math.random() * 999)}`,
      city,
      postalCode: randPostal(),
      phone: randPhone(),
      notes: 'Dummy data — bulk seed',
      isActive: Math.random() > 0.1, // ~90% active
    });
  }

  if (rows.length === 0) {
    console.log(`✓ All ${COUNT} dummy branches already exist. Nothing to do.`);
    return;
  }

  // createMany skipDuplicates as belt-and-suspenders
  const result = await prisma.erpBranch.createMany({
    data: rows,
    skipDuplicates: true,
  });

  console.log(`✓ Inserted ${result.count} dummy branches (requested ${rows.length}, skipped ${COUNT - rows.length} pre-existing).`);
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(async () => {
    await prisma.$disconnect();
  });
