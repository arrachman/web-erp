import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { QueryErpPermissionDto } from './dto/query-erp-permission.dto';

@Injectable()
export class ErpPermissionsService {
  constructor(private readonly prisma: PrismaService) {}

  async findAll(query: QueryErpPermissionDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;

    const where: Prisma.ErpPermissionWhereInput = { deletedAt: null };

    if (query.resource?.trim()) {
      where.group = { contains: query.resource.trim(), mode: 'insensitive' };
    }

    if (query.action?.trim()) {
      where.code = { contains: query.action.trim(), mode: 'insensitive' };
    }

    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { code: { equals: q, mode: 'insensitive' } },
        { name: { contains: q, mode: 'insensitive' } },
        { group: { contains: q, mode: 'insensitive' } },
        { description: { contains: q, mode: 'insensitive' } },
      ];
    }

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpPermission.findMany({
        where,
        orderBy: [{ [query.sortBy ?? 'code']: query.sortDir ?? 'asc' }],
        skip,
        take: limit,
      }),
      this.prisma.erpPermission.count({ where }),
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
    const item = await this.prisma.erpPermission.findFirst({
      where: { id: id as bigint, deletedAt: null },
    });
    if (!item) {
      throw new NotFoundException('ERP Permission not found');
    }
    return { success: true, data: item };
  }
}
