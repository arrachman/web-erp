import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { isUniqueViolation, throwDuplicate } from '../common/errors/duplicate.util';
import { PrismaService } from '../prisma/prisma.service';
import { ErpAuditService } from '../erp-audit/erp-audit.service';
import { CreateErpSchoolDto } from './dto/create-erp-school.dto';
import { QueryErpSchoolDto } from './dto/query-erp-school.dto';
import { UpdateErpSchoolDto } from './dto/update-erp-school.dto';
import {
  SCHOOL_DETAIL_INCLUDE,
  SCHOOL_LIST_INCLUDE,
  buildErpSchoolWhere,
  buildErpSchoolOrderBy,
} from './erp-school.query-builders';
import { buildErpSchoolCreateData, buildErpSchoolUpdatePatch } from './erp-school.data-mappers';
import {
  buildOrderHistory,
  contractExpiryWindow,
  getNotOrderedIds,
  getSchoolOrderSummary,
} from './erp-school.insights';
import { ErpSchoolRelationsService } from './erp-school.relations.service';

/**
 * A school = a customer-capable `md_partners` row (partner_type_id = CUST-SCHOOL)
 * plus its 1:1 `md_school_profiles` row. Every order / invoice / AR / BAST
 * behaviour stays on the partner row untouched; the profile carries the
 * vertical CRM attributes (NPSN, jenjang, BOS pagu, student headcount,
 * pipeline stage). Contacts (with CRM role), the activity log, alerts and
 * the cross-channel order history complete the A1 scope.
 */
