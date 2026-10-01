import { BadRequestException, Injectable } from '@nestjs/common';
import { ReportDocument, ReportFormat } from './report-types';
import { renderXlsx } from './report-export.xlsx';
import { renderPdf } from './report-export.pdf';
import { renderDoc } from './report-export.doc';
import { ReportEngineService } from '../erp-report-engine/report-engine.service';
import { finReportColumns, finReportContext } from './report-engine-adapter';

export interface RenderedReport {
  buffer: Buffer;
  contentType: string;
  filename: string;
}

@Injectable()
export class ReportExportService {
  constructor(private readonly engine: ReportEngineService) {}

  async render(doc: ReportDocument, format: ReportFormat): Promise<RenderedReport> {
    switch (format) {
      case 'xlsx': {
        const buffer = await renderXlsx(doc);
        return {
          buffer,
          contentType:
            'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
          filename: `${doc.key}.xlsx`,
        };
      }
      case 'pdf': {
        // Template-driven render when a Report Designer template is bound to this
        // report (`fin.<key>`); otherwise fall back to the built-in pdfkit layout.
        const templated = await this.engine.renderReport(
          `fin.${doc.key}`,
          finReportColumns(doc),
          finReportContext(doc),
        );
        const buffer = templated ?? (await renderPdf(doc));
        return { buffer, contentType: 'application/pdf', filename: `${doc.key}.pdf` };
      }
      case 'docx': {
        const buffer = renderDoc(doc);
        return { buffer, contentType: 'application/msword', filename: `${doc.key}.doc` };
      }
      default:
        throw new BadRequestException(`Unsupported export format: ${format}`);
    }
  }
}
