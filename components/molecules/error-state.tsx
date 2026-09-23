'use client';

/**
 * ErrorState — molecule untuk menampilkan blok error pada halaman list/detail
 * ERP. Pesan dihumanize lewat helper bersama `lib/error-message` agar konsisten
 * dengan toast notification.
 *
 * Atomic tier: Molecule (Icon atom + button atom).
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { presentError } from '@/lib/error-message';

interface ErrorStateProps {
  message: string;
  onRetry?: () => void;
  retrying?: boolean;
}

export function ErrorState({ message, onRetry, retrying }: ErrorStateProps) {
  const meta = React.useMemo(() => presentError(message), [message]);

  return (
    <div
      role="alert"
      aria-live="polite"
      className="flex flex-col items-center justify-center gap-3 px-6 py-12 text-center"
    >
      <div className="flex h-12 w-12 items-center justify-center rounded-full bg-danger/10 text-danger">
        <Icon name={meta.icon} size={22} />
      </div>
      <div className="space-y-1">
        <div className="text-sm font-medium text-foreground">{meta.title}</div>
        <div className="max-w-md text-xs text-muted-foreground">
          {meta.description}
        </div>
      </div>
      {onRetry && (
        <button
          type="button"
          className="btn ghost sm mt-1"
          onClick={onRetry}
          disabled={retrying}
        >
          <Icon name="refresh" size={12} />
          {retrying ? 'Memuat ulang…' : 'Coba lagi'}
        </button>
      )}
    </div>
  );
}
