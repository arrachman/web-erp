'use client';

/**
 * Item form "Lain-lain" + "Custom" sections (legacy MyERP+ tab parity).
 * Both write into the JSON sidecar `metadata.others` / `metadata.custom`
 * (no dedicated columns, §2.38) — so they need no schema/migration.
 *  - Lain-lain: Nama Alias 1–4, Notes RC, Catatan.
 *  - Custom: production/moulding attributes (Kategori/Kelompok Produksi,
 *    Max Qty SO/RC, Kapasitas Per Jam, Allowance, WIP 1–3, Mould Finish,
 *    Mold Semi 1–2, MIN/MAX 1–2).
 * Atomic tier: Organism.
 */

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import type { ItemOthersData, ItemCustomData } from '@/lib/api/items';
import type { ItemFormData } from './items-form';
import { Section, NumField, LookupField } from './items-form-parts';
import { loadProductClassOptions } from './items-form-lookups';

type SectionProps = { data: ItemFormData; onChange: (d: ItemFormData) => void };

/** Labelled free-text field (mirrors the inline pattern used in items-form-fields). */
function TextField({
  id, label, value, onChange, full,
}: { id: string; label: string; value: string; onChange: (v: string) => void; full?: boolean }) {
  return (
    <FormField label={label} htmlFor={id} className={full ? 'col-span-2 grid-cols-[110px_1fr]' : undefined}>
      <Input id={id} value={value} onChange={(e) => onChange(e.target.value)} placeholder="Opsional" />
    </FormField>
  );
}

export function ItemLainLainSection({ data, onChange }: SectionProps) {
  const o = data.others;
  const setOther = (k: keyof ItemOthersData, v: string) =>
    onChange({ ...data, others: { ...data.others, [k]: v } });

  return (
    <Section title="Lain-lain" hint="Nama alias & catatan tambahan (paritas MyERP+)">
      <TextField id="if-alias1" label="Nama Alias 1" value={o.aliasName1 ?? ''} onChange={(v) => setOther('aliasName1', v)} />
      <TextField id="if-alias2" label="Nama Alias 2" value={o.aliasName2 ?? ''} onChange={(v) => setOther('aliasName2', v)} />
      <TextField id="if-alias3" label="Nama Alias 3" value={o.aliasName3 ?? ''} onChange={(v) => setOther('aliasName3', v)} />
      <TextField id="if-alias4" label="Nama Alias 4" value={o.aliasName4 ?? ''} onChange={(v) => setOther('aliasName4', v)} />
      <TextField id="if-notesrc" label="Notes RC" value={o.notesRc ?? ''} onChange={(v) => setOther('notesRc', v)} full />
      <TextField id="if-catatan" label="Catatan" value={o.catatan ?? ''} onChange={(v) => setOther('catatan', v)} full />
    </Section>
  );
}

export function ItemCustomSection({ data, onChange }: SectionProps) {
  const c = data.custom;
  const setCustom = (k: keyof ItemCustomData, v: string) =>
    onChange({ ...data, custom: { ...data.custom, [k]: v } });

  return (
    <Section title="Custom" hint="Atribut produksi & moulding (paritas MyERP+)">
      <LookupField id="if-pclass" label="Kelas Produk" value={data.productClassId} onPick={(v) => onChange({ ...data, productClassId: v })} loader={loadProductClassOptions} placeholder="Pilih kelas…" initialLabel={data.productClassLabel} />
      <TextField id="if-prodcat" label="Kategori Produksi" value={c.productionCategory ?? ''} onChange={(v) => setCustom('productionCategory', v)} />
      <TextField id="if-prodgrp" label="Kelompok Produksi" value={c.productionGroup ?? ''} onChange={(v) => setCustom('productionGroup', v)} />
      <NumField id="if-maxqtyso" label="Max Qty SO" value={c.maxQtySo ?? ''} onChange={(v) => setCustom('maxQtySo', v)} />
      <NumField id="if-capph" label="Kapasitas Per Jam" value={c.capacityPerHour ?? ''} onChange={(v) => setCustom('capacityPerHour', v)} />
      <NumField id="if-maxqtyrc" label="Max Qty RC" value={c.maxQtyRc ?? ''} onChange={(v) => setCustom('maxQtyRc', v)} />
      <NumField id="if-allowance" label="Allowance" value={c.allowance ?? ''} onChange={(v) => setCustom('allowance', v)} />
      <TextField id="if-wip1" label="WIP 1" value={c.wip1 ?? ''} onChange={(v) => setCustom('wip1', v)} />
      <TextField id="if-wip2" label="WIP 2" value={c.wip2 ?? ''} onChange={(v) => setCustom('wip2', v)} />
      <TextField id="if-wip3" label="WIP 3" value={c.wip3 ?? ''} onChange={(v) => setCustom('wip3', v)} />
      <TextField id="if-mouldfin" label="Mould Finish" value={c.mouldFinish ?? ''} onChange={(v) => setCustom('mouldFinish', v)} />
      <TextField id="if-moldsemi1" label="Mold Semi 1" value={c.moldSemi1 ?? ''} onChange={(v) => setCustom('moldSemi1', v)} />
      <TextField id="if-moldsemi2" label="Mold Semi 2" value={c.moldSemi2 ?? ''} onChange={(v) => setCustom('moldSemi2', v)} />
      <TextField id="if-min1" label="MIN1" value={c.min1 ?? ''} onChange={(v) => setCustom('min1', v)} />
      <TextField id="if-max1" label="MAX1" value={c.max1 ?? ''} onChange={(v) => setCustom('max1', v)} />
      <TextField id="if-min2" label="MIN2" value={c.min2 ?? ''} onChange={(v) => setCustom('min2', v)} />
      <TextField id="if-max2" label="MAX2" value={c.max2 ?? ''} onChange={(v) => setCustom('max2', v)} />
    </Section>
  );
}
