'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { Card, CardHeader, CardTitle, CardSubtitle, CardBody } from '@/components/ui/card';
import { Icon, type IconName } from '@/components/ui/icons';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/tabs';
import { SearchSelect } from '@/components/molecules/search-select';
import { NumInput } from '@/components/molecules/num-input';
import { PartnerContactsEditor } from '@/components/organisms/partner-contacts-editor';
import { PartnerAddressesEditor } from '@/components/organisms/partner-addresses-editor';
import { MultiLookupField } from './items-form-parts';
import {
  loadBranchOptions,
  loadWarehouseOptions,
  loadLocationOptions,
  loadCustomerCategoryOptions,
  loadSupplierCategoryOptions,
  loadSalesmanCategoryOptions,
  loadSalesmanPartnerOptions,
  loadPartnerTypeOptions,
} from './items-form-lookups';
import { loadPaymentTermOptions } from './pur-form-lookups';
import { loadReceivableAccounts, loadPayableAccounts, loadCurrencyOptions } from './partners-lookups';
import type { FormErrors } from '@/lib/form-validation';
import type { PartnerForm } from './partners-form-types';

/** Titled, icon-led card used to group related transaksi fields. */
function TrxSection({
  icon,
  title,
  subtitle,
  children,
}: {
  icon: IconName;
  title: string;
  subtitle?: string;
  children: React.ReactNode;
}) {
  return (
    <Card>
      <CardHeader>
        <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-[var(--radius)] bg-[var(--primary-soft)] text-[var(--primary-soft-fg)]">
          <Icon name={icon} size={15} />
        </span>
        <div className="min-w-0">
          <CardTitle>{title}</CardTitle>
          {subtitle && <CardSubtitle>{subtitle}</CardSubtitle>}
        </div>
      </CardHeader>
      <CardBody className="py-2">{children}</CardBody>
    </Card>
  );
}

