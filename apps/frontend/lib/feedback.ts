/**
 * Lightweight feedback helpers — scaffold replacement for the prototype's
 * `window.toast` / `window.bulkAction` / `window.confirmAction` globals.
 *
 * Toasts are delegated to `sonner` (Toaster mounted in the root layout).
 * Confirm dialogs are delegated to `<ConfirmDialogHost>` (mounted in the
 * app shell) via a global `app-confirm` CustomEvent — keeping this module
 * importable from anywhere without prop-drilling or circular imports.
 */
import { toast } from 'sonner';
import type { IconName } from '@/components/ui/icons';
import { tGlobal } from './mock';

export type NotifyType = 'success' | 'info' | 'danger' | 'warn';

/** Mirrors prototype `window.toast(msg, { type })`. */
export function notify(msg: string, type: NotifyType = 'info'): void {
  const text = (msg ?? '').trim() || 'Terjadi kesalahan tanpa pesan';
  if (type === 'success') return void toast.success(text);
  if (type === 'danger') return void toast.error(text);
  if (type === 'warn') return void toast.warning(text);
  toast(text);
}

// ---------------------------------------------------------------------------
// Confirm dialog — types + imperative API
// ---------------------------------------------------------------------------

export type ConfirmVariant = 'primary' | 'danger' | 'warn' | 'success';

export interface ConfirmItem {
  label: string;
  val?: string | number;
}

export interface ConfirmOptions {
  title?: string;
  message?: string;
  items?: ConfirmItem[];
  confirmLabel?: string;
  confirmIcon?: IconName;
  cancelLabel?: string;
  variant?: ConfirmVariant;
  icon?: IconName;
  onConfirm?: () => void;
}

/**
 * Dispatch a confirm dialog. `<ConfirmDialogHost>` (in `components/organisms/`)
 * must be mounted somewhere in the React tree to actually render the modal.
 * Calling this on the server is a no-op.
 */
export function confirmAction(opts: ConfirmOptions): void {
  if (typeof window === 'undefined') return;
  window.dispatchEvent(
    new CustomEvent<ConfirmOptions>('app-confirm', { detail: opts }),
  );
}

// ---------------------------------------------------------------------------
// Bulk action — shared confirm → toast flow used by every list page
// ---------------------------------------------------------------------------

interface BulkSpec {
  title: string;
  variant: ConfirmVariant;
  icon: IconName;
  confirmLabel: string;
  confirmIcon: IconName;
  verb: string;
  type: NotifyType;
}

const BULK: Record<string, BulkSpec> = {
  approve: {
    title: 'Setujui dokumen terpilih?',
    variant: 'primary',
    icon: 'check',
    confirmLabel: 'Setujui',
    confirmIcon: 'check',
    verb: 'disetujui',
    type: 'success',
  },
  reject: {
    title: 'Tolak dokumen terpilih?',
    variant: 'danger',
    icon: 'x',
    confirmLabel: 'Tolak',
    confirmIcon: 'x',
    verb: 'ditolak',
    type: 'danger',
  },
  post: {
    title: 'Posting dokumen terpilih?',
    variant: 'primary',
    icon: 'play',
    confirmLabel: 'Posting',
    confirmIcon: 'play',
    verb: 'diposting',
    type: 'success',
  },
  delete: {
    title: 'Hapus dokumen terpilih?',
    variant: 'danger',
    icon: 'trash',
    confirmLabel: 'Hapus',
    confirmIcon: 'trash',
    verb: 'dihapus',
    type: 'danger',
  },
  export: {
    title: 'Export data terpilih?',
    variant: 'primary',
    icon: 'download',
    confirmLabel: 'Export',
    confirmIcon: 'download',
    verb: 'diekspor',
    type: 'success',
  },
};

/**
 * Mirrors prototype `window.bulkAction(kind, count, clearSel, items)`.
 * Opens a confirmation dialog; on confirm, clears the selection and
 * surfaces a summary toast. Cancel = no-op (selection preserved).
 *
 * Signature stays backwards compatible with existing callers
 * (`bulkAction(kind, count, clearSel)`); `items` is optional and renders
 * up to 8 rows inside the dialog body.
 */
export function bulkAction(
  kind: string,
  count: number,
  clearSel: () => void,
  items?: ConfirmItem[],
): void {
  const b = BULK[kind] ?? BULK.export;
  const tail =
    kind === 'delete'
      ? tGlobal('Tindakan ini tidak dapat dibatalkan.')
      : tGlobal('Aksi tercatat di audit trail.');
  confirmAction({
    title: tGlobal(b.title),
    message: `${count} ${tGlobal('dokumen akan')} ${tGlobal(b.verb)}. ${tail}`,
    items: items?.slice(0, 8),
    variant: b.variant,
    icon: b.icon,
    confirmLabel: tGlobal(b.confirmLabel),
    confirmIcon: b.confirmIcon,
    onConfirm: () => {
      clearSel();
      notify(`${count} ${tGlobal('dokumen')} ${tGlobal(b.verb)}`, b.type);
    },
  });
}
