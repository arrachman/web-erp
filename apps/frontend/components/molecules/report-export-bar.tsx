'use client';

/**
 * Report export toolbar — three buttons (Excel / PDF / Word) that trigger a
 * download in the chosen format. Reusable across all finance report pages.
 *
 * Atomic tier: Molecule.
 */

import * as React from 'react';
import { Button } from '@/components/ui/button';
import { Icon } from '@/components/ui/icons';
import type { ExportFormat } from '@/lib/api/fin-reports';

export interface ReportExportBarProps {
  onExport: (format: ExportFormat) => void;
  busy?: boolean;
}

const FORMATS: { format: ExportFormat; label: string }[] = [
  { format: 'xlsx', label: 'Excel' },
  { format: 'pdf', label: 'PDF' },
  { format: 'docx', label: 'Word' },
];

export function ReportExportBar({ onExport, busy }: ReportExportBarProps) {
  return (
    <div className="flex items-center gap-1.5">
      {FORMATS.map(({ format, label }) => (
        <Button
          key={format}
          variant="default"
          size="sm"
          disabled={busy}
          onClick={() => onExport(format)}
          title={`Export ${label}`}
        >
          <Icon name="download" size={12} />
          {label}
        </Button>
      ))}
    </div>
  );
}
