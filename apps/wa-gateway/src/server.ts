import express, { type Express, type Request, type Response } from 'express';
import { pairingRouter } from './routes/pairing.routes';
import { messagingRouter } from './routes/messaging.routes';
import { sessionManager } from './session-manager';
import { logger } from './logger';

/** Bangun Express app dengan parser + routes (tanpa listen). */
export function createServer(): Express {
  const app = express();

  // Fonnte client kirim body sebagai x-www-form-urlencoded; dukung JSON juga.
  // Limit dinaikkan supaya lampiran dokumen base64 (invoice/bukti bayar PDF) muat.
  app.use(express.urlencoded({ extended: false, limit: '20mb' }));
  app.use(express.json({ limit: '20mb' }));

  // Healthcheck (untuk Docker / monitoring).
  app.get('/', (_req: Request, res: Response) => {
    res.json({ status: true, service: 'wa-gateway', ts: Date.now() });
  });

  /**
   * GET /health — status koneksi tiap device tanpa perlu device token.
   * Sengaja tanpa auth (service hanya listen localhost) supaya bisa dipakai
   * Docker healthcheck / monitoring luar. Tidak membocorkan isi pesan; token
   * dipotong agar tidak bisa dipakai ulang dari output monitoring.
   * `degraded` = ada device ter-pair yang sedang tidak connect.
   */
  app.get('/health', (_req: Request, res: Response) => {
    const devices = sessionManager.healthSnapshot().map((d) => ({
      ...d,
      token: `${d.token.slice(0, 6)}…`,
    }));
    const degraded = devices.some((d) => d.status !== 'connect');
    res.json({ status: true, service: 'wa-gateway', degraded, devices, ts: Date.now() });
  });

  // Endpoint Fonnte-compatible (account + device level).
  app.use('/', pairingRouter);
  app.use('/', messagingRouter);

  // 404 fallback ala Fonnte.
  app.use((req: Request, res: Response) => {
    logger.warn({ path: req.path, method: req.method }, 'Route tidak dikenal');
    res.status(404).json({ status: false, reason: 'unknown endpoint' });
  });

  return app;
}
