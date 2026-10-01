import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { isUniqueViolation, throwDuplicate } from '../common/errors/duplicate.util';
import { RefCacheService } from '../common/cache/ref-cache.service';
import { PrismaService } from '../prisma/prisma.service';
import { toAuditUserId } from '../common/utils/audit-user.util';
import { BulkErpCurrencyDto, BulkStatusErpCurrencyDto } from './dto/bulk-erp-currency.dto';
import { CreateErpCurrencyDto } from './dto/create-erp-currency.dto';
import { QueryErpCurrencyDto } from './dto/query-erp-currency.dto';
import { UpdateErpCurrencyDto } from './dto/update-erp-currency.dto';
import { CreateErpCurrencyRateDto } from './dto/create-erp-currency-rate.dto';

@Injectable()
export class ErpCurrenciesService {
  constructor(
    private prisma: PrismaService,
    private readonly refCache: RefCacheService,
  ) {}

  async create(dto: CreateErpCurrencyDto, actorId?: string) {
    const existing = await this.prisma.erpCurrency.findFirst({
      where: { code: dto.code },
      select: { id: true, deletedAt: true },
    });
    if (existing) {
      throwDuplicate({
        fieldLabel: 'Currency code',
        value: dto.code,
        isSoftDeleted: Boolean(existing.deletedAt),
      });
    }

    const isBase = dto.isBase ?? false;
    const decimalPlaces = dto.decimalPlaces ?? 2;

    let created;
    try {
      created = await this.prisma.$transaction(async (tx) => {
        if (isBase) {
          await tx.erpCurrency.updateMany({
            where: { isBase: true, deletedAt: null },
            data: { isBase: false, updatedById: toAuditUserId(actorId) },
          });
        }
        return tx.erpCurrency.create({
          data: {
            code: dto.code,
            name: dto.name,
            symbol: dto.symbol,
            decimalPlaces,
            isBase,
            isActive: dto.isActive ?? true,
            createdById: toAuditUserId(actorId),
            updatedById: toAuditUserId(actorId),
          },
        });
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code'])) {
        throwDuplicate({ fieldLabel: 'Currency code', value: dto.code });
      }
      throw error;
    }

    await this.refCache.invalidateNs('currencies');
    return { success: true, data: created };
  }

  async findAll(query: QueryErpCurrencyDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;
    const sortBy = query.sortBy ?? 'createdAt';
    const sortDir = query.sortDir ?? 'desc';

    const where: Prisma.ErpCurrencyWhereInput = { deletedAt: null };
    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { code: { equals: q, mode: 'insensitive' } },
        { name: { contains: q, mode: 'insensitive' } },
      ];
    }
    if (query.isActive !== undefined) {
      where.isActive = query.isActive;
    }

    const cacheable =
      !query.search?.trim() &&
      page === 1 &&
      limit <= 200 &&
      sortBy === 'createdAt' &&
      sortDir === 'desc';
    const cachePart = `p1:l${limit}:a${query.isActive === undefined ? 'any' : query.isActive}`;

    const load = async () => {
      const [items, total] = await this.prisma.$transaction([
        this.prisma.erpCurrency.findMany({
          where,
          orderBy: [{ [sortBy]: sortDir }],
          skip,
          take: limit,
        }),
        this.prisma.erpCurrency.count({ where }),
      ]);
      return {
        success: true as const,
        data: items,
        meta: {
          page,
          limit,
          total,
          totalPages: Math.ceil(total / limit) || 1,
        },
      };
    };

