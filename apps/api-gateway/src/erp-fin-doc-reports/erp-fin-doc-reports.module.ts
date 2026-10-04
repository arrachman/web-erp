import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ReportEngineModule } from '../erp-report-engine/report-engine.module';
import { ErpFinDocReportsController } from './erp-fin-doc-reports.controller';
import { FinDocReportExportService } from './fin-doc-report-export.service';
import { FinDocReportsService } from './fin-doc-reports.service';

@Module({
  imports: [PrismaModule, ReportEngineModule],
  controllers: [ErpFinDocReportsController],
  providers: [FinDocReportsService, FinDocReportExportService],
})
export class ErpFinDocReportsModule {}
