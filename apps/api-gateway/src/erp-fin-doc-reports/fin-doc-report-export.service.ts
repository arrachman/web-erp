/** Reuses the generic report exporters for Finance document datasets. */

import { Injectable } from '@nestjs/common';
import { ReportEngineService } from '../erp-report-engine/report-engine.service';
import { ReportExportService as InvReportExportService } from '../erp-inv-reports/report-export.service';
import { ReportDataset, ReportFormat } from './report-types';

@Injectable()
export class FinDocReportExportService {
  constructor(
    private readonly engine: ReportEngineService,
  ) {}

  async render(dataset: ReportDataset, format: ReportFormat) {
    const exporter = new InvReportExportService(this.engine, 'fin');
    return exporter.render(dataset, format);
  }
}
