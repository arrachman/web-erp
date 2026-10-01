'use client';

/**
 * Outstanding-giro picker dialog for clearing transactions (RGC/SGC). Lists
 * `listOutstandingGiros(type, search)` results with checkboxes, excluding giros
 * already chosen on the parent form. Confirming returns the selected giros so the
 * parent can append them as clearing rows. Atomic tier: Organism.
 */

import * as React from 'react';
import { Input } from '@/components/ui/input';
import {
  Modal,
  ModalContent,
  ModalHeader,
  ModalTitle,
  ModalFooter,
} from '@/components/organisms/modal';
import {
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableHead,
  TableCell,
  TableEmpty,
} from '@/components/organisms/table';
import { formatNumber } from '@/lib/format';
import {
  listOutstandingGiros,
  type GiroType,
  type OutstandingGiro,
} from '@/lib/api/fin-giro-entries';

export function GiroPickerDialog({
  open,
  onOpenChange,
  type,
  excludeIds,
  onConfirm,
}: {
  open: boolean;
  onOpenChange: (v: boolean) => void;
  type: GiroType;
  /** giroIds already chosen on the parent form (hidden from the list). */
  excludeIds: string[];
  onConfirm: (giros: OutstandingGiro[]) => void;
}) {
  const [search, setSearch] = React.useState('');
  const [debounced, setDebounced] = React.useState('');
  const [rows, setRows] = React.useState<OutstandingGiro[]>([]);
  const [loading, setLoading] = React.useState(false);
  const [picked, setPicked] = React.useState<Set<string>>(new Set());

  React.useEffect(() => {
    const t = setTimeout(() => setDebounced(search), 300);
    return () => clearTimeout(t);
  }, [search]);

  React.useEffect(() => {
    if (!open) return;
    setPicked(new Set());
  }, [open]);

  React.useEffect(() => {
    if (!open) return;
    let alive = true;
    setLoading(true);
    listOutstandingGiros({ type, search: debounced || undefined })
      .then((data) => { if (alive) setRows(data); })
      .catch(() => { if (alive) setRows([]); })
      .finally(() => { if (alive) setLoading(false); });
    return () => { alive = false; };
  }, [open, type, debounced]);

  const exclude = React.useMemo(() => new Set(excludeIds), [excludeIds]);
  const visible = rows.filter((r) => !exclude.has(r.id));

  const toggle = (id: string) =>
    setPicked((s) => {
      const n = new Set(s);
      n.has(id) ? n.delete(id) : n.add(id);
      return n;
    });

  const confirm = () => {
    onConfirm(visible.filter((r) => picked.has(r.id)));
    onOpenChange(false);
  };

  return (
    <Modal open={open} onOpenChange={onOpenChange}>
      <ModalContent size="lg">
        <ModalHeader>
          <ModalTitle>Pilih Giro Outstanding</ModalTitle>
        </ModalHeader>
        <div className="px-4 pb-2">
          <Input
            value={search}
            placeholder="Cari no giro / bank…"
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
        <div className="max-h-[50vh] overflow-auto px-4">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead style={{ width: 36 }} />
                <TableHead>No Giro</TableHead>
                <TableHead>Bank</TableHead>
                <TableHead>Jatuh Tempo</TableHead>
                <TableHead style={{ textAlign: 'right' }}>Nominal</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                <TableRow>
                  <TableCell colSpan={5} className="text-center text-muted-foreground py-4">
                    Memuat…
                  </TableCell>
                </TableRow>
              ) : visible.length === 0 ? (
                <TableEmpty colSpan={5} />
              ) : (
                visible.map((r) => (
                  <TableRow key={r.id} className="cursor-pointer" onClick={() => toggle(r.id)}>
                    <TableCell style={{ textAlign: 'center' }}>
                      <input
                        type="checkbox"
                        checked={picked.has(r.id)}
                        onChange={() => toggle(r.id)}
                        onClick={(e) => e.stopPropagation()}
                      />
                    </TableCell>
                    <TableCell className="mono">{r.giroNumber}</TableCell>
                    <TableCell>{r.bankName ?? '—'}</TableCell>
                    <TableCell>{r.dueDate.slice(0, 10)}</TableCell>
                    <TableCell className="tabular-nums" style={{ textAlign: 'right' }}>
                      {formatNumber(Number(r.amount || 0), 2)}
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </div>
        <ModalFooter>
          <button className="btn ghost" onClick={() => onOpenChange(false)}>Batal</button>
          <button className="btn primary" onClick={confirm} disabled={picked.size === 0}>
            Tambah {picked.size > 0 ? `(${picked.size})` : ''}
          </button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}
