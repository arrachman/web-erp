import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ReportEngineModule } from '../erp-report-engine/report-engine.module';
import { ErpFinReportsModule } from '../erp-fin-reports/erp-fin-reports.module';
import { ErpSlsReportsModule } from '../erp-sls-reports/erp-sls-reports.module';
import { ErpPurReportsModule } from '../erp-pur-reports/erp-pur-reports.module';
import { ErpInvReportsModule } from '../erp-inv-reports/erp-inv-reports.module';
import { ErpReportsController } from './erp-reports.controller';
import { ErpReportRegistryModule } from '../erp-report-registry/erp-report-registry.module';
import { ErpMdReportsModule } from '../erp-md-reports/erp-md-reports.module';
import { ErpReportsService } from './erp-reports.service';
import { ReportColumnsResolver } from './report-columns-resolver';

@Module({
  imports: [
    PrismaModule,
    ReportEngineModule,
    ErpFinReportsModule,
    ErpSlsReportsModule,
    ErpPurReportsModule,
    ErpInvReportsModule,
    ErpReportRegistryModule,
    ErpMdReportsModule,
  ],
  controllers: [ErpReportsController],
  providers: [ErpReportsService, ReportColumnsResolver],
})
export class ErpReportsModule {}
