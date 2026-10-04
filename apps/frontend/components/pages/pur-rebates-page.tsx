'use client';

/**
 * D2 — Rabat Supplier: publisher/supplier rebate agreements (percent of
 * net posted purchases per year, optionally per item category) plus the
 * accrued-rebate summary for a chosen year. Master CRUD on
 * SimpleMasterPage; accrual card is informational (no GL posting).
 */

import * as React from 'react';
import { SimpleMasterPage, type ExtraColumn } from '@/components/organisms/simple-master-page';
import type { FormErrors } from '@/lib/form-validation';
import type { PaginationParams } from '@/lib/api/types';
import { listPartners } from '@/lib/api/partners';
import { listItemCategories } from '@/lib/api/item-categories';
import {
  bulkDeleteRebates,
  bulkRebateStatus,
  createRebate,
  deleteRebate,
  getRebateAccrual,
  listRebates,
  updateRebate,
  type PurRebate,
  type PurRebateAccrual,
} from '@/lib/api/pur-rebates';

const inputCls =
  'w-full rounded-md border border-input bg-background px-3 py-2 text-sm shadow-sm focus:outline-none focus:ring-2 focus:ring-ring';
const labelCls = 'mb-1 block text-xs font-medium text-muted-foreground';
const rp = (v: number) =>
  new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR', maximumFractionDigits: 0 }).format(v);

type RebateRow = Omit<PurRebate, 'id'> & { id: string; code: string; name: string };
const toRow = (r: PurRebate): RebateRow => ({
  ...r,
  id: String(r.id),
  code: r.supplier?.code ?? '-',
  name: r.supplier?.name ?? '-',
});

interface RebateForm {
  supplierId: string;
  categoryId: string;
  percent: string;
  periodYear: string;
  notes: string;
}

function RebateFormFields({ data, onChange }: { data: RebateForm; onChange: (d: RebateForm) => void }) {
  const [suppliers, setSuppliers] = React.useState<{ id: string; label: string }[]>([]);
  const [categories, setCategories] = React.useState<{ id: string; label: string }[]>([]);
  React.useEffect(() => {
    let alive = true;
    listPartners({ page: 1, limit: 200 } as PaginationParams)
      .then((res) => {
        if (alive) setSuppliers(res.data.map((p) => ({ id: String(p.id), label: `${p.code} — ${p.name}` })));
      })
      .catch(() => undefined);
    listItemCategories({ page: 1, limit: 200 } as PaginationParams)
      .then((res) => {
        if (alive) setCategories(res.data.map((c) => ({ id: String(c.id), label: `${c.code} — ${c.name}` })));
      })
      .catch(() => undefined);
    return () => { alive = false; };
  }, []);
  const set = (k: keyof RebateForm, v: string) => onChange({ ...data, [k]: v });
  return (
    <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
      <div>
        <label className={labelCls} htmlFor="rb-supplier">Supplier / Penerbit *</label>
        <select id="rb-supplier" className={inputCls} value={data.supplierId} onChange={(e) => set('supplierId', e.target.value)}>
          <option value="">— Pilih supplier —</option>
          {suppliers.map((s) => <option key={s.id} value={s.id}>{s.label}</option>)}
        </select>
      </div>
      <div>
        <label className={labelCls} htmlFor="rb-category">Kategori Item</label>
        <select id="rb-category" className={inputCls} value={data.categoryId} onChange={(e) => set('categoryId', e.target.value)}>
          <option value="">Semua kategori</option>
          {categories.map((c) => <option key={c.id} value={c.id}>{c.label}</option>)}
        </select>
      </div>
      <div>
        <label className={labelCls} htmlFor="rb-percent">Rabat (%) *</label>
        <input id="rb-percent" type="number" min={0} max={100} step="0.01" className={inputCls}
          value={data.percent} onChange={(e) => set('percent', e.target.value)} placeholder="cth: 25" />
      </div>
      <div>
        <label className={labelCls} htmlFor="rb-year">Tahun Periode *</label>
        <input id="rb-year" type="number" min={2000} max={2100} className={inputCls}
          value={data.periodYear} onChange={(e) => set('periodYear', e.target.value)} />
      </div>
      <div className="md:col-span-2">
        <label className={labelCls} htmlFor="rb-notes">Catatan</label>
        <input id="rb-notes" className={inputCls} value={data.notes} onChange={(e) => set('notes', e.target.value)}
          placeholder="cth: Rabat tahunan kontrak 2026" />
      </div>
    </div>
  );
}

