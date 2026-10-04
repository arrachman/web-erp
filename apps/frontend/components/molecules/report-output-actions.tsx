'use client';

/**
 * Client-side report output actions shared by the inventory and purchasing
 * report toolbars. Server-format exports remain owned by their toolbars.
 *
 * Atomic tier: Molecule.
 */

import { Button } from '@/components/ui/button';
import { Icon } from '@/components/ui/icons';
import { exportRowsToCsv } from '@/lib/export-csv';

export interface ReportOutputColumn {
  key: string;
  header: string;
}

export interface ReportOutputData {
  key: string;
  columns: ReportOutputColumn[];
  rows: Record<string, unknown>[];
}

export interface ReportOutputActionsProps {
  dataset: ReportOutputData | null;
  disabled?: boolean;
}

function csvValue(value: unknown): string | number | null {
  if (typeof value === 'number' || typeof value === 'string') return value;
  if (value == null) return null;
  return String(value);
}

export function ReportOutputActions({ dataset, disabled }: ReportOutputActionsProps) {
  const canExportCsv = !disabled && Boolean(dataset);

  const handleCsv = () => {
    if (!dataset) return;
    exportRowsToCsv(
      `${dataset.key}.csv`,
      dataset.rows,
      dataset.columns.map((column) => ({
        header: column.header,
        value: (row) => csvValue(row[column.key]),
      })),
    );
  };

  return (
    <>
      <Button
        variant="default"
        className="cursor-pointer"
        disabled={disabled}
        onClick={() => window.print()}
        title="Cetak laporan"
      >
        <Icon name="download" size={12} />
        Cetak
      </Button>
      <Button
        variant="default"
        className="cursor-pointer"
        disabled={!canExportCsv}
        onClick={handleCsv}
        title="Ekspor CSV"
      >
        <Icon name="download" size={12} />
        CSV
      </Button>
    </>
  );
}
