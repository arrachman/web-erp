'use client';

/**
 * Toolbar yang tampil di atas tabel saat ≥ 1 baris dipilih. Kontrak
 * §2.9.H — wajib hadir di list page yang punya bulk action. Dipakai
 * SimpleMasterPage; bisa di-reuse list page custom lainnya.
 * Atomic tier: Molecule.
 */

import * as React from 'react';
import { tGlobal } from '@/lib/mock';

export interface BulkActionBarProps {
  count: number;
  onActivate: () => void;
  onDeactivate: () => void;
  onDelete: () => void;
  onCancel: () => void;
  entityLabel?: string;
}

export function BulkActionBar({ count, onActivate, onDeactivate, onDelete, onCancel }: BulkActionBarProps) {
  if (count <= 0) return null;
  return (
    <div className="bulk-bar">
      <span className="count">{count} {tGlobal('baris dipilih')}</span>
      <div className="divider" />
      <button className="ba-btn" onClick={onActivate}>{tGlobal('Aktifkan')}</button>
      <button className="ba-btn" onClick={onDeactivate}>{tGlobal('Nonaktifkan')}</button>
      <div className="divider" />
      <button className="ba-btn danger" onClick={onDelete}>{tGlobal('Hapus')}</button>
      <div className="divider" />
      <button className="ba-btn" onClick={onCancel}>{tGlobal('Batal')}</button>
    </div>
  );
}
