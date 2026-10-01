/**
 * Seed for Initial Setup (M0.CFG) rich pages so the purpose-built pages open
 * with sensible defaults instead of an empty list:
 *   - sys_home_widgets   (ErpHomeWidget)   — dashboard widget layout
 *   - sys_approval_rules (ErpApprovalRule) — approval rule per document type
 *   - sys_bank_accounts  (ErpBankAccount)  — company bank accounts
 *
 * Idempotent: upsert on each model's natural unique key. `update: {}` keeps
 * admin edits on re-seed (only missing defaults created), mirroring the
 * Form Builder / grid seed convention.
 */

import { PrismaClient, Prisma } from '@prisma/client';

// ── Home widgets — default dashboard layout (4-col grid) ────────────────────
interface WidgetDef {
  widgetKey: string;
  title: string;
  description: string;
  sortOrder: number;
  colSpan: number;
}

const HOME_WIDGETS: WidgetDef[] = [
  { widgetKey: 'sales-summary',    title: 'Ringkasan Penjualan',  description: 'Total penjualan & tren periode berjalan',        sortOrder: 1, colSpan: 2 },
  { widgetKey: 'purchasing-summary', title: 'Ringkasan Pembelian', description: 'Total pembelian & PO terbuka',                    sortOrder: 2, colSpan: 2 },
  { widgetKey: 'cash-position',    title: 'Posisi Kas & Bank',    description: 'Saldo kas/bank terkini',                          sortOrder: 3, colSpan: 1 },
  { widgetKey: 'ar-aging',         title: 'Umur Piutang',         description: 'Piutang jatuh tempo per bucket umur',             sortOrder: 4, colSpan: 1 },
  { widgetKey: 'ap-aging',         title: 'Umur Hutang',          description: 'Hutang jatuh tempo per bucket umur',              sortOrder: 5, colSpan: 1 },
  { widgetKey: 'low-stock',        title: 'Stok Menipis',         description: 'Item di bawah titik pesan ulang',                 sortOrder: 6, colSpan: 1 },
  { widgetKey: 'pending-approval', title: 'Menunggu Persetujuan', description: 'Dokumen berstatus Need Approve',                  sortOrder: 7, colSpan: 2 },
];

// ── Approval rules — sensible defaults per document type ────────────────────
interface ApprovalDef {
  documentType: string;
  name: string;
  level: number;
  requiresApproval: boolean;
  minAmount: string | null;
}

const APPROVAL_RULES: ApprovalDef[] = [
  { documentType: 'PUR.PO', name: 'Purchase Order',     level: 1, requiresApproval: true,  minAmount: '10000000.0000' },
  { documentType: 'PUR.PR', name: 'Purchase Request',   level: 1, requiresApproval: true,  minAmount: null },
  { documentType: 'SLS.SO', name: 'Sales Order',        level: 1, requiresApproval: false, minAmount: null },
  { documentType: 'FIN.CD', name: 'Kas Keluar',         level: 1, requiresApproval: true,  minAmount: '5000000.0000' },
  { documentType: 'FIN.GJ', name: 'Jurnal Umum',        level: 1, requiresApproval: true,  minAmount: null },
];

// ── Company bank accounts — starter rows ────────────────────────────────────
interface BankDef {
  code: string;
  name: string;
  bankName: string;
  accountNumber: string;
  accountHolder: string;
  branch: string | null;
  swiftCode: string | null;
  isPrimary: boolean;
}

const BANK_ACCOUNTS: BankDef[] = [
  { code: 'BCA-OPR', name: 'BCA Operasional', bankName: 'Bank BCA',     accountNumber: '1234567890', accountHolder: 'PT Sentient Factory', branch: 'KCP MM2100', swiftCode: 'CENAIDJA', isPrimary: true },
  { code: 'MDR-PAY', name: 'Mandiri Payroll', bankName: 'Bank Mandiri', accountNumber: '9876543210', accountHolder: 'PT Sentient Factory', branch: 'KC Bekasi',  swiftCode: 'BMRIIDJA', isPrimary: false },
];

export async function seedInitialSetup(prisma: PrismaClient): Promise<void> {
  for (const w of HOME_WIDGETS) {
    await prisma.erpHomeWidget.upsert({
      where: { widgetKey: w.widgetKey },
      create: {
        widgetKey: w.widgetKey,
        title: w.title,
        description: w.description,
        enabled: true,
        sortOrder: w.sortOrder,
        colSpan: w.colSpan,
      },
      update: {},
    });
  }

  for (const r of APPROVAL_RULES) {
    await prisma.erpApprovalRule.upsert({
      where: { documentType_level: { documentType: r.documentType, level: r.level } },
      create: {
        documentType: r.documentType,
        name: r.name,
        level: r.level,
        requiresApproval: r.requiresApproval,
        minAmount: r.minAmount ? new Prisma.Decimal(r.minAmount) : null,
        isActive: true,
      },
      update: {},
    });
  }

  for (const b of BANK_ACCOUNTS) {
    await prisma.erpBankAccount.upsert({
      where: { code: b.code },
      create: {
        code: b.code,
        name: b.name,
        bankName: b.bankName,
        accountNumber: b.accountNumber,
        accountHolder: b.accountHolder,
        branch: b.branch,
        swiftCode: b.swiftCode,
        isPrimary: b.isPrimary,
        isActive: true,
      },
      update: {},
    });
  }

  console.log(
    `✓ initial setup — ${HOME_WIDGETS.length} home widgets, ${APPROVAL_RULES.length} approval rules, ${BANK_ACCOUNTS.length} bank accounts`,
  );
}
