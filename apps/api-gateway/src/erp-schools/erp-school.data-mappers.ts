import { Prisma } from '@prisma/client';
import { CreateErpSchoolDto } from './dto/create-erp-school.dto';
import { UpdateErpSchoolDto } from './dto/update-erp-school.dto';

/**
 * Build junction rows for one multi-select dimension.
 */
export function buildDimRows<K extends string>(
  ids: string[] | undefined,
  key: K,
): Record<K, bigint>[] | undefined {
  if (!ids) return undefined;
  const unique = Array.from(new Set(ids.filter((v) => v !== '')));
  return unique.map((v) => ({ [key]: BigInt(v) }) as Record<K, bigint>);
}

/**
 * Denormalized single branch column = first branch id of the array.
 */
export function firstBranchSync(branchIds: string[] | undefined): bigint | null | undefined {
  if (branchIds === undefined) return undefined;
  const f = branchIds.find((v) => v !== '');
  return f ? BigInt(f) : null;
}

/**
 * Build the Prisma `data` payload for create.
 *
 * A school is created as a partner row (partner_type_id = SCHOOL) plus its
 * 1:1 school profile, in one nested-create. The salesman field is optional —
 * a school is a customer, so it may have a salesman but is not itself one.
 */
export function buildErpSchoolCreateData(
  dto: CreateErpSchoolDto,
  schoolTypeId: bigint,
  actorBigInt: bigint | undefined,
): Prisma.ErpPartnerUncheckedCreateInput {
  const salesmanBigInt = dto.salesmanId ? BigInt(dto.salesmanId) : null;

  return {
    code: dto.code,
    name: dto.name,
    partnerTypeId: schoolTypeId,
    salesmanId: salesmanBigInt,
    taxNumber: dto.taxNumber,
    isTaxable: dto.isTaxable ?? false,
    isActive: dto.isActive ?? true,
    createdById: actorBigInt,
    updatedById: actorBigInt,
    branchId: firstBranchSync(dto.branchIds) ?? null,
    ...(buildDimRows(dto.branchIds, 'branchId')
      ? { dimBranches: { create: buildDimRows(dto.branchIds, 'branchId') } }
      : {}),
    ...(buildDimRows(dto.warehouseIds, 'warehouseId')
      ? { dimWarehouses: { create: buildDimRows(dto.warehouseIds, 'warehouseId') } }
      : {}),
    ...(buildDimRows(dto.locationIds, 'locationId')
      ? { dimLocations: { create: buildDimRows(dto.locationIds, 'locationId') } }
      : {}),
    schoolProfile: {
      create: {
        npsn: dto.npsn,
        jenjang: dto.jenjang,
        negeriSwasta: dto.negeriSwasta,
        accreditation: dto.accreditation,
        studentCount: dto.studentCount,
        classCount: dto.classCount,
        bosPagu: dto.bosPagu != null ? BigInt(dto.bosPagu) : null,
        bosRealisasi: dto.bosRealisasi != null ? BigInt(dto.bosRealisasi) : null,
        bosPeriodLabel: dto.bosPeriodLabel,
        bosPeriodStage: dto.bosPeriodStage,
        bosPeriodYear: dto.bosPeriodYear,
        pipelineStage: dto.pipelineStage,
        studentsPerGrade: dto.studentsPerGrade,
        lastVisitAt: dto.lastVisitAt ? new Date(dto.lastVisitAt) : null,
        visitNotes: dto.visitNotes,
        negotiationNotes: dto.negotiationNotes,
        contractExpiryAt: dto.contractExpiryAt ? new Date(dto.contractExpiryAt) : null,
        isActive: dto.isActive ?? true,
        createdById: actorBigInt,
        updatedById: actorBigInt,
      },
    },
    ...(dto.contacts && dto.contacts.length > 0
      ? {
          contacts: {
            create: dto.contacts.map((c) => ({
              name: c.name,
              role: c.role,
              title: c.title,
              phone: c.phone,
              email: c.email,
              isDefault: c.isDefault ?? false,
              createdById: actorBigInt,
              updatedById: actorBigInt,
            })),
          },
        }
      : {}),
  };
}

