'use client';

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { notify } from '@/lib/feedback';
import {
  getAccountCodeFormat,
  updateAccountCodeFormat,
  type AccountCodeFormat,
} from '@/lib/api/accounts';
import { invalidateAccountCodeFormatCache } from './accounts-form';
import { AccountCodePresetList } from './account-code-format-presets';

const SEPARATOR_OPTIONS: Array<{ value: string; label: string }> = [
  { value: '.', label: 'Titik  ( . )' },
  { value: '-', label: 'Strip  ( - )' },
  { value: '/', label: 'Garis miring  ( / )' },
  { value: '', label: 'Tanpa pemisah' },
];

function buildPreview(segments: number[], separator: string): string {
  return segments
    .map((n, i) => (i === 0 ? '1'.padEnd(n, '1') : '0'.padStart(n, '0')))
    .join(separator);
}

function maxLengthOf(segments: number[], separator: string): number {
  const sum = segments.reduce((a, b) => a + b, 0);
  return sum + Math.max(0, segments.length - 1) * separator.length;
}

function arraysEqual(a: number[], b: number[]): boolean {
  return a.length === b.length && a.every((v, i) => v === b[i]);
}

export function AccountCodeFormatPage() {
  const [loaded, setLoaded] = React.useState<AccountCodeFormat | null>(null);
  const [error, setError] = React.useState<string | null>(null);
  const [segments, setSegments] = React.useState<number[]>([4, 2, 3]);
  const [separator, setSeparator] = React.useState<string>('.');
  const [saving, setSaving] = React.useState(false);

  React.useEffect(() => {
    let cancelled = false;
    getAccountCodeFormat()
      .then((f) => {
        if (cancelled) return;
        setLoaded(f);
        setSegments(f.segments);
        setSeparator(f.separator);
      })
      .catch((e: unknown) => {
        if (!cancelled) setError(e instanceof Error ? e.message : 'Gagal memuat format kode akun');
      });
    return () => {
      cancelled = true;
    };
  }, []);

  const locked = loaded?.locked ?? false;
  const preview = buildPreview(segments, separator);
  const maxLength = maxLengthOf(segments, separator);
  const dirty = loaded
    ? !arraysEqual(segments, loaded.segments) || separator !== loaded.separator
    : false;

  const setSegmentAt = (idx: number, value: number) => {
    setSegments((prev) => prev.map((v, i) => (i === idx ? value : v)));
  };

  const addSegment = () => {
    if (segments.length >= 5) return;
    setSegments([...segments, 3]);
  };

  const removeSegment = () => {
    if (segments.length <= 1) return;
    setSegments(segments.slice(0, -1));
  };

  const applyPreset = (segs: number[], sep: string) => {
    setSegments(segs);
    setSeparator(sep);
  };

  const handleSave = async () => {
    if (locked) {
      notify(`Format terkunci: ada ${loaded?.accountCount ?? 0} akun aktif. Hapus semua dulu untuk reset format.`, 'warn');
      return;
    }
    if (segments.some((n) => n < 1 || n > 12 || !Number.isInteger(n))) {
      notify('Setiap segmen harus integer 1–12 digit.', 'danger');
      return;
    }
    setSaving(true);
    try {
      const updated = await updateAccountCodeFormat(segments, separator);
      setLoaded({
        segments: updated.segments,
        separator: updated.separator,
        patternSource: updated.patternSource,
        maxLength: updated.maxLength,
        example: updated.example,
        accountCount: loaded?.accountCount ?? 0,
        locked: false,
      });
      invalidateAccountCodeFormatCache();
      notify('Format kode akun disimpan', 'success');
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan format', 'danger');
    } finally {
      setSaving(false);
    }
  };

  const handleReset = () => {
    if (!loaded) return;
    setSegments(loaded.segments);
    setSeparator(loaded.separator);
  };

  return (
    <div className="page">
      <div className="page-header">
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
          <div
            style={{
              width: 28,
              height: 28,
              borderRadius: 'var(--radius)',
              background: 'var(--primary-soft)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: 'var(--primary)',
            }}
          >
            <Icon name="calculator" size={14} />
          </div>
          <h1 className="page-title" style={{ margin: 0 }}>Format Kode Akun</h1>
        </div>
        <div className="page-actions">
          <button className="btn ghost" onClick={handleReset} disabled={!dirty || saving}>
            <Icon name="refresh" size={12} />
            Reset
          </button>
          <button
            className="btn primary"
            onClick={handleSave}
            disabled={!dirty || saving || locked}
            title={locked ? 'Terkunci: sudah ada akun' : undefined}
          >
            <Icon name="save" size={12} />
            {saving ? 'Menyimpan…' : 'Simpan'}
          </button>
        </div>
      </div>

      <div className="scrollbar" style={{ flex: 1, overflow: 'auto', padding: '20px 24px' }}>
        {error && (
          <div className="card" style={{ maxWidth: 720, marginBottom: 16, borderColor: 'var(--danger-soft, var(--border))' }}>
            <div style={{ padding: 16, color: 'var(--danger)' }}>{error}</div>
          </div>
        )}

        {locked && (
          <div
            className="card"
            style={{
              maxWidth: 720,
              marginBottom: 16,
              borderColor: 'color-mix(in srgb, var(--warning, #f59e0b) 40%, transparent)',
              background: 'color-mix(in srgb, var(--warning, #f59e0b) 8%, transparent)',
            }}
          >
            <div style={{ display: 'flex', gap: 12, padding: 16, alignItems: 'flex-start' }}>
              <Icon name="shield" size={16} />
              <div>
                <div style={{ fontWeight: 600, fontSize: 'calc(13px * var(--font-scale, 1))' }}>
                  Format terkunci — sudah ada {loaded?.accountCount} akun
                </div>
                <div style={{ fontSize: 'calc(12px * var(--font-scale, 1))', color: 'var(--fg-muted)', marginTop: 4, lineHeight: 1.55 }}>
                  Untuk mengganti format, hapus seluruh akun di <code>md_accounts</code> terlebih dahulu (Master → Accounts → Hapus semua). Mengubah format saat ada data berisiko meng-korupsi referensi <code>parentId</code> dan range akun di laporan keuangan.
                </div>
              </div>
            </div>
          </div>
        )}

        <div className="card" style={{ maxWidth: 720 }}>
          <div className="card-h">
            <span
              style={{
                display: 'inline-flex',
                width: 22,
                height: 22,
                alignItems: 'center',
                justifyContent: 'center',
                background: 'var(--primary-soft)',
                color: 'var(--primary-soft-fg)',
                borderRadius: 5,
              }}
            >
              <Icon name="calculator" size={12} />
            </span>
            <div>
              <div className="title">Komposisi segmen</div>
              <div className="sub">Tentukan panjang tiap segmen kode akun (digit) dan pemisah antar-segmen.</div>
            </div>
          </div>

          <div className="card-b" style={{ paddingTop: 8 }}>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
              <div>
                <label
                  style={{
                    fontSize: 'calc(12.5px * var(--font-scale, 1))',
                    color: 'var(--fg)',
                    display: 'block',
                    marginBottom: 6,
                  }}
                >
                  Segmen ({segments.length} segmen, total {maxLength} karakter)
                </label>
                <div style={{ display: 'flex', alignItems: 'center', gap: 6, flexWrap: 'wrap' }}>
                  {segments.map((n, i) => (
                    <React.Fragment key={i}>
                      <Input
                        type="number"
                        min={1}
                        max={12}
                        value={n}
                        onChange={(e) => setSegmentAt(i, Number(e.target.value) || 1)}
                        disabled={locked}
                        style={{ width: 72, textAlign: 'center' }}
                      />
                      {i < segments.length - 1 && (
                        <span style={{ fontFamily: 'var(--font-mono)', color: 'var(--fg-muted)' }}>
                          {separator || '·'}
                        </span>
                      )}
                    </React.Fragment>
                  ))}
                  <button
                    className="btn ghost"
                    onClick={removeSegment}
                    disabled={segments.length <= 1 || locked}
                    type="button"
                    style={{ marginLeft: 8 }}
                  >
                    <Icon name="x" size={12} />
                    Hapus segmen
                  </button>
                  <button
                    className="btn ghost"
                    onClick={addSegment}
                    disabled={segments.length >= 5 || locked}
                    type="button"
                  >
                    <Icon name="plus" size={12} />
                    Tambah segmen
                  </button>
                </div>
                <div style={{ fontSize: 'calc(11px * var(--font-scale, 1))', color: 'var(--fg-muted)', marginTop: 6 }}>
                  Maks 5 segmen, masing-masing 1–12 digit.
                </div>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '260px 1fr', gap: 16, alignItems: 'center' }}>
                <label style={{ fontSize: 'calc(12.5px * var(--font-scale, 1))', color: 'var(--fg)' }}>
                  Pemisah antar segmen
                </label>
                <Select value={separator || '__empty__'} onValueChange={(v) => setSeparator(v === '__empty__' ? '' : v)} disabled={locked}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    {SEPARATOR_OPTIONS.map((o) => (
                      <SelectItem key={o.value || '__empty__'} value={o.value || '__empty__'}>{o.label}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div
                style={{
                  background: 'var(--panel-2)',
                  border: '1px solid var(--border)',
                  borderRadius: 'var(--radius)',
                  padding: '14px 16px',
                }}
              >
                <div style={{ fontSize: 'calc(11px * var(--font-scale, 1))', color: 'var(--fg-muted)', textTransform: 'uppercase', letterSpacing: 0.5 }}>
                  Pratinjau
                </div>
                <div
                  style={{
                    fontFamily: 'var(--font-mono)',
                    fontSize: 'calc(20px * var(--font-scale, 1))',
                    fontWeight: 600,
                    color: 'var(--fg)',
                    marginTop: 4,
                  }}
                >
                  {preview}
                </div>
                <div style={{ fontSize: 'calc(11.5px * var(--font-scale, 1))', color: 'var(--fg-muted)', marginTop: 4 }}>
                  Layout {segments.join(separator || ' ')}  ·  max {maxLength} karakter
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="card" style={{ maxWidth: 720, marginTop: 16 }}>
          <div className="card-h">
            <span
              style={{
                display: 'inline-flex',
                width: 22,
                height: 22,
                alignItems: 'center',
                justifyContent: 'center',
                background: 'var(--primary-soft)',
                color: 'var(--primary-soft-fg)',
                borderRadius: 5,
              }}
            >
              <Icon name="layers" size={12} />
            </span>
            <div>
              <div className="title">Preset cepat</div>
              <div className="sub">Pilih template umum, atau gunakan editor di atas untuk format kustom.</div>
            </div>
          </div>
          <AccountCodePresetList
            segments={segments}
            separator={separator}
            disabled={locked}
            onApply={applyPreset}
          />
        </div>
      </div>

      <div className="pager">
        <span className="muted">Administrator · Format Kode Akun (sys_settings group "account-code")</span>
      </div>
    </div>
  );
}