function RebateAccrualCard() {
  const thisYear = new Date().getFullYear();
  const [year, setYear] = React.useState(thisYear);
  const [accrual, setAccrual] = React.useState<PurRebateAccrual | null>(null);
  React.useEffect(() => {
    let alive = true;
    getRebateAccrual(year).then((res) => { if (alive) setAccrual(res); }).catch(() => { if (alive) setAccrual(null); });
    return () => { alive = false; };
  }, [year]);
  return (
    <div className="rounded-lg border bg-card p-4">
      <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
        <div>
          <div className="text-sm font-semibold">Akrual Rabat</div>
          <div className="text-xs text-muted-foreground">
            Persen × pembelian bersih ter-posting per perjanjian. Informatif — belum dijurnal.
          </div>
        </div>
        <select className={inputCls + ' w-32'} value={year} onChange={(e) => setYear(Number(e.target.value))}>
          {[thisYear - 1, thisYear, thisYear + 1].map((y) => <option key={y} value={y}>{y}</option>)}
        </select>
      </div>
      {!accrual || accrual.data.length === 0 ? (
        <p className="text-sm text-muted-foreground">Tidak ada perjanjian rabat aktif untuk tahun {year}.</p>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b text-left text-xs text-muted-foreground">
                <th className="py-2 pr-3">Supplier</th>
                <th className="py-2 pr-3">Kategori</th>
                <th className="py-2 pr-3 text-right">%</th>
                <th className="py-2 pr-3 text-right">Basis Pembelian</th>
                <th className="py-2 text-right">Akrual</th>
              </tr>
            </thead>
            <tbody>
              {accrual.data.map((r) => (
                <tr key={r.id} className="border-b last:border-0">
                  <td className="py-2 pr-3">{r.supplier?.name ?? '-'}</td>
                  <td className="py-2 pr-3">{r.category?.name ?? 'Semua'}</td>
                  <td className="py-2 pr-3 text-right">{r.percent}</td>
                  <td className="py-2 pr-3 text-right">{rp(r.baseAmount)}</td>
                  <td className="py-2 text-right font-medium">{rp(r.accruedAmount)}</td>
                </tr>
              ))}
              <tr>
                <td colSpan={4} className="py-2 pr-3 text-right font-semibold">Total Akrual {year}</td>
                <td className="py-2 text-right font-semibold">{rp(accrual.totalAccrued)}</td>
              </tr>
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

const CURRENT_YEAR = new Date().getFullYear();
const YEAR_OPTIONS = ['', String(CURRENT_YEAR - 1), String(CURRENT_YEAR), String(CURRENT_YEAR + 1)];

export function ErpPurRebatesPage() {
  const [year, setYear] = React.useState('');
  const list = React.useCallback(
    async (p: PaginationParams) => {
      const res = await listRebates({ ...p, ...(year ? { periodYear: Number(year) } : {}) });
      return { ...res, data: res.data.map(toRow) };
    },
    [year],
  );
  const extraColumns: ExtraColumn<RebateRow>[] = [
    { key: 'category', label: 'Kategori', render: (r) => r.category?.name ?? 'Semua Kategori' },
    { key: 'percent', label: 'Rabat %', render: (r) => `${r.percent}%` },
    { key: 'periodYear', label: 'Tahun', render: (r) => r.periodYear },
    { key: 'notes', label: 'Catatan', render: (r) => r.notes ?? '—' },
  ];
  return (
    <div className="flex flex-col gap-4">
      <RebateAccrualCard />
      <SimpleMasterPage<RebateRow, RebateForm>
        title="Rabat Supplier"
        code="PUR.RABAT"
        entityLabel="perjanjian rabat"
        storageKey="pur-rebates"
        auditEntityName="PurSupplierRebate"
        list={list}
        create={async (payload) => toRow(await createRebate(payload))}
        update={async (id, payload) => toRow(await updateRebate(id, payload))}
        remove={deleteRebate}
        bulkStatus={bulkRebateStatus}
        bulkDelete={bulkDeleteRebates}
        defaultForm={() => ({
          supplierId: '', categoryId: '', percent: '',
          periodYear: String(CURRENT_YEAR), notes: '',
        })}
        fromRecord={(row) => ({
          supplierId: String(row.supplierId),
          categoryId: row.categoryId ? String(row.categoryId) : '',
          percent: String(row.percent),
          periodYear: String(row.periodYear),
          notes: row.notes ?? '',
        })}
        toPayload={(f) => ({
          supplierId: Number(f.supplierId),
          categoryId: f.categoryId ? Number(f.categoryId) : undefined,
          percent: Number(f.percent),
          periodYear: Number(f.periodYear),
          notes: f.notes || undefined,
        })}
        validate={(f) => {
          const errors: FormErrors<RebateForm> = {};
          if (!f.supplierId) errors.supplierId = 'Supplier wajib dipilih';
          const pct = Number(f.percent);
          if (f.percent === '' || Number.isNaN(pct) || pct < 0 || pct > 100) errors.percent = 'Rabat harus 0–100';
          const y = Number(f.periodYear);
          if (!y || y < 2000 || y > 2100) errors.periodYear = 'Tahun tidak valid';
          return errors;
        }}
        FormFields={RebateFormFields}
        extraColumns={extraColumns}
        extraFilters={[
          {
            key: 'periodYear', label: 'Tahun', defaultValue: '',
            options: YEAR_OPTIONS.map((y) => ({ label: y === '' ? 'Semua' : y, value: y })),
          },
        ]}
        onExtraFilterChange={(v) => setYear(v.periodYear ?? '')}
        defaultSortBy="periodYear"
        defaultSortDir="desc"
      />
    </div>
  );
}
