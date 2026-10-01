import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpCostCentersController } from './erp-cost-centers.controller';
import { ErpCostCentersService } from './erp-cost-centers.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpCostCentersController],
  providers: [ErpCostCentersService],
  exports: [ErpCostCentersService],
})
export class ErpCostCentersModule {}
