import { Injectable } from '@nestjs/common';
import { PrismaService } from '../prisma/prisma.service';
import {
  DEFAULT_FORMAT_SETTINGS,
  parseFormatSettings,
  type ReportFormatSettings,
} from './report-format';

/**
 * Loads report format settings from sys_settings (group 'company',
 * keys report_format_*), seeded by migration 20261005_033. Falls back to
 * id-ID defaults when the keys are absent. Also loads the company context
 * used by converted .mrt headers (PTNAMA etc.).
 */
@Injectable()
export class ReportFormatService {
  constructor(private readonly prisma: PrismaService) {}

  async getSettings(): Promise<ReportFormatSettings> {
    try {
      const rows = await this.prisma.erpSetting.findMany({ where: { group: 'company' } });
      const map: Record<string, string> = {};
      for (const r of rows) if (r.value != null) map[r.key] = String(r.value);
      return parseFormatSettings(map);
    } catch {
      return DEFAULT_FORMAT_SETTINGS;
    }
  }

  async getCompany(): Promise<{
    name: string;
    address?: string;
    city?: string;
    phone?: string;
    email?: string;
    npwp?: string;
    logoUrl?: string;
  }> {
    try {
      const rows = await this.prisma.erpSetting.findMany({ where: { group: 'company' } });
      const map: Record<string, string> = {};
      for (const r of rows) if (r.value != null) map[r.key] = String(r.value);
      const address = [map.address_line1, map.address_line2].filter(Boolean).join(' ');
      return {
        name: map.name ?? 'Perusahaan',
        address: address || undefined,
        city: map.city,
        phone: map.phone,
        email: map.email,
        npwp: map.npwp,
        logoUrl: map.logo_url,
      };
    } catch {
      return { name: 'Perusahaan' };
    }
  }
}
