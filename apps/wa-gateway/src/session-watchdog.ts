import { logger } from './logger';
import { registry } from './registry';
import { type Session, WATCHDOG_INTERVAL_MS, STUCK_AFTER_MS } from './session-types';

/** Kemampuan SessionManager yang dibutuhkan watchdog. */
export interface WatchdogHost {
  /** Sesi in-memory untuk token, bila sudah pernah dibuat. */
  peek(token: string): Session | undefined;
  /** True bila token punya creds tersimpan (sudah pernah pairing). */
  hasStoredCreds(token: string): boolean;
  /** Mulai / pastikan koneksi untuk token. */
  start(token: string): Promise<unknown>;
  /** Tandai kemajuan supaya sesi tidak dianggap macet lagi. */
  touch(s: Session): void;
}

/**
 * Jaring pengaman terakhir untuk sesi yang berhenti diam-diam.
 *
 * Backoff + timeout + perbaikan guard `start()` sudah menutup jalur macet yang
 * diketahui dari insiden 29 Jul 2026, tapi Baileys punya banyak jalur internal
 * yang bisa berhenti tanpa memancarkan event. Watchdog memaksa start ulang sesi
 * yang seharusnya hidup namun tidak connect dan tidak punya reconnect terjadwal.
 *
 * Tanpa ini, insiden tadi baru ketahuan setelah ~4 jam WA mati tanpa satu pun
 * baris log — hanya ketahuan karena ada yang membuka halaman admin.
 */
export function startWatchdog(host: WatchdogHost): void {
  const timer = setInterval(() => tick(host), WATCHDOG_INTERVAL_MS);
  timer.unref?.(); // jangan menahan proses keluar
}

function tick(host: WatchdogHost): void {
  const now = Date.now();
  for (const r of registry.list()) {
    if (!host.hasStoredCreds(r.token)) continue; // belum pernah pair → biarkan

    const s = host.peek(r.token);
    if (!s) {
      logger.warn({ token: r.token }, 'Watchdog: sesi hilang, mulai ulang');
      void host.start(r.token).catch(() => undefined);
      continue;
    }

    if (s.status === 'connect' || s.manualLogout) continue;
    if (s.reconnectTimer) continue; // sudah ada rencana reconnect
    if (now - s.lastProgressAt < STUCK_AFTER_MS) continue;

    // `starting` yang menyangkut > STUCK_AFTER_MS = flag basi (percobaan connect
    // mati tanpa sempat melepasnya). Jangan skip — justru inilah kasus yang
    // harus dipulihkan; paksa lepas supaya start() tidak diblokir selamanya.
    if (s.starting) {
      logger.warn({ token: r.token }, 'Watchdog: flag starting basi, dilepas');
      s.starting = false;
    }

    logger.warn(
      { token: r.token, status: s.status, idleMs: now - s.lastProgressAt },
      'Watchdog: sesi macet, paksa reconnect',
    );
    host.touch(s);
    void host.start(r.token).catch((err) =>
      logger.error({ err, token: r.token }, 'Watchdog reconnect gagal'),
    );
  }
}
