import { PrismaService } from '../prisma/prisma.service';
import { toAuditUserId } from '../common/utils/audit-user.util';

/**
 * Entity adapter registry — the core of the importer.
 * Each adapter declares its headers, maps a parsed row to insert data, and
 * performs the Prisma insert. Required NOT NULL columns that cannot be
 * defaulted become required headers. Entities whose hard FK requirements
 * cannot be satisfied from a flat file are intentionally omitted.
 */

export interface ImportRow {
  [header: string]: string;
}

export interface EntityAdapter {
  label: string;
  requiredHeaders: string[];
  optionalHeaders: string[];
  /** Map a validated row of cells (by header) into a plain data object. */
  rowToData(row: ImportRow): Record<string, unknown>;
  /** Insert one record. May throw (duplicate, FK) — caller catches per row. */
  insert(prisma: PrismaService, data: Record<string, unknown>, actorId?: string): Promise<void>;
}

// ─── Helpers ────────────────────────────────────────────────────────────────

function audit(actorId?: string) {
  return { createdById: toAuditUserId(actorId), updatedById: toAuditUserId(actorId) };
}

function str(row: ImportRow, key: string): string {
  return (row[key] ?? '').trim();
}

function optStr(row: ImportRow, key: string): string | undefined {
  const v = (row[key] ?? '').trim();
  return v === '' ? undefined : v;
}

function toBool(v: string | undefined, fallback = true): boolean {
  if (v === undefined || v.trim() === '') return fallback;
  const s = v.trim().toLowerCase();
  return ['1', 'true', 'yes', 'y', 'aktif', 'active'].includes(s);
}

function toIntOrThrow(v: string | undefined, label: string): number {
  const n = Number((v ?? '').trim());
  if (!Number.isFinite(n)) throw new Error(`${label} harus berupa angka`);
  return Math.trunc(n);
}

function toNumOrThrow(v: string | undefined, label: string): number {
  const n = Number((v ?? '').trim());
  if (!Number.isFinite(n)) throw new Error(`${label} harus berupa angka`);
  return n;
}

const ACCOUNT_TYPES = ['ASSET', 'LIABILITY', 'EQUITY', 'REVENUE', 'EXPENSE'];
const ACCOUNT_KINDS = ['HEADER', 'POSTABLE'];
const NORMAL_BALANCES = ['DEBIT', 'CREDIT'];

// ─── Registry ─────────────────────────────────────────────────────────────────

