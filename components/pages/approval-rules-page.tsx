'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { NumInput } from '@/components/molecules/num-input';
import { Badge } from '@/components/ui/badge';
import { SimpleMasterPage } from '@/components/organisms/simple-master-page';
import { formatNumber } from '@/lib/format';
import {
  listApprovalRules,
  createApprovalRule,
  updateApprovalRule,
  deleteApprovalRule,
  bulkUpdateErpApprovalRuleStatus,
  bulkDeleteErpApprovalRules,
  type ErpApprovalRule,
  type CreateApprovalRulePayload,
} from '@/lib/api/approval-rules';
import { validateForm, type FormErrors } from '@/lib/form-validation';

interface ApprovalRuleForm {
  id?: string;
  documentType: string;
  name: string;
  level: number;
  requiresApproval: boolean;
  minAmount: string;
  notes: string;
  isActive: boolean;
}

const defaultForm = (): ApprovalRuleForm => ({
  documentType: '',
  name: '',
  level: 1,
  requiresApproval: true,
  minAmount: '',
  notes: '',
  isActive: true,
});

const fromRecord = (r: ErpApprovalRule): ApprovalRuleForm => ({
  id: r.id,
  documentType: r.documentType,
  name: r.name,
  level: r.level,
  requiresApproval: r.requiresApproval,
  minAmount: r.minAmount ?? '',
  notes: r.notes ?? '',
  isActive: r.isActive,
});

const toPayload = ({ id: _id, ...f }: ApprovalRuleForm): CreateApprovalRulePayload => ({
  documentType: f.documentType,
  name: f.name,
  level: f.level,
  requiresApproval: f.requiresApproval,
  minAmount: f.minAmount || undefined,
  notes: f.notes || undefined,
  isActive: f.isActive,
});

const validateApprovalRule = (form: ApprovalRuleForm) =>
  validateForm(form, [
    { field: 'documentType', label: 'Jenis Dokumen', required: true },
    { field: 'name', label: 'Nama Aturan', required: true },
  ]);

function FormFields({
  data,
  onChange,
  errors = {},
}: {
  data: ApprovalRuleForm;
  onChange: (d: ApprovalRuleForm) => void;
  errors?: FormErrors<ApprovalRuleForm>;
}) {
  const set = (k: keyof ApprovalRuleForm, v: string | boolean | number) =>
    onChange({ ...data, [k]: v });
  return (
    <div className="p-4">
      <FormField label="Jenis Dokumen" htmlFor="apv-doctype" required error={errors.documentType}>
        <Input
          id="apv-doctype"
          value={data.documentType}
          onChange={(e) => set('documentType', e.target.value)}
          placeholder="PUR.PO"
          aria-invalid={!!errors.documentType}
        />
      </FormField>
      <FormField label="Nama Aturan" htmlFor="apv-name" required error={errors.name}>
        <Input
          id="apv-name"
          value={data.name}
          onChange={(e) => set('name', e.target.value)}
          placeholder="Persetujuan PO di atas 10 juta"
          aria-invalid={!!errors.name}
        />
      </FormField>
      <FormField label="Level" htmlFor="apv-level">
        <NumInput
          id="apv-level"
          decimals={0}
          value={String(data.level ?? '')}
          onChange={(raw) => set('level', raw === '' ? 1 : Number(raw))}
        />
      </FormField>
      <FormField label="Perlu Persetujuan" htmlFor="apv-requires">
        <BooleanRadio
          id="apv-requires"
          value={data.requiresApproval}
          onValueChange={(v) => set('requiresApproval', v)}
          trueLabel="Ya"
          falseLabel="Tidak"
        />
      </FormField>
      <FormField label="Nilai Minimum (threshold)" htmlFor="apv-minamount">
        <NumInput
          id="apv-minamount"
          value={data.minAmount}
          onChange={(raw) => set('minAmount', raw)}
        />
      </FormField>
      <FormField label="Catatan" htmlFor="apv-notes">
        <Input
          id="apv-notes"
          value={data.notes}
          onChange={(e) => set('notes', e.target.value)}
        />
      </FormField>
      <FormField label="Status" htmlFor="apv-active">
        <BooleanRadio id="apv-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>
    </div>
  );
}

export function ErpApprovalRulesPage() {
  return (
    <SimpleMasterPage<ErpApprovalRule, ApprovalRuleForm>
      title="Pengaturan Persetujuan"
      code="APV"
      entityLabel="aturan persetujuan"
      storageKey="approval-rules"
      auditEntityName="ErpApprovalRule"
      list={listApprovalRules}
      create={createApprovalRule}
      update={updateApprovalRule}
      remove={deleteApprovalRule}
      bulkStatus={bulkUpdateErpApprovalRuleStatus}
      bulkDelete={bulkDeleteErpApprovalRules}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={FormFields}
      validate={validateApprovalRule}
      extraColumns={[
        {
          key: 'level',
          label: 'Level',
          render: (row) => <span style={{ display: 'block', textAlign: 'right' }}>{row.level}</span>,
        },
        {
          key: 'requiresApproval',
          label: 'Wajib?',
          render: (row) => (
            <Badge variant={row.requiresApproval ? 'success' : 'default'}>
              {row.requiresApproval ? 'Ya' : 'Tidak'}
            </Badge>
          ),
        },
        {
          key: 'minAmount',
          label: 'Threshold',
          render: (row) => (
            <span style={{ display: 'block', textAlign: 'right' }}>
              {row.minAmount != null ? formatNumber(row.minAmount) : '—'}
            </span>
          ),
        },
      ]}
    />
  );
}
