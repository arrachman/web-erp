# DECISIONS — Master Data — partner, item, CoA, mata uang, termin

> Bagian dari decision log Web-ERP. Dipindahkan dari `DECISIONS.md`
> (2026-10-04) agar file indeks ramping; **isi entri tidak diubah** dan
> nomor `§` dipertahankan sebagai anchor stabil. Indeks semua entri:
> [`DECISIONS.md`](../../DECISIONS.md).

---

### Master Data Partner — Tipe Partner sebagai master data (2026-07-11)

Keputusan user: dropdown statis `Tipe` di `/master/partners` **diganti menjadi
master data** `md_partner_types` + CRUD `/master/partner-types`. Partner kini memakai
1 FK `md_partners.partner_type_id` (1 partner = 1 tipe), bukan lagi 3 boolean
`isCustomer`/`isSupplier`/`isSalesman`.

`md_partner_types.kind` (`CUSTOMER`/`SUPPLIER`/`SALESMAN`/`GENERAL`) menjadi penanda
peran untuk filter lookup Customer/Supplier/Salesman di sales/purchase/inventory dan
form Partner. `md_partner_categories` tetap terpisah sebagai kategori/sub-segmen
partner (termasuk `salesTier`) — tipe ≠ kategori.

**Update 2026-07-14:** field input **Jenis/kind di form dihapus**. `kind` **di-derive
otomatis dari `code`** (server + FE payload): `CUST` → `CUSTOMER`, `SUP` → `SUPPLIER`,
`SLS` → `SALESMAN`, kode lain → `GENERAL`. Kolom list/filter Kind tetap (read-only
display). Client `kind` di body create/update diabaikan.

**Update 2026-07-14 (sort):** list `/master/partner-types` **selalu pin** tipe
terkunci di atas lewat SQL `ORDER BY CASE code WHEN 'CUST' THEN 0 WHEN 'SUP'
THEN 1 WHEN 'SLS' THEN 2 ELSE 3 END`, lalu sort sekunder user
(`sortBy`/`sortDir`, default `createdAt desc`). Pin by **code** (bukan kind)
agar hanya 3 baris sistem yang naik, bukan seluruh kind CUSTOMER/SUPPLIER.
Berlaku di semua page/pagination — bukan sort klien.

---

### Master Data Finance — Other Costs default GL + HPP flag (2026-07-14)

Other Costs (`md_other_costs`, `/master/other-costs`) sekarang menyimpan default
akun GL: **Akun Debit** dan **Akun Kredit**. Kedua picker wajib mengambil CoA
**level terakhir / `POSTABLE`** saja; akun `HEADER` tidak boleh dipakai untuk
posting default. Akun Kredit selalu wajib.

`isHPP` menandai biaya lain yang debit-nya **dialokasikan proporsional ke barang
transaksi**, bukan ke satu akun debit statis. Saat `isHPP=true`, field Akun Debit
di form master dinonaktifkan dan disimpan `null`; Akun Kredit tetap wajib. Saat
`isHPP=false`, Akun Debit dan Akun Kredit sama-sama wajib. Logika alokasi
proporsional ke barang (purchase/sales posting) adalah follow-up di modul
transaksi, bukan di master ini.

---

### 2.18 MD legacy batch (2026-05-20) — 20 master baru dari MyERP+ m1_*

Wave besar menambah 20 entitas master legacy yang belum di-implement.
Branch `feat/erp-md-legacy-batch`, 3 commit.

**Tercakup:** Brand, Material, ItemModel, Size, Section, ItemKind
(table `md_item_types`), ProductClass, ItemLocation, Commission (+amount),
Bank, Expedition, PartnerSubCategory (enum CUSTOMER/SUPPLIER/SALESMAN),
OtherCost, Country, Province (FK Country), City (FK Province), Area
(FK City), ItemTransactionType (+direction), TransactionNote, PriceCategory.

**Keputusan terkunci batch ini:**

1. **`ErpItemKind` ≠ `ErpItemType`.** Enum `ErpItemType` (hardcoded
   `INVENTORY/SERVICE/VOUCHER/ASSEMBLY` di kolom `ErpItem.type`) sudah ada
   sejak m1 init dan tidak boleh bentrok. Master user-configurable dari
   legacy `m1_item_type` → model **`ErpItemKind`** dengan
   `@@map("md_item_types")`. Saat menambah master baru: cek dulu apakah
   nama model bentrok dengan enum/model existing.
2. **Partner sub-category = 1 tabel + enum (`ErpPartnerSubCategoryType`).**
   3 menu sidebar (Customer/Supplier/Salesman Categories) share 1 page +
   1 table + 1 endpoint. Path `/master/{customer,supplier,salesman}-categories`
   semua resolve ke `ErpPartnerSubCategoriesPage` di `ERP_PAGES`. Filter
   type via query string ditambahkan saat dibutuhkan (saat ini belum).
3. **Reference Country→Province→City→Area = FK ditegakkan (intra-domain `md`).**
   Seed di `prisma/seed-md-legacy.ts` (idempotent): **197 Country (seluruh dunia, ISO 3166-1 alpha-2, 2026-05-22)**, 38 Province ID, **514 Kab/Kota lengkap per BPS** (kode BPS 4-digit), Bank, Expedition.
   City upsert pakai `findFirst(bpsCode OR code) + update-by-id` (bukan `upsert`) karena DB bisa mixed-state.
   **`postalCode` hidup di `md_areas` (level kecamatan), bukan di `md_cities`** —
   karena satu kota punya banyak kode pos, masing-masing per kecamatan. Lihat §2.20.
4. **ItemPermission ditunda.** Bukan master "code+name+isActive" — pivot
   `itemId × roleId × {canView,canSell,canBuy}`. `SimpleMasterPage` tidak
   cocok; perlu page custom. Tabel sudah ada (`md_item_permissions`) tapi
   modul API/FE belum.
5. **PriceCategoryDetail & TransactionNoteDetail = child-managed.** Tabel
   ada (cascade delete dari parent), tapi diakses lewat parent form bukan
   menu sidebar. Menu "Txn Note Detail" di sidebar untuk sekarang fallback
   ke ComingSoon.

**Generator script:** `apps/api-gateway/scripts/scaffold-md-batch.mjs` —
one-shot scaffolder. Pattern di-mirror dari `erp-divisions`. Pluralization
manual (Class→Classes, City→Cities, Country→Countries, Category→Categories;
generator default plural = `+ 's'` di-override via sed post-process). Re-run
aman: skip file yang sudah ada. **Untuk master sederhana berikutnya
(code+name+isActive ± extra fields), tambah entry di array `ENTITIES`
lalu re-run** — jangan tulis ulang DTO/service per tangan.

**Migration:** `20260520_002_erp_md_legacy_batch` — additive, applied via
`prisma db execute` lalu `prisma migrate resolve --applied` karena shadow
DB drift di migrasi clinic lama. 23 tabel + 1 enum
(`ErpPartnerSubCategoryType`), 0 DROP. Saat shadow DB rusak, route ini
(execute + resolve) lebih aman daripada `migrate dev`.

---

### 2.20 Hierarki geografis & kode pos (WAJIB, 2026-05-22)

Setiap tabel referensi geografis **wajib** terhubung ke level di atasnya via
FK yang ditegakkan — tidak boleh ada tabel wilayah yang berdiri sendiri tanpa
relasi ke induknya.

**Hierarki kanonik ERP (sudah diimplementasi):**

```
md_countries ← md_provinces ← md_cities ← md_areas (kecamatan) ← md_sub_areas (kelurahan)
```

Aturan turunan:

- `md_provinces.countryId` → FK ke `md_countries`.
- `md_cities.provinceId` → FK ke `md_provinces`.
- `md_areas.cityId` → FK ke `md_cities`. Field `postalCode` ada di sini (per kecamatan).
- `md_sub_areas.areaId` → FK ke `md_areas`. Field `postalCode` ada di sini juga (per kelurahan, lebih granular).
- `postalCode` di `md_partner_addresses`, `md_branches`, `md_locations` adalah
  **freetext** (isian manual) — terpisah dari referensi `md_areas`/`md_sub_areas`.
  User mengisi manual atau autofill dari `md_sub_areas` saat memilih kelurahan di form alamat.

**Seed data (2026-05-22):** `prisma/seed-md-geo.ts` (jalankan via `npm run db:seed:geo`).
Sumber: `kode-wilayah-id` (MIT). Data lengkap Indonesia:
38 provinsi, 514 kab/kota, 7.286 kecamatan (+ `postalCode`), 84.270 kelurahan/desa (+ `postalCode`).
Kode = BPS code (2/4/7/10 digit sesuai level). Idempotent — aman dijalankan ulang.

**Model Prisma:**
- `ErpArea` → `@@map("md_areas")`, relasi `subAreas ErpSubArea[]`
- `ErpSubArea` → `@@map("md_sub_areas")`, FK `areaId → ErpArea`, index `postalCode`

Migration: `20260522_004_erp_md_geo_kelurahan` (additive, 0 DROP).

---

### 2.21 Item Information page = `/master/item-info` (canonical, 2026-05-23)

Halaman Item Information (1:1 extension dari `md_items`: produsen, negara
asal, garansi, deskripsi panjang, spesifikasi, tags, catatan). Canonical
path **= `/master/item-info`** (seed `sys_menus` code `M1.ITEM.INFO`).
Long-form `/master/item-informations` dipertahankan **sebagai alias** di
[`shell-route-renderer.tsx`](components/templates/shell-route-renderer.tsx)
dan `ERP_ROUTE_META` (`lib/nav.ts`) untuk URL yang sudah ter-bookmark —
jangan dihapus, tapi jangan dipakai sebagai entry baru.

**Aturan implementasi:**

- Form pakai **`SearchSelect`** untuk `itemId` (load dari `listItems`), **disabled saat edit** karena `itemId @unique` (1:1 ke `ErpItem`) — ganti item = ganti rekor.
- Field form lengkap: `itemId`, `manufacturer`, `countryOfOrigin`, `warrantyPeriodMonths`, `longDescription` (textarea), `specifications` (textarea), `tags`, `notes` (textarea). **Dilarang** drop field DTO dari form tanpa alasan.
- Service `ErpItemInformationsService` **wajib** include `item: { select: { id, code, name } }` di semua query (list/get/create/update) supaya halaman bisa pakai `item.code`/`item.name` sebagai kolom KODE/NAMA tanpa N+1 fetch.
- `SimpleMasterPage` adapter: `code = item.code` (fallback `INF-${id}`), `name = item.name` (fallback `Item #${itemId}`), `isActive = true` (entity tak punya status sendiri). Extra columns: Produsen, Negara Asal, Garansi.

**Seed dummy:** `prisma/seed-erp-item-informations-dummy.ts` (100 rows, idempotent — `findMany({ information: { is: null } }) + createMany skipDuplicates`). Butuh `md_items` ter-seed lebih dulu (lihat `seed-erp-md-dummy.ts`).

---

### 2.23 Item master = form lengkap sectioned (2026-05-24)

Master Item (`/master/items`, `ErpItemsPage`) di-expand dari 7 field → paritas
header MyERP+ "Barang". Field katalog otoritatif = `db-design/entities-m1-master-data.md`.

