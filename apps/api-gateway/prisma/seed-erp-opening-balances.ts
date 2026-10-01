/**
 * One-off: seed opening balances (Saldo Awal CoA) for all leaf balance-sheet
 * accounts, dated 2024-12-31 (before the dummy ledger which starts 2025-01-01).
 *
 * Writes ONE posted OPENING_BALANCE journal (header + lines + balanced GL ledger
 * rows), mirroring JournalPostingService exactly. Each Asset / Liability / Equity
 * leaf account gets a realistic round opening figure on its NORMAL side. The 7
 * cash/bank accounts get figures that exceed their worst running drawdown from the
 * dummy transactions, so their running balance never goes negative in reports.
 * Saldo Laba Ditahan (3103.01.001) is the computed balancing plug (Σdebit=Σcredit).
 *
 * Revenue/Expense accounts are intentionally excluded — they start each fiscal
 * year at zero.
 *
 * Run: npx ts-node prisma/seed-erp-opening-balances.ts
 * Idempotent: deletes a prior run (same docNumber) before re-inserting.
 */
import { PrismaClient, Prisma } from '@prisma/client';

const prisma = new PrismaClient();

const DOC_NUMBER = 'JV-OB-20241231';
const ENTRY_DATE = new Date(Date.UTC(2024, 11, 31)); // 2024-12-31
const FISCAL_PERIOD_ID = 60n; // Des 2024
const BRANCH_HQ = 1n;
const CURRENCY_IDR = 1n;
const ACTOR = 1n;
const SOURCE = 'JOURNAL';
const SOURCE_DOC_TYPE = 'fin_journal_entries';

const PLUG_ACCOUNT_CODE = '3103.01.001'; // Saldo Laba Ditahan — absorbs the balancing figure

/**
 * Opening figure per account code (IDR). Placed on the account's NORMAL side.
 * Cash/bank figures exceed the worst running drawdown from the dummy ledger:
 *   Kas Besar 923M · Kas Kecil 264M · BCA 199M · Mandiri-IDR 583M · BNI 98M ·
 *   BRI 98M · CIMB 571M. Codes with amount 0 (or absent) are skipped.
 * PLUG_ACCOUNT_CODE is deliberately omitted — its figure is computed to balance.
 */
