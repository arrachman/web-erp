'use client';

/** A3 — Helper bersama halaman Paket Dokumen: modal & format tanggal. */

import * as React from 'react';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';

export const fmtDate = (v: string | null) =>
  v ? new Date(v).toLocaleDateString('id-ID', { day: '2-digit', month: 'short', year: 'numeric' }) : '-';

export function DocModal({
  title,
  onClose,
  children,
}: {
  title: string;
  onClose: () => void;
  children: React.ReactNode;
}) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
      <div className="w-full max-w-md" onClick={(e) => e.stopPropagation()}>
        <Card>
          <div className="flex items-center justify-between border-b px-4 py-3">
            <div className="font-semibold">{title}</div>
            <Button variant="ghost" onClick={onClose}>Tutup</Button>
          </div>
          <div className="space-y-3 px-4 py-4">{children}</div>
        </Card>
      </div>
    </div>
  );
}
