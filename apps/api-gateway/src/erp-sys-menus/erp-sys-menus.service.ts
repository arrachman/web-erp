import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { ErpMenu } from '@prisma/client';
import { toAuditUserId } from '../common/utils/audit-user.util';
import { PrismaService } from '../prisma/prisma.service';
import { BulkErpSysMenuDto, BulkStatusErpSysMenuDto } from './dto/bulk-erp-sys-menu.dto';
import { CreateErpSysMenuDto } from './dto/create-erp-sys-menu.dto';
import { QueryErpSysMenuDto } from './dto/query-erp-sys-menu.dto';
import { ReorderErpSysMenuDto } from './dto/reorder-erp-sys-menu.dto';
import { UpdateErpSysMenuDto } from './dto/update-erp-sys-menu.dto';

type MenuNode = ErpMenu & { children: MenuNode[] };

function buildTree(items: ErpMenu[], parentId: bigint | null = null): MenuNode[] {
  return items
    .filter((item) => item.parentId === parentId)
    .sort((a, b) => a.sortOrder - b.sortOrder)
    .map((item) => ({ ...item, children: buildTree(items, item.id) }));
}

/**
 * Remove ITEM nodes not in allowedIds; drop GROUP that become empty.
 * MODULE selalu dipertahankan (stub kosong = "coming soon" placeholder di sidebar).
 */
function pruneTree(nodes: MenuNode[], allowedIds: Set<bigint> | null): MenuNode[] {
  return nodes.flatMap((node) => {
    if (node.type === 'ITEM') {
      return allowedIds === null || allowedIds.has(node.id) ? [node] : [];
    }
    const prunedChildren = pruneTree(node.children, allowedIds);
    if (node.type === 'MODULE') {
      return [{ ...node, children: prunedChildren }];
    }
    return prunedChildren.length > 0 ? [{ ...node, children: prunedChildren }] : [];
  });
}

@Injectable()
export class ErpSysMenusService {
  constructor(private prisma: PrismaService) {}

  async create(dto: CreateErpSysMenuDto, actorId?: string) {
    const existing = await this.prisma.erpMenu.findFirst({
      where: { code: dto.code },
      select: { id: true, deletedAt: true },
    });

    const actorBigInt = toAuditUserId(actorId) ? BigInt(toAuditUserId(actorId) as number) : undefined;
    const sharedData = {
      title: dto.title,
      path: dto.path,
      icon: dto.icon,
      type: dto.type,
      parentId: dto.parentId ? BigInt(dto.parentId) : null,
      sortOrder: dto.sortOrder,
      isActive: dto.isActive,
      updatedById: actorBigInt,
      deletedAt: null,
      updatedAt: new Date(),
    };

    if (existing) {
      // Restore soft-deleted or update live record with matching code
      const updated = await this.prisma.erpMenu.update({
        where: { id: existing.id },
        data: sharedData,
      });
      return { success: true, data: updated };
    }

    const created = await this.prisma.erpMenu.create({
      data: {
        code: dto.code,
        ...sharedData,
        createdById: actorBigInt,
      },
    });

    return { success: true, data: created };
  }

  async findAll(query: QueryErpSysMenuDto) {
    const where: {
      deletedAt: null;
      type?: typeof query.type;
      parentId?: bigint | null;
      isActive?: boolean;
    } = { deletedAt: null };

    if (query.type) where.type = query.type;
    if (query.parentId === 'null') {
      where.parentId = null;
    } else if (query.parentId) {
      where.parentId = BigInt(query.parentId);
    }
    if (query.isActive !== undefined) where.isActive = query.isActive;

    const items = await this.prisma.erpMenu.findMany({
      where,
      orderBy: [{ sortOrder: 'asc' }, { title: 'asc' }],
    });

    return { success: true, data: items };
  }

  async findOne(id: bigint) {
    const item = await this.prisma.erpMenu.findFirst({
      where: { id, deletedAt: null },
      include: { children: { where: { deletedAt: null }, orderBy: { sortOrder: 'asc' } } },
    });
    if (!item) throw new NotFoundException('ERP menu not found');
    return { success: true, data: item };
  }

  async getTree() {
    const all = await this.prisma.erpMenu.findMany({
      where: { deletedAt: null },
      orderBy: [{ sortOrder: 'asc' }, { title: 'asc' }],
    });
    return { success: true, data: buildTree(all) };
  }

  /**
   * Return only the menus the requesting user may see:
   * - CENTRAL level → all active menus (no role filter)
   * - Other levels  → ITEMs where user's roles have canView=true; MODULE/GROUP
   *                   containers included automatically if they have visible children
   */
  async getMyMenus(userId: string, erpLevel: string) {
    let allowedIds: Set<bigint> | null = null; // null = no filter

    if (erpLevel !== 'CENTRAL') {
      const userRoles = await this.prisma.erpUserRole.findMany({
        where: { userId: BigInt(userId) },
        select: { roleId: true },
      });
      const roleIds = userRoles.map((r) => r.roleId);

      const roleMenus = await this.prisma.erpRoleMenu.findMany({
        where: { roleId: { in: roleIds }, canView: true },
        select: { menuId: true },
      });
      allowedIds = new Set(roleMenus.map((rm) => rm.menuId));
    }

    const all = await this.prisma.erpMenu.findMany({
      where: { deletedAt: null, isActive: true },
      orderBy: [{ sortOrder: 'asc' }, { title: 'asc' }],
    });

    const tree = buildTree(all);
    const data = pruneTree(tree, allowedIds);
    return { success: true, data };
  }

