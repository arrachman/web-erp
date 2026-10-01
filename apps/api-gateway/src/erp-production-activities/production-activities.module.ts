import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpProductionActivitiesController } from './production-activities.controller';
import { ErpProductionActivitiesService } from './production-activities.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpProductionActivitiesController],
  providers: [ErpProductionActivitiesService],
  exports: [ErpProductionActivitiesService],
})
export class ErpProductionActivitiesModule {}
