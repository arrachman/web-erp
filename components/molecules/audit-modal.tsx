'use client';

/**
 * Modal pembungkus AuditHistoryPanel — dipakai SimpleMasterPage saat user
 * pilih "Riwayat" di kebab menu row. Render konsisten lintas-entitas.
 * Atomic tier: Molecule.
 */

import * as React from 'react';
import { Modal, ModalContent, ModalHeader, ModalTitle } from '@/components/organisms/modal';
import { AuditHistoryPanel } from '@/components/organisms/audit-history-panel';
import { tGlobal } from '@/lib/mock';

export interface AuditTargetLike { id: string; code: string; name: string }

export function AuditModal({
  target, onClose, entityName,
}: { target: AuditTargetLike | null; onClose: () => void; entityName: string }) {
  return (
    <Modal open={!!target} onOpenChange={(v) => { if (!v) onClose(); }}>
      <ModalContent size="lg">
        <ModalHeader>
          <ModalTitle>{tGlobal('Riwayat Perubahan')} — {target?.code} {target?.name}</ModalTitle>
        </ModalHeader>
        <div style={{ padding: '0', maxHeight: '60vh', overflowY: 'auto' }}>
          {target && <AuditHistoryPanel entityName={entityName} entityId={target.id} />}
        </div>
      </ModalContent>
    </Modal>
  );
}
