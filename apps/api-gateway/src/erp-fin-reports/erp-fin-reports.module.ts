import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ReportEngineModule } from '../erp-report-engine/report-engine.module';
import { ControlReconciliationService } from './control-reconciliation.service';
import { ErpFinReportsExtService } from './erp-fin-reports-ext.service';
import { ErpFinReportsController } from './erp-fin-reports.controller';
import { ErpFinReportsService } from './erp-fin-reports.service';
import { ReportExportService } from './report-export.service';

@Module({
  imports: [PrismaModule, ReportEngineModule],
  controllers: [ErpFinReportsController],
  providers: [ControlReconciliationService, ErpFinReportsService, ErpFinReportsExtService, ReportExportService],
  exports: [ErpFinReportsService, ErpFinReportsExtService],
})
export class ErpFinReportsModule {}
