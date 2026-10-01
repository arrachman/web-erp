import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { SlsDeliveryReportPostingService } from './sls-delivery-report-posting.service';
import { ErpSlsDeliveryReportsController } from './erp-sls-delivery-reports.controller';
import { ErpSlsDeliveryReportsService } from './erp-sls-delivery-reports.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpSlsDeliveryReportsController],
  providers: [ErpSlsDeliveryReportsService, SlsDeliveryReportPostingService],
  exports: [ErpSlsDeliveryReportsService],
})
export class ErpSlsDeliveryReportsModule {}
