/**
 * Read-only Warehouse Statistics endpoints. Mirrors erp-inv-reports: guarded by
 * ErpJwtAuthGuard, controller prefix 'erp/inv/stats' (no global API prefix on
 * ERP routes). Every response is `{ success: true, data, ... }`.
 *
 *   GET /erp/inv/stats/top-revenue
 *   GET /erp/inv/stats/best-selling
 *   GET /erp/inv/stats/most-profitable
 *   GET /erp/inv/stats/below-minimum
 *   GET /erp/inv/stats/approvals
 *   GET /erp/inv/stats/kpi
 *
 * Common optional query: dateFrom, dateTo, branchId, warehouseId, limit (top-N).
 */

import { Controller, Get, Query, UseGuards } from '@nestjs/common';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { InvStatsService } from './inv-stats.service';
import { StatsFilters } from './inv-stats.types';

function bigId(v?: string): bigint | null {
  if (v == null || v === '') return null;
  try {
    return BigInt(v);
  } catch {
    return null;
  }
}

function pickFilters(q: Record<string, string | undefined>): StatsFilters {
  const lim = Number(q.limit);
  return {
    dateFrom: q.dateFrom || null,
    dateTo: q.dateTo || null,
    branchId: bigId(q.branchId),
    warehouseId: bigId(q.warehouseId),
    limit: Number.isFinite(lim) && lim > 0 ? Math.min(lim, 200) : 20,
  };
}

@UseGuards(ErpJwtAuthGuard)
@Controller('erp/inv/stats')
export class ErpInvStatsController {
  constructor(private readonly stats: InvStatsService) {}

  @Get('top-revenue')
  async topRevenue(@Query() q: Record<string, string>) {
    return { success: true, data: await this.stats.topRevenue(pickFilters(q)) };
  }

  @Get('best-selling')
  async bestSelling(@Query() q: Record<string, string>) {
    return { success: true, data: await this.stats.bestSelling(pickFilters(q)) };
  }

  @Get('most-profitable')
  async mostProfitable(@Query() q: Record<string, string>) {
    const data = await this.stats.mostProfitable(pickFilters(q));
    return {
      success: true,
      data,
      note: 'COGS sourced from sls_invoice_lines.unit_cost (per-line; null=0).',
    };
  }

  @Get('below-minimum')
  async belowMinimum(@Query() q: Record<string, string>) {
    return {
      success: true,
      data: await this.stats.belowMinimum(pickFilters(q)),
    };
  }

  @Get('approvals')
  async approvals(@Query() q: Record<string, string>) {
    const { data, total } = await this.stats.approvals(pickFilters(q));
    return { success: true, data, total };
  }

  @Get('kpi')
  async kpi(@Query() q: Record<string, string>) {
    return { success: true, data: await this.stats.kpi(pickFilters(q)) };
  }
}
