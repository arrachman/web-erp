import { config } from './config';
import { logger } from './logger';

/**
 * Event delivery yang di-POST balik ke api-gateway (format kompatibel dengan
 * SentiWaWebhookDto / ClinicWaService.handleWebhook):
 *   { id, sender, status: sent|delivered|read|failed, device, reason? }
 * `id`     = message id (sama dengan yang dibalas /send)
 * `sender` = nomor lawan (recipient) format 62xxx → dipakai fallback match log
 */
export interface DeliveryEvent {
  id?: string;
  sender?: string;
  status: 'sent' | 'delivered' | 'read' | 'failed';
  device?: string;
  reason?: string;
}

/** POST event ke WA_GATEWAY_WEBHOOK_URL (fire-and-forget, tidak melempar error). */
export async function emitWebhook(ev: DeliveryEvent): Promise<void> {
  if (!config.webhookUrl) return; // belum dikonfigurasi → no-op
  try {
    const headers: Record<string, string> = { 'Content-Type': 'application/json' };
    if (config.webhookSecret) headers['X-Webhook-Secret'] = config.webhookSecret;
    const res = await fetch(config.webhookUrl, {
      method: 'POST',
      headers,
      body: JSON.stringify(ev),
    });
    if (!res.ok) {
      logger.warn({ httpStatus: res.status, ev }, 'Webhook balas non-2xx');
    }
  } catch (err) {
    logger.warn({ err, ev }, 'Webhook POST gagal');
  }
}
