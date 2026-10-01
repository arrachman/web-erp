'use client';

import * as React from 'react';
import {
  DndContext, PointerSensor, KeyboardSensor,
  useSensor, useSensors, closestCenter, type DragEndEvent,
} from '@dnd-kit/core';
import {
  SortableContext, verticalListSortingStrategy, useSortable,
  sortableKeyboardCoordinates, arrayMove,
} from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { Input } from '@/components/ui/input';
import { Checkbox } from '@/components/ui/checkbox';
import { Icon } from '@/components/ui/icons';
import {
  Table, TableHeader, TableBody, TableRow, TableHead, TableCell,
} from '@/components/organisms/table';
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select';
import {
  FORM_FIELD_TYPES, FORM_COLUMN_SLOTS, FORM_FIELD_TYPE_LABELS,
  type ErpFormField, type FormFieldType, type FormColumnSlot,
} from '@/lib/api/form-fields';
import { LOOKUP_SOURCE_OPTIONS, sourceLabelOf } from '@/lib/lookup-source-registry';
import { FieldSettingsPopover } from './form-builder-field-settings';

const SLOT_LABELS: Record<FormColumnSlot, string> = { LEFT: 'Kiri', CENTER: 'Tengah', RIGHT: 'Kanan' };
const TEXT_VIEW = 'cursor-text truncate rounded pr-2 py-0.5 text-sm hover:bg-secondary/60';

function isFieldChanged(a: ErpFormField, b: ErpFormField | undefined): boolean {
  if (!b) return true;
  return (
    a.label !== b.label || a.fieldType !== b.fieldType ||
    a.isRequired !== b.isRequired || a.isVisible !== b.isVisible ||
    a.columnSlot !== b.columnSlot || (a.lookupSource ?? null) !== (b.lookupSource ?? null) ||
    JSON.stringify(a.lookupDefaultFilter ?? null) !== JSON.stringify(b.lookupDefaultFilter ?? null) ||
    (a.lookupDefaultSort ?? null) !== (b.lookupDefaultSort ?? null) ||
    (a.placeholder ?? null) !== (b.placeholder ?? null) ||
    (a.defaultValue ?? null) !== (b.defaultValue ?? null) ||
    (a.isReadonly ?? false) !== (b.isReadonly ?? false)
  );
}

function LabelCell({ value, onChange }: { value: string; onChange: (v: string) => void }) {
  const [editing, setEditing] = React.useState(false);
  if (editing) {
    return (
      <Input
        autoFocus
        className="h-6 px-1.5 py-0 text-sm"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        onBlur={() => setEditing(false)}
        onKeyDown={(e) => { if (e.key === 'Enter' || e.key === 'Escape') setEditing(false); }}
      />
    );
  }
  return (
    <div className={TEXT_VIEW} title="Klik untuk edit" onClick={() => setEditing(true)}>
      {value || <span className="text-muted-foreground italic">—</span>}
    </div>
  );
}

