import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import {
  ErpMfgJobCostsController,
  ErpMfgPrintJobCostActionsController,
  ErpMfgPrintJobCostsController,
} from './erp-mfg-job-costs.controller';
import { ErpMfgJobCostsService } from './erp-mfg-job-costs.service';

@Module({
  imports: [PrismaModule],
  controllers: [
    ErpMfgPrintJobCostsController,
    ErpMfgPrintJobCostActionsController,
    ErpMfgJobCostsController,
  ],
  providers: [ErpMfgJobCostsService],
  exports: [ErpMfgJobCostsService],
})
export class ErpMfgJobCostsModule {}