@Injectable()
export class ErpSchoolsService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly audit: ErpAuditService,
    private readonly relations: ErpSchoolRelationsService,
  ) {}

  async create(dto: CreateErpSchoolDto, actorId?: string) {
    const schoolTypeId = await this.getSchoolTypeId();
    const actorBigInt = actorId ? BigInt(actorId) : undefined;

    const existing = await this.prisma.erpPartner.findFirst({
      where: { code: dto.code },
      select: { id: true, deletedAt: true },
    });
    if (existing) {
      throwDuplicate({
        fieldLabel: 'Sekolah code',
        value: dto.code,
        isSoftDeleted: Boolean(existing.deletedAt),
      });
    }

    const data = buildErpSchoolCreateData(dto, schoolTypeId, actorBigInt);

    let created: any;
    try {
      created = await this.prisma.erpPartner.create({
        data,
        include: { ...SCHOOL_DETAIL_INCLUDE },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code', 'md_partners_code_key'])) {
        throwDuplicate({ fieldLabel: 'Sekolah code', value: dto.code });
      }
      throw error;
    }

    if (dto.newActivity) {
      await this.relations.appendActivity(created.id, dto.newActivity, actorBigInt);
      created = await this.prisma.erpPartner.findFirst({
        where: { id: created.id },
        include: { ...SCHOOL_DETAIL_INCLUDE },
      });
    }

    this.audit.log({
      action: 'CREATE',
      entityName: 'erp_schools',
      entityId: created.id,
      summary: `Sekolah "${created.code} - ${created.name}" dibuat`,
      actorId: actorBigInt,
    });

    return { success: true, data: created };
  }

  async findAll(query: QueryErpSchoolDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;

    let where = buildErpSchoolWhere(query);
    where = await this.applyAlertFilter(where, query);
    const orderBy = buildErpSchoolOrderBy(query);

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpPartner.findMany({
        where,
        orderBy,
        skip,
        take: limit,
        include: { ...SCHOOL_LIST_INCLUDE },
      }),
      this.prisma.erpPartner.count({ where }),
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

  async findOne(id: bigint) {
    const item = await this.prisma.erpPartner.findFirst({
      where: { id, deletedAt: null },
      include: { ...SCHOOL_DETAIL_INCLUDE },
    });
    if (!item) {
      throw new NotFoundException('ERP Sekolah not found');
    }
    const summary = await getSchoolOrderSummary(
      this.prisma,
      id,
      (item as any).schoolProfile?.pipelineStage ?? 'PROSPEK',
    );
    return { success: true, data: { ...item, orderSummary: summary } };
  }

  async update(id: bigint, dto: UpdateErpSchoolDto, actorId?: string) {
    const existing = await this.prisma.erpPartner.findFirst({
      where: { id, deletedAt: null },
    });
    if (!existing) {
      throw new NotFoundException('ERP Sekolah not found');
    }

    if (dto.code && dto.code !== existing.code) {
      const duplicate = await this.prisma.erpPartner.findFirst({
        where: { code: dto.code, NOT: { id } },
        select: { id: true, deletedAt: true },
      });
      if (duplicate) {
        throwDuplicate({
          fieldLabel: 'Sekolah code',
          value: dto.code,
          isSoftDeleted: Boolean(duplicate.deletedAt),
        });
      }
    }

    const actorBigInt = actorId ? BigInt(actorId) : undefined;
    const data = buildErpSchoolUpdatePatch(dto, actorBigInt);

    let updated: any;
    try {
      updated = await this.prisma.erpPartner.update({
        where: { id },
        data,
        include: { ...SCHOOL_DETAIL_INCLUDE },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code', 'md_partners_code_key'])) {
        throwDuplicate({ fieldLabel: 'Sekolah code', value: dto.code ?? existing.code });
      }
      throw error;
    }

    if (dto.contacts !== undefined) {
      await this.relations.syncContacts(id, dto.contacts, actorBigInt);
    }
    if (dto.newActivity) {
      await this.relations.appendActivity(id, dto.newActivity, actorBigInt);
    }
    if (dto.contacts !== undefined || dto.newActivity) {
      updated = await this.prisma.erpPartner.findFirst({
        where: { id },
        include: { ...SCHOOL_DETAIL_INCLUDE },
      });
    }

    this.audit.log({
      action: 'UPDATE',
      entityName: 'erp_schools',
      entityId: id,
      summary: `Sekolah "${updated.code} - ${updated.name}" diperbarui`,
      actorId: actorBigInt,
    });

    return { success: true, data: updated };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpPartner.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('ERP Sekolah not found');
    }

    const actorBigInt = actorId ? BigInt(actorId) : undefined;

    await this.prisma.erpPartner.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actorBigInt },
    });

    this.audit.log({
      action: 'DELETE',
      entityName: 'erp_schools',
      entityId: id,
      summary: `Sekolah dihapus`,
      actorId: actorBigInt,
    });

    return { success: true, message: 'ERP Sekolah deleted' };
  }

  async bulkStatus(ids: string[], isActive: boolean, actorId?: string) {
    const actorBigInt = actorId ? BigInt(actorId) : undefined;
    const res = await this.prisma.erpPartner.updateMany({
      where: { id: { in: ids.map((v) => BigInt(v)) }, deletedAt: null },
      data: { isActive, updatedById: actorBigInt },
    });
    return { success: true, affected: res.count };
  }

  async bulkRemove(ids: string[], actorId?: string) {
    const actorBigInt = actorId ? BigInt(actorId) : undefined;
    const res = await this.prisma.erpPartner.updateMany({
      where: { id: { in: ids.map((v) => BigInt(v)) }, deletedAt: null },
      data: { deletedAt: new Date(), updatedById: actorBigInt },
    });
    return { success: true, affected: res.count };
  }

  // ---------------------------------------------------------------------------
  // Alerts (peringatan CRM)
  // ---------------------------------------------------------------------------

  /** "Belum belanja" pada tahun BOS + "kontrak akan berakhir" (≤ 90 hari). */
  async findAlerts(bosYear?: number) {
    const year = bosYear ?? new Date().getFullYear();
    const baseWhere = buildErpSchoolWhere({} as QueryErpSchoolDto);
    const schools = await this.prisma.erpPartner.findMany({
      where: baseWhere,
      select: {
        id: true,
        code: true,
        name: true,
        schoolProfile: {
          select: { npsn: true, jenjang: true, contractExpiryAt: true, bosPeriodYear: true },
        },
      },
    });
    const ids = schools.map((s) => s.id);
    const notOrderedIds = new Set((await getNotOrderedIds(this.prisma, ids, year)).map(String));
    const { gte, lte } = contractExpiryWindow();
    const now = new Date();

    const notOrdered = schools
      .filter((s) => notOrderedIds.has(s.id.toString()))
      .map((s) => ({
        id: s.id,
        code: s.code,
        name: s.name,
        npsn: s.schoolProfile?.npsn ?? null,
        jenjang: s.schoolProfile?.jenjang ?? null,
      }));
    const contractExpiring = schools
      .filter((s) => {
        const exp = s.schoolProfile?.contractExpiryAt;
        return exp != null && exp >= gte && exp <= lte;
      })
      .map((s) => ({
        id: s.id,
        code: s.code,
        name: s.name,
        npsn: s.schoolProfile?.npsn ?? null,
        contractExpiryAt: s.schoolProfile?.contractExpiryAt ?? null,
        daysRemaining: Math.ceil(
          (new Date(s.schoolProfile!.contractExpiryAt!).getTime() - now.getTime()) / 86_400_000,
        ),
      }));

    return { success: true, data: { bosYear: year, notOrdered, contractExpiring } };
  }

  // ---------------------------------------------------------------------------
  // Order history (lintas kanal)
  // ---------------------------------------------------------------------------

  async getOrderHistory(id: bigint) {
    await this.relations.assertSchool(id);
    const rows = await buildOrderHistory(this.prisma, id);
    return { success: true, data: rows };
  }

  // ---------------------------------------------------------------------------
  // Helpers
  // ---------------------------------------------------------------------------

  private async applyAlertFilter(
    where: Prisma.ErpPartnerWhereInput,
    query: QueryErpSchoolDto,
  ): Promise<Prisma.ErpPartnerWhereInput> {
    if (!query.alert) return where;
    if (query.alert === 'CONTRACT_EXPIRING') {
      return {
        AND: [where, { schoolProfile: { is: { contractExpiryAt: contractExpiryWindow() } } }],
      };
    }
    // NOT_ORDERED: resolve candidate ids first, then exclude schools that
    // already ordered in the BOS year (bosYear, default tahun berjalan).
    const year = query.bosYear ?? new Date().getFullYear();
    const candidates = await this.prisma.erpPartner.findMany({ where, select: { id: true } });
    const allowed = await getNotOrderedIds(
      this.prisma,
      candidates.map((c) => c.id),
      year,
    );
    return { AND: [where, { id: { in: allowed } }] };
  }

  private async getSchoolTypeId(): Promise<bigint> {
    const type = await this.prisma.erpPartnerType.findFirst({
      where: { code: 'CUST-SCHOOL', deletedAt: null, isActive: true },
      select: { id: true },
    });
    if (!type) {
      throw new NotFoundException(
        'Partner type "CUST-SCHOOL" tidak ditemukan di master tipe partner.',
      );
    }
    return type.id;
  }
}
