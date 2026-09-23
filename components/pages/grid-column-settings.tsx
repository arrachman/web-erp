'use client';

/**
 * Gear dialog for a Kustomisasi-Grid column — shown for EVERY column type
 * (mirrors the Form Builder's FieldSettingsPopover). Holds the generic knobs
 * (placeholder, default value) plus, for lookup-style columns, the lookup
 * source/sort/filter config (LookupSortFilterFields). Read-only is intentionally
 * omitted here — the grid already exposes a per-column "Edit" (isEditable) flag.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Input } from '@/components/ui/input';
import {
  Dialog, DialogTrigger, DialogContent, DialogHeader, DialogTitle, DialogBody,
} from '@/components/ui/dialog';
import { LookupSortFilterFields } from './form-builder-lookup-config';
import { GridColumnDefaultEditor, effectiveColumnType } from './grid-column-default-editor';
import type { ErpGridColumn } from '@/lib/api/transaction-grids';

/** True when a column carries any field-setting or lookup config (for gear highlight). */
export function hasGridColumnConfig(col: ErpGridColumn): boolean {
  return (
    !!col.placeholder ||
    !!col.defaultValue ||
    !!col.lookupDefaultSort ||
    (!!col.lookupDefaultFilter && Object.keys(col.lookupDefaultFilter).length > 0)
  );
}

export function GridColumnSettings({ col, onPatch }: {
  col: ErpGridColumn;
  onPatch: (patch: Partial<ErpGridColumn>) => void;
}) {
  const isLookup = effectiveColumnType(col) === 'lookup';
  const configured = hasGridColumnConfig(col);

  return (
    <Dialog>
      <DialogTrigger asChild>
        <button
          type="button"
          className={`iconbtn ${configured ? 'text-primary' : 'text-muted-foreground'}`}
          title="Konfigurasi kolom (placeholder, nilai default, lookup)"
          onClick={(e) => e.stopPropagation()}
        >
          <Icon name="gear" size={12} />
        </button>
      </DialogTrigger>
      <DialogContent className="w-[520px] max-w-[95vw] max-h-[90vh] flex flex-col">
        <DialogHeader>
          <DialogTitle>Konfigurasi Kolom — {col.headerText || col.dataField}</DialogTitle>
        </DialogHeader>
        <DialogBody className="flex flex-col gap-5 flex-1 max-h-[calc(90vh-64px)]">

          {/* Placeholder */}
          <div className="flex flex-col gap-1">
            <span className="text-xs text-muted-foreground">Placeholder</span>
            <Input
              className="h-8 text-xs"
              placeholder="Teks petunjuk saat sel kosong…"
              value={col.placeholder ?? ''}
              onChange={(e) => onPatch({ placeholder: e.target.value || null })}
            />
          </div>

          {/* Default value (applied when a new line row is added) */}
          <div className="flex flex-col gap-1">
            <span className="text-xs text-muted-foreground">Nilai default (saat tambah baris baru)</span>
            <GridColumnDefaultEditor
              col={col}
              onChange={(v, label) => onPatch({ defaultValue: v, defaultValueLabel: label ?? null })}
            />
          </div>

          {/* Lookup-specific config */}
          {isLookup && (
            <div className="flex flex-col gap-5 border-t border-border pt-4">
              <LookupSortFilterFields
                source={col.lookupSource}
                sourceEditable
                defaultSort={col.lookupDefaultSort}
                defaultFilter={col.lookupDefaultFilter}
                onChange={onPatch}
                resetKey={col.dataField}
              />
            </div>
          )}

        </DialogBody>
      </DialogContent>
    </Dialog>
  );
}