export function PartnerFormFields({
  data,
  onChange,
  errors = {},
}: {
  data: PartnerForm;
  onChange: (d: PartnerForm) => void;
  errors?: FormErrors<PartnerForm>;
}) {
  const set = <K extends keyof PartnerForm>(k: K, v: PartnerForm[K]) =>
    onChange({ ...data, [k]: v });

  const showCustomerFields = data.partnerTypeKind === 'CUSTOMER';
  const showSupplierFields = data.partnerTypeKind === 'SUPPLIER';
  const showSalesmanFields = data.partnerTypeKind === 'SALESMAN';

  const savedHint = (
    <div className="p-4 text-[12.5px] text-muted-foreground">
      Simpan partner terlebih dahulu untuk menambah kontak &amp; alamat.
    </div>
  );

  return (
    <Tabs defaultValue="umum">
      <TabsList>
        <TabsTrigger value="umum">Umum</TabsTrigger>
        <TabsTrigger value="transaksi">Transaksi</TabsTrigger>
        <TabsTrigger value="kontak">Kontak</TabsTrigger>
        <TabsTrigger value="alamat">Alamat</TabsTrigger>
      </TabsList>

      <TabsContent value="umum">
        <div className="p-4">
          <FormField label="Kode" htmlFor="pf-code" required error={errors.code}>
            <Input
              id="pf-code"
              value={data.code}
              onChange={(e) => set('code', e.target.value)}
              placeholder="CUST-001"
              aria-invalid={!!errors.code}
            />
          </FormField>
          <FormField label="Nama" htmlFor="pf-name" required error={errors.name}>
            <Input
              id="pf-name"
              value={data.name}
              onChange={(e) => set('name', e.target.value)}
              placeholder="PT Maju Bersama"
              aria-invalid={!!errors.name}
            />
          </FormField>
          <FormField label="Tipe" htmlFor="pf-type" required error={errors.partnerTypeId}>
            <SearchSelect
              id="pf-type"
              value={data.partnerTypeId}
              onValueChange={(v) => {
                if (v) {
                  set('partnerTypeId', v);
                  return;
                }
                onChange({
                  ...data,
                  partnerTypeId: '',
                  partnerTypeLabel: '',
                  partnerTypeKind: '',
                  customerCategoryId: '',
                  customerCategoryLabel: '',
                  supplierCategoryId: '',
                  supplierCategoryLabel: '',
                  salesmanCategoryId: '',
                  salesmanCategoryLabel: '',
                  salesmanId: '',
                  salesmanLabel: '',
                });
              }}
              onPick={(opt) => onChange({
                ...data,
                partnerTypeId: opt.value,
                partnerTypeLabel: opt.label,
                partnerTypeKind: (opt.meta ?? '') as PartnerForm['partnerTypeKind'],
                customerCategoryId: opt.meta === 'CUSTOMER' ? data.customerCategoryId : '',
                customerCategoryLabel: opt.meta === 'CUSTOMER' ? data.customerCategoryLabel : '',
                supplierCategoryId: opt.meta === 'SUPPLIER' ? data.supplierCategoryId : '',
                supplierCategoryLabel: opt.meta === 'SUPPLIER' ? data.supplierCategoryLabel : '',
                salesmanCategoryId: opt.meta === 'SALESMAN' ? data.salesmanCategoryId : '',
                salesmanCategoryLabel: opt.meta === 'SALESMAN' ? data.salesmanCategoryLabel : '',
              })}
              placeholder="Pilih tipe partner…"
              loadOptions={loadPartnerTypeOptions}
              initialLabel={data.partnerTypeLabel}
              title="Tipe Partner"
              aria-invalid={!!errors.partnerTypeId}
            />
          </FormField>
          {showCustomerFields && (
            <FormField label="Kategori Customer" htmlFor="pf-cust-cat">
              <SearchSelect
                id="pf-cust-cat"
                value={data.customerCategoryId}
                onValueChange={(v) => set('customerCategoryId', v)}
                placeholder="Pilih kategori customer…"
                loadOptions={loadCustomerCategoryOptions}
                initialLabel={data.customerCategoryLabel}
                title="Kategori Customer"
              />
            </FormField>
          )}
          {showCustomerFields && (
            <FormField label="Salesman" htmlFor="pf-salesman" required error={errors.salesmanId}>
              <SearchSelect
                id="pf-salesman"
                value={data.salesmanId}
                onValueChange={(v) => set('salesmanId', v)}
                placeholder="Pilih salesman…"
                loadOptions={loadSalesmanPartnerOptions}
                initialLabel={data.salesmanLabel}
                title="Salesman"
                aria-invalid={!!errors.salesmanId}
              />
            </FormField>
          )}
          {showSupplierFields && (
            <FormField label="Kategori Supplier" htmlFor="pf-supp-cat">
              <SearchSelect
                id="pf-supp-cat"
                value={data.supplierCategoryId}
                onValueChange={(v) => set('supplierCategoryId', v)}
                placeholder="Pilih kategori supplier…"
                loadOptions={loadSupplierCategoryOptions}
                initialLabel={data.supplierCategoryLabel}
                title="Kategori Supplier"
              />
            </FormField>
          )}
          {showSalesmanFields && (
            <FormField label="Kategori Salesman" htmlFor="pf-sales-cat">
              <SearchSelect
                id="pf-sales-cat"
                value={data.salesmanCategoryId}
                onValueChange={(v) => set('salesmanCategoryId', v)}
                placeholder="Pilih kategori salesman…"
                loadOptions={loadSalesmanCategoryOptions}
                initialLabel={data.salesmanCategoryLabel}
                title="Kategori Salesman"
              />
            </FormField>
          )}
          <FormField label="NPWP" htmlFor="pf-tax">
            <Input
              id="pf-tax"
              value={data.taxNumber}
              onChange={(e) => set('taxNumber', e.target.value)}
              placeholder="01.234.567.8-901.000"
            />
          </FormField>
          <MultiLookupField
            id="pf-branch"
            label="Cabang"
            values={data.branchIds}
            labels={data.branchLabels}
            onChange={(ids, labels) => onChange({ ...data, branchIds: ids, branchLabels: labels })}
            loader={loadBranchOptions}
            placeholder="Pilih cabang…"
          />
          <MultiLookupField
            id="pf-warehouse"
            label="Gudang"
            values={data.warehouseIds}
            labels={data.warehouseLabels}
            onChange={(ids, labels) => onChange({ ...data, warehouseIds: ids, warehouseLabels: labels })}
            loader={loadWarehouseOptions}
            placeholder="Pilih gudang…"
          />
          <MultiLookupField
            id="pf-location"
            label="Lokasi"
            values={data.locationIds}
            labels={data.locationLabels}
            onChange={(ids, labels) => onChange({ ...data, locationIds: ids, locationLabels: labels })}
            loader={loadLocationOptions}
            placeholder="Pilih lokasi…"
          />
          <FormField label="Status" htmlFor="pf-active">
            <BooleanRadio
              id="pf-active"
              value={data.isActive}
              onValueChange={(v) => set('isActive', v)}
            />
          </FormField>
        </div>
      </TabsContent>

      <TabsContent value="transaksi">
        <div className="flex flex-col gap-4 p-4">
          <TrxSection
            icon="coins"
            title="Mata Uang"
            subtitle="Mata uang default untuk transaksi partner ini."
          >
            <FormField label="Uang" htmlFor="pf-currency">
              <SearchSelect
                id="pf-currency"
                value={data.currencyId}
                onValueChange={(v) => set('currencyId', v)}
                placeholder="Pilih mata uang…"
                loadOptions={loadCurrencyOptions}
                initialLabel={data.currencyLabel}
                title="Mata Uang"
              />
            </FormField>
          </TrxSection>

          <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
            <TrxSection
              icon="cart"
              title="Pembelian"
              subtitle="Syarat saat partner menjadi pemasok (hutang)."
            >
              <FormField label="Termin" htmlFor="pf-pur-term">
                <SearchSelect
                  id="pf-pur-term"
                  value={data.purchaseTermId}
                  onValueChange={(v) => set('purchaseTermId', v)}
                  placeholder="Pilih termin pembelian…"
                  loadOptions={loadPaymentTermOptions}
                  initialLabel={data.purchaseTermLabel}
                  title="Termin Pembelian"
                />
              </FormField>
              <FormField
                label="Batas Hutang"
                htmlFor="pf-ap-limit"
                help="0 = tanpa batas."
              >
                <NumInput
                  id="pf-ap-limit"
                  value={data.apCreditLimit}
                  onChange={(v) => set('apCreditLimit', v)}
                  decimals={2}
                  placeholder="0,00"
                />
              </FormField>
              <FormField label="Rek. Hutang" htmlFor="pf-pay-acct">
                <SearchSelect
                  id="pf-pay-acct"
                  value={data.payableAccountId}
                  onValueChange={(v) => set('payableAccountId', v)}
                  placeholder="Cari akun hutang…"
                  loadOptions={loadPayableAccounts}
                  initialLabel={data.payableAccountLabel}
                  title="Akun Hutang"
                />
              </FormField>
            </TrxSection>

            <TrxSection
              icon="tag"
              title="Penjualan"
              subtitle="Syarat saat partner menjadi pelanggan (piutang)."
            >
              <FormField label="Termin" htmlFor="pf-sale-term">
                <SearchSelect
                  id="pf-sale-term"
                  value={data.saleTermId}
                  onValueChange={(v) => set('saleTermId', v)}
                  placeholder="Pilih termin penjualan…"
                  loadOptions={loadPaymentTermOptions}
                  initialLabel={data.saleTermLabel}
                  title="Termin Penjualan"
                />
              </FormField>
              <FormField
                label="Batas Piutang"
                htmlFor="pf-ar-limit"
                help="0 = tanpa batas."
              >
                <NumInput
                  id="pf-ar-limit"
                  value={data.arCreditLimit}
                  onChange={(v) => set('arCreditLimit', v)}
                  decimals={2}
                  placeholder="0,00"
                />
              </FormField>
              <FormField label="Rek. Piutang" htmlFor="pf-recv-acct">
                <SearchSelect
                  id="pf-recv-acct"
                  value={data.receivableAccountId}
                  onValueChange={(v) => set('receivableAccountId', v)}
                  placeholder="Cari akun piutang…"
                  loadOptions={loadReceivableAccounts}
                  initialLabel={data.receivableAccountLabel}
                  title="Akun Piutang"
                />
              </FormField>
              <FormField
                label="Tingkat Harga"
                htmlFor="pf-price-tier"
                help="Level harga jual 1–10 untuk auto-isi harga."
              >
                <NumInput
                  id="pf-price-tier"
                  value={data.salesPriceTier}
                  onChange={(v) => set('salesPriceTier', v)}
                  decimals={0}
                  placeholder="1"
                />
              </FormField>
            </TrxSection>
          </div>
        </div>
      </TabsContent>

      <TabsContent value="kontak">
        {data.id ? <PartnerContactsEditor partnerId={data.id} /> : savedHint}
      </TabsContent>

      <TabsContent value="alamat">
        {data.id ? <PartnerAddressesEditor partnerId={data.id} /> : savedHint}
      </TabsContent>
    </Tabs>
  );
}
