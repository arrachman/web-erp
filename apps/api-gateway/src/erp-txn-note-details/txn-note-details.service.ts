import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { ErpAuditService } from '../erp-audit/erp-audit.service';
import { diffFields } from '../erp-common/utils/diff-fields.util';
import { BulkErpTxnNoteDetailDto } from './dto/bulk-txn-note-detail.dto';
import { CreateErpTxnNoteDetailDto } from './dto/create-txn-note-detail.dto';
import { QueryErpTxnNoteDetailDto } from './dto/query-txn-note-detail.dto';
import { UpdateErpTxnNoteDetailDto } from './dto/update-txn-note-detail.dto';

const ENTITY = 'ErpTransactionNoteDetail';
const LABEL_ID = 'Detail Catatan Transaksi';

const NOTE_SELECT = { id: true, code: true, name: true };

@Injectable()
export class ErpTxnNoteDetailsService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly audit: ErpAuditService,
  ) {}

  async create(dto: CreateErpTxnNoteDetailDto, actorId?: string) {
    const noteId = BigInt(dto.transactionNoteId);
    const note = await this.prisma.erpTransactionNote.findFirst({ where: { id: noteId, deletedAt: null } });
    if (!note) throw new NotFoundException('Transaction Note not found');

    const actorBigInt = actorId ? BigInt(actorId) : null;
    const created = await this.prisma.erpTransactionNoteDetail.create({
      data: {
        transactionNoteId: noteId,
        sortOrder: dto.sortOrder ?? 0,
        content: dto.content,
        legacyCode: dto.legacyCode,
        createdById: actorBigInt,
        updatedById: actorBigInt,
      } as any,
      include: { transactionNote: { select: NOTE_SELECT } },
    });
    this.audit.log({
      action: 'CREATE', entityName: ENTITY, entityId: created.id,
      summary: `${LABEL_ID} untuk note ${note.code} dibuat`, actorId: actorBigInt ?? undefined,
    });
    return { success: true, data: created };
  }

  async findAll(query: QueryErpTxnNoteDetailDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;
    const where: Prisma.ErpTransactionNoteDetailWhereInput = { deletedAt: null };
    if (query.search?.trim()) {
      where.content = { contains: query.search.trim(), mode: 'insensitive' };
    }
    if (query.transactionNoteId) {
      where.transactionNoteId = BigInt(query.transactionNoteId);
    }
    const sortBy = query.sortBy ?? 'createdAt';
    const sortDir = query.sortDir ?? 'desc';
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpTransactionNoteDetail.findMany({
        where, orderBy: [{ [sortBy]: sortDir }], skip, take: limit,
        include: { transactionNote: { select: NOTE_SELECT } },
      }),
      this.prisma.erpTransactionNoteDetail.count({ where }),
    ]);
    return { success: true, data: items, meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 } };
  }

  async findOne(id: bigint) {
    const item = await this.prisma.erpTransactionNoteDetail.findFirst({
      where: { id, deletedAt: null },
      include: { transactionNote: { select: NOTE_SELECT } },
    });
    if (!item) throw new NotFoundException(`${ENTITY} not found`);
    return { success: true, data: item };
  }

  async findByNoteId(noteId: bigint) {
    const items = await this.prisma.erpTransactionNoteDetail.findMany({
      where: { transactionNoteId: noteId, deletedAt: null },
      orderBy: [{ sortOrder: 'asc' }],
      include: { transactionNote: { select: NOTE_SELECT } },
    });
    return { success: true, data: items };
  }

  async update(id: bigint, dto: UpdateErpTxnNoteDetailDto, actorId?: string) {
    const existing = await this.prisma.erpTransactionNoteDetail.findFirst({ where: { id, deletedAt: null } });
    if (!existing) throw new NotFoundException(`${ENTITY} not found`);
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const data: Prisma.ErpTransactionNoteDetailUpdateInput = {};
    if (dto.transactionNoteId !== undefined) {
      const noteId = BigInt(dto.transactionNoteId);
      const note = await this.prisma.erpTransactionNote.findFirst({ where: { id: noteId, deletedAt: null } });
      if (!note) throw new NotFoundException('Transaction Note not found');
      data.transactionNote = { connect: { id: noteId } };
    }
    if (dto.sortOrder !== undefined) data.sortOrder = dto.sortOrder;
    if (dto.content !== undefined) data.content = dto.content;
    if (dto.legacyCode !== undefined) data.legacyCode = dto.legacyCode;
    (data as any).updatedById = actorBigInt;
    const updated = await this.prisma.erpTransactionNoteDetail.update({
      where: { id }, data,
      include: { transactionNote: { select: NOTE_SELECT } },
    });
    const changes = diffFields(existing as unknown as Record<string, unknown>, updated as unknown as Record<string, unknown>);
    this.audit.log({
      action: 'UPDATE', entityName: ENTITY, entityId: id, changes,
      summary: `${LABEL_ID} id=${id} diperbarui`, actorId: actorBigInt ?? undefined,
    });
    return { success: true, data: updated };
  }

  async bulkDelete(dto: BulkErpTxnNoteDetailDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpTransactionNoteDetail.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { deletedAt: new Date(), updatedById: actorBigInt, updatedAt: new Date() } as any,
    });
    return { success: true, affected: count };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpTransactionNoteDetail.findFirst({ where: { id, deletedAt: null }, select: { id: true } });
    if (!existing) throw new NotFoundException(`${ENTITY} not found`);
    const actorBigInt = actorId ? BigInt(actorId) : null;
    await this.prisma.erpTransactionNoteDetail.update({
      where: { id }, data: { deletedAt: new Date(), updatedById: actorBigInt } as any,
    });
    this.audit.log({ action: 'DELETE', entityName: ENTITY, entityId: id, summary: `${LABEL_ID} id=${id} dihapus`, actorId: actorBigInt ?? undefined });
    return { success: true, message: `${ENTITY} deleted` };
  }
}
