/**
 * Imperative CRUD/bulk handlers for Bagan Akun page.
 * Keeps accounts-page.tsx under the 400-line cap.
 */

import {
  createAccount,
  updateAccount,
  deleteAccount,
  bulkUpdateAccountStatus,
  bulkDeleteAccounts,
  type ErpAccount,
} from '@/lib/api/accounts';
import { confirmAction, notify } from '@/lib/feedback';
import { hasErrors, type FormErrors } from '@/lib/form-validation';
import { tGlobal } from '@/lib/mock';
import {
  toAccountPayload,
  validateAccount,
  type AccountFormData,
} from './accounts-form';

export async function saveAccount(opts: {
  form: AccountFormData;
  editing: ErpAccount | null;
  setFormErrors: (e: FormErrors<AccountFormData>) => void;
  setSaving: (v: boolean) => void;
  setOpen: (v: boolean) => void;
  setForm: (f: AccountFormData) => void;
  defaultForm: () => AccountFormData;
  reload: () => void;
  keepOpen: boolean;
}): Promise<void> {
  const errors = validateAccount(opts.form);
  if (hasErrors(errors)) {
    opts.setFormErrors(errors);
    setTimeout(() => {
      document
        .querySelector<HTMLElement>('[role="dialog"] [aria-invalid="true"]')
        ?.focus();
    }, 0);
    return;
  }
  opts.setFormErrors({});
  opts.setSaving(true);
  try {
    if (opts.editing) {
      await updateAccount(opts.editing.id, toAccountPayload(opts.form));
      notify(`${tGlobal('Bagan Akun')} ${tGlobal('diperbarui')}`, 'success');
    } else {
      await createAccount(toAccountPayload(opts.form));
      notify(`${tGlobal('Bagan Akun')} ${tGlobal('dibuat')}`, 'success');
    }
    if (opts.keepOpen && !opts.editing) {
      opts.setForm(opts.defaultForm());
      setTimeout(() => {
        document
          .querySelector<HTMLElement>(
            '[role="dialog"] input:not([type="hidden"]):not([disabled])',
          )
          ?.focus();
      }, 0);
    } else {
      opts.setOpen(false);
    }
    opts.reload();
  } catch (e: unknown) {
    notify(e instanceof Error ? e.message : tGlobal('Gagal menyimpan'), 'danger');
  } finally {
    opts.setSaving(false);
  }
}

export function confirmDeleteAccount(row: ErpAccount, reload: () => void): void {
  const entityT = tGlobal('akun');
  confirmAction({
    title: `${tGlobal('Hapus')} ${entityT}?`,
    message: `${row.code} — ${row.name} ${tGlobal('akan dihapus permanen.')}`,
    variant: 'danger',
    confirmLabel: tGlobal('Hapus'),
    confirmIcon: 'trash',
    onConfirm: async () => {
      try {
        await deleteAccount(row.id);
        notify(`${tGlobal('Bagan Akun')} ${tGlobal('dihapus')}`, 'success');
        reload();
      } catch (e: unknown) {
        notify(e instanceof Error ? e.message : tGlobal('Gagal'), 'danger');
      }
    },
  });
}

export function confirmBulkStatus(
  ids: string[],
  isActive: boolean,
  reload: () => void,
  clearSelection: () => void,
): void {
  const entityT = tGlobal('akun');
  const actionLabel = tGlobal(isActive ? 'Aktifkan' : 'Nonaktifkan');
  const doneLabel = tGlobal(isActive ? 'diaktifkan' : 'dinonaktifkan');
  confirmAction({
    title: `${actionLabel} ${ids.length} ${entityT}?`,
    message: `${tGlobal('Semua')} ${entityT} ${tGlobal('yang dipilih akan')} ${doneLabel}.`,
    variant: isActive ? 'primary' : 'warn',
    confirmLabel: actionLabel,
    onConfirm: async () => {
      try {
        const { affected } = await bulkUpdateAccountStatus(ids, isActive);
        notify(`${affected} ${entityT} ${doneLabel}`, 'success');
        clearSelection();
        reload();
      } catch (e: unknown) {
        notify(e instanceof Error ? e.message : tGlobal('Gagal'), 'danger');
      }
    },
  });
}

export function confirmBulkDelete(
  ids: string[],
  reload: () => void,
  clearSelection: () => void,
): void {
  const entityT = tGlobal('akun');
  confirmAction({
    title: `${tGlobal('Hapus')} ${ids.length} ${entityT}?`,
    message: `${tGlobal('Semua')} ${entityT} ${tGlobal('akan dihapus permanen.')}`,
    variant: 'danger',
    confirmLabel: tGlobal('Hapus'),
    confirmIcon: 'trash',
    onConfirm: async () => {
      try {
        const { affected } = await bulkDeleteAccounts(ids);
        notify(`${affected} ${entityT} ${tGlobal('dihapus')}`, 'success');
        clearSelection();
        reload();
      } catch (e: unknown) {
        notify(e instanceof Error ? e.message : tGlobal('Gagal'), 'danger');
      }
    },
  });
}
