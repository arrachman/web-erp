'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { DateRangePicker } from '@/components/ui/date-range-picker';
import { SimpleMasterPage, type ExtraColumn } from '@/components/organisms/simple-master-page';
import {
  listProjects, createProject, updateProject, deleteProject,
  bulkUpdateProjectStatus, bulkDeleteProjects,
  type ErpProject, type CreateProjectPayload,
} from '@/lib/api/projects';
import { validateForm, type FormErrors } from '@/lib/form-validation';

interface ProjectForm {
  code: string;
  name: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
}

const defaultForm = (): ProjectForm => ({ code: '', name: '', startDate: '', endDate: '', isActive: true });

const fromRecord = (r: ErpProject): ProjectForm => ({
  code: r.code, name: r.name,
  startDate: r.startDate ? r.startDate.slice(0, 10) : '',
  endDate: r.endDate ? r.endDate.slice(0, 10) : '',
  isActive: r.isActive,
});

const validateProject = (form: ProjectForm) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama', required: true },
  ]);

const toPayload = (f: ProjectForm): CreateProjectPayload => ({
  code: f.code, name: f.name,
  startDate: f.startDate || undefined,
  endDate: f.endDate || undefined,
  isActive: f.isActive,
});

function ProjectFormFields({ data, onChange, errors = {} }: { data: ProjectForm; onChange: (d: ProjectForm) => void; errors?: FormErrors<ProjectForm> }) {
  const set = (k: keyof ProjectForm, v: string | boolean) => onChange({ ...data, [k]: v });
  return (
    <div className="p-4">
      <FormField label="Kode" htmlFor="pr-code" required error={errors.code}>
        <Input id="pr-code" value={data.code} onChange={(e) => set('code', e.target.value)} placeholder="PRJ-2026-A" aria-invalid={!!errors.code} />
      </FormField>
      <FormField label="Nama" htmlFor="pr-name" required error={errors.name}>
        <Input id="pr-name" value={data.name} onChange={(e) => set('name', e.target.value)} placeholder="Project Alpha 2026" aria-invalid={!!errors.name} />
      </FormField>
      <FormField label="Periode" htmlFor="pr-period">
        <DateRangePicker
          id="pr-period"
          from={data.startDate}
          to={data.endDate}
          onChangeFrom={(v) => set('startDate', v)}
          onChangeTo={(v) => set('endDate', v)}
        />
      </FormField>
      <FormField label="Status" htmlFor="pr-active">
        <BooleanRadio id="pr-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>
    </div>
  );
}

const extraColumns: ExtraColumn<ErpProject>[] = [
  { key: 'startDate', label: 'Mulai', render: (r) => r.startDate ? r.startDate.slice(0, 10) : '—' },
  { key: 'endDate', label: 'Selesai', render: (r) => r.endDate ? r.endDate.slice(0, 10) : '—' },
];

export function ErpProjectsPage() {
  return (
    <SimpleMasterPage<ErpProject, ProjectForm>
      title="Project" code="PRJ" entityLabel="project"
      storageKey="projects" auditEntityName="ErpProject"
      list={listProjects} create={createProject} update={updateProject} remove={deleteProject}
      bulkStatus={bulkUpdateProjectStatus} bulkDelete={bulkDeleteProjects}
      defaultForm={defaultForm} fromRecord={fromRecord} toPayload={toPayload}
      FormFields={ProjectFormFields}
      validate={validateProject}
      extraColumns={extraColumns}
    />
  );
}
