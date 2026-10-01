/**
 * Sales document-list resolvers — orchestrates parts A (quotations…delivery-reports)
 * and B (invoices…opening-ar-balance).
 */

import { PrismaService } from '../prisma/prisma.service';
import { ReportDef } from './report-types';
import { buildDocReportsA } from './report-resolvers-docs-a';
import { buildDocReportsB } from './report-resolvers-docs-b';

export function buildDocReports(prisma: PrismaService): ReportDef[] {
  return [...buildDocReportsA(prisma), ...buildDocReportsB(prisma)];
}
