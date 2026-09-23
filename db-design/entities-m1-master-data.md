# Master Data: Entity Catalog (MVP core)

> Legacy "Master Data" (m1) maps to the **`md_*`** semantic domain per
> [web-erp/CLAUDE.md §1](../CLAUDE.md). No `erp_` prefix, no numeric `m<n>` segment.

Field-level model. Types are Prisma/Postgres (PK/FK = **`BigInt`**, resolved
[README §8](README.md#8-resolved-decisions-2026-05-17) #2). All entities also carry the global
**audit + soft-delete** columns from [README §3](README.md#3-global-conventions) — omitted below.
Every `md_*` master also carries **`legacyCode String?`** (nullable, `@@index`; original
MyERP+ code for CDC/ETL backfill — resolved §8 #7) — omitted per-row below.

Legend: 🔑 business key · ➜ FK · ◆ enum · ○ nullable.

The biggest modernization is **`m1_contact` (128 fields, 4 inline address blocks) →
normalized `Partner` + `PartnerAddress` + `PartnerContact` + `PartnerBankAccount`**.

---

## Organization (`md_*`)

### Branch  → `md_branches`  (legacy `m1_branch`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | `bkode` |
| name | String | `bnama` |
| addressLine1 ○ | String | `balamat1` |
| addressLine2 ○ | String | `balamat2` |
| city ○ | String | `bkota` |
| postalCode ○ | String | `bkodepos` |
| phone ○ | String | `bnotelp` |
| fax ○ | String | `bnofax` |
| notes ○ | String | `bcatatan` |
| isActive | Boolean | `baktif` |

Relations: `locations Location[]`.

### Location  → `md_locations`  (legacy `m1_location`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | `lkode` |
| name | String | `lnama` |
| branchId ➜ | BigInt → Branch | `lcabang` |
| addressLine1 ○ | String | `lalamat1` |
| city ○ | String | `lkota` |
| postalCode ○ | String | `lkodepos` |
| phone ○ | String | `lnotelp` |
| notes ○ | String | `lcatatan` |
| isActive | Boolean | `laktif` |

Relations: `branch Branch`, `warehouses Warehouse[]`.

### Warehouse  → `md_warehouses`  (legacy `m1_warehouse`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | `wkode` |
| name | String | `wnama` |
| locationId ➜ | BigInt → Location | `wlokasi` |
| allowNegativeStock | Boolean | policy flag (new; legacy implicit) |
| notes ○ | String | `wcatatan` |
| isActive | Boolean | `waktif` |

> Legacy `wdivisi` (division) intentionally dropped from MVP — org hierarchy deferred.

---

## Items (`md_*`)

### Unit  → `md_units`  (legacy `m1_unit`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | `ukode` |
| name | String | `unama` |
| conversionFactor | Decimal(19,4) | factor to base unit (`unilai`) |
| notes ○ | String | `uketerangan` |
| isActive | Boolean | `uaktif` |

> Alternate units per item (multi-UOM) → future `ItemUnit` table; MVP = one base unit/item.

### ItemCategory  → `md_item_categories`  (legacy `m1_item_category`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | `ickode` |
| name | String | `icnama` |
| parentId ○ ➜ | BigInt → ItemCategory | nesting (modern; legacy flat) |
| inventoryAccountId ○ ➜ | BigInt → Account | default GL (`icrekpersediaan`) |
| cogsAccountId ○ ➜ | BigInt → Account | default GL (`icrekhargapokok`) |
| salesAccountId ○ ➜ | BigInt → Account | default GL (`icrekpenjualan`) |
| salesReturnAccountId ○ ➜ | BigInt → Account | default GL retur penjualan (2026-06-12, paritas 8-akun legacy) |
| salesDiscountAccountId ○ ➜ | BigInt → Account | default GL diskon penjualan |
| purchaseReturnAccountId ○ ➜ | BigInt → Account | default GL retur pembelian |
| purchaseDiscountAccountId ○ ➜ | BigInt → Account | default GL diskon pembelian |
| consignmentAccountId ○ ➜ | BigInt → Account | default GL konsinyasi |
| isActive | Boolean | `icaktif` |

Relations: self `parent`/`children`, `items Item[]`, GL accounts.

### ItemKind  → `md_item_types`  (legacy `m1_item_type` — "Tipe Produk")

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | legacy `Kode` (RM, WIP, FG, FA, MRO, ORM, UM, WO) |
| name | String | legacy `Nama` (RAW MATERIAL, FINISH GOODS, …) |
| isActive | Boolean | |
| metadata ○ | Json | |

> **Naming caveat (resolved §8 #?, ref CLAUDE.md §2.18):** model = `ErpItemKind`,
> **bukan** `ErpItemType` — `ErpItemType` sudah dipakai sebagai *enum* di
> `ErpItem.type`. Tabel ini = master user-configurable yang mereplikasi legacy
> "Tipe Produk" (peran bisnis / manufacturing stage), bukan enum perilaku sistem.

Relations: `items Item[]`.

### Item classification — 3 sumbu independen (resolved 2026-05-24, dengan user)

Satu item dilihat dari **tiga sudut pandang berbeda**, masing-masing kolom
sendiri (legacy MyERP+ cuma punya 2: Tipe Produk + Kategori Produk):

| Sumbu | Kolom | Jenis | Menjawab | Contoh |
| --- | --- | --- | --- | --- |
| **Sistem** | `type` | enum `ErpItemType` | bagaimana sistem memperlakukan item (stockable? hit COGS/expense? punya BOM?) | INVENTORY, SERVICE |
| **Peran bisnis / stage** | `kindId ➜ ItemKind` | master | posisi di alur bisnis/produksi | RM, WIP, FG, FA, MRO, WO |
| **Material / klasifikasi** | `categoryId ➜ ItemCategory` | master (hierarkis) | terbuat dari apa / kelompok produk | BRASS, ALLOY, CHEMICAL |

- **Sumbu sistem (`type`)** = enum kecil & tetap → drives logika akuntansi/inventory.
- **Sumbu peran (`kind`)** = `md_item_types`, replikasi legacy "Tipe Produk", user-extend.
- **Sumbu material (`category`)** = `md_item_categories`, replikasi legacy "Kategori Produk", mendukung nesting via `parentId`.
- **Validasi cross-field (UI, hard rule):** kombinasi `type` × `kind` yang tidak masuk akal harus diblok di form (mis. `type=SERVICE` tidak boleh `kind=RAW MATERIAL`). State machine validasi hidup di backend; form menampilkan error.

> **RESOLVED 2026-05-24 (dengan user) — enum `ErpItemType` = set sifat stok/aset:**
> `INVENTORY · SERVICE · CONSUMABLE · ASSET · NON_INVENTORY`. Nilai lama
> `VOUCHER`/`ASSEMBLY` **dihapus** — keduanya redundan dengan model 3-sumbu:
> "ber-BOM/rakitan" = fakta terpisah (ada baris `mfg_boms`), bukan tipe; "voucher"
> = `NON_INVENTORY`. Backfill data seed lama: `VOUCHER→NON_INVENTORY`,
> `ASSEMBLY→INVENTORY` (peran rakitan pindah ke `kind`, mis. FG/WIP).
>
> **✅ IMPLEMENTED 2026-05-24** — migrasi enum
> `20260524_002_erp_item_type_enum_reshape` (recreate type Postgres + USING
> backfill; 600 seed rows = INVENTORY, no-op). FE `ErpItemType` di
> `lib/api/items.ts`, `ITEM_TYPES`, dan filter di `items-page.tsx` sudah memakai
> set baru — backend & FE kini konsisten.

### Item  → `md_items`  (legacy `m1_item` — trimmed 128 → ~24 core)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | `bkode` |
| name | String | `bnama` |
| barcode ○ | String | `bbarcode` |
| type ◆ | `ErpItemType` | sumbu **sistem** — INVENTORY/SERVICE/CONSUMABLE/ASSET/NON_INVENTORY |
| costMethod ◆ | `ErpCostingMethod` | AVG/FIFO/STD — legacy "HPP" (`bmetodehpp`), default AVG |
| kindId ○ ➜ | BigInt → ItemKind | sumbu **peran/stage** — legacy "Tipe Produk" (`bjenis`/`btipe`) |
| productClassId ○ ➜ | BigInt → ProductClass | legacy "Kelas Produk" (`bkelasproduk`) |
| brandId ○ ➜ | BigInt → Brand | atribut (legacy `bmerk`) |
| materialId ○ ➜ | BigInt → Material | atribut |
| itemModelId ○ ➜ | BigInt → ItemModel | atribut |
| sizeId ○ ➜ | BigInt → Size | atribut |
| colorId ○ ➜ | BigInt → Color | atribut |
| sectionId ○ ➜ | BigInt → Section | atribut |
| categoryId ➜ | BigInt → ItemCategory | sumbu **material** — legacy "Kategori Produk" (`bkategori`) |
| baseUnitId ➜ | BigInt → Unit | `bsatuan` |
| standardCost | Decimal(19,4) | `bhpp` — "Hpp Update" (manual HPP) |
| averageCost | Decimal(19,4) | `bhppaverage` — "Hpp rata-rata" (computed, readonly) |
| purchasePrice | Decimal(19,4) | `bhargabeli` — "Harga Beli Terakhir" |
| purchaseDiscount | Decimal(9,4) | "Diskon Pembelian" (persen) — **implemented 2026-05-30** |
| salePrice | Decimal(19,4) | `bhargajual1` — cache denormalized level-1 dari `md_item_prices` |
| minStock | Decimal(19,4) | `bstokminimal` |
| maxStock | Decimal(19,4) | `bstokmaksimal` |
| reorderQty | Decimal(19,4) | `breorder` |
| minOrderQty | Decimal(19,4) | legacy "Min Order" (`bminorder`) |
| tracksSerial | Boolean | `bserial` → `inv_serials` (resolved §8 #24) |
| tracksBatch | Boolean | `bbatch` → `inv_lots` (resolved §8 #24) |
| tracksBin | Boolean | new — opt-in `inv_bins` location (resolved §8 #26) |
| inventoryAccountId ○ ➜ | BigInt → Account | tab Akun "Persediaan" (`brekpersediaan`) |
| salesAccountId ○ ➜ | BigInt → Account | tab Akun "Penjualan" (`brekpenjualan`) |
| salesReturnAccountId ○ ➜ | BigInt → Account | tab Akun "Retur Penjualan" |
| salesDiscountAccountId ○ ➜ | BigInt → Account | tab Akun "Diskon Penjualan" |
| cogsAccountId ○ ➜ | BigInt → Account | tab Akun "Hpp" (`brekhargapokok`) |
| purchaseReturnAccountId ○ ➜ | BigInt → Account | tab Akun "Retur Pembelian" |
| purchaseDiscountAccountId ○ ➜ | BigInt → Account | tab Akun "Diskon Pembelian" |
| consignmentAccountId ○ ➜ | BigInt → Account | tab Akun "Konsinyasi" |
| purchaseTaxId ○ ➜ | BigInt → Tax | Pajak Beli 1 (`bpajakbeli`) |
| purchaseTax2Id ○ ➜ | BigInt → Tax | Pajak Beli 2 (additive, optional) |
| saleTaxId ○ ➜ | BigInt → Tax | Pajak Jual 1 (`bpajakjual`) |
| saleTax2Id ○ ➜ | BigInt → Tax | Pajak Jual 2 (additive, optional) |
| primarySupplierId ○ ➜ | BigInt → Partner | `bsuplier` |
| weight ○ | Decimal(19,4) | optional logistics |
| ageCategory ○ | String | legacy "Kategori Umur" (`bkategoriumur`) — freetext |
| validUntil ○ | Date | legacy "Berlaku s.d" (`bberlaku`) |
| isVatable | Boolean | legacy "BKP" (`bkp`) — Barang Kena Pajak, default true |
| isSpecial | Boolean | legacy "Spesial" (`bspesial`), default false |
| isActive | Boolean | `baktif` |
| metadata ○ | Json | `bcustom1..15`, dimensions, etc. |

**GL / organizational dimensions** (legacy header lookups; all `○` nullable FK → `md_*`):
`divisionId → Division`, `subdivisionId → Subdivision`, `departmentId → Department`,
`subDepartmentId → SubDepartment`, `branchId → Branch`, `defaultLocationId → Location`,
`defaultWarehouseId → Warehouse`, `projectId → Project`, `costCenterId → CostCenter`.
Semua FK intra-domain `md` ditegakkan (named `@relation` + back-pointer di parent).

> **`stockOnHand` is intentionally NOT a column.** Legacy `bstok` is a denormalized
> cache; modern stock is derived from inventory transactions (future module).

> **✅ IMPLEMENTED 2026-05-24** — kolom klasifikasi (kind/productClass/brand/
> material/itemModel/size/color/section), dimensi GL (9 di atas), `costMethod`,
> `minOrderQty`, `ageCategory`, `validUntil`, `isVatable`, `isSpecial` ditambahkan
> via migrasi `20260524_001_erp_item_dimensions_classification`
> (kolom atribut/klasifikasi sebagian sudah dari `20260523_001`). Form FE
> (`items-form.tsx` + `items-form-fields.tsx`, modal `lg` sectioned 2-kolom)
> meng-expose seluruh field. Belum di-expose ke form: tab Atribut multi-varian,
> distributor multi-supplier (deferred).

> **✅ IMPLEMENTED 2026-05-30 — price tiers 1–10** (`md_item_prices`, model
> `ErpItemPrice`) + `md_items.purchaseDiscount`. Mengakhiri "tiers 2–10
> deferred". Migrasi `20260530_001_erp_item_price_tiers` (additive). Detail
> keputusan model + pemetaan field MyERP+ = `apps/web-erp/CLAUDE.md §2.32`.

### Item Price Tiers (`md_item_prices` / `ErpItemPrice`) — child of `md_items`

| Kolom | Tipe | Catatan |
| --- | --- | --- |
| id | BigInt PK | |
| itemId ➜ | BigInt → Item | FK enforced, `onDelete: Cascade` |
| level | Int | 1..N kontigu (tingkat harga dinamis/unlimited, nyambung `md_partners.salesTier`) |
| price | Decimal(19,4) | "Harga Jual N" (`bhargajualN`) |
| discountPercent | Decimal(9,4) | "Diskon Jual N" (persen) |
| audit | — | createdAt/updatedAt/createdById/updatedById |

> `@@unique([itemId, level])`. Tingkat **dinamis/unlimited** (lihat DECISIONS.md
> §2.32 UPDATE 2026-06-13 — item baru mulai 1 tier, tambah/hapus hanya di akhir);
> `md_items.salePrice` = cache level-1. Row level dgn price+diskon kosong
> **tidak** disimpan (sparse).

### Item Media (`md_item_media` / `ErpItemMedia`) — child of `md_items` (new 2026-06-12)

| Kolom | Tipe | Catatan |
| --- | --- | --- |
| id | BigInt PK | |
| itemId ➜ | BigInt → Item | FK enforced, `onDelete: Cascade` |
| kind | enum `ErpItemMediaKind` | `IMAGE` \| `VIDEO` |
| fileName | String | nama file asli upload (display) |
| storedName | String unique | nama acak on-disk `<itemId>-<uuid>.<ext>` |
| mimeType / sizeBytes | String / Int | whitelist mime; img ≤ 5MB, video ≤ 50MB |
| sortOrder / isPrimary | Int / Boolean | urutan galeri; satu gambar "Utama" per item |
| createdAt / createdById | timestamptz / BigInt? | |

> Galeri produk: **max 8 gambar** (satu primary) + **1 video pendek** (upload
> baru mengganti yang lama). File binary di `apps/api-gateway/uploads/erp-items/`
> (gitignored, persist via bind mount); endpoint stream
> `GET /erp/items/:id/media/:mediaId/file` (guard `ErpJwtAuthGuard`, support
> Range untuk video). Tanpa equivalent legacy (MyERP+ tidak punya media item).

---

## Partners — unified Customer / Supplier / Salesman (`md_*`)

### PartnerType  → `md_partner_types`  (new — replaces static tipe dropdown)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | e.g. `CUST`, `SUP`, `SLS`, `GEN` |
| name | String | display label (Customer, Supplier, Salesman, General) |
| kind ◆ | `PartnerTypeKind` | CUSTOMER/SUPPLIER/SALESMAN/GENERAL — **derived from code** (`CUST`→CUSTOMER, `SUP`→SUPPLIER, `SLS`→SALESMAN, else GENERAL); not a form input; drives lookup filtering |
| isActive | Boolean | |

> **✅ IMPLEMENTED 2026-07-11** — `Tipe Partner` yang sebelumnya hardcoded di UI
> dan disimpan sebagai 3 boolean `isCustomer/isSupplier/isSalesman` kini menjadi
> master data `md_partner_types` + FK tunggal `md_partners.partner_type_id`.
> Keputusan user: 1 partner = 1 tipe; lookup Customer/Supplier/Salesman filter
> via `partnerType.kind`. **Update 2026-07-14:** form tidak input `kind`; server
> derive dari `code`.

### Partner  → `md_partners`  (legacy `m1_contact` — normalized)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | `kkode` |
| name | String | `knama` |
| partnerTypeId ○ ➜ | BigInt → PartnerType | peran inti partner; replaces boolean `isCustomer/isSupplier/isSalesman` |
| categoryId ○ ➜ | BigInt → PartnerCategory | `kkategori` |
| taxNumber ○ | String | NPWP (`knpwp`) |
| isTaxable | Boolean | PKP flag (`kpkp`) |
| currencyId ○ ➜ | BigInt → Currency | `kmatauang` |
| receivableAccountId ○ ➜ | BigInt → Account | AR control (`krekpiutang`) |
| payableAccountId ○ ➜ | BigInt → Account | AP control (`krekhutang`) |
| arCreditLimit ○ | Decimal(19,4) | `kbataspiutang` |
| apCreditLimit ○ | Decimal(19,4) | `kbatashutang` |
| saleTermId ○ ➜ | BigInt → PaymentTerm | `kterminjual` |
| purchaseTermId ○ ➜ | BigInt → PaymentTerm | `kterminbeli` |
| salesmanId ○ ➜ | BigInt → Partner | self-ref, salesman (`ksalesman`) |
| commissionRate ○ | Decimal(9,4) | `kkomisipenjualan` |
| branchId ○ ➜ | BigInt → Branch | `kcabang` |
| isActive | Boolean | `kaktif` |
| metadata ○ | Json | `kcustomtext*`, misc |

Relations: `partnerType`, `category`, `currency`, `receivableAccount`/`payableAccount`,
`saleTerm`/`purchaseTerm`, `salesman` (self), `addresses PartnerAddress[]`,
`contacts PartnerContact[]`, `bankAccounts PartnerBankAccount[]`.

> Balances `ktotalpiutang`/`ktotalhutang` are **derived** (AR/AP ledger) — not stored.

### PartnerAddress  → `md_partner_addresses`  (legacy `m1_contact` k1..k4 blocks)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| partnerId ➜ | BigInt → Partner | |
| type ◆ | `AddressType` | BILLING/SHIPPING/OFFICE/OTHER |
| isDefault | Boolean | one default per type |
| addressLine1 | String | |
| addressLine2 ○ | String | |
| city ○ | String | |
| province ○ | String | |
| country ○ | String | |
| postalCode ○ | String | |
| phone ○ | String | |
| fax ○ | String | |

`@@index([partnerId])`. Replaces 4 hard-coded inline address blocks.

### PartnerContact  → `md_partner_contacts`  (legacy `m1_contact_attention`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| partnerId ➜ | BigInt → Partner | `kaidkontak` |
| name | String | `kanama` |
| title ○ | String | role/position |
| phone ○ | String | |
| email ○ | String | |
| isDefault | Boolean | `kadefault` |

### PartnerBankAccount  → `md_partner_bank_accounts`  (legacy `kbank`/`knorekening`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| partnerId ➜ | BigInt → Partner | |
| bankName | String | `kbank` |
| accountNumber | String | `knorekening` |
| accountHolder ○ | String | |
| isDefault | Boolean | |

### PartnerCategory  → `md_partner_categories`  (merged contact/customer/supplier/salesman categories)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | |
| name | String | |
| kind ◆ | `PartnerCategoryKind` | CUSTOMER/SUPPLIER/SALESMAN/GENERAL |
| salesTier ○ | Int | `cctingkatjual` (customer price tier) |
| isActive | Boolean | |

---

## Finance (`md_*`)

### Currency  → `md_currencies`  (legacy `m1_currency`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | ISO 4217 recommended (`ckode`) |
| name | String | `cnama` |
| symbol ○ | String | `csimbol` |
| decimalPlaces | Int default 2 | jumlah desimal tampilan/entry (0–6) |
| isBase | Boolean default false | mata uang dasar org (maks 1 aktif) |
| isActive | Boolean | `caktif` |

> No `exchangeRate` snapshot column — rates are dated rows in `CurrencyRate`
> (resolved §8 #8; replaces legacy single `ckurs`). Relations: `rates CurrencyRate[]`.
> Base currency (`isBase=true`) is the home currency; rates are units of base per 1 unit of this currency.

### CurrencyRate  → `md_currency_rates`  (new — dated FX, resolved §8 #8)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| currencyId ➜ | BigInt → Currency | |
| rateDate | Date | effective date of the rate |
| rate | Decimal(19,6) | units of base currency per 1 unit of `currency` |
| isActive | Boolean | |

Unique: `@@unique([currencyId, rateDate])`. `@@index([currencyId, rateDate])`.
Rate at a transaction date = latest row with `rateDate <= txnDate`. Base currency
(the org's home currency) has no rows (implicit 1.0); configured in `sys_settings`.

### Account  → `md_accounts`  (legacy `m1_coa` — Chart of Accounts)

**Format kode dinamis dari `sys_settings` (group `account-code`).** Default seed
`NNNN.NN.NNN` (4-2-3, dual dot, 11 char) — decision 2026-05-27 (`README.md §8`
#43 → #44). 4 digit prefix = kelompok-grup PSAK (`1xxx` Aset, `2xxx` Liab,
`3xxx` Ekuitas, `4xxx` Revenue, `5xxx` HPP, `6xxx` Beban, `7xxx` Pos Luar Biasa
& Pajak); 2 digit middle = sub-grup (max 99 anak per cabang); 3 digit leaf =
nomor urut akun (max 999, mirror legacy `.NNN`). HEADER pakai trailing zero
`NNNN.00.000`; POSTABLE pakai non-zero, default `NNNN.01.001`. Validasi format
dinamis di service `ErpAccountsService` (`validateAccountCode()`); DTO hanya
`@MaxLength(30)` (regex hardcode dihapus). FE form validator dinamis di
`accounts-form.tsx` (hook `useAccountCodeFormat`). Aturan hierarki (2026-07-14,
lihat `DECISIONS.md §2.24.2`): parent-first; parent wajib `HEADER`; tipe child
ter-filter = tipe parent; `level` & `normalBalance` di-derive server-side;
pindah cabang recompute level keturunan; field `currencyId`/`bankName`/
`bankAccountNo` hanya untuk akun di segmen kode terakhir.

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | account number format dinamis (default `NNNN.NN.NNN`; legacy `cnomor`) |
| name | String | `cnama` |
| alias ○ | String | `cnamaalias1` (alias2/3 → metadata) |
| type ◆ | `AccountType` | `ctipe` |
| kind ◆ | `AccountKind` | HEADER vs POSTABLE (`cjenis`) |
| normalBalance ◆ | `NormalBalance` | `cdc` |
| parentId ○ ➜ | BigInt → Account | tree (`cparent`/`csubdari`) |
| level | Int | `clevel` |
| cashFlowCategory ○ ◆ | `CashFlowCategory` | `caruskas` |
| currencyId ○ ➜ | BigInt → Currency | `cmatauang` |
| isControlAccount | Boolean | legacy seed/AR-AP flag (`cbukupembantu`); **removed from form UI** (2026-07-14) |
| bankId ○ ➜ | BigInt → Bank | master bank for cek/giro (`md_banks`) |
| bankName ○ | String | free-text legacy bank name (`ckodebank`); prefer `bankId` |
| bankAccountNo ○ | String | `cnorekbank` |
| openingBalance | Decimal(19,4) | `csaldoawal` |
| isActive | Boolean | `caktif` |
| notes ○ | String | `ccatatan` |

Relations: self `parent`/`children`, `currency`, `bank`, multi-dim junctions
`md_account_dim_branches` / `md_account_dim_locations` / `md_account_dim_divisions`
(empty = no scope restriction).
> Running balance `csaldoberjalan` is **derived** from journal entries (finance phase).
> CoA multi-dimension scope on master account (cabang/lokasi/divisi) enabled 2026-07-14.

### Tax  → `md_taxes`  (legacy `m1_tax`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | `tkode` |
| name | String | `tnama` |
| rate | Decimal(9,4) | percent (`tnilai`) |
| saleAccountId ○ ➜ | BigInt → Account | output tax GL (`takunjual`) |
| purchaseAccountId ○ ➜ | BigInt → Account | input tax GL (`takunbeli`) |
| isActive | Boolean | `taktif` |

### PaymentTerm  → `md_payment_terms`  (legacy `m1_terms`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | `trkode` |
| name | String | `trnama` |
| netDays | Int | due (`trharijatuhtempo`) |
| discountPercent1 ○ | Decimal(9,4) | `trdiskon1` |
| discountDays1 ○ | Int | `trharidiskon1` |
| discountPercent2 ○ | Decimal(9,4) | `trdiskon2` |
| discountDays2 ○ | Int | `trharidiskon2` |
| penaltyPercent ○ | Decimal(9,4) | `trdenda` |
| penaltyPeriod ○ | String | per day/month (`trdendaper`) |
| isActive | Boolean | `traktif` |

### OtherCost  → `md_other_costs`  (legacy finance master "Other Cost")

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | semantic other-cost code |
| name | String | display name |
| debitAccountId ○ ➜ | BigInt → Account | default debit GL; required when `isHPP=false`; POSTABLE only |
| creditAccountId ➜ | BigInt → Account | default credit GL; always required by API/UI; POSTABLE only |
| isHPP | Boolean | true = debit allocated proportionally to transaction items; debit account stored null |
| isActive | Boolean | |

> `isHPP=true` is a master-data flag only; proportional allocation is performed
> by transaction posting modules (purchase/sales follow-up), not by this master.

---

## Geographic Reference (`md_*`)

> Hierarki kanonik: `md_countries` ← `md_provinces` ← `md_cities` ← `md_areas`.
> FK **wajib ditegakkan** di setiap level (intra-domain `md`). Kode pos (`postalCode`)
> berada di `md_areas` (kecamatan), bukan di `md_cities` — karena satu kota punya
> banyak kode pos, masing-masing per kecamatan. Lihat `CLAUDE.md §2.20`.
>
> Jika kelurahan dibutuhkan di masa depan → tambah `md_sub_areas` (FK ke `md_areas`).

### Country  → `md_countries`  (legacy `m1_country`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | ISO 3166-1 alpha-2 direkomendasikan |
| name | String | |
| isoCode ○ | String | ISO 3166-1 alpha-3 |
| isActive | Boolean | |

Relations: `provinces Province[]`.

### Province  → `md_provinces`  (legacy `m1_province`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | |
| name | String | |
| countryId ➜ | BigInt → Country | FK wajib |
| isActive | Boolean | |

Relations: `country Country`, `cities City[]`.

### City  → `md_cities`  (legacy `m1_city` — kota/kabupaten)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | |
| name | String | |
| provinceId ➜ | BigInt → Province | FK wajib |
| isActive | Boolean | |

> **Tidak ada `postalCode` di level ini** — satu kota punya banyak kode pos.

Relations: `province Province`, `areas Area[]`.

### Area  → `md_areas`  (kecamatan — BPS 7-digit code)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | BPS 7-digit, e.g. `"3173080"` |
| name | String | nama kecamatan |
| cityId ➜ | BigInt → City | FK wajib |
| postalCode ○ | String | kode pos per kecamatan (dari kelurahan pertama) |
| isActive | Boolean | |

Relations: `city City`, `subAreas SubArea[]`.

### SubArea  → `md_sub_areas`  (kelurahan/desa — BPS 10-digit code)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | BPS 10-digit, e.g. `"3173080003"` |
| name | String | nama kelurahan/desa |
| areaId ➜ | BigInt → Area | FK wajib |
| postalCode ○ | String | **kode pos per kelurahan** — SSOT paling akurat |
| isActive | Boolean | |

`@@index([postalCode])` — untuk autofill dari kelurahan ke kode pos.

Relations: `area Area`.

> **Seed data Indonesia lengkap** tersedia via `npm run db:seed:geo`
> (`prisma/seed-md-geo.ts`, sumber `kode-wilayah-id` MIT):
> 38 provinsi · 514 kab/kota · 7.286 kecamatan · **84.270 kelurahan/desa**.
> Semua kode = BPS code. Migration: `20260522_004_erp_md_geo_kelurahan`.

---

**Count:** 19 Master Data (`md_*`) core entities (3 org, 3 items, 6 partner, 6 finance¹ + ItemCategory/Unit).
¹ finance = Currency, **CurrencyRate**, Account, Tax, PaymentTerm, OtherCost.
Combined MVP total = **33 tables** (14 Administrator `sys_*`/`adm_*` + 19 `md_*`).
Geographic reference (4 tabel: Country, Province, City, Area) ditambahkan via §2.18 batch.
Legacy field-mapping appendix: **[legacy-mapping.md](legacy-mapping.md)**.
