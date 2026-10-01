import { Injectable, NotFoundException } from '@nestjs/common';
import { PrismaService } from '../prisma/prisma.service';
import { toAuditUserId } from '../common/utils/audit-user.util';
import { QueryErpSettingDto } from './dto/query-erp-setting.dto';
import { UpdateErpSettingDto } from './dto/update-erp-setting.dto';
import {
  buildNumberFormat,
  NumberFormat,
  parseDecimals,
  parseDecimalSep,
  parseThousandsSep,
} from './number-format';
import { buildDateFormat, DateFormat, parseDateFormatToken } from './date-format';

const NUMBER_FORMAT_GROUP = 'number-format';
const KEY_THOUSANDS = 'number_thousands_sep';
const KEY_DECIMAL = 'number_decimal_sep';
const KEY_DECIMALS = 'number_decimals';

const FORMAT_GROUP = 'format';
const KEY_DATE_FORMAT = 'date_format';

@Injectable()
export class ErpSettingsService {
  constructor(private prisma: PrismaService) {}

  async getNumberFormat(): Promise<NumberFormat> {
    const rows = await this.prisma.erpSetting.findMany({
      where: {
        group: NUMBER_FORMAT_GROUP,
        key: { in: [KEY_THOUSANDS, KEY_DECIMAL, KEY_DECIMALS] },
        deletedAt: null,
      },
    });
    const map = new Map(rows.map((r) => [r.key, r.value]));
    return buildNumberFormat(
      parseThousandsSep(map.get(KEY_THOUSANDS)),
      parseDecimalSep(map.get(KEY_DECIMAL)),
      parseDecimals(map.get(KEY_DECIMALS)),
    );
  }

  async updateNumberFormat(
    thousandsSep: string,
    decimalSep: string,
    decimals: number,
    actorId?: string,
  ): Promise<NumberFormat> {
    const format = buildNumberFormat(
      parseThousandsSep(thousandsSep),
      parseDecimalSep(decimalSep),
      parseDecimals(String(decimals)),
    );
    const updatedById = toAuditUserId(actorId);
    const writes: Array<{ key: string; value: string; name: string; dataType: string }> = [
      { key: KEY_THOUSANDS, value: format.thousandsSep, name: 'Pemisah Ribuan', dataType: 'string' },
      { key: KEY_DECIMAL, value: format.decimalSep, name: 'Pemisah Desimal', dataType: 'string' },
      { key: KEY_DECIMALS, value: String(format.decimals), name: 'Jumlah Desimal', dataType: 'integer' },
    ];
    for (const w of writes) {
      await this.prisma.erpSetting.upsert({
        where: {
          module_group_key: { module: 'system', group: NUMBER_FORMAT_GROUP, key: w.key },
        },
        create: {
          module: 'system',
          group: NUMBER_FORMAT_GROUP,
          key: w.key,
          name: w.name,
          value: w.value,
          dataType: w.dataType,
        },
        update: { value: w.value, updatedById },
      });
    }
    return format;
  }

  async getDateFormat(): Promise<DateFormat> {
    const row = await this.prisma.erpSetting.findFirst({
      where: { group: FORMAT_GROUP, key: KEY_DATE_FORMAT, deletedAt: null },
    });
    return buildDateFormat(row?.value);
  }

  async updateDateFormat(format: string, actorId?: string): Promise<DateFormat> {
    const token = parseDateFormatToken(format);
    const updatedById = toAuditUserId(actorId);
    await this.prisma.erpSetting.upsert({
      where: {
        module_group_key: { module: 'system', group: FORMAT_GROUP, key: KEY_DATE_FORMAT },
      },
      create: {
        module: 'system',
        group: FORMAT_GROUP,
        key: KEY_DATE_FORMAT,
        name: 'Format Tanggal',
        value: token,
        dataType: 'string',
      },
      update: { value: token, updatedById },
    });
    return buildDateFormat(token);
  }

  async findAll(query: QueryErpSettingDto) {
    const where: {
      deletedAt: null;
      group?: string;
      key?: string;
    } = { deletedAt: null };

    if (query.group?.trim()) {
      where.group = query.group.trim();
    }
    if (query.key?.trim()) {
      where.key = query.key.trim();
    }

    const items = await this.prisma.erpSetting.findMany({
      where,
      orderBy: [{ group: 'asc' }, { sortOrder: 'asc' }, { key: 'asc' }],
    });

    return { success: true, data: items };
  }

  async findOne(key: string) {
    const item = await this.prisma.erpSetting.findFirst({
      where: { key, deletedAt: null },
    });
    if (!item) {
      throw new NotFoundException(`ERP setting with key "${key}" not found`);
    }
    return { success: true, data: item };
  }

  async upsert(key: string, dto: UpdateErpSettingDto, actorId?: string) {
    const existing = await this.prisma.erpSetting.findFirst({
      where: { key, deletedAt: null },
    });

    if (!existing) {
      throw new NotFoundException(`ERP setting with key "${key}" not found`);
    }

    const updated = await this.prisma.erpSetting.update({
      where: { id: existing.id },
      data: {
        value: dto.value ?? existing.value,
        updatedById: toAuditUserId(actorId)
          ? BigInt(toAuditUserId(actorId) as number)
          : undefined,
      },
    });

    return { success: true, data: updated };
  }
}