- **`md_items` kolom baru** (migrasi `20260524_001_erp_item_dimensions_classification`):
  `costMethod` (enum `ErpCostingMethod` AVG/FIFO/STD), `minOrderQty`, `ageCategory`,
  `validUntil`, `isVatable` (BKP), `isSpecial`, + 9 dimensi GL FK (`divisionId`,
  `subdivisionId`, `departmentId`, `subDepartmentId`, `branchId`, `defaultLocationId`,
  `defaultWarehouseId`, `projectId`, `costCenterId`). Klasifikasi (`kindId`,
  `productClassId`, `brandId`, `materialId`, `itemModelId`, `sizeId`, `colorId`,
  `sectionId`) sebagian sudah dari `20260523_001` — schema.prisma kini meng-expose
  semuanya. **FK intra-domain `md` ditegakkan** (named `@relation` + back-pointer parent).
- **`ErpItemType` enum di-reshape** (migrasi `20260524_002_erp_item_type_enum_reshape`):
  `INVENTORY/SERVICE/CONSUMABLE/ASSET/NON_INVENTORY` (hapus `VOUCHER/ASSEMBLY`).
  FE & DB konsisten. Saat menyentuh `type`, pakai set ini.
- **Form FE**: dipecah `items-form.tsx` (types/adapters/validation) +
  `items-form-fields.tsx` (UI organism) + `items-form-lookups.ts` (loader SearchSelect).
  Layout = **modal `lg` (900px), section 2-kolom** (Identitas · Satuan & Penilaian ·
  Stok & Tracking · Harga & Pajak · Akun GL · Dimensi & Supplier · Deskripsi).
  Bukan tab legacy — keputusan user: "UX nyaman & compact". Sub-komponen
  `LookupField`/`NumField`/`YesNoField` **di module-level** (jangan inline di render —
  remount + hilang focus).
- **`SimpleMasterPage` dapat prop `modalSize?: 'md' | 'lg'`** (default `md`). Form
  kaya (banyak field) → pakai `modalSize="lg"`. Diteruskan ke `<ModalContent size>`.
- **Deferred** (belum di form): tab Atribut multi-varian, distributor
  multi-supplier. Item Information tetap halaman 1:1 terpisah (§2.21).
  (Price tiers 1–10 sudah **implemented** — lihat §2.32.)

---

### 2.24 Format kode akun CoA = dinamis dari `sys_settings` (2026-05-27)

Format `md_accounts.code` **tidak** lagi hard-locked ke `NNNN.NN.NNN` (#43
db-design). Pakai 2 setting global di `sys_settings` group `account-code`:

- `account_code_segments` (JSON array int, mis. `[4,2,3]` / `[5]` / `[7]` / `[6,3]`) — panjang tiap segmen.
- `account_code_separator` (string: `.` / `-` / `/` / `""` tanpa pemisah).

**Backend SSOT** = `apps/api-gateway/src/erp-accounts/account-code-format.ts`:
`buildAccountCodeFormat(segments, separator)` → `{pattern, maxLength, example}`.
`ErpAccountsService.create()`/`update()` baca setting + `validateAccountCode()`
sebelum insert. DTO `CreateErpAccountDto` hanya `@MaxLength(30)` (tidak ada
`@Matches`). Endpoint: `GET /erp/accounts/code-format` (segments, separator,
patternSource, maxLength, example, accountCount, locked) + `PUT /erp/accounts/code-format`
(409 ConflictException bila `md_accounts.count > 0` — **lock-after-data**).

**Frontend:**
- `components/pages/accounts-form.tsx` cache format module-level + hook
  `useAccountCodeFormat()`; `validateAccount` baca cache; `AccountFormFields`
  pakai `maxLength`/`placeholder`/`example` dari format aktif.
- Halaman dedicated `/admin/account-code-format`
  (`account-code-format-page.tsx` + molecule `account-code-format-presets.tsx`)
  di group `M0.SYS` (Administrator → System) — segments editor (1–5 segmen
  × 1–12 digit), separator picker, preset cepat (PSAK 4-2-3, flat 5/6/7,
  4-3, 4-3-3, legacy 6-3), preview live, lock card saat ada akun. Setelah
  PUT sukses → `invalidateAccountCodeFormatCache()` supaya form refresh.

**Saat onboarding klien dengan CoA legacy berbeda**: admin Senti pilih
format di `/admin/account-code-format` **sebelum** import CoA / jalankan
`seed-erp-accounts.ts`. Default seed tetap PSAK 4-2-3 — klien yang OK
dengan PSAK tinggal pakai. Ganti format setelah ada akun → tolak 409;
harus hapus semua akun dulu (out of scope MVP: tool migrasi rename code).

---

### 2.24.1 Saldo Normal CoA = derived dari Tipe Akun, bukan input manual (2026-07-13)

Field **Saldo Normal** (`md_accounts.normalBalance`, DEBIT/CREDIT) di form
Bagan Akun **tidak** lagi diinput manual — ditentukan otomatis dari **Tipe
Akun** mengikuti prinsip akuntansi baku. Alasan (keputusan user): saldo normal
adalah konsekuensi deterministik dari tipe akun; input manual hanya membuka
peluang data tidak konsisten (mis. ASSET ber-saldo normal CREDIT).

Map otoritatif (`TYPE_NORMAL_BALANCE_MAP` di `components/pages/accounts-form.tsx`):

| Tipe Akun | Saldo Normal |
| --- | --- |
| ASSET | DEBIT |
| EXPENSE | DEBIT |
| LIABILITY | CREDIT |
| EQUITY | CREDIT |
| REVENUE | CREDIT |

Implementasi FE: helper `normalBalanceForAccountType()` dipakai di tiga jalur:
`fromAccount()` (edit data lama dinormalisasi saat form dibuka), handler
`onValueChange` "Tipe Akun" (satu `onChange`, immutable), dan
`toAccountPayload()` (payload selalu derive ulang sebelum dikirim). Field
auto/read-only (Jenis + Saldo Normal, dan hint tipe=parent bila child) di-render
**compact** sebagai satu strip `FormField` label "Auto" berisi badge + help
pendek — bukan baris penuh per field + teks italic panjang (keputusan UI
2026-07-14: form Bagan Akun lebih minimalis untuk nilai otomatis). Payload
tetap mengirim `normalBalance` (diturunkan), kontrak API tidak berubah.
`NORMAL_BALANCES` di `lib/api/accounts.ts` dipertahankan untuk filter list &
lookup lain.

> Status 2026-07-14: backend `erp-accounts` sekarang juga menderivasi
> `normalBalance` dari tipe akun efektif, sehingga payload yang mencoba
> mengirim saldo normal tidak konsisten tidak lagi menjadi sumber kebenaran.

### 2.24.2 CoA parent-first + validasi scope akun (2026-07-14)

Form Bagan Akun (`/master/accounts`) wajib **parent-first**: user memilih
Parent terlebih dahulu, baru mengisi kode/nama dan atribut akun. Untuk akun
root, **Tipe Akun** tetap manual. Untuk akun child, tipe manual tetapi
**ter-filter/terkunci mengikuti tipe parent** — contoh `Kas` berada di bawah
ASSET, `Modal` di bawah EQUITY; sistem tidak menebak dari nama, melainkan dari
scope parent agar tidak salah setting.

Backend `erp-accounts` adalah SSOT validasi hierarki:
- Parent wajib akun aktif/non-deleted dengan `kind=HEADER`; `POSTABLE` tidak
  boleh punya anak.
- Child `accountType` wajib sama dengan parent; root bebas memilih tipe.
- `level` di-derive server-side (`root=1`, child=`parent.level+1`) dan saat
  parent cabang dipindah, level seluruh keturunan dihitung ulang.
- `normalBalance` di-derive dari tipe efektif (§2.24.1).
- Update parent menolak self-parent dan descendant-parent (cycle).
- Hapus akun yang masih punya anak aktif ditolak; hapus anak dulu.

Field **Mata Uang**, **Bank**, dan **No. Rekening** hanya valid untuk akun di
**segmen kode terakhir** sesuai format dinamis `sys_settings` (§2.24), yaitu
segmen terakhir kode memiliki digit non-nol. FE menampilkan section "Detail
Akun Posting" hanya saat kode terdeteksi leaf (= POSTABLE); backend
tetap menjadi penentu final dan menolak field tersebut pada akun non-leaf.
`currencyId` wajib mengarah ke `md_currencies` aktif/non-deleted. Tidak ada
migrasi DB karena `currencyId`, `bankName`, `bankAccountNo`, `level`, dan
`parentId` sudah ada di `md_accounts`.

### 2.24.3 CoA Jenis (POSTABLE/HEADER) otomatis dari kode leaf (2026-07-14)

**Strict auto-kind** (opsi 1): field **Jenis** di form Bagan Akun **bukan lagi
pilihan manual**. Backend `erp-accounts` adalah SSOT:

| Kode (format dinamis `sys_settings`) | `kind` |
| --- | --- |
| Leaf — segmen terakhir punya digit `1–9` | `POSTABLE` |
| Non-leaf — segmen terakhir semua `0` | `HEADER` |

Aturan ini **format-agnostic**: diganti di halaman Format Kode Akun
(`NNNN.NN.NNN`, `NNNN-NN`, compact tanpa separator, dll.) → deteksi leaf
ikut `segments`+`separator` → POSTABLE ikut benar tanpa hardcode pattern.

Implementasi:
- Helper `accountKindFromCode(code, format)` di BE (`account-hierarchy.ts`)
  dan FE (`accounts-form.tsx`) — mirror `isLeafAccountCode`.
- `CreateErpAccountDto.accountKind` jadi **opsional**; service create/update
  **selalu** menulis `kind` hasil derive (payload diabaikan).
- Update: jika derive jadi POSTABLE tapi akun masih punya anak → ditolak
  (`assertPostableHasNoChildren`).
- FE: "Jenis" di-render read-only `Badge` + catatan otomatis (sama pola
  Saldo Normal §2.24.1). Saat user mengetik kode, `accountKind` di form
  state ikut; detail bank/mata uang tampil hanya untuk leaf.
- `ACCOUNT_KINDS` di `lib/api/accounts.ts` tetap untuk filter list.

Konvensi entri: HEADER pakai trailing zero di segmen terakhir
(`1100.00.000`); POSTABLE pakai non-zero (`1101.01.001`).

### 2.24.3b CoA level-3 sub-grup HEADER (Kas/Bank/Piutang/…) (2026-07-14)

Bagan Akun **wajib** punya parent **level 3 HEADER** di bawah tiap grup
level 2, agar list tree bisa digroup (Kas, Bank, Piutang, Persediaan, …)
bukan leaf langsung di bawah Aset Lancar.

Hierarki seed kanonik (`prisma/seed-erp-accounts.ts`):

| Level | kind | Contoh kode | Contoh nama |
| --- | --- | --- | --- |
| 1 | HEADER | `1000.00.000` | Aset |
| 2 | HEADER | `1100.00.000` | Aset Lancar |
| 3 | HEADER | `1101.00.000` / `1110.00.000` / `1120.00.000` | Kas / Bank / Piutang |
| 4 | POSTABLE | `1101.01.001` | Kas Besar |

- Kode HEADER L3 = prefix 4-digit sub-grup + `.00.000` (non-leaf).
- Leaf postable dipindah `parentId` ke HEADER L3; `level` di-derive
  server-side = 4 (seed set eksplisit; re-seed idempotent by code).
- Tidak ada migrasi schema — hanya data seed. Re-apply:
  `npm run db:seed:accounts` di `apps/api-gateway`.
- UI tree list sudah parentId-driven (`lib/accounts-tree.ts`); tidak
  perlu ubah FE — expand/collapse L3 muncul otomatis setelah seed.

### 2.24.4 CoA auto-kode sibling (increment / mulai 1) (2026-07-14)

Saat **Tambah** akun di form Bagan Akun, field **Kode** diisi otomatis dengan
cara **menyisipkan baris baru di bawah parent** dan **meneruskan format sibling**:

