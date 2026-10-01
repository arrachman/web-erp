import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { ErpFiscalPeriodStatus } from '@prisma/client';
import { toAuditUserId } from '../common/utils/audit-user.util';
import { PrismaService } from '../prisma/prisma.service';
import { CreateErpFiscalPeriodDto } from './dto/create-erp-fiscal-period.dto';
import { QueryErpFiscalPeriodDto } from './dto/query-erp-fiscal-period.dto';
import { UpdateErpFiscalPeriodDto } from './dto/update-erp-fiscal-period.dto';

@Injectable()
export class ErpFiscalPeriodsService {
  constructor(private prisma: PrismaService) {}

  async create(dto: CreateErpFiscalPeriodDto, actorId?: string) {
    const existing = await this.prisma.erpFiscalPeriod.findFirst({
      where: { year: dto.year, periodNo: dto.periodNo },
      select: { id: true, deletedAt: true },
    });
    if (existing) {
      if (existing.deletedAt) {
        throw new BadRequestException(
          `Fiscal period ${dto.year}/${dto.periodNo} already exists (soft-deleted).`,
        );
      }
      throw new BadRequestException(`Fiscal period ${dto.year}/${dto.periodNo} already exists`);
    }

    const actorBigInt = toAuditUserId(actorId) ? BigInt(toAuditUserId(actorId) as number) : undefined;

    const created = await this.prisma.erpFiscalPeriod.create({
      data: {
        year: dto.year,
        periodNo: dto.periodNo,
        name: dto.name,
        startDate: dto.startDate,
        endDate: dto.endDate,
        status: dto.status ?? ErpFiscalPeriodStatus.OPEN,
        createdById: actorBigInt,
        updatedById: actorBigInt,
      },
    });

    return { success: true, data: created };
  }

  async findAll(query: QueryErpFiscalPeriodDto) {
    const where: {
      deletedAt: null;
      year?: number;
      status?: ErpFiscalPeriodStatus;
    } = { deletedAt: null };

    if (query.year) where.year = query.year;
    if (query.status) where.status = query.status;

    const items = await this.prisma.erpFiscalPeriod.findMany({
      where,
      orderBy: [{ year: 'desc' }, { periodNo: 'asc' }],
    });

    return { success: true, data: items };
  }

  async findOne(id: bigint) {
    const item = await this.prisma.erpFiscalPeriod.findFirst({
      where: { id, deletedAt: null },
    });
    if (!item) throw new NotFoundException('Fiscal period not found');
    return { success: true, data: item };
  }

  async update(id: bigint, dto: UpdateErpFiscalPeriodDto, actorId?: string) {
    const existing = await this.prisma.erpFiscalPeriod.findFirst({
      where: { id, deletedAt: null },
    });
    if (!existing) throw new NotFoundException('Fiscal period not found');

    if (
      (dto.year !== undefined && dto.year !== existing.year) ||
      (dto.periodNo !== undefined && dto.periodNo !== existing.periodNo)
    ) {
      const newYear = dto.year ?? existing.year;
      const newPeriodNo = dto.periodNo ?? existing.periodNo;
      const duplicate = await this.prisma.erpFiscalPeriod.findFirst({
        where: { year: newYear, periodNo: newPeriodNo, NOT: { id } },
        select: { id: true },
      });
      if (duplicate) {
        throw new BadRequestException(`Fiscal period ${newYear}/${newPeriodNo} already exists`);
      }
    }

    const actorBigInt = toAuditUserId(actorId) ? BigInt(toAuditUserId(actorId) as number) : undefined;

    const updated = await this.prisma.erpFiscalPeriod.update({
      where: { id },
      data: {
        year: dto.year,
        periodNo: dto.periodNo,
        name: dto.name,
        startDate: dto.startDate,
        endDate: dto.endDate,
        status: dto.status,
        updatedById: actorBigInt,
      },
    });

    return { success: true, data: updated };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpFiscalPeriod.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) throw new NotFoundException('Fiscal period not found');

    const actorBigInt = toAuditUserId(actorId) ? BigInt(toAuditUserId(actorId) as number) : undefined;

    await this.prisma.erpFiscalPeriod.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actorBigInt },
    });

    return { success: true, message: 'Fiscal period deleted' };
  }

  async openPeriod(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpFiscalPeriod.findFirst({
      where: { id, deletedAt: null },
    });
    if (!existing) throw new NotFoundException('Fiscal period not found');

    const allowedFromStatuses: ErpFiscalPeriodStatus[] = [
      ErpFiscalPeriodStatus.SOFT_CLOSED,
      ErpFiscalPeriodStatus.CLOSED,
    ];
    if (!allowedFromStatuses.includes(existing.status)) {
      throw new BadRequestException(
        `Cannot open period with current status "${existing.status}". Must be SOFT_CLOSED or CLOSED.`,
      );
    }

    const actorBigInt = toAuditUserId(actorId) ? BigInt(toAuditUserId(actorId) as number) : undefined;

    const updated = await this.prisma.erpFiscalPeriod.update({
      where: { id },
      data: {
        status: ErpFiscalPeriodStatus.REOPENED,
        reopenedAt: new Date(),
        reopenedById: actorBigInt,
        updatedById: actorBigInt,
      },
    });

    return { success: true, data: updated };
  }

  async closePeriod(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpFiscalPeriod.findFirst({
      where: { id, deletedAt: null },
    });
    if (!existing) throw new NotFoundException('Fiscal period not found');

    const closeableStatuses: ErpFiscalPeriodStatus[] = [
      ErpFiscalPeriodStatus.OPEN,
      ErpFiscalPeriodStatus.SOFT_CLOSED,
      ErpFiscalPeriodStatus.REOPENED,
    ];
    if (!closeableStatuses.includes(existing.status)) {
      throw new BadRequestException(
        `Cannot close period with current status "${existing.status}".`,
      );
    }

    const actorBigInt = toAuditUserId(actorId) ? BigInt(toAuditUserId(actorId) as number) : undefined;

    const updated = await this.prisma.erpFiscalPeriod.update({
      where: { id },
      data: {
        status: ErpFiscalPeriodStatus.CLOSED,
        closedAt: new Date(),
        closedById: actorBigInt,
        updatedById: actorBigInt,
      },
    });

    return { success: true, data: updated };
  }
}
