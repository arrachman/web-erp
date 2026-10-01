/**
 * Seed data geografis Indonesia lengkap:
 *   38 provinsi, 514 kab/kota, 7286 kecamatan, 84270 kelurahan/desa
 * Sumber: kode-wilayah-id (MIT license) — BPS codes, postal codes included.
 * Idempotent: createMany skipDuplicates=true; upsert per code untuk parent.
 * Prasyarat: seed-md-legacy.ts sudah jalan (Country 'ID' harus ada).
 */
import { PrismaClient } from '@prisma/client';
import {
  getProvinces,
  getRegencies,
  getDistricts,
  getVillages,
} from 'kode-wilayah-id';

const prisma = new PrismaClient();
const BATCH = 500;

function toTitle(s: string): string {
  return s
    .toLowerCase()
    .replace(/\b\w/g, (c) => c.toUpperCase())
    .replace(/\bKab\b/g, 'Kab.')
    .replace(/\bKota\b/g, 'Kota');
}

async function insertBatches<T extends object>(
  label: string,
  model: { createMany: (args: { data: T[]; skipDuplicates: boolean }) => Promise<{ count: number }> },
  rows: T[],
): Promise<number> {
  let total = 0;
  for (let i = 0; i < rows.length; i += BATCH) {
    const { count } = await model.createMany({ data: rows.slice(i, i + BATCH), skipDuplicates: true });
    total += count;
    if (i % (BATCH * 20) === 0) process.stdout.write(`\r  ${label}: ${i}/${rows.length}...`);
  }
  console.log(`\r  ${label}: ${rows.length} input → ${total} inserted (${rows.length - total} skipped)`);
  return total;
}

async function main() {
  // ── 1. Country ──────────────────────────────────────────────────────────────
  const indonesia = await prisma.erpCountry.findUnique({ where: { code: 'ID' } });
  if (!indonesia) throw new Error('Country ID not found — run db:seed:erp or seed-md-legacy.ts first');

  // ── 2. Provinces (38 BPS 2-digit codes) ────────────────────────────────────
  console.log('Seeding provinces...');
  const rawProvinces = getProvinces();
  for (const p of rawProvinces) {
    await prisma.erpProvince.upsert({
      where:  { code: p.bps_code },
      update: { name: toTitle(p.name), countryId: indonesia.id },
      create: { code: p.bps_code, name: toTitle(p.name), countryId: indonesia.id },
    });
  }
  console.log(`  Provinces: ${rawProvinces.length} upserted`);

  // Province id map: bps_code → DB id
  const provRows = await prisma.erpProvince.findMany({ where: { countryId: indonesia.id }, select: { id: true, code: true } });
  const provMap: Record<string, bigint> = Object.fromEntries(provRows.map((p) => [p.code, p.id]));

  // ── 3. Regencies / Cities (514) ─────────────────────────────────────────────
  console.log('Seeding regencies/cities...');
  const rawRegencies = getRegencies();
  for (const r of rawRegencies) {
    const provinceId = provMap[r.bps_province_code];
    if (!provinceId) continue;
    await prisma.erpCity.upsert({
      where:  { code: r.bps_code },
      update: { name: toTitle(r.name), provinceId },
      create: { code: r.bps_code, name: toTitle(r.name), provinceId },
    });
  }
  console.log(`  Cities: ${rawRegencies.length} upserted`);

  // City id map: bps_code (4-digit) → DB id
  const cityRows = await prisma.erpCity.findMany({ select: { id: true, code: true } });
  const cityMap: Record<string, bigint> = Object.fromEntries(cityRows.map((c) => [c.code, c.id]));

  // ── 4. Districts / Areas / Kecamatan (7286) ─────────────────────────────────
  console.log('Seeding kecamatan (areas)...');
  const rawDistricts = getDistricts();
  const rawVillages  = getVillages();

  // Build first postal code per district from villages
  const distPostal: Record<string, string> = {};
  for (const v of rawVillages) {
    if (v.postal_code && !distPostal[v.bps_district_code]) {
      distPostal[v.bps_district_code] = v.postal_code;
    }
  }

  const areaRows = rawDistricts
    .filter((d) => cityMap[d.bps_regency_code])
    .map((d) => ({
      code:       d.bps_code,
      name:       toTitle(d.name),
      cityId:     cityMap[d.bps_regency_code],
      postalCode: distPostal[d.bps_code] ?? null,
      isActive:   true,
      legacyCode: null as string | null,
    }));

  await insertBatches('kecamatan', prisma.erpArea, areaRows);

  // Area id map: bps_code (7-digit) → DB id
  const areaDbRows = await prisma.erpArea.findMany({ select: { id: true, code: true } });
  const areaMap: Record<string, bigint> = Object.fromEntries(areaDbRows.map((a) => [a.code, a.id]));

  // ── 5. Villages / SubAreas / Kelurahan (84270) ──────────────────────────────
  console.log('Seeding kelurahan (sub areas)...');
  const subAreaRows = rawVillages
    .filter((v) => areaMap[v.bps_district_code])
    .map((v) => ({
      code:       v.bps_code,
      name:       toTitle(v.name),
      areaId:     areaMap[v.bps_district_code],
      postalCode: v.postal_code ?? null,
      isActive:   true,
      legacyCode: null as string | null,
    }));

  await insertBatches('kelurahan', prisma.erpSubArea, subAreaRows);

  // ── Summary ──────────────────────────────────────────────────────────────────
  const [pCount, cCount, aCount, saCount] = await Promise.all([
    prisma.erpProvince.count(),
    prisma.erpCity.count(),
    prisma.erpArea.count(),
    prisma.erpSubArea.count(),
  ]);
  console.log('\n✓ Geographic seed complete:', { provinces: pCount, cities: cCount, kecamatan: aCount, kelurahan: saCount });
}

main().catch((e) => { console.error(e); process.exit(1); }).finally(() => prisma.$disconnect());
