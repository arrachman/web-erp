/**
 * One-off: seed 500 dummy salesman partner rows into md_partners.
 * Run: npx ts-node prisma/seed-erp-salesman-dummy.ts
 *
 * Idempotent: skips if md_partners already has >=500 salesman rows.
 */
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

const FIRST_NAMES = [
  'Andi',
  'Budi',
  'Citra',
  'Dedi',
  'Eko',
  'Fajar',
  'Galih',
  'Hendra',
  'Indra',
  'Joko',
  'Kevin',
  'Lina',
  'Muhammad',
  'Nina',
  'Oscar',
  'Putri',
  'Rudi',
  'Sari',
  'Tono',
  'Umar',
  'Vina',
  'Wahyu',
  'Yanuar',
  'Zahra',
  'Ahmad',
  'Bagas',
  'Cindy',
  'Dian',
  'Erwin',
  'Fitri',
  'Guntur',
  'Hani',
  'Irfan',
  'Jeni',
  'Krisna',
  'Laila',
  'Mirza',
  'Nanda',
  'Oki',
  'Pita',
  'Rahmat',
  'Sonya',
  'Teguh',
  'Ulfa',
  'Vito',
  'Wulan',
  'Yudi',
  'Zara',
] as const;

const LAST_NAMES = [
  'Santoso',
  'Wijaya',
  'Hidayat',
  'Kusuma',
  'Pratama',
  'Setiawan',
  'Nugroho',
  'Purnomo',
  'Saputra',
  'Wibowo',
  'Hartono',
  'Susanto',
  'Yulianto',
  'Firmansyah',
  'Ramadhani',
  'Maulana',
  'Ardiansyah',
  'Oktaviani',
  'Kurniawan',
  'Handoyo',
  'Gunawan',
  'Siregar',
  'Nasution',
  'Sinaga',
  'Harahap',
  'Manurung',
  'Situmorang',
  'Panjaitan',
  'Hutabarat',
] as const;

const rp = <T>(a: readonly T[]): T => a[Math.floor(Math.random() * a.length)];
const pad = (n: number) => String(n).padStart(4, '0');

async function main() {
  const salesmanType = await prisma.erpPartnerType.findUnique({ where: { code: 'SLS' } });
  if (!salesmanType) throw new Error('Missing md_partner_types SLS seed');

  const existing = await prisma.erpPartner.count({ where: { partnerTypeId: salesmanType.id } });
  if (existing >= 500) {
    console.log(`✓ Skipped: already ${existing} salesman rows`);
    return;
  }

  const salesmanCats = await prisma.erpPartnerCategory.findMany({
    where: { kind: 'SALESMAN' },
    select: { id: true },
    take: 20,
  });

  const rows = Array.from({ length: 500 }, (_, i) => ({
    code: `SLS-${pad(i + 1)}`,
    name: `${rp(FIRST_NAMES)} ${rp(LAST_NAMES)}`,
    partnerTypeId: salesmanType.id,
    salesmanCategoryId: salesmanCats.length ? salesmanCats[i % salesmanCats.length].id : null,
    isActive: true,
  }));

  const result = await prisma.erpPartner.createMany({ data: rows, skipDuplicates: true });
  console.log(`✓ Inserted ${result.count} salesman rows`);
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(() => prisma.$disconnect());
