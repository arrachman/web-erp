/**
 * Renders any ReportDataset to a downloadable file: Excel (exceljs), PDF
 * (pdfmake — pure JS, no Chromium), or Word (docx). Column `type` drives
 * number/date/money formatting + alignment, identical to the on-screen table.
 */

import { BadRequestException, Injectable } from '@nestjs/common';

/** Hard cap for sync in-memory export (xlsx/pdf/docx). */
const MAX_EXPORT_ROWS = 5_000;
import { ReportDataset, ReportFormat } from './report-types';
import { buildFilename } from './report-export.format';
import { renderXlsx } from './report-export.xlsx';
import { renderPdf } from './report-export.pdf';
import { renderDocx } from './report-export.docx';
import { ReportEngineService } from '../erp-report-engine/report-engine.service';
import { datasetColumns, datasetContext } from '../erp-report-engine/dataset-adapter';

export interface RenderedReport {
  buffer: Buffer;
  contentType: string;
  filename: string;
}

const CONTENT_TYPE: Record<ReportFormat, string> = {
  xlsx: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
  pdf: 'application/pdf',
  docx: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
};

@Injectable()
export class ReportExportService {
  constructor(private readonly engine: ReportEngineService) {}

  async render(dataset: ReportDataset, format: ReportFormat): Promise<RenderedReport> {
    if (dataset.rows.length > MAX_EXPORT_ROWS) {
      throw new BadRequestException(
        `Export dibatasi ${MAX_EXPORT_ROWS} baris (dataset: ${dataset.rows.length}). ` +
          `Persempit filter atau ekspor bertahap.`,
      );
    }
    let buffer: Buffer;
    switch (format) {
      case 'xlsx':
        buffer = await renderXlsx(dataset);
        break;
      case 'pdf': {
        // Template-driven render when a Report Designer template is bound
        // (`sls.<key>`, or the `sls.__default` module template); else pdfmake.
        const templated = await this.engine.renderReport(
          `sls.${dataset.key}`,
          datasetColumns(dataset),
          datasetContext(dataset),
        );
        buffer = templated ?? (await renderPdf(dataset));
        break;
      }
      case 'docx':
        buffer = await renderDocx(dataset);
        break;
      default:
        throw new BadRequestException(`Format export tidak didukung: ${format}`);
    }

    return {
      buffer,
      contentType: CONTENT_TYPE[format],
      filename: buildFilename(dataset.key, dataset.generatedAt, format),
    };
  }
}
