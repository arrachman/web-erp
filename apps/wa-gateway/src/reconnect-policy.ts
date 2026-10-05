import { DisconnectReason } from '@whiskeysockets/baileys';

/**
 * Kebijakan reconnect wa-gateway.
 *
 * Latar (insiden 29 Jul 2026): device mati jam 05:00 WIB setelah rentetan
 * `503 unavailableService` → `405` → `405` dalam ~15 detik. Delay reconnect
 * dulu **flat 2 detik** tanpa backoff, sehingga saat WhatsApp menolak beruntun
 * kita justru menghantam server berulang kali (pola yang memicu rate-limit dan
 * berisiko ban nomor). Sekarang delay naik eksponensial + jitter.
 */

/** Delay dasar reconnect (percobaan pertama). */
const BASE_DELAY_MS = 2_000;

/** Batas atas delay — tetap responsif saat WA pulih, tanpa spam. */
const MAX_DELAY_MS = 5 * 60 * 1000;

/**
 * Status yang berarti kredensial sudah tidak sah. Reconnect otomatis tidak
 * akan menolong — sesi harus dibersihkan dan device di-pair ulang via QR.
 * `419` (UNAUTHORIZED di Baileys) ikut disertakan bersama 401/403.
 */
const FATAL_CODES = new Set<number>([
  DisconnectReason.loggedOut, // 401
  DisconnectReason.forbidden, // 403
  419,
]);

/** True bila penyebab close menandakan sesi mati permanen (butuh pairing ulang). */
export function isFatalDisconnect(code: number | undefined): boolean {
  return code !== undefined && FATAL_CODES.has(code);
}

/**
 * Delay reconnect untuk percobaan ke-`attempt` (1 = percobaan pertama).
 * Eksponensial 2s → 4s → 8s … dibatasi 5 menit, plus jitter ±20% supaya
 * reconnect tidak selalu jatuh di milidetik yang sama.
 */
export function reconnectDelayMs(attempt: number): number {
  const n = Math.max(1, attempt);
  const exp = BASE_DELAY_MS * 2 ** (n - 1);
  const capped = Math.min(exp, MAX_DELAY_MS);
  const jitter = capped * 0.2 * (Math.random() * 2 - 1);
  return Math.max(BASE_DELAY_MS, Math.round(capped + jitter));
}
