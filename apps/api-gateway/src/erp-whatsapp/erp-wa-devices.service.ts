import {
  BadRequestException,
  Injectable,
  Logger,
  ServiceUnavailableException,
} from '@nestjs/common';
import { ErpWhatsappConfigService } from './erp-whatsapp-config.service';
import {
  extractDeviceList,
  gatewayPost,
  mapGatewayDevice,
  type GatewayResponse,
} from './wa-gateway-http.util';
import {
  ActivateWaDeviceDto,
  CreateWaDeviceDto,
  UpdateWaDeviceDto,
  WaDeviceQrDto,
} from './dto/erp-whatsapp.dto';

/**
 * Pairing device WhatsApp ke wa-gateway self-hosted (API kompatibel Fonnte).
 * Device tersimpan di registry gateway; backend hanya menyimpan token device
 * AKTIF + nomor pengirim di sys_settings.
 */
@Injectable()
export class ErpWaDevicesService {
  private readonly logger = new Logger(ErpWaDevicesService.name);

  constructor(private readonly settings: ErpWhatsappConfigService) {}

  private async requireAccountToken(): Promise<string> {
    const token = await this.settings.accountToken();
    if (!token) {
      throw new ServiceUnavailableException(
        'Token akun WA gateway belum diatur di server (sys_settings WHATSAPP.ACCOUNT_TOKEN).',
      );
    }
    return token;
  }

  private async post(
    path: string,
    authToken: string,
    body: Record<string, string>,
  ): Promise<GatewayResponse> {
    const apiUrl = await this.settings.gatewayUrl();
    return gatewayPost(apiUrl, path, authToken, body);
  }

  async listDevices() {
    const accountToken = await this.requireAccountToken();
    const { ok, json } = await this.post('/get-devices', accountToken, {});
    const active = await this.settings.activeDeviceToken();
    const devices = extractDeviceList(json).map((d) => mapGatewayDevice(d, active));
    if (!ok && devices.length === 0) {
      throw new BadRequestException(
        `WA gateway /get-devices gagal: ${typeof json.reason === 'string' ? json.reason : 'unknown'}`,
      );
    }
    return { success: true, data: { devices, hasActiveDevice: !!active } };
  }

  async addDevice(dto: CreateWaDeviceDto) {
    const accountToken = await this.requireAccountToken();
    const { ok, json } = await this.post('/add-device', accountToken, {
      name: dto.name,
      device: dto.phone,
      autoread: dto.autoread === 'on' ? 'true' : 'false',
    });
    const token = typeof json.token === 'string' ? json.token : undefined;
    if (ok && token) {
      return {
        success: true,
        data: {
          deviceToken: token,
          devicePhone: typeof json.device === 'string' ? json.device : dto.phone,
        },
      };
    }
    // Gateway idempoten per nomor — bila token tidak kembali, cari dari list.
    const list = await this.post('/get-devices', accountToken, {});
    const existing = extractDeviceList(list.json).find(
      (d) => d.device === dto.phone || d.device === `+${dto.phone}`,
    );
    if (existing && typeof existing.token === 'string') {
      return {
        success: true,
        data: { deviceToken: existing.token, devicePhone: dto.phone, reused: true },
      };
    }
    throw new BadRequestException(
      `WA gateway /add-device gagal: ${typeof json.reason === 'string' ? json.reason : 'token device tidak diterima'}`,
    );
  }

  async getDeviceQr(dto: WaDeviceQrDto) {
    // Cek status aktual dulu — device yang sudah connect tidak perlu QR.
    try {
      const status = await this.post('/device', dto.deviceToken, {});
      if (status.json.status === true && status.json.device_status === 'connect') {
        if (!dto.force) return { success: true, data: { alreadyConnected: true } };
        await this.post('/disconnect', dto.deviceToken, {}).catch(() => undefined);
      }
    } catch {
      // abaikan — lanjut minta QR
    }
    const qr = await this.post('/qr', dto.deviceToken, {});
    const reason = typeof qr.json.reason === 'string' ? qr.json.reason : '';
    const rawUrl = typeof qr.json.url === 'string' ? qr.json.url : undefined;
    if (!rawUrl && /already connect/i.test(reason)) {
      if (!dto.force) return { success: true, data: { alreadyConnected: true } };
      await this.post('/disconnect', dto.deviceToken, {}).catch(() => undefined);
      const retry = await this.post('/qr', dto.deviceToken, {});
      const retryUrl = typeof retry.json.url === 'string' ? retry.json.url : undefined;
      if (retryUrl) return { success: true, data: { qrUrl: this.asImgSrc(retryUrl) } };
      throw new BadRequestException(
        `WA gateway /qr gagal: ${typeof retry.json.reason === 'string' ? retry.json.reason : reason}`,
      );
    }
    if (!rawUrl) {
      // "QR belum siap" dari gateway = sesi Baileys masih mulai; frontend retry.
      throw new BadRequestException(`WA gateway /qr gagal: ${reason || 'QR tidak diterima'}`);
    }
    return { success: true, data: { qrUrl: this.asImgSrc(rawUrl) } };
  }

