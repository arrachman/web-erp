'use client';

/**
 * Kustomisasi Grid (/admin/grid-customization).
 * Features: undo/redo (Ctrl+Z / Ctrl+Y), export/import JSON, clone dari menu lain,
 * bulk selection, change indicator per-baris.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { notify } from '@/lib/feedback';
import {
  listTransactionTypes,
  getTransactionGrids,
  saveTransactionGrids,
  type ErpTransactionType,
  type ErpTransactionGrid,
  type ErpGridColumn,
} from '@/lib/api/transaction-grids';
import { useUndoHistory } from '@/lib/use-undo-history';
import { GridCustomizationTree } from './grid-customization-tree';
import { GridCustomizationTabs } from './grid-customization-tabs';
import { GridCustomizationColumns } from './grid-customization-columns';
import { GridCloneDialog } from '@/components/molecules/grid-clone-dialog';

const blankColumn = (index: number): ErpGridColumn => ({
  sortOrder: index,
  headerText: 'Kolom Baru',
  dataField: `customText${index + 1}`,
  width: 120,
  isVisible: true,
  isRequired: false,
  isEditable: true,
  isSkippable: false,
  kind: 'CUSTOM',
  dataType: 'TEXT',
  lookupSource: null,
  labelFormatter: null,
  headerRenderer: null,
  cellRenderer: null,
  cellEditor: null,
});

function downloadJson(data: unknown, filename: string) {
  const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  a.click();
  URL.revokeObjectURL(url);
}

export function GridCustomizationPage() {
  const [types, setTypes] = React.useState<ErpTransactionType[]>([]);
  const [selected, setSelected] = React.useState<string | undefined>();
  const [savedGrids, setSavedGrids] = React.useState<ErpTransactionGrid[]>([]);
  const [activeKey, setActiveKey] = React.useState<string | undefined>();
  const [loadingCols, setLoadingCols] = React.useState(false);
  const [saving, setSaving] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);
  const [cloneOpen, setCloneOpen] = React.useState(false);
  const importRef = React.useRef<HTMLInputElement>(null);

  const { value: grids, push: pushGrids, undo, redo, canUndo, canRedo, reset: resetGrids } =
    useUndoHistory<ErpTransactionGrid[]>([]);

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
    setLoadingCols(true);
    getTransactionGrids(selected)
      .then((r) => {
        resetGrids(r.grids);
        setSavedGrids(r.grids);
        setActiveKey((r.grids.find((g) => g.isPrimary) ?? r.grids[0])?.key);
      })
      .catch((e) => notify(e?.message ?? 'Gagal memuat grid', 'danger'))
      .finally(() => setLoadingCols(false));
  }, [selected]); // eslint-disable-line react-hooks/exhaustive-deps

  React.useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      const meta = e.ctrlKey || e.metaKey;
      if (!meta) return;
      if (e.key === 'z' && !e.shiftKey) { e.preventDefault(); undo(); }
      if ((e.key === 'y') || (e.key === 'z' && e.shiftKey)) { e.preventDefault(); redo(); }
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [undo, redo]);

  const selectedType = types.find((t) => t.code === selected);
  const activeGrid = grids.find((g) => g.key === activeKey) ?? grids[0];
  const savedActiveGrid = savedGrids.find((g) => g.key === activeGrid?.key);

  const setActiveColumns = (cols: ErpGridColumn[]) =>
    pushGrids(grids.map((g) => (g.key === activeGrid?.key ? { ...g, columns: cols } : g)));

  const addColumn = () => {
    if (!activeGrid) return;
    setActiveColumns([...activeGrid.columns, blankColumn(activeGrid.columns.length)]);
  };

  const handleSave = async () => {
    if (!selected) return;
    setSaving(true);
    try {
      const r = await saveTransactionGrids(selected, grids);
      resetGrids(r.grids);
      setSavedGrids(r.grids);
      setActiveKey((prev) => (r.grids.some((g) => g.key === prev) ? prev : (r.grids.find((g) => g.isPrimary) ?? r.grids[0])?.key));
      notify('Layout grid disimpan', 'success');
    } catch (e) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally {
      setSaving(false);
    }
  };

  const handleExport = () => {
    if (!activeGrid || !selected) return;
    downloadJson(activeGrid.columns, `grid-${selected}-${activeGrid.key}.json`);
    notify('Config diekspor sebagai JSON', 'success');
  };

  const handleImport = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = (ev) => {
      try {
        const parsed = JSON.parse(ev.target?.result as string) as ErpGridColumn[];
        if (!Array.isArray(parsed)) throw new Error('Bukan array');
        setActiveColumns(parsed.map((c, i) => ({ ...c, sortOrder: i })));
        notify(`${parsed.length} kolom diimpor`, 'success');
      } catch {
        notify('File JSON tidak valid', 'danger');
      }
      if (importRef.current) importRef.current.value = '';
    };
    reader.readAsText(file);
  };

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
          <h2 className="text-sm font-semibold">{selectedType?.name ?? 'Pilih transaksi'}</h2>
          <div className="flex-1" />

          <button
            type="button"
            className="iconbtn"
            title="Undo (Ctrl+Z)"
            disabled={!canUndo}
            onClick={undo}
          >
            <Icon name="history" size={14} />
          </button>
          <button
            type="button"
            className="iconbtn"
            title="Redo (Ctrl+Y)"
            disabled={!canRedo}
            onClick={redo}
          >
            <Icon name="swap" size={14} />
          </button>

          <div className="mx-1 h-4 w-px bg-border" />

          <button
            type="button"
            className="btn"
            title="Salin kolom dari transaksi lain"
            disabled={!selected}
            onClick={() => setCloneOpen(true)}
          >
            <Icon name="layers" size={12} /> Salin dari…
          </button>
          <button
            type="button"
            className="btn"
            title="Export konfigurasi kolom aktif sebagai JSON"
            disabled={!activeGrid}
            onClick={handleExport}
          >
            <Icon name="download" size={12} /> Export
          </button>
          <label className="btn" title="Import konfigurasi kolom dari JSON" style={{ cursor: 'pointer' }}>
            <Icon name="upload" size={12} /> Import
            <input
              ref={importRef}
              type="file"
              accept=".json"
              className="sr-only"
              onChange={handleImport}
            />
          </label>

          <div className="mx-1 h-4 w-px bg-border" />

          <button type="button" className="btn" disabled={!activeGrid} onClick={addColumn}>
            <Icon name="plus" size={12} /> Tambah Kolom
          </button>
          <button type="button" className="btn primary" disabled={!selected || saving} onClick={handleSave}>
            <Icon name="save" size={13} /> Simpan
          </button>
        </div>

        {!loadingCols && grids.length > 0 && (
          <GridCustomizationTabs
            grids={grids}
            activeKey={activeKey}
            onSelect={setActiveKey}
            onGridsChange={pushGrids}
          />
        )}

        <div className="flex-1 overflow-auto">
          {loadingCols ? (
            <div className="p-4 text-sm text-muted-foreground">Memuat…</div>
          ) : activeGrid ? (
            <GridCustomizationColumns
              columns={activeGrid.columns}
              savedColumns={savedActiveGrid?.columns ?? []}
              onColumnsChange={setActiveColumns}
            />
          ) : (
            <div className="p-4 text-sm text-muted-foreground">Pilih jenis transaksi.</div>
          )}
        </div>
      </section>

      {selected && (
        <GridCloneDialog
          open={cloneOpen}
          onClose={() => setCloneOpen(false)}
          currentCode={selected}
          onApply={setActiveColumns}
        />
      )}
    </div>
  );
}
