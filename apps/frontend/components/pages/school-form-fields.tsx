'use client';

/**
 * A1 CRM Sekolah — form fields untuk SimpleMasterPage (kontrak
 * { data, onChange, errors }). Section kontak & aktivitas ada di
 * school-form-sections.tsx.
 * Atomic tier: Molecule (dipakai schools-page).
 */

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { DateInput } from '@/components/ui/date-input';
import { NumInput } from '@/components/molecules/num-input';
import { BooleanRadio } from '@/components/ui/radio-group';
import type { FormErrors } from '@/lib/form-validation';
import type { ErpSchoolActivity } from '@/lib/api/schools';
import {
  EnumSelect,
  SchoolContactsFields,
  SchoolActivityFields,
  type SchoolContactForm,
  type SchoolNewActivityForm,
} from './school-form-sections';

export type { SchoolContactForm, SchoolNewActivityForm };

export const JENJANG_OPTIONS = [
  { value: 'PAUD', label: 'PAUD' },
  { value: 'TK', label: 'TK' },
  { value: 'SD', label: 'SD' },
  { value: 'SMP', label: 'SMP' },
  { value: 'SMA', label: 'SMA' },
  { value: 'SMK', label: 'SMK' },
  { value: 'SLB', label: 'SLB' },
  { value: 'OTHER', label: 'Lainnya' },
];

export const NEGERI_SWASTA_OPTIONS = [
  { value: 'NEGERI', label: 'Negeri' },
  { value: 'SWASTA', label: 'Swasta' },
];

export const BOS_STAGE_OPTIONS = [
  { value: 'TAHAP_1', label: 'Tahap 1' },
  { value: 'TAHAP_2', label: 'Tahap 2' },
];

export const PIPELINE_OPTIONS = [
  { value: 'PROSPEK', label: 'Prospek' },
  { value: 'PENAWARAN', label: 'Penawaran' },
  { value: 'PESANAN', label: 'Pesanan' },
  { value: 'TERKIRIM', label: 'Terkirim' },
  { value: 'LUNAS', label: 'Lunas' },
];

export const PIPELINE_LABELS: Record<string, string> = Object.fromEntries(
  PIPELINE_OPTIONS.map((o) => [o.value, o.label]),
);

/** Kelas per jenjang untuk input jumlah siswa per kelas (grade). */
export function gradesForJenjang(jenjang: string): string[] {
  switch (jenjang) {
    case 'SD':
    case 'SLB':
      return ['1', '2', '3', '4', '5', '6'];
    case 'SMP':
      return ['7', '8', '9'];
    case 'SMA':
    case 'SMK':
      return ['10', '11', '12'];
    default:
      return [];
  }
}

export interface SchoolFormData {
  code: string;
  name: string;
  taxNumber: string;
  npsn: string;
  jenjang: string;
  negeriSwasta: string;
  accreditation: string;
  bosPagu: string;
  bosRealisasi: string;
  bosPeriodLabel: string;
  bosPeriodStage: string;
  bosPeriodYear: string;
  pipelineStage: string;
  contractExpiryAt: string;
  studentCount: string;
  classCount: string;
  studentsPerGrade: Record<string, string>;
  lastVisitAt: string;
  visitNotes: string;
  negotiationNotes: string;
  isActive: boolean;
  contacts: SchoolContactForm[];
  activities: ErpSchoolActivity[];
  newActivity: SchoolNewActivityForm;
}

function SectionTitle({ children }: { children: React.ReactNode }) {
  return (
    <div className="col-span-full pt-3 text-xs font-semibold uppercase tracking-wide text-[var(--fg-subtle)]">
      {children}
    </div>
  );
}

