'use client';

/**
 * Dialogs for TreeDndMasterPage — create/edit form modal + audit history
 * modal. Atomic tier: organism (co-located sibling of
 * tree-dnd-master-page.tsx). Pure presentational; parent owns form state,
 * save handler, and audit target.
 */

import * as React from 'react';
import {
  Modal,
  ModalContent,
  ModalHeader,
  ModalTitle,
  ModalFooter,
} from '@/components/organisms/modal';
import { AuditHistoryPanel } from '@/components/organisms/audit-history-panel';
import { tGlobal } from '@/lib/mock';
import type { FormErrors } from '@/lib/form-validation';
import type { TreeRow } from './tree-dnd-master-page.types';

export interface TreeDndMasterFormDialogProps<F> {
  open: boolean;
  editing: boolean;
  title: string;
  form: F;
  formErrors: FormErrors<F>;
  saving: boolean;
  FormFields: React.ComponentType<{
    data: F;
    onChange: (d: F) => void;
    errors?: FormErrors<F>;
  }>;
  onOpenChange: (open: boolean) => void;
  onChange: (d: F) => void;
  onSave: () => void;
}

export function TreeDndMasterFormDialog<F>({
  open,
  editing,
  title,
  form,
  formErrors,
  saving,
  FormFields,
  onOpenChange,
  onChange,
  onSave,
}: TreeDndMasterFormDialogProps<F>) {
  return (
    <Modal open={open} onOpenChange={onOpenChange}>
      <ModalContent>
        <ModalHeader>
          <ModalTitle>
            {editing
              ? `${tGlobal('Edit')} ${tGlobal(title)}`
              : `${tGlobal('Tambah')} ${tGlobal(title)}`}
          </ModalTitle>
        </ModalHeader>
        <FormFields data={form} onChange={onChange} errors={formErrors} />
        <ModalFooter>
          <button className="btn ghost" onClick={() => onOpenChange(false)}>
            {tGlobal('Batal')}
          </button>
          <button className="btn primary" onClick={onSave} disabled={saving} title="Ctrl+Enter">
            {saving ? tGlobal('Menyimpan...') : tGlobal('Simpan')}
          </button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}

export interface TreeDndMasterAuditDialogProps<T extends TreeRow> {
  auditTarget: T | null;
  auditEntityName: string;
  onOpenChange: (open: boolean) => void;
}

export function TreeDndMasterAuditDialog<T extends TreeRow>({
  auditTarget,
  auditEntityName,
  onOpenChange,
}: TreeDndMasterAuditDialogProps<T>) {
  return (
    <Modal open={!!auditTarget} onOpenChange={(v) => { if (!v) onOpenChange(false); }}>
      <ModalContent size="lg">
        <ModalHeader>
          <ModalTitle>
            {tGlobal('Riwayat Perubahan')} — {auditTarget?.code} {auditTarget?.name}
          </ModalTitle>
        </ModalHeader>
        <div style={{ padding: 0, maxHeight: '60vh', overflowY: 'auto' }}>
          {auditTarget && (
            <AuditHistoryPanel entityName={auditEntityName} entityId={auditTarget.id} />
          )}
        </div>
      </ModalContent>
    </Modal>
  );
}