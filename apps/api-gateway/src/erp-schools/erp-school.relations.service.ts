import { Injectable, NotFoundException } from '@nestjs/common';
import { ErpSchoolActivityType } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { CreateSchoolActivityDto, SchoolContactInputDto } from './dto/school-contact.dto';

/**
 * Sub-resources of a school: contacts (with CRM role) and the activity log
 * (kunjungan / negosiasi / catatan). Split from ErpSchoolsService so each
 * file stays within the project size limit; also used by the main service
 * for the form-driven replace-set contact sync and inline new-activity.
 */
@Injectable()
export class ErpSchoolRelationsService {
  constructor(private readonly prisma: PrismaService) {}

  async assertSchool(id: bigint) {
    const existing = await this.prisma.erpPartner.findFirst({
      where: {
        id,
        deletedAt: null,
        partnerType: { is: { code: 'CUST-SCHOOL', deletedAt: null } },
      },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('ERP Sekolah not found');
    }
  }

  // -------------------------------------------------------------------------
  // Activities
  // -------------------------------------------------------------------------

  async findActivities(id: bigint, page = 1, limit = 20) {
    await this.assertSchool(id);
    const skip = (page - 1) * limit;
    const where = { partnerId: id, deletedAt: null };
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpSchoolActivity.findMany({
        where,
        orderBy: { activityAt: 'desc' },
        skip,
        take: limit,
        include: { contact: { select: { id: true, name: true, role: true } } },
      }),
      this.prisma.erpSchoolActivity.count({ where }),
    ]);
    return {
      success: true,
      data: items,
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  async createActivity(id: bigint, dto: CreateSchoolActivityDto, actorId?: string) {
    await this.assertSchool(id);
    const created = await this.appendActivity(
      id,
      dto,
      actorId ? BigInt(actorId) : undefined,
      dto.contactId ? BigInt(dto.contactId) : undefined,
    );
    return { success: true, data: created };
  }

  async removeActivity(id: bigint, activityId: bigint, actorId?: string) {
    await this.assertSchool(id);
    const existing = await this.prisma.erpSchoolActivity.findFirst({
      where: { id: activityId, partnerId: id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('Aktivitas sekolah not found');
    }
    await this.prisma.erpSchoolActivity.update({
      where: { id: activityId },
      data: { deletedAt: new Date(), updatedById: actorId ? BigInt(actorId) : undefined },
    });
    return { success: true, message: 'Aktivitas dihapus' };
  }

  /** Append one activity; a VISIT also refreshes the profile's lastVisitAt. */
  async appendActivity(
    partnerId: bigint,
    input: { type: ErpSchoolActivityType; activityAt?: string; notes: string },
    actorBigInt?: bigint,
    contactId?: bigint,
  ) {
    const activityAt = input.activityAt ? new Date(input.activityAt) : new Date();
    const created = await this.prisma.erpSchoolActivity.create({
      data: {
        partnerId,
        contactId,
        type: input.type,
        activityAt,
        notes: input.notes,
        createdById: actorBigInt,
        updatedById: actorBigInt,
      },
    });
    if (input.type === ErpSchoolActivityType.VISIT) {
      await this.prisma.erpSchoolProfile.updateMany({
        where: {
          partnerId,
          OR: [{ lastVisitAt: null }, { lastVisitAt: { lt: activityAt } }],
        },
        data: { lastVisitAt: activityAt, updatedById: actorBigInt },
      });
    }
    return created;
  }

  // -------------------------------------------------------------------------
  // Contacts
  // -------------------------------------------------------------------------

  async addContact(id: bigint, dto: SchoolContactInputDto, actorId?: string) {
    await this.assertSchool(id);
    const actorBigInt = actorId ? BigInt(actorId) : undefined;
    const created = await this.prisma.erpPartnerContact.create({
      data: {
        partnerId: id,
        name: dto.name,
        role: dto.role,
        title: dto.title,
        phone: dto.phone,
        email: dto.email,
        isDefault: dto.isDefault ?? false,
        createdById: actorBigInt,
        updatedById: actorBigInt,
      },
    });
    return { success: true, data: created };
  }

  async updateContact(id: bigint, contactId: bigint, dto: SchoolContactInputDto, actorId?: string) {
    await this.assertSchool(id);
    const existing = await this.prisma.erpPartnerContact.findFirst({
      where: { id: contactId, partnerId: id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('Kontak sekolah not found');
    }
    const updated = await this.prisma.erpPartnerContact.update({
      where: { id: contactId },
      data: {
        name: dto.name,
        role: dto.role,
        title: dto.title,
        phone: dto.phone,
        email: dto.email,
        isDefault: dto.isDefault,
        updatedById: actorId ? BigInt(actorId) : undefined,
      },
    });
    return { success: true, data: updated };
  }

  async removeContact(id: bigint, contactId: bigint, actorId?: string) {
    await this.assertSchool(id);
    const existing = await this.prisma.erpPartnerContact.findFirst({
      where: { id: contactId, partnerId: id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('Kontak sekolah not found');
    }
    await this.prisma.erpPartnerContact.update({
      where: { id: contactId },
      data: { deletedAt: new Date(), updatedById: actorId ? BigInt(actorId) : undefined },
    });
    return { success: true, message: 'Kontak dihapus' };
  }

  /** Replace-set sync of a school's contacts from the form payload. */
  async syncContacts(
    partnerId: bigint,
    contacts: SchoolContactInputDto[],
    actorBigInt?: bigint,
  ) {
    const existing = await this.prisma.erpPartnerContact.findMany({
      where: { partnerId, deletedAt: null },
      select: { id: true },
    });
    const keepIds = new Set(
      contacts.filter((c) => c.id).map((c) => BigInt(c.id as string).toString()),
    );

    await this.prisma.$transaction(async (tx) => {
      for (const row of existing) {
        if (!keepIds.has(row.id.toString())) {
          await tx.erpPartnerContact.update({
            where: { id: row.id },
            data: { deletedAt: new Date(), updatedById: actorBigInt },
          });
        }
      }
      for (const c of contacts) {
        const payload = {
          name: c.name,
          role: c.role,
          title: c.title,
          phone: c.phone,
          email: c.email,
          isDefault: c.isDefault ?? false,
          updatedById: actorBigInt,
        };
        if (c.id) {
          await tx.erpPartnerContact.update({ where: { id: BigInt(c.id) }, data: payload });
        } else {
          await tx.erpPartnerContact.create({
            data: { partnerId, ...payload, createdById: actorBigInt },
          });
        }
      }
    });
  }
}
