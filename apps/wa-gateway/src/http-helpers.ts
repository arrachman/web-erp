import type { Request, Response, NextFunction, RequestHandler } from 'express';
import { config } from './config';
import { registry, type DeviceRecord } from './registry';
import { logger } from './logger';

/** Request yang sudah lolos deviceAuth membawa record device-nya. */
export interface DeviceRequest extends Request {
  device?: DeviceRecord;
}

/** Bentuk response sukses ala Fonnte: `{ status: true, ...extra }`. */
export function ok(extra: Record<string, unknown> = {}): Record<string, unknown> {
  return { status: true, ...extra };
}

/** Bentuk response gagal ala Fonnte: `{ status: false, reason }`. */
export function fail(reason: string): Record<string, unknown> {
  return { status: false, reason };
}

/** Bungkus handler async → error ke-catch jadi response 500 ala Fonnte. */
export function asyncHandler(
  fn: (req: Request, res: Response) => Promise<void>,
): RequestHandler {
  return (req, res) => {
    fn(req, res).catch((err) => {
      logger.error({ err, path: req.path }, 'Handler error');
      res.status(500).json(fail(err instanceof Error ? err.message : 'internal error'));
    });
  };
}

/** Ambil token dari header Authorization (Fonnte pakai raw token, tanpa "Bearer"). */
function bearer(req: Request): string {
  const raw = req.header('authorization') ?? '';
  return raw.replace(/^Bearer\s+/i, '').trim();
}

/** Middleware endpoint level-akun: validasi account token. */
export function accountAuth(req: Request, res: Response, next: NextFunction): void {
  const token = bearer(req);
  if (!config.accountToken || token !== config.accountToken) {
    res.status(401).json(fail('invalid account token'));
    return;
  }
  next();
}

/** Middleware endpoint level-device: resolve device token → record. */
export function deviceAuth(req: DeviceRequest, res: Response, next: NextFunction): void {
  const token = bearer(req);
  const record = token ? registry.getByToken(token) : undefined;
  if (!record) {
    res.status(401).json(fail('invalid device token'));
    return;
  }
  req.device = record;
  next();
}
