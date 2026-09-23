'use client';

/**
 * User create/edit form — rich 2-column layout mirroring MyERP+ preferensi.
 * Tabs: Role, Cabang, Lokasi, Gudang, Akun (Role fungsional, lainnya stub).
 * Atomic tier: Organism.
 */

import * as React from 'react';
import { DateInput } from '@/components/ui/date-input';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { BooleanRadio } from '@/components/ui/radio-group';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/tabs';
import { Icon } from '@/components/ui/icons';
import { SearchSelect } from '@/components/molecules/search-select';
import { listBranches } from '@/lib/api/branches';
import { listWarehouses } from '@/lib/api/warehouses';
import { listRoles } from '@/lib/api/roles';
import type { ErpRole } from '@/lib/api/roles';
import type {
  ErpUser,
  ErpUserLevel,
  ErpUserRoleRef,
  CreateUserPayload,
  UpdateUserPayload,
} from '@/lib/api/users';

const loadBranchOptions = async (search: string, page: number, limit: number) => {
  const res = await listBranches({ page, limit, search: search || undefined, isActive: true });
  return { data: res.data.map((b) => ({ value: b.id, label: b.name, code: b.code })), total: res.meta.total };
};

const loadWarehouseOptions = async (search: string, page: number, limit: number) => {
  const res = await listWarehouses({ page, limit, search: search || undefined, isActive: true });
  return { data: res.data.map((w) => ({ value: w.id, label: w.name, code: w.code })), total: res.meta.total };
};

const LEVELS: ErpUserLevel[] = [
  'CENTRAL', 'POS', 'POS_AND_CENTRAL', 'BI', 'BI_AND_CENTRAL',
];

const LANGUAGES = [
  { code: 'id', label: 'INA — Indonesia' },
  { code: 'en', label: 'ENG — English' },
];

export interface UserFormData {
  username: string;
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
  erpLevel: ErpUserLevel;
  language: string;
  branchId: string;
  branchLabel?: string;
  homeWarehouseId: string;
  homeWarehouseLabel?: string;
  expiresAt: string;
  isActive: boolean;
  roleIds: string[];
}

const defaultForm = (): UserFormData => ({
  username: '', fullName: '', email: '', password: '', confirmPassword: '',
  erpLevel: 'CENTRAL', language: 'id', branchId: '', homeWarehouseId: '',
  expiresAt: '', isActive: true, roleIds: [],
});

function fromUser(u: ErpUser, roles?: ErpUserRoleRef[]): UserFormData {
  return {
    username: u.username,
    fullName: u.fullName,
    email: u.email ?? '',
    password: '',
    confirmPassword: '',
    erpLevel: u.erpLevel,
    language: u.language ?? 'id',
    branchId: u.homeBranchId ?? '',
    branchLabel: '',
    homeWarehouseId: u.homeWarehouseId ?? '',
    homeWarehouseLabel: '',
    expiresAt: u.expiresAt ? u.expiresAt.slice(0, 10) : '',
    isActive: u.isActive,
    roleIds: (roles ?? u.roles ?? []).map((r) => r.role.id),
  };
}

export function toCreatePayload(f: UserFormData): CreateUserPayload {
  return {
    username: f.username, fullName: f.fullName, email: f.email || undefined,
    password: f.password, erpLevel: f.erpLevel, language: f.language,
    branchId: f.branchId || undefined, homeWarehouseId: f.homeWarehouseId || undefined,
    expiresAt: f.expiresAt || undefined, isActive: f.isActive,
    roleIds: f.roleIds.length ? f.roleIds : undefined,
  };
}

export function toUpdatePayload(f: UserFormData): UpdateUserPayload {
  return {
    username: f.username, fullName: f.fullName, email: f.email || undefined,
    password: f.password || undefined, erpLevel: f.erpLevel, language: f.language,
    branchId: f.branchId || undefined, homeWarehouseId: f.homeWarehouseId || undefined,
    expiresAt: f.expiresAt || null, isActive: f.isActive, roleIds: f.roleIds,
  };
}

