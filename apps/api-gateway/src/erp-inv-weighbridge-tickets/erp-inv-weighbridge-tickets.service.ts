import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { InvWeighbridgeTicketPostingService } from './inv-weighbridge-ticket-posting.service';
import { enrichWeighbridgeTickets } from './inv-weighbridge-ticket-enrich';
import { CreateInvWeighbridgeTicketDto } from './dto/create-inv-weighbridge-ticket.dto';
import { QueryInvWeighbridgeTicketsDto } from './dto/query-inv-weighbridge-tickets.dto';
import { UpdateInvWeighbridgeTicketDto } from './dto/update-inv-weighbridge-ticket.dto';
import {
  InvWeighbridgeTicketTransitionAction as A,
  TransitionInvWeighbridgeTicketDto,
} from './dto/transition-inv-weighbridge-ticket.dto';
import {
  toBigInt,
  EDITABLE,
  NEXT,
  buildWeighbridgeWhere,
  genDocNumber,
} from './inv-weighbridge-ticket.helpers';

@Injectable()
export class ErpInvWeighbridgeTicketsService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly posting: InvWeighbridgeTicketPostingService,
  ) {}

  private async resolvePeriod(
    tx: Prisma.TransactionClient,
    fiscalPeriodId: string | undefined,
    date: string,
  ): Promise<bigint> {
    if (fiscalPeriodId) return BigInt(fiscalPeriodId);
    const d = new Date(date);
    const period = await tx.erpFiscalPeriod.findFirst({
      where: { deletedAt: null, startDate: { lte: d }, endDate: { gte: d } },
      select: { id: true },
    });
    if (!period) {
      throw new BadRequestException(`Tidak ada periode fiskal yang memuat tanggal ${date}.`);
    }
    return period.id;
  }

  private async findRaw(id: bigint) {
    const row = await this.prisma.erpInvWeighbridgeTicket.findFirst({
      where: { id, deletedAt: null },
    });
    if (!row) throw new NotFoundException('Weighbridge ticket tidak ditemukan');
    return row;
  }

  private async one(id: bigint) {
    const row = await this.findRaw(id);
    const [enriched] = await enrichWeighbridgeTickets(this.prisma, [row]);
    return { success: true, data: enriched };
  }

  // ── CRUD ──────────────────────────────────────────────────────────────────

  async create(dto: CreateInvWeighbridgeTicketDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;

    const created = await this.prisma.$transaction(async (tx) => {
      const fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.ticketDate);
      const docNumber = dto.docNumber ?? (await genDocNumber(tx, 'RW'));

      const gross = new Prisma.Decimal(dto.grossWeight ?? '0');
      const tare = new Prisma.Decimal(dto.tareWeight ?? '0');
      const net = gross.minus(tare);

      const row = await tx.erpInvWeighbridgeTicket.create({
        data: {
          docNumber,
          branchId: BigInt(dto.branchId),
          locationId: toBigInt(dto.locationId),
          ticketDate: new Date(dto.ticketDate),
          fiscalPeriodId,
          vehiclePlate: dto.vehiclePlate ?? null,
          driverName: dto.driverName ?? null,
          partnerId: toBigInt(dto.partnerId),
          itemId: toBigInt(dto.itemId),
          grossWeight: gross,
          tareWeight: tare,
          netWeight: net,
          unitPrice: dto.unitPrice != null ? new Prisma.Decimal(dto.unitPrice) : null,
          description: dto.description ?? null,
          notes: dto.notes ?? null,
          status: 'DRAFT',
          postingStatus: 'UNPOSTED',
          legacyCode: dto.legacyCode ?? null,
          createdById: actor,
          updatedById: actor,
        },
        select: { id: true },
      });
      return row;
    });
    return this.one(created.id);
  }

  async findAll(query: QueryInvWeighbridgeTicketsDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const where = buildWeighbridgeWhere(query);

    const sortBy = query.sortBy ?? 'ticketDate';
    const sortDir = query.sortDir ?? 'desc';

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpInvWeighbridgeTicket.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }, { id: 'desc' }],
        skip: (page - 1) * limit,
        take: limit,
      }),
      this.prisma.erpInvWeighbridgeTicket.count({ where }),
    ]);

    return {
      success: true,
      data: await enrichWeighbridgeTickets(this.prisma, items),
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  findOne(id: bigint) {
    return this.one(id);
  }

  async update(id: bigint, dto: UpdateInvWeighbridgeTicketDto, actorId?: string) {
    const existing = await this.findRaw(id);
    if (!EDITABLE.has(existing.status)) {
      throw new BadRequestException(
        `Dokumen berstatus ${existing.status} tidak bisa diedit. Reopen dulu bila perlu.`,
      );
    }
    const actor = actorId ? BigInt(actorId) : null;

    await this.prisma.$transaction(async (tx) => {
      const data: Prisma.ErpInvWeighbridgeTicketUpdateInput = { updatedById: actor };

      if (dto.docNumber !== undefined) data.docNumber = dto.docNumber;
      if (dto.branchId !== undefined) data.branchId = BigInt(dto.branchId);
      if (dto.locationId !== undefined) data.locationId = toBigInt(dto.locationId);
      if (dto.vehiclePlate !== undefined) data.vehiclePlate = dto.vehiclePlate;
      if (dto.driverName !== undefined) data.driverName = dto.driverName;
      if (dto.partnerId !== undefined) data.partnerId = toBigInt(dto.partnerId);
      if (dto.itemId !== undefined) data.itemId = toBigInt(dto.itemId);
      if (dto.description !== undefined) data.description = dto.description;
      if (dto.notes !== undefined) data.notes = dto.notes;
      if (dto.legacyCode !== undefined) data.legacyCode = dto.legacyCode;
      if (dto.unitPrice !== undefined) {
        data.unitPrice = dto.unitPrice != null ? new Prisma.Decimal(dto.unitPrice) : null;
      }

      // Recompute netWeight if gross or tare changes
      const newGross =
        dto.grossWeight !== undefined
          ? new Prisma.Decimal(dto.grossWeight)
          : existing.grossWeight;
      const newTare =
        dto.tareWeight !== undefined ? new Prisma.Decimal(dto.tareWeight) : existing.tareWeight;
      if (dto.grossWeight !== undefined || dto.tareWeight !== undefined) {
        data.grossWeight = newGross;
        data.tareWeight = newTare;
        data.netWeight = newGross.minus(newTare);
      }

      if (dto.ticketDate !== undefined) {
        data.ticketDate = new Date(dto.ticketDate);
        data.fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.ticketDate);
      } else if (dto.fiscalPeriodId !== undefined) {
        data.fiscalPeriodId = BigInt(dto.fiscalPeriodId);
      }

      await tx.erpInvWeighbridgeTicket.update({ where: { id }, data });
    });
    return this.one(id);
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.findRaw(id);
    if (existing.status === 'POSTED') {
      throw new BadRequestException('Dokumen POSTED tidak bisa dihapus. Reopen dulu.');
    }
    const actor = actorId ? BigInt(actorId) : null;
    await this.prisma.erpInvWeighbridgeTicket.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actor },
    });
    return { success: true, message: 'Weighbridge ticket dihapus' };
  }

  // ── workflow (§2.7 state machine) ─────────────────────────────────────────

  async transition(id: bigint, dto: TransitionInvWeighbridgeTicketDto, actorId?: string) {
    const ticket = await this.findRaw(id);
    const actor = actorId ? BigInt(actorId) : null;
    const next = NEXT[ticket.status]?.[dto.action];
    if (!next) {
      throw new BadRequestException(`Aksi ${dto.action} tidak valid dari status ${ticket.status}.`);
    }
    if (dto.action === A.REJECT && !dto.reason?.trim()) {
      throw new BadRequestException('Alasan reject wajib diisi.');
    }

    if (dto.action === A.POST) {
      const period = await this.prisma.erpFiscalPeriod.findUnique({
        where: { id: ticket.fiscalPeriodId },
        select: { status: true },
      });
      if (period?.status === 'CLOSED') {
        throw new BadRequestException('Periode fiskal sudah ditutup — tidak bisa posting.');
      }
      await this.prisma.$transaction(async (tx) => {
        await this.posting.reverseTicket(tx, ticket.id);
        await this.posting.postTicket(tx, ticket.id, actor);
        await tx.erpInvWeighbridgeTicket.update({
          where: { id },
          data: {
            status: 'POSTED',
            postingStatus: 'POSTED',
            postedAt: new Date(),
            updatedById: actor,
          },
        });
      });
      return this.one(id);
    }

    if (dto.action === A.REOPEN) {
      await this.prisma.$transaction(async (tx) => {
        await this.posting.reverseTicket(tx, ticket.id);
        await tx.erpInvWeighbridgeTicket.update({
          where: { id },
          data: {
            status: 'DRAFT',
            postingStatus: 'UNPOSTED',
            postedAt: null,
            updatedById: actor,
          },
        });
      });
      return this.one(id);
    }

    await this.prisma.erpInvWeighbridgeTicket.update({
      where: { id },
      data: {
        status: next as never,
        updatedById: actor,
        ...(dto.action === A.REJECT
          ? { metadata: { ...((ticket.metadata as object) ?? {}), rejectReason: dto.reason } }
          : {}),
      },
    });
    return this.one(id);
  }
}
