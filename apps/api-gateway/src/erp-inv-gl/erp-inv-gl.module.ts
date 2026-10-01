import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { InvMovingAverageCostService } from './inv-moving-average-cost.service';

/**
 * Foundation module for inventory GL valuation. Exposes the derived
 * moving-average cost service for other inventory modules to import. Pure
 * helpers live in inv-gl-posting.helpers.ts (no provider needed).
 */
@Module({
  imports: [PrismaModule],
  providers: [InvMovingAverageCostService],
  exports: [InvMovingAverageCostService],
})
export class ErpInvGlModule {}
