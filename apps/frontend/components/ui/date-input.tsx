'use client';

import * as React from 'react';
import * as Popover from '@radix-ui/react-popover';
import { DayPicker } from 'react-day-picker';
import { id as idLocale } from 'react-day-picker/locale';
import 'react-day-picker/style.css';
import { cn } from '@/lib/utils';
import {
  calendarNavBounds,
  formatDate,
  parseDisplayDate,
  parseIsoDate,
  toIsoDate,
  useDateFormat,
} from '@/lib/date-format';

export interface DateInputProps {
  /** Canonical ISO date string (YYYY-MM-DD), or '' when empty. */
  value: string;
  /** Called with the new ISO date string ('' when cleared). */
  onChange: (iso: string) => void;
  id?: string;
  name?: string;
  disabled?: boolean;
  /** Soft empty-state text. */
  placeholder?: string;
  'aria-invalid'?: boolean | 'true' | 'false';
  className?: string;
}

/**
 * Single-date field — replaces native <input type="date">.
 * The field is a free-text input: the user can TYPE the date directly (parsed
 * liberally per the active display format, see parseDisplayDate) or open the
 * Radix Popover day-picker via the calendar icon. Empty state shows a friendly
 * placeholder (no browser dd/mm/yyyy); filled state shows the date formatted
 * per sys_settings (lib/date-format.ts).
 */
export function DateInput({
  value,
  onChange,
  id,
  name,
  disabled,
  placeholder = 'Pilih tanggal',
  className,
  ...rest
}: DateInputProps) {
  const fmt = useDateFormat();
  const navBounds = React.useMemo(calendarNavBounds, []);
  const [open, setOpen] = React.useState(false);
  const [month, setMonth] = React.useState<Date>(() => parseIsoDate(value) ?? new Date());
  const invalid = rest['aria-invalid'] === true || rest['aria-invalid'] === 'true';

  // Local draft text so the user can type freely; committed on blur/Enter.
  const [draft, setDraft] = React.useState<string | null>(null);
  const display = formatDate(value, fmt);
  const text = draft ?? display;

  // Restrict typed chars to digits + the separators of the active format
  // (so DD/MM/YYYY allows only digits and "/"). Letters/spaces stay allowed
  // only when the format uses a month name (MMM/MMMM, e.g. "5 Mei 2026").
  const sanitize = React.useCallback(
    (raw: string) => {
      const token = fmt.format;
      const seps = Array.from(new Set(token.replace(/[A-Za-z]/g, '')))
        .map((c) => c.replace(/[.*+?^${}()|[\]\\]/g, '\\$&'))
        .join('');
      const cls = /MMM/.test(token) ? `\\dA-Za-z\\s${seps}` : `\\d${seps}`;
      return raw.replace(new RegExp(`[^${cls}]`, 'g'), '');
    },
    [fmt.format],
  );

  function handleOpenChange(next: boolean) {
    if (disabled) return;
    if (next) setMonth(parseIsoDate(value) ?? new Date());
    setOpen(next);
  }

  function handleSelect(date: Date | undefined) {
    setDraft(null);
    onChange(toIsoDate(date));
    setOpen(false);
  }

  /** Parse the typed draft and commit it (or revert to the last valid value). */
  function commitDraft() {
    if (draft === null) return;
    const iso = parseDisplayDate(draft, fmt);
    if (iso !== null) {
      // '' clears, a valid ISO sets — either way, accept it.
      if (iso !== value) onChange(iso);
    }
    // Invalid (null) → drop the draft and fall back to the formatted value.
    setDraft(null);
  }

  const selected = parseIsoDate(value);

  return (
    <Popover.Root open={open} onOpenChange={handleOpenChange}>
      <div
        className={cn(
          'flex h-[26px] w-full min-w-0 items-center gap-1.5 rounded-md border border-border bg-card px-2 text-[12.5px] text-foreground transition-[border-color,box-shadow] duration-75',
          'focus-within:border-primary focus-within:shadow-[0_0_0_2px_color-mix(in_oklab,var(--primary)_22%,transparent)]',
          'hover:border-[var(--fg-subtle)]',
          disabled && 'cursor-not-allowed opacity-45',
          invalid && 'border-danger',
          open && 'border-primary shadow-[0_0_0_2px_color-mix(in_oklab,var(--primary)_22%,transparent)]',
          className,
        )}
      >
        <input
          type="text"
          inputMode="numeric"
          id={id}
          name={name}
          disabled={disabled}
          autoComplete="off"
          aria-invalid={rest['aria-invalid']}
          placeholder={placeholder}
          value={text}
          onChange={(e) => setDraft(sanitize(e.target.value))}
          onBlur={commitDraft}
          onKeyDown={(e) => {
            if (e.key === 'Enter') {
              e.preventDefault();
              commitDraft();
            } else if (e.key === 'Escape' && draft !== null) {
              e.preventDefault();
              setDraft(null);
            }
          }}
          className="min-w-0 flex-1 bg-transparent outline-none placeholder:text-[var(--fg-subtle)] disabled:cursor-not-allowed"
        />

        {display && !disabled && (
          <button
            type="button"
            tabIndex={-1}
            aria-label="Hapus tanggal"
            onClick={() => {
              setDraft(null);
              onChange('');
            }}
            className="flex shrink-0 items-center text-[var(--fg-subtle)] hover:text-foreground"
          >
            <XIcon />
          </button>
        )}

        <Popover.Trigger asChild>
          <button
            type="button"
            tabIndex={-1}
            disabled={disabled}
            aria-label="Buka kalender"
            className="flex shrink-0 items-center text-[var(--fg-muted)] hover:text-foreground disabled:cursor-not-allowed"
          >
            <CalendarIcon />
          </button>
        </Popover.Trigger>
      </div>

      <Popover.Portal>
        <Popover.Content
          align="start"
          sideOffset={4}
          style={{
            background: 'var(--panel)',
            border: '1px solid var(--border)',
            borderRadius: 8,
            boxShadow: 'var(--shadow-flyout)',
            padding: 12,
            zIndex: 9999,
            color: 'var(--fg)',
          }}
        >
          <DayPicker
            mode="single"
            selected={selected}
            onSelect={handleSelect}
            month={month}
            onMonthChange={setMonth}
            captionLayout="dropdown"
            startMonth={navBounds.startMonth}
            endMonth={navBounds.endMonth}
            locale={idLocale}
          />
        </Popover.Content>
      </Popover.Portal>
    </Popover.Root>
  );
}

function CalendarIcon() {
  return (
    <svg width="14" height="14" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.5">
      <rect x="1" y="2.5" width="14" height="12" rx="2" />
      <path d="M1 6.5h14M5 1v3M11 1v3" />
    </svg>
  );
}

function XIcon() {
  return (
    <svg width="12" height="12" viewBox="0 0 12 12" fill="none" stroke="currentColor" strokeWidth="1.8">
      <path d="M2 2l8 8M10 2l-8 8" />
    </svg>
  );
}
