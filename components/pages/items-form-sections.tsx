'use client';

/**
 * Item form section bodies. Each `SectionId` maps to one panel of fields.
 * Pure presentation over `ItemFormData` + `onChange`; nav/mode/orchestration
 * live in items-form-fields. Atomic tier: Organism.
 */

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { BooleanRadio } from '@/components/ui/radio-group';
import { DescriptiveRadioCards } from '@/components/molecules/descriptive-radio-cards';
import { ItemTypeInfoButton, getItemTypeTraits } from '@/components/molecules/item-type-info';
import type { FormErrors } from '@/lib/form-validation';
import { nextCodePreview } from '@/lib/items-code-generator';
import type { ItemFormData } from './items-form-model';
import {
  ITEM_STOCK_TRACKING_OPTIONS,
  ITEM_TYPES,
  stockTrackingFlagsFromMode,
  stockTrackingModeFromFlags,
  type ItemStockTrackingMode,
} from './items-form-model';
import { Section, LookupField, MultiLookupField, NumField, YesNoField } from './items-form-parts';
import { ItemMediaUpload } from '@/components/organisms/item-media-upload';
import { ItemAttachmentUpload } from '@/components/organisms/item-attachment-upload';
import { ItemWarehouseStocksEditor } from './items-form-warehouse-stocks';
import { ItemAtributSection } from './items-form-atribut';
import { ItemLainLainSection, ItemCustomSection } from './items-form-lainlain';
import type { SectionId } from './items-form-nav';
import {
  loadCategoryOptions, loadUnitOptions, loadKindOptions,
  loadDivisionOptions, loadSubDivisionOptions, loadDepartmentOptions, loadSubDepartmentOptions,
  loadBranchOptions, loadLocationOptions, loadWarehouseOptions, loadProjectOptions,
  loadCostCenterOptions, loadAccountOptions, loadTaxOptions, loadPartnerOptions,
} from './items-form-lookups';

export interface SectionBodyProps {
  data: ItemFormData;
  onChange: (d: ItemFormData) => void;
  errors: FormErrors<ItemFormData>;
  generating: boolean;
  onAutoCode: () => void;
}

