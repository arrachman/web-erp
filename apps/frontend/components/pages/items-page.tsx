'use client';

/**
 * F3 Master Data — Item (produk/bahan) page.
 * Uses SimpleMasterPage organism for full-feature CRUD.
 * Atomic tier: Page.
 */

import * as React from 'react';
import { SimpleMasterPage, type ExtraColumn, type ExtraFilterDef } from '@/components/organisms/simple-master-page';
import {
  listItems, createItem, updateItem, deleteItem,
  bulkUpdateItemStatus, bulkDeleteItems,
  type ErpItem,
} from '@/lib/api/items';
import { ItemFormFields, defaultItemForm, fromItem, toItemPayload, validateItem, type ItemFormData } from './items-form';

// ─── Extra columns ─────────────────────────────────────────────────────────────

const extraColumns: ExtraColumn<ErpItem>[] = [
  {
    key: 'itemType',
    label: 'Tipe',
    render: (r) => <span className="code">{r.itemType}</span>,
  },
  {
    key: 'unit',
    label: 'Satuan',
    render: (r) => <span className="muted">{r.unit?.code ?? r.unitId}</span>,
  },
  {
    key: 'category',
    label: 'Kategori',
    render: (r) => <span className="muted">{r.category?.name ?? r.categoryId}</span>,
  },
];

// ─── Extra filters ─────────────────────────────────────────────────────────────

const itemTypeFilters: ExtraFilterDef[] = [
  { key: 'itemType', label: 'Tipe', options: [
    { label: 'Inventory', value: 'INVENTORY' },
    { label: 'Service', value: 'SERVICE' },
    { label: 'Consumable', value: 'CONSUMABLE' },
    { label: 'Asset', value: 'ASSET' },
    { label: 'Non-Inventory', value: 'NON_INVENTORY' },
  ]},
];

// ─── Page ──────────────────────────────────────────────────────────────────────

export function ErpItemsPage() {
  return (
    <SimpleMasterPage<ErpItem, ItemFormData>
      title="Item"
      code="ITM"
      entityLabel="item"
      storageKey="items"
      auditEntityName="ErpItem"
      list={listItems}
      create={createItem}
      update={updateItem}
      remove={deleteItem}
      bulkStatus={bulkUpdateItemStatus}
      bulkDelete={bulkDeleteItems}
      defaultForm={defaultItemForm}
      fromRecord={fromItem}
      toPayload={toItemPayload}
      FormFields={ItemFormFields}
      validate={validateItem}
      extraColumns={extraColumns}
      defaultSortBy="code"
      defaultSortDir="asc"
      extraFilters={itemTypeFilters}
      modalSize="xl"
    />
  );
}
