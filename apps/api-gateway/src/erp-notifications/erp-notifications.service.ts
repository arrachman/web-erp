import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { PrismaService } from '../prisma/prisma.service';
import { CreateErpNotificationDto } from './dto/create-notification.dto';
import { QueryErpNotificationsDto } from './dto/query-notifications.dto';

function serialize(n: any) {
  return {
    id: n.id.toString(),
    recipientId: n.recipientId.toString(),
    type: n.type,
    title: n.title,
    body: n.body,
    referenceEntity: n.referenceEntity,
    referenceId: n.referenceId ? n.referenceId.toString() : null,
    actionUrl: n.actionUrl,
    readAt: n.readAt,
    archivedAt: n.archivedAt,
    expiresAt: n.expiresAt,
    createdAt: n.createdAt,
  };
}

@Injectable()
export class ErpNotificationsService {
  constructor(private prisma: PrismaService) {}

  private toBigInt(value: unknown, label: string): bigint {
    if (value === undefined || value === null || value === '') {
      throw new BadRequestException(`${label} missing`);
    }
    try {
      return BigInt(value as any);
    } catch {
      throw new BadRequestException(`Invalid ${label}`);
    }
  }

  async listForUser(userId: unknown, q: QueryErpNotificationsDto) {
    const uid = this.toBigInt(userId, 'user id');
    const page = q.page ?? 1;
    const limit = q.limit ?? 25;
    const where: any = {
      recipientId: uid,
      archivedAt: null,
    };
    if (q.unreadOnly) where.readAt = null;

    const [rows, total, unreadCount] = await this.prisma.$transaction([
      this.prisma.erpNotification.findMany({
        where,
        orderBy: { createdAt: 'desc' },
        skip: (page - 1) * limit,
        take: limit,
      }),
      this.prisma.erpNotification.count({ where }),
      this.prisma.erpNotification.count({
        where: { recipientId: uid, archivedAt: null, readAt: null },
      }),
    ]);

    return {
      success: true,
      data: rows.map(serialize),
      meta: {
        page,
        limit,
        total,
        totalPages: Math.max(1, Math.ceil(total / limit)),
        unreadCount,
      },
    };
  }

  async unreadCount(userId: unknown) {
    const uid = this.toBigInt(userId, 'user id');
    const count = await this.prisma.erpNotification.count({
      where: { recipientId: uid, archivedAt: null, readAt: null },
    });
    return { success: true, data: { unreadCount: count } };
  }

  async markRead(userId: unknown, id: string) {
    const uid = this.toBigInt(userId, 'user id');
    const nid = this.toBigInt(id, 'notification id');
    const existing = await this.prisma.erpNotification.findUnique({ where: { id: nid } });
    if (!existing || existing.recipientId !== uid) {
      throw new NotFoundException('Notification not found');
    }
    if (existing.readAt) return { success: true, data: serialize(existing) };
    const updated = await this.prisma.erpNotification.update({
      where: { id: nid },
      data: { readAt: new Date() },
    });
    return { success: true, data: serialize(updated) };
  }

  async markAllRead(userId: unknown) {
    const uid = this.toBigInt(userId, 'user id');
    const result = await this.prisma.erpNotification.updateMany({
      where: { recipientId: uid, archivedAt: null, readAt: null },
      data: { readAt: new Date() },
    });
    return { success: true, data: { updated: result.count } };
  }

  async archive(userId: unknown, id: string) {
    const uid = this.toBigInt(userId, 'user id');
    const nid = this.toBigInt(id, 'notification id');
    const existing = await this.prisma.erpNotification.findUnique({ where: { id: nid } });
    if (!existing || existing.recipientId !== uid) {
      throw new NotFoundException('Notification not found');
    }
    await this.prisma.erpNotification.update({
      where: { id: nid },
      data: { archivedAt: new Date() },
    });
    return { success: true };
  }

  async create(dto: CreateErpNotificationDto) {
    const created = await this.prisma.erpNotification.create({
      data: {
        recipientId: this.toBigInt(dto.recipientId, 'recipientId'),
        type: dto.type,
        title: dto.title,
        body: dto.body,
        referenceEntity: dto.referenceEntity,
        referenceId: dto.referenceId ? this.toBigInt(dto.referenceId, 'referenceId') : undefined,
        actionUrl: dto.actionUrl,
        expiresAt: dto.expiresAt ? new Date(dto.expiresAt) : undefined,
      },
    });
    return { success: true, data: serialize(created) };
  }
}
