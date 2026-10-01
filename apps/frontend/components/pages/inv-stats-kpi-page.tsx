'use client';

/**
 * Warehouse Statistics — KPI cards page.
 *
 * Renders 6 metric cards from `GET /inv/stats/kpi`. Money via formatRupiah,
 * counts via formatNumber(x, 0). No hardcoded colors — token classes only.
 *
 * Atomic tier: Page.
 */

import * as React from 'react';
import { Card } from '@/components/ui/card';
import { StatPageShell, useStatData } from '@/components/organisms/stat-page-shell';
import { getKpi, type StatsKpi } from '@/lib/api/inv-stats';
import { formatNumber, formatRupiah } from '@/lib/format';

interface Metric {
  label: string;
  value: string;
}

function buildMetrics(kpi: StatsKpi): Metric[] {
  return [
    { label: 'Total Item', value: formatNumber(kpi.totalItems, 0) },
    { label: 'Item di Bawah Minimum', value: formatNumber(kpi.belowMinCount, 0) },
    { label: 'Menunggu Persetujuan', value: formatNumber(kpi.pendingApprovals, 0) },
    { label: 'Pendapatan Periode', value: formatRupiah(kpi.periodRevenue) },
    { label: 'Qty Terjual Periode', value: formatNumber(kpi.periodQtySold, 0) },
    { label: 'Nilai Stok', value: formatRupiah(kpi.stockValue) },
  ];
}

export function InvStatsKpiPage() {
  const { data, loading, error } = useStatData<StatsKpi>(() => getKpi());
  const metrics = data ? buildMetrics(data) : [];

  return (
    <StatPageShell
      title="KPI Warehouse"
      code="inv/stats/kpi"
      loading={loading}
      error={error}
      empty={!data}
    >
      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
        {metrics.map((m) => (
          <Card key={m.label} className="p-4">
            <div className="text-xs text-muted-foreground">{m.label}</div>
            <div className="mt-1 text-xl font-semibold tabular-nums text-foreground">
              {m.value}
            </div>
          </Card>
        ))}
      </div>
    </StatPageShell>
  );
}
