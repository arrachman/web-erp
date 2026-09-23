# Laporan Status Master Data — Senti ERP

> Dibuat: 2026-06-03  
> Cakupan: Modul M1 (Master Data) — semua grup & halaman  
> Metode audit: kode frontend (`components/pages/`, `lib/api/`, `lib/erp-route-meta.ts`,
> `components/templates/shell-route-renderer.tsx`) + backend (`apps/api-gateway/src/erp-*/`)
> + seed (`prisma/seed-erp.ts`)

---

## Ringkasan Eksekutif

| Dimensi | Jumlah | Status |
| --- | --- | --- |
| Grup menu M1 | 6 | ✅ Semua di-seed di `sys_menus` |
| Total halaman master data | **59** | ✅ Semua ada |
| Frontend page component | 59 / 59 | ✅ Lengkap |
| Backend API module | 59 / 59 | ✅ Lengkap |
| Route meta (breadcrumb) | 59 / 59 | ✅ Lengkap |
| Seed menu `sys_menus` | 59 / 59 | ✅ Lengkap |
| Halaman Coming Soon / stub | 0 / 59 | ✅ Tidak ada |

**Kesimpulan:** Semua 59 halaman master data sudah terimplementasi penuh — frontend page, backend API module, route meta breadcrumb, dan entry `sys_menus`. Tidak ada halaman stub atau placeholder.

---

## Grup 1 — Organization (9 halaman)

> Menu parent: `M1.ORG` · Prefix route: `/org/`

| Kode Menu | Judul | Route | File Page | Backend Module |
| --- | --- | --- | --- | --- |
| M1.ORG.BRANCH | Branch | `/org/branches` | `branches-page.tsx` | `erp-branches` |
| M1.ORG.LOCATION | Location | `/org/locations` | `locations-page.tsx` | `erp-locations` |
| M1.ORG.WAREHOUSE | Warehouse | `/org/warehouses` | `warehouses-page.tsx` | `erp-warehouses` |
| M1.ORG.DIVISION | Division | `/org/divisions` | `divisions-page.tsx` | `erp-divisions` |
| M1.ORG.SUBDIVISION | Sub Division | `/org/sub-divisions` | `sub-divisions-page.tsx` | `erp-sub-divisions` |
| M1.ORG.PROJECT | Project | `/org/projects` | `projects-page.tsx` | `erp-projects` |
| M1.ORG.COST-CENTER | Cost Center | `/org/cost-centers` | `cost-centers-page.tsx` | `erp-cost-centers` |
| M1.ORG.DEPARTMENT | Department | `/org/departments` | `departments-page.tsx` | `erp-departments` |
| M1.ORG.SUBDEPT | Sub Department | `/org/sub-departments` | `sub-departments-page.tsx` | `erp-sub-departments` |

**Catatan:**
- Branch, Location, Warehouse punya hierarki: `Branch → Location → Warehouse`.
- Location & Warehouse punya SearchSelect FK ke parent.
- Form Branch tersendiri (`branches-form.tsx`) dengan field alamat lengkap.

---

## Grup 2 — Items (20 halaman)

> Menu parent: `M1.ITEM` · Prefix route: `/master/`