const OPENING: Record<string, number> = {
  // ── Kas & Bank (must exceed worst running drawdown) ──
  '1101.01.001': 1_000_000_000, // Kas Besar
  '1102.01.001': 300_000_000, // Kas Kecil
  '1110.01.001': 250_000_000, // Bank BCA - Giro IDR
  '1111.01.001': 650_000_000, // Bank Mandiri - Giro IDR
  '1112.01.001': 150_000_000, // Bank BNI - Giro IDR
  '1113.01.001': 150_000_000, // Bank BRI - Giro IDR
  '1114.01.001': 650_000_000, // Bank CIMB Niaga - Giro IDR
  '1115.01.001': 200_000_000, // Bank Mandiri - Giro USD
  // ── Piutang & aset lancar lain ──
  '1120.01.001': 1_500_000_000, // Piutang Dagang
  '1121.01.001': 75_000_000, // Cadangan Kerugian Piutang (contra, kredit)
  '1122.01.001': 50_000_000, // Piutang Karyawan
  '1123.01.001': 120_000_000, // Uang Muka Pembelian
  '1124.01.001': 30_000_000, // Piutang Lainnya
  '1125.01.001': 25_000_000, // Giro Masuk Dalam Proses Kliring
  // ── Persediaan ──
  '1130.01.001': 800_000_000, // Bahan Baku
  '1131.01.001': 150_000_000, // Bahan Pembantu
  '1132.01.001': 250_000_000, // Barang Dalam Proses
  '1133.01.001': 600_000_000, // Barang Jadi
  '1134.01.001': 100_000_000, // Perlengkapan / Suku Cadang
  // ── Pajak dibayar dimuka ──
  '1140.01.001': 90_000_000, // PPN Masukan
  '1141.01.001': 15_000_000, // PPh 22 Dibayar Dimuka
  '1142.01.001': 12_000_000, // PPh 23 Dibayar Dimuka
  '1143.01.001': 20_000_000, // PPh 25 Dibayar Dimuka
  '1144.01.001': 10_000_000, // Fiskal Tahun Berjalan
  // ── Biaya dibayar dimuka ──
  '1150.01.001': 60_000_000, // Sewa Dibayar Dimuka
  '1151.01.001': 40_000_000, // Asuransi Dibayar Dimuka
  '1152.01.001': 15_000_000, // Biaya Lainnya Dibayar Dimuka
  // ── Aset tetap & akumulasi penyusutan (contra, kredit) ──
  '1201.01.001': 5_000_000_000, // Tanah
  '1202.01.001': 3_000_000_000, // Bangunan dan Prasarana
  '1203.01.001': 600_000_000, // Akum. Penyusutan Bangunan
  '1210.01.001': 4_000_000_000, // Mesin Produksi
  '1211.01.001': 1_200_000_000, // Akum. Penyusutan Mesin
  '1212.01.001': 800_000_000, // Peralatan Pabrik
  '1213.01.001': 300_000_000, // Akum. Penyusutan Peralatan Pabrik
  '1220.01.001': 1_200_000_000, // Kendaraan
  '1221.01.001': 500_000_000, // Akum. Penyusutan Kendaraan
  '1230.01.001': 400_000_000, // Peralatan Kantor
  '1231.01.001': 150_000_000, // Akum. Penyusutan Peralatan Kantor
  '1240.01.001': 250_000_000, // Inventaris Kantor
  '1241.01.001': 100_000_000, // Akum. Penyusutan Inventaris
  // ── Aset tak berwujud & amortisasi (contra, kredit) ──
  '1301.01.001': 200_000_000, // Lisensi Software
  '1302.01.001': 80_000_000, // Akum. Amortisasi Lisensi
  '1310.01.001': 150_000_000, // Hak Merek dan Paten
  '1311.01.001': 50_000_000, // Akum. Amortisasi Hak Merek
  '1320.01.001': 60_000_000, // Biaya Pendirian / Organisasi
  '1321.01.001': 30_000_000, // Akum. Amortisasi Biaya Pendirian
  // ── Investasi ──
  '1401.01.001': 1_000_000_000, // Investasi Saham Entitas Anak
  '1402.01.001': 500_000_000, // Investasi Saham Entitas Asosiasi
  '1403.01.001': 300_000_000, // Investasi Obligasi
  // ── Liabilitas (kredit) ──
  '2101.01.001': 1_200_000_000, // Hutang Dagang
  '2102.01.001': 800_000_000, // Hutang Bank Jangka Pendek
  '2103.01.001': 150_000_000, // Hutang Gaji Karyawan
  '2104.01.001': 100_000_000, // Biaya Yang Masih Harus Dibayar
  '2105.01.001': 200_000_000, // Uang Muka Penjualan
  '2106.01.001': 50_000_000, // Giro Keluar Dalam Proses
  '2110.01.001': 120_000_000, // PPN Keluaran
  '2111.01.001': 40_000_000, // PPh 21 Terutang
  '2112.01.001': 15_000_000, // PPh 23 Terutang
  '2113.01.001': 20_000_000, // PPh 25 Terutang
  '2114.01.001': 80_000_000, // PPh Badan Terutang
  '2115.01.001': 25_000_000, // BPJS Ketenagakerjaan Terutang
  '2116.01.001': 20_000_000, // BPJS Kesehatan Terutang
  '2120.01.001': 100_000_000, // Dividen Yang Harus Dibayar
  '2121.01.001': 300_000_000, // Hutang Jangka Panjang - Jatuh Tempo
  '2201.01.001': 2_000_000_000, // Hutang Bank Jangka Panjang
  '2202.01.001': 1_000_000_000, // Hutang Obligasi
  '2203.01.001': 250_000_000, // Hutang Sewa Pembiayaan (Leasing)
  '2210.01.001': 150_000_000, // Liabilitas Pajak Tangguhan
  '2220.01.001': 400_000_000, // Cadangan Imbalan Pasca-Kerja
  // ── Ekuitas (kredit) — 3103 = plug (dihitung), 3104 = 0 (tahun berjalan) ──
  '3101.01.001': 10_000_000_000, // Modal Saham Disetor
  '3102.01.001': 2_000_000_000, // Tambahan Modal Disetor (Agio)
  '3110.01.001': 500_000_000, // Cadangan Umum
  '3111.01.001': 200_000_000, // Cadangan Khusus
  '3120.01.001': 50_000_000, // Selisih Kurs Penjabaran
};

interface LeafAccount {
  id: bigint;
  code: string;
  name: string;
  normalBalance: 'DEBIT' | 'CREDIT';
}

