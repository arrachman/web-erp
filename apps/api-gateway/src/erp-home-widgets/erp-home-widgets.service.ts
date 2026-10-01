import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { isUniqueViolation, throwDuplicate } from '../common/errors/duplicate.util';
import { PrismaService } from '../prisma/prisma.service';
import { toAuditUserId } from '../common/utils/audit-user.util';
import {
  BulkErpHomeWidgetDto,
  BulkStatusErpHomeWidgetDto,
} from './dto/bulk-erp-home-widget.dto';
import { CreateErpHomeWidgetDto } from './dto/create-erp-home-widget.dto';
import { QueryErpHomeWidgetDto } from './dto/query-erp-home-widget.dto';
import { UpdateErpHomeWidgetDto } from './dto/update-erp-home-widget.dto';

@Injectable()
export class ErpHomeWidgetsService {
  constructor(private prisma: PrismaService) {}

  async create(dto: CreateErpHomeWidgetDto, actorId?: string) {
    const existing = await this.prisma.erpHomeWidget.findFirst({
      where: { widgetKey: dto.widgetKey },
      select: { id: true, deletedAt: true },
    });
    if (existing) {
      throwDuplicate({
        fieldLabel: 'Widget key',
        value: dto.widgetKey,
        isSoftDeleted: Boolean(existing.deletedAt),
      });
    }

    let created;
    try {
      created = await this.prisma.erpHomeWidget.create({
        data: {
          widgetKey: dto.widgetKey,
          title: dto.title,
          description: dto.description,
          enabled: dto.enabled ?? true,
          sortOrder: dto.sortOrder ?? 0,
          colSpan: dto.colSpan ?? 1,
          config: dto.config as Prisma.InputJsonValue | undefined,
          createdById: toAuditUserId(actorId),
          updatedById: toAuditUserId(actorId),
        },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['widgetKey'])) {
        throwDuplicate({ fieldLabel: 'Widget key', value: dto.widgetKey });
      }
      throw error;
    }

    return { success: true, data: created };
  }

  async findAll(query: QueryErpHomeWidgetDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;

    const where: Prisma.ErpHomeWidgetWhereInput = { deletedAt: null };
    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { widgetKey: { contains: q, mode: 'insensitive' } },
        { title: { contains: q, mode: 'insensitive' } },
      ];
    }
    if (query.enabled !== undefined) {
      where.enabled = query.enabled;
    }

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpHomeWidget.findMany({
        where,
        orderBy: [{ [query.sortBy ?? 'sortOrder']: query.sortDir ?? 'asc' }],
        skip,
        take: limit,
      }),
      this.prisma.erpHomeWidget.count({ where }),
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

  async findOne(id: bigint) {
    const item = await this.prisma.erpHomeWidget.findFirst({
      where: { id, deletedAt: null },
    });
    if (!item) {
      throw new NotFoundException('Home widget not found');
    }
    return { success: true, data: item };
  }

  async update(id: bigint, dto: UpdateErpHomeWidgetDto, actorId?: string) {
    const existing = await this.prisma.erpHomeWidget.findFirst({
      where: { id, deletedAt: null },
    });
    if (!existing) {
      throw new NotFoundException('Home widget not found');
    }

    if (dto.widgetKey && dto.widgetKey !== existing.widgetKey) {
      const duplicate = await this.prisma.erpHomeWidget.findFirst({
        where: { widgetKey: dto.widgetKey, NOT: { id } },
        select: { id: true, deletedAt: true },
      });
      if (duplicate) {
        throwDuplicate({
          fieldLabel: 'Widget key',
          value: dto.widgetKey,
          isSoftDeleted: Boolean(duplicate.deletedAt),
        });
      }
    }

    let updated;
    try {
      updated = await this.prisma.erpHomeWidget.update({
        where: { id },
        data: {
          widgetKey: dto.widgetKey,
          title: dto.title,
          description: dto.description,
          enabled: dto.enabled,
          sortOrder: dto.sortOrder,
          colSpan: dto.colSpan,
          config: dto.config as Prisma.InputJsonValue | undefined,
          updatedById: toAuditUserId(actorId),
        },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['widgetKey'])) {
        throwDuplicate({ fieldLabel: 'Widget key', value: dto.widgetKey ?? existing.widgetKey });
      }
      throw error;
    }

    return { success: true, data: updated };
  }

  async bulkUpdateStatus(dto: BulkStatusErpHomeWidgetDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const { count } = await this.prisma.erpHomeWidget.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { enabled: dto.enabled, updatedById: toAuditUserId(actorId), updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async bulkDelete(dto: BulkErpHomeWidgetDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const { count } = await this.prisma.erpHomeWidget.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { deletedAt: new Date(), updatedById: toAuditUserId(actorId), updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpHomeWidget.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('Home widget not found');
    }

    await this.prisma.erpHomeWidget.update({
      where: { id },
      data: {
        deletedAt: new Date(),
        updatedById: toAuditUserId(actorId),
      },
    });

    return { success: true, message: 'Home widget deleted' };
  }
}