export function useUserForm(editing: ErpUser | null, editRoles?: ErpUserRoleRef[]) {
  const [data, setData] = React.useState<UserFormData>(() =>
    editing ? fromUser(editing, editRoles) : defaultForm(),
  );
  React.useEffect(() => {
    setData(editing ? fromUser(editing, editRoles) : defaultForm());
  }, [editing, editRoles]);
  return { data, setData };
}

// ─── Role Tab ─────────────────────────────────────────────────────────────────

function RoleTab({
  roleIds, onChange, allRoles,
}: { roleIds: string[]; onChange: (ids: string[]) => void; allRoles: ErpRole[] }) {
  const assigned = allRoles.filter((r) => roleIds.includes(r.id));
  const available = allRoles.filter((r) => !roleIds.includes(r.id));
  const [adding, setAdding] = React.useState(false);

  return (
    <div className="p-4">
      <div className="mb-2 flex items-center gap-2">
        <button className="btn sm primary" onClick={() => setAdding((v) => !v)}>
          <Icon name="plus" />
          Tambah
        </button>
      </div>
      {adding && available.length > 0 && (
        <div className="mb-3 rounded border border-border bg-secondary p-2">
          <p className="mb-1 text-xs text-muted-foreground">Pilih role untuk ditambahkan:</p>
          <div className="flex flex-wrap gap-1">
            {available.map((r) => (
              <button
                key={r.id}
                className="btn xs ghost"
                onClick={() => { onChange([...roleIds, r.id]); setAdding(false); }}
              >
                {r.code} — {r.name}
              </button>
            ))}
          </div>
        </div>
      )}
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-border text-left text-xs text-muted-foreground">
            <th className="py-1 pr-4">Kode</th>
            <th className="py-1">Nama</th>
            <th />
          </tr>
        </thead>
        <tbody>
          {assigned.length === 0 && (
            <tr><td colSpan={3} className="py-4 text-center text-xs text-muted-foreground">Belum ada role</td></tr>
          )}
          {assigned.map((r) => (
            <tr key={r.id} className="border-b border-border/50 hover:bg-secondary/50">
              <td className="mono py-1.5 pr-4">{r.code}</td>
              <td className="py-1.5">{r.name}</td>
              <td className="py-1.5 text-right">
                <button
                  className="btn xs danger ghost"
                  onClick={() => onChange(roleIds.filter((id) => id !== r.id))}
                >
                  <Icon name="x" />
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

// ─── Main Form ────────────────────────────────────────────────────────────────

interface UserFormProps {
  editing: ErpUser | null;
  data: UserFormData;
  onChange: (d: UserFormData) => void;
}

export function UserForm({ editing, data, onChange }: UserFormProps) {
  const set = (k: keyof UserFormData, v: string | boolean | string[]) =>
    onChange({ ...data, [k]: v });

  const [allRoles, setAllRoles] = React.useState<ErpRole[]>([]);

  React.useEffect(() => {
    listRoles({ limit: 100 }).then((r) => setAllRoles(r.data)).catch(() => {});
  }, []);

  return (
    <div className="overflow-y-auto" style={{ maxHeight: 'calc(80vh - 120px)' }}>
      {/* 2-column grid */}
      <div className="grid grid-cols-2 gap-x-6 gap-y-0 p-5">
        {/* Left */}
        <div>
          <FormField label="Kode" htmlFor="uf-code" required>
            <Input id="uf-code" value={data.username}
              onChange={(e) => set('username', e.target.value)} placeholder="sales1" />
          </FormField>
          <FormField label="Password" htmlFor="uf-pass" required={!editing}>
            <Input id="uf-pass" type="password" value={data.password}
              onChange={(e) => set('password', e.target.value)}
              placeholder={editing ? 'Kosongkan jika tidak diubah' : 'Min 8 karakter'} />
          </FormField>
          <FormField label="Email" htmlFor="uf-email">
            <Input id="uf-email" type="email" value={data.email}
              onChange={(e) => set('email', e.target.value)} placeholder="user@sentient.id" />
          </FormField>
          <FormField label="Lokasi" htmlFor="uf-loc">
            <Input id="uf-loc" value="" readOnly disabled placeholder="—" />
          </FormField>
          <FormField label="Kota" htmlFor="uf-city">
            <Input id="uf-city" value="" readOnly disabled placeholder="Dari cabang" />
          </FormField>
          <FormField label="Tgl Berakhir" htmlFor="uf-exp">
            <DateInput id="uf-exp" value={data.expiresAt}
              onChange={(v) => set('expiresAt', v)} />
          </FormField>
          <FormField label="Aktif" htmlFor="uf-active">
            <BooleanRadio id="uf-active" value={data.isActive}
              onValueChange={(v) => set('isActive', v)} />
          </FormField>
        </div>
        {/* Right */}
        <div>
          <FormField label="Nama" htmlFor="uf-fname" required>
            <Input id="uf-fname" value={data.fullName}
              onChange={(e) => set('fullName', e.target.value)} placeholder="HARLY" />
          </FormField>
          <FormField label="Konfirmasi Password" htmlFor="uf-cpass" required={!editing}>
            <Input id="uf-cpass" type="password" value={data.confirmPassword}
              onChange={(e) => set('confirmPassword', e.target.value)}
              placeholder={editing ? '—' : 'Ulangi password'} />
          </FormField>
          <FormField label="Cabang" htmlFor="uf-branch">
            <SearchSelect
              id="uf-branch"
              value={data.branchId}
              onValueChange={(v) => set('branchId', v)}
              placeholder="Cari cabang…"
              loadOptions={loadBranchOptions}
              initialLabel={data.branchLabel}
              title="Cabang"
            />
          </FormField>
          <FormField label="Gudang" htmlFor="uf-wh">
            <SearchSelect
              id="uf-wh"
              value={data.homeWarehouseId}
              onValueChange={(v) => set('homeWarehouseId', v)}
              placeholder="Cari gudang…"
              loadOptions={loadWarehouseOptions}
              initialLabel={data.homeWarehouseLabel}
              title="Gudang"
            />
          </FormField>
          <FormField label="Bahasa" htmlFor="uf-lang">
            <Select value={data.language} onValueChange={(v) => set('language', v)}>
              <SelectTrigger id="uf-lang">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {LANGUAGES.map((l) => (
                  <SelectItem key={l.code} value={l.code}>{l.label}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </FormField>
          <FormField label="Level" htmlFor="uf-level" required>
            <Select value={data.erpLevel} onValueChange={(v) => set('erpLevel', v as ErpUserLevel)}>
              <SelectTrigger id="uf-level"><SelectValue /></SelectTrigger>
              <SelectContent>
                {LEVELS.map((l) => (
                  <SelectItem key={l} value={l}>{l}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </FormField>
        </div>
      </div>

      {/* Tabs */}
      <Tabs defaultValue="role" className="border-t border-border">
        <TabsList>
          <TabsTrigger value="role">Role</TabsTrigger>
          <TabsTrigger value="cabang">Cabang</TabsTrigger>
          <TabsTrigger value="lokasi">Lokasi</TabsTrigger>
          <TabsTrigger value="gudang">Gudang</TabsTrigger>
          <TabsTrigger value="akun">Akun</TabsTrigger>
        </TabsList>
        <TabsContent value="role">
          <RoleTab
            roleIds={data.roleIds}
            onChange={(ids) => set('roleIds', ids)}
            allRoles={allRoles}
          />
        </TabsContent>
        {(['cabang', 'lokasi', 'gudang', 'akun'] as const).map((t) => (
          <TabsContent key={t} value={t}>
            <p className="p-6 text-center text-xs text-muted-foreground">Segera hadir</p>
          </TabsContent>
        ))}
      </Tabs>
    </div>
  );
}
