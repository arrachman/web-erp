'use client';

/**
 * Banner ringkas yang menampilkan jumlah error form di top modal,
 * sebelum FormFields. Dipasang oleh SimpleMasterPage saat validasi
 * client-side gagal. Atomic tier: Molecule.
 */

import * as React from 'react';
import type { FormErrors } from '@/lib/form-validation';
import { tGlobal } from '@/lib/mock';

const MAX_SHOWN = 5;

export function FormErrorSummary<F>({ errors }: { errors: FormErrors<F> }) {
  const list = Object.entries(errors).filter(([, msg]) => !!msg) as [string, string][];
  if (list.length === 0) return null;
  const overflow = list.length - MAX_SHOWN;
  return (
    <div className="mx-5 mt-3 rounded-[var(--radius)] border border-danger/30 bg-danger/5 px-3 py-2" role="alert">
      <p className="text-xs font-semibold text-danger">
        {list.length} {tGlobal('field perlu diperbaiki')}
      </p>
      <ul className="ml-4 mt-1 list-disc text-[11px] text-danger/90">
        {list.slice(0, MAX_SHOWN).map(([k, v]) => <li key={k}>{v}</li>)}
        {overflow > 0 && <li>… {tGlobal('dan')} {overflow} {tGlobal('lainnya')}</li>}
      </ul>
    </div>
  );
}
