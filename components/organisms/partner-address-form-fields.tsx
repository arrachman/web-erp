'use client';

/**
 * Cascading location fields (Negara → Provinsi → Kota → Kecamatan → Kelurahan)
 * + remaining contact fields for the partner address add/edit fieldset.
 * Atomic tier: Organism.
 */

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input, Textarea } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { SearchSelect, type SearchSelectProps } from '@/components/molecules/search-select';
import { ADDRESS_TYPE_LABELS } from '@/lib/partner-address-lookups';
import type { ErpAddressType } from '@/lib/api/partners';

export interface DraftAddress {
  type: ErpAddressType;
  addressLine1: string;
  countryId: string;
  provinceId: string;
  cityId: string;
  areaId: string;
  subAreaId: string;
  postalCode: string;
  phone: string;
  fax: string;
  email: string;
  website: string;
  isDefault: boolean;
}

export interface AddressInitialLabels {
  country?: string;
  province?: string;
  city?: string;
  area?: string;
  subArea?: string;
}

export interface PartnerAddressFormFieldsProps {
  draft: DraftAddress;
  setD: (k: keyof DraftAddress, v: string | boolean) => void;
  setDraft: React.Dispatch<React.SetStateAction<DraftAddress>>;
  initialLabels: AddressInitialLabels;
  provinceLoader: SearchSelectProps['loadOptions'];
  cityLoader: SearchSelectProps['loadOptions'];
  areaLoader: SearchSelectProps['loadOptions'];
  subAreaLoader: SearchSelectProps['loadOptions'];
  loadCountryOptions: SearchSelectProps['loadOptions'];
}

export function PartnerAddressFormFields({
  draft,
  setD,
  setDraft,
  initialLabels,
  provinceLoader,
  cityLoader,
  areaLoader,
  subAreaLoader,
  loadCountryOptions,
}: PartnerAddressFormFieldsProps) {
  return (
    <>
      <FormField label="Tipe" htmlFor="pa-type">
        <Select value={draft.type} onValueChange={(v) => setD('type', v as ErpAddressType)}>
          <SelectTrigger id="pa-type"><SelectValue /></SelectTrigger>
          <SelectContent>
            {(Object.keys(ADDRESS_TYPE_LABELS) as ErpAddressType[]).map((t) => (
              <SelectItem key={t} value={t}>{ADDRESS_TYPE_LABELS[t]}</SelectItem>
            ))}
          </SelectContent>
        </Select>
      </FormField>

      <FormField label="Alamat utama" htmlFor="pa-default">
        <BooleanRadio id="pa-default" value={draft.isDefault} onValueChange={(v) => setD('isDefault', v)} trueLabel="Ya" falseLabel="Tidak" />
      </FormField>

      <div className="col-span-2">
        <FormField label="Alamat" htmlFor="pa-line1" required>
          <Textarea id="pa-line1" value={draft.addressLine1} onChange={(e) => setD('addressLine1', e.target.value)} placeholder="Jl. Sudirman No. 1" rows={3} />
        </FormField>
      </div>

      <FormField label="Negara" htmlFor="pa-country">
        <SearchSelect
          id="pa-country"
          value={draft.countryId}
          onValueChange={(v) => setD('countryId', v)}
          onPick={(opt) => {
            setDraft((d) => ({ ...d, countryId: opt.value, provinceId: '', cityId: '', areaId: '', postalCode: '' }));
            // key berubah → Provinsi remount; fokus setelah React flush
            setTimeout(() => document.getElementById('pa-prov')?.focus(), 0);
          }}
          loadOptions={loadCountryOptions}
          placeholder="Pilih negara…"
          title="Negara"
          initialLabel={initialLabels.country}
        />
      </FormField>

      <FormField label="Provinsi" htmlFor="pa-prov">
        <SearchSelect
          key={`prov-${draft.countryId}`}
          id="pa-prov"
          value={draft.provinceId}
          onValueChange={(v) => setD('provinceId', v)}
          onPick={(opt) => {
            setDraft((d) => ({ ...d, provinceId: opt.value, cityId: '', areaId: '', postalCode: '' }));
            setTimeout(() => document.getElementById('pa-city')?.focus(), 0);
          }}
          loadOptions={provinceLoader}
          placeholder="Pilih provinsi…"
          title="Provinsi"
          initialLabel={initialLabels.province}
        />
      </FormField>

      <FormField label="Kota" htmlFor="pa-city">
        <SearchSelect
          key={`city-${draft.provinceId}`}
          id="pa-city"
          value={draft.cityId}
          onValueChange={(v) => setD('cityId', v)}
          onPick={(opt) => {
            setDraft((d) => ({ ...d, cityId: opt.value, areaId: '', postalCode: '' }));
            setTimeout(() => document.getElementById('pa-area')?.focus(), 0);
          }}
          loadOptions={cityLoader}
          placeholder="Pilih kota…"
          title="Kota"
          initialLabel={initialLabels.city}
        />
      </FormField>

      <FormField label="Kecamatan" htmlFor="pa-area">
        <SearchSelect
          key={`area-${draft.cityId}`}
          id="pa-area"
          value={draft.areaId}
          onValueChange={(v) => setD('areaId', v)}
          onPick={(opt) => {
            setDraft((d) => ({ ...d, areaId: opt.value, subAreaId: '', postalCode: opt.meta ? String(opt.meta) : d.postalCode }));
            setTimeout(() => document.getElementById('pa-subarea')?.focus(), 0);
          }}
          loadOptions={areaLoader}
          placeholder="Pilih kecamatan…"
          title="Kecamatan"
          initialLabel={initialLabels.area}
        />
      </FormField>

      <FormField label="Kelurahan" htmlFor="pa-subarea">
        <SearchSelect
          key={`subarea-${draft.areaId}`}
          id="pa-subarea"
          value={draft.subAreaId}
          onValueChange={(v) => setD('subAreaId', v)}
          onPick={(opt) => {
            setDraft((d) => ({ ...d, subAreaId: opt.value, postalCode: opt.meta ? String(opt.meta) : d.postalCode }));
          }}
          loadOptions={subAreaLoader}
          placeholder="Pilih kelurahan…"
          title="Kelurahan"
          initialLabel={initialLabels.subArea}
        />
      </FormField>

      <FormField label="Kode Pos" htmlFor="pa-postal">
        <Input id="pa-postal" value={draft.postalCode} onChange={(e) => setD('postalCode', e.target.value)} placeholder="10220" />
      </FormField>

      <FormField label="No HP" htmlFor="pa-phone">
        <Input id="pa-phone" value={draft.phone} onChange={(e) => setD('phone', e.target.value)} placeholder="021-5551234" />
      </FormField>

      <FormField label="Fax" htmlFor="pa-fax">
        <Input id="pa-fax" value={draft.fax} onChange={(e) => setD('fax', e.target.value)} placeholder="021-5554321" />
      </FormField>

      <FormField label="Email" htmlFor="pa-email">
        <Input id="pa-email" value={draft.email} onChange={(e) => setD('email', e.target.value)} placeholder="info@example.com" />
      </FormField>

      <FormField label="Website" htmlFor="pa-website">
        <Input id="pa-website" value={draft.website} onChange={(e) => setD('website', e.target.value)} placeholder="https://www.example.com" />
      </FormField>
    </>
  );
}
