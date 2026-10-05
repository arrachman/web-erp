import { Module } from '@nestjs/common';
import { ErpReportRegistryModule } from '../erp-report-registry/erp-report-registry.module';
import { ErpMdReportsService } from './erp-md-reports.service';

/**
 * Master Data report dataset builders (Wave G1). Self-registers into
 * the report registry as the `md.*` data provider on module init.
 */
@Module({
  imports: [ErpReportRegistryModule],
  providers: [ErpMdReportsService],
})
export class ErpMdReportsModule {}