- Ambil anak langsung parent (`listAccounts({ parentId })`; root =
  `parentId='null'`), sort `code desc`, hitung max + step pada segmen urut.
- Ada sibling → **increment** dengan step terinfer (GCD selisih multi, atau
  trailing-zero single: `1100`→+100). Contoh di bawah `1000.00.000` dengan
  `1100`/`1200`/`1300` → usul `1400.00.000`; leaf `1101.01.001`…`1110.01.001`
  → `1111.01.001`.
- Belum ada sibling → **mulai dari 1** pada segmen pertama setelah trailing zero
  parent (contoh parent `1100.00.000` → `1100.01.000`; root kosong →
  `1000.00.000`).
- Parent dipilih ulang / dikosongkan → kode disarankan ulang. Tombol **Auto**
  di samping field Kode memaksa re-generate (override manual).
- Bukan transactional sequence; race dua user → backend unique `code` tetap
  menolak. Pola mirror `lib/items-code-generator.ts`.

**Implementasi FE:** `lib/accounts-code-generator.ts` (pure
`suggestNextAccountCode` / `firstChildAccountCode` / `inferSequenceStep` +
`generateNextAccountCode`) + wire di `components/pages/accounts-form.tsx`
(`handleParentPick`, mount create kosong, tombol Auto). Parent option label =
`code — name` agar parent code tersedia tanpa fetch ekstra.

### 2.24.5 CoA list = tree expand/collapse (2026-07-14)

List **Bagan Akun** (`/master/accounts`) menampilkan hierarki `parentId`
dengan **expand/collapse** per parent (chevron), indent depth, dan toolbar
**Expand / Collapse** semua. Anak hanya terlihat saat parent dibuka.

Bukan `TreeDndMasterPage` (§2.22): CoA **tidak** punya `sortOrder` /
MODULE·GROUP·ITEM, dan user hanya minta tree view — bukan DnD reorder.
`TreeDndMasterPage` tetap untuk menu/kategori ber-sortOrder.

**Perilaku:**
- Load full chart client-side (`limit=5000`; DTO accounts `@Max(5000)`).
- Default: semua parent expanded. Search client-side agar match + ancestor
  tetap di tree (API search leaf-only memutus hierarki).
- Filter status / tipe / jenis tetap server-side. Footer count-only
  (`footerSummary` di `ErpListLayout`), tanpa pagination baris.
- Form create/edit/bulk/audit tetap (bukan SimpleMasterPage flat).

**File:** `lib/accounts-tree.ts`, `components/pages/accounts-page.tsx`,
`accounts-tree-table.tsx`, `accounts-page-actions.ts`.

---

### 2.25 Item master form redesign — quick-add + side-nav (2026-05-27)

Lanjutan §2.23. Form item kini punya **2 mode entri** dan layout berbeda
per mode. Keputusan ini berlaku **khusus form item** — masih sectioned
2-kolom kompak, namun layout level-form berubah.

**Mode entri:**
- **Cepat** — default saat tambah item baru (`data.code === ''`). Hanya
  dua section **Identitas** + **Klasifikasi** (~7-8 field wajib). Layout scroll.
- **Lengkap** — default saat edit. Semua section, **side-nav 200px di
  kiri** + content section aktif di kanan. Section conditional (Inventory
  & Tracking) hanya muncul saat `isStockable(itemType)`. Dot merah di
  nav item menandakan section dgn error validasi.

Toggle Cepat/Lengkap di top form (pill segmented). User bebas switch
tanpa kehilangan state form. Modal pakai `size="xl"` (1100px) supaya
side-nav + content tidak crowded.

**Section grouping inti:**
Identitas · Klasifikasi · Media · Lampiran · Atribut · Inventory & Tracking
(conditional) · Pergerakan Stok · Harga · Pajak · Akuntansi · Dimensi GL ·
Supplier. Restructure dari 7 section lama (§2.23) yang campur identitas
dengan satuan, dan Akun GL dengan Dimensi/Supplier.

**Identitas + Klasifikasi digabung (2026-07-11):**
Atas permintaan user, dua entry side-nav `identitas` dan `klasifikasi` dilebur
menjadi satu section **Identitas & Klasifikasi**. Field tetap sama: Kode, Nama,
Barcode, Status, Tipe, Kategori, Satuan, Jenis Barang. Quick-add tetap hanya
menampilkan field inti/wajib, tetapi kini sebagai satu section agar alur input
lebih ringkas. Error validasi `code`/`name`/`categoryId`/`unitId` dan marker
terisi dirutekan ke section gabungan.

**Identitas & Klasifikasi dipecah kembali → 2 section (2026-07-11):**
Atas permintaan user (edit modal `/app/master/items`), nav `identitas`
menjadi **"Identitas"** (Kode, Nama, Barcode, Status) dan ditambah nav baru
`klasifikasi` **"Klasifikasi"** (Tipe, Kategori, Satuan, Jenis Barang) tepat di
bawahnya — urutan `identitas` → `klasifikasi` → `media` … Side-nav berubah dari
satu entry jadi dua, berurutan di group `inti`. Quick-add (`CEPAT_SECTIONS`)
sekarang merender **kedua** section berurutan (Identitas lalu Klasifikasi).
Error validasi `code`/`name` → nav `identitas`; `categoryId`/`unitId` → nav
`klasifikasi`. Marker terisi dipisah: identitas = `code`/`name`/`kindId`,
klasifikasi = `categoryId`/`unitId`.

**Atribut + Lain-lain + Custom digabung jadi 1 section (2026-06-12):**
Atas permintaan user, tiga entry side-nav terpisah (`atribut`, `lainlain`,
`custom`) dilebur jadi **satu** nav "Atribut". `renderById.atribut` kini
me-render `ItemAtributSection` + `ItemLainLainSection` + `ItemCustomSection`
berurutan dalam satu fragment — masing-masing tetap `<Section>` sendiri
(judul "Dimensi & Berat" · "Klasifikasi Produk" · "Penanganan & Regulasi" ·
"Lain-lain" · "Custom") jadi pemisahan visual tetap. `SectionId` `'lainlain'`/
`'custom'` dihapus. Komponen di `items-form-lainlain.tsx` tidak diubah (masih
nulis ke JSON sidecar `metadata.others`/`metadata.custom`, §2.38).

**Polish side-nav + file split (2026-06-13):**
Atas permintaan user ("input lebih mudah & mengesankan"), layout Lengkap
dinaikkan kelasnya tanpa mengubah data/flow:
- **Side-nav berkelompok + ikon** — section dikelompokkan jadi 4 grup
  (Inti · Detail · Keuangan · Dimensi & Supplier), tiap nav item ber-ikon
  (`Icon` set). Marker per item: dot merah (error) · centang hijau (terisi)
  · ring kosong (belum). "Terisi" diturunkan dari `sectionFilled(id,data)`.
- **Identity header live** (`ItemFormContextHeader`) di atas konten: tile
  ikon + Nama + Kode (mono) + badge tipe + badge Aktif/Nonaktif — selalu
  terlihat di kedua mode supaya konteks tak hilang saat pindah section.
- **Footer progress + Prev/Next** (`ItemFormFooter`): bar progress
  "Terisi X/Y" + posisi "Bagian i dari N" + tombol Sebelumnya/Berikutnya
  (navigasi antar section available). Hanya di mode Lengkap.
- **Mode toggle** dipindah ke header bar dgn label deskriptif + ikon
  (Cepat=tambah kilat, Lengkap=semua detail). Default tetap: Cepat untuk
  item baru, Lengkap saat edit.
- **File split (§3, <400 baris)**: `items-form-fields.tsx` jadi orchestrator
  tipis (94 baris); section bodies → `items-form-sections.tsx`; metadata nav
  + fill-detection + `SectionNav`/`ModeToggle` → `items-form-nav.tsx`;
  identity header + footer → `items-form-chrome.tsx`. `Section` (di
  `items-form-parts.tsx`) dapat prop `icon?` opsional (backward-compat).

**Conditional disclosure per itemType:**
- `INVENTORY/CONSUMABLE/ASSET` → tampilkan Inventory & Tracking
- `SERVICE/NON_INVENTORY` → sembunyikan Inventory & Tracking
- `SERVICE` → sembunyikan juga Berat (kg)
- `tracksBatch=Ya` → tidak menampilkan lagi field legacy Kategori Umur; tracking cukup via mode Batch/Lot

**Smart features:**
- Tombol **Auto** di samping field Kode → fetch item dgn prefix
  matching itemType (ITM-/SVC-/CNS-/AST-/NIN-), generate sequence
  berikutnya client-side. Helper di `lib/items-code-generator.ts`.
  Bukan transactional — `sys_document_numberings` endpoint
  ditangguhkan sampai BE-nya dibuat.
- **Duplikat** di row kebab menu → buka modal create dgn prefill,
  kode dikosongkan supaya user isi baru. Berlaku untuk semua master
  ERP (SimpleMasterPage), tidak hanya item.

**SimpleMasterPage enhancement (universal):**
Berlaku untuk **semua** halaman master ERP yang pakai
`SimpleMasterPage`, bukan items-only:
- **Validation summary banner** di top modal saat client-side validasi
  gagal — list 5 error pertama + count. Komponen
  `components/molecules/form-error-summary.tsx`.
- **Footer multi-save**: tombol "Simpan & Tambah Baru" muncul di create
  mode (selain "Simpan"). Save → reset form → auto-focus → modal tetap
  buka untuk batch entry.
- **Keyboard shortcuts**: Ctrl/Cmd+S = Simpan, Ctrl/Cmd+Enter = Simpan
  & Tambah Baru (create mode). Hook `lib/use-modal-shortcuts.ts`.
- **Row action Duplikat** di kebab + right-click menu (urutan: Edit →
  Duplikat → Riwayat → Hapus).
- `modalSize` prop diperluas: `'md' | 'lg' | 'xl'` (560/900/1100px).

**UX polish (item form):**
- Section header: bg `var(--panel-2)` muted + padding (bukan caps
  tipis tanpa bg). Tambah prop `hint` untuk one-liner di sebelah judul.
- Required label: semibold + foreground color (bukan muted) di atom
  `Label` — berlaku universal untuk semua form ERP, tidak hanya item.
  Optional label tetap regular muted.
- Helper text untuk field ambigu: Spesial, Tipe, Metode HPP, BKP,
  Harga berlaku s.d.
- Placeholder lookup field: "Cari xxx…" → "Pilih xxx…" (verb action
  konsisten dgn pattern dropdown).
- Field "Berlaku s.d" → "Harga berlaku s.d" + helper text agar
  konteks (masa berlaku harga, bukan masa berlaku item) jelas.

**Atomic refactor pendukung:**
- `components/molecules/form-error-summary.tsx` — molecule baru
- `components/molecules/bulk-action-bar.tsx` — extract dari organism
- `components/molecules/audit-modal.tsx` — extract dari organism
- `components/pages/items-form-parts.tsx` — helper Section/Lookup/
  Num/YesNo + visibility rules untuk items-form-fields
- `lib/use-modal-shortcuts.ts` — hook keyboard shortcuts reusable
- `lib/items-code-generator.ts` — generator client-side auto-code

**Konsekuensi vibe coding:**
- Membuat form master baru dgn banyak field (>20)? Pertimbangkan
  pattern Cepat/Lengkap + side-nav layout. Pola sudah ada di items;
  bisa di-port ke entitas lain bila kebutuhannya sama.
- Membuat row action baru di list? Reuse pola Edit → Duplikat →
  Riwayat → (sep) → Hapus di kebab menu. Tambah aksi entitas-spesifik
  di antara Duplikat dan Riwayat.

