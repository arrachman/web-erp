# Senti ERP — Roadmap Implementasi Per File

> Berdasarkan audit codebase aktual pada 2026-10-03 dan diselaraskan dengan
> `temp/PRD-ERP-CV-Bahtera-Madani-A4.pdf` pada 2026-10-04.
>
> **Scope produk MVP CV Bahtera Madani** adalah Fase 1 PRD: CRM Sekolah,
> Order Hub, Dokumen, Pajak, Katalog, Pembelian, Persediaan, serta Keuangan. Batas,
> gap analysis, urutan vertikal, gerbang satu periode BOS, dan scope fase berikutnya
> ada di [`bahtera-madani-mvp-scope.md`](bahtera-madani-mvp-scope.md). Bagian
> “MVP-0–3” di dokumen ini adalah **fondasi teknis distributor** bagi MVP produk,
> bukan keseluruhan MVP produk.
>
> Root aplikasi: `apps/api-gateway` (NestJS/Prisma) dan `apps/frontend` (Next.js/React).
> Purchasing, Sales, Inventory, dan Finance sudah memiliki modul API serta halaman frontend. Roadmap ini membedakan **verifikasi/fix** fondasi dari file **baru/diubah** untuk fitur yang belum tersedia.
>
> Konvensi wajib: `ErpJwtAuthGuard`, DTO paginasi (`page`, `limit`, `search`, `sortBy`, `sortDir`), menu dikelola langsung di tabel `sys_menus`, list melalui `ErpListLayout`, file aplikasi maksimal 400 baris, migrasi additive hand-written SQL + `prisma migrate deploy` + Prisma generate di container.

---

## Fondasi MVP produk — Validasi alur inti distributor

**Tujuan:** memastikan implementasi yang sudah tersedia mampu menjalankan dua siklus bisnis utama sebelum kapabilitas vertikal sekolah ditambahkan. Fondasi ini tidak dianggap lulus MVP produk sampai pilot satu periode BOS memenuhi gerbang pada `bahtera-madani-mvp-scope.md`.

### MVP-1 — Procure-to-Pay: PO → GRN → Purchase Invoice → AP Payment

| File | Jenis | Pekerjaan / acceptance criteria |
|---|---|---|
| `apps/api-gateway/src/erp-pur-orders/erp-pur-orders.service.ts` | Verifikasi | Buat, submit, approve, dan post PO; pastikan nomor dokumen, supplier, warehouse, item, unit, harga, diskon, dan pajak tersimpan konsisten. |
| `apps/api-gateway/src/erp-pur-goods-receipts/erp-pur-goods-receipts.service.ts` | Verifikasi/fix | Pastikan GRN hanya dapat direferensikan dari PO valid dan qty receipt tidak melampaui outstanding PO. |
| `apps/api-gateway/src/erp-pur-goods-receipts/pur-goods-receipt-outstanding.helpers.ts` | Verifikasi/fix | Uji kalkulasi outstanding PO melalui receipt parsial, receipt kedua, reopen, dan return. |
| `apps/api-gateway/src/erp-pur-goods-receipts/pur-goods-receipt-posting.service.ts` | Verifikasi/fix | Pada POST: `acceptedQty` membentuk `inv_stock_movements`, update `purchasePrice`/`lastHpp`, dan jurnal Dr Inventory / Cr GR-IR. Tidak boleh menghasilkan posting ganda saat retry. |
| `apps/api-gateway/src/erp-pur-invoices/erp-pur-invoices.service.ts` | Verifikasi/fix | Uji purchase invoice dari GRN dan direct invoice; cegah invoice melebihi receipt/open balance. |
| `apps/api-gateway/src/erp-pur-invoices/pur-invoice-outstanding.helpers.ts` | Verifikasi/fix | Cocokkan outstanding GRN/PI dan perilaku saat reopen/void. |
| `apps/api-gateway/src/erp-pur-invoices/pur-invoice-posting.service.ts` | Verifikasi/fix | Pastikan posting memindahkan GR-IR menjadi AP sesuai akun pada item/header. |
| `apps/api-gateway/src/erp-fin-ap-payments/erp-fin-ap-payments.service.ts` | Verifikasi/fix | Uji draft allocation terhadap satu dan banyak PI; jumlah alokasi tidak boleh melebihi hutang terbuka. |
| `apps/api-gateway/src/erp-fin-ap-payments/ap-payment-posting.service.ts` | Verifikasi/fix | Saat POST: Dr AP, Cr Kas/Bank/Giro; materialisasi settlement allocation; reversal/reopen harus mengembalikan saldo dengan benar. |
| `apps/frontend/components/pages/pur-orders-page.tsx` dan file form terkait | Smoke test | Form/list PO dapat membuka, menyimpan, submit, dan menampilkan status. |
| `apps/frontend/components/pages/pur-goods-receipts-page.tsx` dan file form terkait | Smoke test | Form GRN dapat mengambil referensi PO dan menampilkan outstanding qty. |
| `apps/frontend/components/pages/pur-invoices-page.tsx` dan file form terkait | Smoke test | Form PI dapat mengambil GRN serta nilai dokumen konsisten dengan backend. |
| `apps/frontend/components/pages/fin-ap-payments-page.tsx` dan file form terkait | Smoke test | Form payment memuat invoice terbuka dan menampilkan allocation. |

