'use client';

/**
 * Dynamic parameter form (organism) — renders inputs from the registry
 * paramSchema of the selected report (m0_reports.params). Values are
 * plain strings keyed by param name; empty values are omitted by the
 * caller before render.
 *
 * Atomic tier: Organism.
 */

import * as React from 'react';
import { Input } from '@/components/ui/input';
import type { RegistryParamField } from '@/lib/api/report-registry';

export interface ReportParamFormProps {
  schema: RegistryParamField[];
  values: Record<string, string>;
  onChange: (values: Record<string, string>) => void;
  disabled?: boolean;
}

export function ReportParamForm({ schema, values, onChange, disabled }: ReportParamFormProps) {
  if (schema.length === 0) return null;
  return (
    <div className="flex flex-wrap items-end gap-3">
      {schema.map((field) => (
        <label key={field.name} className="flex flex-col gap-1 text-xs text-muted-foreground">
          <span>{field.label}</span>
          <Input
            type={field.type === 'date' ? 'date' : 'text'}
            value={values[field.name] ?? ''}
            disabled={disabled}
            onChange={(e) => onChange({ ...values, [field.name]: e.target.value })}
            className="w-48"
          />
        </label>
      ))}
    </div>
  );
}