export function SchoolFormFields({
  data,
  onChange,
  errors = {},
}: {
  data: SchoolFormData;
  onChange: (d: SchoolFormData) => void;
  errors?: FormErrors<SchoolFormData>;
}) {
  const set = <K extends keyof SchoolFormData>(k: K, v: SchoolFormData[K]) =>
    onChange({ ...data, [k]: v });

  const grades = gradesForJenjang(data.jenjang);
  const perGradeSum = grades.reduce((acc, g) => acc + (Number(data.studentsPerGrade[g]) || 0), 0);

  return (
    <div className="p-4">
      <SectionTitle>Identitas Sekolah</SectionTitle>
      <div className="grid grid-cols-2 gap-x-4">
        <FormField label="Kode" htmlFor="sf-code" required error={errors.code}>
          <Input id="sf-code" value={data.code} onChange={(e) => set('code', e.target.value)} placeholder="SCH-001" aria-invalid={!!errors.code} />
        </FormField>
        <FormField label="NPSN" htmlFor="sf-npsn" error={errors.npsn} help="Nomor Pokok Sekolah Nasional (8 digit)">
          <Input id="sf-npsn" value={data.npsn} onChange={(e) => set('npsn', e.target.value)} placeholder="20104567" aria-invalid={!!errors.npsn} />
        </FormField>
      </div>
      <FormField label="Nama Sekolah" htmlFor="sf-name" required error={errors.name}>
        <Input id="sf-name" value={data.name} onChange={(e) => set('name', e.target.value)} placeholder="SDN 01 Pagi" aria-invalid={!!errors.name} />
      </FormField>
      <div className="grid grid-cols-2 gap-x-4">
        <FormField label="Jenjang" htmlFor="sf-jenjang">
          <EnumSelect id="sf-jenjang" value={data.jenjang} onChange={(v) => set('jenjang', v)} options={JENJANG_OPTIONS} />
        </FormField>
        <FormField label="Status" htmlFor="sf-negeri">
          <EnumSelect id="sf-negeri" value={data.negeriSwasta} onChange={(v) => set('negeriSwasta', v)} options={NEGERI_SWASTA_OPTIONS} />
        </FormField>
        <FormField label="Akreditasi" htmlFor="sf-akreditasi">
          <Input id="sf-akreditasi" value={data.accreditation} onChange={(e) => set('accreditation', e.target.value)} placeholder="A / B / C" />
        </FormField>
        <FormField label="NPWP" htmlFor="sf-npwp">
          <Input id="sf-npwp" value={data.taxNumber} onChange={(e) => set('taxNumber', e.target.value)} placeholder="NPWP sekolah / yayasan" />
        </FormField>
      </div>
      <FormField label="Status Data" htmlFor="sf-active">
        <BooleanRadio id="sf-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>

      <SectionTitle>Dana BOS & Pipeline</SectionTitle>
      <div className="grid grid-cols-2 gap-x-4">
        <FormField label="Pagu BOS" htmlFor="sf-pagu" help="Estimasi pagu BOS tahun berjalan (Rp)">
          <NumInput id="sf-pagu" value={data.bosPagu} onChange={(v) => set('bosPagu', v)} decimals={0} placeholder="0" />
        </FormField>
        <FormField label="Realisasi BOS" htmlFor="sf-realisasi" help="Realisasi belanja BOS (Rp)">
          <NumInput id="sf-realisasi" value={data.bosRealisasi} onChange={(v) => set('bosRealisasi', v)} decimals={0} placeholder="0" />
        </FormField>
        <FormField label="Periode BOS" htmlFor="sf-bos-label">
          <Input id="sf-bos-label" value={data.bosPeriodLabel} onChange={(e) => set('bosPeriodLabel', e.target.value)} placeholder="cth. BOS 2026" />
        </FormField>
        <FormField label="Tahap BOS" htmlFor="sf-bos-stage">
          <EnumSelect id="sf-bos-stage" value={data.bosPeriodStage} onChange={(v) => set('bosPeriodStage', v)} options={BOS_STAGE_OPTIONS} />
        </FormField>
        <FormField label="Tahun BOS" htmlFor="sf-bos-year">
          <NumInput id="sf-bos-year" value={data.bosPeriodYear} onChange={(v) => set('bosPeriodYear', v)} decimals={0} placeholder="2026" />
        </FormField>
        <FormField label="Tahap Pipeline" htmlFor="sf-pipeline" help="Tahap manual; tahap dari dokumen (pesanan/terkirim/lunas) tampil di detail">
          <EnumSelect id="sf-pipeline" value={data.pipelineStage} onChange={(v) => set('pipelineStage', v)} options={PIPELINE_OPTIONS} />
        </FormField>
        <FormField label="Kontrak s.d." htmlFor="sf-contract" help="Tanggal berakhir kontrak harga (peringatan ≤ 90 hari)">
          <DateInput id="sf-contract" value={data.contractExpiryAt} onChange={(iso) => set('contractExpiryAt', iso)} />
        </FormField>
      </div>

      <SectionTitle>Siswa</SectionTitle>
      <div className="grid grid-cols-2 gap-x-4">
        <FormField label="Jumlah Siswa" htmlFor="sf-students" help={grades.length > 0 && perGradeSum > 0 ? `Otomatis dari per-kelas: ${perGradeSum}` : 'Total siswa'}>
          <NumInput id="sf-students" value={data.studentCount} onChange={(v) => set('studentCount', v)} decimals={0} placeholder="0" />
        </FormField>
        <FormField label="Jumlah Kelas" htmlFor="sf-classes" help="Rombongan belajar">
          <NumInput id="sf-classes" value={data.classCount} onChange={(v) => set('classCount', v)} decimals={0} placeholder="0" />
        </FormField>
      </div>
      {grades.length > 0 && (
        <div className="grid grid-cols-3 gap-x-4">
          {grades.map((g) => (
            <FormField key={g} label={`Kelas ${g}`} htmlFor={`sf-grade-${g}`}>
              <NumInput
                id={`sf-grade-${g}`}
                value={data.studentsPerGrade[g] ?? ''}
                onChange={(v) =>
                  set('studentsPerGrade', { ...data.studentsPerGrade, [g]: v })
                }
                decimals={0}
                placeholder="0"
              />
            </FormField>
          ))}
        </div>
      )}

      <SectionTitle>Kontak Sekolah</SectionTitle>
      <SchoolContactsFields contacts={data.contacts} onChange={(c) => set('contacts', c)} />

      <SectionTitle>Aktivitas & Catatan</SectionTitle>
      <SchoolActivityFields
        activities={data.activities}
        draft={data.newActivity}
        onDraftChange={(d) => set('newActivity', d)}
      />
      <div className="grid grid-cols-2 gap-x-4 pt-2">
        <FormField label="Kunjungan Terakhir" htmlFor="sf-lastvisit" help="Terisi otomatis dari aktivitas Kunjungan terbaru">
          <DateInput id="sf-lastvisit" value={data.lastVisitAt} onChange={(iso) => set('lastVisitAt', iso)} />
        </FormField>
      </div>
      <FormField label="Catatan Kunjungan" htmlFor="sf-visit-notes">
        <textarea id="sf-visit-notes" className="input w-full" rows={2} value={data.visitNotes} onChange={(e) => set('visitNotes', e.target.value)} placeholder="Ringkasan kunjungan terakhir" />
      </FormField>
      <FormField label="Catatan Negosiasi" htmlFor="sf-nego-notes">
        <textarea id="sf-nego-notes" className="input w-full" rows={2} value={data.negotiationNotes} onChange={(e) => set('negotiationNotes', e.target.value)} placeholder="Poin negosiasi / komitmen harga" />
      </FormField>
    </div>
  );
}
