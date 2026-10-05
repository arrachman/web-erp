/**
 * Mark imported reports as VERIFIED after E2E verification (Wave G1).
 * This is the documented status-transition path — import never sets
 * VERIFIED, and this tool only promotes CONVERTED rows, listing what it
 * changed.
 *
 * Usage (from apps/api-gateway):
 *   npx ts-node tools/report-import/mark-verified.ts --codes RPT-M1-ITEM,RPT-M1-COA
 */

import * as fs from 'fs';
import * as path from 'path';
import { PrismaClient } from '@prisma/client';

function loadDatabaseUrl(): void {
  if (process.env.DATABASE_URL) return;
  const envPath = path.resolve(__dirname, '../../.env');
  if (fs.existsSync(envPath)) {
    for (const line of fs.readFileSync(envPath, 'utf8').split('\n')) {
      const m = /^DATABASE_URL=(.*)$/.exec(line.trim());
      if (m) process.env.DATABASE_URL = m[1].replace(/^"|"$/g, '');
    }
  }
}

async function main(): Promise<void> {
  const idx = process.argv.indexOf('--codes');
  const codes = idx >= 0 ? (process.argv[idx + 1] ?? '').split(',').map((c) => c.trim()).filter(Boolean) : [];
  if (!codes.length) {
    console.error('Pakai --codes RPT-M1-ITEM,RPT-M1-COA,…');
    process.exit(2);
  }
  loadDatabaseUrl();
  const prisma = new PrismaClient();
  try {
    for (const code of codes) {
      const row = await prisma.erpM0Report.findUnique({ where: { code } });
      if (!row) {
        console.log(`TIDAK ADA: ${code}`);
        continue;
      }
      if (row.translationStatus !== 'CONVERTED') {
        console.log(`LEWATI ${code}: status=${row.translationStatus} (hanya CONVERTED yang bisa VERIFIED)`);
        continue;
      }
      if (!row.rptTemplateId || !row.reportKey) {
        console.log(`LEWATI ${code}: belum terhubung ke template`);
        continue;
      }
      await prisma.erpM0Report.update({
        where: { code },
        data: { translationStatus: 'VERIFIED' },
      });
      console.log(`VERIFIED ${code} (${row.reportKey})`);
    }
  } finally {
    await prisma.$disconnect();
  }
}

void main();