/**
 * Build the Prisma `data` payload for update.
 *
 * CRITICAL create-vs-update distinction: update distinguishes `undefined`
 * ("no change") from `null` ("clear the value"). The school profile is
 * updated via a nested connect-update (the profile always exists once the
 * partner is a school).
 */
export function buildErpSchoolUpdatePatch(
  dto: UpdateErpSchoolDto,
  actorBigInt: bigint | undefined,
): Prisma.ErpPartnerUncheckedUpdateInput {
  const salesmanBigInt =
    dto.salesmanId !== undefined ? (dto.salesmanId ? BigInt(dto.salesmanId) : null) : undefined;

  const profilePatch: Prisma.ErpSchoolProfileUncheckedUpdateInput = {};
  if (dto.npsn !== undefined) profilePatch.npsn = dto.npsn;
  if (dto.jenjang !== undefined) profilePatch.jenjang = dto.jenjang;
  if (dto.negeriSwasta !== undefined) profilePatch.negeriSwasta = dto.negeriSwasta;
  if (dto.accreditation !== undefined) profilePatch.accreditation = dto.accreditation;
  if (dto.studentCount !== undefined) profilePatch.studentCount = dto.studentCount;
  if (dto.classCount !== undefined) profilePatch.classCount = dto.classCount;
  if (dto.bosPagu !== undefined) profilePatch.bosPagu = dto.bosPagu != null ? BigInt(dto.bosPagu) : null;
  if (dto.bosRealisasi !== undefined)
    profilePatch.bosRealisasi = dto.bosRealisasi != null ? BigInt(dto.bosRealisasi) : null;
  if (dto.bosPeriodLabel !== undefined) profilePatch.bosPeriodLabel = dto.bosPeriodLabel;
  if (dto.bosPeriodStage !== undefined) profilePatch.bosPeriodStage = dto.bosPeriodStage;
  if (dto.bosPeriodYear !== undefined) profilePatch.bosPeriodYear = dto.bosPeriodYear;
  if (dto.pipelineStage !== undefined) profilePatch.pipelineStage = dto.pipelineStage;
  if (dto.studentsPerGrade !== undefined)
    profilePatch.studentsPerGrade = dto.studentsPerGrade ?? undefined;
  if (dto.lastVisitAt !== undefined)
    profilePatch.lastVisitAt = dto.lastVisitAt ? new Date(dto.lastVisitAt) : null;
  if (dto.visitNotes !== undefined) profilePatch.visitNotes = dto.visitNotes;
  if (dto.negotiationNotes !== undefined) profilePatch.negotiationNotes = dto.negotiationNotes;
  if (dto.contractExpiryAt !== undefined)
    profilePatch.contractExpiryAt = dto.contractExpiryAt ? new Date(dto.contractExpiryAt) : null;
  if (dto.isActive !== undefined) profilePatch.isActive = dto.isActive;
  profilePatch.updatedById = actorBigInt;

  return {
    code: dto.code,
    name: dto.name,
    salesmanId: salesmanBigInt,
    taxNumber: dto.taxNumber,
    isTaxable: dto.isTaxable,
    isActive: dto.isActive,
    updatedById: actorBigInt,
    branchId: firstBranchSync(dto.branchIds),
    ...(dto.branchIds !== undefined
      ? { dimBranches: { deleteMany: {}, create: buildDimRows(dto.branchIds, 'branchId') } }
      : {}),
    ...(dto.warehouseIds !== undefined
      ? {
          dimWarehouses: {
            deleteMany: {},
            create: buildDimRows(dto.warehouseIds, 'warehouseId'),
          },
        }
      : {}),
    ...(dto.locationIds !== undefined
      ? {
          dimLocations: {
            deleteMany: {},
            create: buildDimRows(dto.locationIds, 'locationId'),
          },
        }
      : {}),
    ...(Object.keys(profilePatch).length > 0
      ? { schoolProfile: { update: profilePatch } }
      : {}),
  };
}