/**
 * One-shot migration: ganti format kode `md_accounts.code` ke `NNNN.NN.NNN`.
 * Decision 2026-05-27 di `apps/web-erp/db-design/README.md §8` #43.
 *
 * Strategi:
 *   1. Pre-check: pastikan tidak ada FK refs aktif ke akun format lama
 *      (md_partners.payable/receivable, md_items.inventory/cogs/sales,
 *      fin_journal_lines.accountId). Jika ada → abort, perlu in-place UPDATE.
 *   2. DELETE semua row yang TIDAK match `^\d{4}\.\d{2}\.\d{3}$`.
 *   3. User panggil `seed-erp-accounts.ts` setelahnya (atau lewat `npm run db:seed`).
 *
 * Run: npx ts-node prisma/migrate-erp-accounts-coa-format.ts
 * Idempotent: re-run aman; abort cepat kalau sudah dalam format baru.
 */
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();
const NEW_FORMAT = /^\d{4}\.\d{2}\.\d{3}$/;

async function main() {
  console.log('Migrasi format kode md_accounts → NNNN.NN.NNN');

  const all = await prisma.erpAccount.findMany({ select: { id: true, code: true } });
  const oldFmt = all.filter((r) => !NEW_FORMAT.test(r.code));
  const newFmt = all.filter((r) => NEW_FORMAT.test(r.code));
  console.log(`  current: ${all.length} total · ${newFmt.length} new-format · ${oldFmt.length} old-format`);

  if (oldFmt.length === 0) {
    console.log('Tidak ada row format lama. Selesai.');
    return;
  }

  const oldIds = oldFmt.map((r) => r.id);

  const [partnerPayable, partnerRecv, itemInv, itemCogs, itemSales, journalRefs] = await Promise.all([
    prisma.erpPartner.count({ where: { payableAccountId: { in: oldIds } } }),
    prisma.erpPartner.count({ where: { receivableAccountId: { in: oldIds } } }),
    prisma.erpItem.count({ where: { inventoryAccountId: { in: oldIds } } }),
    prisma.erpItem.count({ where: { cogsAccountId: { in: oldIds } } }),
    prisma.erpItem.count({ where: { salesAccountId: { in: oldIds } } }),
    prisma.erpFinJournalLine.count({ where: { accountId: { in: oldIds } } }),
  ]);

  const totalRefs = partnerPayable + partnerRecv + itemInv + itemCogs + itemSales + journalRefs;
  if (totalRefs > 0) {
    console.error('ABORT: ada FK refs aktif ke akun format lama:');
    console.error(`  partner.payableAccountId   : ${partnerPayable}`);
    console.error(`  partner.receivableAccountId: ${partnerRecv}`);
    console.error(`  item.inventoryAccountId    : ${itemInv}`);
    console.error(`  item.cogsAccountId         : ${itemCogs}`);
    console.error(`  item.salesAccountId        : ${itemSales}`);
    console.error(`  fin_journal_lines.accountId: ${journalRefs}`);
    console.error('Perlu in-place UPDATE script, bukan DELETE. Hubungi maintainer.');
    process.exit(1);
  }

  const result = await prisma.erpAccount.deleteMany({ where: { id: { in: oldIds } } });
  console.log(`  deleted ${result.count} row format lama.`);
  console.log('Selesai. Jalankan: npx ts-node prisma/seed-erp-accounts.ts');
}

main()
  .catch((err) => {
    console.error(err);
    process.exit(1);
  })
  .finally(() => prisma.$disconnect());
