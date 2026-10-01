import { Injectable, NotFoundException } from '@nestjs/common';
import { isUniqueViolation, throwDuplicate } from '../common/errors/duplicate.util';
import { PrismaService } from '../prisma/prisma.service';
import { CreateErpPartnerDto } from './dto/create-erp-partner.dto';
import { QueryErpPartnerDto } from './dto/query-erp-partner.dto';
import { UpdateErpPartnerDto } from './dto/update-erp-partner.dto';
import { CreateErpPartnerAddressDto } from './dto/create-erp-partner-address.dto';
import { UpdateErpPartnerAddressDto } from './dto/update-erp-partner-address.dto';
import { CreateErpPartnerContactDto } from './dto/create-erp-partner-contact.dto';
import { UpdateErpPartnerContactDto } from './dto/update-erp-partner-contact.dto';
import { CreateErpPartnerBankAccountDto } from './dto/create-erp-partner-bank-account.dto';
import {
  PARTNER_LIST_INCLUDE,
  PARTNER_DETAIL_INCLUDE,
  PARTNER_MUTATION_INCLUDE,
  PARTNER_ADDRESS_GEO_INCLUDE,
  buildErpPartnerWhere,
  buildErpPartnerOrderBy,
} from './erp-partner.query-builders';
import {
  buildErpPartnerCreateData,
  buildErpPartnerUpdatePatch,
} from './erp-partner.data-mappers';

@Injectable()
export class ErpPartnersService {
  constructor(private readonly prisma: PrismaService) {}

  // ---------------------------------------------------------------------------
  // Partner CRUD
  // ---------------------------------------------------------------------------

