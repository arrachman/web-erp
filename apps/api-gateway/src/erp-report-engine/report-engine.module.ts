import { Module } from '@nestjs/common';
import { ReportEngineService } from './report-engine.service';

/**
 * Shared report-engine module. Imported by erp-fin/sls/pur/inv-reports modules so
 * a single engine instance backs template-driven PDF rendering across all reports.
 */
@Module({
  providers: [ReportEngineService],
  exports: [ReportEngineService],
})
export class ReportEngineModule {}
