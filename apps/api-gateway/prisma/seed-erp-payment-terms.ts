/**
 * Seed real payment terms into md_payment_terms.
 * Wipes existing dummy data, inserts 100 real industry-standard terms.
 * Run: npm run db:seed:payment-terms  (or npx ts-node prisma/seed-erp-payment-terms.ts)
 */
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

type Term = {
  code: string;
  name: string;
  netDays: number;
  discountPercent1?: number;
  discountDays1?: number;
  discountPercent2?: number;
  discountDays2?: number;
  penaltyPercent?: number;
  penaltyPeriod?: string;
  legacyCode?: string;
  isActive?: boolean;
};

const PAYMENT_TERMS: Term[] = [
  // ── Group A: Immediate / Cash (10) ──────────────────────────────────────
  { code: 'COD',        name: 'Cash on Delivery',           netDays: 0,  legacyCode: 'COD' },
  { code: 'CBD',        name: 'Cash Before Delivery',       netDays: 0,  legacyCode: 'CBD' },
  { code: 'CIA',        name: 'Cash in Advance',            netDays: 0,  legacyCode: 'CIA' },
  { code: 'TUNAI',      name: 'Tunai',                      netDays: 0,  legacyCode: 'TUN' },
  { code: 'PIF',        name: 'Paid in Full',               netDays: 0,  legacyCode: 'PIF' },
  { code: 'CND',        name: 'Cash Next Delivery',         netDays: 0,  legacyCode: 'CND' },
  { code: 'T/T',        name: 'Telegraphic Transfer',       netDays: 0,  legacyCode: 'TT'  },
  { code: 'D/P',        name: 'Documents Against Payment',  netDays: 0,  legacyCode: 'DP'  },
  { code: 'NOTA',       name: 'Bayar per Nota',             netDays: 0                     },
  { code: 'ADVANCE-50', name: 'Uang Muka 50%',             netDays: 0                     },

  // ── Group B: Net terms — no discount (18) ───────────────────────────────
  { code: 'NET-7',   name: 'Net 7 Hari',    netDays: 7,   legacyCode: '1-1'  },
  { code: 'NET-10',  name: 'Net 10 Hari',   netDays: 10,  legacyCode: '1-2'  },
  { code: 'NET-14',  name: 'Net 14 Hari',   netDays: 14,  legacyCode: '1-3'  },
  { code: 'NET-15',  name: 'Net 15 Hari',   netDays: 15,  legacyCode: '1-4'  },
  { code: 'NET-21',  name: 'Net 21 Hari',   netDays: 21,  legacyCode: '1-5'  },
  { code: 'NET-30',  name: 'Net 30 Hari',   netDays: 30,  legacyCode: '1-6'  },
  { code: 'NET-45',  name: 'Net 45 Hari',   netDays: 45,  legacyCode: '1-7'  },
  { code: 'NET-60',  name: 'Net 60 Hari',   netDays: 60,  legacyCode: '1-8'  },
  { code: 'NET-75',  name: 'Net 75 Hari',   netDays: 75,  legacyCode: '1-9'  },
  { code: 'NET-90',  name: 'Net 90 Hari',   netDays: 90,  legacyCode: '1-10' },
  { code: 'NET-120', name: 'Net 120 Hari',  netDays: 120, legacyCode: '1-11' },
  { code: 'NET-150', name: 'Net 150 Hari',  netDays: 150, legacyCode: '1-12' },
  { code: 'NET-180', name: 'Net 180 Hari',  netDays: 180, legacyCode: '1-13' },
  { code: 'NET-270', name: 'Net 270 Hari',  netDays: 270                     },
  { code: 'NET-360', name: 'Net 360 Hari',  netDays: 360                     },
  { code: 'EOM-7',   name: 'Akhir Bulan +7 Hari',  netDays: 7                },
  { code: 'EOM-14',  name: 'Akhir Bulan +14 Hari', netDays: 14               },
  { code: 'EOM-30',  name: 'Akhir Bulan +30 Hari', netDays: 30               },

  // ── Group C: Discount 2% terms (10) ─────────────────────────────────────
  { code: '2/7-NET-30',   name: '2% disc 7 Hari, Net 30',   netDays: 30,  discountPercent1: 2, discountDays1: 7  },
  { code: '2/7-NET-45',   name: '2% disc 7 Hari, Net 45',   netDays: 45,  discountPercent1: 2, discountDays1: 7  },
  { code: '2/10-NET-30',  name: '2% disc 10 Hari, Net 30',  netDays: 30,  discountPercent1: 2, discountDays1: 10 },
  { code: '2/10-NET-45',  name: '2% disc 10 Hari, Net 45',  netDays: 45,  discountPercent1: 2, discountDays1: 10 },
  { code: '2/10-NET-60',  name: '2% disc 10 Hari, Net 60',  netDays: 60,  discountPercent1: 2, discountDays1: 10 },
  { code: '2/10-NET-90',  name: '2% disc 10 Hari, Net 90',  netDays: 90,  discountPercent1: 2, discountDays1: 10 },
  { code: '2/10-NET-120', name: '2% disc 10 Hari, Net 120', netDays: 120, discountPercent1: 2, discountDays1: 10 },
  { code: '2/14-NET-30',  name: '2% disc 14 Hari, Net 30',  netDays: 30,  discountPercent1: 2, discountDays1: 14 },
  { code: '2/14-NET-60',  name: '2% disc 14 Hari, Net 60',  netDays: 60,  discountPercent1: 2, discountDays1: 14 },
  { code: '2/21-NET-60',  name: '2% disc 21 Hari, Net 60',  netDays: 60,  discountPercent1: 2, discountDays1: 21 },

  // ── Group D: Discount 1% terms (10) ─────────────────────────────────────
  { code: '1/7-NET-30',   name: '1% disc 7 Hari, Net 30',   netDays: 30,  discountPercent1: 1, discountDays1: 7  },
  { code: '1/7-NET-45',   name: '1% disc 7 Hari, Net 45',   netDays: 45,  discountPercent1: 1, discountDays1: 7  },
  { code: '1/7-NET-60',   name: '1% disc 7 Hari, Net 60',   netDays: 60,  discountPercent1: 1, discountDays1: 7  },
  { code: '1/10-NET-30',  name: '1% disc 10 Hari, Net 30',  netDays: 30,  discountPercent1: 1, discountDays1: 10 },
  { code: '1/10-NET-45',  name: '1% disc 10 Hari, Net 45',  netDays: 45,  discountPercent1: 1, discountDays1: 10 },
  { code: '1/10-NET-60',  name: '1% disc 10 Hari, Net 60',  netDays: 60,  discountPercent1: 1, discountDays1: 10 },
  { code: '1/10-NET-90',  name: '1% disc 10 Hari, Net 90',  netDays: 90,  discountPercent1: 1, discountDays1: 10 },
  { code: '1/14-NET-30',  name: '1% disc 14 Hari, Net 30',  netDays: 30,  discountPercent1: 1, discountDays1: 14 },
  { code: '1/14-NET-60',  name: '1% disc 14 Hari, Net 60',  netDays: 60,  discountPercent1: 1, discountDays1: 14 },
  { code: '1/21-NET-60',  name: '1% disc 21 Hari, Net 60',  netDays: 60,  discountPercent1: 1, discountDays1: 21 },

  // ── Group E: Discount 3% & 5% terms (8) ─────────────────────────────────
  { code: '3/7-NET-30',   name: '3% disc 7 Hari, Net 30',   netDays: 30,  discountPercent1: 3, discountDays1: 7  },
  { code: '3/10-NET-30',  name: '3% disc 10 Hari, Net 30',  netDays: 30,  discountPercent1: 3, discountDays1: 10 },
  { code: '3/10-NET-60',  name: '3% disc 10 Hari, Net 60',  netDays: 60,  discountPercent1: 3, discountDays1: 10 },
  { code: '3/14-NET-30',  name: '3% disc 14 Hari, Net 30',  netDays: 30,  discountPercent1: 3, discountDays1: 14 },
  { code: '5/7-NET-30',   name: '5% disc 7 Hari, Net 30',   netDays: 30,  discountPercent1: 5, discountDays1: 7  },
  { code: '5/10-NET-30',  name: '5% disc 10 Hari, Net 30',  netDays: 30,  discountPercent1: 5, discountDays1: 10 },
  { code: '5/10-NET-60',  name: '5% disc 10 Hari, Net 60',  netDays: 60,  discountPercent1: 5, discountDays1: 10 },
  { code: '5/14-NET-30',  name: '5% disc 14 Hari, Net 30',  netDays: 30,  discountPercent1: 5, discountDays1: 14 },

  // ── Group F: Double discount terms (8) ──────────────────────────────────
  { code: '2/7-1/14-NET-30',   name: '2%/7hr, 1%/14hr, Net 30',  netDays: 30, discountPercent1: 2, discountDays1: 7,  discountPercent2: 1, discountDays2: 14 },
  { code: '2/7-1/14-NET-60',   name: '2%/7hr, 1%/14hr, Net 60',  netDays: 60, discountPercent1: 2, discountDays1: 7,  discountPercent2: 1, discountDays2: 14 },
  { code: '2/10-1/20-NET-60',  name: '2%/10hr, 1%/20hr, Net 60', netDays: 60, discountPercent1: 2, discountDays1: 10, discountPercent2: 1, discountDays2: 20 },
  { code: '3/7-1/14-NET-30',   name: '3%/7hr, 1%/14hr, Net 30',  netDays: 30, discountPercent1: 3, discountDays1: 7,  discountPercent2: 1, discountDays2: 14 },
  { code: '5/7-2/14-NET-60',   name: '5%/7hr, 2%/14hr, Net 60',  netDays: 60, discountPercent1: 5, discountDays1: 7,  discountPercent2: 2, discountDays2: 14 },
  { code: '1/7-0.5/14-NET-30', name: '1%/7hr, 0.5%/14hr, Net 30',netDays: 30, discountPercent1: 1, discountDays1: 7,  discountPercent2: 0.5, discountDays2: 14 },
  { code: '3/7-2/14-NET-45',   name: '3%/7hr, 2%/14hr, Net 45',  netDays: 45, discountPercent1: 3, discountDays1: 7,  discountPercent2: 2, discountDays2: 14 },
  { code: '2/14-1/21-NET-60',  name: '2%/14hr, 1%/21hr, Net 60', netDays: 60, discountPercent1: 2, discountDays1: 14, discountPercent2: 1, discountDays2: 21 },

  // ── Group G: Penalty terms (10) ──────────────────────────────────────────
  { code: 'NET-7-P2',    name: 'Net 7, Denda 2%/bln',   netDays: 7,   penaltyPercent: 2,   penaltyPeriod: 'monthly' },
  { code: 'NET-14-P1',   name: 'Net 14, Denda 1%/bln',  netDays: 14,  penaltyPercent: 1,   penaltyPeriod: 'monthly' },
  { code: 'NET-14-P2',   name: 'Net 14, Denda 2%/bln',  netDays: 14,  penaltyPercent: 2,   penaltyPeriod: 'monthly' },
  { code: 'NET-30-P1',   name: 'Net 30, Denda 1%/bln',  netDays: 30,  penaltyPercent: 1,   penaltyPeriod: 'monthly' },
  { code: 'NET-30-P2',   name: 'Net 30, Denda 2%/bln',  netDays: 30,  penaltyPercent: 2,   penaltyPeriod: 'monthly' },
  { code: 'NET-45-P1',   name: 'Net 45, Denda 1%/bln',  netDays: 45,  penaltyPercent: 1,   penaltyPeriod: 'monthly' },
  { code: 'NET-60-P1',   name: 'Net 60, Denda 1%/bln',  netDays: 60,  penaltyPercent: 1,   penaltyPeriod: 'monthly' },
  { code: 'NET-60-P2',   name: 'Net 60, Denda 2%/bln',  netDays: 60,  penaltyPercent: 2,   penaltyPeriod: 'monthly' },
  { code: 'NET-90-P1',   name: 'Net 90, Denda 1%/bln',  netDays: 90,  penaltyPercent: 1,   penaltyPeriod: 'monthly' },
  { code: 'NET-120-P1',  name: 'Net 120, Denda 1%/bln', netDays: 120, penaltyPercent: 1,   penaltyPeriod: 'monthly' },

  // ── Group H: Combined discount + penalty (6) ─────────────────────────────
  { code: '2/10-NET-30-P1',   name: '2%/10hr, Net 30, Denda 1%/bln',   netDays: 30,  discountPercent1: 2, discountDays1: 10, penaltyPercent: 1, penaltyPeriod: 'monthly' },
  { code: '2/10-NET-60-P1',   name: '2%/10hr, Net 60, Denda 1%/bln',   netDays: 60,  discountPercent1: 2, discountDays1: 10, penaltyPercent: 1, penaltyPeriod: 'monthly' },
  { code: '1/10-NET-30-P1',   name: '1%/10hr, Net 30, Denda 1%/bln',   netDays: 30,  discountPercent1: 1, discountDays1: 10, penaltyPercent: 1, penaltyPeriod: 'monthly' },
  { code: '3/10-NET-30-P2',   name: '3%/10hr, Net 30, Denda 2%/bln',   netDays: 30,  discountPercent1: 3, discountDays1: 10, penaltyPercent: 2, penaltyPeriod: 'monthly' },
  { code: '2/14-NET-60-P1',   name: '2%/14hr, Net 60, Denda 1%/bln',   netDays: 60,  discountPercent1: 2, discountDays1: 14, penaltyPercent: 1, penaltyPeriod: 'monthly' },
  { code: '1/14-NET-60-P1.5', name: '1%/14hr, Net 60, Denda 1.5%/bln', netDays: 60,  discountPercent1: 1, discountDays1: 14, penaltyPercent: 1.5, penaltyPeriod: 'monthly' },

  // ── Group I: International trade terms (6) ───────────────────────────────
  { code: 'LC-30',   name: 'Letter of Credit 30 Hari',  netDays: 30  },
  { code: 'LC-60',   name: 'Letter of Credit 60 Hari',  netDays: 60  },
  { code: 'LC-90',   name: 'Letter of Credit 90 Hari',  netDays: 90  },
  { code: 'LC-120',  name: 'Letter of Credit 120 Hari', netDays: 120 },
  { code: 'DA-30',   name: 'Documents Against Acceptance 30 Hari', netDays: 30 },
  { code: 'DA-60',   name: 'Documents Against Acceptance 60 Hari', netDays: 60 },

  // ── Group J: Special / Indonesian (14) ──────────────────────────────────
  { code: 'KONSINYASI',    name: 'Konsinyasi (Titip Jual)', netDays: 60  },
  { code: 'DP50-NET-30',   name: 'DP 50%, Sisa Net 30',    netDays: 30  },
  { code: 'DP30-NET-60',   name: 'DP 30%, Sisa Net 60',    netDays: 60  },
  { code: 'DP20-NET-90',   name: 'DP 20%, Sisa Net 90',    netDays: 90  },
  { code: 'INST-2X-60',    name: 'Cicilan 2x, 60 Hari',    netDays: 60  },
  { code: 'INST-3X-90',    name: 'Cicilan 3x, 90 Hari',    netDays: 90  },
  { code: 'INST-4X-120',   name: 'Cicilan 4x, 120 Hari',   netDays: 120 },
  { code: 'INST-6X-180',   name: 'Cicilan 6x, 180 Hari',   netDays: 180 },
  { code: 'KREDIT-7',      name: 'Kredit 7 Hari',           netDays: 7,  legacyCode: 'K7'  },
  { code: 'KREDIT-30',     name: 'Kredit 30 Hari',          netDays: 30, legacyCode: 'K30' },
  { code: 'KREDIT-60',     name: 'Kredit 60 Hari',          netDays: 60, legacyCode: 'K60' },
  { code: 'KREDIT-90',     name: 'Kredit 90 Hari',          netDays: 90, legacyCode: 'K90' },
  { code: 'NET-30-P1.5',   name: 'Net 30, Denda 1.5%/bln', netDays: 30, penaltyPercent: 1.5, penaltyPeriod: 'monthly' },
  { code: 'NET-60-P1.5',   name: 'Net 60, Denda 1.5%/bln', netDays: 60, penaltyPercent: 1.5, penaltyPeriod: 'monthly' },
];

async function main() {
  console.log('Deleting dummy payment terms…');
  const deleted = await prisma.erpPaymentTerm.deleteMany({});
  console.log(`  deleted ${deleted.count} rows`);

  console.log(`Inserting ${PAYMENT_TERMS.length} real payment terms…`);
  const result = await prisma.erpPaymentTerm.createMany({
    data: PAYMENT_TERMS.map(t => ({
      code:             t.code,
      name:             t.name,
      netDays:          t.netDays,
      discountPercent1: t.discountPercent1 ?? null,
      discountDays1:    t.discountDays1    ?? null,
      discountPercent2: t.discountPercent2 ?? null,
      discountDays2:    t.discountDays2    ?? null,
      penaltyPercent:   t.penaltyPercent   ?? null,
      penaltyPeriod:    t.penaltyPeriod    ?? null,
      legacyCode:       t.legacyCode       ?? null,
      isActive:         t.isActive         ?? true,
    })),
    skipDuplicates: false,
  });
  console.log(`  inserted ${result.count} rows`);

  const total = await prisma.erpPaymentTerm.count();
  console.log(`Done. md_payment_terms now has ${total} rows.`);
}

main()
  .catch(err => { console.error(err); process.exit(1); })
  .finally(() => prisma.$disconnect());