---

### 2.27 Master `code` = tanpa entity-scope prefix (2026-05-28)

Kolom `code` master ERP **tidak boleh** dipayungi prefix yang sekadar
mengulang entity scope-nya (mis. `CAT-XX` untuk item category, `BRD-XX`
untuk brand, `UNT-XX` untuk unit). Scope sudah implied oleh tabel & UI
breadcrumb — prefix = noise.

Aturan:

- Master code = bare semantic code (mis. `ZN` untuk Zinc, `MM` untuk Metal
  Misc). Bila legacy punya `legacyCode` 2–4 huruf yang stabil, pakai
  langsung sebagai `code`.
- Prefix tetap **valid** kalau:
  - Multi-segment semantik (mis. `RM-FB` = Raw Material → Fabric — segmen
    pertama bermakna sub-tipe, bukan entitas).
  - Namespace isolasi dataset (mis. `DUMMY-0001` di `seed-erp-md-dummy.ts`
    untuk memisahkan dummy dari real data — segmen `DUMMY` adalah
    dataset-tag, bukan entity-scope tag).
- Saat menulis seed/migration baru: jangan re-introduce pola `<ENTITY>-XX`.
  Auto-code generator (mis. `lib/items-code-generator.ts` untuk item code
  `ITM-/SVC-/CNS-` — itu **item type marker**, beda kasus dgn category).

Migrasi pendukung: `20260528_001_erp_strip_cat_prefix_item_categories`
(strip `CAT-` dari 30 row `md_item_categories.code`; FK aman karena semua
referensi pakai `categoryId` BigInt). Seed `seed-erp-items-real.ts` juga
disinkron — 28 entry `CATEGORIES` sekarang bare code (`AB`, `AL`, ... `ZN`).

Garment vertical (`db-design/seed-data-garment.md`) **tidak** dihabisi
prefiks-nya karena belum dieksekusi & vertical-spesifik — saat slicing
garment, terapkan aturan §2.27 ini.

---

### 2.32 Item — tab Harga paritas MyERP+ (price tiers 1–10) (2026-05-30)

> **🔄 UPDATE 2026-06-13 — tingkat harga jadi dinamis/unlimited (keputusan user).**
> Tab Harga **tidak lagi dibatasi 10 tingkat**. Aturan baru:
> - Item baru mulai dengan **1 tingkat**; tombol "+ Tambah tingkat" / "− Hapus
>   tingkat" menambah/menghapus **hanya di akhir** (level tetap kontigu 1..N).
> - **Tanpa batas atas** (unlimited). Mirror level-1 → `salePrice` tetap.
> - Frontend-only: `defaultItemForm()` → `salePrices/saleDiscounts = ['']`;
>   `tierColumn()` panjang = level tertinggi yang ada (min 1); `buildPriceTiers()`
>   loop sepanjang array. Kontrak API `prices: ItemPriceTier[]` sudah var-length
>   sejak awal — `md_item_prices.level` cukup longgar (Int, no cap di app layer).
> - File: `items-form-model.ts`, `items-form.tsx`, `items-form-sections.tsx`.

Section **Harga** di form item (§2.25) di-expand ke paritas tab "Harga"
MyERP+: 10 tingkat harga jual + diskon per tingkat. Mengakhiri "deferred
price tiers 2–10" dari §2.23.

**Model data = tabel anak ternormalisasi** (keputusan user 2026-05-30):
- `md_item_prices` (model `ErpItemPrice`): `itemId` FK (cascade), `level`
  (1–10), `price` Decimal(19,4), `discountPercent` Decimal(9,4), audit cols.
  `@@unique([itemId, level])`. **Bukan** kolom flat `salePrice1..10` —
  sejalan prinsip ternormalisasi + nyambung ke `md_partners.salesTier`
  (legacy `cctingkatjual`) untuk logika pricing modul Sales nanti.
- `md_items.purchaseDiscount` Decimal(9,4) — "Diskon Pembelian" (persen).
- Migrasi `20260530_001_erp_item_price_tiers` (additive, 0 DROP). **Hand-written
  SQL + `prisma migrate deploy`** (bukan `migrate dev`) — `migrate dev` gagal di
  shadow DB karena migrasi clinic lama tidak replay bersih; DB live sendiri
  `up to date`. Pola ini berlaku untuk semua migrasi ERP berikutnya.

**Pemetaan field MyERP+ → schema (jangan bikin kolom redundan):**
- "Harga Beli Terakhir" → `purchasePrice` (sudah ada).
- "Hpp rata-rata" → `averageCost` (computed sistem; data internal, **tidak**
  ditampilkan di form dan **tidak** dikirim di payload).
- "Hpp Update" → `standardCost` (manual/standard HPP, legacy `bhpp`) — **bukan**
  kolom baru.
- "Harga Jual 1..10" / "Diskon Jual 1..10" → `md_item_prices` rows.
- `md_items.salePrice` tetap ada = **cache denormalized level-1** (di-set dari
  `prices[level=1].price` saat simpan; dibaca modul lain). SSOT 10 tier =
  `md_item_prices`.

**Backend:** DTO `ItemPriceDto` (level 1–10 + price/discountPercent string),
`prices?: ItemPriceDto[]` di create DTO (`@ValidateNested`). Service
`buildPriceRows()` skip level yang price+diskon kosong; create = nested
`prices.create`; update = `prices: { deleteMany: {}, create }` (replace
penuh). `ITEM_INCLUDE.prices` + `mapItem` stringify Decimal. **Cache
`md_items.salePrice` di-derive server-side** dari tier level-1 via
`deriveSalePriceFromTiers()` (di `erp-items.mappers.ts`), di-set setelah
`buildDecimalData` sehingga **override** `salePrice` kiriman client — cache
selalu sinkron walau caller (mis. API mentah) tak mengirim `salePrice`. Blank
L1 price → cache tidak disentuh.

**Frontend:** `ItemFormData.salePrices`/`saleDiscounts` = `string[10]` (index
0 = level 1) + `purchaseDiscount`; `averageCost` boleh tetap tersimpan di model
sebagai data internal dari API, tetapi tidak dirender. `fromItem` expand sparse
rows → 10 slot (`tierColumn`); `toItemPayload` collapse → sparse rows
(`buildPriceTiers`, skip kosong) + `salePrice = salePrices[0]`. Layout = 2-kolom
paired (Harga Jual N kiri ‖ Diskon Jual N kanan), buy-side visible = Harga Beli
Terakhir + HPP Terakhir + Diskon Pembelian di atas.

---

### 2.33 Item — tab Akun paritas MyERP+ (8 akun GL) (2026-05-30)

Section **Akuntansi** di form item (§2.25) di-expand ke paritas tab "Akun"
MyERP+: dari 3 akun → **8 akun GL** (urutan legacy). Tambahan 5 akun:
Retur Penjualan, Diskon Penjualan, Retur Pembelian, Diskon Pembelian,
Konsinyasi (Persediaan/Penjualan/HPP sudah ada).

- **`md_items` kolom baru** (semua nullable BigInt → `md_accounts`):
  `salesReturnAccountId`, `salesDiscountAccountId`, `purchaseReturnAccountId`,
  `purchaseDiscountAccountId`, `consignmentAccountId`. Relasi `ItemSalesReturnAcct`
  dst di `ErpItem` + back-pointer di `ErpAccount`. Migrasi
  `20260530_002_erp_item_legacy_gl_accounts` (additive, 0 DROP, FK `ON DELETE
  SET NULL`). Hand-written SQL + `migrate deploy` (pola §2.32).
- **Wajib hanya saat `type=INVENTORY`** (keputusan user 2026-05-30). DB kolom
  tetap nullable (aman untuk 100+ item lama + tipe SERVICE/NON_INVENTORY yang
  tak butuh akun ini). Required di-enforce **FE-only** lewat `validateItem`
  (`REQUIRED_INVENTORY_ACCOUNTS` + `requiredWhenInventory`) — legacy menandai
  semua 8 wajib, tapi modern kita kondisikan ke tipe stok. Backend DTO semua
  optional.
- **UX bridge:** akun GL tidak terlihat di mode entri **Cepat** (§2.25). Kalau
  validasi akun gagal saat simpan di Cepat, `items-form-fields.tsx` auto-switch
  ke **Lengkap** + buka section Akuntansi (efek `accountError && mode==='cepat'`)
  supaya error bisa diperbaiki. Label section pakai nama legacy ringkas
  (Persediaan, Penjualan, Retur Penjualan, …) bukan "Akun Persediaan".

---

### 2.34 Item — section Lokasi multi-gudang (placements) (2026-05-30)

Section **Lokasi** baru di form item (§2.25), paritas tab "Lokasi" MyERP+
(`m1_item_location_warehouse`): per item, daftar baris **(Gudang, Lokasi)** =
penempatan item di banyak gudang/spot. Pelengkap `defaultWarehouseId`/
`defaultLocationId` yang tetap single default.

- **Data model = junction ternormalisasi `md_item_placements`** (model Prisma
  **`ErpItemPlacement`**, bukan `ErpItemLocation`): `itemId` FK (cascade),
  `warehouseId` FK → `md_warehouses` (**Gudang**), `locationId` FK →
  **`md_item_locations`** (**Lokasi** = master named-spot legacy, `code`/`name`/
  `warehouseId`), audit cols. `@@unique([itemId, warehouseId, locationId])`.
  **Penting (keputusan user 2026-05-30):** "Lokasi" = master spot
  `md_item_locations` (sudah punya modul + halaman + seed sendiri), **bukan**
  `md_locations` (itu level cabang/site). Nama tabel `md_item_locations` sudah
  dipakai master spot → junction pakai nama `md_item_placements`.
- Migrasi `20260530_003_erp_item_locations` (CREATE `md_item_placements`,
  additive 0 DROP) + koreksi `20260530_004_erp_item_placement_location_fk`
  (repoint FK `location_id` dari draft `md_locations` → `md_item_locations`;
  draft awal salah target). Hand-written SQL + `migrate deploy` (pola §2.32).
- **Backend:** DTO `ItemLocationDto` (`warehouseId`+`locationId` string),
  `locations?: ItemLocationDto[]` di create DTO. Relasi Prisma di service =
  `placements` (create / `deleteMany`+create saat update); `buildLocationRows`
  skip baris tak-lengkap + **dedupe** pasangan. `mapItem` memetakan
  `placements` → field FE `locations` (`{warehouseId, locationId, warehouse,
  location}`). Helper murni (include graph + builders + `mapItem`) di-extract
  ke **`erp-items.mappers.ts`** supaya service tetap < 400 baris (§3).
- **Frontend:** `ItemFormData.locations: ItemLocationFormRow[]` (punya `key`
  stabil client agar display `SearchSelect` tahan add/remove baris — tidak
  dikirim). Section `lokasi` di side-nav **Lengkap**, **hanya untuk tipe
  stockable** (INVENTORY/CONSUMABLE/ASSET), placement: setelah Inventory.
  Editor multi-baris = organism
  [`components/pages/items-form-locations.tsx`](components/pages/items-form-locations.tsx)
  (tabel No · Gudang · Lokasi · hapus + tombol "Tambah"). Loader Lokasi =
  `loadItemLocationOptions` (master spot), **bukan** `loadLocationOptions`.
  `toItemPayload` drop baris yang Gudang/Lokasi belum lengkap.

**⚠ Naming gotcha — `md_item_locations` ≠ `md_item_placements`** (digabung dari bekas §2.34 "Naming reservation"):

