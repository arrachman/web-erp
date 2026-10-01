'use client';

import * as React from 'react';
import { cn } from '@/lib/utils';

/**
 * Shared types, deterministic dataset, and tiny helpers used by
 * `ContactPicker` and its row/preview sub-organisms. Lives in a sibling
 * module to keep `contact-picker.tsx` < 400 LOC (clean-code rule §3).
 */

export type ContactCategory = 'Customer' | 'Supplier' | 'Karyawan' | 'Umum';

export interface Contact {
  id: number;
  code: string;
  name: string;
  cat: ContactCategory;
  city: string;
  npwp: string;
  phone: string;
  balance: number;
  lastTrx: string;
  trxCount: number;
}

export type CategoryFilter = 'Semua' | ContactCategory;

export const CAT_ORDER: readonly CategoryFilter[] = [
  'Semua',
  'Customer',
  'Supplier',
  'Karyawan',
  'Umum',
];

export const CAT_PILL: Record<ContactCategory, string> = {
  Customer: 'primary',
  Supplier: 'success',
  Karyawan: 'warn',
  Umum: 'info',
};

export const RECENT_IDS: readonly number[] = [0, 2, 16];

export const MONO_FONT: React.CSSProperties = {
  fontFamily: 'Geist Mono, monospace',
};

const CITIES = [
  'Jakarta', 'Bandung', 'Surabaya', 'Tangerang',
  'Bekasi', 'Bogor', 'Semarang', 'Cikarang',
] as const;

const CUST_NAMES = [
  'PT Sumber Rejeki', 'CV Cahaya Abadi', 'PT Mitra Sentosa', 'Toko Berkah Jaya',
  'PT Karya Mandiri', 'CV Tirta Makmur', 'PT Indo Lestari', 'PT Global Niaga',
  'CV Anugerah Jaya', 'PT Bintang Selatan', 'PT Cipta Mandiri', 'PT Sentosa Abadi',
  'CV Permata Hijau', 'PT Surya Cemerlang', 'PT Nusantara Trading',
] as const;

const SUPP_NAMES = [
  'PT Sinar Logam', 'CV Mitra Plastik', 'PT Indo Kemasan', 'PT Tirta Kimia',
  'CV Berkah Tekstil', 'PT Adidaya Mesin', 'PT Cahaya Listrik', 'CV Karya Baja',
  'PT Sumber Energi',
] as const;

const KARYAWAN_NAMES = [
  'Adi Saputra', 'Fitri Handayani', 'Rendra Wibowo', 'Maya Pratiwi',
  'Budi Tirta', 'Sari Indah', 'Joko Susilo', 'Dewi Lestari', 'Andi Prasetyo',
] as const;

const UMUM_NAMES = [
  'Pelanggan Tunai', 'Petty Cash PCI', 'Petty Cash JKT', 'Refund Bank',
  'Setoran Modal Owner',
] as const;

export const CONTACT_DATA: Contact[] = (() => {
  let s = 99;
  const r = (): number => {
    s = (s * 1664525 + 1013904223) >>> 0;
    return s / 4294967296;
  };
  let id = 0;
  const out: Contact[] = [];
  const push = (name: string, cat: ContactCategory): void => {
    id += 1;
    const prefix =
      cat === 'Customer' ? 'C'
        : cat === 'Supplier' ? 'S'
          : cat === 'Karyawan' ? 'E' : 'U';
    const code = prefix + String(id).padStart(4, '0');
    const npwp =
      cat === 'Karyawan' || cat === 'Umum'
        ? '—'
        : `01.${Math.floor(r() * 900 + 100)}.${Math.floor(r() * 900 + 100)}.${Math.floor(r() * 9)}-${Math.floor(r() * 900 + 100)}.000`;
    out.push({
      id,
      code,
      name,
      cat,
      city: CITIES[Math.floor(r() * CITIES.length)],
      npwp,
      phone: `0812-${Math.floor(r() * 9000 + 1000)}-${Math.floor(r() * 9000 + 1000)}`,
      balance: Math.round(((r() - 0.3) * 80000000) / 1000) * 1000,
      lastTrx: `${String(Math.floor(r() * 12) + 1).padStart(2, '0')}/05/2026`,
      trxCount: Math.floor(r() * 80) + 1,
    });
  };
  CUST_NAMES.forEach((n) => push(n, 'Customer'));
  SUPP_NAMES.forEach((n) => push(n, 'Supplier'));
  KARYAWAN_NAMES.forEach((n) => push(n, 'Karyawan'));
  UMUM_NAMES.forEach((n) => push(n, 'Umum'));
  return out;
})();

const HUES = [220, 160, 30, 200, 280, 340] as const;
export const avatarHue = (c: Contact): number => HUES[c.id % HUES.length];

export const initials = (name: string): string => {
  const parts = name
    .split(' ')
    .filter((w) => w.length > 0 && (/^[A-Z]/.test(w[0]) || w.length > 2));
  const letters = parts.slice(0, 2).map((w) => w[0]).join('');
  return letters.toUpperCase() || name.slice(0, 2).toUpperCase();
};

export interface HighlightMatchProps {
  text: string;
  q: string;
}

export function HighlightMatch({ text, q }: HighlightMatchProps): React.ReactElement {
  if (!q) return <>{text}</>;
  const idx = text.toLowerCase().indexOf(q.toLowerCase());
  if (idx === -1) return <>{text}</>;
  return (
    <>
      {text.slice(0, idx)}
      <mark>{text.slice(idx, idx + q.length)}</mark>
      {text.slice(idx + q.length)}
    </>
  );
}

interface StatProps {
  label: string;
  value: string;
  accent?: 'danger';
  mono?: boolean;
}

export function Stat({ label, value, accent, mono }: StatProps): React.ReactElement {
  return (
    <div className="cm-stat">
      <div className="cm-stat-label">{label}</div>
      <div
        className={cn('cm-stat-val', accent === 'danger' && 'danger')}
        style={mono ? { fontFamily: 'Geist Mono, monospace', fontSize: 'calc(12px * var(--font-scale, 1))' } : undefined}
      >
        {value}
      </div>
    </div>
  );
}
