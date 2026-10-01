/**
 * Read-only Warehouse Statistics module. Reuses the moving-average cost engine
 * (ErpInvGlModule) for derived on-hand/avg-cost; no schema changes.
 */

import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpInvGlModule } from '../erp-inv-gl/erp-inv-gl.module';
import { ErpInvStatsController } from './erp-inv-stats.controller';
import { InvStatsService } from './inv-stats.service';

@Module({
  imports: [PrismaModule, ErpInvGlModule],
  controllers: [ErpInvStatsController],
  providers: [InvStatsService],
})
export class ErpInvStatsModule {}
