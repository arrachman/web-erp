/**
 * Sales document-list resolvers — orchestrates parts A (quotations…delivery-reports)
 * and B (invoices…opening-ar-balance).
 */

import { PrismaService } from '../prisma/prisma.service';
import { ReportDef } from './report-types';
import { buildDocReportsA } from './report-resolvers-docs-a';
import { buildDocReportsB } from './report-resolvers-docs-b';
import { buildSalesFactoryReports } from './report-factory-configs';

export function buildDocReports(prisma: PrismaService): ReportDef[] {
  return [
    ...buildDocReportsA(prisma),
    ...buildDocReportsB(prisma),
    ...buildSalesFactoryReports(prisma),
  ];
}
