'use client';

/**
 * F2 Admin — User Management page.
 * List kolom: Kode, Nama, Bahasa, Cabang, Tgl EXP, Status — mengikuti pola MyERP+.
 * Server-driven pagination (page/limit/search → backend).
 * Atomic tier: Page.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import {
  Modal,
  ModalContent,
  ModalHeader,
  ModalTitle,
  ModalFooter,
} from '@/components/organisms/modal';
import {
  ErpListLayout,
  type SummaryConfig,
  type ListPaginationConfig,
} from '@/components/organisms/erp-list-layout';
import {
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableHead,
  TableCell,
  TableEmpty,
} from '@/components/organisms/table';
import { confirmAction, notify } from '@/lib/feedback';
import { useErpList } from '@/lib/use-erp-list';
import { useListPagination } from '@/lib/use-list-pagination';
import {
  listUsers, getUser, createUser, updateUser, deleteUser,
} from '@/lib/api/users';
import type { ErpUser, ErpUserRoleRef } from '@/lib/api/users';
import {
  UserForm, useUserForm, toCreatePayload, toUpdatePayload,
} from './users-form';

// ─── helpers ──────────────────────────────────────────────────────────────────

function fmtDate(iso?: string | null) {
  if (!iso) return '—';
  const d = new Date(iso);
  return `${String(d.getDate()).padStart(2, '0')}/${String(d.getMonth() + 1).padStart(2, '0')}/${d.getFullYear()}`;
}

function fmtLang(code?: string) {
  return code === 'en' ? 'ENG' : 'INA';
}

// ─── Table ────────────────────────────────────────────────────────────────────

function UsersTable({
  rows, onEdit, onToggle, onDelete,
}: {
  rows: ErpUser[];
  onEdit: (u: ErpUser) => void;
  onToggle: (u: ErpUser) => void;
  onDelete: (u: ErpUser) => void;
}) {
  return (
    <div className="lines">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Kode</TableHead>
            <TableHead>Nama</TableHead>
            <TableHead>Bahasa</TableHead>
            <TableHead>Cabang</TableHead>
            <TableHead>Tgl EXP</TableHead>
            <TableHead>Status</TableHead>
            <TableHead />
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.length === 0 ? (
            <TableEmpty colSpan={7} />
          ) : (
            rows.map((u) => (
              <TableRow key={u.id}>
                <TableCell>
                  <button
                    className="link mono text-left"
                    onClick={() => onEdit(u)}
                  >
                    {u.username}
                  </button>
                </TableCell>
                <TableCell>{u.fullName}</TableCell>
                <TableCell className="muted">{fmtLang(u.language)}</TableCell>
                <TableCell className="muted">
                  {u.homeBranchId ? <span className="code">{u.homeBranchId}</span> : '—'}
                </TableCell>
                <TableCell className="muted">{fmtDate(u.expiresAt)}</TableCell>
                <TableCell>
                  <Badge variant={u.isActive ? 'success' : 'default'} dot>
                    {u.isActive ? 'Aktif' : 'Nonaktif'}
                  </Badge>
                </TableCell>
                <TableCell>
                  <div style={{ display: 'flex', gap: 4 }}>
                    <button className="btn sm ghost" onClick={() => onToggle(u)}>
                      {u.isActive ? 'Nonaktifkan' : 'Aktifkan'}
                    </button>
                    <button className="btn sm danger" onClick={() => onDelete(u)}>
                      Hapus
                    </button>
                  </div>
                </TableCell>
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
    </div>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export function ErpUsersPage() {
  const [search, setSearch] = React.useState('');
  const { page, pageSize, setPage, setPageSize } = useListPagination('users');

  // Debounce search to avoid hitting API on every keystroke.
  const [debouncedSearch, setDebouncedSearch] = React.useState(search);
  React.useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(search), 300);
    return () => clearTimeout(t);
  }, [search]);

  const { rows, meta, loading, error, reload } = useErpList(
    () =>
      listUsers({
        page,
        limit: pageSize,
        search: debouncedSearch || undefined,
      }),
    [page, pageSize, debouncedSearch],
  );

  // Reset to page 1 when search/pageSize change.
  React.useEffect(() => { setPage(1); }, [debouncedSearch, pageSize]);

  const [open, setOpen] = React.useState(false);
  const [editing, setEditing] = React.useState<ErpUser | null>(null);
  const [editRoles, setEditRoles] = React.useState<ErpUserRoleRef[]>([]);
  const [saving, setSaving] = React.useState(false);
  const { data, setData } = useUserForm(editing, editRoles);

  const paged = rows;
  const totalRows = meta?.total ?? 0;
  const pageCount = meta?.totalPages ?? 1;

  const userSummary: SummaryConfig = {
    metricLabel: 'Σ user',
    rowCount: totalRows,
    totalCount: totalRows,
  };
  const userPagination: ListPaginationConfig = {
    page, pageCount, pageSize, totalRows, onPage: setPage, onPageSize: setPageSize,
  };

  const openCreate = () => {
    setEditing(null);
    setEditRoles([]);
    setOpen(true);
  };

  const openEdit = async (u: ErpUser) => {
    setEditing(u);
    setEditRoles([]);
    setOpen(true);
    try {
      const detail = await getUser(u.id);
      setEditRoles(detail.roles ?? []);
    } catch {
      // roles stay empty — non-blocking
    }
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      if (editing) {
        await updateUser(editing.id, toUpdatePayload(data));
        notify('User diperbarui', 'success');
      } else {
        await createUser(toCreatePayload(data));
        notify('User dibuat', 'success');
      }
      setOpen(false);
      reload();
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally {
      setSaving(false);
    }
  };

  const handleToggle = (u: ErpUser) => {
    confirmAction({
      title: u.isActive ? 'Nonaktifkan user?' : 'Aktifkan user?',
      message: `${u.username} akan ${u.isActive ? 'dinonaktifkan' : 'diaktifkan'}.`,
      variant: u.isActive ? 'warn' : 'primary',
      confirmLabel: u.isActive ? 'Nonaktifkan' : 'Aktifkan',
      onConfirm: async () => {
        try {
          await updateUser(u.id, { isActive: !u.isActive });
          notify(`User ${u.isActive ? 'dinonaktifkan' : 'diaktifkan'}`, 'success');
          reload();
        } catch (e: unknown) {
          notify(e instanceof Error ? e.message : 'Gagal', 'danger');
        }
      },
    });
  };

  const handleDelete = (u: ErpUser) => {
    confirmAction({
      title: 'Hapus user?',
      message: `${u.username} akan dihapus permanen.`,
      variant: 'danger',
      confirmLabel: 'Hapus',
      confirmIcon: 'trash',
      onConfirm: async () => {
        try {
          await deleteUser(u.id);
          notify('User dihapus', 'success');
          reload();
        } catch (e: unknown) {
          notify(e instanceof Error ? e.message : 'Gagal', 'danger');
        }
      },
    });
  };

  return (
    <>
      <ErpListLayout
        title="Users"
        code="USR"
        loading={loading}
        error={error}
        search={search}
        onSearch={setSearch}
        onAdd={openCreate}
        onRefresh={reload}
        summary={userSummary}
        pagination={userPagination}
      >
        <UsersTable
          rows={paged}
          onEdit={openEdit}
          onToggle={handleToggle}
          onDelete={handleDelete}
        />
      </ErpListLayout>

      <Modal open={open} onOpenChange={setOpen}>
        <ModalContent size="lg">
          <ModalHeader>
            <ModalTitle>{editing ? `Edit User — ${editing.username}` : 'Tambah User'}</ModalTitle>
          </ModalHeader>
          <UserForm editing={editing} data={data} onChange={setData} />
          <ModalFooter>
            <button className="btn ghost" onClick={() => setOpen(false)}>Batal</button>
            <button className="btn primary" onClick={handleSave} disabled={saving}>
              {saving ? 'Menyimpan...' : 'Simpan'}
            </button>
          </ModalFooter>
        </ModalContent>
      </Modal>
    </>
  );
}
