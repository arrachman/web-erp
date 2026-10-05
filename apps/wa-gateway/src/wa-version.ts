import { fetchLatestBaileysVersion, type WAVersion } from '@whiskeysockets/baileys';
import { logger } from './logger';

/**
 * Resolver versi WA Web untuk `makeWASocket`.
 *
 * Kenapa modul ini ada (insiden 29 Jul 2026): `fetchLatestBaileysVersion()`
 * melakukan `fetch()` ke raw.githubusercontent.com **tanpa timeout**. Fungsi itu
 * dipanggil di SETIAP reconnect. Kalau koneksi ke GitHub nge-blackhole (paket
 * di-drop, bukan di-reject), promise-nya tidak pernah settle → `connect()`
 * menggantung selamanya → flag `starting` di SessionManager tidak pernah
 * dilepas → semua reconnect berikutnya ditolak guard `if (s.starting) return`.
 * Gejalanya: log berhenti total dan device stuck `disconnect` tanpa satu pun
 * baris error, sampai container di-restart manual.
 *
 * Mitigasi di sini:
 *  - **Timeout keras** lewat `Promise.race` → tidak pernah menggantung.
 *  - **Cache TTL** → reconnect beruntun tidak menghajar GitHub berulang kali.
 *  - **Fallback aman** → `undefined` berarti "pakai versi bawaan Baileys"
 *    (`DEFAULT_CONNECTION_CONFIG.version`), jadi koneksi tetap bisa jalan
 *    walaupun GitHub tidak terjangkau.
 */

/** Umur cache versi. Versi WA Web tidak berubah tiap menit. */
const VERSION_TTL_MS = 6 * 60 * 60 * 1000;

/** Batas tunggu fetch versi. Lewat ini → pakai cache / versi bawaan. */
const FETCH_TIMEOUT_MS = 5_000;

let cached: { version: WAVersion; at: number } | undefined;

/** Race promise dengan timer; `undefined` bila kelamaan. Tidak pernah reject. */
async function withTimeout<T>(p: Promise<T>, ms: number): Promise<T | undefined> {
  let timer: ReturnType<typeof setTimeout> | undefined;
  const guard = new Promise<undefined>((resolve) => {
    timer = setTimeout(() => resolve(undefined), ms);
  });
  try {
    // `.catch` mencegah unhandled rejection kalau p gagal setelah timeout menang.
    return await Promise.race([p.catch(() => undefined), guard]);
  } finally {
    if (timer) clearTimeout(timer);
  }
}

/**
 * Versi WA Web terbaru, atau `undefined` bila tidak bisa diambil tepat waktu.
 * Tidak pernah melempar dan tidak pernah menggantung.
 */
export async function getWaVersion(): Promise<WAVersion | undefined> {
  const now = Date.now();
  if (cached && now - cached.at < VERSION_TTL_MS) return cached.version;

  const res = await withTimeout(fetchLatestBaileysVersion(), FETCH_TIMEOUT_MS);
  const version = res?.version;

  if (!version) {
    if (cached) {
      logger.warn({ version: cached.version }, 'Fetch versi WA gagal/timeout — pakai cache lama');
      return cached.version;
    }
    logger.warn('Fetch versi WA gagal/timeout — pakai versi bawaan Baileys');
    return undefined;
  }

  cached = { version, at: now };
  return version;
}
