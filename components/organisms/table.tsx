import * as React from 'react';
import { cn } from '@/lib/utils';
import { Checkbox } from '@/components/ui/checkbox';
import { Icon, type IconName } from '@/components/ui/icons';
import { Button } from '@/components/ui/button';
import { Kbd } from '@/components/ui/kbd';
import { tGlobal } from '@/lib/mock';

/**
 * Dense data-grid table primitives. Mirrors prototype `.lines table` /
 * `.tbl` — 12px text, sticky muted header, hover row tint, compact
 * cell padding. Use `numeric` on Th/Td for right-aligned mono numbers.
 */
export const Table = React.forwardRef<
  HTMLTableElement,
  React.TableHTMLAttributes<HTMLTableElement>
>(({ className, ...props }, ref) => (
  <table
    ref={ref}
    className={cn(
      'w-full border-separate border-spacing-0 text-xs text-foreground',
      className,
    )}
    {...props}
  />
));
Table.displayName = 'Table';

export const TableHeader = React.forwardRef<
  HTMLTableSectionElement,
  React.HTMLAttributes<HTMLTableSectionElement>
>(({ className, ...props }, ref) => (
  <thead ref={ref} className={className} {...props} />
));
TableHeader.displayName = 'TableHeader';

export const TableBody = React.forwardRef<
  HTMLTableSectionElement,
  React.HTMLAttributes<HTMLTableSectionElement>
>(({ className, ...props }, ref) => (
  <tbody ref={ref} className={className} {...props} />
));
TableBody.displayName = 'TableBody';

export const TableFooter = React.forwardRef<
  HTMLTableSectionElement,
  React.HTMLAttributes<HTMLTableSectionElement>
>(({ className, ...props }, ref) => (
  <tfoot
    ref={ref}
    className={cn(
      '[&_td]:border-t [&_td]:border-border [&_td]:bg-secondary [&_td]:font-medium',
      className,
    )}
    {...props}
  />
));
TableFooter.displayName = 'TableFooter';

export const TableRow = React.forwardRef<
  HTMLTableRowElement,
  React.HTMLAttributes<HTMLTableRowElement>
>(({ className, ...props }, ref) => (
  <tr
    ref={ref}
    className={cn(
      'transition-colors hover:[&>td]:bg-[color-mix(in_oklab,var(--panel-hover)_60%,transparent)]',
      'data-[selected=true]:[&>td]:!bg-[var(--primary-soft)]',
      // focused row: full 1px primary outline only (no bg tint)
      'data-[focused=true]:[&>td]:shadow-[inset_0_1px_0_var(--primary),inset_0_-1px_0_var(--primary)]',
      // focused + hover → primary-soft bg (override gray hover)
      'data-[focused=true]:hover:[&>td]:!bg-[var(--primary-soft)]',
      'data-[focused=true]:[&>td:first-child]:shadow-[inset_1px_1px_0_var(--primary),inset_0_-1px_0_var(--primary),inset_1px_-1px_0_var(--primary)]',
      'data-[focused=true]:[&>td:last-child]:shadow-[inset_0_1px_0_var(--primary),inset_-1px_0_0_var(--primary),inset_-1px_-1px_0_var(--primary)]',
      className,
    )}
    {...props}
  />
));
TableRow.displayName = 'TableRow';

export const TableHead = React.forwardRef<
  HTMLTableCellElement,
  React.ThHTMLAttributes<HTMLTableCellElement> & { numeric?: boolean; truncate?: boolean }
>(({ className, numeric, truncate = true, ...props }, ref) => (
  <th
    ref={ref}
    className={cn(
      'sticky top-0 z-10 h-[var(--header-h)] overflow-hidden border-b border-border bg-secondary !px-[10px] !py-0 text-left text-[11px] font-medium uppercase tracking-wide text-muted-foreground',
      truncate && 'truncate',
      numeric && 'text-right',
      className,
    )}
    {...props}
  />
));
TableHead.displayName = 'TableHead';

export const TableCell = React.forwardRef<
  HTMLTableCellElement,
  React.TdHTMLAttributes<HTMLTableCellElement> & { numeric?: boolean; truncate?: boolean }
>(({ className, numeric, truncate = true, ...props }, ref) => (
  <td
    ref={ref}
    className={cn(
      'h-[var(--row-h)] overflow-hidden border-b border-border !px-[10px] !py-0',
      truncate && 'truncate',
      numeric &&
        'text-right font-mono tabular-nums',
      className,
    )}
    {...props}
  />
));
TableCell.displayName = 'TableCell';

/** Checkbox header cell for the select-all column. */
export function CheckboxHead({
  checked,
  onCheckedChange,
}: {
  checked: boolean | 'indeterminate';
  onCheckedChange: (v: boolean) => void;
}) {
  return (
    <TableHead className="w-8 align-middle">
      <div className="flex items-center justify-center">
        <Checkbox
          checked={checked}
          onCheckedChange={(v) => onCheckedChange(v === true)}
          aria-label={tGlobal('Pilih semua')}
        />
      </div>
    </TableHead>
  );
}

/** Checkbox data cell for a selectable row. */
export function CheckboxCell({
  checked,
  onCheckedChange,
}: {
  checked: boolean;
  onCheckedChange: (v: boolean) => void;
}) {
  return (
    <TableCell className="w-8 align-middle" onClick={(e) => e.stopPropagation()}>
      <div className="flex items-center justify-center">
        <Checkbox
          checked={checked}
          onCheckedChange={(v) => onCheckedChange(v === true)}
          aria-label={tGlobal('Pilih baris')}
        />
      </div>
    </TableCell>
  );
}

