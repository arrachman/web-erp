'use client';

/**
 * Item media gallery — upload + preview gambar produk (max 8, satu "Utama")
 * dan satu video pendek. Dropzone drag&drop + klik, thumbnail grid dengan
 * aksi hover (jadikan utama / hapus), lightbox preview, dan player video.
 * Media butuh itemId tersimpan — mode create menampilkan empty state.
 * Atomic tier: Organism.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { confirmAction, notify } from '@/lib/feedback';
import {
  listItemMedia, uploadItemMedia, deleteItemMedia, setPrimaryItemMedia,
  itemMediaFileUrl, type ItemMedia, type ItemMediaKind,
} from '@/lib/api/items';

const IMAGE_ACCEPT = 'image/jpeg,image/png,image/webp,image/gif';
const VIDEO_ACCEPT = 'video/mp4,video/webm,video/quicktime';
const IMAGE_HINT = 'JPG · PNG · WebP · GIF — maks 5MB, hingga 8 gambar';
const VIDEO_HINT = 'MP4 · WebM · MOV — maks 50MB, 1 video (upload baru mengganti yang lama)';
const MAX_IMAGES = 8;

const errMsg = (err: unknown, fallback: string) =>
  err instanceof Error ? err.message : fallback;

function formatBytes(bytes: number): string {
  if (bytes >= 1024 * 1024) return `${(bytes / 1024 / 1024).toFixed(1)} MB`;
  return `${Math.max(1, Math.round(bytes / 1024))} KB`;
}

// ─── Dropzone (shared untuk gambar & video) ───────────────────────────────────

function Dropzone({
  accept, multiple, hint, label, compact, busy, onFiles,
}: {
  accept: string; multiple?: boolean; hint: string; label: string;
  compact?: boolean; busy: boolean; onFiles: (files: File[]) => void;
}) {
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
      className={`flex cursor-pointer flex-col items-center justify-center gap-1 rounded-[var(--radius)] border border-dashed transition-colors ${compact ? 'aspect-square w-full p-1' : 'w-full px-4 py-6'} ${dragOver ? 'border-primary bg-[var(--panel-hover)]' : 'border-border bg-[var(--panel-2)] hover:bg-[var(--panel-hover)]'} disabled:cursor-not-allowed disabled:opacity-60`}
      aria-label={label}
      title={hint}
    >
      <input
        ref={inputRef} type="file" accept={accept} multiple={multiple} hidden
        onChange={(e) => { pick(e.target.files); e.target.value = ''; }}
      />
      <Icon name="upload" size={compact ? 16 : 20} className="text-[var(--fg-muted)]" />
      {busy ? (
        <span className="text-[11px] text-[var(--fg-muted)]">Mengunggah…</span>
      ) : (
        <>
          <span className={`font-medium ${compact ? 'text-[10px]' : 'text-xs'}`}>{label}</span>
          {!compact && <span className="text-[11px] text-[var(--fg-subtle)]">{hint}</span>}
        </>
      )}
    </button>
  );
}

// ─── Kartu gambar (thumbnail + aksi hover) ────────────────────────────────────

function ImageCard({
  media, itemId, onPreview, onSetPrimary, onDelete,
}: {
  media: ItemMedia; itemId: string;
  onPreview: () => void; onSetPrimary: () => void; onDelete: () => void;
}) {
  return (
    <div className="group relative aspect-square overflow-hidden rounded-[var(--radius)] border border-border bg-[var(--panel-2)]">
      <button type="button" onClick={onPreview} className="h-full w-full cursor-pointer" title={`${media.fileName} (${formatBytes(media.sizeBytes)}) — klik untuk preview`}>
        {/* eslint-disable-next-line @next/next/no-img-element */}
        <img src={itemMediaFileUrl(itemId, media.id)} alt={media.fileName} className="h-full w-full object-cover" loading="lazy" />
      </button>
      {media.isPrimary && (
        <span className="absolute left-1 top-1 rounded-[var(--radius)] bg-primary px-1.5 py-0.5 text-[9px] font-semibold uppercase tracking-wide text-primary-foreground">
          Utama
        </span>
      )}
      <div className="absolute inset-x-0 bottom-0 flex justify-end gap-1 bg-gradient-to-t from-black/60 to-transparent p-1 opacity-0 transition-opacity group-hover:opacity-100">
        {!media.isPrimary && (
          <button type="button" onClick={onSetPrimary} className="cursor-pointer rounded-[var(--radius)] bg-black/50 p-1 text-white hover:bg-black/70" title="Jadikan gambar utama">
            <Icon name="check" size={12} />
          </button>
        )}
        <button type="button" onClick={onDelete} className="cursor-pointer rounded-[var(--radius)] bg-black/50 p-1 text-white hover:bg-danger" title="Hapus gambar">
          <Icon name="trash" size={12} />
        </button>
      </div>
    </div>
  );
}

