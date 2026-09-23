// Humanize error messages from the ERP API client.
// Used by both ErrorState molecule and toast notifications so that
// raw HTTP statusText ("Not Found", "Failed to fetch") never leaks
// to the user.

import type { IconName } from '@/components/ui/icons';

export interface ErrorPresentation {
  icon: IconName;
  title: string;
  description: string;
}

const NETWORK_RE = /failed to fetch|networkerror|load failed|err_network/i;
const NOT_FOUND_RE = /not found|http_404/i;
const AUTH_RE = /unauthor|http_401|forbidden|http_403/i;
const SERVER_RE = /http_5\d\d|internal server/i;

export function presentError(raw: string): ErrorPresentation {
  const msg = (raw ?? '').trim();

  if (NETWORK_RE.test(msg)) {
    return {
      icon: 'wifi-off',
      title: 'Tidak bisa terhubung ke server',
      description:
        'Periksa koneksi internet atau status backend ERP, lalu coba lagi.',
    };
  }

  if (NOT_FOUND_RE.test(msg)) {
    return {
      icon: 'alert-triangle',
      title: 'Data tidak ditemukan',
      description:
        'Endpoint atau resource yang diminta belum tersedia di server. Hubungi admin bila menu ini seharusnya sudah aktif.',
    };
  }

  if (AUTH_RE.test(msg)) {
    return {
      icon: 'shield',
      title: 'Akses ditolak',
      description:
        'Sesi mungkin sudah berakhir atau role Anda tidak punya izin untuk membuka data ini.',
    };
  }

  if (SERVER_RE.test(msg)) {
    return {
      icon: 'alert-triangle',
      title: 'Server bermasalah',
      description: `Server tidak bisa memproses permintaan. (${msg})`,
    };
  }

  return {
    icon: 'alert-triangle',
    title: 'Gagal memuat data',
    description: msg || 'Terjadi kesalahan yang tidak diketahui.',
  };
}

/**
 * Compact one-line message for toast notifications.
 * Toast tidak punya ruang untuk deskripsi panjang — gabung title + hint
 * pendek bila ada.
 *
 * Prefer pesan bisnis dari API (mis. "Cannot delete a protected partner type.")
 * daripada title generik — user harus melihat alasan 400/500 yang sebenarnya.
 */
export function toToastMessage(raw: string): string {
  const msg = (raw ?? '').trim();
  if (!msg) return 'Terjadi kesalahan yang tidak diketahui.';

  const { title, description } = presentError(msg);
  // Fallback branch (bukan network/auth/404/5xx): description === raw message.
  // Tampilkan pesan API apa adanya agar toast delete/validasi berguna.
  if (description === msg) return msg;
  return `${title} · ${description}`;
}
