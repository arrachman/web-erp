'use client';

/**
 * Optional visual summary for generic report datasets. The report contract is
 * additive: reports without chart series render exactly as before.
 *
 * Atomic tier: Organism.
 */

import { BarChart } from '@/components/ui/bar-chart';
import { DonutChart } from '@/components/ui/donut-chart';

export interface ReportChartData {
  kind: 'bar' | 'donut';
  title: string;
  labels: string[];
  values: number[];
}

export interface ReportChartsProps {
  charts?: ReportChartData[];
}

const DONUT_COLORS = [
  'var(--primary)',
  'var(--success)',
  'var(--info)',
  'var(--warn)',
  'var(--danger)',
];

function hasUsableSeries(chart: ReportChartData): boolean {
  return chart.labels.length > 0 && chart.labels.length === chart.values.length;
}

function ChartCard({ chart }: { chart: ReportChartData }) {
  const labels = chart.labels.map((label, index) => ({
    label,
    value: chart.values[index],
  }));

  return (
    <section className="rounded-lg border border-border bg-card p-3" aria-label={chart.title}>
      <h2 className="text-sm font-medium text-foreground">{chart.title}</h2>
      <div className="mt-3 flex items-center gap-4">
        {chart.kind === 'bar' ? (
          <BarChart data={chart.values} className="text-muted-foreground" />
        ) : (
          <DonutChart
            slices={labels.map((item, index) => ({
              v: item.value,
              color: DONUT_COLORS[index % DONUT_COLORS.length],
              label: item.label,
            }))}
            className="shrink-0"
          />
        )}
        <ul className="min-w-0 space-y-1 text-xs text-muted-foreground">
          {labels.map((item) => (
            <li key={item.label} className="flex justify-between gap-4">
              <span className="truncate">{item.label}</span>
              <span className="font-mono tabular-nums text-foreground">{item.value}</span>
            </li>
          ))}
        </ul>
      </div>
    </section>
  );
}

export function ReportCharts({ charts }: ReportChartsProps) {
  const usableCharts = charts?.filter(hasUsableSeries) ?? [];
  if (usableCharts.length === 0) return null;

  return (
    <div className="grid grid-cols-1 gap-3 lg:grid-cols-2">
      {usableCharts.map((chart) => (
        <ChartCard key={chart.title} chart={chart} />
      ))}
    </div>
  );
}
