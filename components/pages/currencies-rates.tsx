'use client';

/**
 * Currency rates sub-panel — embedded inside the currency edit modal.
 * Lists existing dated rates and lets the user add rates for a date range.
 *
 * "Periode berlaku" (Mulai → Selesai) di-expand jadi **satu baris per hari**
 * (upsert `rateDate` + nilai rate yang sama). Tanpa tanggal Selesai = 1 hari.
 *
 * Atomic tier: Organism sub-part.
 */

import * as React from 'react';
import { addDays, differenceInCalendarDays } from 'date-fns';
import { DateRangePicker } from '@/components/ui/date-range-picker';
import { FormField } from '@/components/ui/form-field';
import { NumInput } from '@/components/molecules/num-input';
import {
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableHead,
  TableCell,
  TableEmpty,
} from '@/components/organisms/table';
import { notify } from '@/lib/feedback';
import { formatDate, parseIsoDate, toIsoDate } from '@/lib/date-format';
import { formatNumber } from '@/lib/format';
import {
  listCurrencyRates,
  addCurrencyRate,
} from '@/lib/api/currencies';
import type { ErpCurrencyRate } from '@/lib/api/currencies';

interface AddForm {
  from: string;
  to: string;
  rate: string;
}

/** Soft cap so a mis-typed multi-year range cannot fire thousands of POSTs. */
const MAX_RATE_DAYS = 366;
const LIST_LIMIT = 200;

const emptyAddForm = (): AddForm => ({ from: '', to: '', rate: '' });

/**
 * Inclusive list of ISO dates from `from` through `to` (or just `from` if
 * `to` is empty). Returns null when dates are invalid / inverted.
 */
export function expandRateDates(fromIso: string, toIso: string): string[] | null {
  const start = parseIsoDate(fromIso);
  if (!start) return null;
  const end = toIso ? parseIsoDate(toIso) : start;
  if (!end) return null;
  if (end < start) return null;

  const span = differenceInCalendarDays(end, start) + 1;
  if (span > MAX_RATE_DAYS) return null;

  const days: string[] = [];
  for (let i = 0; i < span; i += 1) {
    days.push(toIsoDate(addDays(start, i)));
  }
  return days;
}

export function CurrencyRatesPanel({ currencyId }: { currencyId: string }) {
  const [rates, setRates] = React.useState<ErpCurrencyRate[]>([]);
  const [loading, setLoading] = React.useState(false);
  const [form, setForm] = React.useState<AddForm>(emptyAddForm);
  const [adding, setAdding] = React.useState(false);
  const [progress, setProgress] = React.useState<string | null>(null);

  const load = React.useCallback(async () => {
    setLoading(true);
    try {
      const res = await listCurrencyRates(currencyId, { limit: LIST_LIMIT });
      setRates(res.data);
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal memuat rate', 'danger');
    } finally {
      setLoading(false);
    }
  }, [currencyId]);

  React.useEffect(() => {
    const task = window.setTimeout(() => {
      void load();
    }, 0);
    return () => window.clearTimeout(task);
  }, [load]);

  const handleAdd = async () => {
    if (!form.from || !form.rate) {
      notify('Tanggal mulai dan nilai rate wajib diisi', 'danger');
      return;
    }
    const days = expandRateDates(form.from, form.to);
    if (!days) {
      if (form.to && parseIsoDate(form.to)! < parseIsoDate(form.from)!) {
        notify('Tanggal selesai tidak boleh sebelum tanggal mulai', 'danger');
      } else {
        notify(
          `Rentang maksimal ${MAX_RATE_DAYS} hari. Periksa periode berlaku.`,
          'danger',
        );
      }
      return;
    }

    setAdding(true);
    setProgress(null);
    let ok = 0;
    try {
      // Sequential upserts — unique (currencyId, rateDate); keeps load gentle.
      for (let i = 0; i < days.length; i += 1) {
        setProgress(`${i + 1}/${days.length}`);
        await addCurrencyRate(currencyId, { rateDate: days[i], rate: form.rate });
        ok += 1;
      }
      notify(
        days.length === 1
          ? 'Rate ditambahkan'
          : `${ok} baris rate ditambahkan (${formatDate(days[0])} → ${formatDate(days[days.length - 1])})`,
        'success',
      );
      setForm(emptyAddForm());
      await load();
    } catch (e: unknown) {
      notify(
        e instanceof Error
          ? ok > 0
            ? `${ok}/${days.length} tersimpan, lalu gagal: ${e.message}`
            : e.message
          : 'Gagal menambah rate',
        'danger',
      );
      if (ok > 0) await load();
    } finally {
      setAdding(false);
      setProgress(null);
    }
  };

  return (
    <div>
      <FormField
        label="Periode"
        htmlFor="cr-from"
        required
        help="1 baris/hari di rentang · kosongkan Selesai = 1 hari"
      >
        <DateRangePicker
          id="cr-from"
          from={form.from}
          to={form.to}
          onChangeFrom={(v) => setForm((f) => ({ ...f, from: v }))}
          onChangeTo={(v) => setForm((f) => ({ ...f, to: v }))}
        />
      </FormField>
      <FormField
        label="Kurs"
        htmlFor="cr-rate"
        required
        controlAddon={
          <button
            type="button"
            className="btn primary sm ml-2 shrink-0"
            onClick={handleAdd}
            disabled={adding}
          >
            {adding ? (progress ? `… ${progress}` : '…') : 'Tambah'}
          </button>
        }
      >
        <NumInput
          id="cr-rate"
          value={form.rate}
          onChange={(raw) => setForm((f) => ({ ...f, rate: raw }))}
          placeholder="15.750,00"
          className="max-w-[10rem]"
        />
      </FormField>

      <div className="mt-2">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Tanggal</TableHead>
              <TableHead className="text-right">Rate</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={2} className="muted">
                  Memuat...
                </TableCell>
              </TableRow>
            ) : rates.length === 0 ? (
              <TableEmpty colSpan={2} />
            ) : (
              rates.map((r) => (
                <TableRow key={r.id} className={r.isActive ? undefined : 'opacity-50'}>
                  <TableCell>{formatDate(r.rateDate) || '—'}</TableCell>
                  <TableCell className="text-right tabular-nums">
                    {formatNumber(r.rate, 2)}
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}