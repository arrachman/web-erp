/**
 * Resolves a report's REAL column definitions by `reportKey` — used to materialize an
 * auto-template into editable bands matching the actual report. Sales/Purchasing/
 * Inventory expose static columns from their registry (no DB); Finance computes columns
 * inside its builders, so they run with year-to-date defaults (a design-time call).
 */
import { BadRequestException, Injectable } from '@nestjs/common';
import { ErpFinReportsService } from '../erp-fin-reports/erp-fin-reports.service';
import { ErpFinReportsExtService } from '../erp-fin-reports/erp-fin-reports-ext.service';
import { finReportColumns } from '../erp-fin-reports/report-engine-adapter';
import type { ReportDocument } from '../erp-fin-reports/report-types';
import { SlsReportsService } from '../erp-sls-reports/sls-reports.service';
import { PurReportsService } from '../erp-pur-reports/pur-reports.service';
import { InvReportsService } from '../erp-inv-reports/inv-reports.service';
import { columnsToDefs, type EngineDatasetColumn } from '../erp-report-engine/dataset-adapter';
import type { TableColumnDef } from '../erp-report-engine/template-builder';

@Injectable()
export class ReportColumnsResolver {
  constructor(
    private readonly fin: ErpFinReportsService,
    private readonly finExt: ErpFinReportsExtService,
    private readonly sls: SlsReportsService,
    private readonly pur: PurReportsService,
    private readonly inv: InvReportsService,
  ) {}

  async resolve(reportKey: string): Promise<TableColumnDef[]> {
    const dot = reportKey.indexOf('.');
    const module = dot === -1 ? reportKey : reportKey.slice(0, dot);
    const key = dot === -1 ? '' : reportKey.slice(dot + 1);
    switch (module) {
      case 'sls':
        return columnsToDefs(this.sls.getColumns(key) as EngineDatasetColumn[]);
      case 'pur':
        return columnsToDefs(this.pur.getColumns(key) as EngineDatasetColumn[]);
      case 'inv':
        return columnsToDefs(this.inv.getColumns(key) as EngineDatasetColumn[]);
      case 'fin':
        return finReportColumns(await this.buildFin(key));
      default:
        throw new BadRequestException(`reportKey tidak dikenal: ${reportKey}`);
    }
  }

  private buildFin(key: string): Promise<ReportDocument> {
    const now = new Date();
    const to = now.toISOString().slice(0, 10);
    const from = `${now.getFullYear()}-01-01`;
    const asOf = to;
    switch (key) {
      case 'trial-balance':
        return this.fin.buildTrialBalance(from, to);
      case 'income-statement':
        return this.fin.buildIncomeStatement(from, to);
      case 'balance-sheet':
        return this.fin.buildBalanceSheet(asOf);
      case 'movement-balance':
        return this.fin.buildMovementBalance(from, to);
      case 'equity-changes':
        return this.fin.buildEquityChanges(from, to);
      case 'general-ledger':
        return this.fin.buildGeneralLedger(from, to);
      case 'cash-flow':
        return this.finExt.buildCashFlow(from, to);
      case 'daily-cash-bank':
        return this.finExt.buildDailyCashBank(from, to);
      case 'ar-card':
        return this.finExt.buildArCard(from, to);
      case 'ar-aging':
        return this.finExt.buildArAging(asOf);
      case 'ap-card':
        return this.finExt.buildApCard(from, to);
      case 'ap-aging':
        return this.finExt.buildApAging(asOf);
      case 'giro-maturity':
        return this.finExt.buildGiroMaturity(from, to);
      case 'budget-realization':
        return this.finExt.buildBudgetRealization(from, to);
      default:
        throw new BadRequestException(`Laporan finance tidak dikenal: ${key}`);
    }
  }
}