export function SectionBody({ id, ...p }: SectionBodyProps & { id: SectionId }) {
  const { data, onChange, errors, generating, onAutoCode } = p;
  const set = (k: keyof ItemFormData, v: string | boolean) => onChange({ ...data, [k]: v });
  const setTier = (arr: 'salePrices' | 'saleDiscounts', i: number, v: string) => {
    const next = [...data[arr]];
    next[i] = v;
    onChange({ ...data, [arr]: next });
  };
  const addTier = () =>
    onChange({ ...data, salePrices: [...data.salePrices, ''], saleDiscounts: [...data.saleDiscounts, ''] });
  const removeLastTier = () => {
    if (data.salePrices.length <= 1) return;
    onChange({
      ...data,
      salePrices: data.salePrices.slice(0, -1),
      saleDiscounts: data.saleDiscounts.slice(0, -1),
    });
  };
  const taxFields: Array<{
    id: string;
    label: string;
    valueKey: keyof ItemFormData;
    labelKey: keyof ItemFormData;
  }> = [
    // Kiri: Pajak Beli 1 → 2. Kanan: Pajak Jual 1 → 2 (grid 2-kolom mengalir kiri→kanan).
    { id: 'if-buytax-1', label: 'Pajak Beli 1', valueKey: 'purchaseTaxId', labelKey: 'purchaseTaxLabel' },
    { id: 'if-selltax-1', label: 'Pajak Jual 1', valueKey: 'saleTaxId', labelKey: 'saleTaxLabel' },
    { id: 'if-buytax-2', label: 'Pajak Beli 2', valueKey: 'purchaseTax2Id', labelKey: 'purchaseTax2Label' },
    { id: 'if-selltax-2', label: 'Pajak Jual 2', valueKey: 'saleTax2Id', labelKey: 'saleTax2Label' },
  ];

  switch (id) {
    case 'identitas':
      return (
        <Section title="Identitas" icon="user" hint="Kode, nama, dan status item">
          <FormField label="Kode" htmlFor="if-code" required error={errors.code}>
            <div className="flex w-full items-center gap-1.5">
              <Input id="if-code" value={data.code} onChange={(e) => set('code', e.target.value)} placeholder={nextCodePreview(data.itemType)} aria-invalid={!!errors.code} />
              <button type="button" onClick={onAutoCode} disabled={generating} className="shrink-0 rounded-[var(--radius)] border border-border bg-[var(--panel-2)] px-2 py-1 text-[11px] font-medium hover:bg-[var(--panel-hover)] disabled:opacity-50" title={`Isi otomatis (${nextCodePreview(data.itemType)})`}>
                {generating ? '…' : 'Auto'}
              </button>
            </div>
          </FormField>
          <FormField label="Nama" htmlFor="if-name" required error={errors.name}>
            <Input id="if-name" value={data.name} onChange={(e) => set('name', e.target.value)} placeholder="Baja Plat 2mm" aria-invalid={!!errors.name} />
          </FormField>
          <FormField label="Status" htmlFor="if-active">
            <BooleanRadio id="if-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
          </FormField>
        </Section>
      );

    case 'klasifikasi':
      return (
        <Section title="Klasifikasi" icon="layers" hint="Tipe, kategori, satuan, dan jenis barang">
          <div className="grid grid-cols-[110px_1fr] items-center gap-x-3 py-[5px]">
            <div className="flex items-center gap-1">
              <Label htmlFor="if-type" required>Tipe</Label>
              <ItemTypeInfoButton currentType={data.itemType} />
            </div>
            <div className="relative flex items-center">
              <Select value={data.itemType} onValueChange={(v) => set('itemType', v)}>
                <SelectTrigger id="if-type"><SelectValue /></SelectTrigger>
                <SelectContent>{ITEM_TYPES.map((t) => <SelectItem key={t} value={t}>{t}</SelectItem>)}</SelectContent>
              </Select>
            </div>
            <p className="col-start-2 pt-0.5 text-[11px] text-[var(--fg-subtle)]">{getItemTypeTraits(data.itemType)}</p>
          </div>
          <LookupField id="if-cat" label="Kategori" value={data.categoryId} onPick={(v) => set('categoryId', v)} loader={loadCategoryOptions} placeholder="Pilih kategori…" required initialLabel={data.categoryLabel} error={!!errors.categoryId} />
          <LookupField id="if-unit" label="Satuan" value={data.unitId} onPick={(v) => set('unitId', v)} loader={loadUnitOptions} placeholder="Pilih satuan…" required initialLabel={data.unitLabel} error={!!errors.unitId} />
          <LookupField id="if-kind" label="Jenis Barang" value={data.kindId} onPick={(v) => set('kindId', v)} loader={loadKindOptions} placeholder="Pilih jenis…" initialLabel={data.kindLabel} />
        </Section>
      );

    case 'media':
      return (
        <Section title="Media" icon="eye" hint="Gambar produk + video pendek; tersimpan langsung saat diunggah">
          <div className="col-span-2"><ItemMediaUpload itemId={data.id || null} /></div>
        </Section>
      );

    case 'lampiran':
      return (
        <Section title="Lampiran" icon="file" hint="Dokumen pendukung (datasheet, sertifikat, kontrak); tersimpan langsung saat diunggah">
          <div className="col-span-2"><ItemAttachmentUpload itemId={data.id || null} /></div>
        </Section>
      );

    case 'atribut':
      return (
        <>
          <ItemAtributSection data={data} onChange={onChange} />
          <ItemLainLainSection data={data} onChange={onChange} />
          <ItemCustomSection data={data} onChange={onChange} />
          <Section title="Catatan" icon="file">
            <FormField label="Deskripsi" htmlFor="if-desc" className="col-span-2 grid-cols-[110px_1fr]">
              <Input id="if-desc" value={data.description} onChange={(e) => set('description', e.target.value)} placeholder="Opsional" />
            </FormField>
          </Section>
        </>
      );

    case 'inventory':
      return (
        <Section title="Inventory & Tracking" icon="box" hint="Stok Min/Maks + Min Order di bawah = nilai global (default semua gudang)">
          <NumField id="if-minstock" label="Stok Min" value={data.minStock} onChange={(v) => set('minStock', v)} />
          <NumField id="if-maxstock" label="Stok Maks" value={data.maxStock} onChange={(v) => set('maxStock', v)} />
          <NumField id="if-minorder" label="Min Order" value={data.minOrderQty} onChange={(v) => set('minOrderQty', v)} />
          <div className="col-span-2 pt-2">
            <p className="pb-1 text-[11px] font-medium uppercase tracking-wide text-[var(--fg-muted)]">Pengaturan per Gudang</p>
            <p className="pb-1 text-[11px] text-[var(--fg-subtle)]">Override Stok Min/Maks + Min Order untuk gudang tertentu. Kolom kosong = pakai nilai global.</p>
            <ItemWarehouseStocksEditor rows={data.warehouseStocks} onChange={(rows) => onChange({ ...data, warehouseStocks: rows })} />
          </div>
          <YesNoField id="if-bin" label="Bin / Rak" value={data.tracksBin} onChange={(v) => set('tracksBin', v)} />
        </Section>
      );

    case 'pergerakanstok':
      return (
        <Section title="Pergerakan Stok" icon="swap" hint="Pilih bagaimana item dikenali saat stok masuk, dipindah, dan keluar">
          <FormField label="Tracking" className="col-span-2 grid-cols-[110px_1fr]">
            <DescriptiveRadioCards<ItemStockTrackingMode>
              value={stockTrackingModeFromFlags(data.tracksBatch, data.tracksSerial)}
              onValueChange={(mode) => onChange({ ...data, ...stockTrackingFlagsFromMode(mode) })}
              options={ITEM_STOCK_TRACKING_OPTIONS}
              className="w-full"
              aria-label="Cara pencatatan stok"
            />
          </FormField>
        </Section>
      );

    case 'harga':
      return (
        <Section title="Harga" icon="coins" hint="Harga Jual bertingkat + diskon per tingkat — tambah sebanyak yang dibutuhkan">
          <NumField id="if-buy" label="Harga Beli Terakhir" value={data.purchasePrice} onChange={() => {}} readOnly help="Otomatis dari transaksi pembelian terakhir" />
          <NumField id="if-lasthpp" label="HPP Terakhir" value={data.lastHpp} onChange={() => {}} readOnly help="Otomatis dari transaksi pembelian terakhir" />
          <NumField id="if-buydisc" label="Diskon Pembelian" value={data.purchaseDiscount} onChange={(v) => set('purchaseDiscount', v)} placeholder="0" decimals={2} help="Persen (%) · jadi default diskon di PR/PO/RI/PRT" />
          {data.salePrices.map((_, i) => (
            <React.Fragment key={i}>
              <NumField id={`if-sell-${i}`} label={`Harga Jual ${i + 1}`} value={data.salePrices[i] ?? ''} onChange={(v) => setTier('salePrices', i, v)} />
              <NumField id={`if-selldisc-${i}`} label={`Diskon Jual ${i + 1}`} value={data.saleDiscounts[i] ?? ''} onChange={(v) => setTier('saleDiscounts', i, v)} placeholder="0" decimals={2} />
            </React.Fragment>
          ))}
          <div className="col-span-2 flex items-center gap-1.5 pt-2">
            <button type="button" onClick={addTier} className="shrink-0 rounded-[var(--radius)] border border-border bg-[var(--panel-2)] px-2 py-1 text-[11px] font-medium hover:bg-[var(--panel-hover)]" title="Tambah tingkat harga">
              + Tambah tingkat
            </button>
            <button type="button" onClick={removeLastTier} disabled={data.salePrices.length <= 1} className="shrink-0 rounded-[var(--radius)] border border-border bg-[var(--panel-2)] px-2 py-1 text-[11px] font-medium hover:bg-[var(--panel-hover)] disabled:opacity-50" title="Hapus tingkat terakhir">
              − Hapus tingkat
            </button>
            <span className="text-[11px] text-[var(--fg-subtle)]">{data.salePrices.length} tingkat</span>
          </div>
        </Section>
      );

    case 'pajak':
      return (
        <Section title="Pajak" icon="receipt">
          {taxFields.map((field) => (
            <LookupField
              key={field.id}
              id={field.id}
              label={field.label}
              value={String(data[field.valueKey] ?? '')}
              onPick={(v) => set(field.valueKey, v)}
              loader={loadTaxOptions}
              placeholder="Pilih pajak…"
              initialLabel={String(data[field.labelKey] ?? '')}
            />
          ))}
        </Section>
      );

    case 'akuntansi': {
      const req = data.itemType === 'INVENTORY';
      return (
        <Section title="Akuntansi" icon="calculator" hint={req ? 'Wajib diisi untuk item Inventory' : 'Opsional untuk tipe non-Inventory'}>
          <LookupField id="if-acc-inv" label="Persediaan" value={data.inventoryAccountId} onPick={(v) => set('inventoryAccountId', v)} loader={loadAccountOptions} placeholder="Pilih akun…" initialLabel={data.inventoryAccountLabel} required={req} error={!!errors.inventoryAccountId} />
          <LookupField id="if-acc-sales" label="Penjualan" value={data.salesAccountId} onPick={(v) => set('salesAccountId', v)} loader={loadAccountOptions} placeholder="Pilih akun…" initialLabel={data.salesAccountLabel} required={req} error={!!errors.salesAccountId} />
          <LookupField id="if-acc-sret" label="Retur Penjualan" value={data.salesReturnAccountId} onPick={(v) => set('salesReturnAccountId', v)} loader={loadAccountOptions} placeholder="Pilih akun…" initialLabel={data.salesReturnAccountLabel} required={req} error={!!errors.salesReturnAccountId} />
          <LookupField id="if-acc-sdisc" label="Diskon Penjualan" value={data.salesDiscountAccountId} onPick={(v) => set('salesDiscountAccountId', v)} loader={loadAccountOptions} placeholder="Pilih akun…" initialLabel={data.salesDiscountAccountLabel} required={req} error={!!errors.salesDiscountAccountId} />
          <LookupField id="if-acc-cogs" label="HPP" value={data.cogsAccountId} onPick={(v) => set('cogsAccountId', v)} loader={loadAccountOptions} placeholder="Pilih akun…" initialLabel={data.cogsAccountLabel} required={req} error={!!errors.cogsAccountId} />
          <LookupField id="if-acc-pret" label="Retur Pembelian" value={data.purchaseReturnAccountId} onPick={(v) => set('purchaseReturnAccountId', v)} loader={loadAccountOptions} placeholder="Pilih akun…" initialLabel={data.purchaseReturnAccountLabel} required={req} error={!!errors.purchaseReturnAccountId} />
          <LookupField id="if-acc-pdisc" label="Diskon Pembelian" value={data.purchaseDiscountAccountId} onPick={(v) => set('purchaseDiscountAccountId', v)} loader={loadAccountOptions} placeholder="Pilih akun…" initialLabel={data.purchaseDiscountAccountLabel} required={req} error={!!errors.purchaseDiscountAccountId} />
          <LookupField id="if-acc-cons" label="Konsinyasi" value={data.consignmentAccountId} onPick={(v) => set('consignmentAccountId', v)} loader={loadAccountOptions} placeholder="Pilih akun…" initialLabel={data.consignmentAccountLabel} required={req} error={!!errors.consignmentAccountId} />
        </Section>
      );
    }

    case 'dimensi':
      return (
        <Section title="Dimensi GL" icon="building">
          <MultiLookupField id="if-branch" label="Cabang" values={data.branchIds} labels={data.branchLabels} onChange={(ids, labels) => onChange({ ...data, branchIds: ids, branchLabels: labels })} loader={loadBranchOptions} placeholder="Pilih cabang…" />
          <MultiLookupField id="if-wh" label="Gudang Default" values={data.defaultWarehouseIds} labels={data.defaultWarehouseLabels} onChange={(ids, labels) => onChange({ ...data, defaultWarehouseIds: ids, defaultWarehouseLabels: labels })} loader={loadWarehouseOptions} placeholder="Pilih gudang…" />
          <MultiLookupField id="if-loc" label="Lokasi Default" values={data.defaultLocationIds} labels={data.defaultLocationLabels} onChange={(ids, labels) => onChange({ ...data, defaultLocationIds: ids, defaultLocationLabels: labels })} loader={loadLocationOptions} placeholder="Pilih lokasi…" />
          <LookupField id="if-div" label="Divisi" value={data.divisionId} onPick={(v) => set('divisionId', v)} loader={loadDivisionOptions} placeholder="Pilih divisi…" initialLabel={data.divisionLabel} />
          <LookupField id="if-subdiv" label="Sub Divisi" value={data.subdivisionId} onPick={(v) => set('subdivisionId', v)} loader={loadSubDivisionOptions} placeholder="Pilih sub divisi…" initialLabel={data.subdivisionLabel} />
          <LookupField id="if-dept" label="Departemen" value={data.departmentId} onPick={(v) => set('departmentId', v)} loader={loadDepartmentOptions} placeholder="Pilih departemen…" initialLabel={data.departmentLabel} />
          <LookupField id="if-subdept" label="Sub Departemen" value={data.subDepartmentId} onPick={(v) => set('subDepartmentId', v)} loader={loadSubDepartmentOptions} placeholder="Pilih sub departemen…" initialLabel={data.subDepartmentLabel} />
          <LookupField id="if-cc" label="Cost Center" value={data.costCenterId} onPick={(v) => set('costCenterId', v)} loader={loadCostCenterOptions} placeholder="Pilih cost center…" initialLabel={data.costCenterLabel} />
          <LookupField id="if-proj" label="Proyek" value={data.projectId} onPick={(v) => set('projectId', v)} loader={loadProjectOptions} placeholder="Pilih proyek…" initialLabel={data.projectLabel} />
        </Section>
      );

    case 'supplier':
      return (
        <Section title="Supplier" icon="truck">
          <LookupField id="if-supplier" label="Supplier Utama" value={data.primarySupplierId} onPick={(v) => set('primarySupplierId', v)} loader={loadPartnerOptions} placeholder="Pilih supplier…" initialLabel={data.primarySupplierLabel} />
        </Section>
      );

    default:
      return null;
  }
}