**`ErpItemLocation` / `md_item_locations` = master "Item Location" legacy**
(code/name/warehouse, modul `erp-item-locations/` + halaman + data ada).
**JANGAN** pakai nama ini untuk junction "tab Lokasi" item. Junction per-item
(Gudang + Lokasi) = **`ErpItemPlacement` / `md_item_placements`** (relasi
`ErpItem.placements`, migrasi `20260530_003`). Bug history: fitur tab Lokasi
sempat mendefinisikan ulang `model ErpItemLocation @@map("md_item_locations")`
→ skema invalid (duplicate model) + migrasi `CREATE TABLE IF NOT EXISTS`
jadi no-op (tabel master sudah ada) → **semua** operasi item gagal dengan
`PrismaClientValidationError` (filter `all-exceptions.filter.ts` menamai-nya
"Invalid query parameters" — menyesatkan, bukan soal query param). Resolusi:
rename junction ke `ErpItemPlacement` + relasi field `placements`. **Catatan
ops:** tiap rename schema item **wajib** `prisma generate` **di dalam container**
+ restart (named-volume `api_gateway_node_modules` ≠ host; `nest --watch`
recompile TS tapi **tidak** regen Prisma client → client basi = error di atas).

---

### 2.36 Item — tab Distributor (item-supplier list) (2026-05-31)

Section **Distributor** baru di form item (§2.25), paritas tab "Distributor"
item master MyERP+ (`m1_item_supplier`): per item, daftar baris **partner
distributor/supplier**. Tabel legacy hanya menampilkan **Kode + Nama** partner →
modern kita simpan referensi partner saja (tanpa kolom catatan/SKU; keputusan
user 2026-05-31). **Coexist** dengan field tunggal `primarySupplierId` di section
**Supplier**: yang single = supplier utama/default; tab Distributor = daftar
distributor/supplier tambahan.

- **Data model = junction ternormalisasi `md_item_distributors`** (model Prisma
  **`ErpItemDistributor`**): `itemId` FK (cascade), `partnerId` FK → `md_partners`,
  `sortOrder` (jaga urutan tampil legacy), audit cols. `@@unique([itemId,
  partnerId])` — satu partner tak duplikat per item. Relasi back-pointer
  `ErpPartner.itemDistributors`.
- Migrasi `20260531_002_erp_item_distributors` (CREATE `md_item_distributors`,
  additive 0 DROP; FK item cascade, partner `ON DELETE RESTRICT`). Hand-written
  SQL + `migrate deploy` (pola §2.32). **Catatan ops:** `prisma generate` **di
  dalam container** + restart wajib setelah schema berubah (lihat §2.34).
- **Backend:** DTO `ItemDistributorDto` (`partnerId` string `@IsNotEmpty`),
  `distributors?: ItemDistributorDto[]` di create DTO (inherited di update via
  `PartialType`). `buildDistributorRows` (di `erp-items.mappers.ts`) skip partner
  kosong + **dedupe by partner** + isi `sortOrder` urut index; create = nested
  `distributors.create`, update = `distributors: { deleteMany: {}, create }`
  (replace penuh, pola placements/prices). `ITEM_INCLUDE.distributors` +
  `mapItem` memetakan ke field FE `distributors` (`{partnerId, partner}`),
  `orderBy: [{ sortOrder }, { id }]`.
- **Frontend:** `ItemFormData.distributors: ItemDistributorFormRow[]` (punya `key`
  stabil client agar display `SearchSelect` tahan add/remove baris — tidak
  dikirim). Section `distributor` di side-nav **Lengkap** (`available: true`,
  **tidak** dibatasi tipe stockable — distributor relevan untuk semua tipe item),
  urutan setelah Supplier. Editor multi-baris = organism
  [`components/pages/items-form-distributors.tsx`](components/pages/items-form-distributors.tsx)
  (tabel No · Distributor · hapus + tombol "Tambah"). Loader =
  `loadSupplierOptions` — partner **difilter `isSupplier=true`** (keputusan user
  2026-05-31), bukan semua partner. `toItemPayload` drop baris yang partner belum
  dipilih.

---

### 2.37 Item — tab Branch multi-cabang (item-branch) (2026-05-31)

Section **Branch** baru di form item (§2.25), paritas tab "Branch" item master
MyERP+: per item, daftar baris **(Cabang, Cost Center)** = penempatan item di
banyak cabang dengan cost center per cabang. **Coexist** dengan field tunggal
`branchId` + `costCenterId` di section **Dimensi GL** (keputusan user
2026-05-31): yang single tetap = cabang/cost center **home/default**; tab Branch
= daftar penempatan tambahan. **Cost Center wajib** per baris (Cabang + Cost
Center sama-sama wajib agar baris tersimpan).

- **Data model = junction ternormalisasi `md_item_branches`** (model Prisma
  **`ErpItemBranch`**): `itemId` FK (cascade), `branchId` FK → `md_branches`
  (**Cabang**), `costCenterId` FK → `md_cost_centers` (**Cost Center**), audit
  cols. `@@unique([itemId, branchId])` — satu cost center per cabang per item.
  Relasi back-pointer `ErpBranch.itemBranches` + `ErpCostCenter.itemBranches`.
- Migrasi `20260531_003_erp_item_branches` (CREATE `md_item_branches`, additive
  0 DROP; FK item cascade, cabang/cost center `ON DELETE RESTRICT`). Hand-written
  SQL + `migrate deploy` (pola §2.32). **Catatan ops:** `prisma generate` **di
  dalam container** + restart wajib setelah schema berubah (lihat §2.34).
- **Backend:** DTO `ItemBranchDto` (`branchId`+`costCenterId` string, keduanya
  `@IsNotEmpty`), `branches?: ItemBranchDto[]` di create DTO (inherited di update
  via `PartialType`). `buildBranchRows` (di `erp-items.mappers.ts`) skip baris
  tak-lengkap + **dedupe by branch**; create = nested `branches.create`, update =
  `branches: { deleteMany: {}, create }` (replace penuh, pola placements/prices).
  `ITEM_INCLUDE.branches` + `mapItem` memetakan ke field FE `branches`
  (`{branchId, costCenterId, branch, costCenter}`).
- **Frontend:** `ItemFormData.branches: ItemBranchFormRow[]` (punya `key` stabil
  client agar display `SearchSelect` tahan add/remove baris — tidak dikirim).
  Section `branch` di side-nav **Lengkap** (`available: true`, **tidak**
  dibatasi tipe stockable — cabang relevan untuk semua tipe item), urutan
  setelah Distributor. Editor multi-baris = organism
  [`components/pages/items-form-branches.tsx`](components/pages/items-form-branches.tsx)
  (tabel No · Cabang · Cost Center · hapus + tombol "Tambah"). Loader =
  `loadBranchOptions` + `loadCostCenterOptions`. `toItemPayload` drop baris yang
  Cabang/Cost Center belum lengkap.

---

## §2.35 — Item master "Atribut" tab (legacy MyERP+ parity) (2026-05-31)

Menambahkan tab **Atribut** ke form item (`/app/master/items`), meniru tab
"Atribut" legacy MyERP+ dengan UI/UX dimodernkan (amati-tiru-modifikasi).

**Keputusan dengan user:**
- **Model lookup = reuse master existing + tambah minimal** (BUKAN tabel generik
  `md_item_attributes`). 6 master atribut sudah ada (Warna/Merk/Ukuran/Material/
  Section/Desainer) dengan kolom FK di `md_items` tapi belum punya relasi Prisma →
  relasi di-wire sekarang. **Vendor → reuse `md_partners`**. **Satuan Lapangan →
  reuse `md_units`** (relasi `ItemFieldUnit`; `baseUnit` dinamai `ItemBaseUnit`).
  Alasan menolak tabel generik: destruktif (drop 6 tabel+FK), langgar norma
  migrasi additive §2.32 (0 DROP), buang kerja yang sudah jalan.
- **Nozzle & Oem benar-benar baru** → master kecil baru `md_nozzles` + `md_oems`
  (mirror `md_colors`: code+name+isActive), konsisten pola master atribut lain
  (tabel + FK + halaman CRUD + SearchSelect). Bukan teks bebas.
- **Cakupan = semua field legacy** (~20). Scalar baru di `md_items`:
  `length/width/height/volume` (Decimal nullable), `conversion_kg_pcs`
  (Decimal default 1), `registration_no` (No. Ijin Edar), `is_returnable`
  (Retur, default **true** sesuai legacy), `is_mobile` (default false).
