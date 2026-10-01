/**
 * sync-finance-menus.ts — idempotent reconciler for the M2 (Finance) menu tree.
 *
 * Why standalone: `seedErp()` also `deleteMany`s warehouses/locations (would wipe
 * dummy data) and the upsert-by-code seed never prunes stale menu rows. This script
 * touches ONLY `sys_menus` under M2 — prune obsolete codes, upsert the corrected
 * Transactions + Reports tree. Safe to re-run. See web-erp DECISIONS.md
 * "§ Finance (M2) menu parity (2026-05-31)".
 *
 * Run: npx ts-node prisma/sync-finance-menus.ts
 */
import { PrismaClient, ErpMenuType } from '@prisma/client';

const prisma = new PrismaClient();

// Menu codes superseded by the 2026-05-31 parity fix (+ older /keuangan/ duplicates).
// FK adm_role_menus.menu_id = ON DELETE CASCADE → role-maps removed automatically.
const STALE_CODES = [
  'M2.TX.CASHBANK-TRANSFER', // → M2.TX.OPENING-BALANCE
  'M2.TX.RECEIPT-MEMO',      // → M2.TX.BANK-RECEIPT
  'M2.TX.SEND-MEMO',         // → M2.TX.BANK-PAYMENT
  'M2.TX.AP-PAYMENT',        // old /keuangan/ duplicate → BANK-PAYMENT
  'M2.TX.AR-RECEIPT',        // old /keuangan/ duplicate → BANK-RECEIPT
  'M2.TX.GIRO',              // old /keuangan/ duplicate → RECEIPT/SEND-GIRO
  'M2.TX.JOURNAL',           // old /keuangan/ duplicate → GENERAL-JOURNAL
];

const TX_ITEMS = [
  { code: 'M2.TX.CASH-RECEIPT',       title: 'Cash Receipt',          path: '/finance/cash-receipts',          legacyCode: 'CR'  },
  { code: 'M2.TX.CASH-DISBURSEMENT',  title: 'Cash Disbursement',     path: '/finance/cash-disbursements',     legacyCode: 'CD'  },
  { code: 'M2.TX.BANK-RECEIPT',       title: 'Bank Receipt',          path: '/finance/bank-receipts',          legacyCode: 'RM'  },
  { code: 'M2.TX.BANK-PAYMENT',       title: 'Bank Payment',          path: '/finance/bank-payments',          legacyCode: 'SM'  },
  { code: 'M2.TX.BANK-DISBURSEMENT',  title: 'Bank Disbursement',     path: '/finance/bank-disbursements',     legacyCode: 'BD'  },
  { code: 'M2.TX.GENERAL-JOURNAL',    title: 'General Journal',       path: '/finance/general-journals',       legacyCode: 'GJ'  },
  { code: 'M2.TX.ADJUSTMENT-JOURNAL', title: 'Adjustment Journal',    path: '/finance/adjustment-journals',    legacyCode: 'AJ'  },
  { code: 'M2.TX.MEMORIAL-JOURNAL',   title: 'Memorial Journal',      path: '/finance/memorial-journals',      legacyCode: 'JM'  },
  { code: 'M2.TX.REVALUATION',        title: 'FX Revaluation',        path: '/finance/revaluations',           legacyCode: 'RV'  },
  { code: 'M2.TX.RECEIPT-GIRO',       title: 'Receipt Giro',          path: '/finance/receipt-giros',          legacyCode: 'RG'  },
  { code: 'M2.TX.SEND-GIRO',          title: 'Send Giro',             path: '/finance/send-giros',             legacyCode: 'SG'  },
  { code: 'M2.TX.RECEIPT-GIRO-CLR',   title: 'Receipt Giro Clearing', path: '/finance/receipt-giro-clearings', legacyCode: 'RGC' },
  { code: 'M2.TX.SEND-GIRO-CLR',      title: 'Send Giro Clearing',    path: '/finance/send-giro-clearings',    legacyCode: 'SGC' },
  { code: 'M2.TX.OPENING-BALANCE',    title: 'Opening Balance (CoA)', path: '/finance/opening-balances',       legacyCode: 'CB'  },
];