**Output MVP-1:** satu skenario test berulang yang lulus: `PO 100 pcs → GRN 60 pcs → PI 60 pcs → bayar 60 pcs`, beserta jurnal dan saldo stok/AP yang dapat ditelusuri.

### MVP-2 — Order-to-Cash: SO → DO → Sales Invoice → AR Receipt

| File | Jenis | Pekerjaan / acceptance criteria |
|---|---|---|
| `apps/api-gateway/src/erp-sls-orders/erp-sls-orders.service.ts` | Verifikasi | Uji create/approve SO, customer, warehouse, stock availability, harga, pajak, dan outstanding delivery. SO tidak boleh memposting stok atau GL. |
| `apps/api-gateway/src/erp-sls-delivery-orders/erp-sls-delivery-orders.service.ts` | Verifikasi/fix | Uji reference SO dan qty delivery parsial; jangan boleh deliver melebihi SO. |
| `apps/api-gateway/src/erp-sls-delivery-orders/sls-delivery-order-outstanding.helpers.ts` | Verifikasi/fix | Validasi outstanding line per SO saat multiple DO, reopen, dan return. |
| `apps/api-gateway/src/erp-sls-delivery-orders/sls-delivery-order-posting.service.ts` | Verifikasi/fix | POST menciptakan satu `ISSUE` stock movement, kemudian COGS/inventory melalui `InvStockMovementPostingService`; retry tidak membuat movement kedua. |
| `apps/api-gateway/src/erp-sls-invoices/erp-sls-invoices.service.ts` | Verifikasi/fix | Uji SI dari DO dan direct SI; SI yang berasal dari DO tidak boleh kembali mengurangi stok. |
| `apps/api-gateway/src/erp-sls-invoices/sls-invoice-posting.service.ts` | Verifikasi/fix | POST membuat Dr AR / Cr Revenue / Cr Tax; direct SI saja yang membentuk stock movement. Uji advance dan diskon header. |
| `apps/api-gateway/src/erp-sls-invoices/sls-invoice-void.helpers.ts` | Verifikasi/fix | Void menggunakan dated reversing journal, bukan hard-delete; tanggal harus berada di fiscal period terbuka. |
| `apps/api-gateway/src/erp-sls-invoices/sls-invoice-void.spec.ts` | Test | Perluas kasus: void direct SI (dengan movement) dan SI dari DO (tanpa movement baru). |
| `apps/api-gateway/src/erp-fin-ar-receipts/erp-fin-ar-receipts.service.ts` | Verifikasi/fix | Uji draft allocation dan larangan menerima lebih dari AR terbuka. |
| `apps/api-gateway/src/erp-fin-ar-receipts/ar-receipt-posting.service.ts` | Verifikasi/fix | POST membuat Dr Kas/Bank / Cr AR dan membuat allocation sesudah ledger row tersedia; reopen membalik ledger serta menghapus materialized allocation. |
| `apps/frontend/components/pages/sls-orders-page.tsx` dan file form terkait | Smoke test | SO dapat dibuat/approve tanpa posting saldo. |
| `apps/frontend/components/pages/sls-delivery-orders-page.tsx` dan file form terkait | Smoke test | DO dapat dibuat dari SO dan posted. |
| `apps/frontend/components/pages/sls-invoices-page.tsx` dan file form terkait | Smoke test | SI dapat dibuat dari DO/direct; action void tersedia melalui kebab menu. |
| `apps/frontend/components/pages/fin-ar-receipts-page.tsx` dan file form terkait | Smoke test | Receipt dapat dialokasikan ke satu/multiple SI. |

**Output MVP-2:** skenario lulus: `SO 100 pcs → DO 60 pcs → SI 60 pcs → receipt 60 pcs`, dengan stok hanya berkurang sekali, AR lunas, dan jurnal seimbang.

### MVP-3 — Giro, retur, startup data, dan test coverage awal