/** Source selector for LOOKUP fields (custom only). */
function LookupSourceSelect({ value, onChange }: { value?: string | null; onChange: (v: string | null) => void }) {
  return (
    <Select value={value ?? ''} onValueChange={(v) => onChange(v || null)}>
      <SelectTrigger className="h-6 text-xs"><SelectValue placeholder="Pilih sumber…" /></SelectTrigger>
      <SelectContent>
        {LOOKUP_SOURCE_OPTIONS.map((s) => (
          <SelectItem key={s.value} value={s.value}>{s.label}</SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}

function FieldRow({
  field,
  savedField,
  onUpdate,
  onDelete,
  onSave,
  saving,
}: {
  field: ErpFormField;
  savedField?: ErpFormField;
  onUpdate: (patch: Partial<ErpFormField>) => void;
  onDelete?: () => void;
  onSave?: () => void | Promise<void>;
  saving?: boolean;
}) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } =
    useSortable({ id: field.fieldKey });

  const style = { transform: CSS.Transform.toString(transform), transition };
  const changed = isFieldChanged(field, savedField);
  const isCustom = field.kind === 'CUSTOM';

  return (
    <TableRow ref={setNodeRef} style={style} className={isDragging ? 'opacity-50' : ''}>
      <TableCell className="w-6 px-1 text-center">
        <span
          {...attributes}
          {...listeners}
          className="cursor-grab text-muted-foreground"
          title="Seret untuk ubah urutan"
        >
          <Icon name="grip-vertical" size={12} />
        </span>
      </TableCell>

      <TableCell className="w-2 px-1 text-center">
        {changed && <span className="inline-block h-2 w-2 rounded-full bg-warning" title="Belum disimpan" />}
      </TableCell>

      <TableCell className="w-[280px]">
        <LabelCell value={field.label} onChange={(v) => onUpdate({ label: v })} />
      </TableCell>

      <TableCell className="w-[140px]">
        {isCustom ? (
          <Select value={field.fieldType} onValueChange={(v) => onUpdate({ fieldType: v as FormFieldType })}>
            <SelectTrigger className="h-6 text-xs"><SelectValue /></SelectTrigger>
            <SelectContent>
              {FORM_FIELD_TYPES.map((t) => (
                <SelectItem key={t} value={t}>{FORM_FIELD_TYPE_LABELS[t]}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        ) : (
          <Select value={field.fieldType} disabled>
            <SelectTrigger
              className="h-6 text-xs cursor-not-allowed opacity-70"
              title="Tipe field sistem terikat kolom DB & posting GL — tidak dapat diubah"
            >
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {FORM_FIELD_TYPES.map((t) => (
                <SelectItem key={t} value={t}>{FORM_FIELD_TYPE_LABELS[t]}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        )}
      </TableCell>

      <TableCell className="w-[160px]">
        <div className="flex items-center gap-1">
          {field.fieldType === 'LOOKUP' ? (
            <div className="flex-1 min-w-0">
              <LookupSourceSelect
                value={field.lookupSource}
                onChange={(v) => onUpdate({ lookupSource: v })}
              />
            </div>
          ) : (['PARTNER','ACCOUNT','BRANCH','LOCATION','CURRENCY'] as string[]).includes(field.fieldType) ? (
            <span className="flex-1 pr-1 text-xs text-muted-foreground truncate">
              {sourceLabelOf(field.fieldType === 'PARTNER' ? 'partners'
                : field.fieldType === 'ACCOUNT' ? 'accounts'
                : field.fieldType === 'BRANCH' ? 'branches'
                : field.fieldType === 'LOCATION' ? 'locations'
                : 'currencies')}
            </span>
          ) : (
            <span className="flex-1 pr-1 text-xs text-muted-foreground">—</span>
          )}
          <FieldSettingsPopover field={field} onUpdate={onUpdate} onSave={onSave} saving={saving} />
        </div>
      </TableCell>

      <TableCell className="w-[110px]">
        <Select value={field.columnSlot} onValueChange={(v) => onUpdate({ columnSlot: v as FormColumnSlot })}>
          <SelectTrigger className="h-6 text-xs"><SelectValue /></SelectTrigger>
          <SelectContent>
            {FORM_COLUMN_SLOTS.map((s) => (
              <SelectItem key={s} value={s}>{SLOT_LABELS[s]}</SelectItem>
            ))}
          </SelectContent>
        </Select>
      </TableCell>

      <TableCell className="w-10 text-center">
        <Checkbox
          checked={field.isRequired}
          onCheckedChange={(c) => onUpdate({ isRequired: Boolean(c) })}
        />
      </TableCell>

      <TableCell className="w-10 text-center">
        <Checkbox
          checked={field.isVisible}
          onCheckedChange={(c) => onUpdate({ isVisible: Boolean(c) })}
        />
      </TableCell>

      <TableCell className="w-8 text-center">
        {isCustom && onDelete ? (
          <button
            type="button"
            className="iconbtn text-danger"
            title="Hapus field kustom"
            onClick={onDelete}
          >
            <Icon name="trash" size={12} />
          </button>
        ) : (
          <span className="inline-block h-3 w-3 rounded-full border border-border" title="Field sistem (tidak dapat dihapus)" />
        )}
      </TableCell>
    </TableRow>
  );
}

export function FormBuilderFields({
  fields,
  savedFields,
  onFieldsChange,
  onSave,
  saving,
}: {
  fields: ErpFormField[];
  savedFields: ErpFormField[];
  onFieldsChange: (fields: ErpFormField[]) => void;
  onSave?: () => void | Promise<void>;
  saving?: boolean;
}) {
  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 5 } }),
    useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates }),
  );

  const handleDragEnd = (e: DragEndEvent) => {
    const { active, over } = e;
    if (!over || active.id === over.id) return;
    const oldIdx = fields.findIndex((f) => f.fieldKey === active.id);
    const newIdx = fields.findIndex((f) => f.fieldKey === over.id);
    if (oldIdx === -1 || newIdx === -1) return;
    onFieldsChange(arrayMove(fields, oldIdx, newIdx).map((f, i) => ({ ...f, sortOrder: i })));
  };

  const update = (key: string, patch: Partial<ErpFormField>) =>
    onFieldsChange(fields.map((f) => (f.fieldKey === key ? { ...f, ...patch } : f)));

  const deleteField = (key: string) =>
    onFieldsChange(fields.filter((f) => f.fieldKey !== key).map((f, i) => ({ ...f, sortOrder: i })));

  const savedMap = Object.fromEntries(savedFields.map((f) => [f.fieldKey, f]));

  return (
    <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={handleDragEnd}>
      <SortableContext items={fields.map((f) => f.fieldKey)} strategy={verticalListSortingStrategy}>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="w-6" />
              <TableHead className="w-2" />
              <TableHead className="w-[280px]">Label</TableHead>
              <TableHead className="w-[140px]">Tipe</TableHead>
              <TableHead className="w-[160px]">Sumber / Konfigurasi</TableHead>
              <TableHead className="w-[110px]">Kolom</TableHead>
              <TableHead className="w-10 text-center">Wajib</TableHead>
              <TableHead className="w-10 text-center">Tampil</TableHead>
              <TableHead className="w-8" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {fields.map((f) => (
              <FieldRow
                key={f.fieldKey}
                field={f}
                savedField={savedMap[f.fieldKey]}
                onUpdate={(patch) => update(f.fieldKey, patch)}
                onDelete={f.kind === 'CUSTOM' ? () => deleteField(f.fieldKey) : undefined}
                onSave={onSave}
                saving={saving}
              />
            ))}
          </TableBody>
        </Table>
      </SortableContext>
    </DndContext>
  );
}
