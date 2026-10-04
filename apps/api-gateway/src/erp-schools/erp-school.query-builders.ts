import { Prisma } from '@prisma/client';
import { QueryErpSchoolDto } from './dto/query-erp-school.dto';

/**
 * School list/detail relation includes.
 *
 * A school IS a partner, so we reuse the partner relation include shape but
 * narrow it to what the CRM list/detail actually needs (no supplier/category
 * plumbing beyond partner type + dimensions). `schoolProfile` is always
 * included — it is the whole point of the CRM entity.
 */
export const SCHOOL_RELATION_INCLUDE: Prisma.ErpPartnerInclude = {
  partnerType: { select: { id: true, code: true, name: true, kind: true } },
  salesman: { select: { id: true, code: true, name: true } },
  schoolProfile: {
    select: {
      id: true,
      npsn: true,
      jenjang: true,
      negeriSwasta: true,
      accreditation: true,
      studentCount: true,
      classCount: true,
      studentsPerGrade: true,
      pipelineStage: true,
      bosPagu: true,
      bosRealisasi: true,
      bosPeriodLabel: true,
      bosPeriodStage: true,
      bosPeriodYear: true,
      lastVisitAt: true,
      visitNotes: true,
      negotiationNotes: true,
      contractExpiryAt: true,
      isActive: true,
    },
  },
  // Contacts (with standardised CRM role) and the latest activities are
  // part of the list payload: the master-page edit form is fed from the
  // list row, so both must travel with it.
  contacts: {
    where: { deletedAt: null },
    orderBy: { createdAt: 'asc' as const },
    select: {
      id: true,
      name: true,
      role: true,
      title: true,
      phone: true,
      email: true,
      isDefault: true,
    },
  },
  schoolActivities: {
    where: { deletedAt: null },
    orderBy: { activityAt: 'desc' as const },
    take: 10,
    select: {
      id: true,
      type: true,
      activityAt: true,
      notes: true,
      contactId: true,
      contact: { select: { id: true, name: true } },
    },
  },
  // Primary address → wilayah (city / province) shown on the CRM list.
  addresses: {
    where: { deletedAt: null },
    orderBy: [{ isDefault: 'desc' as const }, { createdAt: 'asc' as const }],
    take: 1,
    select: {
      id: true,
      addressLine1: true,
      city: { select: { id: true, name: true } },
      province: { select: { id: true, name: true } },
    },
  },
};

export const SCHOOL_DIM_INCLUDE: Prisma.ErpPartnerInclude = {
  dimBranches: {
    select: { branchId: true, branch: { select: { id: true, code: true, name: true } } },
    orderBy: { id: 'asc' as const },
  },
  dimWarehouses: {
    select: { warehouseId: true, warehouse: { select: { id: true, code: true, name: true } } },
    orderBy: { id: 'asc' as const },
  },
  dimLocations: {
    select: { locationId: true, location: { select: { id: true, code: true, name: true } } },
    orderBy: { id: 'asc' as const },
  },
};

export const SCHOOL_LIST_INCLUDE: Prisma.ErpPartnerInclude = {
  ...SCHOOL_RELATION_INCLUDE,
  ...SCHOOL_DIM_INCLUDE,
};

export const SCHOOL_DETAIL_INCLUDE: Prisma.ErpPartnerInclude = {
  ...SCHOOL_RELATION_INCLUDE,
  addresses: {
    where: { deletedAt: null },
    orderBy: { createdAt: 'asc' },
    include: {
      country: { select: { id: true, name: true } },
      province: { select: { id: true, name: true } },
      city: { select: { id: true, name: true } },
      area: { select: { id: true, name: true, postalCode: true } },
      subArea: { select: { id: true, name: true, postalCode: true } },
    },
  },
  contacts: { where: { deletedAt: null }, orderBy: { createdAt: 'asc' } },
  bankAccounts: { where: { deletedAt: null }, orderBy: { createdAt: 'asc' } },
  ...SCHOOL_DIM_INCLUDE,
};

/**
 * Build the Prisma `where` clause for findAll from the query DTO.
 *
 * A school is a partner whose partner_type_id = the SCHOOL type. The query
 * DTO exposes the vertical filters (jenjang / bosStage / bosYear) which map
 * onto the 1:1 profile, so they are joined through the schoolProfile relation.
 */
export function buildErpSchoolWhere(
  query: QueryErpSchoolDto,
): Prisma.ErpPartnerWhereInput {
  const base: Prisma.ErpPartnerWhereInput = {
    deletedAt: null,
    partnerType: { is: { code: 'SCHOOL', deletedAt: null, isActive: true } },
  };

  const profileFilter: Prisma.ErpSchoolProfileWhereInput = {};
  if (query.jenjang !== undefined) profileFilter.jenjang = query.jenjang;
  if (query.negeriSwasta !== undefined) profileFilter.negeriSwasta = query.negeriSwasta;
  if (query.bosStage !== undefined) profileFilter.bosPeriodStage = query.bosStage;
  if (query.bosYear !== undefined) profileFilter.bosPeriodYear = query.bosYear;
  if (query.pipelineStage !== undefined) profileFilter.pipelineStage = query.pipelineStage;

  const conditions: Prisma.ErpPartnerWhereInput[] = [base];

  if (query.search?.trim()) {
    const q = query.search.trim();
    conditions.push({
      OR: [
        { code: { equals: q, mode: 'insensitive' } },
        { name: { contains: q, mode: 'insensitive' } },
        { taxNumber: { contains: q, mode: 'insensitive' } },
        { schoolProfile: { is: { npsn: { contains: q, mode: 'insensitive' } } } },
      ],
    });
  }

  if (Object.keys(profileFilter).length > 0) {
    conditions.push({ schoolProfile: { is: profileFilter } });
  }

  if (query.isActive !== undefined) {
    conditions.push({ isActive: query.isActive });
  }

  return conditions.length === 1 ? base : { AND: conditions };
}

/** Build the dynamic `orderBy` clause preserving `[sortBy]: sortDir` semantics. */
export function buildErpSchoolOrderBy(
  query: QueryErpSchoolDto,
): Prisma.ErpPartnerOrderByWithRelationInput[] {
  const sortBy = query.sortBy ?? 'createdAt';
  const sortDir = query.sortDir ?? 'desc';
  // NPSN and contract expiry live on the 1:1 profile, not on the partner row.
  if (sortBy === 'npsn') return [{ schoolProfile: { npsn: sortDir } }];
  if (sortBy === 'contractExpiryAt') return [{ schoolProfile: { contractExpiryAt: sortDir } }];
  return [{ [sortBy]: sortDir }];
}