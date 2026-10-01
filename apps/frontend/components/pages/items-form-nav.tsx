'use client';

/**
 * Item form navigation: section metadata (label/icon/group), per-section
 * "filled" detection, the grouped side-nav, and the Cepat/Lengkap mode
 * toggle. Atomic tier: Molecule. Consumed by items-form-fields.
 */

import * as React from 'react';
import { Icon, type IconName } from '@/components/ui/icons';
import type { FormErrors } from '@/lib/form-validation';
import type { ItemFormData } from './items-form-model';
import { isStockable } from './items-form-parts';

export type Mode = 'cepat' | 'lengkap';
export type SectionId =
  | 'identitas' | 'klasifikasi' | 'media' | 'lampiran' | 'atribut' | 'inventory'
  | 'pergerakanstok' | 'harga' | 'pajak' | 'akuntansi' | 'dimensi' | 'supplier';

export const CEPAT_SECTIONS: SectionId[] = ['identitas', 'klasifikasi'];

type GroupId = 'inti' | 'detail' | 'keuangan' | 'lainnya';

const GROUP_LABEL: Record<GroupId, string> = {
  inti: 'Inti',
  detail: 'Detail',
  keuangan: 'Keuangan',
  lainnya: 'Dimensi & Supplier',
};
const GROUP_ORDER: GroupId[] = ['inti', 'detail', 'keuangan', 'lainnya'];

interface SectionMeta { label: string; icon: IconName; group: GroupId }

const SECTION_META: Record<SectionId, SectionMeta> = {
  identitas: { label: 'Identitas', icon: 'user', group: 'inti' },
  klasifikasi: { label: 'Klasifikasi', icon: 'layers', group: 'inti' },
  media: { label: 'Media', icon: 'eye', group: 'detail' },
  lampiran: { label: 'Lampiran', icon: 'file', group: 'detail' },
  atribut: { label: 'Atribut', icon: 'tag', group: 'detail' },
  inventory: { label: 'Inventory & Tracking', icon: 'box', group: 'detail' },
  pergerakanstok: { label: 'Pergerakan Stok', icon: 'swap', group: 'detail' },
  harga: { label: 'Harga', icon: 'coins', group: 'keuangan' },
  pajak: { label: 'Pajak', icon: 'receipt', group: 'keuangan' },
  akuntansi: { label: 'Akuntansi', icon: 'calculator', group: 'keuangan' },
  dimensi: { label: 'Dimensi GL', icon: 'building', group: 'lainnya' },
  supplier: { label: 'Supplier', icon: 'truck', group: 'lainnya' },
};

const ORDER: SectionId[] = [
  'identitas', 'klasifikasi', 'media', 'lampiran', 'atribut', 'inventory',
  'pergerakanstok', 'harga', 'pajak', 'akuntansi', 'dimensi', 'supplier',
];

const anySet = (...vals: (string | undefined)[]) => vals.some((v) => !!v && v.trim() !== '');

/** Whether the user has put meaningful data into a section (drives the side-nav fill dot). */
function sectionFilled(id: SectionId, d: ItemFormData): boolean {
  switch (id) {
    case 'identitas': return anySet(d.code, d.name, d.kindId);
    case 'klasifikasi': return anySet(d.categoryId, d.unitId);
    case 'atribut': return anySet(
      d.brandId, d.materialId, d.sizeId, d.colorId, d.sectionId, d.designerId,
      d.nozzleId, d.oemId, d.vendorId, d.description,
    ) || Object.keys(d.others ?? {}).length > 0 || Object.keys(d.custom ?? {}).length > 0;
    case 'inventory': return anySet(d.minStock, d.maxStock, d.minOrderQty) || d.warehouseStocks.length > 0 || d.tracksBin;
    case 'pergerakanstok': return d.tracksSerial || d.tracksBatch;
    case 'harga': return d.salePrices.some((v) => anySet(v)) || anySet(d.purchaseDiscount);
    case 'pajak': return anySet(d.purchaseTaxId, d.purchaseTax2Id, d.saleTaxId, d.saleTax2Id);
    case 'akuntansi': return anySet(d.inventoryAccountId, d.salesAccountId, d.cogsAccountId);
    case 'dimensi': return d.branchIds.length > 0 || d.defaultWarehouseIds.length > 0
      || d.defaultLocationIds.length > 0 || anySet(d.divisionId, d.departmentId, d.costCenterId, d.projectId);
    case 'supplier': return anySet(d.primarySupplierId);
    case 'media': return false; // media count not in form state — no fill dot
    case 'lampiran': return false; // attachment count not in form state — no fill dot
    default: return false;
  }
}

export interface SectionDef {
  id: SectionId;
  label: string;
  icon: IconName;
  group: GroupId;
  available: boolean;
  hasError: boolean;
  filled: boolean;
}

