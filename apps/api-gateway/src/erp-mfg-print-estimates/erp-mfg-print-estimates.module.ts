import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpSlsQuotationsModule } from '../erp-sls-quotations/erp-sls-quotations.module';
import { ErpMfgPrintEstimatesController } from './erp-mfg-print-estimates.controller';
import { ErpMfgPrintEstimatesService } from './erp-mfg-print-estimates.service';

@Module({
  imports: [PrismaModule, ErpSlsQuotationsModule],
  controllers: [ErpMfgPrintEstimatesController],
  providers: [ErpMfgPrintEstimatesService],
  exports: [ErpMfgPrintEstimatesService],
})
export class ErpMfgPrintEstimatesModule {}
