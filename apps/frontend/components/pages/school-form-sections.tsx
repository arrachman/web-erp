'use client';

/**
 * A1 CRM Sekolah — form sub-sections: kontak (dengan peran standar) dan
 * aktivitas (riwayat kunjungan/negosiasi + input aktivitas baru).
 * Dipakai oleh school-form-fields.tsx agar tiap file < 400 baris.
 */

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { DateInput } from '@/components/ui/date-input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import type { ErpSchoolActivity } from '@/lib/api/schools';

export interface SchoolContactForm {
  id?: string;
  name: string;
  role: string;
  title: string;
  phone: string;
  email: string;
  isDefault: boolean;
}

export const CONTACT_ROLE_OPTIONS = [
  { value: 'KEPALA_SEKOLAH', label: 'Kepala Sekolah' },
  { value: 'BENDAHARA', label: 'Bendahara' },
  { value: 'OPERATOR', label: 'Operator' },
  { value: 'TU', label: 'Tata Usaha' },
  { value: 'OTHER', label: 'Lainnya' },
];

export const CONTACT_ROLE_LABELS: Record<string, string> = Object.fromEntries(
  CONTACT_ROLE_OPTIONS.map((o) => [o.value, o.label]),
);

export const ACTIVITY_TYPE_OPTIONS = [
  { value: 'VISIT', label: 'Kunjungan' },
  { value: 'NEGOTIATION', label: 'Negosiasi' },
  { value: 'NOTE', label: 'Catatan' },
];

export const ACTIVITY_TYPE_LABELS: Record<string, string> = Object.fromEntries(
  ACTIVITY_TYPE_OPTIONS.map((o) => [o.value, o.label]),
);

const NONE = '_none';

