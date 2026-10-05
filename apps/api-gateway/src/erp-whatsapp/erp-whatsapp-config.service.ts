import { Injectable } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import { PrismaService } from '../prisma/prisma.service';

/**
 * Konfigurasi WhatsApp gateway — disimpan di `sys_settings`
 * (module=WHATSAPP, group=GATEWAY) supaya bisa diatur tanpa menyentuh .env.
 * Env `SENTIWA_API_URL` / `SENTIWA_ACCOUNT_TOKEN` (bila di-set) menimpa DB,
 * mengikuti pola cutover gateway self-hosted.
 *
 * Token akun & token device TIDAK pernah dikembalikan lewat API admin —
 * service ini hanya dipakai internal backend.
 */
@Injectable()
export class ErpWhatsappConfigService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly config: ConfigService,
  ) {}

  private async get(key: string): Promise<string | null> {
    const row = await this.prisma.erpSetting.findFirst({
      where: { module: 'WHATSAPP', group: 'GATEWAY', key, deletedAt: null },
      select: { value: true },
    });
    return row?.value ?? null;
  }

  private async set(key: string, value: string | null, dataType = 'string'): Promise<void> {
    await this.prisma.erpSetting.upsert({
      where: { module_group_key: { module: 'WHATSAPP', group: 'GATEWAY', key } },
      create: {
        module: 'WHATSAPP',
        group: 'GATEWAY',
        key,
        name: key,
        value,
        dataType,
        isActive: true,
      },
      update: { value, deletedAt: null },
    });
  }

  async gatewayUrl(): Promise<string> {
    const env = this.config.get<string>('SENTIWA_API_URL');
    if (env && env.trim() !== '') return env.replace(/\/+$/, '');
    const db = await this.get('GATEWAY_URL');
    return (db && db.trim() !== '' ? db : 'http://localhost:3204').replace(/\/+$/, '');
  }

  async accountToken(): Promise<string | null> {
    const env = this.config.get<string>('SENTIWA_ACCOUNT_TOKEN');
    if (env && env.trim() !== '') return env;
    const db = await this.get('ACCOUNT_TOKEN');
    return db && db.trim() !== '' ? db : null;
  }

  async webhookSecret(): Promise<string | null> {
    const db = await this.get('WEBHOOK_SECRET');
    return db && db.trim() !== '' ? db : null;
  }

  async activeDeviceToken(): Promise<string | null> {
    const db = await this.get('ACTIVE_DEVICE_TOKEN');
    if (db && db.trim() !== '') return db;
    const env = this.config.get<string>('SENTIWA_API_TOKEN');
    return env && env.trim() !== '' ? env : null;
  }

  async senderNumber(): Promise<string | null> {
    const db = await this.get('SENDER_NUMBER');
    return db && db.trim() !== '' ? db : null;
  }

  async sendEnabled(): Promise<boolean> {
    const db = await this.get('SEND_ENABLED');
    return db === 'true';
  }

  async setActiveDevice(token: string | null, senderNumber?: string | null): Promise<void> {
    await this.set('ACTIVE_DEVICE_TOKEN', token);
    if (senderNumber !== undefined) await this.set('SENDER_NUMBER', senderNumber);
  }

  async setSenderNumber(senderNumber: string | null): Promise<void> {
    await this.set('SENDER_NUMBER', senderNumber);
  }

  async setSendEnabled(enabled: boolean): Promise<void> {
    await this.set('SEND_ENABLED', enabled ? 'true' : 'false', 'boolean');
  }

  /** Nilai bebas di group WATCH (watermark notifier, dsb). */
  async watchValue(key: string): Promise<string | null> {
    const row = await this.prisma.erpSetting.findFirst({
      where: { module: 'WHATSAPP', group: 'WATCH', key, deletedAt: null },
      select: { value: true },
    });
    return row?.value ?? null;
  }

  async setWatchValue(key: string, value: string): Promise<void> {
    await this.prisma.erpSetting.upsert({
      where: { module_group_key: { module: 'WHATSAPP', group: 'WATCH', key } },
      create: {
        module: 'WHATSAPP',
        group: 'WATCH',
        key,
        name: key,
        value,
        dataType: 'string',
        isActive: true,
      },
      update: { value, deletedAt: null },
    });
  }

  /** Ringkasan aman untuk UI (tanpa nilai token). */
  async summary(): Promise<{
    gatewayUrl: string;
    accountConfigured: boolean;
    webhookSecretConfigured: boolean;
    activeDeviceConfigured: boolean;
    senderNumber: string | null;
    sendEnabled: boolean;
  }> {
    const [gatewayUrl, account, secret, device, sender, enabled] = await Promise.all([
      this.gatewayUrl(),
      this.accountToken(),
      this.webhookSecret(),
      this.activeDeviceToken(),
      this.senderNumber(),
      this.sendEnabled(),
    ]);
    return {
      gatewayUrl,
      accountConfigured: !!account,
      webhookSecretConfigured: !!secret,
      activeDeviceConfigured: !!device,
      senderNumber: sender,
      sendEnabled: enabled,
    };
  }
}