function SortIndicator({ active, dir }: { active: boolean; dir?: 'asc' | 'desc' }) {
  return (
    <span className={cn('shrink-0 text-[9px] leading-none', active ? 'text-foreground' : 'opacity-20')}>
      {!active ? '↕' : dir === 'asc' ? '↑' : '↓'}
    </span>
  );
}

/**
 * Sortable column header. Click cycles asc → desc → asc.
 * Pass `sortBy`/`sortDir` (current sort state) and `onSort` callback.
 */
export function SortableHead({
  field,
  children,
  sortBy,
  sortDir,
  onSort,
  numeric,
  className,
}: {
  field: string;
  children: React.ReactNode;
  sortBy: string;
  sortDir: 'asc' | 'desc';
  onSort: (field: string, dir: 'asc' | 'desc') => void;
  numeric?: boolean;
  className?: string;
}) {
  const isActive = sortBy === field;
  const nextDir: 'asc' | 'desc' = isActive && sortDir === 'asc' ? 'desc' : 'asc';
  return (
    <TableHead
      className={cn('cursor-pointer select-none hover:bg-[var(--panel-hover)]', className)}
      numeric={numeric}
      onClick={() => onSort(field, nextDir)}
    >
      <div className={cn('flex items-center gap-1', numeric && 'justify-end')}>
        <span>{children}</span>
        <SortIndicator active={isActive} dir={isActive ? sortDir : undefined} />
      </div>
    </TableHead>
  );
}

/**
 * Code cell rendered as a clickable blue link. Canonical pattern per
 * `apps/web-erp/CLAUDE.md` — KODE column is the entity's primary link
 * to open detail/edit. Stops row-click propagation.
 */
export function CodeLinkCell({
  code,
  onOpen,
  className,
}: {
  code: string;
  onOpen: () => void;
  className?: string;
}) {
  return (
    <TableCell className={cn('mono tabular-nums', className)}>
      <button
        type="button"
        className="bg-transparent border-0 p-0 m-0 text-primary font-[inherit] hover:underline focus:underline focus:outline-none"
        onClick={(e) => {
          e.stopPropagation();
          onOpen();
        }}
      >
        {code}
      </button>
    </TableCell>
  );
}

/**
 * Full-width empty-state cell. Renders an illustrated empty state with
 * icon, title, description, and optional CTA. Two variants per
 * `apps/web-erp/CLAUDE.md §2.9`:
 *
 * - `empty` (default): no data yet — invites first entry.
 * - `filtered`: filter/search returned nothing — invites reset.
 *
 * Backward-compat: existing callers with only `colSpan` keep working —
 * defaults render a sensible "Belum ada data" state. Pass `children` to
 * fully override the body (legacy behaviour).
 */
export function TableEmpty({
  colSpan,
  variant = 'empty',
  title,
  description,
  entityLabel,
  searchTerm,
  icon,
  actionLabel,
  actionShortcut,
  onAction,
  children,
  className,
}: {
  colSpan: number;
  variant?: 'empty' | 'filtered';
  title?: React.ReactNode;
  description?: React.ReactNode;
  entityLabel?: string;
  searchTerm?: string;
  icon?: IconName;
  actionLabel?: string;
  actionShortcut?: string;
  onAction?: () => void;
  children?: React.ReactNode;
  className?: string;
}) {
  const isFiltered = variant === 'filtered';
  const resolvedIcon: IconName = icon ?? (isFiltered ? 'search' : 'database');

  const defaultTitle = isFiltered
    ? 'Tidak ada hasil'
    : entityLabel
      ? `Belum ada ${entityLabel.toLowerCase()}`
      : 'Belum ada data';

  const defaultDescription = isFiltered ? (
    searchTerm ? (
      <>
        Tidak ditemukan hasil untuk{' '}
        <span className="font-medium text-foreground">“{searchTerm}”</span>.
        Coba kata kunci lain atau reset filter.
      </>
    ) : (
      'Filter aktif tidak cocok dengan data manapun. Coba kata kunci lain atau reset filter.'
    )
  ) : entityLabel ? (
    <>Mulai dengan menambahkan {entityLabel.toLowerCase()} pertama Anda.</>
  ) : (
    'Mulai dengan menambahkan entri pertama Anda.'
  );

  return (
    <tr>
      <td
        colSpan={colSpan}
        className={cn('border-b border-border p-0', className)}
      >
        {children ?? (
          <div className="flex flex-col items-center justify-center gap-3 px-6 py-14 text-center">
            <div
              className={cn(
                'relative flex h-14 w-14 items-center justify-center rounded-full border border-border bg-[var(--panel-hover)] text-muted-foreground',
                'before:absolute before:inset-[-6px] before:rounded-full before:border before:border-border/60 before:opacity-60',
                'after:absolute after:inset-[-12px] after:rounded-full after:border after:border-border/30 after:opacity-50',
              )}
            >
              <Icon name={resolvedIcon} size={22} stroke={1.4} />
            </div>
            <div className="space-y-1">
              <div className="text-[13px] font-medium text-foreground">
                {title ?? defaultTitle}
              </div>
              <div className="max-w-sm text-xs leading-relaxed text-muted-foreground">
                {description ?? defaultDescription}
              </div>
            </div>
            {onAction && actionLabel && (
              <div className="mt-1">
                <Button
                  variant="primary"
                  size="sm"
                  onClick={onAction}
                  className="gap-1.5"
                >
                  <Icon name={isFiltered ? 'refresh' : 'plus'} size={12} />
                  {actionLabel}
                  {actionShortcut && (
                    <Kbd className="ml-1 border-primary-foreground/30 bg-primary-foreground/15 text-primary-foreground">
                      {actionShortcut}
                    </Kbd>
                  )}
                </Button>
              </div>
            )}
          </div>
        )}
      </td>
    </tr>
  );
}