export function EnumSelect({
  id,
  value,
  onChange,
  options,
  placeholder = '— Pilih —',
}: {
  id?: string;
  value: string;
  onChange: (v: string) => void;
  options: { value: string; label: string }[];
  placeholder?: string;
}) {
  return (
    <Select value={value || NONE} onValueChange={(v) => onChange(v === NONE ? '' : v)}>
      <SelectTrigger id={id} className="w-full">
        <SelectValue placeholder={placeholder} />
      </SelectTrigger>
      <SelectContent>
        <SelectItem value={NONE}>{placeholder}</SelectItem>
        {options.map((o) => (
          <SelectItem key={o.value} value={o.value}>
            {o.label}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}

// ─── Kontak ───────────────────────────────────────────────────────────────────

export function SchoolContactsFields({
  contacts,
  onChange,
}: {
  contacts: SchoolContactForm[];
  onChange: (contacts: SchoolContactForm[]) => void;
}) {
  const setRow = (idx: number, patch: Partial<SchoolContactForm>) =>
    onChange(contacts.map((c, i) => (i === idx ? { ...c, ...patch } : c)));
  const removeRow = (idx: number) => onChange(contacts.filter((_, i) => i !== idx));
  const addRow = () =>
    onChange([...contacts, { name: '', role: '', title: '', phone: '', email: '', isDefault: false }]);

  return (
    <div>
      {contacts.length === 0 && (
        <p className="py-1 text-[11px] text-[var(--fg-subtle)]">
          Belum ada kontak. Tambahkan kepala sekolah, bendahara, operator, atau TU.
        </p>
      )}
      {contacts.map((c, idx) => (
        <div key={c.id ?? `new-${idx}`} className="mb-2 rounded border border-[var(--border)] p-2">
          <div className="grid grid-cols-2 gap-x-3">
            <FormField label="Nama" htmlFor={`ct-name-${idx}`} required>
              <Input
                id={`ct-name-${idx}`}
                value={c.name}
                onChange={(e) => setRow(idx, { name: e.target.value })}
                placeholder="Nama kontak"
              />
            </FormField>
            <FormField label="Peran" htmlFor={`ct-role-${idx}`}>
              <EnumSelect
                id={`ct-role-${idx}`}
                value={c.role}
                onChange={(v) => setRow(idx, { role: v })}
                options={CONTACT_ROLE_OPTIONS}
              />
            </FormField>
            <FormField label="Jabatan" htmlFor={`ct-title-${idx}`}>
              <Input
                id={`ct-title-${idx}`}
                value={c.title}
                onChange={(e) => setRow(idx, { title: e.target.value })}
                placeholder="cth. Wakasek Kurikulum"
              />
            </FormField>
            <FormField label="Telepon" htmlFor={`ct-phone-${idx}`}>
              <Input
                id={`ct-phone-${idx}`}
                value={c.phone}
                onChange={(e) => setRow(idx, { phone: e.target.value })}
                placeholder="08xx"
              />
            </FormField>
            <FormField label="Email" htmlFor={`ct-email-${idx}`}>
              <Input
                id={`ct-email-${idx}`}
                value={c.email}
                onChange={(e) => setRow(idx, { email: e.target.value })}
                placeholder="email@sekolah.sch.id"
              />
            </FormField>
            <div className="flex items-center justify-end gap-2 py-[5px]">
              <label className="flex items-center gap-1.5 text-xs">
                <input
                  type="checkbox"
                  checked={c.isDefault}
                  onChange={(e) => setRow(idx, { isDefault: e.target.checked })}
                />
                Kontak utama
              </label>
              <button type="button" className="btn ghost" onClick={() => removeRow(idx)}>
                Hapus
              </button>
            </div>
          </div>
        </div>
      ))}
      <button type="button" className="btn ghost" onClick={addRow}>
        + Tambah Kontak
      </button>
    </div>
  );
}

// ─── Aktivitas ────────────────────────────────────────────────────────────────

export interface SchoolNewActivityForm {
  type: string;
  activityAt: string;
  notes: string;
}

function formatDateId(iso: string): string {
  const d = new Date(iso);
  return Number.isNaN(d.getTime()) ? iso : d.toLocaleDateString('id-ID', { day: '2-digit', month: 'short', year: 'numeric' });
}

export function SchoolActivityFields({
  activities,
  draft,
  onDraftChange,
}: {
  activities: ErpSchoolActivity[];
  draft: SchoolNewActivityForm;
  onDraftChange: (d: SchoolNewActivityForm) => void;
}) {
  return (
    <div>
      {activities.length > 0 && (
        <div className="mb-2 space-y-1.5">
          {activities.map((a) => (
            <div key={a.id} className="flex items-start gap-2 text-xs">
              <Badge variant="default">{ACTIVITY_TYPE_LABELS[a.type] ?? a.type}</Badge>
              <span className="text-[var(--fg-subtle)]">{formatDateId(a.activityAt)}</span>
              <span className="flex-1">{a.notes}</span>
              {a.contact?.name && <span className="text-[var(--fg-subtle)]">({a.contact.name})</span>}
            </div>
          ))}
        </div>
      )}
      <div className="rounded border border-[var(--border)] p-2">
        <div className="grid grid-cols-2 gap-x-3">
          <FormField label="Jenis" htmlFor="act-type">
            <EnumSelect
              id="act-type"
              value={draft.type}
              onChange={(v) => onDraftChange({ ...draft, type: v })}
              options={ACTIVITY_TYPE_OPTIONS}
            />
          </FormField>
          <FormField label="Tanggal" htmlFor="act-date">
            <DateInput
              id="act-date"
              value={draft.activityAt}
              onChange={(iso) => onDraftChange({ ...draft, activityAt: iso })}
            />
          </FormField>
        </div>
        <FormField label="Catatan" htmlFor="act-notes">
          <textarea
            id="act-notes"
            className="input w-full"
            rows={2}
            value={draft.notes}
            onChange={(e) => onDraftChange({ ...draft, notes: e.target.value })}
            placeholder="Hasil kunjungan / poin negosiasi (diisi = tersimpan sebagai aktivitas baru saat Simpan)"
          />
        </FormField>
      </div>
    </div>
  );
}
