'use client';

/**
 * Dialog clone: salin konfigurasi kolom dari transaksi lain ke grid aktif.
 * Atomic tier: Molecule.
 */

import * as React from 'react';
import { Modal, ModalContent, ModalHeader, ModalTitle, ModalFooter } from '@/components/organisms/modal';
import { Icon } from '@/components/ui/icons';
import { notify } from '@/lib/feedback';
import {
  listTransactionTypes,
  getTransactionGrids,
  type ErpTransactionType,
  type ErpGridColumn,
} from '@/lib/api/transaction-grids';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';

interface GridCloneDialogProps {
  open: boolean;
  onClose: () => void;
  currentCode: string;
  onApply: (cols: ErpGridColumn[]) => void;
}

export function GridCloneDialog({ open, onClose, currentCode, onApply }: GridCloneDialogProps) {
  const [types, setTypes] = React.useState<ErpTransactionType[]>([]);
  const [selectedCode, setSelectedCode] = React.useState('');
  const [loading, setLoading] = React.useState(false);

  React.useEffect(() => {
    if (!open) return;
    listTransactionTypes()
      .then((t) => setTypes(t.filter((x) => x.code !== currentCode)))
      .catch(() => notify('Gagal memuat daftar transaksi', 'danger'));
  }, [open, currentCode]);

  const handleApply = async () => {
    if (!selectedCode) return;
    setLoading(true);
    try {
      const r = await getTransactionGrids(selectedCode);
      const src = r.grids.find((g) => g.isPrimary) ?? r.grids[0];
      if (!src?.columns.length) {
        notify('Transaksi sumber tidak memiliki kolom', 'warn');
        return;
      }
      onApply(src.columns.map((c, i) => ({ ...c, id: undefined, sortOrder: i })));
      notify(`Kolom disalin dari ${r.type.name}`, 'success');
      onClose();
    } catch {
      notify('Gagal memuat kolom sumber', 'danger');
    } finally {
      setLoading(false);
    }
  };

  const grouped = React.useMemo(() => {
    const map = new Map<string, ErpTransactionType[]>();
    for (const t of types) {
      const g = t.moduleLabel;
      if (!map.has(g)) map.set(g, []);
      map.get(g)!.push(t);
    }
    return map;
  }, [types]);

  return (
    <Modal open={open} onOpenChange={(v) => { if (!v) onClose(); }}>
      <ModalContent size="md">
        <ModalHeader>
          <ModalTitle>Salin Kolom dari Transaksi Lain</ModalTitle>
        </ModalHeader>
        <div className="px-5 py-3">
          <p className="mb-3 text-sm text-muted-foreground">
            Pilih transaksi sumber. Kolom pada tab utama transaksi tersebut akan disalin ke grid aktif.
          </p>
          <Select value={selectedCode} onValueChange={setSelectedCode}>
            <SelectTrigger>
              <SelectValue placeholder="Pilih transaksi sumber…" />
            </SelectTrigger>
            <SelectContent>
              {[...grouped.entries()].map(([module, items]) => (
                <React.Fragment key={module}>
                  <div className="px-2 py-1 text-xs font-semibold text-muted-foreground">{module}</div>
                  {items.map((t) => (
                    <SelectItem key={t.code} value={t.code}>{t.name} ({t.code})</SelectItem>
                  ))}
                </React.Fragment>
              ))}
            </SelectContent>
          </Select>
        </div>
        <ModalFooter>
          <button type="button" className="btn" onClick={onClose}>Batal</button>
          <button
            type="button"
            className="btn primary"
            disabled={!selectedCode || loading}
            onClick={handleApply}
          >
            {loading && <Icon name="refresh" size={13} />}
            Salin Kolom
          </button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}
