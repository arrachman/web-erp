'use client';

/**
 * Report type combo box (organism) — one select for ALL report types of
 * a module, fed by the Registry API and ordered by `urutan` (design D8:
 * one Reports menu per module, no menu per report). Converted/verified
 * status is shown so users see translation progress honestly.
 *
 * Atomic tier: Organism.
 */

import * as React from 'react';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import type { RegistryReportItem } from '@/lib/api/report-registry';

export interface ReportTypeSelectProps {
  items: RegistryReportItem[];
  value: string;
  onChange: (code: string) => void;
  disabled?: boolean;
}

const STATUS_BADGE: Record<string, string> = {
  VERIFIED: '✓',
  CONVERTED: '●',
  PENDING: '○',
};

export function ReportTypeSelect({ items, value, onChange, disabled }: ReportTypeSelectProps) {
  return (
    <Select value={value} onValueChange={onChange} disabled={disabled}>
      <SelectTrigger className="w-full max-w-xl">
        <SelectValue placeholder="Pilih jenis laporan…" />
      </SelectTrigger>
      <SelectContent>
        {items.map((item) => (
          <SelectItem key={item.code} value={item.code}>
            {STATUS_BADGE[item.translationStatus] ?? '●'} {item.title}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}