| Kode Menu | Judul | Route | File Page | Backend Module |
| --- | --- | --- | --- | --- |
| M1.ITEM.ITEMS | Items | `/master/items` | `items-page.tsx` | `erp-items` |
| M1.ITEM.CATEGORIES | Item Categories | `/master/item-categories` | `item-categories-page.tsx` | `erp-item-categories` |
| M1.ITEM.TYPES | Item Types | `/master/item-types` | `item-types-page.tsx` | `erp-item-types` |
| M1.ITEM.UNITS | Units | `/master/units` | `units-page.tsx` | `erp-units` |
| M1.ITEM.LOCATIONS | Item Locations | `/master/item-locations` | `item-locations-page.tsx` | `erp-item-locations` |
| M1.ITEM.INFO | Item Information | `/master/item-info` | `item-informations-page.tsx` | `erp-item-informations` |
| M1.ITEM.PRODUCT-CLASS | Product Class | `/master/product-classes` | `product-classes-page.tsx` | `erp-product-classes` |
| M1.ITEM.PRICE-INDEX | Price Index | `/master/price-indices` | `price-indices-page.tsx` | `erp-price-indices` |
| M1.ITEM.PRICE-CATEGORY | Price Category | `/master/price-categories` | `price-categories-page.tsx` | `erp-price-categories` |
| M1.ITEM.COMMISSION | Commission | `/master/commissions` | `commissions-page.tsx` | `erp-commissions` |
| M1.ITEM.PERMISSIONS | Item Permissions | `/master/item-permissions` | `item-permissions-page.tsx` | `erp-item-permissions` |
| M1.ITEM.CLASS | Class | `/master/classes` | `classes-page.tsx` | `erp-classes` |
| M1.ITEM.MODEL | Model | `/master/models` | `item-models-page.tsx` | `erp-item-models` |
| M1.ITEM.SIZE | Size | `/master/sizes` | `sizes-page.tsx` | `erp-sizes` |
| M1.ITEM.COLOR | Color | `/master/colors` | `colors-page.tsx` | `erp-colors` |
| M1.ITEM.NOZZLE | Nozzle | `/master/nozzles` | `nozzles-page.tsx` | `erp-nozzles` |
| M1.ITEM.OEM | OEM | `/master/oems` | `oems-page.tsx` | `erp-oems` |
| M1.ITEM.BRAND | Brand | `/master/brands` | `brands-page.tsx` | `erp-brands` |
| M1.ITEM.MATERIAL | Material | `/master/materials` | `materials-page.tsx` | `erp-materials` |
| M1.ITEM.SECTION | Section | `/master/sections` | `sections-page.tsx` | `erp-sections` |

**Catatan:**
- Items adalah halaman paling kompleks: form multi-tab (`items-form.tsx` + 7 sub-form),
  field 24+, filter by item type, classification 3-axis (Kind/Brand/Color).
- Item Categories punya hierarki parent-child & FK ke 3 akun GL
  (inventoryAccountId, cogsAccountId, salesAccountId).
- Item Types (`ErpItemKind`): 8 jenis baku (RM, WIP, FG, FA, MRO, ORM, UM, WO).
- Class/Sub Class, Color, Brand, Material, Size, Section, Nozzle, OEM: atribut sederhana
  pola `code + name + isActive`, semuanya punya backend module terpisah.

---

## Grup 3 — Partners (8 halaman)

> Menu parent: `M1.PARTNER` · Prefix route: `/master/`

| Kode Menu | Judul | Route | File Page | Backend Module |
| --- | --- | --- | --- | --- |
| M1.PARTNER.PARTNERS | Partners | `/master/partners` | `partners-page.tsx` | `erp-partners` |
| M1.PARTNER.CATEGORIES | Partner Categories | `/master/partner-categories` | `partner-categories-page.tsx` | `erp-partner-categories` |
| M1.PARTNER.CUSTOMER-CAT | Customer Categories | `/master/customer-categories` | `partner-sub-categories-page.tsx` | `erp-partner-sub-categories` |
| M1.PARTNER.SUPPLIER-CAT | Supplier Categories | `/master/supplier-categories` | `partner-sub-categories-page.tsx` | `erp-partner-sub-categories` |
| M1.PARTNER.SALESMAN-CAT | Salesman Categories | `/master/salesman-categories` | `partner-sub-categories-page.tsx` | `erp-partner-sub-categories` |
| M1.PARTNER.BANK | Bank | `/master/banks` | `banks-page.tsx` | `erp-banks` |
| M1.PARTNER.EXPEDITION | Expedition | `/master/expeditions` | `expeditions-page.tsx` | `erp-expeditions` |
| M1.PARTNER.VENDOR | Vendor | `/master/vendors` | `vendors-page.tsx` | `erp-partners` |

**Catatan:**
- Partners: form terpadu untuk Customer/Supplier/Salesman/keduanya (type enum).
  Field khusus: taxNumber, isTaxable, GL accounts AR/AP, credit limit AR/AP,
  payment terms, partner category FK.
- Customer/Supplier/Salesman Categories: satu page component (`partner-sub-categories-page.tsx`)
  diparameterisasi `type=CUSTOMER/SUPPLIER/SALESMAN`.
- Partner Categories punya field `kind` (CUSTOMER/SUPPLIER/SALESMAN/GENERAL) &
  `salesTier`.
- Vendor: tampilan khusus partner type SUPPLIER, reuse `erp-partners` module.

---

## Grup 4 — Finance Masters (5 halaman)

