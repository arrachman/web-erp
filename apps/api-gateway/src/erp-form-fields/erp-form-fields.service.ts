import { Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { SaveFormFieldsDto } from './dto/save-form-fields.dto';
import { withDefaultValueLabels } from './lookup-label-resolver';
import { INV_DEFAULTS_BY_CODE } from './inv-form-field-defaults';

// Default structural field configs per transaction-type code.
// Only keys that differ per type need separate entries — most fields are shared.
interface FormFieldDefault {
  fieldKey: string;
  kind: 'STRUCTURAL' | 'CUSTOM';
  label: string;
  fieldType: 'TEXT' | 'NUMBER' | 'DATE' | 'PARTNER' | 'ACCOUNT' | 'BRANCH' | 'LOCATION' | 'CURRENCY' | 'LOOKUP';
  isRequired: boolean;
  isVisible: boolean;
  sortOrder: number;
  columnSlot: 'LEFT' | 'CENTER' | 'RIGHT';
  lookupDefaultFilter?: Prisma.InputJsonValue;
}

// Filter for cash/bank account pickers: postable ASSET accounts with a debit normal balance.
const CASH_ACCOUNT_FILTER = { accountType: 'ASSET', accountKind: 'POSTABLE', normalBalance: 'DEBIT', isActive: true };

const CR_DEFAULTS: FormFieldDefault[] = [
  { fieldKey: 'partnerId',     kind: 'STRUCTURAL', label: 'Terima Dari',  fieldType: 'PARTNER',   isRequired: true,  isVisible: true,  sortOrder: 0, columnSlot: 'LEFT'   },
  { fieldKey: 'bankAccountId', kind: 'STRUCTURAL', label: 'Akun Kas [D]', fieldType: 'ACCOUNT',   isRequired: true,  isVisible: true,  sortOrder: 1, columnSlot: 'LEFT',   lookupDefaultFilter: { accountType: 'ASSET', accountKind: 'POSTABLE', normalBalance: 'DEBIT', isActive: true } },
  { fieldKey: 'description',   kind: 'STRUCTURAL', label: 'Uraian',       fieldType: 'TEXT',      isRequired: true,  isVisible: true,  sortOrder: 2, columnSlot: 'LEFT'   },
  { fieldKey: 'branchId',      kind: 'STRUCTURAL', label: 'Cabang',       fieldType: 'BRANCH',    isRequired: true,  isVisible: true,  sortOrder: 3, columnSlot: 'CENTER' },
  { fieldKey: 'locationId',    kind: 'STRUCTURAL', label: 'Lokasi',       fieldType: 'LOCATION',  isRequired: false, isVisible: true,  sortOrder: 4, columnSlot: 'CENTER' },
  { fieldKey: 'transactionDate', kind: 'STRUCTURAL', label: 'Tanggal',     fieldType: 'DATE',      isRequired: true,  isVisible: true,  sortOrder: 5, columnSlot: 'RIGHT'  },
  { fieldKey: 'docNumber',     kind: 'STRUCTURAL', label: 'No Transaksi', fieldType: 'TEXT',      isRequired: false, isVisible: true,  sortOrder: 6, columnSlot: 'RIGHT'  },
  { fieldKey: 'currencyId',    kind: 'STRUCTURAL', label: 'Uang',         fieldType: 'CURRENCY',  isRequired: true,  isVisible: true,  sortOrder: 7, columnSlot: 'RIGHT'  },
];

const CD_DEFAULTS = CR_DEFAULTS.map((f) => {
  if (f.fieldKey === 'partnerId')     return { ...f, label: 'Bayar Ke' };
  if (f.fieldKey === 'bankAccountId') return { ...f, label: 'Akun Kas [K]' };
  return f;
});

const BD_DEFAULTS = CR_DEFAULTS.map((f) => {
  if (f.fieldKey === 'partnerId')     return { ...f, label: 'Bayar Ke' };
  if (f.fieldKey === 'bankAccountId') return { ...f, label: 'Akun Bank [K]' };
  return f;
});

const RM_DEFAULTS = CR_DEFAULTS.map((f) => {
  if (f.fieldKey === 'bankAccountId') return { ...f, label: 'Akun Bank [D]' };
  return f;
});

// ── Journal family (GJ/AJ/JM/RV/BB) → fin_journal_lines (Akun · Debit · Kredit) ──
// Header has no single cash account; identity = Uraian + dimensions; entryDate (not
// transactionDate) is the journal-native date key bound by a future config-driven form.
const JOURNAL_DEFAULTS: FormFieldDefault[] = [
  { fieldKey: 'description', kind: 'STRUCTURAL', label: 'Uraian',       fieldType: 'TEXT',     isRequired: true,  isVisible: true, sortOrder: 0, columnSlot: 'LEFT'   },
  { fieldKey: 'notes',       kind: 'STRUCTURAL', label: 'Catatan',      fieldType: 'TEXT',     isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'LEFT'   },
  { fieldKey: 'branchId',    kind: 'STRUCTURAL', label: 'Cabang',       fieldType: 'BRANCH',   isRequired: true,  isVisible: true, sortOrder: 0, columnSlot: 'CENTER' },
  { fieldKey: 'entryDate',   kind: 'STRUCTURAL', label: 'Tanggal',      fieldType: 'DATE',     isRequired: true,  isVisible: true, sortOrder: 0, columnSlot: 'RIGHT'  },
  { fieldKey: 'docNumber',   kind: 'STRUCTURAL', label: 'No Transaksi', fieldType: 'TEXT',     isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'RIGHT'  },
  { fieldKey: 'currencyId',  kind: 'STRUCTURAL', label: 'Uang',         fieldType: 'CURRENCY', isRequired: true,  isVisible: true, sortOrder: 2, columnSlot: 'RIGHT'  },
];

// Adjustment Journal adds an optional partner (for partner-scoped adjustments).
const AJ_DEFAULTS: FormFieldDefault[] = [
  { fieldKey: 'partnerId', kind: 'STRUCTURAL', label: 'Partner', fieldType: 'PARTNER', isRequired: false, isVisible: true, sortOrder: 0, columnSlot: 'LEFT' },
  ...JOURNAL_DEFAULTS.map((f) => (f.columnSlot === 'LEFT' ? { ...f, sortOrder: f.sortOrder + 1 } : f)),
];

// Opening Balance (CoA) — same journal header, date labelled per its meaning.
const BB_DEFAULTS: FormFieldDefault[] = JOURNAL_DEFAULTS.map((f) =>
  (f.fieldKey === 'entryDate' ? { ...f, label: 'Tanggal Saldo Awal' } : f));

// ── Giro family (RG/SG) → fin_giros (instruments captured in the detail grid) ──
// Instrument fields (No Giro/Bank/Jatuh Tempo/Nominal) live in the grid, not the header.
const RG_DEFAULTS: FormFieldDefault[] = [
  { fieldKey: 'partnerId',     kind: 'STRUCTURAL', label: 'Terima Dari',   fieldType: 'PARTNER',  isRequired: true,  isVisible: true, sortOrder: 0, columnSlot: 'LEFT'   },
  { fieldKey: 'description',   kind: 'STRUCTURAL', label: 'Uraian',        fieldType: 'TEXT',     isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'LEFT'   },
  { fieldKey: 'giroAccountId', kind: 'STRUCTURAL', label: 'Akun Giro',     fieldType: 'ACCOUNT',  isRequired: false, isVisible: true, sortOrder: 2, columnSlot: 'LEFT', lookupDefaultFilter: CASH_ACCOUNT_FILTER },
  { fieldKey: 'branchId',      kind: 'STRUCTURAL', label: 'Cabang',        fieldType: 'BRANCH',   isRequired: true,  isVisible: true, sortOrder: 0, columnSlot: 'CENTER' },
  { fieldKey: 'entryDate',   kind: 'STRUCTURAL', label: 'Tanggal',      fieldType: 'DATE',     isRequired: true,  isVisible: true, sortOrder: 0, columnSlot: 'RIGHT'  },
  { fieldKey: 'docNumber',   kind: 'STRUCTURAL', label: 'No Transaksi', fieldType: 'TEXT',     isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'RIGHT'  },
  { fieldKey: 'currencyId',  kind: 'STRUCTURAL', label: 'Uang',         fieldType: 'CURRENCY', isRequired: true,  isVisible: true, sortOrder: 2, columnSlot: 'RIGHT'  },
];

const SG_DEFAULTS: FormFieldDefault[] = RG_DEFAULTS.map((f) =>
  (f.fieldKey === 'partnerId' ? { ...f, label: 'Bayar Ke' } : f));

// ── Giro clearing (RGC/SGC) → fin_giros (clear outstanding giros into a bank account) ──
const RGC_DEFAULTS: FormFieldDefault[] = [
  { fieldKey: 'bankAccountId', kind: 'STRUCTURAL', label: 'Akun Bank',     fieldType: 'ACCOUNT',  isRequired: true,  isVisible: true, sortOrder: 0, columnSlot: 'LEFT',   lookupDefaultFilter: CASH_ACCOUNT_FILTER },
  { fieldKey: 'description',   kind: 'STRUCTURAL', label: 'Uraian',        fieldType: 'TEXT',     isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'LEFT'   },
  { fieldKey: 'branchId',      kind: 'STRUCTURAL', label: 'Cabang',        fieldType: 'BRANCH',   isRequired: true,  isVisible: true, sortOrder: 0, columnSlot: 'CENTER' },
  { fieldKey: 'entryDate',     kind: 'STRUCTURAL', label: 'Tanggal Cair',  fieldType: 'DATE',     isRequired: true,  isVisible: true, sortOrder: 0, columnSlot: 'RIGHT'  },
  { fieldKey: 'docNumber',     kind: 'STRUCTURAL', label: 'No Transaksi',  fieldType: 'TEXT',     isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'RIGHT'  },
  { fieldKey: 'currencyId',    kind: 'STRUCTURAL', label: 'Uang',          fieldType: 'CURRENCY', isRequired: true,  isVisible: true, sortOrder: 2, columnSlot: 'RIGHT'  },
];

const DEFAULTS_BY_CODE: Record<string, FormFieldDefault[]> = {
  'FIN.CR': CR_DEFAULTS,
  'FIN.CD': CD_DEFAULTS,
  'FIN.BD': BD_DEFAULTS,
  'FIN.RM': RM_DEFAULTS,
  'FIN.GJ': JOURNAL_DEFAULTS,
  'FIN.AJ': AJ_DEFAULTS,
  'FIN.JM': JOURNAL_DEFAULTS,
  'FIN.RV': JOURNAL_DEFAULTS,
  'FIN.BB': BB_DEFAULTS,
  'FIN.RG': RG_DEFAULTS,
  'FIN.SG': SG_DEFAULTS,
  'FIN.RGC': RGC_DEFAULTS,
  'FIN.SGC': RGC_DEFAULTS,
  // M3 Warehouse & Inventory header layouts (see inv-form-field-defaults.ts).
  ...INV_DEFAULTS_BY_CODE,
};

@Injectable()
export class ErpFormFieldsService {
  constructor(private readonly prisma: PrismaService) {}

  /** Get form fields for a transaction type. Lazily seeds structural defaults if none exist. */
  async getFields(code: string) {
    let fields = await this.prisma.erpFormField.findMany({
      where: { transactionTypeCode: code, deletedAt: null },
      orderBy: { sortOrder: 'asc' },
    });

    if (fields.length === 0) {
      const defaults = DEFAULTS_BY_CODE[code] ?? CR_DEFAULTS;
      await this.prisma.erpFormField.createMany({
        data: defaults.map((d) => ({ ...d, transactionTypeCode: code })),
        skipDuplicates: true,
      });
      fields = await this.prisma.erpFormField.findMany({
        where: { transactionTypeCode: code, deletedAt: null },
        orderBy: { sortOrder: 'asc' },
      });
    } else {
      // Backfill lookupDefaultFilter for existing records that were seeded before
      // filter support was added (null = never set by admin; {} = admin explicitly cleared).
      const defaults = DEFAULTS_BY_CODE[code];
      if (defaults) {
        const toBackfill = fields.filter((f) => {
          if (f.lookupDefaultFilter !== null) return false;
          const def = defaults.find((d) => d.fieldKey === f.fieldKey);
          return def && 'lookupDefaultFilter' in def && def.lookupDefaultFilter != null;
        });
        if (toBackfill.length > 0) {
          await Promise.all(
            toBackfill.map((f) => {
              const def = defaults.find((d) => d.fieldKey === f.fieldKey)!;
              return this.prisma.erpFormField.update({
                where: { id: f.id },
                data: { lookupDefaultFilter: def.lookupDefaultFilter as Prisma.InputJsonValue },
              });
            }),
          );
          fields = await this.prisma.erpFormField.findMany({
            where: { transactionTypeCode: code, deletedAt: null },
            orderBy: { sortOrder: 'asc' },
          });
        }
      }
    }

    return { code, fields: await withDefaultValueLabels(this.prisma, fields) };
  }

  /** Replace all fields for a transaction type (bulk save). */
  async saveFields(code: string, dto: SaveFormFieldsDto, actorId?: string) {
    const updatedById = actorId ? BigInt(actorId) : null;
    const incomingKeys = new Set(dto.fields.map((f) => f.fieldKey));

    await this.prisma.$transaction(async (tx) => {
      // Soft-delete active fields that are no longer in the incoming list.
      await tx.erpFormField.updateMany({
        where: {
          transactionTypeCode: code,
          deletedAt: null,
          NOT: { fieldKey: { in: [...incomingKeys] } },
        },
        data: { deletedAt: new Date() },
      });

      // Upsert each incoming field — updates existing (restoring if soft-deleted), creates if new.
      for (const f of dto.fields) {
        const fieldData = {
          kind: f.kind,
          label: f.label,
          fieldType: f.fieldType,
          lookupSource: f.lookupSource ?? null,
          lookupDefaultFilter: (f.lookupDefaultFilter as Prisma.InputJsonValue) ?? Prisma.DbNull,
          lookupDefaultSort: f.lookupDefaultSort ?? null,
          placeholder: f.placeholder ?? null,
          defaultValue: f.defaultValue ?? null,
          isReadonly: f.isReadonly ?? false,
          isRequired: f.isRequired,
          isVisible: f.isVisible,
          sortOrder: f.sortOrder,
          columnSlot: f.columnSlot,
          updatedById,
          deletedAt: null,
        };
        await tx.erpFormField.upsert({
          where: { transactionTypeCode_fieldKey: { transactionTypeCode: code, fieldKey: f.fieldKey } },
          create: { transactionTypeCode: code, fieldKey: f.fieldKey, createdById: updatedById, ...fieldData },
          update: fieldData,
        });
      }
    });

    return this.getFields(code);
  }
}