// ─── Lightbox preview ─────────────────────────────────────────────────────────

function Lightbox({ src, alt, onClose }: { src: string; alt: string; onClose: () => void }) {
  React.useEffect(() => {
    const onKey = (e: KeyboardEvent) => { if (e.key === 'Escape') onClose(); };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [onClose]);
  return (
    <div className="fixed inset-0 z-[90] flex items-center justify-center bg-black/75 p-6" onClick={onClose} role="dialog" aria-label={`Preview ${alt}`}>
      {/* eslint-disable-next-line @next/next/no-img-element */}
      <img src={src} alt={alt} className="max-h-[85vh] max-w-[90vw] rounded-[var(--radius)] object-contain shadow-xl" />
      <button type="button" onClick={onClose} className="absolute right-4 top-4 cursor-pointer rounded-[var(--radius)] bg-black/60 p-2 text-white hover:bg-black/80" title="Tutup (Esc)">
        <Icon name="x" size={16} />
      </button>
    </div>
  );
}

// ─── Organism utama ───────────────────────────────────────────────────────────

export function ItemMediaUpload({ itemId }: { itemId: string | null }) {
  const [media, setMedia] = React.useState<ItemMedia[]>([]);
  const [loading, setLoading] = React.useState(false);
  const [busyKind, setBusyKind] = React.useState<ItemMediaKind | null>(null);
  const [preview, setPreview] = React.useState<ItemMedia | null>(null);

  const reload = React.useCallback(async () => {
    if (!itemId) return;
    setLoading(true);
    try {
      setMedia(await listItemMedia(itemId));
    } catch (err) {
      notify(errMsg(err, 'Gagal memuat media'), 'danger');
    } finally {
      setLoading(false);
    }
  }, [itemId]);

  React.useEffect(() => { void reload(); }, [reload]);

  if (!itemId) {
    return (
      <div className="m-5 flex flex-col items-center gap-1.5 rounded-[var(--radius)] border border-dashed border-border bg-[var(--panel-2)] px-4 py-8 text-center">
        <Icon name="upload" size={20} className="text-[var(--fg-muted)]" />
        <p className="text-xs font-medium">Media belum bisa diunggah</p>
        <p className="text-[11px] text-[var(--fg-subtle)]">Simpan item terlebih dahulu, lalu buka kembali form ini untuk mengunggah gambar produk &amp; video.</p>
      </div>
    );
  }

  const images = media.filter((m) => m.kind === 'IMAGE');
  const video = media.find((m) => m.kind === 'VIDEO') ?? null;

  const upload = async (kind: ItemMediaKind, files: File[]) => {
    setBusyKind(kind);
    try {
      for (const file of files) {
        await uploadItemMedia(itemId, kind, file); // sequential: backend jaga limit & primary
      }
      notify(kind === 'IMAGE' ? `${files.length} gambar diunggah` : 'Video diunggah', 'success');
    } catch (err) {
      notify(errMsg(err, 'Unggahan gagal'), 'danger'); // sebagian file bisa saja sudah masuk
    } finally {
      setBusyKind(null);
      await reload();
    }
  };

  const handleSetPrimary = async (m: ItemMedia) => {
    try {
      await setPrimaryItemMedia(itemId, m.id);
      notify('Gambar utama diperbarui', 'success');
      await reload();
    } catch (err) {
      notify(errMsg(err, 'Gagal mengubah gambar utama'), 'danger');
    }
  };

  const handleDelete = (m: ItemMedia) => {
    const label = m.kind === 'IMAGE' ? 'Gambar' : 'Video';
    confirmAction({
      title: `Hapus ${label}?`,
      message: `${m.fileName} akan dihapus permanen.`,
      confirmLabel: 'Hapus',
      variant: 'danger',
      onConfirm: () => {
        void (async () => {
          try {
            await deleteItemMedia(itemId, m.id);
            notify(`${label} dihapus`, 'success');
            await reload();
          } catch (err) {
            notify(errMsg(err, 'Gagal menghapus media'), 'danger');
          }
        })();
      },
    });
  };

  return (
    <div className="flex flex-col gap-4 px-5 py-4">
      {/* Gambar produk */}
      <div>
        <div className="flex items-baseline justify-between pb-1.5">
          <p className="text-[11px] font-medium uppercase tracking-wide text-[var(--fg-muted)]">Gambar Produk</p>
          <span className="text-[11px] text-[var(--fg-subtle)]">{images.length}/{MAX_IMAGES} · {IMAGE_HINT}</span>
        </div>
        {loading && media.length === 0 ? (
          <p className="py-4 text-center text-[11px] text-[var(--fg-subtle)]">Memuat…</p>
        ) : (
          <div className="grid grid-cols-4 gap-2 sm:grid-cols-5 md:grid-cols-6">
            {images.map((m) => (
              <ImageCard
                key={m.id} media={m} itemId={itemId}
                onPreview={() => setPreview(m)}
                onSetPrimary={() => void handleSetPrimary(m)}
                onDelete={() => handleDelete(m)}
              />
            ))}
            {images.length < MAX_IMAGES && (
              <Dropzone
                accept={IMAGE_ACCEPT} multiple compact
                label="Tambah" hint={IMAGE_HINT}
                busy={busyKind === 'IMAGE'}
                onFiles={(files) => void upload('IMAGE', files)}
              />
            )}
          </div>
        )}
        <p className="pt-1 text-[11px] text-[var(--fg-subtle)]">Gambar berlabel <span className="font-medium">Utama</span> dipakai sebagai foto produk default. Hover gambar untuk jadikan utama / hapus; klik untuk preview besar.</p>
      </div>

      {/* Video pendek */}
      <div>
        <div className="flex items-baseline justify-between pb-1.5">
          <p className="text-[11px] font-medium uppercase tracking-wide text-[var(--fg-muted)]">Video Pendek</p>
          <span className="text-[11px] text-[var(--fg-subtle)]">{VIDEO_HINT}</span>
        </div>
        {video ? (
          <div className="overflow-hidden rounded-[var(--radius)] border border-border bg-[var(--panel-2)]">
            {/* eslint-disable-next-line jsx-a11y/media-has-caption */}
            <video
              key={video.id}
              src={itemMediaFileUrl(itemId, video.id)}
              controls preload="metadata"
              className="max-h-[260px] w-full bg-black object-contain"
            />
            <div className="flex items-center justify-between gap-2 border-t border-border px-3 py-1.5">
              <span className="truncate text-[11px] text-[var(--fg-muted)]" title={video.fileName}>
                {video.fileName} · {formatBytes(video.sizeBytes)}
              </span>
              <div className="flex shrink-0 items-center gap-1.5">
                <Dropzone
                  accept={VIDEO_ACCEPT} label="Ganti video" hint={VIDEO_HINT}
                  busy={busyKind === 'VIDEO'}
                  onFiles={(files) => void upload('VIDEO', files.slice(0, 1))}
                />
                <button type="button" onClick={() => handleDelete(video)} className="flex shrink-0 cursor-pointer items-center gap-1 rounded-[var(--radius)] border border-border bg-card px-2 py-1 text-[11px] font-medium text-danger hover:bg-[var(--panel-hover)]" title="Hapus video">
                  <Icon name="trash" size={12} /> Hapus
                </button>
              </div>
            </div>
          </div>
        ) : (
          <Dropzone
            accept={VIDEO_ACCEPT}
            label="Seret video ke sini atau klik untuk pilih" hint={VIDEO_HINT}
            busy={busyKind === 'VIDEO'}
            onFiles={(files) => void upload('VIDEO', files.slice(0, 1))}
          />
        )}
      </div>

      {preview && (
        <Lightbox
          src={itemMediaFileUrl(itemId, preview.id)}
          alt={preview.fileName}
          onClose={() => setPreview(null)}
        />
      )}
    </div>
  );
}
