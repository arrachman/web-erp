/**
 * Purchasing (M4) reports module. Registers the generic view + export controller.
 */

import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ReportEngineModule } from '../erp-report-engine/report-engine.module';
import { ErpPurReportsController } from './erp-pur-reports.controller';
import { PurReportsService } from './pur-reports.service';
import { ReportExportService } from './report-export.service';

@Module({
  imports: [PrismaModule, ReportEngineModule],
  controllers: [ErpPurReportsController],
  providers: [PurReportsService, ReportExportService],
  exports: [PurReportsService],
})
export class ErpPurReportsModule {}