/** Build the ordered section list with availability + error + fill state. */
export function buildSections(data: ItemFormData, errors: FormErrors<ItemFormData>): SectionDef[] {
  const accountError = !!(
    errors.inventoryAccountId || errors.salesAccountId || errors.salesReturnAccountId
    || errors.salesDiscountAccountId || errors.cogsAccountId || errors.purchaseReturnAccountId
    || errors.purchaseDiscountAccountId || errors.consignmentAccountId
  );
  const errorById: Partial<Record<SectionId, boolean>> = {
    identitas: !!(errors.code || errors.name),
    klasifikasi: !!(errors.categoryId || errors.unitId),
    akuntansi: accountError,
  };
  return ORDER.map((id) => ({
    id,
    label: SECTION_META[id].label,
    icon: SECTION_META[id].icon,
    group: SECTION_META[id].group,
    available: id === 'inventory' || id === 'pergerakanstok' ? isStockable(data.itemType) : true,
    hasError: !!errorById[id],
    filled: sectionFilled(id, data),
  }));
}

export const hasAccountError = (s: SectionDef[]) => s.find((x) => x.id === 'akuntansi')?.hasError ?? false;

// ─── Mode toggle (header) ────────────────────────────────────────────────────

export function ModeToggle({ mode, onMode }: { mode: Mode; onMode: (m: Mode) => void }) {
  return (
    <div className="flex items-center gap-2 border-b border-border bg-[var(--panel-2)] px-5 py-2">
      <span className="text-[11px] font-medium uppercase tracking-wide text-[var(--fg-subtle)]">Mode entri</span>
      <span className="text-[11px] text-[var(--fg-subtle)]">
        {mode === 'cepat' ? '— hanya field wajib' : '— semua detail item'}
      </span>
      <div className="ml-auto inline-flex overflow-hidden rounded-[var(--radius)] border border-border">
        <button
          type="button"
          onClick={() => onMode('cepat')}
          className={`flex items-center gap-1 px-3 py-1 text-[11px] font-medium transition-colors ${mode === 'cepat' ? 'bg-primary text-primary-foreground' : 'bg-card text-foreground hover:bg-[var(--panel-hover)]'}`}
          title="Tambah kilat: hanya Identitas & Klasifikasi"
        >
          <Icon name="redo" size={11} /> Cepat
        </button>
        <button
          type="button"
          onClick={() => onMode('lengkap')}
          className={`flex items-center gap-1 border-l border-border px-3 py-1 text-[11px] font-medium transition-colors ${mode === 'lengkap' ? 'bg-primary text-primary-foreground' : 'bg-card text-foreground hover:bg-[var(--panel-hover)]'}`}
          title="Semua field, navigasi via side-nav"
        >
          <Icon name="layers" size={11} /> Lengkap
        </button>
      </div>
    </div>
  );
}

// ─── Grouped side-nav ─────────────────────────────────────────────────────────

function NavMarker({ s }: { s: SectionDef }) {
  if (s.hasError) return <span className="ml-2 h-1.5 w-1.5 shrink-0 rounded-full bg-danger" aria-label="Ada error" />;
  if (s.filled) return <span className="ml-2 text-success" aria-label="Terisi"><Icon name="save" size={11} /></span>;
  return <span className="ml-2 h-1.5 w-1.5 shrink-0 rounded-full border border-border" aria-hidden />;
}

export function SectionNav({
  sections, activeId, onSelect,
}: { sections: SectionDef[]; activeId: SectionId; onSelect: (id: SectionId) => void }) {
  return (
    <nav className="w-[210px] shrink-0 overflow-y-auto border-r border-border bg-[var(--panel-2)] py-2" aria-label="Navigasi section form">
      {GROUP_ORDER.map((g) => {
        const items = sections.filter((s) => s.available && s.group === g);
        if (items.length === 0) return null;
        return (
          <div key={g} className="mb-1">
            <p className="px-4 pb-1 pt-2 text-[10px] font-semibold uppercase tracking-wider text-[var(--fg-subtle)]">{GROUP_LABEL[g]}</p>
            {items.map((s) => {
              const active = s.id === activeId;
              return (
                <button
                  key={s.id}
                  type="button"
                  onClick={() => onSelect(s.id)}
                  aria-current={active ? 'page' : undefined}
                  className={`relative flex w-full items-center gap-2 py-1.5 pl-4 pr-3 text-left text-xs transition-colors ${active ? 'bg-card font-medium text-foreground' : 'text-[var(--fg-muted)] hover:bg-[var(--panel-hover)] hover:text-foreground'}`}
                >
                  {active && <span className="absolute inset-y-0 left-0 w-[2px] bg-primary" aria-hidden />}
                  <Icon name={s.icon} size={13} />
                  <span className="flex-1 truncate">{s.label}</span>
                  <NavMarker s={s} />
                </button>
              );
            })}
          </div>
        );
      })}
    </nav>
  );
}
