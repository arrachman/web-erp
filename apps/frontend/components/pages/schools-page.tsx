'use client';

/**
 * A1 CRM — Master Sekolah (profil berbasis NPSN di atas partner SCHOOL).
 * Memakai SimpleMasterPage organism (§2.7 standard list): server-driven
 * pagination/search/sort, filter jenjang/status BOS/pipeline/alert,
 * bulk actions, audit panel.
 * Atomic tier: Page.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { SimpleMasterPage, type ExtraColumn } from '@/components/organisms/simple-master-page';
import {
  listSchools,
  createSchool,
  updateSchool,
  deleteSchool,
  bulkUpdateSchoolStatus,
  bulkDeleteSchools,
  type ErpSchool,
  type CreateSchoolPayload,
  type ErpPartnerContactRole,
  type ErpSchoolActivityType,
} from '@/lib/api/schools';
import { formatRupiah } from '@/lib/format';
import { validateForm } from '@/lib/form-validation';
import {
  SchoolFormFields,
  JENJANG_OPTIONS,
  NEGERI_SWASTA_OPTIONS,
  BOS_STAGE_OPTIONS,
  PIPELINE_OPTIONS,
  PIPELINE_LABELS,
  gradesForJenjang,
  type SchoolFormData,
} from './school-form-fields';

// ─── Form mapping ─────────────────────────────────────────────────────────────

const toDateInput = (iso?: string | null): string => (iso ? iso.slice(0, 10) : '');

const defaultForm = (): SchoolFormData => ({
  code: '',
  name: '',
  taxNumber: '',
  npsn: '',
  jenjang: '',
  negeriSwasta: '',
  accreditation: '',
  bosPagu: '',
  bosRealisasi: '',
  bosPeriodLabel: '',
  bosPeriodStage: '',
  bosPeriodYear: '',
  pipelineStage: 'PROSPEK',
  contractExpiryAt: '',
  studentCount: '',
  classCount: '',
  studentsPerGrade: {},
  lastVisitAt: '',
  visitNotes: '',
  negotiationNotes: '',
  isActive: true,
  contacts: [],
  activities: [],
  newActivity: { type: '', activityAt: '', notes: '' },
});

const fromRecord = (row: ErpSchool): SchoolFormData => {
  const p = row.schoolProfile;
  return {
    code: row.code,
    name: row.name,
    taxNumber: row.taxNumber ?? '',
    npsn: p?.npsn ?? '',
    jenjang: p?.jenjang ?? '',
    negeriSwasta: p?.negeriSwasta ?? '',
    accreditation: p?.accreditation ?? '',
    bosPagu: p?.bosPagu != null ? String(Math.trunc(Number(p.bosPagu))) : '',
    bosRealisasi: p?.bosRealisasi != null ? String(Math.trunc(Number(p.bosRealisasi))) : '',
    bosPeriodLabel: p?.bosPeriodLabel ?? '',
    bosPeriodStage: p?.bosPeriodStage ?? '',
    bosPeriodYear: p?.bosPeriodYear != null ? String(p.bosPeriodYear) : '',
    pipelineStage: p?.pipelineStage ?? 'PROSPEK',
    contractExpiryAt: toDateInput(p?.contractExpiryAt),
    studentCount: p?.studentCount != null ? String(p.studentCount) : '',
    classCount: p?.classCount != null ? String(p.classCount) : '',
    studentsPerGrade: Object.fromEntries(
      Object.entries(p?.studentsPerGrade ?? {}).map(([k, v]) => [k, String(v)]),
    ),
    lastVisitAt: toDateInput(p?.lastVisitAt),
    visitNotes: p?.visitNotes ?? '',
    negotiationNotes: p?.negotiationNotes ?? '',
    isActive: row.isActive,
    contacts: (row.contacts ?? []).map((c) => ({
      id: c.id,
      name: c.name,
      role: c.role ?? '',
      title: c.title ?? '',
      phone: c.phone ?? '',
      email: c.email ?? '',
      isDefault: c.isDefault,
    })),
    activities: row.schoolActivities ?? [],
    newActivity: { type: '', activityAt: '', notes: '' },
  };
};

const numOrUndefined = (v: string): number | undefined => {
  if (v.trim() === '') return undefined;
  const n = Number(v);
  return Number.isFinite(n) ? Math.trunc(n) : undefined;
};

const strOrUndefined = (v: string): string | undefined => (v.trim() === '' ? undefined : v.trim());

const toPayload = (f: SchoolFormData): CreateSchoolPayload => {
  const perGrade: Record<string, number> = {};
  for (const [g, v] of Object.entries(f.studentsPerGrade)) {
    const n = numOrUndefined(v);
    if (n !== undefined) perGrade[g] = n;
  }
  const grades = gradesForJenjang(f.jenjang);
  const perGradeSum = grades.reduce((acc, g) => acc + (perGrade[g] ?? 0), 0);
  const hasPerGrade = grades.some((g) => perGrade[g] !== undefined);

  const contacts = f.contacts
    .filter((c) => c.name.trim() !== '')
    .map((c) => ({
      id: c.id,
      name: c.name.trim(),
      role: (strOrUndefined(c.role) as ErpPartnerContactRole | undefined) ?? undefined,
      title: strOrUndefined(c.title),
      phone: strOrUndefined(c.phone),
      email: strOrUndefined(c.email),
      isDefault: c.isDefault,
    }));

  const newActivity = f.newActivity.notes.trim()
    ? {
        type: ((strOrUndefined(f.newActivity.type) as ErpSchoolActivityType | undefined) ?? 'NOTE') as ErpSchoolActivityType,
        activityAt: strOrUndefined(f.newActivity.activityAt),
        notes: f.newActivity.notes.trim(),
      }
    : undefined;

  return {
    code: f.code.trim(),
    name: f.name.trim(),
    taxNumber: strOrUndefined(f.taxNumber),
    isActive: f.isActive,
    npsn: strOrUndefined(f.npsn),
    jenjang: (strOrUndefined(f.jenjang) as CreateSchoolPayload['jenjang']) ?? undefined,
    negeriSwasta: (strOrUndefined(f.negeriSwasta) as CreateSchoolPayload['negeriSwasta']) ?? undefined,
    accreditation: strOrUndefined(f.accreditation),
    bosPagu: numOrUndefined(f.bosPagu),
    bosRealisasi: numOrUndefined(f.bosRealisasi),
    bosPeriodLabel: strOrUndefined(f.bosPeriodLabel),
    bosPeriodStage: (strOrUndefined(f.bosPeriodStage) as CreateSchoolPayload['bosPeriodStage']) ?? undefined,
    bosPeriodYear: numOrUndefined(f.bosPeriodYear),
    pipelineStage: (strOrUndefined(f.pipelineStage) as CreateSchoolPayload['pipelineStage']) ?? undefined,
    contractExpiryAt: strOrUndefined(f.contractExpiryAt),
    studentCount: hasPerGrade ? perGradeSum : numOrUndefined(f.studentCount),
    classCount: numOrUndefined(f.classCount),
    studentsPerGrade: perGrade,
    lastVisitAt: strOrUndefined(f.lastVisitAt),
    visitNotes: strOrUndefined(f.visitNotes),
    negotiationNotes: strOrUndefined(f.negotiationNotes),
    contacts,
    newActivity,
  };
};

const validateSchool = (form: SchoolFormData) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama Sekolah', required: true },
    {
      field: 'npsn',
      label: 'NPSN',
      validate: (_v, form) =>
        form.npsn && !/^\d{8}$/.test(form.npsn.trim()) ? 'NPSN harus 8 digit angka' : undefined,
    },
  ]);

// ─── List columns ─────────────────────────────────────────────────────────────

function contractBadge(row: ErpSchool): React.ReactNode {
  const exp = row.schoolProfile?.contractExpiryAt;
  if (!exp) return null;
  const days = Math.ceil((new Date(exp).getTime() - Date.now()) / 86_400_000);
  if (days < 0) return <Badge variant="danger">Kontrak berakhir</Badge>;
  if (days <= 90) return <Badge variant="info">Kontrak {days} hari lagi</Badge>;
  return null;
}

const extraColumns: ExtraColumn<ErpSchool>[] = [
  { key: 'npsn', label: 'NPSN', sortable: true, render: (r) => r.schoolProfile?.npsn ?? '—' },
  { key: 'jenjang', label: 'Jenjang', render: (r) => r.schoolProfile?.jenjang ?? '—' },
  {
    key: 'negeriSwasta',
    label: 'Negeri/Swasta',
    render: (r) =>
      r.schoolProfile?.negeriSwasta ? (
        <Badge variant={r.schoolProfile.negeriSwasta === 'NEGERI' ? 'info' : 'default'}>
          {r.schoolProfile.negeriSwasta === 'NEGERI' ? 'Negeri' : 'Swasta'}
        </Badge>
      ) : (
        '—'
      ),
  },
  {
    key: 'wilayah',
    label: 'Wilayah',
    render: (r) => {
      const a = r.addresses?.[0];
      const parts = [a?.city?.name, a?.province?.name].filter(Boolean);
      return parts.length > 0 ? parts.join(', ') : '—';
    },
  },
  {
    key: 'siswa',
    label: 'Siswa',
    render: (r) =>
      r.schoolProfile?.studentCount != null
        ? new Intl.NumberFormat('id-ID').format(r.schoolProfile.studentCount)
        : '—',
  },
  {
    key: 'paguBos',
    label: 'Pagu BOS',
    render: (r) =>
      r.schoolProfile?.bosPagu != null ? formatRupiah(Number(r.schoolProfile.bosPagu)) : '—',
  },
  {
    key: 'pipeline',
    label: 'Pipeline',
    render: (r) => {
      const stage = r.schoolProfile?.pipelineStage ?? 'PROSPEK';
      const bosStage = r.schoolProfile?.bosPeriodStage;
      return (
        <span className="flex flex-col items-start gap-1">
          <Badge variant={stage === 'LUNAS' ? 'success' : stage === 'PROSPEK' ? 'default' : 'primary'}>
            {PIPELINE_LABELS[stage] ?? stage}
          </Badge>
          {bosStage && (
            <span className="text-[11px] text-[var(--fg-subtle)]">
              BOS {bosStage === 'TAHAP_1' ? 'Tahap 1' : 'Tahap 2'}
              {r.schoolProfile?.bosPeriodYear ? ` ${r.schoolProfile.bosPeriodYear}` : ''}
            </span>
          )}
          {contractBadge(r)}
        </span>
      );
    },
  },
];

const currentYear = new Date().getFullYear();
const extraFilters = [
  { key: 'jenjang', label: 'Jenjang', options: JENJANG_OPTIONS },
  { key: 'negeriSwasta', label: 'Negeri/Swasta', options: NEGERI_SWASTA_OPTIONS },
  { key: 'bosStage', label: 'Tahap BOS', options: BOS_STAGE_OPTIONS },
  {
    key: 'bosYear',
    label: 'Tahun BOS',
    options: [currentYear - 1, currentYear, currentYear + 1].map((y) => ({
      label: String(y),
      value: String(y),
    })),
  },
  { key: 'pipelineStage', label: 'Pipeline', options: PIPELINE_OPTIONS },
  {
    key: 'alert',
    label: 'Peringatan',
    options: [
      { label: 'Belum belanja (tahun BOS)', value: 'NOT_ORDERED' },
      { label: 'Kontrak akan berakhir', value: 'CONTRACT_EXPIRING' },
    ],
  },
];

export function ErpSchoolsPage() {
  return (
    <SimpleMasterPage<ErpSchool, SchoolFormData>
      title="Sekolah"
      code="SCH"
      entityLabel="sekolah"
      storageKey="erp-schools"
      auditEntityName="erp_schools"
      list={listSchools}
      create={createSchool}
      update={updateSchool}
      remove={deleteSchool}
      bulkStatus={bulkUpdateSchoolStatus}
      bulkDelete={bulkDeleteSchools}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={SchoolFormFields}
      validate={validateSchool}
      extraColumns={extraColumns}
      extraFilters={extraFilters}
      modalSize="lg"
    />
  );
}
