'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { SearchSelect } from '@/components/molecules/search-select';
import { SimpleMasterPage, type ExtraColumn } from '@/components/organisms/simple-master-page';
import {
  listSubDepartments, createSubDepartment, updateSubDepartment, deleteSubDepartment,
  bulkUpdateSubDepartmentStatus, bulkDeleteSubDepartments,
  type ErpSubDepartment, type CreateSubDepartmentPayload,
} from '@/lib/api/sub-departments';
import { listDepartments } from '@/lib/api/departments';
import { validateForm, type FormErrors } from '@/lib/form-validation';

const loadDepartmentOptions = async (search: string, page: number, limit: number) => {
  const res = await listDepartments({ page, limit, search: search || undefined, isActive: true });
  return { data: res.data.map((d) => ({ value: d.id, label: d.name, code: d.code })), total: res.meta.total };
};

interface SubDepartmentForm {
  code: string;
  name: string;
  departmentId: string;
  departmentLabel?: string;
  isActive: boolean;
}

const defaultForm = (): SubDepartmentForm => ({ code: '', name: '', departmentId: '', isActive: true });

const fromRecord = (r: ErpSubDepartment): SubDepartmentForm => ({
  code: r.code, name: r.name, departmentId: r.departmentId,
  departmentLabel: r.department ? (r.department.code + ' — ' + r.department.name) : '',
  isActive: r.isActive,
});

const toPayload = (f: SubDepartmentForm): CreateSubDepartmentPayload => ({
  code: f.code, name: f.name, departmentId: f.departmentId, isActive: f.isActive,
});

const validateSubDepartment = (form: SubDepartmentForm) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama', required: true },
    { field: 'departmentId', label: 'Department', required: true },
  ]);

function SubDepartmentFormFields({ data, onChange, errors = {} }: { data: SubDepartmentForm; onChange: (d: SubDepartmentForm) => void; errors?: FormErrors<SubDepartmentForm> }) {
  const set = <K extends keyof SubDepartmentForm>(k: K, v: SubDepartmentForm[K]) =>
    onChange({ ...data, [k]: v });
  return (
    <div className="p-4">
      <FormField label="Kode" htmlFor="sp-code" required error={errors.code}>
        <Input id="sp-code" value={data.code} onChange={(e) => set('code', e.target.value)} placeholder="SUB-DEPT-A" aria-invalid={!!errors.code} />
      </FormField>
      <FormField label="Nama" htmlFor="sp-name" required error={errors.name}>
        <Input id="sp-name" value={data.name} onChange={(e) => set('name', e.target.value)} placeholder="Sub Department A" aria-invalid={!!errors.name} />
      </FormField>
      <FormField label="Department" htmlFor="sp-dept" required error={errors.departmentId}>
        <SearchSelect
          id="sp-dept"
          value={data.departmentId}
          onValueChange={(v) => set('departmentId', v)}
          placeholder="Cari department…"
          loadOptions={loadDepartmentOptions}
          initialLabel={data.departmentLabel}
          title="Department"
          error={!!errors.departmentId}
        />
      </FormField>
      <FormField label="Status" htmlFor="sp-active">
        <BooleanRadio id="sp-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>
    </div>
  );
}

const extraColumns: ExtraColumn<ErpSubDepartment>[] = [
  { key: 'department', label: 'Department', render: (r) => r.department ? `${r.department.code} — ${r.department.name}` : '—' },
];

export function ErpSubDepartmentsPage() {
  return (
    <SimpleMasterPage<ErpSubDepartment, SubDepartmentForm>
      title="Sub Department" code="SDEPT" entityLabel="sub departemen"
      storageKey="sub-departments" auditEntityName="ErpSubDepartment"
      list={listSubDepartments} create={createSubDepartment} update={updateSubDepartment} remove={deleteSubDepartment}
      bulkStatus={bulkUpdateSubDepartmentStatus} bulkDelete={bulkDeleteSubDepartments}
      defaultForm={defaultForm} fromRecord={fromRecord} toPayload={toPayload}
      FormFields={SubDepartmentFormFields}
      extraColumns={extraColumns}
      validate={validateSubDepartment}
    />
  );
}
