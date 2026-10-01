/**
 * Purchasing (M4) report resolver registry. Combines part-1 and part-2 resolvers
 * into a single ordered list consumed by PurReportsService.
 */

import { PrismaService } from '../prisma/prisma.service';
import { ReportDef } from './report-types';
import { buildPurReportsPart1 } from './pur-report-resolvers-1';
import { buildPurReportsPart2 } from './pur-report-resolvers-2';

type Deps = { prisma: PrismaService };

export function buildPurReports(deps: Deps): ReportDef[] {
  return [...buildPurReportsPart1(deps), ...buildPurReportsPart2(deps)];
}