const RPT_ITEMS = [
  { code: 'M2.RPT.LEDGER',             title: 'General Ledger',        path: '/finance/ledger',             legacyCode: '2-41'  },
  { code: 'M2.RPT.TRIAL-BALANCE',      title: 'Trial Balance',         path: '/finance/trial-balance',      legacyCode: '2-42'  },
  { code: 'M2.RPT.BALANCE-SHEET',      title: 'Balance Sheet',         path: '/finance/balance-sheet',      legacyCode: '2-45'  },
  { code: 'M2.RPT.INCOME-STATEMENT',   title: 'Income Statement',      path: '/finance/income-statement',   legacyCode: '2-46'  },
  { code: 'M2.RPT.CASH-FLOW',          title: 'Cash Flow',             path: '/finance/cash-flow',          legacyCode: '2-104' },
  { code: 'M2.RPT.DAILY-CASH-BANK',    title: 'Daily Cash & Bank',     path: '/finance/daily-cash-bank',    legacyCode: '2-43'  },
  { code: 'M2.RPT.AR-CARD',            title: 'AR Card',               path: '/finance/ar-card',            legacyCode: '2-47'  },
  { code: 'M2.RPT.AR-AGING',           title: 'AR Aging',              path: '/finance/ar-aging',           legacyCode: '2-92'  },
  { code: 'M2.RPT.AP-CARD',            title: 'AP Card',               path: '/finance/ap-card',            legacyCode: '2-50'  },
  { code: 'M2.RPT.AP-AGING',           title: 'AP Aging',              path: '/finance/ap-aging',           legacyCode: '2-93'  },
  { code: 'M2.RPT.GIRO-MATURITY',      title: 'Giro Maturity',         path: '/finance/giro-maturity',      legacyCode: '2-9'   },
  { code: 'M2.RPT.BUDGET-REALIZATION', title: 'Budget vs Realization', path: '/finance/budget-realization', legacyCode: '2-135' },
];

async function upsertMenu(p: {
  code: string; title: string; type: ErpMenuType;
  parentId?: bigint; path?: string; icon?: string; sortOrder: number; legacyCode?: string;
}) {
  return prisma.erpMenu.upsert({
    where: { code: p.code },
    create: { code: p.code, title: p.title, type: p.type, parentId: p.parentId, path: p.path, icon: p.icon, sortOrder: p.sortOrder, legacyCode: p.legacyCode, isActive: true },
    update: { title: p.title, type: p.type, parentId: p.parentId ?? null, path: p.path ?? null, icon: p.icon ?? null, sortOrder: p.sortOrder, legacyCode: p.legacyCode ?? undefined },
  });
}

async function upsertItems(items: typeof TX_ITEMS, parentId: bigint) {
  for (const [i, item] of items.entries()) {
    await upsertMenu({ ...item, type: ErpMenuType.ITEM, parentId, sortOrder: i + 1 });
  }
}

async function main() {
  const pruned = await prisma.erpMenu.deleteMany({ where: { code: { in: STALE_CODES } } });
  console.log(`Pruned ${pruned.count} stale M2 menu row(s).`);

  const m2 = await upsertMenu({ code: 'M2', title: 'Finance & Accounting', icon: 'calculator', type: ErpMenuType.MODULE, sortOrder: 3, legacyCode: '2-1' });
  const txGrp = await upsertMenu({ code: 'M2.TX', title: 'Transactions', type: ErpMenuType.GROUP, parentId: m2.id, sortOrder: 1 });
  await upsertItems(TX_ITEMS, txGrp.id);
  const rptGrp = await upsertMenu({ code: 'M2.RPT', title: 'Reports', type: ErpMenuType.GROUP, parentId: m2.id, sortOrder: 2 });
  await upsertItems(RPT_ITEMS, rptGrp.id);

  console.log(`Synced M2: ${TX_ITEMS.length} transactions + ${RPT_ITEMS.length} reports.`);
}

main()
  .catch((e) => { console.error(e); process.exit(1); })
  .finally(() => prisma.$disconnect());
