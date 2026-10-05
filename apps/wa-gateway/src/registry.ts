import { randomUUID } from 'node:crypto';
import fs from 'node:fs';
import path from 'node:path';
import { config } from './config';
import { logger } from './logger';

/** Metadata satu device (1 record = 1 sesi WhatsApp / nomor). */
export interface DeviceRecord {
  /** Device token unik — dipakai api-gateway sebagai Authorization per device. */
  token: string;
  /** Nama label device (ditampilkan di Linked Devices & UI Pengaturan). */
  name: string;
  /** Nomor label (dari /add-device). Nomor WA aktual yg connect bisa beda. */
  phone: string;
  /** Setara field autoread Fonnte (di gateway ini: selalu non-aktif → notif HP aman). */
  autoread: boolean;
  createdAt: string;
}

/** Bandingkan nomor toleran terhadap prefix `+` dan spasi. */
function samePhone(a: string, b: string): boolean {
  const norm = (s: string) => s.replace(/[^0-9]/g, '');
  return norm(a) === norm(b);
}

/**
 * Registry device berbasis file JSON (atomic write). Jumlah device kecil
 * (klinik tunggal) → file cukup, tidak perlu DB.
 */
class DeviceRegistry {
  private records: DeviceRecord[] = [];
  private loaded = false;

  private ensureLoaded(): void {
    if (this.loaded) return;
    try {
      if (fs.existsSync(config.registryFile)) {
        const raw = fs.readFileSync(config.registryFile, 'utf8');
        const parsed = JSON.parse(raw);
        if (Array.isArray(parsed)) this.records = parsed as DeviceRecord[];
      }
    } catch (err) {
      logger.error({ err }, 'Gagal load registry.json — mulai dari kosong');
      this.records = [];
    }
    this.loaded = true;
  }

  private persist(): void {
    fs.mkdirSync(path.dirname(config.registryFile), { recursive: true });
    const tmp = `${config.registryFile}.tmp`;
    fs.writeFileSync(tmp, JSON.stringify(this.records, null, 2));
    fs.renameSync(tmp, config.registryFile); // atomic replace
  }

  list(): DeviceRecord[] {
    this.ensureLoaded();
    return [...this.records];
  }

  getByToken(token: string): DeviceRecord | undefined {
    this.ensureLoaded();
    return this.records.find((r) => r.token === token);
  }

  getByPhone(phone: string): DeviceRecord | undefined {
    this.ensureLoaded();
    return this.records.find((r) => samePhone(r.phone, phone));
  }

  /** Tambah device baru; generate token. Idempoten by phone. */
  add(input: { name: string; phone: string; autoread?: boolean }): DeviceRecord {
    this.ensureLoaded();
    const existing = this.getByPhone(input.phone);
    if (existing) return existing;
    const record: DeviceRecord = {
      token: randomUUID().replace(/-/g, ''),
      name: input.name,
      phone: input.phone,
      autoread: input.autoread ?? false,
      createdAt: new Date().toISOString(),
    };
    this.records.push(record);
    this.persist();
    return record;
  }

  update(token: string, patch: Partial<Pick<DeviceRecord, 'name' | 'phone'>>): DeviceRecord | undefined {
    this.ensureLoaded();
    const record = this.records.find((r) => r.token === token);
    if (!record) return undefined;
    if (patch.name !== undefined) record.name = patch.name;
    if (patch.phone !== undefined) record.phone = patch.phone;
    this.persist();
    return record;
  }

  remove(token: string): boolean {
    this.ensureLoaded();
    const before = this.records.length;
    this.records = this.records.filter((r) => r.token !== token);
    if (this.records.length === before) return false;
    this.persist();
    return true;
  }
}

export const registry = new DeviceRegistry();
