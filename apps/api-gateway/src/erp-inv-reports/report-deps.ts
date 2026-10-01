/**
 * Shared dependency bundle passed to every resolver builder. Resolvers query
 * Prisma read-only and reuse the derived moving-average engine for valuation.
 */

import { PrismaService } from '../prisma/prisma.service';
import { InvMovingAverageCostService } from '../erp-inv-gl/inv-moving-average-cost.service';

export interface ReportDeps {
  prisma: PrismaService;
  movingAvg: InvMovingAverageCostService;
}