> Menu parent: `M1.FIN` · Prefix route: `/master/`

| Kode Menu | Judul | Route | File Page | Backend Module |
| --- | --- | --- | --- | --- |
| M1.FIN.ACCOUNTS | Chart of Accounts | `/master/accounts` | `accounts-page.tsx` | `erp-accounts` |
| M1.FIN.TAXES | Taxes | `/master/taxes` | `taxes-page.tsx` | `erp-taxes` |
| M1.FIN.CURRENCIES | Currencies | `/master/currencies` | `currencies-page.tsx` | `erp-currencies` |
| M1.FIN.TERMS | Payment Terms | `/master/payment-terms` | `payment-terms-page.tsx` | `erp-payment-terms` |
| M1.FIN.OTHER-COSTS | Other Costs | `/master/other-costs` | `other-costs-page.tsx` | `erp-other-costs` |

**Catatan:**
- Chart of Accounts: tree hierarki parent-first, validasi format kode dinamis dari
  `sys_settings` (default `NNNN.NN.NNN`), field `kind` (HEADER/POSTABLE), `type`
  (ASSET/LIABILITY/EQUITY/REVENUE/EXPENSE), `cashFlowCategory`, `openingBalance`,
  `currencyId`, `bankName`, `bankAccountNo`, `isControlAccount`. Form terpisah
  `accounts-form.tsx`; tipe child mengikuti parent, level & saldo normal di-derive
  server-side, detail bank/mata-uang hanya untuk segmen kode terakhir.
- Currencies: punya panel tarif `currencies-rates.tsx` (CurrencyRate bertanggal —
  keputusan 2026-05-17).
- Taxes: punya FK ke akun GL sale & purchase.
- Payment Terms: tiered discount (discountPercent1/2, discountDays1/2) + penalty.
- Other Costs: default Akun Debit + Akun Kredit (CoA `POSTABLE`), plus flag `isHPP`.
  `isHPP=true` menonaktifkan Akun Debit karena debit dialokasikan proporsional ke barang
  oleh modul transaksi; Akun Kredit tetap wajib.

---

## Grup 5 — Reference (11 halaman)

> Menu parent: `M1.REF` · Prefix route: `/master/`

| Kode Menu | Judul | Route | File Page | Backend Module |
| --- | --- | --- | --- | --- |
| M1.REF.COUNTRY | Country | `/master/countries` | `countries-page.tsx` | `erp-countries` |
| M1.REF.PROVINCE | Province | `/master/provinces` | `provinces-page.tsx` | `erp-provinces` |
| M1.REF.CITY | City | `/master/cities` | `cities-page.tsx` | `erp-cities` |
| M1.REF.AREA | Area | `/master/areas` | `areas-page.tsx` | `erp-areas` |
| M1.REF.TXN-NOTE | Transaction Notes | `/master/transaction-notes` | `transaction-notes-page.tsx` | `erp-transaction-notes` |
| M1.REF.TXN-NOTE-DETAIL | Txn Note Detail | `/master/txn-note-details` | `txn-note-details-page.tsx` | `erp-txn-note-details` |
| M1.REF.ITEM-TXN-TYPE | Stock Adjustment Types | `/master/stock-adjustment-types` | `stock-adjustment-types-page.tsx` | `erp-stock-adjustment-types` |
| M1.REF.PRODUCTION-CAT | Production Categories | `/master/production-categories` | `production-categories-page.tsx` | `erp-production-categories` |
| M1.REF.WORK-ESTIMATE | Work Estimate | `/master/work-estimates` | `work-estimates-page.tsx` | `erp-work-estimates` |
| M1.REF.POINT-CAT | Point Categories | `/master/point-categories` | `point-categories-page.tsx` | `erp-point-categories` |
| M1.REF.MISC | Miscellaneous | `/master/miscellaneous` | `miscellaneous-page.tsx` | `erp-miscellaneous` |

**Catatan:**
- Country → Province → City → Area: hierarki 4 level, setiap level punya FK ke level atas.
  Area (Kecamatan) punya field `postalCode`. Seed geodata tersedia:
  `npm run db:seed:geo` (38 provinsi · 514 kab/kota · 7.286 kecamatan · 84.270 kelurahan).
- SubArea (Kelurahan): **tidak diimplementasi** (ditunda/deferred) —
  bukan bagian MVP; bisa ditambahkan sebagai `areas-page.tsx`-like dengan FK ke Area.
