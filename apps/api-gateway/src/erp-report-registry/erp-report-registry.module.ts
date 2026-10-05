import { Module } from '@nestjs/common';
import { ReportEngineModule } from '../erp-report-engine/report-engine.module';
import { ErpReportRegistryController } from './erp-report-registry.controller';
import { ErpReportRegistryService } from './erp-report-registry.service';
import { ReportProviderRegistry } from './report-data-provider';

/**
 * Registry API module. Domain data providers (erp-md-reports, later the
 * fin/inv/pur/sls waves) import this module and self-register into
 * ReportProviderRegistry on init.
 */
@Module({
  imports: [ReportEngineModule],
  controllers: [ErpReportRegistryController],
  providers: [ErpReportRegistryService, ReportProviderRegistry],
  exports: [ReportProviderRegistry, ErpReportRegistryService],
})
export class ErpReportRegistryModule {}