| File | Jenis | Pekerjaan / acceptance criteria |
|---|---|---|
| `apps/api-gateway/src/erp-fin-giros/erp-fin-giros.service.ts` | Verifikasi/fix | Uji giro masuk/keluar, status outstanding, clearing, rejected, dan period validation. |
| `apps/api-gateway/src/erp-fin-giro-entries/giro-posting.service.ts` | Verifikasi/fix | Uji jurnal penerbitan, pencairan, dan penolakan giro. |
| `apps/api-gateway/src/erp-pur-returns/*` | Verifikasi/fix | Return purchase mengurangi stok dan membentuk credit/outstanding yang benar. |
| `apps/api-gateway/src/erp-sls-returns/*` | Verifikasi/fix | Return sales menambah stok serta membentuk credit memo yang benar. |
| Tabel `sys_settings`, `sys_fiscal_periods`, `md_currencies`, `md_taxes`, `sys_document_numberings` (DB) | Verifikasi | Periksa langsung di DB: fiscal period aktif, currency, tax, accounting defaults, dan document numbering sudah terisi. |
| Data opening stock & saldo AR/AP (DB) | Verifikasi | Stock opening dan saldo AR/AP konsisten dengan ledger awal; cek via query DB. |
| Tabel `sys_menus` (DB) | Verifikasi | Menu P2P/O2C/Giro tersedia dan path-nya cocok dengan frontend registry. |
| `apps/api-gateway/src/erp-pur-goods-receipts/pur-goods-receipt-posting.service.spec.ts` | Baru | Unit/integration test posting GRN: accepted/rejected qty, stock movement, GL, double-post protection. |
| `apps/api-gateway/src/erp-sls-delivery-orders/sls-delivery-order-posting.service.spec.ts` | Baru | Test DO: satu movement, COGS/stock ledger, reversal. |
| `apps/api-gateway/src/erp-fin-ap-payments/ap-payment-posting.service.spec.ts` | Baru | Test allocation AP, over-allocation, repost/reopen. |
| `apps/api-gateway/src/erp-fin-ar-receipts/ar-receipt-posting.service.spec.ts` | Baru | Test allocation AR, over-allocation, repost/reopen. |

### MVP-0 — Gap prasyarat master data & konfigurasi (hasil audit 2026-10-04; seed dihapus, semua pengecekan via DB)

Posting GRN/DO/SI akan gagal (BadRequest) bila data berikut kosong. Kerjakan SEBELUM MVP-1/2.

| File | Jenis | Gap konkret |
|---|---|---|
| Tabel `sys_document_numberings` (DB) | Audit via DB | Bandingkan kode dokumen yang dipakai service (DO, DOI, SI, SII, GRI, PI, PII, SR, RNR, RNRI, RP, VP, IP, PR, SQ, SA, DC, DR, AS, BS, IB, SIE, PL, SP, DNRI, RFQ) dengan isi tabel. Yang belum ada ditambahkan langsung di DB / menu Document Numbering; tanpa itu nomor jatuh ke fallback `count+1`. |
| Tabel `sys_settings` grup `inventory/accounts` (DB) | Audit via DB | Cek `glPostingEnabled`, `defaultCogsAccountId`, `defaultInventoryAccountId`, `defaultOpeningEquityAccountId`; isi lewat halaman Setting bila kosong (DO/stock movement melempar error HPP/persediaan bila kosong). |
| `md_items` / `md_item_categories` (DB) | Audit via DB | Cari item/kategori tanpa `inventoryAccountId`, `cogsAccountId`, `salesAccountId` (SI melempar error bila tidak ada); lengkapi data aktual. |
| `md_partners` (DB) | Audit via DB | Cari customer tanpa `receivableAccountId` dan supplier tanpa `payableAccountId` (atau default di dokumen); lengkapi data aktual. |
| `md_taxes` (DB) | Audit via DB | Pajak harus punya `saleAccountId` (PPN Keluaran) dan akun PPN Masukan untuk pembelian. |
| `apps/api-gateway/src/erp-settings/*` + FE halaman Setting | Verifikasi | Validasi di UI/endpoint: peringatan bila default account GL belum diisi sebelum go-live. |
| `apps/api-gateway/src/**/*.spec.ts` | Baru | Baru 6 spec, semua helper. Belum ada test posting GRN/DO/SI/AP/AR (sudah di MVP-3, naikkan prioritas). |

**Definition of done fondasi:** kedua flow inti, retur, dan giro melewati test manual; test otomatis mencakup posting stok dan ledger untuk jalur kritis; semua ketidaksesuaian diperbaiki sebelum workstream vertikal sekolah dimulai. Ini belum sama dengan definition of done MVP produk.

---

## Workstream vertikal MVP CV Bahtera Madani

Urutan otoritatif ada di [`bahtera-madani-mvp-scope.md`](bahtera-madani-mvp-scope.md):

1. **MVP-1:** model sekolah dan CRM berbasis NPSN/BOS (tanpa yayasan);
2. **MVP-2:** Order Hub channel-ready + input admin/sales + adapter SIPLah;
3. **MVP-3:** paket dokumen BOS/non-BOS + BAST + arsip;
4. **MVP-3:** aplikasi tax subledger PPN/PPh + bukti potong + Coretax/export;
5. **MVP-4:** hardening katalog sekolah, purchasing, inventory, finance, dan laporan margin;
6. **MVP-5:** pilot satu periode BOS tanpa rekap Excel paralel.