  async update(id: bigint, dto: UpdateErpSysMenuDto, actorId?: string) {
    const existing = await this.prisma.erpMenu.findFirst({
      where: { id, deletedAt: null },
    });
    if (!existing) throw new NotFoundException('ERP menu not found');

    if (dto.code && dto.code !== existing.code) {
      const duplicate = await this.prisma.erpMenu.findFirst({
        where: { code: dto.code, NOT: { id } },
        select: { id: true },
      });
      if (duplicate) throw new BadRequestException(`Menu code "${dto.code}" already exists`);
    }

    const actorBigInt = toAuditUserId(actorId) ? BigInt(toAuditUserId(actorId) as number) : undefined;

    const updated = await this.prisma.erpMenu.update({
      where: { id },
      data: {
        code: dto.code,
        title: dto.title,
        path: dto.path,
        icon: dto.icon,
        type: dto.type,
        parentId: dto.parentId !== undefined ? (dto.parentId ? BigInt(dto.parentId) : null) : undefined,
        sortOrder: dto.sortOrder,
        isActive: dto.isActive,
        updatedById: actorBigInt,
      },
    });

    return { success: true, data: updated };
  }

  async bulkUpdateStatus(dto: BulkStatusErpSysMenuDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = toAuditUserId(actorId) ? BigInt(toAuditUserId(actorId) as number) : undefined;
    const { count } = await this.prisma.erpMenu.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { isActive: dto.isActive, updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async bulkDelete(dto: BulkErpSysMenuDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = toAuditUserId(actorId) ? BigInt(toAuditUserId(actorId) as number) : undefined;
    const { count } = await this.prisma.erpMenu.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { deletedAt: new Date(), updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  /**
   * Atomic cross-parent reorder. Client sends final desired state for the
   * affected items: `{ id, parentId, sortOrder }[]`. Server validates type
   * hierarchy rules (MODULE root-only; GROUP under MODULE; ITEM under
   * MODULE/GROUP) + no-cycle (cannot move a node into itself or its
   * descendant), then applies all updates in one transaction.
   */
  async reorder(dto: ReorderErpSysMenuDto, actorId?: string) {
    if (dto.items.length === 0) return { success: true, affected: 0 };

    const all = await this.prisma.erpMenu.findMany({
      where: { deletedAt: null },
      select: { id: true, type: true, parentId: true },
    });
    const byId = new Map(all.map((m) => [m.id.toString(), m]));

    // Pre-compute descendants for cycle check (parent → children index)
    const childrenIndex = new Map<string, string[]>();
    for (const m of all) {
      const pk = m.parentId ? m.parentId.toString() : 'ROOT';
      const arr = childrenIndex.get(pk) ?? [];
      arr.push(m.id.toString());
      childrenIndex.set(pk, arr);
    }
    const collectDescendants = (rootId: string): Set<string> => {
      const out = new Set<string>();
      const stack = [rootId];
      while (stack.length) {
        const cur = stack.pop()!;
        const kids = childrenIndex.get(cur) ?? [];
        for (const k of kids) {
          if (!out.has(k)) {
            out.add(k);
            stack.push(k);
          }
        }
      }
      return out;
    };

    for (const item of dto.items) {
      const node = byId.get(item.id);
      if (!node) {
        throw new NotFoundException(`Menu ${item.id} not found`);
      }
      const newParentId = item.parentId ?? null;

      // Type hierarchy rule check
      if (node.type === 'MODULE' && newParentId !== null) {
        throw new BadRequestException(
          `MODULE menu cannot have a parent (id=${item.id})`,
        );
      }
      if (newParentId !== null) {
        const parent = byId.get(newParentId);
        if (!parent) {
          throw new BadRequestException(
            `Parent menu ${newParentId} not found`,
          );
        }
        if (node.type === 'GROUP' && parent.type !== 'MODULE') {
          throw new BadRequestException(
            `GROUP must be a child of MODULE (id=${item.id})`,
          );
        }
        if (node.type === 'ITEM' && parent.type === 'ITEM') {
          throw new BadRequestException(
            `ITEM cannot be a child of ITEM (id=${item.id})`,
          );
        }
        // No cycle: newParentId must not be the node itself or any descendant
        if (newParentId === item.id) {
          throw new BadRequestException(
            `Menu ${item.id} cannot be its own parent`,
          );
        }
        const descendants = collectDescendants(item.id);
        if (descendants.has(newParentId)) {
          throw new BadRequestException(
            `Menu ${item.id} cannot be moved under its own descendant`,
          );
        }
      }
    }

    const actorBigInt = toAuditUserId(actorId)
      ? BigInt(toAuditUserId(actorId) as number)
      : undefined;
    const now = new Date();

    await this.prisma.$transaction(
      dto.items.map((item) =>
        this.prisma.erpMenu.update({
          where: { id: BigInt(item.id) },
          data: {
            parentId: item.parentId ? BigInt(item.parentId) : null,
            sortOrder: item.sortOrder,
            updatedById: actorBigInt,
            updatedAt: now,
          },
        }),
      ),
    );

    return { success: true, affected: dto.items.length };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpMenu.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) throw new NotFoundException('ERP menu not found');

    const actorBigInt = toAuditUserId(actorId) ? BigInt(toAuditUserId(actorId) as number) : undefined;

    await this.prisma.erpMenu.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actorBigInt },
    });

    return { success: true, message: 'ERP menu deleted' };
  }
}
