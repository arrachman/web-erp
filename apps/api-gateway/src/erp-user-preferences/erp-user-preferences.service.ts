import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { UpdateErpUserPreferencesDto } from './dto/update-erp-user-preferences.dto';

@Injectable()
export class ErpUserPreferencesService {
  constructor(private prisma: PrismaService) {}

  private toUserBigInt(userId: string | number | undefined): bigint {
    if (userId === undefined || userId === null || userId === '') {
      throw new BadRequestException('Authenticated user id missing');
    }
    try {
      return BigInt(userId);
    } catch {
      throw new BadRequestException('Invalid user id format');
    }
  }

  async findForUser(userId: string | number | undefined) {
    const uid = this.toUserBigInt(userId);
    const existing = await this.prisma.erpUserPreferences.findUnique({
      where: { userId: uid },
    });
    return { success: true, data: existing };
  }

  async upsertForUser(
    userId: string | number | undefined,
    dto: UpdateErpUserPreferencesDto,
  ) {
    const uid = this.toUserBigInt(userId);
    const metadata =
      dto.metadata === undefined
        ? undefined
        : (dto.metadata as Prisma.InputJsonValue);

    const data = {
      theme: dto.theme,
      language: dto.language,
      timezone: dto.timezone,
      dateFormat: dto.dateFormat,
      numberFormat: dto.numberFormat,
      tablePageSize: dto.tablePageSize,
      sidebarCollapsed: dto.sidebarCollapsed,
      metadata,
    };

    const saved = await this.prisma.erpUserPreferences.upsert({
      where: { userId: uid },
      create: {
        userId: uid,
        ...data,
        sidebarCollapsed: dto.sidebarCollapsed ?? false,
        createdById: uid,
        updatedById: uid,
      },
      update: {
        ...data,
        updatedById: uid,
      },
    });

    return { success: true, data: saved };
  }
}
