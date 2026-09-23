'use client';

/**
 * F3 Master Data — Partner page (Customer / Supplier).
 * Lists md_partners; supports create, edit, delete, bulk actions.
 * Atomic tier: Page.
 */

import { SimpleMasterPage, type ExtraColumn } from '@/components/organisms/simple-master-page';
import { PARTNER_TYPE_KINDS, PARTNER_TYPE_KIND_LABEL } from '@/lib/api/partner-types';
import {
  listPartners,
  createPartner,
  updatePartner,
  deletePartner,
  bulkUpdatePartnerStatus,
  bulkDeletePartners,
  type ErpPartner,
} from '@/lib/api/partners';
import { PartnerFormFields } from './partners-form-fields';
import {
  defaultForm,
  fromRecord,
  toPayload,
  validatePartner,
  partnerTypeLabel,
  type PartnerForm,
} from './partners-form-types';

// ─── Extra columns ────────────────────────────────────────────────────────────

const extraColumns: ExtraColumn<ErpPartner>[] = [
  { key: 'partnerType', label: 'Tipe', render: (r) => partnerTypeLabel(r) },
];

// ─── Type filter extras ───────────────────────────────────────────────────────

const TYPE_FILTER_EXTRAS = [
  {
    key: 'typeKind',
    label: 'Tipe',
    defaultValue: '',
    options: PARTNER_TYPE_KINDS.map((kind) => ({
      label: PARTNER_TYPE_KIND_LABEL[kind],
      value: kind,
    })),
  },
];

// ─── Page ─────────────────────────────────────────────────────────────────────

export function ErpPartnersPage() {
  return (
    <SimpleMasterPage<ErpPartner, PartnerForm>
      title="Partner"
      code="PTR"
      entityLabel="partner"
      storageKey="partners"
      auditEntityName="ErpPartner"
      list={listPartners}
      create={createPartner}
      update={updatePartner}
      remove={deletePartner}
      bulkStatus={bulkUpdatePartnerStatus}
      bulkDelete={bulkDeletePartners}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={PartnerFormFields}
      validate={validatePartner}
      modalSize="lg"
      extraColumns={extraColumns}
      defaultSortBy="code"
      defaultSortDir="asc"
      extraFilters={TYPE_FILTER_EXTRAS}
    />
  );
}