export const IMPORT_ADAPTERS: Record<string, EntityAdapter> = {
  units: {
    label: 'Satuan (Units)',
    requiredHeaders: ['code', 'name'],
    optionalHeaders: ['conversionFactor', 'isActive'],
    rowToData(row) {
      return {
        code: str(row, 'code'),
        name: str(row, 'name'),
        conversionFactor: optStr(row, 'conversionFactor')
          ? toNumOrThrow(row.conversionFactor, 'conversionFactor')
          : 1,
        isActive: toBool(optStr(row, 'isActive')),
      };
    },
    async insert(prisma, data, actorId) {
      await prisma.erpUnit.create({ data: { ...data, ...audit(actorId) } as never });
    },
  },

  currencies: {
    label: 'Mata Uang',
    requiredHeaders: ['code', 'name'],
    optionalHeaders: ['symbol', 'isActive'],
    rowToData(row) {
      return {
        code: str(row, 'code'),
        name: str(row, 'name'),
        symbol: optStr(row, 'symbol'),
        isActive: toBool(optStr(row, 'isActive')),
      };
    },
    async insert(prisma, data, actorId) {
      await prisma.erpCurrency.create({ data: { ...data, ...audit(actorId) } as never });
    },
  },

  'item-categories': {
    label: 'Kategori Barang',
    requiredHeaders: ['code', 'name'],
    optionalHeaders: ['isActive'],
    rowToData(row) {
      return {
        code: str(row, 'code'),
        name: str(row, 'name'),
        isActive: toBool(optStr(row, 'isActive')),
      };
    },
    async insert(prisma, data, actorId) {
      await prisma.erpItemCategory.create({ data: { ...data, ...audit(actorId) } as never });
    },
  },

  taxes: {
    label: 'Pajak',
    requiredHeaders: ['code', 'name', 'rate'],
    optionalHeaders: ['isActive'],
    rowToData(row) {
      return {
        code: str(row, 'code'),
        name: str(row, 'name'),
        rate: toNumOrThrow(row.rate, 'rate'),
        isActive: toBool(optStr(row, 'isActive')),
      };
    },
    async insert(prisma, data, actorId) {
      await prisma.erpTax.create({ data: { ...data, ...audit(actorId) } as never });
    },
  },

  'payment-terms': {
    label: 'Termin Pembayaran',
    requiredHeaders: ['code', 'name', 'netDays'],
    optionalHeaders: ['isActive'],
    rowToData(row) {
      return {
        code: str(row, 'code'),
        name: str(row, 'name'),
        netDays: toIntOrThrow(row.netDays, 'netDays'),
        isActive: toBool(optStr(row, 'isActive')),
      };
    },
    async insert(prisma, data, actorId) {
      await prisma.erpPaymentTerm.create({ data: { ...data, ...audit(actorId) } as never });
    },
  },

  branches: {
    label: 'Cabang',
    requiredHeaders: ['code', 'name'],
    optionalHeaders: ['city', 'phone', 'isActive'],
    rowToData(row) {
      return {
        code: str(row, 'code'),
        name: str(row, 'name'),
        city: optStr(row, 'city'),
        phone: optStr(row, 'phone'),
        isActive: toBool(optStr(row, 'isActive')),
      };
    },
    async insert(prisma, data, actorId) {
      await prisma.erpBranch.create({ data: { ...data, ...audit(actorId) } as never });
    },
  },

  partners: {
    label: 'Partner / Kontak',
    requiredHeaders: ['code', 'name'],
    optionalHeaders: ['partnerTypeCode', 'taxNumber', 'isActive'],
    rowToData(row) {
      return {
        code: str(row, 'code'),
        name: str(row, 'name'),
        partnerTypeCode: optStr(row, 'partnerTypeCode'),
        taxNumber: optStr(row, 'taxNumber'),
        isActive: toBool(optStr(row, 'isActive')),
      };
    },
    async insert(prisma, data, actorId) {
      const { partnerTypeCode, ...partner } = data as Record<string, unknown>;
      if (!partnerTypeCode) {
        throw new Error('partnerTypeCode is required for partner import');
      }
      const partnerType = await prisma.erpPartnerType.findFirst({
        where: { code: String(partnerTypeCode), deletedAt: null },
      });
      if (!partnerType) {
        throw new Error(`Unknown partnerTypeCode: ${String(partnerTypeCode)}`);
      }
      await prisma.erpPartner.create({
        data: {
          ...partner,
          partnerTypeId: partnerType.id,
          ...audit(actorId),
        } as never,
      });
    },
  },

  accounts: {
    label: 'Akun (Chart of Accounts)',
    requiredHeaders: ['code', 'name', 'type', 'kind', 'normalBalance', 'level'],
    optionalHeaders: ['isActive'],
    rowToData(row) {
      const type = str(row, 'type').toUpperCase();
      const kind = str(row, 'kind').toUpperCase();
      const normalBalance = str(row, 'normalBalance').toUpperCase();
      if (!ACCOUNT_TYPES.includes(type)) {
        throw new Error(`type tidak valid (pilih: ${ACCOUNT_TYPES.join('/')})`);
      }
      if (!ACCOUNT_KINDS.includes(kind)) {
        throw new Error(`kind tidak valid (pilih: ${ACCOUNT_KINDS.join('/')})`);
      }
      if (!NORMAL_BALANCES.includes(normalBalance)) {
        throw new Error(`normalBalance tidak valid (pilih: ${NORMAL_BALANCES.join('/')})`);
      }
      return {
        code: str(row, 'code'),
        name: str(row, 'name'),
        type,
        kind,
        normalBalance,
        level: toIntOrThrow(row.level, 'level'),
        isActive: toBool(optStr(row, 'isActive')),
      };
    },
    async insert(prisma, data, actorId) {
      await prisma.erpAccount.create({ data: { ...data, ...audit(actorId) } as never });
    },
  },

  warehouses: {
    label: 'Gudang',
    // locationId is a hard required FK on ErpWarehouse. We accept a location
    // CODE and resolve it to an id; the column is therefore a required header.
    requiredHeaders: ['code', 'name', 'locationCode'],
    optionalHeaders: ['allowNegativeStock', 'isActive'],
    rowToData(row) {
      return {
        code: str(row, 'code'),
        name: str(row, 'name'),
        locationCode: str(row, 'locationCode'),
        allowNegativeStock: toBool(optStr(row, 'allowNegativeStock'), false),
        isActive: toBool(optStr(row, 'isActive')),
      };
    },
    async insert(prisma, data, actorId) {
      const locationCode = data.locationCode as string;
      const location = await prisma.erpLocation.findFirst({
        where: { code: locationCode, deletedAt: null },
        select: { id: true },
      });
      if (!location) {
        throw new Error(`Lokasi dengan kode "${locationCode}" tidak ditemukan`);
      }
      const { locationCode: _omit, ...rest } = data;
      await prisma.erpWarehouse.create({
        data: { ...rest, locationId: location.id, ...audit(actorId) } as never,
      });
    },
  },
};

export function getAdapter(entity: string): EntityAdapter | undefined {
  return IMPORT_ADAPTERS[entity];
}

export function listEntities() {
  return Object.entries(IMPORT_ADAPTERS).map(([value, a]) => ({
    value,
    label: a.label,
    requiredHeaders: a.requiredHeaders,
    optionalHeaders: a.optionalHeaders,
  }));
}