- **Layout = 1 tab "Atribut" bergrup** (best practice, bukan grid datar legacy):
  3 grup → **Dimensi & Berat** (Panjang/Lebar/Tinggi/Volume/Berat/Konversi
  Kg-Pcs) · **Klasifikasi Produk** (Warna/Merk/Ukuran/Material/Section/Desainer/
  Nozzle/OEM/Vendor) · **Penanganan & Regulasi** (Satuan Default/No. Ijin Edar/
  Retur/Mobile). Section "Atribut" disisipkan di side-nav setelah "Klasifikasi".
  Label UI untuk relasi `fieldUnitId` memakai **Satuan Default** (bukan "Satuan
  Jual Default") agar netral dan tidak menyiratkan hanya untuk penjualan.
- **Berat dipindah** dari section Klasifikasi → grup Dimensi & Berat (hapus
  `showsWeight` gate di Klasifikasi). **Serial/Batch tetap** di section Inventory
  (`tracksSerial`/`tracksBatch`) — tidak diduplikasi di Atribut.
- **itemModel** tidak di-wire (tidak ada di screenshot Atribut legacy).

**Implementasi:**
- DB: migrasi `20260531_001_erp_item_attributes` (additive, idempotent, 0 DROP) —
  13 kolom `md_items` + tabel `md_nozzles`/`md_oems` + FK constraints (termasuk
  untuk kolom brand/material/size/color/section yang dulu belum ber-constraint).
- Backend: `ErpItem` relasi + 2 model baru; `erp-items` DTO/mappers/service
  (`FK_OPTIONAL_FIELDS`+`DECIMAL_FIELDS`+`ITEM_INCLUDE` + create/update flag);
  modul `erp-nozzles`/`erp-oems` (mirror `erp-colors`, guard `ErpJwtAuthGuard`
  §2.5) terdaftar di `app.module.ts`; menu di-seed di `seed-erp.ts`
  (`M1.ITEM.NOZZLE` `/master/nozzles`, `M1.ITEM.OEM` `/master/oems`).
- Frontend: `lib/api/{nozzles,oems}.ts`; loaders di `items-form-lookups.ts`;
  `items-form.tsx` (ItemFormData/default/fromItem/toItemPayload); section UI
  reusable `items-form-atribut.tsx`; halaman master `{nozzles,oems}-page.tsx`
  (pakai organism `SimpleMasterPage`) terdaftar di `ERP_PAGES`
  (shell-route-renderer) + `NAV`/`ERP_ROUTE_META` (`lib/nav.ts`).
- Verifikasi: `tsc --noEmit` BE+FE 0 error, `check:size` clean, migrasi applied,
  endpoint `/api/erp/{nozzles,oems}` 401 (route+guard OK), menu seeded.

---

### 2.38 Item — tab Lain-lain + Custom (metadata JSON sidecar) (2026-05-31)

Menambahkan dua tab terakhir form item legacy MyERP+ (`/app/master/items`):
**Lain-lain** dan **Custom** (amati-tiru-modifikasi). Beda dari tab lain
(Atribut/Distributor/Branch yang pakai kolom/tabel riil), kedua tab ini
**disimpan di `md_items.metadata` (Json)** — **tanpa migrasi/kolom baru**
(keputusan user 2026-05-31: storage = metadata Json).

- **Lain-lain → `metadata.others`:** `aliasName1..4` (Nama Alias 1–4),
  `notesRc` (Notes RC), `catatan` (Catatan).
- **Custom → `metadata.custom`:** `productionCategory`, `productionGroup`,
  `maxQtySo`, `capacityPerHour`, `maxQtyRc`, `allowance`, `wip1..3`,
  `mouldFinish`, `moldSemi1..2`, `min1`/`max1`/`min2`/`max2`. Field kuantitas
  (Max Qty SO/RC, Kapasitas Per Jam, Allowance) = `NumField`; sisanya teks.
  Field lookup legacy (Kategori/Kelompok Produksi, WIP, Mould — punya ikon
  search) **di-modernisasi jadi teks bebas** karena belum ada master-nya;
  promosikan ke lookup saat master dibuat.

**Implementasi (tanpa Prisma/migrasi — kolom `metadata` sudah ada):**
- Backend: DTO nested `ItemOthersDto`/`ItemCustomDto`
  ([`dto/item-metadata.dto.ts`](../../api-gateway/src/erp-items/dto/item-metadata.dto.ts))
  + field `others`/`custom` di `CreateErpItemDto` (`@ValidateNested`). Helper
  `buildItemMetadata(dto, existing?)` di `erp-items.mappers.ts` merakit
  `metadata` (compact buang nilai kosong, **merge** ke metadata existing supaya
  key lain selamat, clear namespace bila semua blank, `undefined` = jangan
  sentuh kolom). Di-wire ke `create` + `update`. `mapItem` sudah meneruskan
  `metadata` apa adanya via `...rest` (tak perlu diubah).
- Frontend: tipe `ItemOthersData`/`ItemCustomData`/`ItemMetadata` +
  `ErpItem.metadata` + `CreateItemPayload.others/custom` di `lib/api/items.ts`;
  `ItemFormData.others/custom` + adapter `fromItem` (baca `item.metadata`) /
  `toItemPayload` di `items-form.tsx`; section UI reusable
  [`items-form-lainlain.tsx`](components/pages/items-form-lainlain.tsx)
  (`ItemLainLainSection` + `ItemCustomSection`), didaftarkan di side-nav
  `items-form-fields.tsx` setelah "Catatan".
- **Catatan ops:** tak ada migrasi & tak perlu `prisma generate`. BE host pakai
  `nest start --watch` (auto-reload). Bila API live disajikan dari container
  Docker, perlu rebuild/restart container agar perubahan TS ikut (pola §2.32).
- Verifikasi: `tsc --noEmit` BE+FE 0 error untuk file item.

---

### 2.43 Master Mata Uang = seed full ISO 4217 (2026-05-31)

Master Mata Uang (`md_currencies` / model `ErpCurrency`) di-seed dengan daftar
**lengkap ISO 4217 aktif** (189 entri, termasuk IDR + logam mulia XAU/XAG/XPT/XPD),
bukan hanya IDR. Data array hidup di modul terpisah
[`apps/api-gateway/prisma/data/iso-4217-currencies.ts`](../../api-gateway/prisma/data/iso-4217-currencies.ts)
(interface `Iso4217Currency` = `code`/`name`/`symbol?`) dan di-import oleh
`seedCurrency()` di `prisma/seed-erp.ts`. Upsert **idempotent** keyed on `code`
(`create` baru + `update` name/symbol untuk yang sudah ada), `isActive: true`
untuk semua. `symbol` diisi bila ada simbol baku; dikosongkan untuk kode tanpa
simbol umum (mis. XDR, BOV).

Alasan: halaman master Currencies (`/master/currencies`) sebelumnya cuma punya
IDR; aplikasi butuh pilihan mata uang dunia lengkap. Data dijadikan modul
terpisah agar `seed-erp.ts` tetap ringkas (file seed exempt dari batas 400 baris).
Frontend (`components/pages/currencies-page.tsx` + `lib/api/currencies.ts`) tidak
berubah — list dibaca dari API `GET /api/erp/currencies`.

**UI kurs nilai tukar (2026-07-14):** form tambah kurs di modal Edit Mata Uang
(`components/pages/currencies-rates.tsx`) memakai input **Periode berlaku**
(`DateRangePicker`, Mulai → Selesai). Model data/API rate tetap **satu baris
per tanggal** (`md_currency_rates.rate_date` unique per currency).

**Expand range → per-hari (2026-07-14, user):** saat user pilih Mulai→Selesai
(mis. 08/07/2026 → 17/08/2026) + satu nilai rate, FE **me-expand rentang
inklusif** jadi N baris (`expandRateDates`) dan `POST` upsert sequential per
`rateDate` (reuse `addCurrencyRate` / backend upsert). Tanpa Selesai = 1 hari.
Soft cap **366 hari** agar typo multi-tahun tidak membanjiri API. List rate
limit dinaikkan ke 200 agar hasil bulk terlihat. Kolom tabel = **Tanggal**
(satu hari per baris). Rate memakai `NumInput` (§2.31), display `formatDate()`
(§2.39), angka right-aligned `tabular-nums` (§2.9).

**DateRangePicker: klik Mulai tidak menutup popover (2026-07-14):** bug UX di
kalender range — klik tanggal awal langsung menutup popover sehingga user
tidak sempat memilih tanggal akhir. Root cause: react-day-picker v9
(`min = 0` default) mengembalikan `{ from, to }` **hari yang sama** pada
klik pertama; `handleSelect` lama menutup popover begitu `from && to`.
Fix awal: klik pertama = start saja, klik kedua = end + tutup.

**DateRangePicker: tombol Terapkan (2026-07-14, follow-up user):** popover
kalender **tidak auto-close** setelah pilih start/end. Selection di
DayPicker = **draft** (state lokal); commit ke parent hanya lewat tombol
**Terapkan** di footer popover (Hapus = clear draft). Input teks Mulai/
Selesai di bar tetap live/langsung. Berlaku di semua konsumen
`DateRangePicker` (filter list, kurs mata uang, project, dll).

---

### Header form transaksi = render 100% dari config (no hardcoded layout) (2026-06-01)

**Keputusan user:** "tidak boleh ada yg statis via source code, harus dinamis full
via form builder." Dipilih level **"render dinamis, binding tetap"** (frontend-only,
backend tak berubah) — bukan full-decouple.

Sebelumnya `cash-bank-transaction-form.tsx` me-render 8 field struktural sebagai
blok `<Field>` JSX hardcoded (urutan & slot terkunci di source), custom field
di-append di belakang. Sekarang header = **satu loop config-ordered** per slot
(LEFT/CENTER/RIGHT), urut `sortOrder`, hormati `isVisible`, dispatch per `kind`:

- **Struktural** → `components/molecules/cash-bank-structural-field.tsx`
  (`CashBankStructuralField` + `StructuralFieldCtx`). `switch` atas 8 `fieldKey`
  = **binding field→kolom `CashBankFormData`** (input spesial: `docNumber` Auto,
  `currencyId` Kurs). Binding ini sengaja tetap di source — itulah arti "binding
  tetap"; yang dinamis = layout/urutan/visibilitas/label/atribut.
- **Custom** → `cash-bank-custom-fields.tsx` direfactor ke **singular**
  `CashBankCustomField` (row tunggal), bukan lagi plural per-slot.
- Baris label+kontrol bersama diekstrak ke `components/molecules/form-field-row.tsx`
  (`FormFieldRow`) — hapus duplikasi `Field` di 2 file.

**Hook (`lib/use-form-fields.ts`):** `FormFieldsConfig` kini `{ byKey, slotFields }`
(`slotFields` = SEMUA field per slot, sorted; `bySlot`/`custom` lama dihapus).
Export `buildFormConfig(fields)`. Fallback `DEFAULT_FORM_FIELDS`
(`lib/api/form-fields.ts`) = layout struktural bawaan saat config belum load /
form tanpa `transactionCode` (cegah flash kosong); form meng-inject label arah
(`labels.partner`/`labels.account`) ke fallback agar "Terima Dari"/"Bayar Ke" benar.

**Type field sistem tetap di-GUARD (tidak bisa diubah).** Tiap field struktural
terikat kolom DB + posting GL (`bankAccountId` wajib ACCOUNT, `transactionDate`
wajib DATE, dst), jadi `fieldType`-nya **tidak** boleh diganti bebas (akan memecah
posting). Di builder (`form-builder-fields.tsx`) Tipe field sistem = **Select
disabled + tooltip** "terikat kolom DB & posting GL" (bukan teks mati). Yang tetap
editable untuk field sistem: label, sumber/filter/sort (gear), visible, wajib,
slot, urutan, placeholder, default, readonly. Field **custom** = bebas penuh
(termasuk tipe & sumber). Kalau type field sistem benar-benar perlu bebas → itu
**full-decouple backend** (JSON bag + posting baca by-key) = fase terpisah, di luar
keputusan ini.

**Scope:** form kas/bank (CR/CD/BD/RM) yang share `cash-bank-transaction-form.tsx`.
Jurnal/giro form terpisah → adopsi pola yang sama bila diperlukan.

## Item form — field "Kelas Produk" pindah ke section Custom (2026-06-12)

Atas permintaan user: lookup **Kelas Produk** (`productClassId`) dipindah dari
section **Klasifikasi** ke section **Custom** di item form
(`items-form-lainlain.tsx` `ItemCustomSection`, baris pertama sebelum atribut
produksi). Binding data tidak berubah — tetap kolom `md_items.product_class_id`
(bukan sidecar `metadata.custom`); hanya penempatan UI. Klasifikasi kini:
Tipe · Kategori · Satuan · Jenis Barang.

---

## Item form — tracking stok eksklusif Batch / Serial / Tidak pakai (2026-07-10)

Keputusan user dari halaman `/app/master/items`, section **Pergerakan Stok**:
mode tracking item harus dipilih sebagai **salah satu** dari **Batch / Lot**,
**Serial No.**, atau **Tidak pakai**. UI tidak boleh lagi menampilkan dua toggle
independen `Serial No.` dan `Batch / Lot`, karena itu memungkinkan kedua flag
aktif bersamaan dan membingungkan saat mutasi stok.

Implementasi frontend tetap memakai kolom/API existing `tracksBatch` +
`tracksSerial` agar tidak perlu migrasi DB. Sejak polish 2026-07-11, tiga opsi
ditampilkan sebagai **kartu deskriptif eksklusif** (arti + contoh penggunaan),
bukan segmented control ringkas, supaya konsekuensi pilihan langsung dipahami
admin ERP. Molecule reusable `DescriptiveRadioCards` menjaga native radio
semantics, keyboard/focus, dan token design system; `items-form-sections.tsx`
memetakan pilihan ke dua boolean secara immutable.

Adapter save `toItemPayload()` menormalisasi ulang dengan prioritas `serial`
bila data lama pernah punya dua flag aktif, sehingga payload keluar selalu
eksklusif. Field legacy `Kategori Umur`/`ageCategory` tidak ditampilkan di UI
Batch/Lot karena copy dan fungsinya membingungkan; kolom tetap dipertahankan di
API/DB untuk kompatibilitas data lama.

---

## Kategori Item — mapping 8 akun GL (2026-06-12)

Paritas legacy MyERP+ "Kategori Produk": `md_item_categories` kini memetakan
**8 akun GL default** per kategori, bukan 3. Existing: `inventory_account_id`
(Persediaan), `cogs_account_id` (HPP), `sales_account_id` (Penjualan). Baru
(migrasi `20260612_001_erp_item_category_gl_accounts`): `sales_return_account_id`,
`sales_discount_account_id`, `purchase_return_account_id`,
`purchase_discount_account_id`, `consignment_account_id` — semua `BigInt NULL`
FK → `md_accounts` (`ON DELETE SET NULL`), mirror pola 8 relasi akun yang sudah
ada di `md_items`.

