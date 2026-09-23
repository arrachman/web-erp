'use client';

/**
 * One cell of the cash/bank contra-account grid, rendered from a GridCol
 * descriptor (Kustomisasi Grid). Default = SELECTED display cell (plain text);
 * only the edited cell renders its real control (SearchSelect / NumInput /
 * DateInput / Input / DiscountInput / StepperInput / ComboboxInput / Textarea
 * / Checkbox), auto-focused.
 *
 * Editor selection priority: col.cellEditor (semantic) → col.dataType (storage).
 */

import * as React from 'react';
import { cn } from '@/lib/utils';
import { Icon } from '@/components/ui/icons';
import { Input, Textarea } from '@/components/ui/input';
import { Checkbox } from '@/components/ui/checkbox';
import { NumInput } from '@/components/molecules/num-input';
import { DateInput } from '@/components/ui/date-input';
import { SearchSelect } from '@/components/molecules/search-select';
import { DiscountInput } from '@/components/molecules/discount-input';
import { StepperInput } from '@/components/molecules/stepper-input';
import { ComboboxInput } from '@/components/molecules/combobox-input';
import { TableCell } from '@/components/organisms/table';
import {
  loadAccountOptionsCoded,
  loadPartnerOptions,
} from '@/components/pages/items-form-lookups';
import {
  gridLookupLoader, canonicalSource, DEFAULT_LOOKUP_LOADER,
} from '@/lib/grid-lookup-loaders';
import { formatNumber } from '@/lib/format';
import { formatDate } from '@/lib/date-format';
import type { GridCol } from './cash-bank-line-model';

type Loader = (s: string, p: number, l: number) => Promise<{ data: { value: string; label: string; code?: unknown }[]; total: number }>;

// Backend list DTOs cap `limit` at 100, so the label map is built by paging
// through all rows (100 at a time) rather than a single oversized request —
// requesting limit > 100 returns HTTP 400.
const LOOKUP_PAGE_SIZE = 100;
const MAX_LOOKUP_PAGES = 50; // safety bound (≤ 5000 rows) for small lookups

async function fetchAllLabels(loader: Loader): Promise<Map<string, string>> {
  const map = new Map<string, string>();
  const first = await loader('', 1, LOOKUP_PAGE_SIZE);
  for (const o of first.data) map.set(o.value, o.label);
  const totalPages = Math.min(
    Math.ceil((first.total || first.data.length) / LOOKUP_PAGE_SIZE),
    MAX_LOOKUP_PAGES,
  );
  for (let page = 2; page <= totalPages; page += 1) {
    const res = await loader('', page, LOOKUP_PAGE_SIZE);
    for (const o of res.data) map.set(o.value, o.label);
  }
  return map;
}

// Lazy-fetch label maps for small lookups (not accounts — too large).
const labelCache = new Map<string, Promise<Map<string, string>>>();
function resolveLabelMap(source: string): Promise<Map<string, string>> {
  const key = canonicalSource(source);
  let p = labelCache.get(key);
  if (!p) {
    const loader = gridLookupLoader(source);
    p = loader ? fetchAllLabels(loader) : Promise.resolve(new Map());
    labelCache.set(key, p);
  }
  return p;
}

function LookupLabel({ source, value, fallback }: { source?: string | null; value: string; fallback?: string }) {
  const [label, setLabel] = React.useState(fallback ?? '');
  React.useEffect(() => {
    // Accounts + items use the stored label (catalogs too large to map eagerly).
    const canon = canonicalSource(source);
    if (fallback || !value || !source || canon === 'accounts' || canon === 'items') return;
    let alive = true;
    resolveLabelMap(source).then((m) => { if (alive) setLabel(m.get(value) ?? ''); });
    return () => { alive = false; };
  }, [source, value, fallback]);
  return <>{label || value}</>;
}

// ─── Props ────────────────────────────────────────────────────────────────────

export interface LineCellProps {
  col: GridCol;
  value: string;
  label?: string;
  /** 0-based row position — rendered (1-based) by the ROWNUM column type. */
  rowIndex?: number;
  selected: boolean;
  editing: boolean;
  seed?: string;
  selectOnFocus: boolean;
  /** Nav-driven: open the lookup search window as soon as edit mode begins. */
  autoOpenModal?: boolean;
  onSet: (value: string, label?: string) => void;
  onSelect: () => void;
  onEdit: () => void;
  onEndEdit: (focusRoot: boolean) => void;
}

// ─── Edit controls ────────────────────────────────────────────────────────────

function effectiveEditor(col: GridCol): string {
  if (col.cellEditor) return col.cellEditor;
  switch (col.dataType) {
    case 'NUMBER': return 'NUMBER';
    case 'DATE':   return 'DATE';
    case 'LOOKUP': return 'LOOKUP';
    default:       return 'TEXT';
  }
}