async function main() {
  console.log('→ Loading leaf balance-sheet accounts…');
  const accounts = await prisma.erpAccount.findMany({
    where: {
      type: { in: ['ASSET', 'LIABILITY', 'EQUITY'] },
      isActive: true,
      deletedAt: null,
      children: { none: {} },
    },
    select: { id: true, code: true, name: true, normalBalance: true },
    orderBy: { code: 'asc' },
  });
  const leaves = accounts as unknown as LeafAccount[];
  const plug = leaves.find((a) => a.code === PLUG_ACCOUNT_CODE);
  if (!plug) throw new Error(`Plug account ${PLUG_ACCOUNT_CODE} not found / not a leaf.`);

  // ── Build lines (everything except the plug, which is computed last) ──
  const zero = new Prisma.Decimal(0);
  type Line = { accountId: bigint; debit: Prisma.Decimal; credit: Prisma.Decimal; notes: string };
  const lines: Line[] = [];
  let totalDebit = zero;
  let totalCredit = zero;

  for (const acc of leaves) {
    if (acc.code === PLUG_ACCOUNT_CODE) continue;
    const amt = OPENING[acc.code];
    if (!amt || amt <= 0) continue; // skip un-specified / zero accounts (e.g. 3104)
    const value = new Prisma.Decimal(amt);
    const onDebit = acc.normalBalance === 'DEBIT';
    lines.push({
      accountId: acc.id,
      debit: onDebit ? value : zero,
      credit: onDebit ? zero : value,
      notes: `Saldo awal ${acc.name}`,
    });
    if (onDebit) totalDebit = totalDebit.add(value); else totalCredit = totalCredit.add(value);
  }

  // ── Balancing plug (Saldo Laba Ditahan, credit-normal) ──
  const plugAmount = totalDebit.sub(totalCredit);
  if (plugAmount.lessThanOrEqualTo(0)) {
    throw new Error(`Plug must be a positive credit; got ${plugAmount}. Adjust the OPENING figures.`);
  }
  lines.push({
    accountId: plug.id,
    debit: zero,
    credit: plugAmount,
    notes: `Saldo awal ${plug.name} (penyeimbang)`,
  });
  totalCredit = totalCredit.add(plugAmount);

  console.log(`  ${lines.length} lines · Σdebit=${totalDebit} · Σcredit=${totalCredit}`);
  if (!totalDebit.equals(totalCredit)) throw new Error('Not balanced — abort.');

  // ── Idempotent cleanup of a prior run ──
  console.log('→ Removing any prior run…');
  const prior = await prisma.erpFinJournalEntry.findUnique({
    where: { docNumber: DOC_NUMBER },
    select: { id: true },
  });
  if (prior) {
    await prisma.erpFinLedgerEntry.deleteMany({
      where: { sourceDocType: SOURCE_DOC_TYPE, sourceId: prior.id },
    });
    await prisma.erpFinJournalEntry.delete({ where: { id: prior.id } });
    console.log(`  removed prior journal #${prior.id} + its ledger rows`);
  }

  // ── Insert header + lines + ledger (one transaction) ──
  console.log('→ Inserting opening-balance journal…');
  const now = new Date();
  await prisma.$transaction(async (tx) => {
    const entry = await tx.erpFinJournalEntry.create({
      data: {
        docNumber: DOC_NUMBER,
        autoNumber: DOC_NUMBER,
        journalType: 'OPENING_BALANCE',
        branchId: BRANCH_HQ,
        source: 'OPENING_BALANCE_SEED',
        entryDate: ENTRY_DATE,
        fiscalPeriodId: FISCAL_PERIOD_ID,
        description: 'Saldo Awal CoA per 31 Desember 2024',
        currencyId: CURRENCY_IDR,
        exchangeRate: new Prisma.Decimal(1),
        status: 'POSTED',
        previousStatus: 'APPROVED',
        postingStatus: 'POSTED',
        postedAt: now,
        postedById: ACTOR,
        createdById: ACTOR,
        updatedById: ACTOR,
        lines: {
          create: lines.map((l, i) => ({
            accountId: l.accountId,
            currencyId: CURRENCY_IDR,
            exchangeRate: new Prisma.Decimal(1),
            debit: l.debit,
            credit: l.credit,
            notes: l.notes,
            lineNo: i + 1,
          })),
        },
      },
      select: { id: true },
    });

    const ledger: Prisma.ErpFinLedgerEntryCreateManyInput[] = lines.map((l, i) => ({
      branchId: BRANCH_HQ,
      source: SOURCE,
      sourceDocType: SOURCE_DOC_TYPE,
      sourceId: entry.id,
      docNumber: DOC_NUMBER,
      entryDate: ENTRY_DATE,
      fiscalPeriodId: FISCAL_PERIOD_ID,
      accountId: l.accountId,
      description: l.notes,
      notes: l.notes,
      currencyId: CURRENCY_IDR,
      exchangeRate: new Prisma.Decimal(1),
      debit: l.debit,
      credit: l.credit,
      reconciliationStatus: 'UNRECONCILED',
      isOpeningBalance: true,
      status: 'POSTED',
      postingStatus: 'POSTED',
      postedAt: now,
      createdById: ACTOR,
      updatedById: ACTOR,
      lineNo: i + 1,
    }));
    await tx.erpFinLedgerEntry.createMany({ data: ledger });

    console.log(`✓ Journal #${entry.id} (${DOC_NUMBER}) + ${ledger.length} ledger rows.`);
  });
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(() => prisma.$disconnect());