- **Backend**: `CreateErpItemCategoryDto` +5 field (Update via `PartialType`);
  service wire create/update + `ACCOUNT_INCLUDES` (8 relasi `{id,code,name}`)
  di `findAll`/`findOne` supaya form dapat label.
- **Frontend**: `item-categories-page.tsx` — section "Akun GL" grid 2 kolom,
  8 `SearchSelect` map-driven (`ACCOUNT_FIELDS`), loader reuse
  `loadAccountOptionsCoded` (trigger "code - name"). Types di
  `lib/api/item-categories.ts` via interface `ItemCategoryAccountIds`.
- Semua akun **opsional** (nullable) — kategori boleh dibuat tanpa mapping;
  fallback resolusi akun per-item/per-transaksi tetap berlaku.

---

## Item Harga tab — biaya otomatis dari pembelian + tier jual per kategori (2026-06-12)

Empat keputusan terkait penetapan harga item (tab **Harga** di master item),
dikonfirmasi dengan user — scope **end-to-end** (master + form transaksi + posting).

**1. Harga Beli Terakhir + HPP Terakhir = otomatis dari pembelian terakhir.**
- Schema: kolom baru `md_items.last_hpp` (`Decimal(19,4)`, default 0; migrasi
  `20260612_007_erp_item_last_hpp`) = **HPP Terakhir** (net landed cost = harga
  satuan − diskon dari Goods Receipt terbaru). `purchase_price` tetap = **Harga
  Beli Terakhir** (gross). `average_cost` tetap = **HPP Rata-rata** (moving avg).
- Posting: `PurGoodsReceiptPostingService.postToLedger` (dipanggil saat GRN POST,
  di dalam `$transaction`) kini meng-update tiap item baris: `purchasePrice` =
  `unitPrice` gross, `lastHpp` = net (helper `netUnitCost`: `unitCost` menang,
  lalu diskon amount/qty, lalu diskon %), dan **seed** `averageCost` = `lastHpp`
  hanya bila masih 0. Moving-average penuh menunggu pass `inv_*` stock movement
  (GRN GL posting masih NO-OP). Reopen/repost = re-stamp; cost stamp tidak di-reverse.
- UI: field biaya yang visible di tab Harga jadi **read-only** (`Harga Beli Terakhir`,
  `HPP Terakhir`), help "Otomatis dari transaksi pembelian terakhir". `HPP Rata-rata`
  (`averageCost`) tetap data internal/seed untuk moving-average, tapi **tidak ditampilkan**
  di form item atas keputusan user 2026-07-11.

**2. "HPP Update" (manual standardCost) dihapus** dari form item. Kolom
`md_items.standard_cost` **tetap ada** (non-destruktif) tapi tidak lagi
di-input user: dibuang dari `CreateErpItemDto`, `DECIMAL_FIELDS` mapper,
`ItemFormData`, `fromItem`/`toItemPayload`, dan `CreateItemPayload` FE.

**3. Diskon Pembelian item = default baris PR/PO/RI/PRT (bisa diubah).** Saat
item dipilih di grid pembelian (`pur-item-lines.tsx`, dipakai semua dokumen via
`purchase-transaction-form.tsx`), fetch `getItemForPurchaseAutoFill` →
default `discountPercent` dari `item.purchaseDiscount`, plus `unitPrice` dari
Harga Beli Terakhir, satuan dasar, dan pajak beli. Semua hanya mengisi sel yang
masih kosong — operator tetap bisa override per baris.

**3a. Pajak item = dua slot beli + dua slot jual (2026-07-11).** Tab **Pajak**
di master item menampilkan **Pajak Beli 1**, **Pajak Beli 2**, **Pajak Jual 1**,
**Pajak Jual 2**. Schema/API menambah kolom nullable `purchase_tax2_id` dan
`sale_tax2_id` (FK `md_taxes`, additive; kolom lama menjadi slot 1). Pajak 2
disimpan end-to-end agar bisa dipakai auto-fill transaksi lanjutan; auto-fill
baris yang sudah ada tetap mengisi slot pajak pertama sampai grid transaksi
dikurasi untuk dua pajak.

**4. Tingkat Harga/Diskon Jual (1–10) ditentukan kategori pelanggan.** Pakai
kolom existing `md_partner_categories.sales_tier` (`salesTier`, 1–10).
- Backend: DTO partner-category +`salesTier` (`@IsInt @Min(1) @Max(10)`),
  service create/update persist; `erp-partners.service` select `category.salesTier`
  agar `/partners/:id` mengembalikannya.
- FE: `partner-categories-page.tsx` — input "Tingkat Jual" muncul saat
  kind=CUSTOMER (+ kolom list + validasi 1–10). Saat pelanggan dipilih di form
  jual (`sls-structural-field.tsx`), `data.salesTier` di-set dari
  `partner.category.salesTier`. Diteruskan ke `SlsItemLinesEditor` (`salesTier`
  prop); saat item dipilih, harga & diskon baris di-default dari
  `item.prices[level=salesTier]` (fallback Harga Jual 1 bila tier tak ada).

---

## Item Media — galeri gambar produk + video pendek (2026-06-12)

UI/UX upload media di master Item (request user: upload image produk dengan
preview ala ERP modern + video pendek per item).

**DB & backend (api-gateway):**
- Tabel baru `md_item_media` (`ErpItemMedia`, migrasi `20260612_008_erp_item_media`):
  child `md_items` cascade, `kind` enum `ErpItemMediaKind` (`IMAGE`|`VIDEO`),
  `fileName` (asli) + `storedName` (acak `<itemId>-<uuid>.<ext>`, unique),
  `mimeType`/`sizeBytes`/`sortOrder`/`isPrimary`.
- Aturan: **max 8 gambar** per item, **satu** ber-flag `isPrimary` (gambar
  pertama auto-primary; hapus primary → promosi gambar berikutnya); **1 video**
  per item — upload video baru menghapus video lama (file+row). Whitelist mime
  (jpeg/png/webp/gif · mp4/webm/mov), limit 5MB gambar / 50MB video; ekstensi
  diturunkan dari mime, bukan nama file user.
- File binary di `apps/api-gateway/uploads/erp-items/` (gitignored; persist di
  host via bind mount `../apps/api-gateway:/app`). Bukan static-assets global:
  streaming lewat endpoint ber-guard `ErpJwtAuthGuard`
  (`GET /erp/items/:itemId/media/:mediaId/file`, `res.sendFile` + Range →
  video bisa seek; cookie `erp_token` ikut karena same-origin).
- Endpoint: `GET /erp/items/:itemId/media` (list) · `POST` multipart
  `file`+`kind` (upload, multer memory storage spt erp-import) ·
  `PATCH :mediaId/primary` · `DELETE :mediaId`. Module: controller+service
  baru `erp-item-media.*` di `ErpItemsModule`.

**Frontend (web-erp):**
- `apiUpload()` baru di `lib/api/client.ts` (multipart; Content-Type dibiarkan
  browser yang set). API media di `lib/api/items.ts` + helper
  `itemMediaFileUrl()` untuk `<img>/<video>` src.
- Organism baru [`item-media-upload.tsx`](components/organisms/item-media-upload.tsx):
  dropzone drag&drop + klik (gambar multiple, video single), thumbnail grid
  aspect-square dengan aksi hover (jadikan utama ✓ / hapus 🗑), badge "Utama",
  lightbox preview (klik gambar, Esc tutup), player `<video controls>` +
  tombol Ganti/Hapus. Feedback via `notify()`; hapus via `confirmAction`
  variant danger. Token design system, tanpa warna hardcode.
- Form item: section side-nav baru **Media** (setelah Klasifikasi, mode
  Lengkap). `ItemFormData.id` ditambahkan (kosong saat create) — media butuh
  item tersimpan; mode create menampilkan empty state "Simpan item terlebih
  dahulu". Upload **langsung tersimpan** saat unggah (bukan bagian payload
  save form) — konsisten dengan pola attachment ERP umum.

## Item Lampiran — dokumen pendukung per item (2026-06-13)