function EditControl({ col, value, label, rowIndex, seed, selectOnFocus, autoOpenModal, onSet, onEndEdit }: LineCellProps & { autoOpenModal?: boolean }) {
  const editor = effectiveEditor(col);

  switch (editor) {
    case 'ROWNUM':
      // Auto sequence — read-only, never truly entered (column forced non-editable).
      return (
        <div className="flex h-[var(--row-h)] items-center justify-center tabular-nums text-[var(--fg-subtle)]">
          {(rowIndex ?? 0) + 1}
        </div>
      );

    case 'LOOKUP':
      return (
        <SearchSelect
          autoFocus
          fill
          autoOpenModal={autoOpenModal}
          initialQuery={seed}
          placeholder={col.placeholder || 'Pilih…'}
          value={value}
          initialLabel={label}
          onValueChange={(v) => onSet(v)}
          onPick={(o) => { onSet(o.value, o.label); onEndEdit(true); }}
          loadOptions={gridLookupLoader(col.lookupSource, col.lookupDefaultFilter, col.lookupDefaultSort) ?? DEFAULT_LOOKUP_LOADER}
        />
      );

    case 'ACCOUNT_PICKER':
      return (
        <SearchSelect
          autoFocus
          fill
          autoOpenModal={autoOpenModal}
          initialQuery={seed}
          placeholder={col.placeholder || 'Pilih akun…'}
          value={value}
          initialLabel={label}
          onValueChange={(v) => onSet(v)}
          onPick={(o) => { onSet(o.value, o.label); onEndEdit(true); }}
          loadOptions={loadAccountOptionsCoded as unknown as Loader}
        />
      );

    case 'PARTNER_PICKER':
      return (
        <SearchSelect
          autoFocus
          fill
          autoOpenModal={autoOpenModal}
          initialQuery={seed}
          placeholder={col.placeholder || 'Pilih partner…'}
          value={value}
          initialLabel={label}
          onValueChange={(v) => onSet(v)}
          onPick={(o) => { onSet(o.value, o.label); onEndEdit(true); }}
          loadOptions={loadPartnerOptions as unknown as Loader}
        />
      );

    case 'NUMBER':
      return (
        <NumInput
          autoFocus
          decimals={2}
          value={value}
          onFocus={(e) => { if (selectOnFocus) e.currentTarget.select(); }}
          onChange={(raw) => onSet(raw)}
        />
      );

    case 'DISCOUNT':
      return (
        <DiscountInput
          autoFocus
          value={value}
          onFocus={(e: React.FocusEvent<HTMLInputElement>) => { if (selectOnFocus) e.currentTarget.select(); }}
          onChange={(raw) => onSet(raw)}
        />
      );

    case 'STEPPER':
      return (
        <StepperInput
          autoFocus
          value={value}
          onChange={(raw) => onSet(raw)}
          decimals={0}
        />
      );

    case 'COMBOBOX':
      return (
        <ComboboxInput
          autoFocus
          value={value}
          options={col.options ?? []}
          onChange={(v) => onSet(v)}
          onCommit={() => onEndEdit(true)}
        />
      );

    case 'DATE':
      return <DateInput value={value} onChange={(v) => onSet(v)} />;

    case 'TEXTAREA':
      return (
        <Textarea
          autoFocus
          value={value}
          rows={2}
          placeholder={col.placeholder ?? undefined}
          onChange={(e) => onSet(e.target.value)}
          onBlur={() => onEndEdit(false)}
          onKeyDown={(e) => {
            if (e.key === 'Escape') onEndEdit(false);
          }}
        />
      );

    case 'CHECKBOX':
      return (
        <div className="flex h-[var(--row-h)] items-center justify-center">
          <Checkbox
            checked={value === 'true' || value === '1'}
            onCheckedChange={(v) => { onSet(v ? 'true' : 'false'); onEndEdit(true); }}
          />
        </div>
      );

    case 'NONE':
      return (
        <div className="flex h-[var(--row-h)] items-center px-[10px] text-[var(--fg-subtle)]">
          {value || '—'}
        </div>
      );

    default:
      return (
        <Input
          autoFocus
          value={value}
          placeholder={col.placeholder ?? undefined}
          onFocus={(e) => { if (selectOnFocus) e.currentTarget.select(); }}
          onChange={(e) => onSet(e.target.value)}
        />
      );
  }
}

// ─── Display cell ─────────────────────────────────────────────────────────────

