'use client';

/**
 * Transaction attachments (lampiran) — upload + kelola dokumen pendukung per
 * transaksi (faktur scan, bukti bayar, surat jalan, kontrak, dll). Generik:
 * props (domain, docType, docId). Dropzone drag&drop + klik (multiple), daftar
 * file dengan catatan editable, buka/unduh, hapus. Butuh transaksi tersimpan
 * (docId) — mode create menampilkan empty state. Atomic tier: Organism.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { confirmAction, notify } from '@/lib/feedback';
import {
  listTransactionAttachments, uploadTransactionAttachment, deleteTransactionAttachment,
  updateTransactionAttachmentNote, transactionAttachmentFileUrl,
  type TransactionAttachment, type AttachmentDomain,
} from '@/lib/api/transaction-attachments';

const ACCEPT = [
  'application/pdf', 'image/jpeg', 'image/png', 'image/webp', 'image/gif',
  '.doc', '.docx', '.xls', '.xlsx', '.ppt', '.pptx', '.csv', '.txt', '.zip',
].join(',');
const HINT = 'PDF · gambar · Word · Excel · PowerPoint · CSV · ZIP — maks 10MB, hingga 30 file';
const MAX_ATTACHMENTS = 30;

const errMsg = (err: unknown, fallback: string) =>
  err instanceof Error ? err.message : fallback;

function formatBytes(bytes: number): string {
  if (bytes >= 1024 * 1024) return `${(bytes / 1024 / 1024).toFixed(1)} MB`;
  return `${Math.max(1, Math.round(bytes / 1024))} KB`;
}

// ─── Dropzone ─────────────────────────────────────────────────────────────────

function Dropzone({ busy, onFiles }: { busy: boolean; onFiles: (files: File[]) => void }) {
  const inputRef = React.useRef<HTMLInputElement>(null);
  const [dragOver, setDragOver] = React.useState(false);

  const pick = (list: FileList | null) => {
    if (!list?.length) return;
    onFiles(Array.from(list));
  };

  return (
    <button
      type="button"
      disabled={busy}
      onClick={() => inputRef.current?.click()}
      onDragOver={(e) => { e.preventDefault(); setDragOver(true); }}
      onDragLeave={() => setDragOver(false)}
      onDrop={(e) => { e.preventDefault(); setDragOver(false); pick(e.dataTransfer.files); }}
      className={`flex w-full cursor-pointer flex-col items-center justify-center gap-1 rounded-[var(--radius)] border border-dashed px-4 py-6 transition-colors ${dragOver ? 'border-primary bg-[var(--panel-hover)]' : 'border-border bg-[var(--panel-2)] hover:bg-[var(--panel-hover)]'} disabled:cursor-not-allowed disabled:opacity-60`}
      aria-label="Unggah lampiran"
      title={HINT}
    >
      <input
        ref={inputRef} type="file" accept={ACCEPT} multiple hidden
        onChange={(e) => { pick(e.target.files); e.target.value = ''; }}
      />
      <Icon name="upload" size={20} className="text-[var(--fg-muted)]" />
      {busy ? (
        <span className="text-[11px] text-[var(--fg-muted)]">Mengunggah…</span>
      ) : (
        <>
          <span className="text-xs font-medium">Seret file ke sini atau klik untuk pilih</span>
          <span className="text-[11px] text-[var(--fg-subtle)]">{HINT}</span>
        </>
      )}
    </button>
  );
}

// ─── Baris lampiran ───────────────────────────────────────────────────────────

function AttachmentRow({
  att, fileUrl, onSaveNote, onDelete,
}: {
  att: TransactionAttachment; fileUrl: string;
  onSaveNote: (note: string) => void; onDelete: () => void;
}) {
  const [note, setNote] = React.useState(att.note ?? '');
  React.useEffect(() => { setNote(att.note ?? ''); }, [att.note]);

  return (
    <div className="flex items-center gap-3 rounded-[var(--radius)] border border-border bg-[var(--panel-2)] px-3 py-2">
      <Icon name="file" size={18} className="shrink-0 text-[var(--fg-muted)]" />
      <div className="min-w-0 flex-1">
        <a
          href={fileUrl} target="_blank" rel="noreferrer"
          className="block truncate text-xs font-medium text-foreground hover:text-primary hover:underline"
          title={`${att.fileName} — klik untuk buka`}
        >
          {att.fileName}
        </a>
        <input
          value={note}
          onChange={(e) => setNote(e.target.value)}
          onBlur={() => { if ((att.note ?? '') !== note) onSaveNote(note); }}
          placeholder="Tambah catatan…"
          className="mt-0.5 w-full bg-transparent text-[11px] text-[var(--fg-muted)] outline-none placeholder:text-[var(--fg-subtle)] focus:text-foreground"
        />
      </div>
      <span className="shrink-0 text-[11px] tabular-nums text-[var(--fg-subtle)]">{formatBytes(att.sizeBytes)}</span>
      <a
        href={fileUrl} download={att.fileName}
        className="flex shrink-0 cursor-pointer items-center rounded-[var(--radius)] border border-border bg-card p-1.5 text-[var(--fg-muted)] hover:bg-[var(--panel-hover)] hover:text-foreground"
        title="Unduh"
      >
        <Icon name="download" size={13} />
      </a>
      <button
        type="button" onClick={onDelete}
        className="flex shrink-0 cursor-pointer items-center rounded-[var(--radius)] border border-border bg-card p-1.5 text-danger hover:bg-[var(--panel-hover)]"
        title="Hapus lampiran"
      >
        <Icon name="trash" size={13} />
      </button>
    </div>
  );
}

// ─── Organism utama ───────────────────────────────────────────────────────────

export interface TransactionAttachmentUploadProps {
  domain: AttachmentDomain;
  docType: string;
  docId: string | null;
}

export function TransactionAttachmentUpload({ domain, docType, docId }: TransactionAttachmentUploadProps) {
  const [items, setItems] = React.useState<TransactionAttachment[]>([]);
  const [loading, setLoading] = React.useState(false);
  const [busy, setBusy] = React.useState(false);

  const reload = React.useCallback(async () => {
    if (!docId) return;
    setLoading(true);
    try {
      setItems(await listTransactionAttachments(domain, docType, docId));
    } catch (err) {
      notify(errMsg(err, 'Gagal memuat lampiran'), 'danger');
    } finally {
      setLoading(false);
    }
  }, [domain, docType, docId]);

  React.useEffect(() => { void reload(); }, [reload]);

  if (!docId) {
    return (
      <div className="m-5 flex flex-col items-center gap-1.5 rounded-[var(--radius)] border border-dashed border-border bg-[var(--panel-2)] px-4 py-8 text-center">
        <Icon name="file" size={20} className="text-[var(--fg-muted)]" />
        <p className="text-xs font-medium">Lampiran belum bisa diunggah</p>
        <p className="text-[11px] text-[var(--fg-subtle)]">Simpan transaksi terlebih dahulu, lalu buka kembali untuk mengunggah dokumen pendukung.</p>
      </div>
    );
  }

  const upload = async (files: File[]) => {
    setBusy(true);
    try {
      for (const file of files) {
        await uploadTransactionAttachment(domain, docType, docId, file); // sequential: backend jaga limit
      }
      notify(`${files.length} lampiran diunggah`, 'success');
    } catch (err) {
      notify(errMsg(err, 'Unggahan gagal'), 'danger'); // sebagian file bisa saja sudah masuk
    } finally {
      setBusy(false);
      await reload();
    }
  };

  const handleSaveNote = async (att: TransactionAttachment, note: string) => {
    try {
      await updateTransactionAttachmentNote(domain, docType, docId, att.id, note);
      setItems((prev) => prev.map((a) => (a.id === att.id ? { ...a, note: note.trim() || null } : a)));
    } catch (err) {
      notify(errMsg(err, 'Gagal menyimpan catatan'), 'danger');
    }
  };

  const handleDelete = (att: TransactionAttachment) => {
    confirmAction({
      title: 'Hapus Lampiran?',
      message: `${att.fileName} akan dihapus permanen.`,
      confirmLabel: 'Hapus',
      variant: 'danger',
      onConfirm: () => {
        void (async () => {
          try {
            await deleteTransactionAttachment(domain, docType, docId, att.id);
            notify('Lampiran dihapus', 'success');
            await reload();
          } catch (err) {
            notify(errMsg(err, 'Gagal menghapus lampiran'), 'danger');
          }
        })();
      },
    });
  };

  return (
    <div className="flex flex-col gap-3 px-5 py-4">
      <div className="flex items-baseline justify-between">
        <p className="text-[11px] font-medium uppercase tracking-wide text-[var(--fg-muted)]">Dokumen Pendukung</p>
        <span className="text-[11px] text-[var(--fg-subtle)]">{items.length}/{MAX_ATTACHMENTS}</span>
      </div>

      {items.length < MAX_ATTACHMENTS && <Dropzone busy={busy} onFiles={(files) => void upload(files)} />}

      {loading && items.length === 0 ? (
        <p className="py-4 text-center text-[11px] text-[var(--fg-subtle)]">Memuat…</p>
      ) : items.length === 0 ? (
        <p className="py-2 text-center text-[11px] text-[var(--fg-subtle)]">Belum ada lampiran.</p>
      ) : (
        <div className="flex flex-col gap-1.5">
          {items.map((att) => (
            <AttachmentRow
              key={att.id} att={att}
              fileUrl={transactionAttachmentFileUrl(domain, docType, docId, att.id)}
              onSaveNote={(note) => void handleSaveNote(att, note)}
              onDelete={() => handleDelete(att)}
            />
          ))}
        </div>
      )}
      <p className="text-[11px] text-[var(--fg-subtle)]">Lampiran tersimpan langsung saat diunggah. Klik nama file untuk membuka, ikon unduh untuk menyimpan, atau isi catatan singkat per file.</p>
    </div>
  );
}
