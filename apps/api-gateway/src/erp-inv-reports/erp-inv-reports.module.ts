/**
 * Warehouse (M3) reports module. Imports the moving-average engine for stock
 * aggregation reports. Registers the generic view + export controller.
 */

import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ReportEngineModule } from '../erp-report-engine/report-engine.module';
import { ErpInvGlModule } from '../erp-inv-gl/erp-inv-gl.module';
import { ErpReportRegistryModule } from '../erp-report-registry/erp-report-registry.module';
import { ErpInvReportsController } from './erp-inv-reports.controller';
import { ErpInvMrtReportsService } from './erp-inv-mrt-reports.service';
import { InvReportsService } from './inv-reports.service';
import { ReportExportService } from './report-export.service';

@Module({
  imports: [PrismaModule, ErpInvGlModule, ReportEngineModule, ErpReportRegistryModule],
  controllers: [ErpInvReportsController],
  providers: [InvReportsService, ReportExportService, ErpInvMrtReportsService],
  exports: [InvReportsService],
})
export class ErpInvReportsModule {}
