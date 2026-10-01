import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { CreateErpRoleDto } from './dto/create-erp-role.dto';
import { UpdateErpRoleDto } from './dto/update-erp-role.dto';
import { QueryErpRoleDto } from './dto/query-erp-role.dto';
import { AssignPermissionsDto } from './dto/assign-permissions.dto';
import { AssignMenusDto } from './dto/assign-menus.dto';

@Injectable()
export class ErpRolesService {
  constructor(private readonly prisma: PrismaService) {}

  async create(dto: CreateErpRoleDto, actorId?: string) {
    const actorBigInt = actorId ? BigInt(actorId) : null;

    const created = await this.prisma.erpRole.create({
      data: {
        code: dto.code,
        name: dto.name,
        description: dto.description ?? null,
        createdById: actorBigInt,
        updatedById: actorBigInt,
      },
    });

    return { success: true, data: created };
  }

  async findAll(query: QueryErpRoleDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;

    const where: Prisma.ErpRoleWhereInput = { deletedAt: null };

    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { code: { equals: q, mode: 'insensitive' } },
        { name: { contains: q, mode: 'insensitive' } },
        { description: { contains: q, mode: 'insensitive' } },
      ];
    }

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpRole.findMany({
        where,
        orderBy: [{ [query.sortBy ?? 'createdAt']: query.sortDir ?? 'desc' }],
        skip,
        take: limit,
      }),
      this.prisma.erpRole.count({ where }),
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
    const item = await this.prisma.erpRole.findFirst({
      where: { id: id as bigint, deletedAt: null },
    });
    if (!item) {
      throw new NotFoundException('ERP Role not found');
    }
    return { success: true, data: item };
  }

  async update(id: BigInt, dto: UpdateErpRoleDto, actorId?: string) {
    const existing = await this.prisma.erpRole.findFirst({
      where: { id: id as bigint, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('ERP Role not found');
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;

    const updateData: Prisma.ErpRoleUpdateInput = { updatedById: actorBigInt };
    if (dto.code !== undefined) updateData.code = dto.code;
    if (dto.name !== undefined) updateData.name = dto.name;
    if (dto.description !== undefined) updateData.description = dto.description;

    const updated = await this.prisma.erpRole.update({
      where: { id: id as bigint },
      data: updateData,
    });

    return { success: true, data: updated };
  }

  async remove(id: BigInt, actorId?: string) {
    const existing = await this.prisma.erpRole.findFirst({
      where: { id: id as bigint, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('ERP Role not found');
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;

    await this.prisma.erpRole.update({
      where: { id: id as bigint },
      data: {
        deletedAt: new Date(),
        updatedById: actorBigInt,
      },
    });

    return { success: true, message: 'ERP Role deleted' };
  }

  async assignPermissions(id: BigInt, dto: AssignPermissionsDto, _actorId?: string) {
    const role = await this.prisma.erpRole.findFirst({
      where: { id: id as bigint, deletedAt: null },
      select: { id: true },
    });
    if (!role) {
      throw new NotFoundException('ERP Role not found');
    }

    const permissionBigIntIds = dto.permissionIds.map((p) => BigInt(p));

    await this.prisma.$transaction([
      this.prisma.erpRolePermission.deleteMany({
        where: { roleId: id as bigint },
      }),
      ...permissionBigIntIds.map((permissionId) =>
        this.prisma.erpRolePermission.create({
          data: { roleId: id as bigint, permissionId },
        }),
      ),
    ]);

    return { success: true, message: 'Permissions assigned to role' };
  }

  async getPermissions(id: BigInt) {
    const role = await this.prisma.erpRole.findFirst({
      where: { id: id as bigint, deletedAt: null },
      select: { id: true },
    });
    if (!role) {
      throw new NotFoundException('ERP Role not found');
    }

    const rows = await this.prisma.erpRolePermission.findMany({
      where: { roleId: id as bigint },
      include: {
        permission: {
          select: { id: true, code: true, name: true, group: true, description: true },
        },
      },
    });

    return { success: true, data: rows.map((r) => r.permission) };
  }

  async assignMenus(id: BigInt, dto: AssignMenusDto, _actorId?: string) {
    const role = await this.prisma.erpRole.findFirst({
      where: { id: id as bigint, deletedAt: null },
      select: { id: true },
    });
    if (!role) {
      throw new NotFoundException('ERP Role not found');
    }

    const menuBigIntIds = dto.menuIds.map((m) => BigInt(m));

    await this.prisma.$transaction([
      this.prisma.erpRoleMenu.deleteMany({
        where: { roleId: id as bigint },
      }),
      ...menuBigIntIds.map((menuId) =>
        this.prisma.erpRoleMenu.create({
          data: { roleId: id as bigint, menuId },
        }),
      ),
    ]);

    return { success: true, message: 'Menus assigned to role' };
  }

  async getMenus(id: BigInt) {
    const role = await this.prisma.erpRole.findFirst({
      where: { id: id as bigint, deletedAt: null },
      select: { id: true },
    });
    if (!role) {
      throw new NotFoundException('ERP Role not found');
    }

    const rows = await this.prisma.erpRoleMenu.findMany({
      where: { roleId: id as bigint },
      include: {
        menu: {
          select: {
            id: true,
            code: true,
            title: true,
            path: true,
            icon: true,
            type: true,
            sortOrder: true,
          },
        },
      },
    });

    return {
      success: true,
      data: rows.map((r) => ({
        ...r.menu,
        canView: r.canView,
        canCreate: r.canCreate,
        canEdit: r.canEdit,
        canDelete: r.canDelete,
        canApprove: r.canApprove,
        canPrint: r.canPrint,
        canExport: r.canExport,
        canImport: r.canImport,
        isFavorite: r.isFavorite,
      })),
    };
  }
}
