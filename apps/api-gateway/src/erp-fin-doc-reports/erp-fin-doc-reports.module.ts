import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ReportEngineModule } from '../erp-report-engine/report-engine.module';
import { ErpReportRegistryModule } from '../erp-report-registry/erp-report-registry.module';
import { ErpFinDocReportsController } from './erp-fin-doc-reports.controller';
import { FinDocReportExportService } from './fin-doc-report-export.service';
import { FinDocReportsService } from './fin-doc-reports.service';
import { ErpFinDocMrtReportsService } from './erp-fin-doc-mrt-reports.service';

@Module({
  imports: [PrismaModule, ReportEngineModule, ErpReportRegistryModule],
  controllers: [ErpFinDocReportsController],
  providers: [FinDocReportsService, FinDocReportExportService, ErpFinDocMrtReportsService],
})
export class ErpFinDocReportsModule {}
