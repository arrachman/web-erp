'use client';

/**
 * Form Builder (/admin/form-builder).
 * Kustomisasi field header form transaksi per jenis transaksi.
 * Structural fields: ubah label, required, visibility, posisi kolom.
 * Custom fields: tambah field baru (TEXT/NUMBER/DATE/LOOKUP/dll).
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { notify } from '@/lib/feedback';
import {
  listTransactionTypes,
  type ErpTransactionType,
} from '@/lib/api/transaction-grids';
import {
  getFormFields,
  saveFormFields,
  STRUCTURAL_FIELD_KEYS,
  type ErpFormField,
} from '@/lib/api/form-fields';
import { useUndoHistory } from '@/lib/use-undo-history';
import { GridCustomizationTree } from './grid-customization-tree';
import { FormBuilderFields } from './form-builder-fields';

const CUSTOM_FIELD_TYPES = ['TEXT', 'NUMBER', 'DATE', 'LOOKUP'] as const;

function nextCustomKey(fields: ErpFormField[]): string {
  const nums = fields
    .filter((f) => f.kind === 'CUSTOM' && f.fieldKey.startsWith('customField'))
    .map((f) => parseInt(f.fieldKey.replace('customField', ''), 10))
    .filter((n) => !isNaN(n));
  return `customField${(Math.max(0, ...nums) + 1)}`;
}

function blankCustomField(fields: ErpFormField[]): ErpFormField {
  return {
    fieldKey: nextCustomKey(fields),
    kind: 'CUSTOM',
    label: 'Field Baru',
    fieldType: 'TEXT',
    lookupSource: null,
    isRequired: false,
    isVisible: true,
    sortOrder: fields.length,
    columnSlot: 'LEFT',
  };
}

export function FormBuilderPage() {
  const [types, setTypes] = React.useState<ErpTransactionType[]>([]);
  const [selected, setSelected] = React.useState<string | undefined>();
  const [savedFields, setSavedFields] = React.useState<ErpFormField[]>([]);
  const [loadingFields, setLoadingFields] = React.useState(false);
  const [saving, setSaving] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);

  const { value: fields, push: pushFields, undo, redo, canUndo, canRedo, reset: resetFields } =
    useUndoHistory<ErpFormField[]>([]);

  React.useEffect(() => {
    listTransactionTypes()
      .then((t) => {
        setTypes(t);
        const first = t.find((x) => x.code === 'FIN.CR') ?? t[0];
        if (first) setSelected(first.code);
      })
      .catch((e) => setError(e?.message ?? 'Gagal memuat daftar transaksi'));
  }, []);

  React.useEffect(() => {
    if (!selected) return;
    setLoadingFields(true);
    getFormFields(selected)
      .then((r) => {
        resetFields(r.fields);
        setSavedFields(r.fields);
      })
      .catch((e) => notify(e?.message ?? 'Gagal memuat form fields', 'danger'))
      .finally(() => setLoadingFields(false));
  }, [selected]); // eslint-disable-line react-hooks/exhaustive-deps

  React.useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      const meta = e.ctrlKey || e.metaKey;
      if (!meta) return;
      if (e.key === 'z' && !e.shiftKey) { e.preventDefault(); undo(); }
      if (e.key === 'y' || (e.key === 'z' && e.shiftKey)) { e.preventDefault(); redo(); }
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [undo, redo]);

  const selectedType = types.find((t) => t.code === selected);

  const handleAddField = () => {
    pushFields([...fields, blankCustomField(fields)]);
  };

  const handleResetDefaults = () => {
    if (!selected) return;
    setLoadingFields(true);
    // Force re-seed by clearing then re-fetching (backend lazy-seeds on empty)
    saveFormFields(selected, [])
      .then(() => getFormFields(selected!))
      .then((r) => {
        resetFields(r.fields);
        setSavedFields(r.fields);
        notify('Field dikembalikan ke default', 'success');
      })
      .catch((e) => notify(e?.message ?? 'Gagal reset', 'danger'))
      .finally(() => setLoadingFields(false));
  };

  const handleSave = async () => {
    if (!selected) return;
    setSaving(true);
    try {
      const r = await saveFormFields(selected, fields);
      resetFields(r.fields);
      setSavedFields(r.fields);
      notify('Konfigurasi form disimpan', 'success');
    } catch (e) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally {
      setSaving(false);
    }
  };

  const structuralCount = fields.filter((f) => f.kind === 'STRUCTURAL').length;
  const customCount = fields.filter((f) => f.kind === 'CUSTOM').length;

  if (error) {
    return <div className="p-4 text-sm text-danger">Gagal memuat: {error}</div>;
  }

  return (
    <div className="flex h-[calc(100vh-var(--topbar-h,96px))] overflow-hidden rounded-lg border border-border">
      <aside className="w-72 shrink-0 border-r border-border">
        <div className="border-b border-border bg-secondary px-3 py-2 text-xs font-semibold uppercase tracking-wide text-muted-foreground">
          Jenis Transaksi
        </div>
        <GridCustomizationTree types={types} selectedCode={selected} onSelect={setSelected} />
      </aside>

      <section className="flex min-w-0 flex-1 flex-col">
        <div className="flex items-center gap-1.5 border-b border-border px-4 py-2">
          <div className="flex flex-col">
            <h2 className="text-sm font-semibold">{selectedType?.name ?? 'Pilih transaksi'}</h2>
            {fields.length > 0 && (
              <span className="text-[11px] text-muted-foreground">
                {structuralCount} field sistem · {customCount} field kustom
              </span>
            )}
          </div>
          <div className="flex-1" />

          <button type="button" className="iconbtn" title="Undo (Ctrl+Z)" disabled={!canUndo} onClick={undo}>
            <Icon name="history" size={14} />
          </button>
          <button type="button" className="iconbtn" title="Redo (Ctrl+Y)" disabled={!canRedo} onClick={redo}>
            <Icon name="swap" size={14} />
          </button>

          <div className="mx-1 h-4 w-px bg-border" />

          <button
            type="button"
            className="btn"
            title="Kembalikan ke konfigurasi default"
            disabled={!selected || saving}
            onClick={handleResetDefaults}
          >
            <Icon name="refresh" size={12} /> Reset Default
          </button>
          <button type="button" className="btn" disabled={!selected} onClick={handleAddField}>
            <Icon name="plus" size={12} /> Tambah Field
          </button>
          <button type="button" className="btn primary" disabled={!selected || saving} onClick={handleSave}>
            <Icon name="save" size={13} /> Simpan
          </button>
        </div>

        <div className="flex-1 overflow-auto">
          {loadingFields ? (
            <div className="p-4 text-sm text-muted-foreground">Memuat…</div>
          ) : fields.length > 0 ? (
            <div>
              <div className="border-b border-border bg-secondary/40 px-4 py-1.5 text-[11px] text-muted-foreground">
                Field sistem (label bisa diubah; tipe & field key tetap). Field kustom bisa ditambah, diedit, dan dihapus.
              </div>
              <FormBuilderFields
                fields={fields}
                savedFields={savedFields}
                onFieldsChange={pushFields}
                onSave={handleSave}
                saving={saving}
              />
            </div>
          ) : (
            <div className="p-4 text-sm text-muted-foreground">Pilih jenis transaksi.</div>
          )}
        </div>
      </section>
    </div>
  );
}