Pekerjaan per-file untuk workstream ini dibuat setelah open decisions PRD yang relevan dikonfirmasi. Kapabilitas vertikal harus mereuse transaksi `sls`/`pur`/`inv`/`fin`, Report Studio, attachment, dan workflow existing—jangan membuat engine transaksi kedua.

> **Yayasan dipindahkan ke Fase 3/B1 (2026-10-04).** Transaksi selalu melekat
> pada sekolah; yayasan hanya relasi opsional untuk negosiasi grup, kontrak
> pusat, dan laporan konsolidasi. MVP-1 = profil sekolah saja, tanpa tabel
> yayasan atau hierarki yayasan→sekolah. Rationale dan kriteria aktivasi ada di
> [`bahtera-madani-mvp-scope.md §7`](bahtera-madani-mvp-scope.md#7-yayasan--kapan--dan-mengapa-dipindahkan-dari-mvp-1).

---

## Backlog pasca-fondasi generik — Master data distributor: multi-satuan dan tier harga

**Tujuan:** item dapat dibeli/dijual dalam satuan berbeda dan harga jual otomatis mengikuti kategori pelanggan.

### P1-A — Konversi multi-satuan per item

Saat ini `ErpUnit.conversionFactor` berlaku global. Fitur baru harus menyimpan konversi khusus per item, misalnya Item A: `1 dus = 24 pcs`, yang dapat berbeda dari Item B.

| File | Status | Pekerjaan |
|---|---|---|
| `apps/api-gateway/prisma/schema/erp-md.prisma` | Ubah | Tambah `ErpItemUnitConversion` → tabel `md_item_unit_conversions`: `id`, `itemId`, `unitId`, `factorToBase`, `isPurchaseDefault`, `isSalesDefault`, `isActive`, audit fields, unique `(itemId, unitId)`, FK ke item/unit, index item dan unit. |
| `apps/api-gateway/prisma/migrations/<timestamp>_add_item_unit_conversions/migration.sql` | Baru | Migrasi SQL additive: create table, FK, indexes, unique constraint. Tidak mengubah tabel/data existing. |
| `apps/api-gateway/src/erp-items/dto/item-unit-conversion.dto.ts` | Baru | DTO reusable untuk `unitId`, `factorToBase`, default purchase/sales, dan status; validasi numeric positif. |
| `apps/api-gateway/src/erp-items/dto/create-erp-item.dto.ts` | Ubah | Tambahkan `unitConversions?: CreateItemUnitConversionDto[]`; validasi nested. |
| `apps/api-gateway/src/erp-items/dto/update-erp-item.dto.ts` | Ubah | Tambahkan payload snapshot/upsert conversion (`id?` + data); document behavior penghapusan conversion yang hilang dari snapshot. |
| `apps/api-gateway/src/erp-items/erp-items.service.ts` | Ubah | Transaksi create/update nested conversion; validasi base unit tidak diduplikasi, factor > 0, satu default pembelian maksimal satu, satu default penjualan maksimal satu, dan unit milik record aktif. |
| `apps/api-gateway/src/erp-items/erp-items.mappers.ts` | Ubah | Include/map conversion ke response item, `BigInt` menjadi string seperti mapper existing. |
| `apps/api-gateway/src/erp-items/erp-items.controller.ts` | Verifikasi | Endpoint item yang ada tetap memakai `ErpJwtAuthGuard`; tidak dibutuhkan endpoint terpisah bila payload nested sudah cukup. |
| `apps/api-gateway/src/erp-common/utils/item-unit-conversion.ts` | Baru | Resolver reusable: validasi unit milik item, konversi qty transaction → `baseQuantity`, ambil default purchase/sales unit. Jangan menggandakan rumus di GRN/DO/SO/SI. |
| `apps/api-gateway/src/erp-pur-orders/erp-pur-orders.service.ts` | Ubah | Saat persist line PO, resolve `unitValue`/`baseQuantity` melalui resolver. |
| `apps/api-gateway/src/erp-pur-goods-receipts/erp-pur-goods-receipt.service.ts` | Ubah | Saat persist GRN line, resolve konversi; receipt harus memakai snapshot nilai conversion pada baris. |
| `apps/api-gateway/src/erp-pur-goods-receipts/pur-goods-receipt-posting.service.ts` | Ubah | Gunakan `baseQuantity` yang tervalidasi untuk stock movement dan kalkulasi cost per base unit, tanpa mengubah tampilan qty transaksi. |
| `apps/api-gateway/src/erp-sls-orders/erp-sls-orders.service.ts` | Ubah | Resolve quantity/satuan line melalui helper agar SO menyimpan unit dan base qty yang konsisten. |
| `apps/api-gateway/src/erp-sls-delivery-orders/erp-sls-delivery-orders.service.ts` | Ubah | Resolve dan validasi conversion sebelum DO posted. |
| `apps/api-gateway/src/erp-sls-delivery-orders/sls-delivery-order-posting.service.ts` | Ubah | Movement tetap memakai snapshot `baseQuantity`; tidak menghitung ulang dari master setelah dokumen disimpan. |
| `apps/api-gateway/src/erp-sls-invoices/erp-sls-invoices.service.ts` | Ubah | Direct SI menyimpan conversion snapshot; SI dari DO mewarisi snapshot line DO. |
| `apps/frontend/components/pages/items-form.tsx` | Ubah | Tambah tab **Satuan** yang merender organism baru; tidak menaruh grid inline besar di form induk. |
| `apps/frontend/components/pages/items-unit-conversions-tab.tsx` | Baru | Organism/tab grid: unit, factor-to-base, radio default beli, radio default jual, hapus; gunakan `NumInput`, `RadioGroup`, `RowActionsMenu`; tampilkan base unit readonly. |
| `apps/frontend/lib/api/items.ts` | Ubah | Tambah tipe dan request/response `unitConversions`. Gunakan API client yang sudah ada, tidak `fetch` ad-hoc. |
| `apps/frontend/components/pages/pur-order-form.tsx` atau grid PO yang dipakai | Ubah | Unit selectable dari conversion item; perubahan unit memperbarui display qty dan `baseQuantity` preview. |
| `apps/frontend/components/pages/pur-goods-receipt-form.tsx` atau grid GRN yang dipakai | Ubah | Tampilkan unit dari PO/item dan base qty readonly. |
| `apps/frontend/components/pages/sls-order-form.tsx` atau grid SO yang dipakai | Ubah | Unit selectable dari conversion sales; preview base quantity. |
| `apps/frontend/components/pages/sls-delivery-order-form.tsx` atau grid DO yang dipakai | Ubah | Mewarisi conversion snapshot dari SO dan menolak unit yang tidak valid. |
| `apps/api-gateway/src/erp-common/utils/item-unit-conversion.spec.ts` | Baru | Test factor, default rules, unit nonaktif, unit yang bukan milik item, dan snapshot semantics. |
| `apps/api-gateway/src/erp-items/erp-items.service.spec.ts` | Baru/ubah | Test create/update conversion, duplicate unit, dua default purchase/sales, dan delete snapshot. |

**Acceptance scenario:** base unit `PCS`; conversion `DUS = 24 PCS`; PO dan GRN `5 DUS` menghasilkan stock movement `baseQuantity = 120 PCS`; SO/DO `2 DUS` mengurangi `48 PCS`, tanpa pembulatan diam-diam.

### P1-B — Tier harga jual per item

`ErpPosItemPriceTier` sudah ada dan `md_partner_categories.sales_tier` sudah tersedia. Phase ini menghubungkan keduanya ke order/invoice sales.

| File | Status | Pekerjaan |
|---|---|---|
| `apps/api-gateway/prisma/schema/erp-pos.prisma` | Verifikasi/ubah bila perlu | Verifikasi relasi `ErpPosItemPrice` ↔ `ErpPosItemPriceTier`, index `(itemPriceId, tierLevel)`, serta unique constraint agar satu tier per price record. Tambah hanya jika belum ada. |
| `apps/api-gateway/src/erp-items/dto/item-price.dto.ts` | Ubah | Tambah DTO nested `priceTiers`: `tierLevel`, `price`, `discountPercent?`, `minQty?`; validasi angka non-negatif dan tier unik. |
| `apps/api-gateway/src/erp-items/erp-items.service.ts` | Ubah | Persist/query price tiers bersama harga item; tier tidak boleh menghapus harga dasar dan perubahan harus transactional. |
| `apps/api-gateway/src/erp-items/erp-items.mappers.ts` | Ubah | Map tier harga ke response item. |
| `apps/api-gateway/src/erp-partner-categories/erp-partner-categories.service.ts` | Verifikasi/fix | Pastikan `salesTier` keluar pada endpoint kategori yang dipakai lookup customer. |
| `apps/api-gateway/src/erp-partners/erp-partner.data-mappers.ts` | Verifikasi/fix | Sertakan tier kategori pada response partner/customer yang dibaca form sales. |
| `apps/api-gateway/src/erp-sls-orders/sls-order-price-resolver.ts` | Baru | Resolver tunggal: customer → partner category → sales tier → item price tier; fall back ke base price; pilih `minQty` tertinggi yang masih memenuhi qty; return source harga untuk audit. |
| `apps/api-gateway/src/erp-sls-orders/erp-sls-orders.service.ts` | Ubah | Jalankan resolver saat add/update line, tetapi simpan snapshot `unitPrice` agar perubahan master price tidak mengubah order lama. Izinkan override hanya dengan permission/approval yang telah ada. |
| `apps/api-gateway/src/erp-sls-invoices/sls-invoice-price-resolver.ts` | Baru | Reuse/export logic harga dari order resolver atau wrapper tipis; jangan duplikasi pricing algorithm. |
| `apps/api-gateway/src/erp-sls-invoices/erp-sls-invoices.service.ts` | Ubah | Direct invoice memakai resolver; invoice dari SO/DO mewarisi snapshot harga dokumen sumber. |
| `apps/frontend/components/pages/items-form.tsx` | Ubah | Tab Harga memasang organism harga tier. |
| `apps/frontend/components/pages/items-price-tiers-tab.tsx` | Baru | Grid harga dasar + tier; gunakan `NumInput`, format Rupiah, dan data tier dari API partner categories. |
| `apps/frontend/lib/api/items.ts` | Ubah | Tambah tipe `ItemPriceTier` pada payload item. |
| `apps/frontend/components/pages/sls-item-lines.tsx` | Ubah | Setelah item/customer/qty dipilih, tampilkan harga rekomendasi dan source tier; perubahan qty memicu recalculation yang ter-debounce. |
| `apps/frontend/components/pages/sls-order-form.tsx` | Ubah | Oper tier customer ke item lines; jangan melakukan pricing business logic sendiri. |
| `apps/frontend/components/pages/sls-invoice-form.tsx` | Ubah | Sama untuk direct SI; dokumen sumber tetap readonly. |
| `apps/api-gateway/src/erp-sls-orders/sls-order-price-resolver.spec.ts` | Baru | Test fallback base price, tier pelanggan, minimum qty, category tanpa tier, serta snapshot price setelah master berubah. |

**Acceptance scenario:** Customer kategori tier 2 membeli Item A qty 15; resolver memakai tier 2 dengan `minQty ≤ 15`; customer tanpa tier memakai harga dasar; harga SO yang sudah approved tidak berubah ketika master price diedit.

### P1-C — Data contoh, deployment, dan regression

| File | Status | Pekerjaan |
|---|---|---|
| Data item distributor (DB) | Input via UI/DB | Item contoh dengan base `PCS`, conversion `DUS`/`KARTON`, base price, dan tier prices diinput langsung. |
| `md_partner_categories` (DB) | Verifikasi | Kategori pelanggan dengan `salesTier` yang konsisten dan terdokumentasi. |
| Tabel `sys_menus` (DB) | Verifikasi | Tidak perlu menu baru jika fitur berada pada Item/SO/PO existing; pastikan izin edit item dan price sudah benar. |
| `apps/api-gateway/package.json` | Verifikasi | Gunakan command existing untuk generate, test, typecheck, dan migration deploy; jangan menambah command duplikat. |
| `apps/frontend/__tests__/items-unit-conversions.test.tsx` | Baru | Form item memuat/menyimpan grid conversion dan mencegah default ganda. |
| `apps/frontend/__tests__/items-price-tiers.test.tsx` | Baru | Tier price tampil sesuai response dan format angka benar. |
| `apps/frontend/__tests__/sls-item-lines-pricing.test.tsx` | Baru | Auto-fill harga untuk tier customer dan harga base fallback. |

**Definition of done Phase 1:** migration berjalan di container, Prisma client tergenerate, test backend/frontend relevan lulus, PO/GRN/SO/DO menggunakan conversion snapshot, dan pricing sales mengikuti tier customer secara deterministik.

---

## Phase 2 — Lot/Batch dan expiry tracking

**Tujuan:** penerimaan barang dapat membentuk lot, stok dan pengiriman dapat ditelusuri per lot/expiry, serta picking menyarankan FEFO.

| File | Status | Pekerjaan |
|---|---|---|
| `apps/api-gateway/prisma/schema/erp-inv.prisma` | Ubah | Lengkapi relasi lot pada line yang butuh traceability (`stock movement`, GRN, DO) dan agregat quantity lot/warehouse bila belum tersedia. |
| `apps/api-gateway/prisma/migrations/<timestamp>_add_lot_tracking/migration.sql` | Baru | Additive migration untuk FK, indexes, dan constraint lot/item consistency. |
| `apps/api-gateway/src/erp-inv-lots/erp-inv-lots.module.ts` | Baru | Module lot, import Prisma module, export service bila dipakai GRN/DO. |
| `apps/api-gateway/src/erp-inv-lots/erp-inv-lots.controller.ts` | Baru | Endpoint guarded list/detail/create/update dengan Swagger dan server-side query. |
| `apps/api-gateway/src/erp-inv-lots/erp-inv-lots.service.ts` | Baru | CRUD lot, validasi item, supplier lot, date validity, status, dan saldo per lot. |
| `apps/api-gateway/src/erp-inv-lots/dto/{create,update,query}-inv-lot.dto.ts` | Baru | DTO validasi. |
| `apps/api-gateway/src/erp-inv-lots/inv-lot-allocation.service.ts` | Baru | Resolver allocation FEFO: hanya lot aktif dengan saldo positif, expiry terdekat dulu, kemudian created date. |
| `apps/api-gateway/src/erp-pur-goods-receipts/erp-pur-goods-receipts.service.ts` | Ubah | Terima lot metadata per GRN line dan validasi konsistensi item. |
| `apps/api-gateway/src/erp-pur-goods-receipts/pur-goods-receipt-posting.service.ts` | Ubah | Buat/reuse lot saat POST dan tulis lot ke movement line. |
| `apps/api-gateway/src/erp-sls-delivery-orders/erp-sls-delivery-orders.service.ts` | Ubah | Validasi allocation lot manual/otomatis tidak melebihi saldo lot. |
| `apps/api-gateway/src/erp-sls-delivery-orders/sls-delivery-order-posting.service.ts` | Ubah | Post lot allocation ke issue movement; reverse/void mengembalikan saldo lot. |
| `apps/api-gateway/src/erp-inv-stock-movements/erp-inv-stock-movements.service.ts` | Ubah | Expose/filter lot dalam history movement. |
| `apps/api-gateway/src/app.module.ts` | Ubah | Registrasi `ErpInvLotsModule`. |
| Tabel `sys_menus` + `adm_role_menus` (DB) | Ubah via DB | Tambah menu canonical `/warehouse/lots` di Inventory dan role mapping (migrasi data SQL). |
| `apps/frontend/components/pages/inv-lots-page.tsx` | Baru | List server-driven memakai `ErpListLayout`, filter status/expiry/item, kebab + context menu. |
| `apps/frontend/components/pages/inv-lots-form.tsx` | Baru | Form lot untuk koreksi metadata yang tidak mengubah stock balance. |
| `apps/frontend/components/pages/pur-goods-receipt-form.tsx` | Ubah | Kolom/section lot number, supplier lot, manufacture/expiry date. |
| `apps/frontend/components/pages/sls-delivery-order-form.tsx` | Ubah | Picker lot FEFO, menampilkan saldo/expiry dan allocation. |
| `apps/frontend/lib/nav.ts`, `apps/frontend/lib/registry.ts` | Ubah | Register route canonical dan breadcrumb lot. |
| `apps/api-gateway/src/erp-inv-lots/inv-lot-allocation.service.spec.ts` | Baru | Test FEFO, saldo nol, expiry null, item mismatch, reversal. |

---

## Phase 3 — Planning / MRP-lite

**Tujuan:** membuat kebijakan reorder, forecast, MRP run, serta mengubah suggestion menjadi draft PO.

| File | Status | Pekerjaan |
|---|---|---|
| `apps/api-gateway/src/erp-pln-reorder-policies/{controller,module,service}.ts` + `dto/*` | Baru | CRUD policy per item/warehouse: safety stock, reorder point, min/max, lead time, supplier preference. |
| `apps/api-gateway/src/erp-pln-demand-forecasts/{controller,module,service}.ts` + `dto/*` | Baru | CRUD forecast dan generate proposal dari historical sales. |
| `apps/api-gateway/src/erp-pln-mrp-runs/{controller,module,service}.ts` + `dto/*` | Baru | Run immutable: stock on-hand, open PO, open SO, forecast, policy → MRP lines. |
| `apps/api-gateway/src/erp-pln-replenishment-suggestions/{controller,module,service}.ts` + `dto/*` | Baru | List approved run output dan action buat draft PO; idempotency key mencegah PO ganda. |
| `apps/api-gateway/src/erp-pln-mrp-runs/mrp-calculation.service.ts` | Baru | Pure calculation service yang dapat diuji: projected stock, shortage, suggested quantity, preferred supplier. |
| `apps/api-gateway/src/erp-pur-orders/erp-pur-orders.service.ts` | Ubah | Entry point internal membuat draft PO dari MRP suggestion, menyimpan reference balik. |
| `apps/api-gateway/src/app.module.ts` | Ubah | Registrasi empat module planning. |
| Tabel `sys_menus` + `adm_role_menus` (DB) | Ubah via DB | Menu canonical Planning dan role mapping (migrasi data SQL). |
| `apps/frontend/components/pages/pln-reorder-policies-page.tsx` | Baru | List/form policy. |
| `apps/frontend/components/pages/pln-demand-forecasts-page.tsx` | Baru | List/form/generate forecast. |
| `apps/frontend/components/pages/pln-mrp-runs-page.tsx` | Baru | Jalankan MRP dan lihat line result read-only. |
| `apps/frontend/components/pages/pln-replenishment-suggestions-page.tsx` | Baru | Review saran, filter supplier, buat draft PO. |
| `apps/frontend/lib/nav.ts`, `apps/frontend/lib/registry.ts` | Ubah | Register routes dan metadata Planning. |
| `apps/api-gateway/src/erp-pln-mrp-runs/mrp-calculation.service.spec.ts` | Baru | Test ROP, safety stock, open supply/demand, max order, dan supplier grouping. |

---

## Phase 4 — Fixed Assets

**Tujuan:** asset register, depreciation run, transfer/disposal, dan GL posting. Prioritas bertahap agar tidak langsung membangun seluruh 22 model `ErpFa*`.

| File | Status | Pekerjaan |
|---|---|---|
| `apps/api-gateway/src/erp-fa-asset-categories/{controller,module,service}.ts` + `dto/*` | Baru | Master kategori aset dan mapping akun/depreciation category. |
| `apps/api-gateway/src/erp-fa-assets/{controller,module,service}.ts` + `dto/*` | Baru | Asset register, nilai perolehan, tanggal in-service, lokasi/department, lifecycle status. |
| `apps/api-gateway/src/erp-fa-depreciation-runs/{controller,module,service}.ts` + `dto/*` | Baru | Generate line depreciation, review, POST dengan ledger helper; idempotent per asset/period. |
| `apps/api-gateway/src/erp-fa-transfers/{controller,module,service}.ts` + `dto/*` | Baru | Transfer lokasi/department tanpa mengubah acquisition cost. |
| `apps/api-gateway/src/erp-fa-disposals/{controller,module,service}.ts` + `dto/*` | Baru | Disposal dan gain/loss ledger. |
| `apps/api-gateway/src/app.module.ts` | Ubah | Registrasi module FA. |
| Tabel `sys_menus` + `adm_role_menus` (DB) | Ubah via DB | Menu Fixed Assets + role mapping (migrasi data SQL). |
| `apps/frontend/components/pages/fa-asset-categories-page.tsx` | Baru | List/form kategori. |
| `apps/frontend/components/pages/fa-assets-page.tsx` | Baru | Asset register list/form. |
| `apps/frontend/components/pages/fa-depreciation-runs-page.tsx` | Baru | Create/review/post run. |
| `apps/frontend/components/pages/fa-transfers-page.tsx`, `fa-disposals-page.tsx` | Baru | Transaction lists/forms. |
| `apps/frontend/lib/nav.ts`, `apps/frontend/lib/registry.ts` | Ubah | Register FA routes. |
| `apps/api-gateway/src/erp-fa-depreciation-runs/*.spec.ts` | Baru | Test straight-line calculation, no duplicate period posting, period closed, reversal. |

---

## Phase 5 — Hardening, reporting, and delivery

| File / area | Pekerjaan |
|---|---|
| `apps/api-gateway/src/erp-pur-*/**/*.spec.ts` | Tambahkan test untuk PO, GRN, PI, return, AP payment; prioritas posting stock/GL. |
| `apps/api-gateway/src/erp-sls-*/**/*.spec.ts` | Tambahkan test untuk SO, DO, SI, void, return, AR receipt. |
| `apps/api-gateway/src/erp-inv-*/**/*.spec.ts` | Test stock movements, stock count, adjustment, opening balance, and lot. |
| `apps/api-gateway/src/erp-fin-*/**/*.spec.ts` | Test cash/bank, giro, journal, ledger, FX revaluation. |
| `apps/api-gateway/src/erp-*-reports/*`, `apps/api-gateway/src/erp-report-engine/*` | Dashboard executive: revenue, margin, AR/AP aging, stock value, dead stock, purchase/sales analysis. |
| `apps/api-gateway/src/erp-import/*` | CSV/XLSX import master data dan opening balance dengan preflight validation + error file. |
| `apps/api-gateway/src/erp-notifications/*` | Notifikasi approval, reorder/low stock, overdue AR/AP. |
| `apps/frontend/__tests__/` | UI smoke/regression test untuk form transaksi kritis, list server pagination, dan error state. |
| Report Studio templates | PO, GRN, DO, sales invoice, receipt, aging, stock card. |

---

## Dependency order

```text
Fondasi teknis: verify/fix P2P + O2C + giro + baseline config
  └─ MVP produk Bahtera Madani:
       CRM sekolah
       → Order Hub + SIPLah
       → dokumen BOS + pajak
       → hardening katalog/purchasing/inventory/finance
       → pilot satu periode BOS

Backlog generik yang mendukung kebutuhan terkonfirmasi:
  multi-satuan/tier harga → lot/batch → planning/MRP-lite

Fixed assets: Fase 4 PRD (bukan MVP)
Tests, reports, import, notifications: dimulai dari fondasi dan berlanjut sepanjang MVP
```

## Cross-phase completion rules

1. Every new ERP controller uses `ErpJwtAuthGuard` and validated DTOs.
2. Any financial/stock POST is atomic and idempotent, validates fiscal period, and leaves audit trail.
3. Every new list is server-driven, has status filter/search/pagination, empty/error/loading states, row kebab/context menu, and no client-side slicing.
4. No hardcoded UI tokens or duplicated business calculation in frontend.
5. Before each commit: inspect diff, check no secrets, run one relevant typecheck/test, then commit conventional message with the required Claude Code co-author trailer.
