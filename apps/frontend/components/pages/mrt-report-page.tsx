'use client';

/**
 * Generic .mrt report page (Wave G1) — one page serves every module's
 * Reports hub: a report-type combo box (Registry API), a dynamic
 * parameter form, an HTML preview of the single pagination model, and
 * PDF/Word/Excel exports of that same model.
 *
 * Atomic tier: Page.
 */

import * as React from 'react';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Icon } from '@/components/ui/icons';
import { ReportExportBar } from '@/components/molecules/report-export-bar';
import { ReportTypeSelect } from '@/components/organisms/report-type-select';
import { ReportParamForm } from '@/components/organisms/report-param-form';
import {
  downloadBlob,
  getRegistryReport,
  getRegistryReports,
  renderRegistryReport,
  type RegistryReportDetail,
  type RegistryReportItem,
} from '@/lib/api/report-registry';
import type { ExportFormat } from '@/lib/api/fin-reports';

export interface MrtReportPageProps {
  /** Legacy module code for the registry feed, e.g. 'M1'. */
  module: string;
  title: string;
  /** Short code shown next to the title, e.g. `master/reports`. */
  code: string;
}

export function MrtReportPage({ module, title, code }: MrtReportPageProps) {
  const [items, setItems] = React.useState<RegistryReportItem[]>([]);
  const [selected, setSelected] = React.useState('');
  const [detail, setDetail] = React.useState<RegistryReportDetail | null>(null);
  const [params, setParams] = React.useState<Record<string, string>>({});
  const [previewHtml, setPreviewHtml] = React.useState('');
  const [xlsxDataMode, setXlsxDataMode] = React.useState(false);
  const [busy, setBusy] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);

  React.useEffect(() => {
    let cancelled = false;
    getRegistryReports(module)
      .then((rows) => {
        if (cancelled) return;
        setItems(rows);
        const firstRenderable = rows.find((r) => r.translationStatus !== 'PENDING') ?? rows[0];
        if (firstRenderable) setSelected(firstRenderable.code);
      })
      .catch((e) => !cancelled && setError(e instanceof Error ? e.message : 'Gagal memuat daftar laporan'));
    return () => {
      cancelled = true;
    };
  }, [module]);

  React.useEffect(() => {
    if (!selected) return;
    let cancelled = false;
    setDetail(null);
    setParams({});
    setPreviewHtml('');
    getRegistryReport(selected)
      .then((d) => !cancelled && setDetail(d))
      .catch((e) => !cancelled && setError(e instanceof Error ? e.message : 'Gagal memuat detail laporan'));
    return () => {
      cancelled = true;
    };
  }, [selected]);

  const cleanParams = React.useMemo(() => {
    const out: Record<string, unknown> = {};
    for (const [k, v] of Object.entries(params)) if (v !== '') out[k] = v;
    return out;
  }, [params]);

  const handlePreview = React.useCallback(async () => {
    if (!selected) return;
    setBusy(true);
    setError(null);
    try {
      const { blob } = await renderRegistryReport(selected, cleanParams, 'html');
      setPreviewHtml(await blob.text());
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Gagal merender pratinjau');
    } finally {
      setBusy(false);
    }
  }, [selected, cleanParams]);

  const handleExport = React.useCallback(
    async (format: ExportFormat) => {
      if (!selected) return;
      setBusy(true);
      setError(null);
      try {
        const { blob } = await renderRegistryReport(
          selected,
          cleanParams,
          format,
          format === 'xlsx' && xlsxDataMode ? 'data' : 'layout',
        );
        downloadBlob(blob, `${selected.toLowerCase()}.${format}`);
      } catch (e) {
        setError(e instanceof Error ? e.message : 'Gagal mengekspor laporan');
      } finally {
        setBusy(false);
      }
    },
    [selected, cleanParams, xlsxDataMode],
  );

  const statusLabel =
    detail?.translationStatus === 'VERIFIED'
      ? 'Terverifikasi'
      : detail?.translationStatus === 'CONVERTED'
        ? 'Terkonversi (belum verifikasi sampel)'
        : 'Belum dikonversi';

  return (
    <div className="flex flex-col gap-4 p-4">
      <div className="flex items-baseline gap-3">
        <h1 className="text-lg font-semibold">{title}</h1>
        <span className="text-xs text-muted-foreground">{code}</span>
      </div>

      <Card className="flex flex-col gap-3 p-4">
        <div className="flex flex-wrap items-end gap-3">
          <div className="flex flex-col gap-1">
            <span className="text-xs text-muted-foreground">Jenis Laporan</span>
            <ReportTypeSelect items={items} value={selected} onChange={setSelected} disabled={busy} />
          </div>
          {detail ? <span className="pb-2 text-xs text-muted-foreground">{statusLabel}</span> : null}
        </div>
        {detail ? (
          <ReportParamForm schema={detail.paramSchema} values={params} onChange={setParams} disabled={busy} />
        ) : null}
        <div className="flex flex-wrap items-center gap-2">
          <Button size="sm" onClick={handlePreview} disabled={busy || !selected}>
            <Icon name="file" size={12} />
            {busy ? 'Memproses…' : 'Pratinjau'}
          </Button>
          <ReportExportBar onExport={handleExport} busy={busy || !selected} />
          <label className="flex items-center gap-1.5 text-xs text-muted-foreground">
            <input
              type="checkbox"
              checked={xlsxDataMode}
              onChange={(e) => setXlsxDataMode(e.target.checked)}
            />
            Excel: mode data mentah
          </label>
        </div>
        {error ? <p className="text-sm text-red-600">{error}</p> : null}
      </Card>

      <Card className="min-h-[480px] overflow-hidden p-0">
        {previewHtml ? (
          <iframe title="Pratinjau laporan" srcDoc={previewHtml} className="h-[720px] w-full border-0" />
        ) : (
          <div className="flex h-[480px] items-center justify-center text-sm text-muted-foreground">
            Pilih jenis laporan lalu klik Pratinjau
          </div>
        )}
      </Card>
    </div>
  );
}
