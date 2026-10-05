import type { WASocket } from '@whiskeysockets/baileys';

export type SessionStatus = 'connecting' | 'connect' | 'disconnect';

/** State satu sesi WhatsApp (1 device token = 1 sesi). */
export interface Session {
  token: string;
  sock?: WASocket;
  status: SessionStatus;
  /** QR terkini sebagai data URL PNG (siap dipakai <img src>). */
  qr?: string;
  /** Timestamp QR terkait. QR Baileys mati saat dirotasi / socket mati. */
  qrAt?: number;
  /** Urutan QR socket berjalan (1 = QR pertama). Reset saat socket baru. */
  qrSeq?: number;
  /** Nomor WA aktual yang connect (digits, format 62xxx). */
  phone?: string;
  starting: boolean;
  /** True saat logout manual → jangan auto-reconnect. */
  manualLogout: boolean;
  /** Interval re-assert presence 'unavailable' (anti-suppress notif HP). */
  presenceTimer?: ReturnType<typeof setInterval>;
  /** Jumlah reconnect beruntun tanpa sempat 'open'. Reset saat connect. */
  reconnectAttempts: number;
  /** Timer reconnect terjadwal — dibatalkan bila ada start() manual. */
  reconnectTimer?: ReturnType<typeof setTimeout>;
  /** Timestamp terakhir kali state berubah — dipakai watchdog. */
  lastProgressAt: number;
}

/** Baris ringkasan sesi untuk endpoint /health. */
export interface SessionHealth {
  token: string;
  name: string;
  phone?: string;
  status: SessionStatus;
  reconnectAttempts: number;
  idleMs: number;
}

/**
 * Interval re-assert presence 'unavailable'. Presence di server WA bisa drift
 * ke 'online' (mis. handler creds.update Baileys mengirim <presence name> tanpa
 * type = available). Re-assert berkala menjaga device tetap "offline" → HP tetap
 * target push notification.
 */
export const PRESENCE_REASSERT_MS = 60_000;

/**
 * Periode watchdog: cek berkala apakah ada sesi yang seharusnya hidup tapi
 * nyangkut di 'connecting'/'disconnect' tanpa reconnect terjadwal.
 */
export const WATCHDOG_INTERVAL_MS = 60_000;

/**
 * Ambang "tidak ada kemajuan". Sesi non-connect yang diam lebih lama dari ini
 * (dan tanpa timer reconnect) dianggap macet → dipaksa start ulang.
 * Insiden 29 Jul 2026: sesi diam ~4 jam tanpa satu pun log.
 */
export const STUCK_AFTER_MS = 3 * 60 * 1000;

/** Batas tunggu satu percobaan `connect()`. Lewat ini → dianggap gagal. */
export const CONNECT_TIMEOUT_MS = 60_000;

/**
 * Umur maksimum QR yang boleh disajikan, dibedakan per urutan (lihat qrSeq):
 *
 *  - QR PERTAMA socket (qrSeq ≤ 1): Baileys membiarkan QR pertama hidup 60 dtk
 *    sebelum rotasi (`qrMs = qrTimeout || 60000`) — sajikan sampai umur 50 dtk
 *    agar admin punya layar stabil ±1 menit tanpa balasan "QR belum siap".
 *  - QR ROTASI (qrSeq ≥ 2): hidup 20 dtk (`qrMs = 20000`) — sajikan hanya sampai
 *    umur 10 dtk supaya QR yang tampil selalu punya ≥ ~10 dtk masa hidup
 *    (kaden refresh UI 8 dtk + waktu mengarahkan kamera).
 *
 * Memindai QR yang sudah dirotasi / milik socket mati membuat HP menjawab
 * "Invalid QR code" (insiden 24 Sep 2026).
 */
export const QR_FIRST_MAX_AGE_MS = 50_000;
export const QR_ROTATED_MAX_AGE_MS = 10_000;

/** Batas tunggu /qr menunggu QR yang layak saji sebelum menyatakan "belum siap". */
export const QR_WAIT_TIMEOUT_MS = 15_000;

/**
 * Jendela "socket masih hidup" untuk socket berstatus 'connecting'. Sesi tanpa
 * creds yang menunggu scan QR berstatus 'connecting' berhari-hari, dan jeda
 * antar-event terpanjangnya = rotasi QR pertama (60 dtk). Jendela ini WAJIB
 * lebih besar dari 60 dtk — bila lebih kecil, start() yang dipicu poll /qr
 * akan mereap socket sehat yang sedang menunggu scan dan membatalkan QR yang
 * sedang ditampilkan (varian lain dari insiden 24 Sep 2026).
 */
export const SOCKET_ALIVE_MS = 75_000;