- Transaction Notes & Txn Note Detail: referensi teks keterangan transaksi.
- Stock Adjustment Types: enum tipe penyesuaian stok (referensi) + No Akun (CoA postable).

---

## Grup 6 — Production (6 halaman)

> Menu parent: `M1.PROD` · Prefix route: `/master/`

| Kode Menu | Judul | Route | File Page | Backend Module |
| --- | --- | --- | --- | --- |
| M1.PROD.LABOR | Labor | `/master/labors` | `labors-page.tsx` | `erp-labors` |
| M1.PROD.MACHINE | Machine | `/master/machines` | `machines-page.tsx` | `erp-machines` |
| M1.PROD.DESIGNER | Designer | `/master/designers` | `designers-page.tsx` | `erp-designers` |
| M1.PROD.ACTIVITY | Production Activity | `/master/production-activities` | `production-activities-page.tsx` | `erp-production-activities` |
| M1.PROD.ROUTE | Production Route | `/master/production-routes` | `production-routes-page.tsx` | `erp-production-routes` |
| M1.PROD.SUBCLASS | Sub Class | `/master/sub-classes` | `sub-classes-page.tsx` | `erp-sub-classes` |

**Catatan:**
- Semua halaman production master menggunakan pola `SimpleMasterPage` (`code + name + isActive`).
- Labor, Machine, Designer: resource master untuk Manufacturing (M6).
- Production Activity & Route: referensi proses produksi (BOM, Work Order).

---

## Peta Komponen Reusable

| Organism / Template | Dipakai oleh |
| --- | --- |
| `SimpleMasterPage` | ~50 dari 59 halaman (semua master sederhana `code+name+isActive`) |
| `TreeDndMasterPage` | Chart of Accounts, Item Categories (tree hierarki + drag-and-drop) |
| `items-form.tsx` + 7 sub-form | Items page |
| `partners-page.tsx` (301 baris) | Halaman Partners (logic penuh, bukan SimpleMasterPage) |
| `accounts-form.tsx` | Chart of Accounts |
| `currencies-rates.tsx` | Panel tarif mata uang (sub-panel di Currencies) |
| `payment-terms-page.tsx` | Payment Terms (tiered discount logic) |
| `partner-sub-categories-page.tsx` | Customer / Supplier / Salesman Categories (1 file, 3 route) |

---

## Artefak yang Relevan

| Artefak | Path | Keterangan |
| --- | --- | --- |
| DB Design | `apps/web-erp/db-design/entities-m1-master-data.md` | Katalog field-level entitas M1 |
| DB Design Hub | `apps/web-erp/db-design/README.md` | Keputusan, konvensi, open decisions |
| Decision Log | `apps/web-erp/DECISIONS.md` | Log per-fitur (appearance, item form, geo, CoA, dll) |
| ERP Pages Registry | `components/templates/shell-route-renderer.tsx` (L149–247) | Mapping route → komponen |
| Route Meta | `lib/erp-route-meta.ts` (L32–98) | Judul & breadcrumb tiap route M1 |
| Seed Menu | `apps/api-gateway/prisma/seed-erp.ts` (L408–494) | Entry `sys_menus` untuk M1 |
| API Client | `apps/web-erp/lib/api/*.ts` | Satu file per entitas (`listX`, `createX`, `updateX`, `deleteX`) |

---

## Item Deferred (tidak diimplementasi di MVP)

| Entitas | Alasan | Catatan |
| --- | --- | --- |
| SubArea (`md_sub_areas`) | Kelurahan-level geo; MVP deferred | Pola sama dengan `areas-page.tsx`; low effort bila dibutuhkan |
| Multi-UOM per item (`md_item_units`) | Deferred; saat ini 1 base unit/item | Future: tabel `ItemUnit` (relasi item ↔ unit dengan faktor konversi) |
| Partner Address / Contact / BankAccount detail page | Dikelola sebagai sub-list dalam form Partner | Sudah ada di DB (`md_partner_addresses`, dll); UI terpisah bisa ditambahkan |

---

*Laporan ini dibuat dari analisis statis kode — belum mencakup uji fungsional runtime (API call, CRUD end-to-end). Untuk audit kualitas implementasi (standar §2.7), jalankan `/erp audit-kualitas` secara terpisah.*
