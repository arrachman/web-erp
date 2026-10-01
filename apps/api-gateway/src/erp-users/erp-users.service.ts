import { Injectable, NotFoundException } from '@nestjs/common';
import { pbkdf2Sync, randomBytes } from 'crypto';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { CreateErpUserDto } from './dto/create-erp-user.dto';
import { UpdateErpUserDto } from './dto/update-erp-user.dto';
import { QueryErpUserDto } from './dto/query-erp-user.dto';

function hashPassword(password: string): string {
  const salt = randomBytes(16).toString('hex');
  const hash = pbkdf2Sync(password, salt, 310000, 32, 'sha256').toString('hex');
  return `${salt}:${hash}`;
}

@Injectable()
export class ErpUsersService {
  constructor(private readonly prisma: PrismaService) {}

  async create(dto: CreateErpUserDto, actorId?: string) {
    const actorBigInt = actorId ? BigInt(actorId) : null;

    const created = await this.prisma.erpUser.create({
      data: {
        code: dto.username,
        name: dto.fullName,
        email: dto.email ?? null,
        passwordHash: hashPassword(dto.password),
        level: dto.erpLevel,
        language: dto.language ?? 'id',
        isActive: dto.isActive ?? true,
        homeBranchId: dto.branchId ? BigInt(dto.branchId) : null,
        homeWarehouseId: dto.homeWarehouseId ? BigInt(dto.homeWarehouseId) : null,
        expiresAt: dto.expiresAt ? new Date(dto.expiresAt) : null,
        createdById: actorBigInt,
        updatedById: actorBigInt,
      },
    });

    if (dto.roleIds?.length) {
      await this.prisma.erpUserRole.createMany({
        data: dto.roleIds.map((rid) => ({ userId: created.id, roleId: BigInt(rid) })),
        skipDuplicates: true,
      });
    }

    return { success: true, data: created };
  }

  async findAll(query: QueryErpUserDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;

    const where: Prisma.ErpUserWhereInput = { deletedAt: null };

    if (query.erpLevel !== undefined) {
      where.level = query.erpLevel;
    }
    if (query.isActive !== undefined) {
      where.isActive = query.isActive;
    }
    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { code: { equals: q, mode: 'insensitive' } },
        { name: { contains: q, mode: 'insensitive' } },
        { email: { contains: q, mode: 'insensitive' } },
      ];
    }

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpUser.findMany({
        where,
        orderBy: [{ [query.sortBy ?? 'createdAt']: query.sortDir ?? 'desc' }],
        skip,
        take: limit,
        select: {
          id: true,
          code: true,
          name: true,
          email: true,
          level: true,
          language: true,
          isActive: true,
          homeBranchId: true,
          homeWarehouseId: true,
          expiresAt: true,
          createdAt: true,
          updatedAt: true,
          createdById: true,
          updatedById: true,
        },
      }),
      this.prisma.erpUser.count({ where }),
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

  async findOne(id: BigInt) {
    const item = await this.prisma.erpUser.findFirst({
      where: { id: id as bigint, deletedAt: null },
      select: {
        id: true,
        code: true,
        name: true,
        email: true,
        level: true,
        language: true,
        isActive: true,
        homeBranchId: true,
        defaultMenuId: true,
        homeWarehouseId: true,
        expiresAt: true,
        createdAt: true,
        updatedAt: true,
        createdById: true,
        updatedById: true,
        roles: {
          select: {
            role: { select: { id: true, code: true, name: true } },
          },
        },
      },
    });

    if (!item) {
      throw new NotFoundException('ERP User not found');
    }

    return { success: true, data: item };
  }

  async update(id: BigInt, dto: UpdateErpUserDto, actorId?: string) {
    const existing = await this.prisma.erpUser.findFirst({
      where: { id: id as bigint, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('ERP User not found');
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;

    const updateData: Prisma.ErpUserUpdateInput = {
      updatedById: actorBigInt,
    };

    if (dto.username !== undefined) updateData.code = dto.username;
    if (dto.fullName !== undefined) updateData.name = dto.fullName;
    if (dto.email !== undefined) updateData.email = dto.email;
    if (dto.erpLevel !== undefined) updateData.level = dto.erpLevel;
    if (dto.language !== undefined) updateData.language = dto.language;
    if (dto.isActive !== undefined) updateData.isActive = dto.isActive;
    if (dto.branchId !== undefined) updateData.homeBranchId = BigInt(dto.branchId);
    if (dto.homeWarehouseId !== undefined)
      updateData.homeWarehouseId = dto.homeWarehouseId ? BigInt(dto.homeWarehouseId) : null;
    if (dto.expiresAt !== undefined)
      updateData.expiresAt = dto.expiresAt ? new Date(dto.expiresAt) : null;
    if (dto.password !== undefined) updateData.passwordHash = hashPassword(dto.password);

    const updated = await this.prisma.erpUser.update({
      where: { id: id as bigint },
      data: updateData,
      select: {
        id: true,
        code: true,
        name: true,
        email: true,
        level: true,
        language: true,
        isActive: true,
        homeBranchId: true,
        homeWarehouseId: true,
        expiresAt: true,
        updatedAt: true,
      },
    });

    if (dto.roleIds !== undefined) {
      await this.prisma.erpUserRole.deleteMany({ where: { userId: id as bigint } });
      if (dto.roleIds.length) {
        await this.prisma.erpUserRole.createMany({
          data: dto.roleIds.map((rid) => ({ userId: id as bigint, roleId: BigInt(rid) })),
          skipDuplicates: true,
        });
      }
    }

    return { success: true, data: updated };
  }

  async findOnline(windowMinutes = 30) {
    const now = new Date();
    const windowStart = new Date(now.getTime() - windowMinutes * 60 * 1000);

    const sessions = await this.prisma.erpUserSession.findMany({
      where: {
        revokedAt: null,
        expiresAt: { gt: now },
        lastActiveAt: { gte: windowStart },
      },
      include: {
        user: {
          select: { id: true, code: true, name: true, email: true, isActive: true },
        },
      },
      orderBy: { lastActiveAt: 'desc' },
    });

    return {
      success: true,
      data: sessions,
      meta: { count: sessions.length, windowMinutes },
    };
  }

  async remove(id: BigInt, actorId?: string) {
    const existing = await this.prisma.erpUser.findFirst({
      where: { id: id as bigint, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('ERP User not found');
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;

    await this.prisma.erpUser.update({
      where: { id: id as bigint },
      data: {
        deletedAt: new Date(),
        updatedById: actorBigInt,
      },
    });

    return { success: true, message: 'ERP User deleted' };
  }
}
