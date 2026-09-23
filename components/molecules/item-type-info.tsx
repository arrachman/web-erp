'use client';

import * as React from 'react';
import * as Popover from '@radix-ui/react-popover';
import { Icon } from '@/components/ui/icons';
import type { ErpItemType } from '@/lib/api/items';

type TraitRow = { label: string; values: Record<ErpItemType, boolean> };

const TRAITS: TraitRow[] = [
  { label: 'Track stok',  values: { INVENTORY: true,  SERVICE: false, CONSUMABLE: true,  ASSET: true,  NON_INVENTORY: false } },
  { label: 'HPP / COGS',  values: { INVENTORY: true,  SERVICE: false, CONSUMABLE: true,  ASSET: false, NON_INVENTORY: false } },
  { label: 'Bisa dijual', values: { INVENTORY: true,  SERVICE: true,  CONSUMABLE: true,  ASSET: false, NON_INVENTORY: true  } },
  { label: 'Punya BOM',   values: { INVENTORY: true,  SERVICE: false, CONSUMABLE: false, ASSET: false, NON_INVENTORY: false } },
  { label: 'Berwujud',    values: { INVENTORY: true,  SERVICE: false, CONSUMABLE: true,  ASSET: true,  NON_INVENTORY: true  } },
];

const COLS: { type: ErpItemType; short: string }[] = [
  { type: 'INVENTORY',     short: 'INV' },
  { type: 'SERVICE',       short: 'SVC' },
  { type: 'CONSUMABLE',    short: 'CNS' },
  { type: 'ASSET',         short: 'AST' },
  { type: 'NON_INVENTORY', short: 'NIN' },
];

const EXAMPLES: Record<ErpItemType, string> = {
  INVENTORY:     'Barang dagang, raw material, finished goods',
  SERVICE:       'Jasa konsultasi, instalasi, tenaga kerja',
  CONSUMABLE:    'ATK, sparepart kecil, suplai habis pakai',
  ASSET:         'Mesin, kendaraan, properti (aset tetap)',
  NON_INVENTORY: 'Voucher, lisensi, biaya pass-through',
};

const TRAIT_LINE: Record<ErpItemType, string> = {
  INVENTORY:     'Track stok · HPP · Dijual · Berwujud',
  SERVICE:       'Tanpa stok · Tanpa HPP · Dijual · Tenaga kerja/jasa',
  CONSUMABLE:    'Track stok · HPP · Dijual · Habis pakai',
  ASSET:         'Track stok · Tanpa HPP · Tidak dijual · Aset tetap',
  NON_INVENTORY: 'Tanpa stok · Tanpa HPP · Dijual · Voucher/lisensi',
};

export function getItemTypeTraits(t: ErpItemType): string {
  return TRAIT_LINE[t] ?? '';
}

export function ItemTypeInfoButton({ currentType }: { currentType?: ErpItemType }) {
  const [open, setOpen] = React.useState(false);
  const closeTimerRef = React.useRef<ReturnType<typeof setTimeout> | null>(null);
  const openTimerRef = React.useRef<ReturnType<typeof setTimeout> | null>(null);

  const clearTimers = () => {
    if (closeTimerRef.current) { clearTimeout(closeTimerRef.current); closeTimerRef.current = null; }
    if (openTimerRef.current) { clearTimeout(openTimerRef.current); openTimerRef.current = null; }
  };
  const scheduleOpen = () => {
    clearTimers();
    openTimerRef.current = setTimeout(() => setOpen(true), 150);
  };
  const scheduleClose = () => {
    clearTimers();
    closeTimerRef.current = setTimeout(() => setOpen(false), 200);
  };

  React.useEffect(() => () => clearTimers(), []);

  return (
    <Popover.Root open={open} onOpenChange={setOpen}>
      <span
        className="inline-flex"
        onMouseEnter={scheduleOpen}
        onMouseLeave={scheduleClose}
      >
        <Popover.Trigger asChild>
          <button
            type="button"
            aria-label="Lihat perbandingan tipe item"
            onFocus={() => setOpen(true)}
            onBlur={scheduleClose}
            className="inline-flex h-5 w-5 items-center justify-center rounded-full text-[var(--fg-subtle)] hover:bg-[var(--panel-hover)] hover:text-foreground focus-visible:outline focus-visible:outline-1 focus-visible:outline-[var(--accent)]"
          >
            <Icon name="info" size={14} />
          </button>
        </Popover.Trigger>
      </span>
      <Popover.Portal>
        <Popover.Content
          side="right"
          align="start"
          sideOffset={8}
          collisionPadding={16}
          onMouseEnter={clearTimers}
          onMouseLeave={scheduleClose}
          onOpenAutoFocus={(e) => e.preventDefault()}
          className="z-[800] w-[460px] rounded-md border border-border bg-[var(--panel)] p-3 text-[var(--fg)] shadow-[var(--shadow-flyout)]"
        >
          <div className="mb-2 flex items-center justify-between">
            <h4 className="text-[12px] font-semibold">Perbandingan Tipe Item</h4>
            <Popover.Close
              aria-label="Tutup"
              className="inline-flex h-5 w-5 items-center justify-center rounded text-[var(--fg-subtle)] hover:bg-[var(--panel-hover)] hover:text-foreground"
            >
              <Icon name="x" size={12} />
            </Popover.Close>
          </div>

          <table className="w-full border-collapse text-[11px]">
            <thead>
              <tr className="border-b border-border text-left">
                <th className="py-1 font-medium text-[var(--fg-subtle)]">Sifat</th>
                {COLS.map((c) => (
                  <th
                    key={c.type}
                    title={c.type}
                    className={`py-1 text-center font-medium ${c.type === currentType ? 'text-[var(--accent)]' : ''}`}
                  >
                    {c.short}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {TRAITS.map((r) => (
                <tr key={r.label} className="border-b border-border/40">
                  <td className="py-1 text-[var(--fg-subtle)]">{r.label}</td>
                  {COLS.map((c) => {
                    const v = r.values[c.type];
                    const isActive = c.type === currentType;
                    return (
                      <td
                        key={c.type}
                        className={`py-1 text-center tabular-nums ${isActive ? 'bg-[var(--panel-2)]' : ''} ${v ? 'text-success' : 'text-[var(--fg-subtle)]'}`}
                      >
                        {v ? '✓' : '–'}
                      </td>
                    );
                  })}
                </tr>
              ))}
            </tbody>
          </table>

          <div className="mt-3 space-y-1 border-t border-border pt-2 text-[11px]">
            {COLS.map((c) => (
              <div
                key={c.type}
                className={`flex gap-2 ${c.type === currentType ? 'text-foreground' : 'text-[var(--fg-subtle)]'}`}
              >
                <span className={`w-10 shrink-0 font-mono text-[10px] ${c.type === currentType ? 'text-[var(--accent)]' : ''}`}>
                  {c.short}
                </span>
                <span className="w-[100px] shrink-0 font-medium">{c.type}</span>
                <span className="flex-1">{EXAMPLES[c.type]}</span>
              </div>
            ))}
          </div>

          <Popover.Arrow className="fill-[var(--panel)]" />
        </Popover.Content>
      </Popover.Portal>
    </Popover.Root>
  );
}
