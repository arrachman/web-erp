import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpMfgWorkOrdersModule } from '../erp-mfg-work-orders/erp-mfg-work-orders.module';
import { ErpMfgPrintJobsController } from './erp-mfg-print-jobs.controller';
import { ErpMfgPrintJobsService } from './erp-mfg-print-jobs.service';

@Module({
  imports: [PrismaModule, ErpMfgWorkOrdersModule],
  controllers: [ErpMfgPrintJobsController],
  providers: [ErpMfgPrintJobsService],
  exports: [ErpMfgPrintJobsService],
})
export class ErpMfgPrintJobsModule {}