  async create(dto: CreateErpPartnerDto, actorId?: string) {
    const existing = await this.prisma.erpPartner.findFirst({
      where: { code: dto.code },
      select: { id: true, deletedAt: true },
    });
    if (existing) {
      throwDuplicate({
        fieldLabel: 'Partner code',
        value: dto.code,
        isSoftDeleted: Boolean(existing.deletedAt),
      });
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;
    const data = buildErpPartnerCreateData(dto, actorBigInt);

    let created;
    try {
      created = await this.prisma.erpPartner.create({
        data,
        include: { ...PARTNER_MUTATION_INCLUDE },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code', 'md_partners_code_key'])) {
        throwDuplicate({ fieldLabel: 'Partner code', value: dto.code });
      }
      throw error;
    }

    return { success: true, data: created };
  }

  async findAll(query: QueryErpPartnerDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;

    const where = buildErpPartnerWhere(query);
    const orderBy = buildErpPartnerOrderBy(query);

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpPartner.findMany({
        where,
        orderBy,
        skip,
        take: limit,
        include: { ...PARTNER_LIST_INCLUDE },
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
      include: { ...PARTNER_DETAIL_INCLUDE },
    });
    if (!item) {
      throw new NotFoundException('ERP Partner not found');
    }
    return { success: true, data: item };
  }

  async update(id: bigint, dto: UpdateErpPartnerDto, actorId?: string) {
    const existing = await this.prisma.erpPartner.findFirst({
      where: { id, deletedAt: null },
    });
    if (!existing) {
      throw new NotFoundException('ERP Partner not found');
    }

    if (dto.code && dto.code !== existing.code) {
      const duplicate = await this.prisma.erpPartner.findFirst({
        where: { code: dto.code, NOT: { id } },
        select: { id: true, deletedAt: true },
      });
      if (duplicate) {
        throwDuplicate({
          fieldLabel: 'Partner code',
          value: dto.code,
          isSoftDeleted: Boolean(duplicate.deletedAt),
        });
      }
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;
    const data = buildErpPartnerUpdatePatch(dto, actorBigInt);

    let updated;
    try {
      updated = await this.prisma.erpPartner.update({
        where: { id },
        data,
        include: { ...PARTNER_MUTATION_INCLUDE },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code', 'md_partners_code_key'])) {
        throwDuplicate({ fieldLabel: 'Partner code', value: dto.code ?? existing.code });
      }
      throw error;
    }

    return { success: true, data: updated };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpPartner.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('ERP Partner not found');
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;

    await this.prisma.erpPartner.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actorBigInt },
    });

    return { success: true, message: 'ERP Partner deleted' };
  }

  // ---------------------------------------------------------------------------
  // Addresses
  // ---------------------------------------------------------------------------

  async addAddress(partnerId: bigint, dto: CreateErpPartnerAddressDto, actorId?: string) {
    await this.assertPartnerExists(partnerId);
    const actorBigInt = actorId ? BigInt(actorId) : null;

    const address = await this.prisma.erpPartnerAddress.create({
      data: {
        partnerId,
        type: dto.type,
        addressLine1: dto.addressLine1,
        addressLine2: dto.addressLine2,
        countryId: dto.countryId ? BigInt(dto.countryId) : null,
        provinceId: dto.provinceId ? BigInt(dto.provinceId) : null,
        cityId: dto.cityId ? BigInt(dto.cityId) : null,
        areaId: dto.areaId ? BigInt(dto.areaId) : null,
        subAreaId: dto.subAreaId ? BigInt(dto.subAreaId) : null,
        postalCode: dto.postalCode,
        phone: dto.phone,
        fax: dto.fax,
        email: dto.email,
        website: dto.website,
        isDefault: dto.isDefault ?? false,
        createdById: actorBigInt,
        updatedById: actorBigInt,
      },
      include: { ...PARTNER_ADDRESS_GEO_INCLUDE },
    });

    return { success: true, data: address };
  }

  async updateAddress(addressId: bigint, dto: UpdateErpPartnerAddressDto, actorId?: string) {
    const existing = await this.prisma.erpPartnerAddress.findFirst({
      where: { id: addressId, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('Partner address not found');
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;

    const address = await this.prisma.erpPartnerAddress.update({
      where: { id: addressId },
      data: {
        type: dto.type,
        addressLine1: dto.addressLine1,
        addressLine2: dto.addressLine2,
        countryId: dto.countryId !== undefined ? (dto.countryId ? BigInt(dto.countryId) : null) : undefined,
        provinceId: dto.provinceId !== undefined ? (dto.provinceId ? BigInt(dto.provinceId) : null) : undefined,
        cityId: dto.cityId !== undefined ? (dto.cityId ? BigInt(dto.cityId) : null) : undefined,
        areaId: dto.areaId !== undefined ? (dto.areaId ? BigInt(dto.areaId) : null) : undefined,
        subAreaId: dto.subAreaId !== undefined ? (dto.subAreaId ? BigInt(dto.subAreaId) : null) : undefined,
        postalCode: dto.postalCode,
        phone: dto.phone,
        fax: dto.fax,
        email: dto.email,
        website: dto.website,
        isDefault: dto.isDefault,
        updatedById: actorBigInt,
      },
      include: { ...PARTNER_ADDRESS_GEO_INCLUDE },
    });

    return { success: true, data: address };
  }

  async removeAddress(addressId: bigint, actorId?: string) {
    const existing = await this.prisma.erpPartnerAddress.findFirst({
      where: { id: addressId, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('Partner address not found');
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;

    await this.prisma.erpPartnerAddress.update({
      where: { id: addressId },
      data: { deletedAt: new Date(), updatedById: actorBigInt },
    });

    return { success: true, message: 'Partner address deleted' };
  }

  // ---------------------------------------------------------------------------
  // Contacts
  // ---------------------------------------------------------------------------

  async addContact(partnerId: bigint, dto: CreateErpPartnerContactDto, actorId?: string) {
    await this.assertPartnerExists(partnerId);
    const actorBigInt = actorId ? BigInt(actorId) : null;

    const contact = await this.prisma.erpPartnerContact.create({
      data: {
        partnerId,
        name: dto.name,
        title: dto.title,
        phone: dto.phone,
        email: dto.email,
        isDefault: dto.isDefault ?? false,
        createdById: actorBigInt,
        updatedById: actorBigInt,
      },
    });

    return { success: true, data: contact };
  }

  async updateContact(contactId: bigint, dto: UpdateErpPartnerContactDto, actorId?: string) {
    const existing = await this.prisma.erpPartnerContact.findFirst({
      where: { id: contactId, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('Partner contact not found');
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;

    const contact = await this.prisma.erpPartnerContact.update({
      where: { id: contactId },
      data: {
        name: dto.name,
        title: dto.title,
        phone: dto.phone,
        email: dto.email,
        isDefault: dto.isDefault,
        updatedById: actorBigInt,
      },
    });

    return { success: true, data: contact };
  }

  async removeContact(contactId: bigint, actorId?: string) {
    const existing = await this.prisma.erpPartnerContact.findFirst({
      where: { id: contactId, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('Partner contact not found');
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;

    await this.prisma.erpPartnerContact.update({
      where: { id: contactId },
      data: { deletedAt: new Date(), updatedById: actorBigInt },
    });

    return { success: true, message: 'Partner contact deleted' };
  }

  // ---------------------------------------------------------------------------
  // Bank Accounts
  // ---------------------------------------------------------------------------

  async addBankAccount(partnerId: bigint, dto: CreateErpPartnerBankAccountDto, actorId?: string) {
    await this.assertPartnerExists(partnerId);
    const actorBigInt = actorId ? BigInt(actorId) : null;

    const bankAccount = await this.prisma.erpPartnerBankAccount.create({
      data: {
        partnerId,
        bankName: dto.bankName,
        accountNumber: dto.accountNumber,
        accountHolder: dto.accountHolder,
        isDefault: dto.isDefault ?? false,
        createdById: actorBigInt,
        updatedById: actorBigInt,
      },
    });

    return { success: true, data: bankAccount };
  }

  async removeBankAccount(bankId: bigint, actorId?: string) {
    const existing = await this.prisma.erpPartnerBankAccount.findFirst({
      where: { id: bankId, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('Partner bank account not found');
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;

    await this.prisma.erpPartnerBankAccount.update({
      where: { id: bankId },
      data: { deletedAt: new Date(), updatedById: actorBigInt },
    });

    return { success: true, message: 'Partner bank account deleted' };
  }

  // ---------------------------------------------------------------------------
  // Private helpers
  // ---------------------------------------------------------------------------

  private async assertPartnerExists(id: bigint): Promise<void> {
    const partner = await this.prisma.erpPartner.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!partner) {
      throw new NotFoundException('ERP Partner not found');
    }
  }
}