Request user (`/erp` "semua master data, transaksi bisa input file, bisa
attachment file"): kapabilitas **lampiran file** ke record. Keputusan terkunci
(4 pertanyaan, 2026-06-13): **Lampiran dulu** (impor file menyusul),
**arsitektur per-modul** (tabel/endpoint khusus per entitas — bukan
`sys_attachments` generik), **implement pilot 1 entitas** = **Item (`md_items`)**.
Catatan: untuk lampiran murni, per-modul = duplikasi tabel/endpoint near-identik
tiap entitas saat scale ke "semua" (vs satu tabel polymorphic) — biaya replikasi
ini di-flag ke user, pilihan per-modul tetap dipakai untuk pilot.

**DB & backend (api-gateway):**
- Tabel baru `md_item_attachments` (`ErpItemAttachment`, migrasi
  `20260613_001_erp_item_attachments`): child `md_items` cascade, **generik**
  (tanpa `kind`/`isPrimary` ala media — semua file setara), `fileName` (asli) +
  `storedName` (acak `att-<itemId>-<uuid>.<ext>`, unique),
  `mimeType`/`sizeBytes`/`sortOrder`/`note?`/`createdById?`.
- Aturan: **max 20 lampiran** per item, **10MB**/file. Whitelist mime: PDF ·
  gambar (jpeg/png/webp/gif) · Word (doc/docx) · Excel (xls/xlsx) ·
  PowerPoint (ppt/pptx) · CSV · teks · ZIP; ekstensi diturunkan dari mime,
  bukan nama file user (cegah path trick).
- File binary di `apps/api-gateway/uploads/erp-items/` (sama dir dengan media —
  gitignored, bind mount host; `storedName` unik jadi tak bentrok). Streaming
  lewat endpoint ber-guard `ErpJwtAuthGuard`
  (`GET /erp/items/:itemId/attachments/:attachmentId/file`, `res.sendFile`,
  `Content-Disposition: inline`; cookie `erp_token` same-origin).
- Endpoint: `GET /erp/items/:itemId/attachments` (list) · `POST` multipart
  `file`+`note?` (upload, multer memory storage) · `PATCH :attachmentId`
  (ubah `note`) · `DELETE :attachmentId`. Module: controller+service baru
  `erp-item-attachments.*` di `ErpItemsModule` (terpisah dari media).

**Frontend (web-erp):**
- API lampiran di `lib/api/items.ts` (`ItemAttachment` + `listItemAttachments`/
  `uploadItemAttachment`/`updateItemAttachmentNote`/`deleteItemAttachment` +
  `itemAttachmentFileUrl`); reuse `apiUpload()` (multipart) yang sudah ada.
- Organism baru [`item-attachment-upload.tsx`](components/organisms/item-attachment-upload.tsx):
  dropzone drag&drop + klik (multiple), daftar baris file (ikon + nama clickable
  buka tab, ukuran, **catatan editable** simpan-on-blur, tombol unduh + hapus),
  feedback via `notify()`, hapus via `confirmAction` variant danger. Token design
  system, tanpa warna hardcode.
- Form item: section side-nav baru **Lampiran** (grup Detail, setelah Media,
  mode Lengkap; ikon `file`). Pakai `ItemFormData.id` (kosong saat create) —
  lampiran butuh item tersimpan; mode create empty state. Upload **langsung
  tersimpan** (bukan bagian payload save form), sama pola dengan Media.

**Replikasi ke entitas lain** (master/transaksi berikutnya, saat diminta): salin
pola per-modul — tabel `<domain>_<entitas>_attachments` + service/controller
`erp-<entitas>-attachments.*` (reuse whitelist + limit) + API client + render
organism `*-attachment-upload.tsx` (bisa digeneralisasi nanti bila user setuju
pindah ke subsistem generik). **Impor file (bulk CSV/XLSX) = fase berikutnya**,
belum dikerjakan.

## § Partner form — Kontak & Alamat (no hp jangan di info utama) (2026-06-13)

Atas permintaan user ("no hp jangan di utama"): nomor telepon **tidak** boleh
berada di field info utama partner. Form partner (`partners-page.tsx`, tetap
`SimpleMasterPage`, `modalSize="lg"`) kini **tab** `Umum` / `Kontak` / `Alamat`:

- **Umum** = field utama partner (Kode, Nama, Tipe, NPWP, Akun Piutang/Hutang,
  Status) — **tanpa** no hp.
- **Kontak** = sub-list `md_partner_contacts` (nama, jabatan, **no hp**, email,
  utama) — organism [`partner-contacts-editor.tsx`](components/organisms/partner-contacts-editor.tsx).
- **Alamat** = sub-list `md_partner_addresses` (tipe, alamat, kota, provinsi,
  kode pos, **no hp**, fax, utama) — organism
  [`partner-addresses-editor.tsx`](components/organisms/partner-addresses-editor.tsx).

Sub-resource = **tambah + edit + hapus**. Endpoint:
`GET /partners/:id` (include contacts/addresses),
`POST|PATCH|DELETE /partners/:id/contacts[/:id]`,
`POST|PATCH|DELETE /partners/:id/addresses[/:id]`
— API client di [`lib/api/partners.ts`](lib/api/partners.ts).
Tab Kontak/Alamat butuh partner **tersimpan** (`PartnerForm.id`); saat create
baru, tab menampilkan hint "Simpan partner dulu". Editor fetch slice-nya sendiri
via `getPartner(id)` saat tab aktif (Radix Tabs unmount konten non-aktif).
**Tidak** ada kolom `phone` di `md_partners` — no hp memang hidup di
contacts/addresses, bukan di master partner.

### Partner — UX list-or-form untuk Kontak & Alamat (2026-07-14)

Masalah: tab Alamat (dan Kontak) menampilkan **list + form input bersamaan**
(form kosong "Tambah alamat" selalu di bawah tabel) → membingungkan, seolah
ada dua mode sekaligus. Screenshot user di `/app/master/partners`.

**Perbaikan UX (list-or-form, bukan nested modal di dalam modal partner):**

| Mode | Tampilan |
| --- | --- |
| **List** (default) | Toolbar `N alamat` + tombol **+ Tambah alamat**; tabel; empty-state dashed + CTA jika 0 baris |
| **Form create** | Judul "Tambah alamat" + **Kembali ke daftar**; fieldset form; Batal / Simpan alamat. List **disembunyikan**. |
| **Form edit** | Sama, judul "Edit alamat" + Simpan perubahan. Dibuka dari kebab row / context menu. |

State: `formMode: null | 'new' | string(id)`. Simpan sukses / Batal →
`formMode=null` kembali ke list. Paritas diterapkan ke
`partner-contacts-editor.tsx` (kontak) agar tab Kontak/Alamat konsisten.
Cascade geo + autofill kode pos dari kelurahan **tidak berubah**.

### Partner — dimensi Cabang/Gudang/Lokasi multi-select (2026-06-13)

Field **Cabang**, **Gudang**, **Lokasi** di form partner (tab **Umum**) dibuat
**multiple** — mirror persis pola dimensi GL item (`md_item_dim_*`). Keputusan
user: ketiga dimensi diadakan sekaligus; kolom tunggal `md_partners.branch_id`
**dipertahankan** sebagai fallback/default (= cabang pertama yang dipilih).

- **DB:** 3 tabel junction baru `md_partner_dim_branches` /
  `md_partner_dim_warehouses` / `md_partner_dim_locations` (`partner_id` +
  `<dim>_id`, unique pair, FK partner `ON DELETE CASCADE`, FK dim `RESTRICT`).
  Migrasi `20260613_003_erp_partner_dim_multi` (hand-written SQL +
  `prisma migrate deploy` di container, lalu `prisma generate` + restart).
  Backfill: `branch_id` lama → baris pertama `md_partner_dim_branches`.
  Gudang & Lokasi **pivot-only** (tak ada kolom tunggal di `md_partners`).
- **Backend:** `CreateErpPartnerDto` + `branchIds`/`warehouseIds`/`locationIds`
  (`string[]`); service `create`/`update` tulis pivot (`deleteMany`+`create`
  saat update) + sync `branchId` ke id pertama via `firstBranchSync`;
  `findAll`/`findOne` include `PARTNER_DIM_INCLUDE` agar form edit bisa prefill.
- **Frontend:** reuse molecule `MultiLookupField` (dari `items-form-parts.tsx`)
  + loader `loadBranchOptions`/`loadWarehouseOptions`/`loadLocationOptions`
  (dari `items-form-lookups.ts`). `PartnerForm` simpan id array + label map per
  dimensi; `fromRecord` derive dari `dimBranches/dimWarehouses/dimLocations`,
  `toPayload` kirim ketiga array. **Jangan** fork komponen multi-select baru —
  pola dim ini SSOT lintas master (item + partner).


### Termin Pembayaran — form add/edit dikelompokkan per seksi (2026-06-13)

Form add/edit `payment-terms-page.tsx` (`FormFields`) yang sebelumnya berupa
daftar datar 11 field di-refactor agar lebih user-friendly, dikelompokkan jadi
4 seksi dengan `SectionTitle` lokal (+ hint): **Identitas** (Kode, Nama),
**Diskon Pembayaran Awal**, **Denda Keterlambatan**, **Status**.

- **Input numerik** patuh §2.31: hari (`netDays`, `discountDays1/2`) pakai
  `NumInput decimals={0}`; persen (`discountPercent1/2`, `penaltyPercent`) pakai
  `DiscountInput` (suffix `%`, clamp 0–100) — bukan lagi `<Input type=number>`
  / teks polos.
- **Diskon tier** disajikan sebagai satu baris ramah-baca lewat helper lokal
  `DiscountTierRow`: "dalam [hari] hari → diskon [%]" (Tier 1 & Tier 2),
  menggantikan 4 field terpisah yang membingungkan.
- **`penaltyPeriod`** kini `Select` (Per hari/minggu/bulan/tahun) — value English
  canonical (`daily/weekly/monthly/yearly`) tetap dikirim apa adanya ke DTO
  string `penaltyPeriod`; bukan lagi free-text "monthly". ≥3 opsi → Select (§2.6).
- Tidak ada perubahan DB/DTO/payload — murni penyajian FE. File 216 baris (<400);
  typecheck+lint bersih. Test smoke (`__tests__/pages/payment-terms-page.test.tsx`)
  dilengkapi mock `bulkUpdateErpPaymentTermStatus`/`bulkDeleteErpPaymentTerms`
  yang sebelumnya hilang.

### Partner form — tab Transaksi dikelompokkan jadi kartu seksi (2026-06-13)

Tab **Transaksi** di form partner (`partners-form-fields.tsx`) dirapikan agar
lebih mudah dipakai. Sebelumnya: header seksi pakai warna **hardcode**
`text-orange-500` (terbaca seperti warning + langgar §2 "tanpa style/warna
hardcode") dan grup **Pembelian** (3 field) di `grid-cols-2` membuat **Rek.
Hutang** yatim di baris kedua.

- Seksi sekarang = helper lokal `TrxSection` (Card + CardHeader ikon-led
  `coins`/`cart`/`tag` + CardBody), bukan teks oranye. Warna ikon pakai token
  `--primary-soft`/`--primary-soft-fg` (selaras `ItemFormContextHeader`).
- 3 grup: **Mata Uang** (currency, full width), lalu **Pembelian** (Termin,
  Batas Hutang, Rek. Hutang) & **Penjualan** (Termin, Batas Piutang, Rek.
  Piutang, Tingkat Harga) berdampingan di `lg:grid-cols-2` (stack di layar
  sempit). Field per kartu = satu kolom `FormField` (label 110px konsisten),
  bukan grid 2-kolom yang merusak alignment.
- Tambah `help` text: Batas Hutang/Piutang "0 = tanpa batas"; Tingkat Harga
  "Level harga jual 1–10 untuk auto-isi harga". Label "Tingkat Harga Jual" →
  "Tingkat Harga" (subtitle kartu sudah konteks Penjualan).
- Tanpa perubahan DB/DTO/payload — murni penyajian FE. File 358 baris (<400);
  typecheck+lint bersih. (Test `__tests__/pages/partners-page.test.tsx` gagal
  pre-existing: mock `@/lib/api/partners` belum ekspor `bulkUpdatePartnerStatus`
  — di luar scope perubahan ini.)

### Stock Adjustment Types — rebrand + No Akun (2026-07-14)

Master **Item Transaction Types** di-rebrand jadi **Stock Adjustment Types**
(selaras label legacy MyERP+ "Tipe Penyesuaian Stok" / stocksadjustmenttype).

- **DB:** tabel `md_item_transaction_types` → `md_stock_adjustment_types`;
  model Prisma `ErpItemTransactionType` → `ErpStockAdjustmentType`;
  kolom baru `account_id` (FK → `md_accounts`, POSTABLE, nullable) = **No Akun**.
  Migrasi: `20260714_003_erp_stock_adjustment_types_rebrand`.
- **API:** endpoint `/erp/stock-adjustment-types` (modul
  `erp-stock-adjustment-types`); validasi akun postable di service
  (pola Other Costs).
- **UI:** page `stock-adjustment-types-page.tsx`, route
  `/master/stock-adjustment-types` (alias legacy `/master/item-txn-types`
  tetap dilayani). Form: SearchSelect CoA postable (label **No Akun**).
- **Menu:** code `M1.REF.ITEM-TXN-TYPE` dipertahankan (stabilitas
  `adm_role_menus`); title/path di-update lewat migrasi + seed.

---

## Account form — bank master + multi-dim + drop Control Account (2026-07-14)

Route: `/master/accounts` (`accounts-form.tsx`).

**Keputusan user (2026-07-14):** form CoA ditambah field scope/bank; Control Account dihapus dari UI.

| Field | UI | Persist |
| --- | --- | --- |
| Mata Uang | `SearchSelect` → `md_currencies` | `md_accounts.currency_id` (leaf/POSTABLE only) |
| Bank (Cek/Giro) | `SearchSelect` → `md_banks` | `md_accounts.bank_id` (+ legacy `bank_name` optional) |
| No. Rekening Bank | text | `md_accounts.bank_account_no` |
| Cabang multi | `MultiLookupField` | junction `md_account_dim_branches` |
| Lokasi multi | `MultiLookupField` | junction `md_account_dim_locations` |
| Divisi multi | `MultiLookupField` | junction `md_account_dim_divisions` |
| Control Account | **dihapus dari form** | kolom DB `is_control_account` tetap (seed AR/AP); create baru force `false` |

Aturan:

- Detail posting + multi-dim **hanya leaf/POSTABLE** (segmen kode terakhir non-nol) — mirror validasi existing currency/bank.
- Empty multi-dim = tidak membatasi scope (semua cabang/lokasi/divisi).
- Mirror pola partner/item dim: `buildAccountDimRows` + include `ACCOUNT_DIM_INCLUDE`.
- Migrasi additive: `20260714_005_erp_account_bank_multi_dims`.

---

