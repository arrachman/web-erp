import type { AnyMessageContent } from '@whiskeysockets/baileys';
import type { Session } from './session-types';

/**
 * Helper pengiriman pesan WA.
 *
 * Dipisah dari SessionManager supaya file lifecycle koneksi tetap fokus (dan
 * di bawah batas 400 baris repo). Semua fungsi di sini sengaja TIDAK menyentuh
 * presence dan tidak pernah mengirim read receipt — itu yang menjaga notifikasi
 * di HP tetap menyala (lihat catatan `markOnlineOnConnect` di session-manager).
 */

/** Ubah nomor bebas-format jadi JID WhatsApp. Lempar bila tidak ada digit. */
function toJid(target: string): string {
  const digits = target.replace(/[^0-9]/g, '');
  if (!digits) throw new Error('target tidak valid');
  return `${digits}@s.whatsapp.net`;
}

/** Pastikan sesi siap kirim; lempar Error yang sama dengan perilaku lama. */
function assertConnected(s: Session | undefined): NonNullable<Session['sock']> {
  if (!s || s.status !== 'connect' || !s.sock) {
    throw new Error('device not connected');
  }
  return s.sock;
}

/** Kirim konten apa pun ke target; balas message id. */
async function send(
  s: Session | undefined,
  target: string,
  content: AnyMessageContent,
): Promise<{ id: string }> {
  const sock = assertConnected(s);
  const sent = await sock.sendMessage(toJid(target), content);
  const id = sent?.key?.id;
  if (!id) throw new Error('send returned no message id');
  return { id };
}

/** Kirim pesan teks biasa. */
export function sendTextMessage(
  s: Session | undefined,
  target: string,
  message: string,
): Promise<{ id: string }> {
  return send(s, target, { text: message });
}

/**
 * Kirim dokumen (PDF/file) + caption opsional. Dipakai untuk lampiran invoice
 * dan bukti pembayaran.
 */
export function sendDocumentMessage(
  s: Session | undefined,
  target: string,
  data: Buffer,
  fileName: string,
  mimetype: string,
  caption?: string,
): Promise<{ id: string }> {
  return send(s, target, {
    document: data,
    mimetype: mimetype || 'application/pdf',
    fileName: fileName || 'document.pdf',
    caption: caption || undefined,
  });
}
