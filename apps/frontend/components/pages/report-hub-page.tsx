'use client';

/**
 * Report hub — the landing page for a module's "Reports" menu group.
 *
 * Fixes the four blank hub routes (`/finance/reports`, `/warehouse/reports`,
 * `/purchasing/reports`, `/sales/reports`): those `sys_menus` rows are GROUPs
 * with `path = NULL`, so nothing rendered there before.
 *
 * One config-driven renderer serves all four modules — same discipline as the
 * document registers (`lib/registers/register-config.ts`): do NOT fork this
 * page per module. Cards are grouped by the backend `group` id and navigate
 * within the shell via `onNavigate` (never <a href>, which would drop the tab).
 *
 * Atomic tier: Page.
 */

import * as React from 'react';
import { Card } from '@/components/ui/card';
import { StatPageShell, useStatData } from '@/components/organisms/stat-page-shell';
import {
  getReportCatalog,
  type ReportCatalogEntry,
  type ReportModule,
} from '@/lib/api/report-catalog';

/** Human labels for the backend group ids (inv/pur/sls + finance). */
const GROUP_LABELS: Record<string, string> = {
  transaction: 'Laporan Transaksi',
  document: 'Laporan Dokumen',
  stock: 'Laporan Stok',
  item: 'Laporan Item',
  analytics: 'Analisis',
  statement: 'Laporan Keuangan Pokok',
  subledger: 'Buku Pembantu',
  control: 'Laporan Kontrol & Analisis',
};

function groupLabel(group: string): string {
  return GROUP_LABELS[group] ?? 'Laporan Lain';
}

export interface ReportHubPageProps {
  module: ReportModule;
  title: string;
  /** Short code shown next to the title, e.g. `warehouse/reports`. */
  code: string;
  onNavigate: (route: string) => void;
}

interface Section {
  group: string;
  label: string;
  entries: ReportCatalogEntry[];
}

/** Bucket the flat catalog into sections, preserving the sorted order. */
function buildSections(entries: ReportCatalogEntry[]): Section[] {
  const byGroup = new Map<string, ReportCatalogEntry[]>();
  for (const entry of entries) {
    const list = byGroup.get(entry.group);
    if (list) list.push(entry);
    else byGroup.set(entry.group, [entry]);
  }
  return [...byGroup.entries()].map(([group, list]) => ({
    group,
    label: groupLabel(group),
    entries: list,
  }));
}

export function ReportHubPage({
  module,
  title,
  code,
  onNavigate,
}: ReportHubPageProps) {
  const { data, loading, error } = useStatData<ReportCatalogEntry[]>(() =>
    getReportCatalog(module),
  );
  const sections = React.useMemo(() => buildSections(data ?? []), [data]);

  return (
    <StatPageShell
      title={title}
      code={code}
      loading={loading}
      error={error}
      empty={!loading && !error && sections.length === 0}
      emptyMessage="Belum ada laporan terdaftar untuk modul ini"
    >
      <div className="flex flex-col gap-5">
        {sections.map((section) => (
          <section key={section.group}>
            <h2 className="mb-2 text-xs font-semibold uppercase tracking-wide text-muted-foreground">
              {section.label}
              <span className="ml-1.5 font-normal normal-case tracking-normal">
                ({section.entries.length})
              </span>
            </h2>
            <div className="grid grid-cols-1 gap-2 sm:grid-cols-2 lg:grid-cols-3">
              {section.entries.map((entry) => (
                <ReportCard
                  key={entry.key}
                  entry={entry}
                  onOpen={() => onNavigate(entry.route)}
                />
              ))}
            </div>
          </section>
        ))}
      </div>
    </StatPageShell>
  );
}

interface ReportCardProps {
  entry: ReportCatalogEntry;
  onOpen: () => void;
}

function ReportCard({ entry, onOpen }: ReportCardProps) {
  return (
    <Card
      role="button"
      tabIndex={0}
      onClick={onOpen}
      onKeyDown={(e: React.KeyboardEvent) => {
        if (e.key === 'Enter' || e.key === ' ') {
          e.preventDefault();
          onOpen();
        }
      }}
      className="cursor-pointer p-3 transition-colors hover:bg-muted focus:outline-none focus-visible:ring-1 focus-visible:ring-primary"
      title={`Buka ${entry.title}`}
    >
      <div className="text-sm font-medium text-foreground">{entry.title}</div>
      <div className="mt-0.5 truncate text-[11px] text-muted-foreground">
        {entry.key}
      </div>
    </Card>
  );
}
