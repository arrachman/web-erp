'use client';

/**
 * Editable sub-list of partner contacts (md_partner_contacts).
 * Phone ("no hp") lives here — deliberately kept out of the partner's
 * main/"Utama" fields.
 * UX: list-or-form — form only visible while adding/editing so the user
 * is not confused by a permanent empty form under the list.
 * Atomic tier: Organism.
 */

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { Badge } from '@/components/ui/badge';
import { Icon } from '@/components/ui/icons';
import {
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableHead,
  TableCell,
  TableEmpty,
} from '@/components/organisms/table';
import { RowActionsMenu, RowContextMenu, type RowActionItem } from '@/components/molecules/row-actions-menu';
import { confirmAction, notify } from '@/lib/feedback';
import {
  getPartner,
  addPartnerContact,
  updatePartnerContact,
  removePartnerContact,
  type ErpPartnerContact,
} from '@/lib/api/partners';

interface DraftContact {
  name: string;
  title: string;
  phone: string;
  email: string;
  isDefault: boolean;
}

/** null = list mode; 'new' = create form; string id = edit form */
type FormMode = null | 'new' | string;

const emptyDraft = (): DraftContact => ({
  name: '',
  title: '',
  phone: '',
  email: '',
  isDefault: false,
});

export function PartnerContactsEditor({ partnerId }: { partnerId: string }) {
  const [items, setItems] = React.useState<ErpPartnerContact[]>([]);
  const [loading, setLoading] = React.useState(true);
  const [draft, setDraft] = React.useState<DraftContact>(emptyDraft);
  const [saving, setSaving] = React.useState(false);
  const [formMode, setFormMode] = React.useState<FormMode>(null);

  React.useEffect(() => {
    let active = true;
    getPartner(partnerId)
      .then((p) => { if (active) setItems(p.contacts ?? []); })
      .catch((e) => notify(e instanceof Error ? e.message : 'Gagal memuat kontak', 'danger'))
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [partnerId]);

  const setD = (k: keyof DraftContact, v: string | boolean) =>
    setDraft((d) => ({ ...d, [k]: v }));

  const openCreate = () => {
    setFormMode('new');
    setDraft(emptyDraft());
  };

  const handleEdit = (c: ErpPartnerContact) => {
    setFormMode(c.id);
    setDraft({
      name: c.name,
      title: c.title ?? '',
      phone: c.phone ?? '',
      email: c.email ?? '',
      isDefault: c.isDefault,
    });
  };

  const handleCancel = () => {
    setFormMode(null);
    setDraft(emptyDraft());
  };

  const handleSubmit = async () => {
    if (!draft.name.trim()) { notify('Nama kontak wajib diisi', 'warn'); return; }
    setSaving(true);
    try {
      const payload = {
        name: draft.name.trim(),
        title: draft.title.trim() || undefined,
        phone: draft.phone.trim() || undefined,
        email: draft.email.trim() || undefined,
        isDefault: draft.isDefault,
      };
      if (formMode && formMode !== 'new') {
        const updated = await updatePartnerContact(partnerId, formMode, payload);
        setItems((prev) => prev.map((x) => (x.id === formMode ? updated : x)));
        notify('Kontak diperbarui', 'success');
      } else {
        const created = await addPartnerContact(partnerId, payload);
        setItems((prev) => [...prev, created]);
        notify('Kontak ditambahkan', 'success');
      }
      handleCancel();
    } catch (e) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan kontak', 'danger');
    } finally {
      setSaving(false);
    }
  };

  const handleRemove = (c: ErpPartnerContact) =>
    confirmAction({
      title: 'Hapus kontak?',
      message: `${c.name} akan dihapus.`,
      variant: 'danger',
      confirmLabel: 'Hapus',
      confirmIcon: 'trash',
      onConfirm: async () => {
        try {
          await removePartnerContact(partnerId, c.id);
          setItems((prev) => prev.filter((x) => x.id !== c.id));
          if (formMode === c.id) handleCancel();
          notify('Kontak dihapus', 'success');
        } catch (e) {
          notify(e instanceof Error ? e.message : 'Gagal menghapus', 'danger');
        }
      },
    });

  // Form mode — hide list so user focuses on one task
  if (formMode !== null) {
    const isEdit = formMode !== 'new';
    return (
      <div className="flex flex-col gap-3 p-4">
        <div className="flex items-center justify-between gap-2">
          <div>
            <div className="text-[13px] font-semibold text-foreground">
              {isEdit ? 'Edit kontak' : 'Tambah kontak'}
            </div>
            <div className="text-[11px] text-muted-foreground">
              Isi data kontak, lalu simpan. Kembali ke daftar dengan Batal.
            </div>
          </div>
          <button type="button" className="btn ghost sm" onClick={handleCancel} disabled={saving}>
            <Icon name="arrowleft" size={12} /> Kembali ke daftar
          </button>
        </div>

        <div className="grid grid-cols-2 gap-x-3 gap-y-1 rounded-[var(--radius)] border border-border bg-secondary/30 p-3">
          <FormField label="Nama" htmlFor="pc-name" required>
            <Input id="pc-name" value={draft.name} onChange={(e) => setD('name', e.target.value)} placeholder="Budi Santoso" />
          </FormField>
          <FormField label="Jabatan" htmlFor="pc-title">
            <Input id="pc-title" value={draft.title} onChange={(e) => setD('title', e.target.value)} placeholder="Manager Pembelian" />
          </FormField>
          <FormField label="No HP" htmlFor="pc-phone">
            <Input id="pc-phone" value={draft.phone} onChange={(e) => setD('phone', e.target.value)} placeholder="08123456789" />
          </FormField>
          <FormField label="Email" htmlFor="pc-email">
            <Input id="pc-email" type="email" value={draft.email} onChange={(e) => setD('email', e.target.value)} placeholder="budi@example.com" />
          </FormField>
          <FormField label="Kontak utama" htmlFor="pc-default">
            <BooleanRadio id="pc-default" value={draft.isDefault} onValueChange={(v) => setD('isDefault', v)} trueLabel="Ya" falseLabel="Tidak" />
          </FormField>
          <div className="col-span-2 flex justify-end gap-2 border-t border-border pt-3 mt-1">
            <button type="button" className="btn ghost" onClick={handleCancel} disabled={saving}>
              Batal
            </button>
            <button type="button" className="btn primary" onClick={handleSubmit} disabled={saving}>
              {saving ? 'Menyimpan…' : isEdit ? 'Simpan perubahan' : 'Simpan kontak'}
            </button>
          </div>
        </div>
      </div>
    );
  }

  // List mode
  return (
    <div className="flex flex-col gap-3 p-4">
      <div className="flex items-center justify-between gap-2">
        <div className="text-[12px] text-muted-foreground">
          {loading ? 'Memuat…' : `${items.length} kontak`}
        </div>
        <button type="button" className="btn sm primary" onClick={openCreate}>
          <Icon name="plus" size={12} /> Tambah kontak
        </button>
      </div>

      <div className="lines">
        <Table className="table-fixed">
          <TableHeader>
            <TableRow>
              <TableHead>Nama</TableHead>
              <TableHead>Jabatan</TableHead>
              <TableHead>No HP</TableHead>
              <TableHead>Email</TableHead>
              <TableHead style={{ width: 80 }}>Utama</TableHead>
              <TableHead style={{ width: 64 }} />
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              <TableRow><TableCell colSpan={6} className="muted">Memuat…</TableCell></TableRow>
            ) : items.length === 0 ? (
              <TableEmpty colSpan={6} variant="empty" entityLabel="kontak" />
            ) : (
              items.map((c) => {
                const rowActions: RowActionItem[] = [
                  { label: 'Edit', onSelect: () => handleEdit(c) },
                  { label: 'Hapus', onSelect: () => handleRemove(c), danger: true, separatorBefore: true },
                ];
                return (
                  <RowContextMenu key={c.id} items={rowActions}>
                    <TableRow>
                      <TableCell>{c.name}</TableCell>
                      <TableCell className="muted">{c.title || '—'}</TableCell>
                      <TableCell>{c.phone || '—'}</TableCell>
                      <TableCell className="muted">{c.email || '—'}</TableCell>
                      <TableCell>{c.isDefault ? <Badge variant="info" dot>Utama</Badge> : null}</TableCell>
                      <TableCell>
                        <RowActionsMenu items={rowActions} />
                      </TableCell>
                    </TableRow>
                  </RowContextMenu>
                );
              })
            )}
          </TableBody>
        </Table>
      </div>

      {!loading && items.length === 0 && (
        <div className="flex flex-col items-center gap-2 rounded-[var(--radius)] border border-dashed border-border py-6 text-center">
          <div className="text-[12px] text-muted-foreground">
            Belum ada kontak. Tambah orang yang bisa dihubungi untuk partner ini.
          </div>
          <button type="button" className="btn sm primary" onClick={openCreate}>
            <Icon name="plus" size={12} /> Tambah kontak pertama
          </button>
        </div>
      )}
    </div>
  );
}