function displayCell(col: GridCol, value: string, label?: string, rowIndex?: number): { node: React.ReactNode; muted: boolean } {
  const editor = effectiveEditor(col);

  // Auto sequence — derived from row position, ignores the (empty) cell value.
  if (editor === 'ROWNUM') {
    return { node: (rowIndex ?? 0) + 1, muted: true };
  }

  if (!value) {
    // A configured placeholder (Kustomisasi Grid) wins as the empty hint; otherwise
    // lookup cells show no "Pilih…" label (the search icon on hover signals the
    // picker) and fall to the same muted "—" / "0" as text / numeric cells.
    const ph =
      col.placeholder
      || (editor === 'NUMBER' || editor === 'DISCOUNT' || editor === 'STEPPER' ? '0'
        : editor === 'NONE' ? ''
        : '—');
    return { node: ph, muted: true };
  }

  switch (editor) {
    case 'LOOKUP':
      return { node: <LookupLabel source={col.lookupSource} value={value} fallback={label} />, muted: false };
    case 'ACCOUNT_PICKER':
      return { node: label || value, muted: false };
    case 'PARTNER_PICKER':
      return { node: <LookupLabel source="partner" value={value} fallback={label} />, muted: false };
    case 'NUMBER':
    case 'STEPPER':
      return { node: formatNumber(Number(value || 0), 0), muted: false };
    case 'DISCOUNT':
      return { node: `${formatNumber(Number(value || 0), 2)} %`, muted: false };
    case 'DATE':
      return { node: formatDate(value), muted: false };
    case 'CHECKBOX':
      return {
        node: (
          <span style={{ color: (value === 'true' || value === '1') ? 'var(--primary)' : 'var(--fg-subtle)', fontWeight: 600 }}>
            {(value === 'true' || value === '1') ? '✓' : '—'}
          </span>
        ),
        muted: false,
      };
    case 'NONE':
      return { node: value, muted: false };
    default:
      return { node: value, muted: false };
  }
}

// ─── LineCell ─────────────────────────────────────────────────────────────────

export function LineCell(props: LineCellProps) {
  const { col, value, label, rowIndex, selected, editing, autoOpenModal, onSelect, onEdit, onEndEdit } = props;
  const editor = effectiveEditor(col);
  const numeric = editor === 'NUMBER' || editor === 'DISCOUNT' || editor === 'STEPPER';
  const isRownum = editor === 'ROWNUM';
  const isLookup = editor === 'LOOKUP' || editor === 'ACCOUNT_PICKER' || editor === 'PARTNER_PICKER';
  // "Skip" flag (Kustomisasi Grid) → view-only: still selectable (click / arrows /
  // Tab land on it) but never editable; Enter-navigation jumps over it.
  const skippable = !!col.isSkippable;
  // Set when the search icon is clicked → enter edit mode AND auto-open the
  // SearchSelect modal window. Reset once edit mode ends. Combined with the
  // nav-driven `autoOpenModal` (Enter landing on a required lookup).
  const [openModalOnEdit, setOpenModalOnEdit] = React.useState(false);
  React.useEffect(() => { if (!editing) setOpenModalOnEdit(false); }, [editing]);

  return (
    <TableCell
      className={cn(
        'p-0 align-middle',
        // Lookup fills the cell (display + edit): drop the cell's own !px so the
        // inner px-[10px] is the only horizontal padding — no double margin,
        // and the placeholder lines up exactly with the editor text.
        !skippable && isLookup && '!px-0',
        selected && !editing && 'shadow-[inset_0_0_0_2px_var(--primary)]',
      )}
    >
      {editing && !skippable ? (
        <div
          onBlur={(e) => {
            // Focus leaving into the SearchSelect modal (portaled outside this
            // cell, role="dialog") must NOT end edit — that would unmount the
            // SearchSelect and close the modal the user just opened.
            const next = e.relatedTarget as HTMLElement | null;
            if (e.currentTarget.contains(next) || next?.closest('[role="dialog"]')) return;
            onEndEdit(false);
          }}
        >
          <EditControl {...props} autoOpenModal={openModalOnEdit || !!autoOpenModal} />
        </div>
      ) : (
        <div
          role="button"
          tabIndex={-1}
          onClick={onSelect}
          onDoubleClick={onEdit}
          className={cn(
            'group flex h-[var(--row-h)] w-full cursor-pointer select-none items-center truncate px-[10px]',
            skippable && 'opacity-70',
            numeric && 'justify-end tabular-nums',
            (isRownum || editor === 'CHECKBOX') && 'justify-center',
            isRownum && 'tabular-nums',
          )}
        >
          {(() => {
            const { node, muted } = displayCell(col, value, label, rowIndex);
            return <span className={cn('min-w-0 truncate', muted && 'text-[var(--fg-subtle)]')}>{node}</span>;
          })()}
          {isLookup && !skippable && (
            <button
              type="button"
              title="Cari…"
              aria-label="Cari…"
              onClick={(e) => { e.stopPropagation(); setOpenModalOnEdit(true); onEdit(); }}
              className="ml-auto flex shrink-0 cursor-pointer items-center pl-1 text-muted-foreground opacity-0 transition-opacity hover:text-foreground group-hover:opacity-100"
            >
              <Icon name="search" size={13} />
            </button>
          )}
        </div>
      )}
    </TableCell>
  );
}
