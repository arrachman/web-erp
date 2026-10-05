import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import {
  CreateWaTemplateDto,
  QueryWaTemplateDto,
  UpdateWaTemplateDto,
} from './dto/erp-whatsapp.dto';

/** CRUD template pesan WhatsApp (`sys_wa_templates`), variabel {{mustache}}. */
@Injectable()
export class ErpWaTemplatesService {
  constructor(private readonly prisma: PrismaService) {}

  async create(dto: CreateWaTemplateDto, actorId?: string) {
    const created = await this.prisma.erpWaTemplate.create({
      data: {
        name: dto.name,
        category: dto.category,
        triggerEvent: dto.triggerEvent ?? null,
        body: dto.body,
        isActive: dto.isActive ?? true,
        createdById: actorId ? BigInt(actorId) : null,
        updatedById: actorId ? BigInt(actorId) : null,
      },
    });
    return { success: true, data: this.view(created), message: 'Template dibuat' };
  }

  async findAll(query: QueryWaTemplateDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 100;
    const where: Prisma.ErpWaTemplateWhereInput = { deletedAt: null };
    if (query.category) where.category = query.category;
    if (query.search?.trim()) {
      const s = query.search.trim();
      where.OR = [
        { name: { contains: s, mode: 'insensitive' } },
        { triggerEvent: { contains: s, mode: 'insensitive' } },
        { body: { contains: s, mode: 'insensitive' } },
      ];
    }
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpWaTemplate.findMany({
        where,
        orderBy: [{ category: 'asc' }, { name: 'asc' }],
        skip: (page - 1) * limit,
        take: limit,
      }),
      this.prisma.erpWaTemplate.count({ where }),
    ]);
    return {
      success: true,
      data: items.map((t) => this.view(t)),
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) },
    };
  }

  async findOne(id: string) {
    const tpl = await this.prisma.erpWaTemplate.findFirst({
      where: { id: BigInt(id), deletedAt: null },
    });
    if (!tpl) throw new NotFoundException(`Template ${id} tidak ditemukan`);
    return { success: true, data: this.view(tpl) };
  }

  async update(id: string, dto: UpdateWaTemplateDto, actorId?: string) {
    await this.findOne(id);
    const updated = await this.prisma.erpWaTemplate.update({
      where: { id: BigInt(id) },
      data: { ...dto, updatedById: actorId ? BigInt(actorId) : null },
    });
    return { success: true, data: this.view(updated), message: 'Template diperbarui' };
  }

  async remove(id: string, actorId?: string) {
    await this.findOne(id);
    await this.prisma.erpWaTemplate.update({
      where: { id: BigInt(id) },
      data: {
        deletedAt: new Date(),
        isActive: false,
        updatedById: actorId ? BigInt(actorId) : null,
      },
    });
    return { success: true, message: 'Template dihapus' };
  }

  view(t: {
    id: bigint;
    name: string;
    category: string;
    triggerEvent: string | null;
    body: string;
    isActive: boolean;
    createdAt: Date;
    updatedAt: Date;
  }) {
    return {
      id: t.id.toString(),
      name: t.name,
      category: t.category,
      triggerEvent: t.triggerEvent,
      body: t.body,
      isActive: t.isActive,
      createdAt: t.createdAt,
      updatedAt: t.updatedAt,
    };
  }
}
