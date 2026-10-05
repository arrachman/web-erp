import { Router, type Request, type Response } from 'express';
import { registry } from '../registry';
import { sessionManager } from '../session-manager';
import {
  accountAuth,
  deviceAuth,
  asyncHandler,
  ok,
  fail,
  type DeviceRequest,
} from '../http-helpers';

export const pairingRouter = Router();

// ───────────────────────── Account-level (account token) ─────────────────────

/** POST /get-devices — list semua device + status live. */
pairingRouter.post(
  '/get-devices',
  accountAuth,
  (_req: Request, res: Response) => {
    const data = registry.list().map((r) => {
      const st = sessionManager.getStatus(r.token);
      return {
        name: r.name,
        device: r.phone,
        status: st.connected ? 'connect' : 'disconnect',
        token: r.token,
        quota: 999999,
        package: 'self-host',
        autoread: r.autoread ? 'true' : 'false',
      };
    });
    res.json(ok({ data }));
  },
);

/** POST /add-device — daftarkan device baru; balas device token. Idempoten by phone. */
pairingRouter.post('/add-device', accountAuth, (req: Request, res: Response) => {
  const name = String(req.body?.name ?? '').trim();
  const phone = String(req.body?.device ?? '').trim();
  const autoread = String(req.body?.autoread ?? 'false') === 'true';
  if (!name || !phone) {
    res.status(400).json(fail('name & device wajib diisi'));
    return;
  }
  const record = registry.add({ name, phone, autoread });
  res.json(ok({ token: record.token, device: record.phone, name: record.name }));
});

/** POST /delete-device — hapus device (disconnect + buang creds + registry). */
pairingRouter.post(
  '/delete-device',
  accountAuth,
  asyncHandler(async (req, res) => {
    const phone = String(req.body?.device ?? '').trim();
    const record = phone ? registry.getByPhone(phone) : undefined;
    if (record) {
      await sessionManager.destroy(record.token);
      registry.remove(record.token);
    }
    res.json(ok()); // idempotent — sukses walau device tidak ditemukan
  }),
);

// ───────────────────────── Device-level (device token) ───────────────────────

/** POST /qr — ambil QR pairing. Connected → reason 'device already connect'. */
pairingRouter.post(
  '/qr',
  deviceAuth,
  asyncHandler(async (req: DeviceRequest, res) => {
    const token = req.device!.token;
    const { qrUrl, alreadyConnected } = await sessionManager.getQr(token);
    if (alreadyConnected) {
      res.json(fail('device already connect'));
      return;
    }
    if (!qrUrl) {
      res.json(fail('QR belum siap, coba lagi'));
      return;
    }
    res.json(ok({ url: qrUrl }));
  }),
);

/** POST /device — status device (dipakai juga untuk polling /check oleh app). */
pairingRouter.post('/device', deviceAuth, (req: DeviceRequest, res: Response) => {
  const token = req.device!.token;
  const st = sessionManager.getStatus(token);
  res.json(
    ok({
      device_status: st.connected ? 'connect' : 'disconnect',
      device: st.phone,
      name: st.name,
    }),
  );
});

/** POST /disconnect — putuskan sesi (logout) supaya bisa pair akun WA lain. */
pairingRouter.post(
  '/disconnect',
  deviceAuth,
  asyncHandler(async (req: DeviceRequest, res) => {
    await sessionManager.logout(req.device!.token);
    res.json(ok());
  }),
);

/** POST /update-device — edit label nama/nomor device (tidak re-pair). */
pairingRouter.post('/update-device', deviceAuth, (req: DeviceRequest, res: Response) => {
  const token = req.device!.token;
  const name = req.body?.name !== undefined ? String(req.body.name).trim() : undefined;
  const phone = req.body?.device !== undefined ? String(req.body.device).trim() : undefined;
  const updated = registry.update(token, { name, phone });
  if (!updated) {
    res.status(404).json(fail('device tidak ditemukan'));
    return;
  }
  res.json(ok());
});
