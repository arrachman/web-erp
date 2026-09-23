'use client';

import * as React from 'react';
import { SimpleMasterPage } from '@/components/organisms/simple-master-page';
import type { ExtraColumn } from '@/components/organisms/simple-master-page';
import {
  listBranches,
  createBranch,
  updateBranch,
  deleteBranch,
  bulkUpdateBranchStatus,
  bulkDeleteBranches,
} from '@/lib/api/branches';
import type { ErpBranch } from '@/lib/api/branches';
import {
  BranchFormFields,
  defaultBranchForm,
  branchFromRecord,
  branchToPayload,
  validateBranch,
  type BranchForm,
} from './branches-form';

const extraColumns: ExtraColumn<ErpBranch>[] = [
  { key: 'city', label: 'Kota', sortable: true, render: (r) => r.city ?? '—' },
  { key: 'phone', label: 'Telepon', sortable: true, render: (r) => r.phone ?? '—' },
];

export function ErpBranchesPage() {
  return (
    <SimpleMasterPage<ErpBranch, BranchForm>
      title="Branch"
      code="BRN"
      entityLabel="cabang"
      storageKey="branches"
      auditEntityName="ErpBranch"
      list={listBranches}
      create={createBranch}
      update={updateBranch}
      remove={deleteBranch}
      bulkStatus={bulkUpdateBranchStatus}
      bulkDelete={bulkDeleteBranches}
      defaultForm={defaultBranchForm}
      fromRecord={branchFromRecord}
      toPayload={branchToPayload}
      FormFields={BranchFormFields}
      validate={validateBranch}
      extraColumns={extraColumns}
      defaultSortBy="code"
      defaultSortDir="asc"
    />
  );
}
