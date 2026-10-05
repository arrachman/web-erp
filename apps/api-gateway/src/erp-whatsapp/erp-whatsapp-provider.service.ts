import { Injectable, Logger } from '@nestjs/common';
import { ErpWhatsappConfigService } from './erp-whatsapp-config.service';
import { normalizePhoneId } from './wa-phone.util';
import { gatewayPost } from './wa-gateway-http.util';

export interface WaSendResult {
  messageId: string;
  status: 'sent' | 'failed';
  errorReason?: string;
  providerResponse?: unknown;
}

/**
 * Pengirim pesan ke wa-gateway self-hosted (`/send`, auth = token device aktif).
 * Tidak pernah melempar error transient — hasil gagal dikembalikan sebagai
 * SendResult supaya pemanggil bisa mencatat log dengan rapi.
 */
@Injectable()
export class ErpWhatsappProvider {
  private readonly logger = new Logger(ErpWhatsappProvider.name);

  constructor(private readonly settings: ErpWhatsappConfigService) {}

  async send(toPhone: string, body: string): Promise<WaSendResult> {
    const token = await this.settings.activeDeviceToken();
    if (!token) {
      return {
        messageId: `wa_unconfigured_${Date.now()}`,
        status: 'failed',
        errorReason: 'Belum ada device WhatsApp aktif. Pairing device dulu di Pengaturan WhatsApp.',
      };
    }
    const apiUrl = await this.settings.gatewayUrl();
    const target = normalizePhoneId(toPhone) ?? toPhone;
    try {
      const { ok, json } = await gatewayPost(apiUrl, '/send', token, {
        target,
        message: body,
      });
      if (!ok || json.status === false) {
        return {
          messageId: `wa_fail_${Date.now()}`,
          status: 'failed',
          errorReason:
            (typeof json.reason === 'string' && json.reason) || 'Gateway menolak pengiriman',
          providerResponse: json,
        };
      }
      const rawId = Array.isArray(json.id) ? json.id[0] : json.id;
      return {
        messageId: rawId !== undefined && rawId !== null ? String(rawId) : `wa_${Date.now()}`,
        status: 'sent',
        providerResponse: json,
      };
    } catch (error) {
      const message = error instanceof Error ? error.message : String(error);
      this.logger.error(`WA send gagal: ${message}`);
      return { messageId: `wa_error_${Date.now()}`, status: 'failed', errorReason: message };
    }
  }

  async sendMedia(params: {
    toPhone: string;
    data: Buffer;
    fileName: string;
    mimetype?: string;
    caption?: string;
  }): Promise<WaSendResult> {
    const token = await this.settings.activeDeviceToken();
    if (!token) {
      return {
        messageId: `wa_unconfigured_${Date.now()}`,
        status: 'failed',
        errorReason: 'Belum ada device WhatsApp aktif. Pairing device dulu di Pengaturan WhatsApp.',
      };
    }
    const apiUrl = await this.settings.gatewayUrl();
    const target = normalizePhoneId(params.toPhone) ?? params.toPhone;
    try {
      const res = await fetch(`${apiUrl}/send-media`, {
        method: 'POST',
        headers: { Authorization: token, 'Content-Type': 'application/json' },
        body: JSON.stringify({
          target,
          file: params.data.toString('base64'),
          filename: params.fileName,
          mimetype: params.mimetype || 'application/pdf',
          caption: params.caption || '',
        }),
      });
      const json = (await res.json().catch(() => ({}))) as Record<string, unknown>;
      if (!res.ok || json.status === false) {
        return {
          messageId: `wa_fail_${Date.now()}`,
          status: 'failed',
          errorReason:
            (typeof json.reason === 'string' && json.reason) || `HTTP ${res.status}`,
          providerResponse: json,
        };
      }
      const rawId = Array.isArray(json.id) ? json.id[0] : json.id;
      return {
        messageId: rawId !== undefined && rawId !== null ? String(rawId) : `wa_${Date.now()}`,
        status: 'sent',
        providerResponse: json,
      };
    } catch (error) {
      const message = error instanceof Error ? error.message : String(error);
      this.logger.error(`WA sendMedia gagal: ${message}`);
      return { messageId: `wa_error_${Date.now()}`, status: 'failed', errorReason: message };
    }
  }
}
