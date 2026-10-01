/**
 * Sales (M1) reports module. Registers the generic view + export controller
 * with service providers. No external module imports needed — PrismaModule is
 * global.
 */

import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ReportEngineModule } from '../erp-report-engine/report-engine.module';
import { ErpSlsReportsController } from './erp-sls-reports.controller';
import { SlsReportsService } from './sls-reports.service';
import { ReportExportService } from './report-export.service';

@Module({
  imports: [PrismaModule, ReportEngineModule],
  controllers: [ErpSlsReportsController],
  providers: [SlsReportsService, ReportExportService],
  exports: [SlsReportsService],
})
export class ErpSlsReportsModule {}
