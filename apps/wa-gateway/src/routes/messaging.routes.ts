import { Router, type Response } from 'express';
import { sessionManager } from '../session-manager';
import { deviceAuth, asyncHandler, ok, fail, type DeviceRequest } from '../http-helpers';

export const messagingRouter = Router();

/**
 * POST /send — kirim pesan teks WA (device token di Authorization).
 * Body form-urlencoded ala Fonnte: `target`, `message` (+ `device`, `webhook`
 * diabaikan; webhook global pakai WA_GATEWAY_WEBHOOK_URL).
 *
 * Sukses → { status: true, id }. Gagal → { status: false, reason } (HTTP 200,
 * sama seperti Fonnte; SentiWaProvider deteksi via json.status === false).
 */
messagingRouter.post(
  '/send',
  deviceAuth,
  asyncHandler(async (req: DeviceRequest, res: Response) => {
    const target = String(req.body?.target ?? '').trim();
    const message = String(req.body?.message ?? '');
    if (!target || !message) {
      res.status(400).json(fail('target & message wajib diisi'));
      return;
    }
    try {
      const { id } = await sessionManager.sendText(req.device!.token, target, message);
      res.json(ok({ id }));
    } catch (err) {
      res.json(fail(err instanceof Error ? err.message : 'send failed'));
    }
  }),
);

/**
 * POST /send-media — kirim dokumen (PDF/file) sebagai lampiran WA.
 * Body JSON: `target`, `file` (base64, boleh data-URL), `filename`, `mimetype`
 * (default application/pdf), `caption` (opsional, jadi teks pesan).
 *
 * Sukses → { status: true, id }. Gagal → { status: false, reason } (HTTP 200).
 */
messagingRouter.post(
  '/send-media',
  deviceAuth,
  asyncHandler(async (req: DeviceRequest, res: Response) => {
    const target = String(req.body?.target ?? '').trim();
    const fileRaw = String(req.body?.file ?? '');
    const filename = String(req.body?.filename ?? 'document.pdf');
    const mimetype = String(req.body?.mimetype ?? 'application/pdf');
    const caption = req.body?.caption ? String(req.body.caption) : undefined;
    if (!target || !fileRaw) {
      res.status(400).json(fail('target & file wajib diisi'));
      return;
    }
    // Buang prefix data-URL kalau ada (data:application/pdf;base64,xxxx).
    const base64 = fileRaw.includes(',') ? fileRaw.slice(fileRaw.indexOf(',') + 1) : fileRaw;
    let buffer: Buffer;
    try {
      buffer = Buffer.from(base64, 'base64');
    } catch {
      res.status(400).json(fail('file base64 tidak valid'));
      return;
    }
    try {
      const { id } = await sessionManager.sendDocument(
        req.device!.token,
        target,
        buffer,
        filename,
        mimetype,
        caption,
      );
      res.json(ok({ id }));
    } catch (err) {
      res.json(fail(err instanceof Error ? err.message : 'send media failed'));
    }
  }),
);
