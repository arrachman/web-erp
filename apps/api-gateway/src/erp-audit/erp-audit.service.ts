import { Injectable, Logger } from '@nestjs/common';
import { ErpAuditAction, Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { QueryErpAuditLogDto } from './dto/query-erp-audit-log.dto';

export interface AuditLogParams {
  action: ErpAuditAction;
  entityName: string;
  entityId?: bigint;
  changes?: Record<string, { from: unknown; to: unknown }>;
  summary?: string;
  actorId?: bigint;
  actorIp?: string;
}

@Injectable()
export class ErpAuditService {
  private readonly logger = new Logger(ErpAuditService.name);

  constructor(private readonly prisma: PrismaService) {}

  /** Fire-and-forget audit write — never throws, never blocks the caller. */
  log(params: AuditLogParams): void {
    this.prisma.erpAuditLog
      .create({
        data: {
          action: params.action,
          entityName: params.entityName,
          entityId: params.entityId ?? null,
          changes: params.changes as Prisma.InputJsonValue ?? Prisma.JsonNull,
          summary: params.summary ?? null,
          actorId: params.actorId ?? null,
          actorIp: params.actorIp ?? null,
          occurredAt: new Date(),
        },
      })
      .catch((err: unknown) => {
        this.logger.warn(`audit log write failed: ${String(err)}`);
      });
  }

  async findAll(query: QueryErpAuditLogDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 20;
    const skip = (page - 1) * limit;

    const where: Prisma.ErpAuditLogWhereInput = {};

    if (query.entityName) where.entityName = query.entityName;
    if (query.entityId) where.entityId = BigInt(query.entityId);
    if (query.action) where.action = query.action;
    if (query.actorId) where.actorId = BigInt(query.actorId);
    if (query.from || query.to) {
      where.occurredAt = {
        ...(query.from && { gte: new Date(query.from) }),
        ...(query.to && { lte: new Date(query.to) }),
      };
    }

    const [logs, total] = await this.prisma.$transaction([
      this.prisma.erpAuditLog.findMany({
        where,
        include: {
          actor: { select: { id: true, code: true, name: true } },
        },
        orderBy: { occurredAt: 'desc' },
        skip,
        take: limit,
      }),
      this.prisma.erpAuditLog.count({ where }),
    ]);

    return {
      success: true,
      data: logs,
      meta: {
        page,
        limit,
        total,
        totalPages: Math.ceil(total / limit) || 1,
      },
    };
  }
}
