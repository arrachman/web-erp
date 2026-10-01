'use client';

import * as React from 'react';
import { Input } from '@/components/ui/input';
import { NumInput } from '@/components/molecules/num-input';
import { Checkbox } from '@/components/ui/checkbox';
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select';
import {
  COLUMN_TYPES, COLUMN_TYPE_LABELS, type ColumnType,
} from '@/lib/api/transaction-grids';
import { LOOKUP_SOURCE_OPTIONS } from '@/lib/lookup-source-registry';
import { TableCell } from '@/components/organisms/table';
import { cn } from '@/lib/utils';

const SEL_BG = 'color-mix(in srgb, var(--primary) 12%, transparent)';
const SEL_OUTLINE = '1px solid color-mix(in srgb, var(--primary) 40%, transparent)';
// `-mx-2` cancels the chip's `px-2` so the text aligns with the header's
// 10px cell padding (header ↔ body left edge match) while keeping the
// hover/edit affordance.
const TV = 'cursor-text truncate rounded -mx-2 px-2 py-0.5 text-sm hover:bg-secondary/60';

export const cellSelectedStyle = (sel: boolean): React.CSSProperties =>
  sel
    ? { backgroundColor: SEL_BG, outline: SEL_OUTLINE, outlineOffset: '-1px', cursor: 'cell' }
    : { cursor: 'cell' };

export type EditableCellHandle = { startEdit: (char?: string) => void };

export const EditableTextCell = React.forwardRef<EditableCellHandle, {
  value: string;
  onChange: (v: string) => void;
  onEditDone?: () => void;
}>(({ value, onChange, onEditDone }, ref) => {
  const [editing, setEditing] = React.useState(false);
  const inputRef = React.useRef<HTMLInputElement>(null);
  const onChangeRef = React.useRef(onChange);
  onChangeRef.current = onChange;

  React.useImperativeHandle(ref, () => ({
    startEdit: (char?: string) => {
      if (char !== undefined) onChangeRef.current(char);
      setEditing(true);
    },
  }), []);

  React.useEffect(() => { if (editing) inputRef.current?.focus(); }, [editing]);

  const done = () => { setEditing(false); onEditDone?.(); };

  if (!editing) {
    return (
      <div className={TV} onDoubleClick={() => setEditing(true)} title={value}>
        {value || <span className="text-xs italic text-muted-foreground">—</span>}
      </div>
    );
  }
  return (
    <Input
      ref={inputRef}
      value={value}
      onChange={(e) => onChange(e.target.value)}
      onBlur={done}
      onKeyDown={(e) => {
        if (e.key === 'Enter' || e.key === 'Escape') { e.preventDefault(); done(); }
      }}
    />
  );
});
EditableTextCell.displayName = 'EditableTextCell';

export const EditableNumCell = React.forwardRef<EditableCellHandle, {
  value: string;
  onChange: (v: string) => void;
  onEditDone?: () => void;
}>(({ value, onChange, onEditDone }, ref) => {
  const [editing, setEditing] = React.useState(false);
  const inputRef = React.useRef<HTMLInputElement>(null);
  const onChangeRef = React.useRef(onChange);
  onChangeRef.current = onChange;

  React.useImperativeHandle(ref, () => ({
    startEdit: (char?: string) => {
      if (char !== undefined) onChangeRef.current(char);
      setEditing(true);
    },
  }), []);

  React.useEffect(() => { if (editing) inputRef.current?.focus(); }, [editing]);

  const done = () => { setEditing(false); onEditDone?.(); };

  if (!editing) {
    return (
      <div
        className="cursor-text rounded -mx-2 px-2 py-0.5 text-right text-sm tabular-nums hover:bg-secondary/60"
        onDoubleClick={() => setEditing(true)}
      >
        {value || '0'}
      </div>
    );
  }
  return (
    <NumInput
      ref={inputRef}
      value={value}
      decimals={0}
      onChange={onChange}
      onBlur={done}
      onKeyDown={(e) => {
        if (e.key === 'Enter' || e.key === 'Escape') { e.preventDefault(); done(); }
      }}
    />
  );
});
EditableNumCell.displayName = 'EditableNumCell';

export function EditableSelectCell({ value, onChange }: {
  value: ColumnType | null | undefined;
  onChange: (v: ColumnType) => void;
}) {
  const [open, setOpen] = React.useState(false);
  const currentType = value ?? 'text';
  return (
    <Select value={currentType} open={open} onOpenChange={setOpen} onValueChange={(v) => onChange(v as ColumnType)}>
      <SelectTrigger
        style={!open ? { border: '1px solid transparent', backgroundColor: 'transparent', boxShadow: 'none' } : undefined}
        className={cn('-mx-2', !open && 'hover:bg-secondary/60')}
      >
        <SelectValue />
      </SelectTrigger>
      <SelectContent>
        {COLUMN_TYPES.map((t) => (
          <SelectItem key={t} value={t}>{COLUMN_TYPE_LABELS[t]}</SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}

/** Source picker shown under the type select when columnType === 'lookup'. */
export function GridLookupSourceCell({ value, onChange }: {
  value: string | null | undefined;
  onChange: (v: string | null) => void;
}) {
  return (
    <Select value={value ?? ''} onValueChange={(v) => onChange(v || null)}>
      <SelectTrigger className="h-7 text-xs"><SelectValue placeholder="Pilih sumber…" /></SelectTrigger>
      <SelectContent>
        {LOOKUP_SOURCE_OPTIONS.map((s) => (
          <SelectItem key={s.value} value={s.value}>{s.label}</SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}

export function FlagCell({ checked, selected, onChange, onSelect }: {
  checked: boolean; selected: boolean;
  onChange: (v: boolean) => void; onSelect: () => void;
}) {
  return (
    <TableCell
      style={{
        width: 56, textAlign: 'center', cursor: 'pointer', userSelect: 'none',
        ...(selected ? { backgroundColor: SEL_BG, outline: SEL_OUTLINE, outlineOffset: '-1px' } : {}),
      }}
      onClick={onSelect}
    >
      <div className="flex justify-center" onClick={(e) => e.stopPropagation()}>
        <Checkbox checked={checked} onCheckedChange={(v) => onChange(v === true)} />
      </div>
    </TableCell>
  );
}
