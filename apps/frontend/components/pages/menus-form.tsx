'use client';

/**
 * Sys Menu Manager — form fields molecule.
 * Used by menus-page.tsx (SimpleMasterPage) inside the create/edit modal.
 * Atomic tier: Organism (form section).
 */

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { SearchSelect } from '@/components/molecules/search-select';
import { validateForm, type FormErrors } from '@/lib/form-validation';
import type {
  ErpSysMenu,
  ErpMenuType,
  CreateSysMenuPayload,
} from '@/lib/api/sys-menus';
import { ERP_MENU_TYPES, listSysMenus } from '@/lib/api/sys-menus';

export interface MenuForm {
  code: string;
  title: string;
  type: ErpMenuType;
  parentId: string;
  parentLabel?: string;
  path: string;
  icon: string;
  sortOrder: number;
  isActive: boolean;
}

export const defaultMenuForm = (): MenuForm => ({
  code: '',
  title: '',
  type: 'MODULE',
  parentId: '',
  parentLabel: '',
  path: '',
  icon: '',
  sortOrder: 0,
  isActive: true,
});

export function fromMenu(m: ErpSysMenu): MenuForm {
  return {
    code: m.code,
    title: m.title,
    type: m.type,
    parentId: m.parentId ?? '',
    parentLabel: '',
    path: m.path ?? '',
    icon: m.icon ?? '',
    sortOrder: m.sortOrder,
    isActive: m.isActive,
  };
}

export function toMenuPayload(f: MenuForm): CreateSysMenuPayload {
  return {
    code: f.code,
    title: f.title,
    type: f.type,
    parentId: f.parentId || null,
    path: f.path || null,
    icon: f.icon || null,
    sortOrder: f.sortOrder,
    isActive: f.isActive,
  };
}

export const validateMenu = (form: MenuForm) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'title', label: 'Judul', required: true },
  ]);

async function loadParentMenuOptions(
  search: string,
  page: number,
  limit: number,
) {
  const res = await listSysMenus();
  const filtered = res.data.filter(
    (m) =>
      (m.type === 'MODULE' || m.type === 'GROUP') &&
      (!search ||
        m.title.toLowerCase().includes(search.toLowerCase()) ||
        m.code.toLowerCase().includes(search.toLowerCase())),
  );
  const start = (page - 1) * limit;
  const pageData = filtered.slice(start, start + limit);
  return {
    data: pageData.map((m) => ({ value: m.id, label: m.title, code: m.code })),
    total: filtered.length,
  };
}

export function MenuFormFields({
  data,
  onChange,
  errors = {},
}: {
  data: MenuForm;
  onChange: (d: MenuForm) => void;
  errors?: FormErrors<MenuForm>;
}) {
  const set = <K extends keyof MenuForm>(k: K, v: MenuForm[K]) =>
    onChange({ ...data, [k]: v });

  return (
    <div className="p-4">
      <FormField label="Kode" htmlFor="mf-code" required error={errors.code}>
        <Input
          id="mf-code"
          value={data.code}
          onChange={(e) => set('code', e.target.value)}
          placeholder="INVENTORY_ITEMS"
          aria-invalid={!!errors.code}
        />
      </FormField>
      <FormField label="Judul" htmlFor="mf-title" required error={errors.title}>
        <Input
          id="mf-title"
          value={data.title}
          onChange={(e) => set('title', e.target.value)}
          placeholder="Inventory Items"
          aria-invalid={!!errors.title}
        />
      </FormField>
      <FormField label="Tipe" htmlFor="mf-type" required>
        <Select
          value={data.type}
          onValueChange={(v) => set('type', v as ErpMenuType)}
        >
          <SelectTrigger id="mf-type">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {ERP_MENU_TYPES.map((t) => (
              <SelectItem key={t} value={t}>
                {t}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </FormField>
      <FormField label="Parent" htmlFor="mf-parent">
        <SearchSelect
          id="mf-parent"
          value={data.parentId}
          onValueChange={(v) => set('parentId', v)}
          placeholder="— Top level —"
          loadOptions={loadParentMenuOptions}
          initialLabel={data.parentLabel}
          title="Menu Parent"
        />
      </FormField>
      <FormField label="Path" htmlFor="mf-path">
        <Input
          id="mf-path"
          value={data.path}
          onChange={(e) => set('path', e.target.value)}
          placeholder="/master/items"
        />
      </FormField>
      <FormField label="Icon" htmlFor="mf-icon">
        <Input
          id="mf-icon"
          value={data.icon}
          onChange={(e) => set('icon', e.target.value)}
          placeholder="package"
        />
      </FormField>
      <FormField label="Urutan" htmlFor="mf-sort">
        <Input
          id="mf-sort"
          type="number"
          value={String(data.sortOrder)}
          onChange={(e) =>
            set('sortOrder', Number(e.target.value) || 0)
          }
          placeholder="0"
        />
      </FormField>
      <FormField label="Status" htmlFor="mf-active">
        <BooleanRadio
          id="mf-active"
          value={data.isActive}
          onValueChange={(v) => set('isActive', v)}
        />
      </FormField>
    </div>
  );
}