    if (cacheable) {
      return this.refCache.getOrLoad('currencies', load, 300, cachePart);
    }
    return load();
  }

  async findOne(id: bigint) {
    const item = await this.prisma.erpCurrency.findFirst({
      where: { id, deletedAt: null },
      include: { rates: { orderBy: { rateDate: 'desc' }, take: 5 } },
    });
    if (!item) {
      throw new NotFoundException('Currency not found');
    }
    return { success: true, data: item };
  }

  async update(id: bigint, dto: UpdateErpCurrencyDto, actorId?: string) {
    const existing = await this.prisma.erpCurrency.findFirst({
      where: { id, deletedAt: null },
    });
    if (!existing) {
      throw new NotFoundException('Currency not found');
    }

    if (dto.code && dto.code !== existing.code) {
      const duplicate = await this.prisma.erpCurrency.findFirst({
        where: { code: dto.code, NOT: { id } },
        select: { id: true, deletedAt: true },
      });
      if (duplicate) {
        throwDuplicate({
          fieldLabel: 'Currency code',
          value: dto.code,
          isSoftDeleted: Boolean(duplicate.deletedAt),
        });
      }
    }

    let updated;
    try {
      updated = await this.prisma.$transaction(async (tx) => {
        if (dto.isBase === true) {
          await tx.erpCurrency.updateMany({
            where: { isBase: true, deletedAt: null, NOT: { id } },
            data: { isBase: false, updatedById: toAuditUserId(actorId) },
          });
        }
        return tx.erpCurrency.update({
          where: { id },
          data: {
            code: dto.code,
            name: dto.name,
            symbol: dto.symbol,
            decimalPlaces: dto.decimalPlaces,
            isBase: dto.isBase,
            isActive: dto.isActive,
            updatedById: toAuditUserId(actorId),
          },
        });
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code'])) {
        throwDuplicate({ fieldLabel: 'Currency code', value: dto.code ?? existing.code });
      }
      throw error;
    }

    await this.refCache.invalidateNs('currencies');
    return { success: true, data: updated };
  }

  async bulkUpdateStatus(dto: BulkStatusErpCurrencyDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const { count } = await this.prisma.erpCurrency.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { isActive: dto.isActive, updatedById: toAuditUserId(actorId), updatedAt: new Date() },
    });
    await this.refCache.invalidateNs('currencies');
    return { success: true, affected: count };
  }

  async bulkDelete(dto: BulkErpCurrencyDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const { count } = await this.prisma.erpCurrency.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { deletedAt: new Date(), updatedById: toAuditUserId(actorId), updatedAt: new Date() },
    });
    await this.refCache.invalidateNs('currencies');
    return { success: true, affected: count };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpCurrency.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('Currency not found');
    }

    await this.prisma.erpCurrency.update({
      where: { id },
      data: {
        deletedAt: new Date(),
        updatedById: toAuditUserId(actorId),
      },
    });

    await this.refCache.invalidateNs('currencies');
    return { success: true, message: 'Currency deleted' };
  }

  async addRate(currencyId: bigint, dto: CreateErpCurrencyRateDto, actorId?: string) {
    const currency = await this.prisma.erpCurrency.findFirst({
      where: { id: currencyId, deletedAt: null },
      select: { id: true },
    });
    if (!currency) {
      throw new NotFoundException('Currency not found');
    }

    const rateDate = new Date(dto.rateDate);
    let rate;
    try {
      rate = await this.prisma.erpCurrencyRate.upsert({
        where: { currencyId_rateDate: { currencyId, rateDate } },
        create: {
          currencyId,
          rateDate,
          rate: new Prisma.Decimal(dto.rate),
          createdById: toAuditUserId(actorId),
          updatedById: toAuditUserId(actorId),
        },
        update: {
          rate: new Prisma.Decimal(dto.rate),
          updatedById: toAuditUserId(actorId),
        },
      });
    } catch (error) {
      throw error;
    }

    return { success: true, data: rate };
  }

  async getRates(currencyId: bigint, query: { page?: number; limit?: number }) {
    const currency = await this.prisma.erpCurrency.findFirst({
      where: { id: currencyId, deletedAt: null },
      select: { id: true },
    });
    if (!currency) {
      throw new NotFoundException('Currency not found');
    }

    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;

    const where: Prisma.ErpCurrencyRateWhereInput = { currencyId };
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpCurrencyRate.findMany({
        where,
        orderBy: [{ rateDate: 'desc' }],
        skip,
        take: limit,
      }),
      this.prisma.erpCurrencyRate.count({ where }),
    ]);

    return {
      success: true,
      data: items,
      meta: {
        page,
        limit,
        total,
        totalPages: Math.ceil(total / limit) || 1,
      },
    };
  }
}