  private asImgSrc(url: string): string {
    return url.startsWith('data:') ? url : `data:image/png;base64,${url}`;
  }

  async checkDeviceConnected(dto: WaDeviceQrDto) {
    try {
      const { json } = await this.post('/device', dto.deviceToken, {});
      return {
        success: true,
        data: {
          connected: json.status === true && json.device_status === 'connect',
          devicePhone: typeof json.device === 'string' ? json.device : undefined,
          deviceName: typeof json.name === 'string' ? json.name : undefined,
        },
      };
    } catch (err) {
      return {
        success: true,
        data: {
          connected: false,
          error: err instanceof Error ? err.message : String(err),
        },
      };
    }
  }

  async activateDevice(dto: ActivateWaDeviceDto) {
    // Verifikasi device benar-benar connect sebelum diaktifkan.
    const status = await this.post('/device', dto.deviceToken, {}).catch(() => null);
    if (!status || status.json.status !== true || status.json.device_status !== 'connect') {
      throw new BadRequestException(
        'Device belum terhubung. Scan QR via WhatsApp → Perangkat tertaut dulu, lalu aktifkan ulang.',
      );
    }
    // Bersihkan device aktif sebelumnya dari gateway (default) supaya tidak menumpuk.
    const removePrev = dto.removePrevious ?? true;
    if (removePrev) {
      const accountToken = await this.settings.accountToken();
      const prevToken = await this.settings.activeDeviceToken();
      if (accountToken && prevToken && prevToken !== dto.deviceToken) {
        try {
          const list = await this.post('/get-devices', accountToken, {});
          const prev = extractDeviceList(list.json).find((d) => d.token === prevToken);
          await this.post('/disconnect', prevToken, {}).catch(() => undefined);
          if (prev && typeof prev.device === 'string') {
            await this.post('/delete-device', accountToken, { device: prev.device });
          }
        } catch (err) {
          this.logger.warn(
            `Cleanup device WA lama gagal: ${err instanceof Error ? err.message : String(err)}`,
          );
        }
      }
    }
    const phone =
      dto.devicePhone ??
      (typeof status.json.device === 'string' ? status.json.device : undefined);
    await this.settings.setActiveDevice(dto.deviceToken, phone ?? null);
    return {
      success: true,
      data: { activeDeviceConfigured: true, senderNumber: phone ?? null },
      message: 'Device WhatsApp diaktifkan',
    };
  }

  async updateDevice(dto: UpdateWaDeviceDto) {
    const { ok, json } = await this.post('/update-device', dto.deviceToken, {
      name: dto.name,
      device: dto.phone,
    });
    if (!ok) {
      throw new BadRequestException(
        `WA gateway /update-device gagal: ${typeof json.reason === 'string' ? json.reason : 'unknown'}`,
      );
    }
    const active = await this.settings.activeDeviceToken();
    if (active && active === dto.deviceToken) {
      await this.settings.setSenderNumber(dto.phone);
    }
    return { success: true, message: 'Device diperbarui' };
  }

  async removeDevice(devicePhone: string) {
    const accountToken = await this.requireAccountToken();
    const list = await this.post('/get-devices', accountToken, {});
    const target = extractDeviceList(list.json).find(
      (d) => d.device === devicePhone || d.device === `+${devicePhone}`,
    );
    if (target && typeof target.token === 'string') {
      await this.post('/disconnect', target.token, {}).catch(() => undefined);
    }
    const del = await this.post('/delete-device', accountToken, { device: devicePhone });
    if (!del.ok) {
      throw new BadRequestException(
        `WA gateway /delete-device gagal: ${typeof del.json.reason === 'string' ? del.json.reason : 'unknown'}`,
      );
    }
    const sender = await this.settings.senderNumber();
    if (sender === devicePhone || sender === `+${devicePhone}`) {
      await this.settings.setActiveDevice(null, '');
    }
    return { success: true, message: 'Device dihapus' };
  }

  async activeStatus() {
    const token = await this.settings.activeDeviceToken();
    if (!token) {
      return { success: true, data: { connected: false, reason: 'Belum ada device aktif' } };
    }
    try {
      const { json } = await this.post('/device', token, {});
      return {
        success: true,
        data: {
          connected: json.status === true && json.device_status === 'connect',
          devicePhone: typeof json.device === 'string' ? json.device : undefined,
          deviceName: typeof json.name === 'string' ? json.name : undefined,
        },
      };
    } catch (err) {
      return {
        success: true,
        data: {
          connected: false,
          reason: err instanceof Error ? err.message : String(err),
        },
      };
    }
  }
}
