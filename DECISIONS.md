# Web-ERP — Decision Log (catatan build per-fitur)

Riwayat keputusan **per-fitur** Senti ERP — dibaca **on-demand** saat menyentuh
fitur terkait, **bukan** tiap sesi. Rulebook invariant yang berlaku setiap saat
ada di [`CLAUDE.md`](CLAUDE.md); file ini melengkapinya dengan konteks + rasional.

Nomor section (`§2.x`) dipertahankan sebagai **anchor stabil** — banyak commit &
dokumen lain me-rujuk `§2.x`, jadi id-nya tidak diubah walau urutannya dirapikan.
Aturan turunan yang **berlaku saat membangun apa pun yang baru** sudah diringkas
di `CLAUDE.md` (section "Aturan turunan lintas-fitur"); di sini detail lengkapnya.

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

### Report Engine — Custom PDF Report Engine (2026-06-06)

Keputusan: Senti ERP membangun **custom report engine sendiri** — tanpa 3rd-party
(Carbone.io, Stimulsoft, pdfme, LibreOffice, dll).

**Riset dilakukan** dengan mempelajari 622+ file `.mrt` (Stimulsoft XML) dari legacy
MyERP+ di `apps/web-erp/preferensi/Backened - myerpplus/report/mrt/m2`, `m4`, `m5`:
- Template format: **JSON** (bukan XML/DOCX/XLSX)
- Data source: **REST API endpoint** per report (tidak embed SQL di template)
- Output: **PDF** + HTML preview
- Rendering stack: **PENDING** — pilihan Puppeteer vs @react-pdf/renderer
- Terbilang: implementasi TypeScript sendiri (bukan MySQL stored function)
- Designer UI: fase berikutnya (MVP = JSON template manual)

**Temuan kunci dari MRT:**
- 6 tipe band: PageHeader, PageFooter, GroupHeader (n-level), Data, GroupFooter (n-level), EmptyBand
- Komponen: Text, Image, HorizontalLine, VerticalLine (Start/EndPointPrimitive)
- Expression: `{field}`, `{Sum()}`, `{IIF()}`, `{Format()}`, `{Replace()}`, `{PageNumber}`, `{TotalPageCount}`, `{Time}`, `{Line}`
- Layout: CanGrow, CanShrink, PrintOnAllPages, NewPageBefore, WordWrap
- Tidak ada Chart/CrossTab/Barcode/SubReport di seluruh m4+m5 (622 file)
- 3 pola report: Form Dokumen, List/Tabulasi, Buku Besar/Ledger

**Dokumen lengkap:** `apps/web-erp/report-engine/README.md` — living doc, update di sana.

---

### Report Engine — Wiring reports → Report Designer templates (2026-06-13)

Keputusan user (`/erp` + `/confirmation`, 2026-06-13): semua **laporan** (Finance,
Sales, Purchasing, Inventory) di-render ke PDF lewat **template yang diedit di
Report Designer** (`/admin/report-designer`) — edit template → output cetak berubah.

**Render stack final = `@react-pdf/renderer`** (menutup open-Q1 README; alternatif
Puppeteer ditolak user). Konsekuensi sadar: fidelity vs canvas designer (HTML) tidak
1:1 → scope engine = **structured report template** (page setup, brand header + logo,
tabel kolom, section heading + subtotal + grand total, page header/footer + nomor
halaman, font/warna). **Komponen free-form absolute-positioned ditunda.**

**Arsitektur (kunci):** *template = LAYOUT, builder = DATA.* Engine mengambil hasil
`ReportDocument` (fin) / `ReportDataset` (sls/pur/inv) yang sudah dihitung
builder/SQL tervalidasi sebagai bind-data `{d}`, lalu menerapkan layout template.
**SQL/`dataSources` di dalam template diabaikan** untuk laporan ber-`reportKey`
(disimpan untuk ad-hoc report masa depan). Alasan: reuse semua logika report
(subtotal, grouping, security), tanpa duplikasi business-logic sebagai raw SQL di
template; `reportKey ↔ ReportDocument.key` jadi 1:1.

**Dua kontrak data laporan berbeda** → engine konsumsi model **ternormalisasi**
(`EngineReportData`) + adapter tipis per kontrak (`ReportDocument`/`ReportDataset`).

**Binding** = field `reportKey` (nullable) di `ErpRptTemplate`/`rpt_templates`;
laporan memuat template aktif (`isActive`) yang `reportKey`-nya cocok; bila tak ada
→ fallback renderer pdfkit lama. **PDF-only** (xlsx/docx tetap renderer lama).

**Engine** = modul backend baru `apps/api-gateway/src/erp-report-engine/`
(shared, di-inject ke 4 modul report); PDF tree pakai `React.createElement` (tanpa
TSX — backend NestJS tanpa konfigurasi `jsx`). Injeksi di choke-point tunggal
`ReportExportService.render(doc, 'pdf')` per modul. Rollout: engine MVP → reportKey
+ seed → Finance pilot → sls/pur/inv. Status README report-engine → IMPLEMENTED saat
fase tuntas.

**Status implementasi (2026-06-13):** Engine + binding + **4 modul ter-wire & live**
(fin/sls/pur/inv; route 401-guarded; 25 unit/integration test hijau). Fallback ke
renderer lama (pdfkit/pdfmake) bila tak ada template aktif.

- **Template "auto"** = presentation-only (`{ auto, pageSize, orientation, margins }`,
  tanpa `bands`); engine **materialize** band dari kolom laporan live saat render
  (`ReportEngineService.materialize`). Seed: 14 `fin.<report>` + 1 `<module>.__default`
  per modul.
- **Fallback default modul:** `renderReport` coba `<module>.<report>` dulu, lalu
  `<module>.__default`. Jadi SEMUA laporan render via engine walau belum punya template
  khusus; edit `__default` me-restyle se-modul, template per-report override.
- **Stack deps:** `@react-pdf/renderer@^3` (CJS) + `react@^18` + `@types/react@^18`
  lokal di api-gateway (v4 ESM-only pecah di CJS/jest; root `react@19` tak dipakai).
- **⚠️ Ops:** `node_modules` api-gateway = **named Docker volume** (bukan bind-mount
  host). Tambah dependency → WAJIB `docker exec sentient-infra-api-gateway npm i ...`
  + `npx prisma generate` di dalam container + restart, bukan cukup install di host.
- Konflik dgn seed lama `seed-erp-report-templates.ts` (Jun 9, format `{field}`/
  `{{SUM()}}`/template-owns-SQL): **superseded**, dibiarkan utuh, unbound (tanpa
  reportKey) — keputusan user 2026-06-13 lanjut arsitektur baru.
- **Preview PDF (done 2026-06-13):** tombol "Preview PDF" di designer →
  `POST /erp/reports/preview { templateJson }` → engine render template (auto-materialize
  dari sample columns) dgn data contoh → PDF blob dibuka tab baru. Endpoint di
  `ErpReportsService.previewTemplate` (sample di `report-preview-sample.ts`); works utk
  template auto & explicit.
- **Materialize (done 2026-06-13):** tombol "Buat layout dari kolom" di designer (muncul
  utk template `auto`) → `POST /erp/reports/:id/materialize` → `ReportColumnsResolver`
  ambil kolom REAL laporan (sls/pur/inv via `getColumns(key)` registry tanpa DB; fin via
  build YTD) → `buildTableTemplate` → band eksplisit disimpan (auto→explicit), bisa
  diedit di canvas + render live cocok. `ErpReportsModule` import 4 modul report.
- **Phase 4 selesai.** Engine + binding + 4 modul + Preview + Materialize semua live &
  ter-verifikasi (route 401-guarded, 25 test hijau, app boot bersih).

---

### 2.4 Command palette = derived dari role-filtered nav (2026-05-20)

`CommandPalette` (`components/organisms/command-palette.tsx`) **tidak boleh**
punya hardcoded menu list. Items diturunkan dari prop `nav: NavItem[]` yang
sama dengan sidebar (state `nav` di `app-shell.tsx`, di-load via
`fetchMyMenus()`). Konsekuensi: search palette = persis semua menu aktif
yang user berhak akses (sesuai `adm_role_menus`). Group palette mengikuti
struktur nav (MODULE → ITEM, atau MODULE → GROUP → ITEM jadi "Module ·
Group"). Hanya group "Aksi" (toggle theme/lang) yang statis. Saat menambah
modul baru: cukup seed di `sys_menus` + `adm_role_menus`, palette ikut.

**Pencarian penuh (2026-05-31):** filter palette match terhadap **label
(raw + ter-translate via `tGlobal`), code/hint, dan nama group/module** —
bukan label saja. Konsekuensi: mengetik kode (`M0.CFG`, `CR`, `BRN`), nama
modul ("administrator", "data master"), atau teks yang tampil di layar (mode
ID/EN/JA) semuanya resolve. Sebelumnya hanya `it.label` raw yang dicocokkan,
jadi kode yang terlihat di kolom hint tidak bisa dicari & teks Indonesia yang
tampil tidak match. Sidebar & palette tetap satu sumber `nav` (coverage sinkron
by construction) — perubahan ini murni di field yang dicocokkan, bukan daftar item.

---

### 2.13 Setting → Tampilan = `/settings/appearance` (2026-05-20)

Halaman preferensi tampilan per-user. Canonical path = `/settings/appearance`
(seeded di `sys_menus` di bawah group `M0.SYS` "System" — Administrator module —
dgn code `M0.SYS.APPEARANCE`; 2026-05-22 dipindah dari `SET` module). Komponen = `AppearancePage` (`components/pages/appearance.tsx`),
ter-register di `ERP_PAGES` + `ERP_ROUTE_META`. Short-id legacy `set-appearance`
tetap jalan sebagai alias fallback NAV statis.

**Persistence:** reuse tabel `adm_user_preferences` (model Prisma
`ErpUserPreferences`) — **tidak** bikin tabel `adm_user_settings` baru.
Pemetaan field:

- `theme` (light/dark) → kolom eksplisit `theme`.
- `language` (id/en/ja) → kolom eksplisit `language`. **3 bahasa** didukung di
  UI sejak 2026-05-20: Indonesia, English, Japanese (日本語). Tipe `Lang` di
  `lib/shell-constants.ts`, `lib/mock.ts`, dan `appearance-parts.tsx` semua
  pakai union `'id' | 'en' | 'ja'`. `AppearancePage` men-derive translator
  lokal dari `tw.lang` via `makeTranslator` agar perubahan bahasa langsung
  refleks di halaman ini (tidak menunggu round-trip ke app-shell).
- **Sinkronisasi shell:** saat user pindah bahasa di AppearancePage, halaman
  dispatch `CustomEvent('erp-set-lang', { detail: { lang } })`. App-shell
  punya listener yang memanggil `setLang(next)` — efeknya
  topbar/sidebar/tab-bar/breadcrumb ikut translate instan tanpa remount.
  App-shell juga memuat `language` awal dari `getMyPreferences()` setelah
  user login (server SSOT), dan shortcut keyboard `L` mencycle id → en →
  ja → id. Sumber kunci i18n untuk modul sidebar = title English seeded
  di `apps/api-gateway/prisma/seed-erp.ts` (mis. "Master Data", "Finance &
  Accounting"); pastikan setiap modul baru ditambah seed-nya juga dimasukkan
  ke `I18N` di `lib/mock.ts` untuk ketiga bahasa.
- Tweaks UI lain (`primary`, `density`, `fontScale`, `sidebar`) → `metadata`
  Json (default dari `DEFAULTS` di `appearance-parts.tsx`).

**Favicon/browser tab (2026-07-10):** ikon tab Senti ERP memakai mark hijau
rounded-square dengan glyph putih dari aset yang diberikan user. Asset kanonik:
[`app/icon.png`](app/icon.png) + [`app/apple-icon.png`](app/apple-icon.png)
+ [`app/favicon.ico`](app/favicon.ico) untuk App Router, fallback browser di
[`public/favicon.ico`](public/favicon.ico) serta PNG 16/32. Metadata `icons`
dan `themeColor` diset di
[`app/layout.tsx`](app/layout.tsx) agar tab browser dan mobile touch icon
konsisten.

**API**: module `erp-user-preferences`
(`apps/api-gateway/src/erp-user-preferences/**`) — `GET /erp/user-preferences/me`
+ `PUT /erp/user-preferences/me` (guard `ErpJwtAuthGuard`). FE pakai client
`getMyPreferences()` / `updateMyPreferences()` di `lib/api/user-preferences.ts`.

**Font scale → global (2026-05-20).** Knob "Ukuran Font" (`sm/base/lg/xl`)
men-drive CSS variable `--font-scale` (0.9/1/1.12/1.25) di `html[data-fontscale]`
(lihat [`styles/erp-tokens.css`](styles/erp-tokens.css)). Rules di
[`styles/erp-components.css`](styles/erp-components.css) (`body`, `input/select/
textarea/button`, `.tbl`, `.btn`, `.muted/.sub/.hint`, `.mono`) memakai
`calc(<base>px * var(--font-scale))` supaya knob menjangkau form controls &
tabel (yang biasa break font inheritance). **Shell juga ikut terskala
(2026-05-20):** selector `.topbar .brand`, `.breadcrumb`, `.cmd-trigger`,
`.kbd`, `.avatar`, `.flyout/.flyout-item`, `.tab-chip/.tab-code/.tab-count/
.tab-ctx-item`, `.user-menu-hd/.user-menu-item/.user-menu-item .mk`,
`.sidebar .nav-label` semua pakai `calc(NNpx * var(--font-scale, 1))` di
[`styles/erp-components.css`](styles/erp-components.css). **Inline px sudah diaudit:** 32
file komponen yang dulu hardcode `style={{ fontSize: NN }}` sudah di-refactor
jadi `fontSize: 'calc(NNpx * var(--font-scale, 1))'` (80 occurrence) supaya
ikut terskala. **Aturan baku:** saat menulis inline `fontSize`, **wajib**
pakai pola `calc(NNpx * var(--font-scale, 1))` — jangan re-introduce literal
numeric. Pengecualian sah: `FONT_PX[fontScale]` di `appearance-parts.tsx`
(intentional bucket preview).

**Load order saat mount AppearancePage**: API (server SSOT) > localStorage
(`erp-appearance` key) > DOM data-attr > `DEFAULTS`. **Auto-save**: setiap
perubahan kontrol langsung apply ke DOM data-attr (live preview) + tulis ke
localStorage; PUT ke API otomatis ter-debounce 500ms (tanpa tombol Simpan).
Hanya error API yang dinotifikasi (toast `danger`); sukses silent supaya tidak
spam saat user geser kontrol berurutan. Tombol Reset tetap ada untuk
mengembalikan ke `DEFAULTS`.

**Cross-device hydration (2026-05-22):** setelah API prefs berhasil di-load,
`AppearancePage` langsung tulis `merged` ke localStorage (`erp-appearance`)
sehingga `readUrlRoutingEnabled()` pada reload berikutnya sudah benar tanpa
menunggu user mengubah setting. `app-shell.tsx` juga melakukan hal yang sama +
dispatch `CustomEvent('erp-hydrate-url-routing')` agar `useUrlRouting` update
state `urlRoutingEnabled` **tanpa** mereset workspace tabs (berbeda dari
`erp-set-url-routing` yang memang reset tabs untuk manual toggle). Ini
mengatasi skenario cross-device / localStorage cleared.

---

### 2.14 Tab navigator = drag-and-drop reorder via @dnd-kit (2026-05-20)

Tab strip di app shell (`components/organisms/tab-bar.tsx`) **wajib**
mendukung reorder manual via drag-and-drop. Library = `@dnd-kit/core` +
`@dnd-kit/sortable` (sudah di `package.json`); **dilarang** native HTML5 DnD
atau `react-beautiful-dnd`.

- Setiap `tab-chip` di-wrap `useSortable({ id: tab.id })`; container pakai
  `DndContext` + `SortableContext` strategi `horizontalListSortingStrategy`.
- Sensor: `PointerSensor` dengan `activationConstraint.distance = 5px`
  supaya klik tab (activate) tidak ke-trigger sebagai drag accidentally.
  `KeyboardSensor` + `sortableKeyboardCoordinates` untuk a11y.
- `onPointerDown` di tombol `tab-x` (close) **wajib** `stopPropagation()`
  supaya tarik dari tombol-X tidak ikut memulai drag.
- State machine reorder di [`lib/use-app-shell-tabs.ts`](lib/use-app-shell-tabs.ts)
  via `reorderTabs(fromId, toId)` (functional setter + `splice`). Persistence
  ke workspace localStorage **otomatis** lewat `useEffect` existing yang
  watch `tabs` di `app-shell.tsx` — jangan tambah jalur simpan baru.
- "+" (new tab), duplicate, dan tab counter tetap **di luar**
  `SortableContext` agar tidak ikut sortable.

---

### 2.15 Sidebar group `Organization` (2026-05-20)

Group nav baru di NAV `Organization` (id `org`, icon `database`) menampung
**9 master org-level**: Branch, Location, Warehouse, Division, Sub Division,
Project, Cost Center, Department, Sub Department.

- Path kanonik = `/org/<entity>` (di-seed di `sys_menus` di bawah module
  `M1` group `M1.ORG`). Path lama `/master/branches`/`/master/locations`/
  `/master/warehouses`/`/master/divisions`/`/master/subdivisions` tetap
  ter-register di `ERP_PAGES` sebagai alias (jangan break link existing).
- Tabel: `md_branches`, `md_locations`, `md_warehouses`, `md_divisions`,
  `md_subdivisions`, `md_projects`, `md_cost_centers`, `md_departments`,
  `md_sub_departments`. Model Prisma: `ErpBranch`, `ErpLocation`,
  `ErpWarehouse`, `ErpDivision`, `ErpSubdivision`, `ErpProject`,
  `ErpCostCenter`, `ErpDepartment`, `ErpSubDepartment`.
- API module per-entitas di `apps/api-gateway/src/erp-*` (controller +
  service + DTO create/update/query/bulk), guard `ErpJwtAuthGuard` (§2.5).
- FE: tiap halaman = ~60-90 baris pakai organism reusable
  `components/organisms/simple-master-page.tsx` (generik CRUD: pagination
  server-driven, kebab+context menu, bulk action, audit panel). Wajib
  pakai organism ini untuk halaman master "code+name+isActive" gaya baru —
  **dilarang** fork branches-page lagi.

---

### 2.16 `SimpleMasterPage<T, F>` organism (2026-05-20)

[`components/organisms/simple-master-page.tsx`](components/organisms/simple-master-page.tsx)
adalah organism reusable untuk halaman list master entitas "simple" (code +
name + isActive + optional kolom/field tambahan). Pattern wajib §2.7–§2.12
sudah built-in di sini.

API page-side (per entitas):
- `defaultForm()` / `fromRecord(row)` / `toPayload(form)` — adapter form.
- `FormFields` — komponen form kustom (atom `FormField` + `Input` +
  `BooleanRadio` + `Select` bila perlu).
- `extraColumns: ExtraColumn<T>[]` — kolom ekstra di antara Nama dan Status
  (untuk relasi parent: Sub Division→Division, Sub Department→Department,
  Project→date range).

Endpoint API client wajib expose `list/create/update/remove/bulkStatus/
bulkDelete` untuk dipasangkan ke organism (lihat
`lib/api/divisions.ts` sebagai template).

#### Standar validasi form `SimpleMasterPage` (WAJIB, 2026-05-23)

Setiap halaman yang pakai `SimpleMasterPage` **wajib** menerapkan pola
validasi berikut — tanpa pengecualian:

1. **`validate` prop wajib ada** di `<SimpleMasterPage ... validate={validateXxx} />`.
   Minimal validasi: `code` required + `name` required (+ FK required bila ada
   field `SearchSelect` yang mandatory).

2. **`FormFields` wajib terima `errors`**:
   ```tsx
   function FormFields({
     data, onChange, errors = {}
   }: { data: F; onChange: (d: F) => void; errors?: FormErrors<F> }) {
   ```

3. **`aria-invalid` wajib pada setiap `<Input>` yang required**:
   ```tsx
   <FormField label="Kode" htmlFor="ef-code" required error={errors.code}>
     <Input ... aria-invalid={!!errors.code} />
   </FormField>
   ```

4. **`error` prop wajib pada `<SearchSelect>` required**:
   ```tsx
   <SearchSelect ... error={!!errors.fieldId} />
   ```
   (Pasangkan `aria-invalid` + border merah sudah built-in di `SearchSelect`.)

5. **Auto-focus ke field error pertama** — sudah built-in di `handleSave`
   organism (query `[role="dialog"] [aria-invalid="true"]`, no-op bila tidak ada).

Konsekuensi: halaman yang skip `validate=` → submit tanpa validasi client-side.
Halaman yang skip `aria-invalid=` → auto-focus gagal menemukan field error.
Kedua ini harus selesai sebelum halaman dideklarasikan done.

---

### 2.17 Modul sidebar Senti ERP — scope final (2026-05-20)

Modul valid di `sys_menus` (sortOrder, sumber `seed-erp.ts`):

| sortOrder | code | title | catatan |
| --- | --- | --- | --- |
| 0 | M8 | Dashboard | pinned paling atas |
| 1 | M0 | Administrator | sys + adm |
| 2 | M1 | Master Data | md (incl. group Organization) |
| 3 | M2 | Finance & Accounting | fin |
| 4 | M3 | Warehouse & Inventory | inv |
| 5 | M4 | Purchasing | pur |
| 6 | M5 | Sales | sls |
| 7 | M6 | Production | mfg |
| 8 | M7 | Fixed Assets | fa |
| 9 | M12 | Point of Sale | pos (singular!) |
| 99 | SET | Settings | preferensi user |

**Dihapus permanen dari ERP scope:** M10 (HR & Payroll), M11 (Hospital —
milik `apps/web-althea`), M13 (Academic), M14 (Cooperative). Tidak ada di
`module-roadmap.md`, bukan scope manufaktur. Kalau perlu dihidupkan lagi:
katalog field-level dulu di `db-design/`, lalu seed.

**Single source of truth seed = `apps/api-gateway/prisma/seed-erp.ts`**.
`prisma/seed.ts` (clinic seed) dulu punya blok `ERP_MENU_SEEDS` paralel
dengan short-id legacy (`master-data`, `administrator`, `md-items`, ...)
yang menabrak/duplicate setiap `npm run db:seed`. **Blok itu dihapus
2026-05-20.** Jangan pernah re-introduce ERP menu seeding di `seed.ts`.

**M2 Finance — paritas legacy m2-finance (2026-05-20).** Sebelumnya seed M2
cuma 4 transaksi + 1 report (`Journal Entries`, `AR Receipts`, `AP Payments`,
`Giros`, `General Ledger`) — terlalu ringkas, tidak match legacy. Sekarang
13 transaction items + 1 report, title **English**, ditahan `legacyCode`
sebagai 2–3 huruf legacy:

| code | title | legacyCode | path |
| --- | --- | --- | --- |
| M2.TX.CASH-RECEIPT | Cash Receipt | CR | /finance/cash-receipts |
| M2.TX.CASH-DISBURSEMENT | Cash Disbursement | CD | /finance/cash-disbursements |
| M2.TX.BANK-DISBURSEMENT | Bank Disbursement | BD | /finance/bank-disbursements |
| M2.TX.CASHBANK-TRANSFER | Cash/Bank Transfer | CB | /finance/cashbank-transfers |
| M2.TX.RECEIPT-GIRO | Receipt Giro | RG | /finance/receipt-giros |
| M2.TX.SEND-GIRO | Send Giro | SG | /finance/send-giros |
| M2.TX.RECEIPT-GIRO-CLR | Receipt Giro Clearing | RGC | /finance/receipt-giro-clearings |
| M2.TX.SEND-GIRO-CLR | Send Giro Clearing | SGC | /finance/send-giro-clearings |
| M2.TX.RECEIPT-MEMO | Receipt Memo | RM | /finance/receipt-memos |
| M2.TX.SEND-MEMO | Send Memo | SM | /finance/send-memos |
| M2.TX.GENERAL-JOURNAL | General Journal | GJ | /finance/general-journals |
| M2.TX.ADJUSTMENT-JOURNAL | Adjustment Journal | AJ | /finance/adjustment-journals |
| M2.RPT.LEDGER | General Ledger | — | /finance/ledger |

Padanan ID untuk konteks user: CR=Kas Masuk, CD=Kas Keluar, BD=Bank
Keluar, RG=Giro Masuk, SG=Giro Keluar. Path FE belum dibangun — menu
muncul di sidebar, route placeholder akan ditambah saat slicing modul M2.

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

### 2.19 Mode "Per-halaman URL" = true single-page (2026-05-22)

**Penamaan resmi untuk vibe coding:**

| Istilah | UI label | Kode | Arti |
| --- | --- | --- | --- |
| **URL routing off** | Internal | `urlRoutingEnabled = false` | navigasi tidak ubah URL, multi-tab aktif |
| **URL routing on** | Per-halaman URL | `urlRoutingEnabled = true` | URL ikut halaman aktif, tab navigator disembunyikan |

Gunakan "URL routing off/on" saat diskusi atau vibe coding — langsung korespondensi ke nama variabel `urlRoutingEnabled`.

Knob URL Routing di Setting → Tampilan punya 2 mode: **Internal** (default,
navigasi tidak mengubah URL, multi-tab) dan **Per-halaman URL**.

- Memilih **Per-halaman URL** **menyembunyikan seluruh tab navigator** —
  `TabBar` tidak dirender. Konsekuensi: user hanya bisa membuka satu halaman
  dalam satu waktu. Navigasi via sidebar/topbar/command-palette/notifikasi
  **mengganti** halaman aktif di tempat (replace), bukan membuka tab baru.
- Ganti mode (dua arah) **wajib** lewat `confirmAction` dengan pesan eksplisit
  per arah: ke Per-halaman URL → "tab navigator dihapus, hanya satu halaman";
  ke Internal → "tab navigator ditampilkan kembali". Saat dikonfirmasi semua
  tab lain ditutup — **halaman yang sedang aktif dipertahankan** (bukan reset
  ke `home`).
- Logika URL-routing diekstrak dari `app-shell.tsx` ke hook
  [`lib/use-url-routing.ts`](lib/use-url-routing.ts) (`useUrlRouting` +
  `readUrlRoutingEnabled`) — mengelola state mode, listener event
  `erp-set-url-routing`/`storage`, sync `window.history.replaceState`, dan
  `navigate()` (replace vs openTab). `app-shell.tsx` memanggil hook ini; saat
  mode aktif, render `TabBar` di-gate dengan `!urlRoutingEnabled`.

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

### 2.22 Menu Manager = TreeDndMasterPage (tree + cross-parent DnD) (2026-05-24, revisi)

`/admin/menus` (komponen `ErpMenusPage` di
[`components/pages/menus-page.tsx`](components/pages/menus-page.tsx))
**memakai organism `TreeDndMasterPage`** — hierarki MODULE→GROUP→ITEM dengan
drag-and-drop reorder (sibling **dan** cross-parent). **Revisi keputusan
2026-05-24** (atas permintaan user): versi flat-list `SimpleMasterPage` yang
sempat dipakai pagi itu **di-rollback**. Trade-off yang diterima ulang dengan
user:

- **Checkbox column diganti drag handle** (icon `grip-vertical`) di kolom
  paling kiri → **tidak ada bulk action** di halaman ini (pengecualian sah
  atas §2.9.H, dikonfirmasi user). Aksi destruktif per-baris tetap ada di
  kebab + right-click menu.
- **Tidak ada pagination/sort/filter server-driven** (pengecualian sah atas
  §2.7/§2.12) — tampilan hierarkis butuh seluruh subtree terlihat agar DnD
  bermakna. Search client-side menyaring baris yang cocok **+ ancestor-nya**
  supaya konteks tree tetap terbaca.

Organism reusable (atomic level organisms), bukan fork SimpleMasterPage:

- [`components/organisms/tree-dnd-master-page.tsx`](components/organisms/tree-dnd-master-page.tsx)
  — shell: state, search, modal create/edit, audit panel, DnD orchestration.
- [`components/organisms/tree-dnd-row.tsx`](components/organisms/tree-dnd-row.tsx)
  — molecule baris (drag handle + indent depth + cells + kebab/context menu).
- [`components/organisms/tree-dnd-helpers.ts`](components/organisms/tree-dnd-helpers.ts)
  — pure helpers (`flattenTree`, `inferNewParent`, `computeReorderChanges`,
  `validateDrop`); dipisah agar shell < 400 baris (§3).
- [`components/organisms/tree-dnd-master-page.types.ts`](components/organisms/tree-dnd-master-page.types.ts)
  — tipe bersama (`TreeRow`, props) untuk hindari import sirkular.

DnD = `@dnd-kit/core` + `@dnd-kit/sortable` (sama lib dgn tab-bar §2.14),
`PointerSensor` `distance: 5`, `verticalListSortingStrategy`.

**Cross-parent drop rule** (`inferNewParent`): setelah `arrayMove` di flat
list, parent baru item diturunkan dari baris tepat di atasnya:
MODULE → selalu root (null); GROUP → nesting di MODULE terdekat ke atas;
ITEM → anak dari MODULE/GROUP container terdekat, atau sibling dari ITEM
terdekat (mewarisi parent ITEM itu). Hanya item yang berubah `parentId`/
`sortOrder` yang dikirim ke backend (optimistic update lokal dulu, rollback
via `reload()` bila API gagal).

Detail kolom & form:

- **Kolom ekstra:** Tipe (badge MODULE=success/GROUP=info/ITEM=default),
  Path (mono muted). Kolom Urutan dihilangkan (urutan kini dari posisi DnD).
- **Parent menu** masih bisa diedit per-row via `SearchSelect` di form
  (filter `MODULE` + `GROUP` only) sebagai jalur alternatif memindah node
  ke container kosong. Validasi cycle/hierarki **server-side**.

Backend (`apps/api-gateway/src/erp-sys-menus/`):

- Endpoint baru: `POST /erp/sys-menus/reorder` (DTO `ReorderErpSysMenuDto`
  = `{ items: { id, parentId, sortOrder }[] }`). Diregister **sebelum**
  route `:id`. Service `reorder()` memvalidasi aturan hierarki tipe
  (MODULE root-only; GROUP di bawah MODULE; ITEM di bawah MODULE/GROUP) +
  **no-cycle** (tak boleh pindah node ke diri sendiri / descendant-nya),
  lalu apply semua update dalam **satu `$transaction`**.
- Endpoint bulk lama (`PATCH bulk/status`, `DELETE bulk`) **tetap ada** di
  service/controller (dipakai API lain / future), tapi **tidak dipasang**
  di halaman menus karena tak ada checkbox.
- `GET /erp/sys-menus` tetap flat tanpa pagination — §2.12 exception. Client
  `loadAll()` di menus-page request `limit:10000` lalu pakai seluruh data
  untuk membangun tree di FE.

**Footer informasional + keyboard nav (2026-05-24).** Walau halaman ini
**tidak** punya pagination (DnD butuh seluruh tree visible), footer tetap
hadir agar konsisten visual dgn list page lain — bentuk **count-only**:
`X dari Y baris` (X = baris visible setelah filter search, Y = total flat) +
hint pintasan keyboard. Footer dirender lewat organism reusable
[`components/organisms/list-footer.tsx`](components/organisms/list-footer.tsx)
yang juga dipakai `ErpListLayout`/SimpleMasterPage — mode dipilih via prop:
`pagination` → TablePagination penuh, `summary` → count-only (tree), plus
`selectable=false` untuk drop hint "X pilih" di halaman tanpa selection.

Keyboard navigation diwirekan via hook reusable
[`lib/use-tree-keyboard-nav.ts`](lib/use-tree-keyboard-nav.ts): **J/↓** &
**K/↑** geser focus, **Enter** open focused row (edit), **N** add new, **/**
focus search. **Tidak ada `X` (select)** — konsisten dgn keputusan "drag
handle ganti checkbox" di atas. Focus state visual via `data-focused` di
`TableRow` (styling otomatis dari `components/organisms/table.tsx`).

Konsekuensi vibe coding:

- Butuh tree+DnD untuk entitas hierarkis lain (mis. CoA tree, kategori
  berjenjang)? **Pakai ulang `TreeDndMasterPage`** — jangan fork menus-page,
  jangan bikin organism tree baru.
- Butuh bulk action di menus lagi? Itu balik konflik dgn keputusan "drag
  handle ganti checkbox" — eskalasi ke user dulu (§5).

**Filter by Modul + Grup (2026-06-17).** Menu Manager kini punya dua dropdown
filter di header (kiri search): **Modul** lalu **Grup**. Diimplementasikan
sebagai prop **reusable** `treeFilters?: TreeFilterConfig[]` di
`TreeDndMasterPage` (default off, backward-compatible — hanya menus-page yang
mengaktifkannya: `[{ type: 'MODULE', label: 'Modul' }, { type: 'GROUP', label:
'Grup' }]`). Aturan:

- Urutan array = **broad → narrow**. Setiap select me-list node bertipe
  `cfg.type` yang dibatasi ke **subtree dari pilihan filter yang lebih luas**
  (pilih Modul → daftar Grup menyusut ke grup dalam modul itu).
- Memilih filter yang lebih luas **mereset** semua pilihan yang lebih sempit
  (`setFilterAt`).
- Tree di-scope ke **node paling spesifik yang dipilih** (`scopeRootId`) +
  seluruh descendant-nya (`scopedFlat`); search tetap berjalan di atas scope.
- Tetap **client-side** (selaras pengecualian §2.12 untuk `menus`). DnD reorder
  **di-disable** selama ada filter aktif (`filterActive`) supaya
  `inferNewParent` tidak salah hitung di flat list terpotong.
- Sentinel `__ALL__` = "Semua" (Radix Select melarang value kosong).

Reuse `treeFilters` untuk tree hierarkis lain (CoA/kategori) — jangan rakit
filter ad-hoc per halaman.

**Fix scroll (2026-06-17).** `TreeDndMasterPage` **tidak** punya pagination
(by design: seluruh tree harus terlihat agar DnD reorder bermakna), tapi dulu
tidak bisa di-scroll saat daftar panjang: `.tabview` = `position:absolute;
inset:0` sehingga halaman wajib mengatur scroll internalnya sendiri, sementara
root `.card` organism ini tidak punya `height`/area scroll. Fix: root card
`height:100%; minHeight:0` (mengisi tabview) + container tabel `.lines`
jadi area scroll vertikal (`flex:1; minHeight:0; overflowY:auto`). Header
kolom (`.lines th { position:sticky; top:0 }`) otomatis ter-pin saat scroll.
Pola ini sama dgn `.page-body` di `ErpListLayout`.

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

### 2.26 Info icon + popover untuk enum berbisnis-logic (2026-05-28)

Field enum dgn semantik bisnis non-trivial (mis. `ErpItemType` —
INVENTORY/SERVICE/CONSUMABLE/ASSET/NON_INVENTORY) **wajib** punya jalur
"cek perbandingan" tanpa keluar form. Pola standar = **info icon di label
+ Radix Popover** berisi tabel perbandingan sifat + contoh kasus, plus
**helper text dinamis** di bawah Select yang menampilkan trait kunci dari
nilai terpilih (ikut berubah saat user ganti pilihan).

Implementasi pertama = item form (§2.25):
- Molecule [`components/molecules/item-type-info.tsx`](components/molecules/item-type-info.tsx)
  — exports `ItemTypeInfoButton({ currentType })` + helper `getItemTypeTraits(type)`.
  Popover highlight kolom & contoh row yang match `currentType`.
- Section Klasifikasi di [`items-form-fields.tsx`](components/pages/items-form-fields.tsx)
  pakai grid manual (bukan `FormField`) supaya icon button bisa berdiri di
  **luar `<label>`** — klik icon tidak menyambar fokus ke Select.

Kapan pakai pola ini (kriteria):
- Enum dgn ≥ 3 nilai yang punya **konsekuensi sistem berbeda** (drive logika
  akuntansi, stok, workflow), bukan sekadar label kosmetik.
- User awam (bukan dev/admin) bakal sering bingung memilih → butuh
  comparison reference yang on-demand.

Kalau cukup dijelaskan satu kalimat helper text statik → tetap pakai
`help` prop `FormField` (jangan pasang popover sekadar dekoratif).
Kandidat untuk diberi pola ini di masa depan: `ErpCostingMethod`
(AVG/FIFO/STD), status workflow approval, role/permission picker.

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

### 2.28 `SearchSelect` modal — stale-while-loading saat ganti halaman (2026-05-28)

Modal `SearchSelect` (`components/molecules/search-select-modal.tsx` +
`use-search-select.ts`) **wajib** memakai pola **stale-while-loading** saat
user menavigasi halaman dgn `←`/`→`:

- Baris hasil halaman sebelumnya **tetap di-render** selama `loading=true`,
  bukan diganti satu baris "Memuat…" yang membuat tbody kolaps & modal
  "berkedip" (collapse → expand) tiap ganti halaman.
- `<tbody>` saat loading dgn data existing → `opacity-50 pointer-events-none
  transition-opacity duration-150` + `aria-busy=true` (a11y).
- Header count `· {total}` **stabil** lintas-halaman (`tabular-nums`) —
  **dilarang** swap ke `· Memuat…` saat loading: `total` tidak berubah
  antar halaman, jadi swap text bikin width goyang & berkedip. Cukup dim
  tbody sebagai sinyal loading.
- Highlight focus baris (`isFocused`) di-suppress saat `loading` — supaya
  outline tidak nyangkut di baris stale yang sebentar lagi diganti.
- `tableActive` **tidak** di-reset di efek fetch maupun di handler
  `ArrowLeft/Right` — user yang sedang navigasi tabel tetap di mode tabel
  setelah halaman berikutnya muncul. Reset `tableActive=false` hanya di
  `openModal` (initial open) supaya search input yang fokus duluan.
- Fallback "Memuat…" full-body **hanya** dipakai saat truly empty
  (`loading && displayOptions.length === 0`, mis. saat modal baru dibuka
  belum ada data sama sekali).
- **Multi-select keyboard submit (2026-07-14):** di mode `multi`, tombol
  **Enter** selalu menjalankan aksi footer **Pilih** (`confirm()`), baik fokus
  sedang di search input maupun di tabel. Toggle baris dilakukan via klik atau
  **Space** saat baris fokus; `Ctrl/Cmd+Enter` tetap submit sebagai alias.
  Alasan: user mengharapkan Enter = tombol utama dialog, bukan no-op saat fokus
  masih di search box.
- **Post-submit focus advance (2026-07-14):** setelah modal **submit**
  (tombol Pilih / Enter / double-click baris / `confirm` / `confirmRow`),
  fokus **wajib** pindah ke field focusable berikutnya di form (A → B),
  lewat `focusNextFrom` di `search-select-focus.ts`. Cancel/ESC tetap
  mengembalikan fokus ke trigger. Implementasi:
  - `closeModal('next')` untuk submit, `closeModal(true|'trigger')` untuk
    batal; queue digabung karena Radix juga memanggil `onClose(true)` saat
    `open→false` — `'next'` tidak boleh di-downgrade jadi `'trigger'`.
  - `DialogContent onCloseAutoFocus` di-prevent agar restore default Radix
    tidak race dengan `focusNextFrom`.
  - Paritas dengan inline Enter auto-pick (sudah `focusNextFrom` sejak dulu).

Konsekuensi vibe coding: kalau menambah list modal-style baru di web-erp,
**dilarang** pola "replace tbody dgn loader row" untuk transisi halaman —
clone pola di atas. List page biasa (`SimpleMasterPage`) tetap pakai
`ErpListLayout` (§2.9) yang punya state loading khusus. Setelah pilih dari
modal lookup, **jangan** biarkan fokus kembali ke field yang sama — advance
ke field berikutnya.

---

### 2.29 Search semantics list endpoint = `code` exact, `name` LIKE (WAJIB, 2026-05-28)

Setiap service list ERP yang menerima `query.search` **wajib** memakai
semantik: **`code` exact-match (case-insensitive)**, **`name` partial
(`contains`, case-insensitive)**. Berlaku untuk semua jalur (SearchSelect
modal & list page search `/`) — backend endpoint sama, jadi satu sumber.

Pola kanonik (Prisma):

```ts
if (query.search?.trim()) {
  const q = query.search.trim();
  where.OR = [
    { code: { equals: q, mode: 'insensitive' } },
    { name: { contains: q, mode: 'insensitive' } },
  ];
}
```

Alasan: `code` adalah identifier unik (mis. `BR-001`, `ITM-MM`, `4.1.001`)
— user yang ngetik kode biasanya tahu persis kodenya & ingin **landing
satu hit**. Partial match (`contains`) di kode → hasil keruh (`BR` match
ratusan `BR-xxx`), bikin SearchSelect tidak deterministik. Sebaliknya
`name` adalah teks bebas → partial WAJIB (user jarang ingat nama persis).

Berlaku **mass refactor 2026-05-28** ke 55 service ERP yang punya pola
`code OR name` search. **Pengecualian sah** (dipertahankan `contains` —
bukan "code"):
- `md_items.barcode` (`erp-items.service.ts`) — barcode bukan kode entitas;
  semantik scan/partial belum dirombak (eskalasi terpisah bila perlu).
- `md_accounts.alias` (`erp-accounts.service.ts`) — alias = teks bebas.

Saat membuat service ERP baru dengan search: **wajib** pakai pola di atas
sejak awal. **Dilarang** re-introduce `{ code: { contains: ... } }` di
service baru.

---

### 2.30 `SearchSelect` inline-search — exact code match auto-pilih (2026-05-28)

Pelengkap §2.29. Saat user mengetik di input `SearchSelect` lalu commit
(blur ke luar input atau tekan Enter), `useSearchSelect` melakukan fetch
satu kali (`loadOptions(text, 1, limit)`) dan memilih jalur berikutnya:

1. **0 hasil** → reset value + buka modal dgn query (user lihat "Tidak ada hasil").
2. **Ada tepat 1 row dgn `code` exact-match (case-insensitive)** → auto-pilih
   row itu, **walaupun total `results.length > 1`** (mis. response 13 row krn
   "um" juga LIKE-match `name`, tapi `code = "UM"` cuma 1 → pilih `UM`).
3. **1 hasil saja** (tanpa exact code match) → auto-pilih row itu.
4. **>1 hasil tanpa exact code match** → buka modal supaya user pilih manual.

Helper `pickExactCodeMatch(results, query)` di
[`components/molecules/use-search-select.ts`](components/molecules/use-search-select.ts)
adalah SSOT logika ini — dipakai di `handleSingleBlur` dan handler Enter
`handleSingleKeyDown`. Defensive: kalau ada >1 row dgn code exact (tidak
seharusnya — code unique), tetap buka modal (`exact.length === 1` only).

Alasan: backend `code` sudah exact-match (§2.29), tapi response tetap berisi
row tambahan dari `name LIKE`. Tanpa shortcut ini, user yang ngetik kode
yang sudah ia hafal masih harus klik modal 1× lagi padahal kandidat-nya
jelas — beat seluruh keuntungan "search-by-code = exact".

---

### 2.30b Loader picker — `label` = nama saja, kode di field `code` (2026-05-31)

Untuk semua loader `SearchSelect` (termasuk akun coded), opsi **wajib**
memisahkan `code` dan `label`:

- `code` → kolom KODE modal + di-prepend `useSearchSelect.optLabel` jadi
  display trigger `"{code} - {name}"` (konvensi akuntansi).
- `label` → kolom NAMA modal = **nama saja**, tanpa prefix kode (KODE sudah
  punya kolom sendiri; menampilkan kode lagi di NAMA = redundan).

`loadAccountOptionsCoded` & `loadCashAccountOptionsCoded`
([`components/pages/items-form-lookups.ts`](components/pages/items-form-lookups.ts))
dulu set `label = "{code} - {name}"` → kolom NAMA dobel kode **dan** trigger
jadi `"{code} - {code} - {name}"` (optLabel prepend lagi). Diperbaiki: set
`label = x.name` saja. **Jangan** embed kode ke `label` di loader baru —
`optLabel` yang urus prefix kode untuk trigger.

---

### 2.31 Format angka dinamis dari `sys_settings` (2026-05-28)

Format angka **tidak** lagi hardcode `id-ID`. Pakai 3 setting global di
`sys_settings` group `number-format`:

- `number_thousands_sep` (string: `.` / `,` / ` ` / `'` / `""` tanpa pemisah)
- `number_decimal_sep` (string: `,` / `.`)
- `number_decimals` (integer 0–6 — default digit desimal)

**Backend SSOT** = [`apps/api-gateway/src/erp-settings/number-format.ts`](../api-gateway/src/erp-settings/number-format.ts):
`buildNumberFormat(thousandsSep, decimalSep, decimals)` → `{thousandsSep,
decimalSep, decimals, example}`. Validasi: `thousandsSep` ≠ `decimalSep`,
`decimals` 0–6. Endpoint: `GET /erp/settings/number-format` +
`PUT /erp/settings/number-format` (guard `ErpJwtAuthGuard`). **Tidak ada
lock-after-data** (beda dgn account-code-format §2.24) — ini display
formatting, ubah kapan saja, semua tampilan ikut refresh.

**Frontend:**

- [`lib/format.ts`](lib/format.ts) — module-level cache + `useNumberFormat()`
  hook + helper `formatNumber(value, decimals?)` / `formatRupiah(value)` /
  `formatQty(value)`. Helper lama tetap kompatibel (delegate ke
  `formatNumber`). Default fallback `{ '.', ',', 0 }` saat API gagal.
- [`lib/format.ts`](lib/format.ts) juga export `formatRawForDisplay(raw,
  fmt, decimals?)` + `parseDisplayToRaw(display, fmt)` — pure helpers untuk
  live mask di input.
- [`components/molecules/num-input.tsx`](components/molecules/num-input.tsx)
  (`NumInput`) — input numerik dgn live thousand-separator masking +
  caret restore via digit-index. Value = raw canonical (`12345` / `12345.5`).
- `NumField` di [`items-form-parts.tsx`](components/pages/items-form-parts.tsx)
  sekarang pakai `NumInput` (semua field numerik items-form ikut format).
- Halaman dedicated `/admin/number-format` ([`number-format-page.tsx`](components/pages/number-format-page.tsx))
  di group `M0.SYS` (Administrator → System) — 3 dropdown/input + preset
  cepat (id-ID, id-ID+2 desimal, en-US, en-US+2 desimal, plain) + preview
  live. Setelah PUT sukses → `invalidateNumberFormatCache(updated)` supaya
  semua subscriber `useNumberFormat()` re-render dgn format baru.

**Saat membuat input numerik baru**: pakai `<NumInput>` (atau `NumField` di
items-form). **Dilarang** `<Input type="number">` atau `<Input
inputMode="decimal">` mentah untuk field qty/harga — tidak ikut format
global. `decimals?` prop bisa override default per field (mis. `decimals={2}`
untuk harga, biarkan undefined untuk qty integer ikut setting global).

**Saat memformat angka di tabel/summary**: pakai `formatNumber/formatRupiah/
formatQty` dari `lib/format.ts` — sudah otomatis ikut setting global (§2.9
"Format Angka" disempurnakan: tidak lagi hardcode locale id-ID).

**Migrasi seed:** key tunggal lama `sys_settings.key='number_format'` di
group `format` (value literal `'1.000,00'`, never dipakai) **dihapus
otomatis** oleh `prisma/seed-erp.ts` (`deleteMany` sebelum upsert) — clean,
non-destructive. Jalankan `npm run db:seed` setelah pull untuk hidupkan
3 key baru + menu `/admin/number-format`.

---

### 2.39 Date field = `<DateInput>` (popover day-picker) + format dinamis (2026-05-31)

**Masalah:** semua field tanggal pakai native `<input type="date">` →
placeholder `dd/mm/yyyy` abu-abu (browser-controlled, tidak bisa di-custom)
+ chrome native yang tidak konsisten dgn design system. User minta UX
placeholder tanggal diperbaiki.

**Keputusan:** ganti **semua** native `<Input type="date">` di form/filter
dengan komponen reusable [`components/ui/date-input.tsx`](components/ui/date-input.tsx)
(`DateInput`) — Radix Popover + `react-day-picker` (mode `single`, locale id)
+ `date-fns`, sejajar pola `date-range-picker.tsx`.

- **Empty state** = placeholder lembut `"Pilih tanggal"` (bukan `dd/mm/yyyy`).
- **Filled state** = tanggal diformat per setting global + tombol clear (X).
- **Kontrak:** `value` = ISO string `YYYY-MM-DD`; `onChange(v: string)` terima
  **string ISO langsung** (bukan event). Props opsional: `id`, `name`,
  `disabled`, `aria-invalid`, `placeholder`, `className`.
- **Bisa diketik manual (2026-05-31):** field = `<input type="text">` editable,
  bukan tombol read-only. User boleh **mengetik** tanggal langsung (tidak wajib
  lewat day-picker); ikon kalender hanya membuka popover sebagai alternatif.
  Draft teks di-commit saat **blur** / **Enter** (`Escape` membatalkan draft).
  Parsing toleran via `parseDisplayDate(text, fmt)` di `lib/date-format.ts`:
  coba format aktif dulu, lalu fallback umum (`5/5/2026`, `05-05-2026`,
  `2026-05-05`, `5 Mei 2026`). Input invalid → revert ke nilai valid terakhir;
  kosong → clear. Format token & day-picker tidak berubah.
- **Karakter diketik dibatasi (2026-05-31):** hanya **digit + separator format
  aktif** yang lolos (sanitizer di `onChange`); selain itu di-strip. Untuk
  `DD/MM/YYYY` → cuma angka & `/`. Separator diturunkan dari token (non-huruf),
  jadi format `DD-MM-YYYY`/`YYYY-MM-DD` otomatis izinkan `-`. Format ber-nama
  bulan (`MMM`/`MMMM`) tambahan izinkan huruf+spasi.

**Format tampilan tanggal dinamis dari `sys_settings`** (sejajar §2.31):

- Key tunggal `system/format/date_format` (sudah ada di seed, value
  `DD/MM/YYYY`). Token moment-style; preset terbatas (`DD/MM/YYYY`,
  `DD-MM-YYYY`, `MM/DD/YYYY`, `YYYY-MM-DD`, `DD MMMM YYYY`, `D MMM YYYY`).
- **Backend SSOT** = [`apps/api-gateway/src/erp-settings/date-format.ts`](../api-gateway/src/erp-settings/date-format.ts):
  `buildDateFormat(token)` → `{format, example}`, validasi token ∈ preset.
  Endpoint `GET`/`PUT /erp/settings/date-format` (guard `ErpJwtAuthGuard`),
  reuse tabel `erpSetting` (tidak ada group/migrasi baru).
- **Frontend** [`lib/date-format.ts`](lib/date-format.ts): cache module-level
  + `useDateFormat()` hook + `formatDate(iso, fmt?)` (token→date-fns pattern
  via `tokenToPattern`) + `parseIsoDate` / `toIsoDate`. Default fallback
  `DD/MM/YYYY` saat API gagal.
- Halaman dedicated `/admin/date-format`
  ([`date-format-page.tsx`](components/pages/date-format-page.tsx)) di group
  `M0.SYS` — preset clickable + preview live; setelah PUT →
  `invalidateDateFormatCache(updated)`.

**Aturan:** field tanggal baru **wajib** `<DateInput>` (atau `formatDate()`
untuk display di tabel) — **dilarang** native `<input type="date">` mentah.
**Pengecualian:** inline grid-cell editor
([`grid-cell-editor.tsx`](components/molecules/grid-cell-editor.tsx)) tetap
native `type="date"` (konteks editor sel spreadsheet autofocus/keyboard,
popover mengganggu).

**`date-range-picker.tsx` ikut aturan ini (2026-05-31):** dua native
`type="date"` di rentang sudah diganti `<input type="text">` editable
(sub-komponen internal `EditableDate`) yang **reuse pola `DateInput`** —
display via `formatDate` per `sys_settings`, ketik-manual + parse via
`parseDisplayDate`, sanitizer karakter, commit on blur/Enter. **Placeholder
`dd/mm/yyyy` browser dihapus** → `"Mulai"` / `"Selesai"`. Popover kalender
tetap mode `range`. Bukan lagi pengecualian.

**Navigasi bulan/tahun cepat via dropdown (2026-06-02):** kalender `<DateInput>`
**dan** `date-range-picker.tsx` pakai `captionLayout="dropdown"` bawaan
react-day-picker v9 → caption bulan & tahun jadi dropdown (mis. pilih
**Desember 1992** tanpa klik panah berkali-kali). Rentang navigasi dari helper
tunggal `calendarNavBounds()` di [`lib/date-format.ts`](lib/date-format.ts):
`startMonth` = Jan 1920, `endMonth` = (tahun-ini + 10) Des — cukup lebar untuk
tanggal lahir / transaksi historis dan beberapa tahun ke depan; end-year dinamis
relatif "now" supaya tidak basi. Styling dropdown = token ERP (caption sebagai
kontrol ber-border + hover, `color-scheme: light dark` untuk option list native)
di [`styles/erp-panels.css`](styles/erp-panels.css) (blok `.rdp-root`). Panah
prev/next tetap ada sebagai pelengkap. Field tanggal baru otomatis dapat ini —
cukup reuse `<DateInput>`/`DateRangePicker`, jangan set `captionLayout` ad-hoc
per pemakaian.

**Seed:** menu `/admin/date-format` (`M0.SYS.DATE-FORMAT`) ditambah di
`prisma/seed-erp.ts`. Jalankan `npm run db:seed` (idempoten) setelah pull
agar item muncul di sidebar dinamis (route tetap reachable via URL/palette
tanpa reseed).

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

## § Finance (M2) menu parity — koreksi label + folder Laporan (2026-05-31)

Audit menu **Keuangan ▸ Transaksi** legacy MyERP+ vs seed kita (`prisma/seed-erp.ts`
grup `M2`). Legacy "Transaksi" = 11 item: Kas Masuk (CR), Kas Keluar (CD), Bank
Masuk (RM), Bank Keluar (SM), Jurnal Umum (GJ), Giro Masuk/Keluar (RG/SG), Giro
Masuk/Keluar Batal (RGC/SGC), Saldo Awal Coa (CB), Buku Besar (GL = laporan).

**Koreksi label seed (kontradiksi legacy + dok desain `entities-m2-finance.md`):**
- **CB** legacy = **Saldo Awal Coa / Opening Balance** (→ `JournalType.OPENING_BALANCE`),
  bukan "Cash/Bank Transfer". Seed lama salah label → diperbaiki jadi
  `M2.TX.OPENING-BALANCE` "Opening Balance (CoA)" `/finance/opening-balances`.
- **RM/SM** legacy = **Bank Masuk/Bank Keluar**; label "Receipt Memo/Send Memo"
  membingungkan → diganti **Bank Receipt** (`RM`) / **Bank Payment** (`SM`),
  path `/finance/bank-receipts` & `/finance/bank-payments`.
  > **⚠️ Koreksi model RM (2026-05-31, lihat § Bank Masuk di bawah):** rencana
  > awal memetakan RM → `fin_ar_receipts` (AR settlement). Setelah lihat layar
  > legacy "Bank Masuk (RM)" (header + baris kontra CoA + Total, identik Kas
  > Masuk), keputusan dengan user: **RM = twin Kas Masuk** di
  > `fin_cash_bank_transactions` (`kind=BANK`, `direction=RECEIPT`), **bukan**
  > `fin_ar_receipts`. `fin_ar_receipts` dicadangkan untuk settlement AR murni
  > bila dibutuhkan, bukan untuk path `/finance/bank-receipts`.
- **BD** (Bank Disbursement) & **AJ** (Adjustment Journal) = tambahan modern,
  **tak ada** di menu Transaksi legacy ini — dipertahankan (didukung skema).

**Folder Laporan (M2.RPT) ditambah.** Legacy MODULEID=2 (`Report.vb`) punya ~150
varian laporan; di seed kita di-kanonkan jadi parent report yang dipenuhi
`fin_ledger_entries`/`fin_budget_realizations`/`fin_giros`: General Ledger,
Trial Balance, Balance Sheet, Income Statement, Cash Flow, Daily Cash & Bank,
AR Card, AR Aging, AP Card, AP Aging, Giro Maturity, Budget vs Realization
(legacyCode = `2-<MENUID>`). Sebelumnya M2.RPT cuma punya General Ledger.

**Tambahan 2 laporan finance (2026-06-07).** Audit ulang `m0_report` legacy modul
Keuangan (1.301 report; mayoritas cetakan transaksi) → jenis laporan keuangan
sejati yang belum live tinggal **dua**: **Neraca Mutasi** (`M2.RPT.MOVEMENT-BALANCE`,
`/finance/movement-balance`, legacy `neracamutasi`) = trial-balance-with-movement
(Saldo Awal · Debit · Kredit · Saldo Akhir per akun, konvensi debit-positif) dan
**Perubahan Modal** (`M2.RPT.EQUITY-CHANGES`, `/finance/equity-changes`) = statement
of changes in equity (Saldo Awal · Mutasi · Saldo Akhir per akun EQUITY + baris
Laba/(Rugi) Tahun Berjalan). Dibangun lewat subsistem **live** `erp-fin-reports`
(builder `statement-builders.ts` → `service.buildMovementBalance/buildEquityChanges`
→ controller `GET /erp/fin/reports/{movement-balance,equity-changes}` → `ReportDocument`
→ `ReportPage` wrapper tipis + route `ERP_PAGES`/`ERP_ROUTE_META` + `ReportKey`).
**Catatan arsitektur penting:** menu Laporan finance yang LIVE = subsistem
`erp-fin-reports` (`ReportPage`→`getReport`→backend SQL nyata), bukan
`financial-report.tsx`/`REPORTS`/`NAV` (itu **fallback statis basi** dengan data
mock — jangan dipakai untuk report baru). Seed `seed-erp.ts` grant semua menu ke
SUPERADMIN → 2 entri langsung tampil. Smoke-test authenticated OK (2026-06-07).

**Cakupan DB:** seluruh 11 item legacy ter-cover oleh 31 tabel `fin_*` (jauh di
atas legacy). Yang belum: **frontend M2 belum dibangun** (path `/finance/*`
ter-seed tapi belum ada entry di `ERP_PAGES`/`ERP_ROUTE_META`).

**Drift live DB (perlu prune).** `sys_menus` di DB akumulasi entri M2 usang dari
iterasi seed lama (seed tak punya prune): `M2.TX.CASHBANK-TRANSFER`,
`M2.TX.RECEIPT-MEMO`, `M2.TX.SEND-MEMO` (digantikan koreksi di atas) + duplikat
prefix `/keuangan/` lama: `M2.TX.AP-PAYMENT`, `M2.TX.AR-RECEIPT`, `M2.TX.GIRO`,
`M2.TX.JOURNAL`. FK `adm_role_menus.menu_id` = `ON DELETE CASCADE`, jadi prune
baris `sys_menus` aman (role-map ikut terhapus). Re-seed menambah kode baru tapi
**tidak** menghapus yang usang — prune manual diperlukan.

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

## § Kas Masuk / Cash Receipt (CR) — transaksi fin pertama (2026-05-31)

Fitur transaksi master-detail penuh pertama di M2 Finance — dibangun dari UI legacy
MyERP+ "Kas Masuk (CR)" tapi pakai design system + standar Senti (§2.7/§2.9).
Keputusan dengan user: **posting GL sekarang**, **state machine Senti**, **backend
cash-bank shared (wire CR dulu)**.

**Status enum diperluas (additive).** `ErpDocumentStatus` ditambah `NEED_APPROVE`,
`APPROVED`, `REJECTED` (migrasi `20260531_004_erp_document_status_workflow`,
`ALTER TYPE ADD VALUE`) agar DB sejalan dengan `lib/status.ts` (5-status canonical)
dan mendukung state machine §2.7. Dipakai semua dokumen fin/inv/pur/sls.

**Backend = 1 modul shared `erp-fin-cash-bank-transactions`** (melayani
RECEIPT/DISBURSEMENT via enum `direction`; CD/BD nyusul gratis). Endpoint
`/erp/fin/cash-bank-transactions` (`ErpJwtAuthGuard`). Pola:
- `create`: `docNumber` auto via `sys_document_numberings` (code `CASH_RECEIPT`,
  prefix `CR`) saat `auto=true`; `fiscalPeriodId` **diturunkan dari
  transactionDate** (cari periode yang memuat tanggal — tidak dipilih manual);
  `amount` header = Σ baris (server-side, tak percaya klien).
- **Workflow** `transition` (state machine): DRAFT→NEED_APPROVE→APPROVED→POSTED
  (+REJECTED, +REOPEN). Edit hanya saat DRAFT/NEED_APPROVE/REJECTED; POSTED tak
  bisa dihapus (reopen dulu).
- **Posting GL** (`cash-bank-posting.service.ts`): saat POST → generate
  `fin_ledger_entries` balanced — RECEIPT = **Dr Akun Kas (header)** + **Cr tiap
  baris**; DISBURSEMENT kebalikannya. Periode `CLOSED` ditolak. REOPEN
  hard-delete ledger milik dokumen (re-post idempoten). Validasi Σbaris=header.
- Cross-domain FK (partner/account/branch/currency) = **scalar tanpa @relation**
  → di-enrich code+name server-side (`cash-bank-enrich.ts`) agar list bawa nama.
- **E2E terverifikasi (2026-05-31):** create→submit→approve→post menghasilkan
  3 ledger entries balanced (Dr 455.000 = Cr 300.000+155.000).

**Frontend.** Editor baris kas/bank = organism reusable
[`components/organisms/cash-bank-lines.tsx`](components/organisms/cash-bank-lines.tsx)
— **satu kolom Total per baris** (No Akun · Nama · Total · Total Valas · Catatan ·
Cost Center), bukan debit/kredit (beda dari `JournalLinesEditor` jurnal umum).
SearchSelect CoA "code - name" (`loadAccountOptionsCoded`) + cost center, `NumInput`,
total footer. Form [`fin-cash-receipts-form.tsx`](components/pages/fin-cash-receipts-form.tsx):
header SearchSelect (Terima Dari/Akun Kas/Cabang/Lokasi) + tab Detail/Info + Total;
**Status read-only (badge), transisi via aksi** (§2.7). List
[`fin-cash-receipts-page.tsx`](components/pages/fin-cash-receipts-page.tsx) §2.7:
kolom legacy (No Transaksi link, Tanggal, Terima Dari, Total, Uang, Kurs, Status),
filter status + rentang tanggal, kebab + context menu workflow actions, bulk hapus,
keyboard nav, list↔form mode (back-arrow). `lib/api/fin-cash-receipts.ts` diselaraskan
ke endpoint shared (`direction=RECEIPT`) + `transitionCashReceipt`.

**Dihapus:** prototype `kas-masuk-list.tsx` + `kas-masuk-list-parts.tsx` (mock
client-side, orphaned, langgar §2.12) + route legacy `'kas-masuk'`.

**Filter list = paritas legacy (2026-05-31).** Panel filter
[`fin-cash-receipts-filters.tsx`](components/pages/fin-cash-receipts-filters.tsx)
(`CashReceiptFilters`): No Transaksi (range), Status, Tanggal (range), Terima Dari,
Lokasi, Cabang, Uraian, Catatan, User — semua **server-driven** (debounce 350ms) +
reset filter, plus search global di header. Backend query DTO menambah
`docNumberFrom/To`, `description`, `notes`, `createdById` (partner/branch/location
sudah ada). Pola filter kaya transaksi: panel terpisah di atas tabel (bukan
`FilterConfig` dropdown ErpListLayout yang cuma cocok untuk master sederhana).

**Header form = label inline 1 baris (2026-05-31).** `Field` di
[`fin-cash-receipts-form.tsx`](components/pages/fin-cash-receipts-form.tsx) diubah dari
label-di-atas (`flex flex-col`) jadi label-kiri-input-kanan (`flex items-center`, label
`w-24 shrink-0 text-left`) atas permintaan user — tiap field header jadi satu baris.

**Akun Kas [D] = picker akun kas saja (2026-05-31).** Picker `bankAccountId`
dibatasi ke akun kas/bank (sebelumnya semua akun). Paritas filter legacy MyERP+
`cgd='D' and caktif=1 and ctipe=0` → Senti `normalBalance=DEBIT` + `isActive=true`
+ `type=ASSET` (`ctipe` legacy = `AccountType`, nilai 0 = ASSET) + tambahan
`kind=POSTABLE` (header spt "Aset Lancar" tak bisa di-posting GL). Loader baru
`loadCashAccountOptionsCoded` ([items-form-lookups.ts](components/pages/items-form-lookups.ts));
DTO query account `apps/api-gateway` ditambah filter `normalBalance` (enum
`ErpNormalBalance`, additive — tanpa migrasi). Catatan: ini ikut legacy = **semua
aset debit** (termasuk piutang/persediaan), bukan murni kas/bank; belum ada flag
`isCashAccount` di `md_accounts`.

**Detail grid = spreadsheet cell-selection (2026-05-31).** Atas permintaan user,
grid Detail bukan lagi deret input aktif (search "Pencarian CoA", tombol "+ Tambah",
dan kolom trash dibuang). Default tiap cell = **terpilih (highlighted), bukan input**;
edit muncul on-demand. Dipecah jadi 4 file (<400 baris, §3): `cash-bank-line-model.ts`
(tipe + `newCashLine` + `cellColumns`), `cash-bank-line-cell.tsx` (display↔edit per
cell), `use-cash-grid-nav.ts` (state machine keyboard), `cash-bank-lines.tsx`
(organism komposit; re-export model untuk form).
- **Masuk edit (Excel-style):** klik pilih cell; **ketik / Enter / F2 / dobel-klik**
  masuk edit. Mengetik karakter langsung menyemai nilai (num/notes di-`patch`,
  akun/cost-center lewat `initialQuery` SearchSelect).
- **Navigasi:** `↑↓←→` pindah cell (saat tidak edit). Saat edit: **Enter** =
  commit & tetap di cell (keluar edit), **Tab** = commit & pindah kanan/kiri,
  **Esc** = batal (revert snapshot). Panah saat edit = gerak caret di input.
- **Tambah baris:** **Tab** di cell terakhir baris terakhir, atau **↓** di baris
  terakhir → append baris baru. **Hapus baris:** **Ctrl/Cmd+Delete** (sisakan ≥1).
- **Fokus:** root `div` `tabIndex=0` menangkap keydown; setelah nav/exit-edit di-
  refocus via `wantRoot` ref + `useLayoutEffect` (tidak mencuri fokus saat blur).
- **SearchSelect** ditambah prop reusable: `autoFocus`, `initialQuery` (semai
  pencarian saat type-to-edit), dan `onPick(value,label)` (cell butuh label untuk
  render display — `onValueChange` hanya kasih value). Additive; caller lama aman.
Berlaku ke **semua** form yang reuse organism ini (CR/CD/BD).

**Kas Keluar / CD = adopter kedua (2026-05-31).** CD disamakan penuh dengan CR:
endpoint shared `direction=DISBURSEMENT`, URL sub-route (§2.3.1), workflow actions,
slim filter bar + drawer (§2.40), keyboard nav. Karena form & filter CR/CD beda
**hanya label + arah**, keduanya diekstrak jadi organism reusable berparameter
(keputusan user — generik & share, bukan duplikat ~380 baris):
- Form: [`cash-bank-transaction-form.tsx`](components/pages/cash-bank-transaction-form.tsx)
  (`CashBankTransactionForm`, prop `labels:{partner,account}`) + model
  [`cash-bank-form-model.ts`](components/pages/cash-bank-form-model.ts)
  (`toCashBankPayload(d, direction)`). CR/CD form = wrapper tipis.
- Filter: [`cash-bank-filters.tsx`](components/pages/cash-bank-filters.tsx)
  (`CashBankFiltersBar`, prop `entityName`+`partnerLabel`) +
  [`cash-bank-filter-fields.tsx`](components/pages/cash-bank-filter-fields.tsx).
  CR/CD filter = wrapper tipis.
- Label arah: CR = "Terima Dari" / "Akun Kas [D]"; CD = "Bayar Ke" / "Akun Kas [K]"
  (paritas legacy: disbursement = Cr Akun Kas, Dr tiap baris).
- `lib/api/fin-cash-disbursements.ts` ditulis ulang ke endpoint shared
  (`direction=DISBURSEMENT`, reuse tipe CR) + `transitionCashDisbursement`. Modal
  CRUD skeleton lama (entryDate/cashAccountId/ID-input) dibuang. Registrasi pindah
  `ERP_PAGES` → `TRX_FORM_PAGES` (shell-route-renderer). File CR filter-fields lama
  dihapus (digantikan shared).

**Config FIN.CD = mirror FIN.CR (2026-06-02).** Setup data config `FIN.CD` (live
DB, bukan seed) disamakan dengan kurasi `FIN.CR` agar `cash-disbursements/new`
tampil identik dgn Kas Masuk:
- **Grid** (`sys_transaction_grid_columns`, primary grid): 15 kolom default seed →
  9 kolom kurasi CR — kolom `No.` (ROWNUM), `No. Akun` (elastis width 0),
  `Total`, `Total Valas` (hidden), `Catatan`, `Cost Center`, **Divisi / Sub Divisi /
  Proyek visible**; slot kustom (customText/Double/Date) dibuang.
- **Form** (`sys_form_fields`): tambah default record-baru CR yg belum ada di CD —
  `transactionDate.defaultValue=@today` + `currencyId.defaultValue=1` (IDR).
- Field struktural CD sudah lengkap & berlabel arah benar ("Bayar Ke"/"Akun Kas [K]")
  sejak adopter kedua; custom field uji CR ("Field Baru") **tidak** disalin.
- Kurasi grid/form = live-DB only (sama spt CR, lewat UI), **bukan** lewat seed —
  re-run `seed-erp-transaction-grids.ts` me-recreate slot kustom default (perilaku
  existing, berlaku CR & CD).

**Code kanonik Bank = FIN.RM / FIN.SM (2026-06-02).** Ditemukan mismatch: page
Bank pakai `transactionCode` **FIN.RM** (Bank Masuk) / **FIN.SM** (Bank Keluar),
tapi seed lama men-seed **FIN.BR / FIN.BP** → backend `typeByCode` lempar 404 →
config grid/form bank **tak pernah ketemu/tersimpan** (selalu fallback default).
Keputusan user: **FIN.RM/FIN.SM kanonik** (selaras frontend + abbreviation legacy
RM/SM). Tindakan:
- DB: `sys_transaction_types.code` FIN.BR→**FIN.RM**, FIN.BP→**FIN.SM** (grid ikut
  via FK, tak ter-orphan).
- Seed `seed-erp-transaction-grids.ts`: TXNS code disesuaikan ke FIN.RM/FIN.SM.
- Grid FIN.RM & FIN.SM dikurasi mirror CR (9 kolom, sama spt CD).
- Form fields FIN.RM/FIN.SM dibuat (copy 8 struktural CD + default @today/IDR),
  label arah: RM = "Terima Dari" / "Akun Bank [D]"; SM = "Bayar Ke" / "Akun Bank [K]".
- Catatan: label form sekarang dari **config DB** (prop `labels` form hanya feed
  fallback DEFAULT_FORM_FIELDS) — makanya label arah wajib benar saat seed config.
- Empat anggota kas/bank (CR/CD/RM/SM) kini paritas penuh: 8–9 form fields + 9
  kolom grid (8 visible).

**Belum (follow-up):** edit dokumen POSTED auto reverse+repost (sekarang diblok —
reopen dulu); kolom User Input di tabel (filter User sudah ada); FE BD/transfer
belum pakai backend baru ini (CR + CD sudah).


---

## §2.36 Layout baku form input transaksi (2026-05-31)

Standar posisi field untuk **semua form input transaksi** (CR sekarang; CD/BD/
giro/jurnal mengikuti). Lahir dari Kas Masuk (§ Kas Masuk) — paritas pola legacy
MyERP+ yang menaruh info dokumen di kanan-atas.

**Grid header 3 kolom** (`grid md:grid-cols-3`):
- **Kiri = identitas transaksi**: pihak/partner (Terima Dari / Bayar Ke), Akun
  Kas/Bank [D/K], Uraian.
- **Tengah = dimensi**: Cabang (required), Lokasi, (Cost Center/Divisi/Proyek bila ada).
- **Kanan = info dokumen, URUTAN BAKU dari atas:**
  1. **Tanggal** (required) — paling atas.
  2. **No Transaksi** — input + checkbox **Auto** satu baris (Auto on → readonly
     `(otomatis saat simpan)`, server generate via `sys_document_numberings`).
  3. **Uang/Kurs** — satu baris: **`SearchSelect` mata uang** + **Kurs read-only**
     inline (muted, `formatNumber(rate,2)`); kurs turunan, bukan input editable.
     Mata uang **bukan** `Select` biasa (2026-05-31): master mata uang lengkap
     (modul dunia, bisa puluhan/ratusan baris) → pakai `SearchSelect` agar bisa
     diketik/dicari + konsisten dgn picker lain di form (partner/akun/cabang).
     Loader = `loadCurrencyOptions` (`items-form-lookups.ts`); trigger label =
     `"<code> - <name>"` via `initialLabel` (derive dari list currencies yg
     sudah di-fetch). Adopter pertama = Kas Masuk (`fin-cash-receipts-form.tsx`).

**Konvensi field umum:**
- Label via helper `Field` (`<label>` horizontal): teks **rata kiri** (`text-left`,
  `w-24 shrink-0`), tanda **required `*` di belakang** teks (bukan depan — biar
  teks label sejajar).
- Status workflow = **badge read-only di toolbar** (kanan), transisi via aksi
  (§2.7) — bukan field editable di header.
- Toolbar atas: Simpan · Simpan & Baru (hanya saat create) · Reset · spacer · Badge status.

**Konsekuensi:** form transaksi baru **mulai dari** komposisi ini; jangan taruh
Tanggal/No Transaksi/Kurs di kiri atau acak. Field di luar daftar → masukkan ke
kolom yang paling sesuai (identitas=kiri, dimensi=tengah, dokumen=kanan).
Referensi implementasi: [`fin-cash-receipts-form.tsx`](components/pages/fin-cash-receipts-form.tsx).

## §2.40 Filter list = slim bar + drawer kanan (enterprise/minimalis) (2026-05-31)

Pola filter baku untuk halaman list transaksi (lahir dari Kas Masuk/CR, modul
fin lain mengikuti). Menggantikan grid 9-field yang selalu terbuka (noisy).
**Satu baris** (keputusan user 2026-05-31): kontrol filter digabung ke baris
summary `ErpListLayout` lewat slot `toolbar` — **tidak** ada baris filter
terpisah. Komposisi:

1. **Inline (di slot `toolbar`, kiri baris summary)**: quick filter **Status** +
   **Tanggal** (`DateRangePicker`) yang **apply live**, tombol **Filter** (ikon
   `filter`) dengan **badge angka** = jumlah filter lanjutan aktif, dan tombol
   **Reset** (tampil hanya saat ada filter aktif; clear semua). `Σ` summary tetap
   di kanan baris yang sama. **Label di kiri tiap kontrol inline (2026-05-31):**
   "Status" & "Tanggal" sebagai `<span class="text-xs text-muted-foreground">`
   di dalam `<label>` (flex, gap kecil) — supaya jelas field mana yang difilter
   (dropdown "Semua" + range `dd/mm/yyyy` ambigu tanpa label; placeholder hilang
   begitu ada nilai jadi tak cukup).
2. **Drawer kanan** (`components/organisms/drawer.tsx`, slide-over Radix Dialog):
   memuat **semua** field (No Transaksi range, Status, Tanggal, Terima Dari,
   Lokasi, Cabang, Uraian, Catatan, User). Edit **draft terstaging** — tidak
   menyentuh list sampai **"Terapkan"**; **"Atur ulang"** clear draft. Quick
   filter di bar (Status/Tanggal) tetap live karena di luar drawer.
3. **Filter lanjutan aktif** TIDAK ditampilkan sebagai chip terpisah (biar tetap
   1 baris) — hanya **badge angka** di tombol Filter; detailnya terlihat di
   dalam drawer. (Varian chip-bar removable sempat dibuat lalu di-rollback demi
   "1 baris aja".)

Aturan turunan:
- **Drawer** = organism reusable baru (`Drawer`/`DrawerContent`/`DrawerHeader`/
  `DrawerBody`/`DrawerFooter`, side `right|left`, size `sm|md|lg`). Mirror
  `Modal` tapi slide dari tepi. Pakai ini untuk panel filter & side surface
  lain; jangan rakit slide-over ad-hoc.
- **Label SearchSelect untuk chip**: `onValueChange` hanya kasih value, jadi
  label di-cache via wrapper `withLabelCache(loader)` (module-level
  `LABEL_CACHE`) lalu disimpan ke `*Label` di `CrFilters` saat dipilih —
  feed chip + `initialLabel`. Jangan ubah molecule `SearchSelect`.
- File: [`fin-cash-receipts-filters.tsx`](components/pages/fin-cash-receipts-filters.tsx)
  (bar + chip + drawer orchestrator) + [`fin-cash-receipts-filter-fields.tsx`](components/pages/fin-cash-receipts-filter-fields.tsx)
  (body form drawer + STATUS_OPTIONS + label cache). Split demi batas 400 baris.
- **`DateRangePicker` inline di bar = `fullWidth={false}` (2026-05-31).** Input
  tanggal **native** (`type="date"`) punya lebar intrinsik browser (~124px) yang
  **tidak bisa menyusut**; dulu dibungkus `<div style={{ width: 250 }}>` → flex
  dalam meluber & **menumpuk** di atas tombol Filter. Keputusan user: tetap
  native (typeable), **lebarkan**. Solusi: prop `fullWidth` di `DateRangePicker`
  — `true` (default) untuk konteks form/drawer (root `width:100%`), `false` untuk
  bar horizontal (root `width:fit-content` → sizing ke konten, anti-luber &
  ikut `--font-scale`). Tiap input pakai basis `124px` (`flex:'1 0 124px'` /
  `flexShrink:0`) agar `dd/mm/yyyy` + ikon picker tak terpotong. **Jangan**
  clamp `DateRangePicker` native ke lebar tetap < ~330px di flex row — pakai
  `fullWidth={false}`.

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

## § Kustomisasi Grid — layout grid transaksi (2026-05-31)

Menu **Kustomisasi Grid** (`/admin/grid-customization`, Administrator → System)
= editor layout kolom grid detail transaksi (paritas layar "Grid" legacy MyERP+).
Kiri = pohon modul→transaksi; kanan = editor kolom. Atas keputusan user:
full-stack + wire ke grid live, atribut simplified, pohon semua modul, dukung
kolom kustom.

**DB (domain `sys`):** `sys_transaction_types` (katalog penggerak pohon: code,
name, module_key/label, group_label, line_table, sort_order) + `sys_transaction_grid_columns`
(per transaksi: sort_order, header_text, data_field, width, is_visible/required/editable,
kind STANDARD|CUSTOM, data_type TEXT|NUMBER|DATE|LOOKUP, lookup_source). Kolom kustom
disimpan di **`fin_cash_bank_lines.custom_fields` (JSONB)** keyed by data_field.
Migrasi `20260531_005_erp_grid_customization` (additive, 0 DROP). Enum disimpan
sebagai TEXT + validasi app (migrasi ringan).

**Backend:** modul `erp-sys-transaction-grids` (guard `ErpJwtAuthGuard`):
`GET /erp/transaction-grids/types`, `GET/PUT /:code/columns` (PUT = replace penuh).
`CashBankLineDto`/`mapLine` + enrich pass-through `customFields`.

**Frontend:** `grid-customization-page.tsx` + `-tree.tsx` + `-columns.tsx`;
API client `lib/api/transaction-grids.ts`. Grid kas/bank (`cash-bank-lines.tsx`)
**config-driven**: prop `columns` eksplisit atau self-fetch via `transactionCode`
(mis. `"FIN.CR"`), fallback `defaultGridCols(showFx)` bila API kosong/404. Cell
render by `dataType`; label lookup non-akun di-resolve dari master kecil (cache
modul). `SearchSelect` dapat prop reusable `autoFocus`/`initialQuery`/`onPick`.

**100% config-driven — tidak ada kolom statis (2026-06-01):** grid render
**hanya** kolom dari config (visible). Kolom "No" (nomor baris) yang dulu
di-hardcode di header + tiap baris **dihapus** — sebelumnya double dengan
layout config & melanggar prinsip "tidak ada yg statis". Konsekuensi: tidak ada
nomor baris bawaan; bila perlu, tambahkan kolom sendiri lewat Kustomisasi Grid.
`colSpan` empty-state = `cols.length` (bukan `+1`).

**Semantik 4 flag kolom — live behavior (2026-06-01):** keempat flag Kustomisasi
Grid di-honor penuh di grid transaksi (`cash-bank-lines.tsx`):
- **Tampil** (`isVisible`) → kolom hanya di-render bila visible (`toGridCols` filter).
- **Edit** (`isEditable`) → cell bisa masuk mode edit; ROWNUM dipaksa non-editable.
- **Skip** (`isSkippable`) → cell **view-only**, bukan unfocusable (revisi
  2026-06-02 dgn user): cell **tetap bisa di-select** (klik / panah ↔ / Tab
  mendarat di sana, render `opacity-70` sbg petunjuk read-only) **tapi tidak bisa
  masuk mode edit**. Yang melompatinya **hanya Enter** — lihat semantik Enter di
  bawah. `useCashGridNav`: `isSelectable` (semua kolom terlihat) untuk klik/panah/
  Tab vs `enterFrom`/`stepEnter` (skip-aware) untuk Enter; `isEditableCol` =
  `isEditable && !isSkippable`.
- **Wajib** (`isRequired`) → cell **harus diisi**. Konsekuensi: (a) **tidak bisa
  tambah baris baru** selagi baris terakhir punya kolom wajib kosong
  (`appendRow` di-gate `rowRequiredMissing`, fallback notif `warn`); (b) **tidak
  bisa simpan transaksi** selagi ada kolom wajib kosong di baris mana pun — editor
  lapor via `onValidityChange` ke form (`cash-bank-transaction-form.tsx`),
  `guardSave` blokir Simpan/Simpan&Baru + notif + pindah ke tab Detail. Helper
  validasi (`isCellFilled`/`rowRequiredMissing`/`linesRequiredMissing`) di
  `cash-bank-line-model.ts`. ROWNUM dianggap selalu terisi (auto).

**Semantik Enter di grid (revisi 2026-06-02 dgn user):** Enter **bukan lagi** pembuka
edit — sekarang = **maju ke cell berikutnya** (alur data-entry cepat ala MyERP+),
**melompati kolom Skip**. Mode edit dibuka via **F2 / double-click / mengetik**.
**Mendarat di sebuah cell hanya menyeleksinya — TIDAK auto-buka editor**, termasuk
saat baris baru di-append (Enter di akhir baris terakhir → tambah baris, mendarat &
**tunggu** di cell, bukan langsung buka search). **Membuka = Enter kedua yang
disengaja:** saat sel terpilih adalah **kolom Wajib (`isRequired`) yang masih KOSONG**,
Enter **membuka** cell-nya (LOOKUP → window search `SearchSelect autoOpenModal`)
alih-alih maju. Kalau kolom wajib itu **sudah terisi** → Enter maju normal (tidak
buka ulang). Saat sedang edit: Enter = commit lalu maju, Tab = commit lalu pindah,
Esc = batal. Wiring: `moveEnter` + guard `shouldOpenOnEnter` (pakai `isCellFilled`)
di `useCashGridNav`; flag `openModal` dialirkan ke `LineCell.autoOpenModal`; helper
`isLookupCol` di `cash-bank-line-model.ts`.

**Catatan:** wiring `transactionCode` ke form CR/CD ada di file refactor cash-bank
(`cash-bank-transaction-form.tsx` dkk). Seed katalog 29 transaksi (15 modul) +
kolom default keluarga kas/bank (CR/CD/RM/SM). **Follow-up:** label lookup dimensi
(costCenter/division/…) di dokumen lama tampil id sampai master ter-resolve;
modul transaksi non-kas/bank belum punya line_table/wiring.

### Update 2026-05-31 — tabbing (banyak grid/menu) + slot renderer + skip-fokus

Atas keputusan user: **1 menu/jenis transaksi bisa punya >1 tabel** → layer
**grid/tab** baru disisipkan, dan tiap kolom dapat slot format/render + flag skip.

- **DB:** model baru **`ErpTransactionGrid` → `@@map("sys_transaction_grids")`**
  (tab: `transaction_type_id` FK, `key`, `label`, `sort_order`, `line_table?`,
  `is_primary`, `is_active`, audit; unique `[transaction_type_id, key]`). Kolom
  **pindah parent** dari `transaction_type_id` → **`grid_id`** (unique
  `[grid_id, data_field]`). Tambahan kolom di `sys_transaction_grid_columns`:
  `is_skippable BOOLEAN` (skip fokus saat tab/arrow di grid entry) +
  `label_formatter` / `header_renderer` / `cell_renderer` / `cell_editor`
  (semua **TEXT nullable**, allowlist app-level, `null` = derive dari `data_type`).
  Migrasi `20260531_007_erp_grid_tabs_renderers` (data-preserving: tiap jenis
  transaksi existing dapat 1 grid `main` primary, semua kolom di-repoint;
  `migrate deploy`).
- **Penamaan (best-practice modern grid, bukan Flex):** `labelFormatter`,
  `headerRenderer`, `cellRenderer`, `cellEditor` (peta dari istilah legacy Flex
  Label function / Header renderer / Item renderer / Item editor).
- **Katalog enum (dropdown FE = allowlist DTO):**
  `labelFormatter`: NONE·NUMBER·DECIMAL·CURRENCY·PERCENT·DATE·DATETIME·BOOLEAN ·
  `headerRenderer`: DEFAULT·REQUIRED·CENTER·WRAP·HELP ·
  `cellRenderer`: TEXT·NUMERIC·CURRENCY·BADGE·CHECK·LINK·LOOKUP ·
  `cellEditor`: TEXT·NUMBER·DATE·LOOKUP·TEXTAREA·CHECKBOX·NONE.
- **Backend:** `GET /:code/grids` + `PUT /:code/grids` (replace penuh grids+kolom);
  `GET /:code/columns` **dipertahankan** (kompat) → balikin kolom grid `is_primary`
  (dibaca `cash-bank-lines.tsx`). `getGrids` **lazy-create** grid `main` primary
  bila jenis transaksi belum punya grid (editor selalu bisa dibuka). DTO:
  `GridInputDto`/`SaveGridsDto` + allowlist enum.
- **Frontend:** tab strip `grid-customization-tabs.tsx` (pilih/ tambah/ rename
  klik-ganda/ geser/ hapus/ set primary; min 1 tab). `-columns.tsx` tambah kolom
  **Skip** (checkbox, setelah Edit) + 4 dropdown slot (opsi `— (auto)` = null).
  `-page.tsx` jadi grids-aware. API client: `getTransactionGrids`/`saveTransactionGrids`.

### Update 2026-06-01 — tipe kolom `rownum` (Nomor Urut)

Setelah kolom "No" hardcoded dihapus (lihat blok "100% config-driven" di atas),
ditambah **tipe kolom semantik `rownum`** (label dropdown **"Nomor Urut"**) supaya
nomor baris bisa dipasang lewat Kustomisasi Grid — bukan statik lagi.

- **Catalog (`lib/api/transaction-grids.ts` + DTO `save-grid-columns.dto.ts`):**
  `columnType` baru `'rownum'` → preset slot `{ labelFormatter: NUMBER, headerRenderer:
  CENTER, cellRenderer: NUMERIC, cellEditor: 'ROWNUM' }`. Slot `cellEditor` dapat
  nilai baru **`ROWNUM`** (ditambah di allowlist FE **dan** DTO backend `@IsIn`).
  `inferColumnType` memetakan `ROWNUM → rownum` agar kolom tersimpan round-trip.
- **Read-only auto:** nilai = posisi baris (`rowIndex + 1`), **tidak** disimpan ke
  data/`custom_fields`. Live grid (`cash-bank-line-cell.tsx`) render via
  `effectiveEditor === 'ROWNUM'` → angka **rata-tengah** `tabular-nums` muted; header
  ikut **rata-tengah**. `toGridCols` (`cash-bank-lines.tsx`) memaksa `isEditable=false`
  untuk kolom ROWNUM (tak bisa diketik/diedit walau admin set Edit). `rowIndex`
  dioper `cash-bank-lines.tsx` → `LineCell`.

**Update 2026-06-02 — alignment Nomor Urut = center.** Atas permintaan user, tipe
`rownum` diset rata-tengah (sebelumnya rata-kanan): preset `headerRenderer: CENTER`,
header live grid `textAlign: center` saat `cellEditor === 'ROWNUM'`, dan cell value
(display + edit control) `justify-center` (dipisah dari flag `numeric` yang tetap
`justify-end`).
- **DB:** tanpa migrasi — `columnType`/`cellEditor` sudah `String?` (allowlist app-level).

### Update 2026-06-02 — tipe kolom `lookup` = "Lookup Kustom" + source picker

Tipe kolom `lookup` (label dropdown diganti **"Lookup Kustom"**) kini bisa memilih
**sumber data** lewat picker di bawah dropdown tipe (muncul hanya saat tipe = lookup),
paritas dgn Form Builder. Sumber = `LOOKUP_SOURCE_OPTIONS` (10: Partner, Akun, Cabang,
Lokasi, Mata Uang, Cost Center, Divisi, Sub Divisi, Gudang, Proyek).

- **Editor:** `grid-customization-columns.tsx` render `GridLookupSourceCell`
  (molecule `grid-editable-cells.tsx`) di sel Tipe Kolom; `handleTypeChange`
  set `dataType='LOOKUP'` saat tipe lookup; `lookupSource` ikut deteksi
  "belum disimpan" (`isColChanged`). `lookupSource` sudah round-trip di
  `saveTransactionGrids` + DTO backend (free-form `@IsString`).
- **Unifikasi sumber (penting):** dulu ada 2 kosakata slug —
  registry Form Builder (`accounts`/`partners`/`cost-centers`/…, 10 sumber) vs
  LOADERS grid live (`account`/`partner`/`costCenter`/…, 6 sumber). Atas keputusan
  user **disatukan ke slug registry**. Resolver loader/label live grid dipindah ke
  modul baru [`lib/grid-lookup-loaders.ts`](lib/grid-lookup-loaders.ts):
  `gridLookupLoader(slug)` + `canonicalSource(slug)`. 6 sumber lama pakai loader
  `items-form-lookups` existing (jaga display akun "No · Nama"); 4 sumber baru
  (Cabang/Lokasi/Mata Uang/Gudang) via `buildLookupLoader` registry. **Slug lama
  di-ALIAS** ke kanonik (`account→accounts`, `costCenter→cost-centers`, dll) →
  baris/seed lama tetap resolve **tanpa migrasi DB**. `cash-bank-line-cell.tsx`
  pakai resolver baru. Seed `seed-erp-transaction-grids.ts` diperbarui ke slug
  kanonik.

### Update 2026-06-03 — Field Settings kolom grid: Placeholder + Nilai Default (paritas Form Builder)

Semua kolom grid (bukan hanya Lookup) kini punya **gear dialog** "Konfigurasi Kolom" berisi
**Placeholder** + **Nilai default (saat tambah baris baru)** — paritas penuh dgn
`FieldSettingsPopover` Form Builder. Lookup Kustom juga tetap dapat Konfigurasi Lookup
(sumber + urutan + filter) di bawah divider dalam dialog yang sama.

- **DB:** 3 kolom baru di `sys_transaction_grid_columns`: `placeholder TEXT`,
  `default_value TEXT`, `default_value_label TEXT` (nullable). Migrasi
  `20260603_001_erp_grid_column_field_settings` (additive, 0 DROP). `default_value_label`
  disimpan di sisi FE saat lookup di-pick (bukan di-resolve server-side) karena sumber
  grid mencakup `taxes` yang tidak punya kolom `code` — tidak bisa pakai
  `withDefaultValueLabels` sama persis seperti Form Builder.
- **Backend:** DTO `GridColumnInputDto` + service create + GET pass-through (include
  columns → 3 field baru otomatis terbawa). Prisma generate di container setelah migrasi.
- **Config UI:** [`grid-column-settings.tsx`](components/pages/grid-column-settings.tsx)
  (`GridColumnSettings`) menggantikan `grid-column-lookup-settings.tsx` (dihapus). Gear
  sekarang **ada di setiap kolom** (bukan hanya lookup) di samping dropdown Tipe. Dialog
  = Placeholder (Input) + Nilai default (type-aware via
  [`grid-column-default-editor.tsx`](components/pages/grid-column-default-editor.tsx):
  SearchSelect untuk lookup/account_picker/partner_picker, NumInput untuk numerik,
  DateInput untuk date, BooleanRadio untuk checkbox, Input untuk text/textarea) + Lookup
  section (hanya tipe lookup). `hasGridColumnConfig` highlight gear biru bila ada config.
- **Live grid — apply defaults (3 consumers):** `GridCol` + `toGridCols` tiap consumer
  (sls/inv/cashbank) propagate `placeholder`/`defaultValue`/`defaultValueLabel`.
  `placeholder` ditampilkan di editor cell (`SearchSelect`, `Input`, `Textarea`) dan di
  empty-cell display. Default diterapkan via 2 path: (a) **saat append baris baru**
  (`useGridNav.appendRow`/`removeRow` call `applyColumnDefaults` dari `grid-line-core.ts`);
  (b) **saat form buka** (`useSeedLineDefaults` hook: sekali saat config load, hanya baris
  pristine/kosong, tidak re-fill setelah user clear). Helper baru di `grid-line-core.ts`:
  `applyColumnDefaults`, `colsHaveDefaults`, `isRowPristine`.

### Update 2026-06-02 — Konfigurasi Lookup kolom grid (sumber + urutan + filter)

Kolom tipe **Lookup Kustom** kini punya **gear popover** (di sel Tipe Kolom, di
samping picker sumber) berisi **Konfigurasi Lookup** lengkap: Sumber data +
**Urutan default** + **Filter default** — paritas penuh dgn Form Builder.

- **DB:** 2 kolom baru di `sys_transaction_grid_columns`:
  `lookup_default_filter JSONB` + `lookup_default_sort TEXT` (nullable, mirror
  `sys_form_fields`). Migrasi `20260602_001_erp_grid_lookup_config` (additive,
  0 DROP; `migrate deploy` + `prisma generate` di container + restart).
- **Backend:** DTO `GridColumnInputDto` + service create + GET pass-through
  (include columns → otomatis terbawa).
- **Editor reuse (DRY):** komponen generik **`LookupSortFilterFields`** diekstrak
  dari `form-builder-lookup-config.tsx` (props: source/sourceEditable/defaultSort/
  defaultFilter/onChange/resetKey). `LookupConfigSection` (Form Builder) jadi
  wrapper tipis; popover grid =
  [`grid-column-lookup-settings.tsx`](components/pages/grid-column-lookup-settings.tsx)
  (`GridColumnLookupSettings`) pakai komponen generik yg sama. Schema sort/filter
  per-sumber dari registry (`getSourceSchema`).
- **Live grid:** `gridLookupLoader(source, defaultFilter?, defaultSort?)` kini
  merge filter+sort ke tiap fetch via `buildLookupLoader` (semua sumber lewat
  registry — loader akun shape-identik dgn loader lama, display "No · Nama"
  tetap). `GridCol` + `toGridCols` + `cash-bank-line-cell.tsx` membawa 2 field
  baru. API client `ErpGridColumn` + `saveTransactionGrids` + `isColChanged`
  (deep-compare filter) ikut.

### Update 2026-06-02 — `inferColumnType` hormati `dataType` (label "Tipe Kolom" jujur)

**Bug:** kolom Cost Center (juga Divisi/Sub Divisi/Proyek/Akun) tampil **"Text"**
di layar Kustomisasi padahal di grid live render **lookup** (icon search). Akar:
kolom seed lama hanya mengisi `dataType` (`LOOKUP`) — slot semantik
`cellEditor`/`columnType` masih `null`. Dua jalur baca tipe **tidak konsisten**:
`effectiveEditor()` (grid live) fallback ke `dataType`, sedangkan `inferColumnType()`
(layar Kustomisasi) **mengabaikan** `dataType` → selalu `return 'text'`.

**Fix:** `inferColumnType(labelFormatter, cellRenderer, cellEditor, dataType?)` —
tambah param `dataType` sebagai fallback terakhir (`LOOKUP→lookup`, `NUMBER→number`,
`DATE→date`, else `text`), mirror `effectiveEditor()`. Call site
`grid-customization-columns.tsx` oper `col.dataType`. Hasil: kolom FK lookup lama
kini tampil **"Lookup Kustom"** + picker sumber-nya muncul; kolom numerik tampil
"Number". **Tanpa migrasi / perubahan seed** — murni perbaikan inferensi label.

**Keputusan dengan user (2026-06-02):** Cost Center/Divisi/Sub Divisi/Proyek
**tetap lookup** (bukan free text). Alasan: field standar = **FK numerik**
(`fin_cash_bank_lines.cost_center_id` dst = `BigInt? → ErpCostCenter`) dan save
path `toBigInt(BigInt(v))` akan **error** bila diisi teks bebas. Free-text "cost
center" (bila perlu) = **kolom CUSTOM** terpisah (disimpan di `customFields` JSON),
bukan mengubah slot FK standar.

---

## § Bank Masuk (RM) — twin Kas Masuk + Cara Bayar + Giro (2026-05-31)

Build halaman **Bank Masuk / Bank Receipt** (`/finance/bank-receipts`, legacyCode
`RM`), meniru layar legacy MyERP+ (amati-tiru-modifikasi).

**Keputusan dengan user:**
- **Model = twin Kas Masuk**, BUKAN `fin_ar_receipts`. Bank Masuk dibangun di
  atas modul **shared** `erp-fin-cash-bank-transactions` (sama dgn CR/CD), dengan
  diskriminator baru `kind=BANK` + `direction=RECEIPT`. Layar legacy "Bank Masuk
  (RM)" = header + baris kontra CoA + Total, identik Kas Masuk (hanya "Akun Bank
  [D]" + Cara Bayar + tab Giro) — jadi reuse pola Kas Masuk, bukan layar alokasi
  AR. Mengoreksi catatan menu-parity yang sempat memetakan RM → `fin_ar_receipts`.
- **Full parity**: sertakan **Cara Bayar** (enum `ErpPaymentMethod`, default
  `TRANSFER`) + **tab Giro** yang berfungsi.

**Skema (additive, migrasi `20260531_006_erp_cash_bank_kind_payment_method`, 0 DROP):**
- Enum baru `ErpCashBankKind { CASH, BANK }`.
- `fin_cash_bank_transactions` + kolom `kind` (`ErpCashBankKind` NOT NULL default
  `CASH` → baris CR/CD lama otomatis `CASH`) & `payment_method`
  (`ErpPaymentMethod` nullable). Index `(kind, direction, status)`.
- **Doc numbering** di-key per (kind, direction): `BANK_RECEIPT` prefix **RM**
  (seed `seed-erp.ts` + insert idempotent ke DB live). CR/CD tetap.
- **Giro tab** = baris giro disimpan sebagai rekor **`fin_giros`** (type
  `INCOMING`, `source='CASH_BANK_TXN'`, `sourceTransactionId`=id transaksi,
  status `OUTSTANDING`). Sinkron seperti baris kontra: hard delete + recreate saat
  update (dokumen masih editable/pre-post → belum ada clearing), soft-delete saat
  transaksi dihapus. Field per giro: No Giro/Cek, Bank Penerbit, Jatuh Tempo,
  Nominal, Catatan.

**Posting GL:** tidak berubah — RECEIPT tetap Dr akun bank (header) / Cr baris
kontra, apa pun Cara Bayar. **Asumsi/ditunda:** nuansa akuntansi giro-belum-cair
(Dr Giro/Notes Receivable lalu pindah saat clearing) belum dimodelkan; giro di tab
ini = pencatatan instrumen + dasar untuk modul Receipt Giro Clearing (RGC) ke
depan. Eskalasi bila perlu posting giro yang berbeda.

**Backend (shared module `erp-fin-cash-bank-transactions`):**
- DTO create + `kind`/`paymentMethod`/`giros[]` (`CashBankGiroDto`); query DTO
  + `kind`. `genDocNumber(tx, kind, direction)`. Helper `syncGiros`/`loadGiros`;
  `one()` melampirkan `giros`. Guard tetap `ErpJwtAuthGuard` (§2.5).
- Regresi Kas Masuk aman: default `kind=CASH`, jalur CR tak berubah.

**Frontend (adopter §2.3.1 — reuse, jangan fork):**
- `lib/api/fin-bank-receipts.ts` (reuse tipe shared dari `fin-cash-receipts`,
  `kind:'BANK'`, + `paymentMethod`/`giros`).
- `fin-bank-receipts-form.tsx` = **wrapper tipis** atas form shared
  `cash-bank-transaction-form.tsx` (header/Detail/Info dari sana). Dua hal khas
  bank di-inject via slot **baru** form shared: **`headerExtra`** (Cara Bayar
  Select §2.6 → 6 opsi) + **`extraTabs`** (tab Giro = organism reusable
  `components/organisms/cash-bank-giros.tsx`, dipakai bareng Bank Keluar). Slot
  additive → CR/CD tak terpengaruh. Model `fin-bank-receipts-form-model.ts`
  extend `CashBankFormData` + `paymentMethod`/`giros`, reuse mapper shared
  (`toCashBankPayload`). (Editor giro standalone yang sempat dibuat → dihapus,
  diganti organism shared agar tak duplikat.)
- `fin-bank-receipts-page.tsx` (list + router sub-route), reuse filter
  `CashReceiptFilters` + workflow `cashBankWorkflowActions`. Kolom list +
  **Cara Bayar**.
- Routing: daftar `/finance/bank-receipts` di `TRX_FORM_PAGES`
  (shell-route-renderer) + `ERP_ROUTE_META` (`lib/nav.ts`).

## § Bank Keluar (SM) — twin Kas Keluar + Cara Bayar + Giro (2026-05-31)

Build halaman **Bank Keluar / Bank Disbursement** (`/finance/bank-disbursements`,
legacyCode `SM`), meniru layar legacy MyERP+ "Bank Keluar (SM)". Sibling Bank Masuk
(RM, § atas) — arah keluar. Reuse fondasi shared yang sama (jangan fork).

**Keputusan dengan user (2026-05-31):**
- `/finance/bank-disbursements` = **Bank Keluar (SM)** di atas modul **shared**
  `erp-fin-cash-bank-transactions`, `kind=BANK` + `direction=DISBURSEMENT`. Judul
  page + sidebar = **"Bank Keluar"**, code-tag **SM** (override konvensi English
  untuk item ini, atas pilihan user eksplisit).
- **Rapikan duplikat**: entri menu lama `/finance/bank-payments` ("Bank Payment",
  SM→`fin_ap_payments`) **dihapus** dari seed + di-prune dari DB live (idempotent),
  karena duplikat konsep dengan halaman ini.

**Skema:** tak ada perubahan baru — reuse migrasi `20260531_006` (kind +
payment_method + index) dari Bank Masuk. **Doc numbering** key
`BANK_DISBURSEMENT` prefix **SM** (seed `seed-erp.ts` + insert idempotent ke DB
live). **Giro tab** = `fin_giros` type **`OUTGOING`** (derive dari direction),
`source='CASH_BANK_TXN'`, sinkron hard delete+recreate (sama pola Bank Masuk;
dasar modul Send Giro Clearing/SGC ke depan).

**Backend (shared module):** tambah filter **`paymentMethod`** di query DTO +
`where` service (melengkapi `kind` dari Bank Masuk). `genDocNumber`/`syncGiros`/
`loadGiros` sudah generik (dipakai RM & SM). Regresi CR/CD/RM aman.

**Frontend (adopter §2.3.1 — reuse, jangan fork):**
- `lib/api/fin-bank-disbursements.ts` (reuse tipe shared dari `fin-cash-receipts`,
  `direction:'DISBURSEMENT'` + `kind:'BANK'`).
- `fin-bank-disbursements-form.tsx` = **wrapper tipis** atas `cash-bank-transaction-form.tsx`,
  inject **Cara Bayar** (Select §2.6, 6 opsi, default `TRANSFER`) via `headerExtra`
  + **tab Giro** via `extraTabs` (organism shared `cash-bank-giros.tsx`). Label
  bank: "Bayar Ke" + "Akun Bank [K]". Export `paymentMethodLabel` untuk kolom list.
- **Model**: SM pakai **shared `cash-bank-form-model.ts`** langsung —
  `CashBankFormData` diperluas `kind`/`paymentMethod`/`giros`; `defaultCashBankForm('BANK')`
  set `paymentMethod='TRANSFER'`; `toCashBankPayload` kirim `giros` hanya saat
  `kind='BANK'` (cash → `undefined`, backend skip sync). Types di `fin-cash-receipts.ts`
  + `ErpCashBankGiro`/`CashBankGiroPayload`/`ErpPaymentMethod`/`ErpCashBankKind`.
- `fin-bank-disbursements-page.tsx` (list + router sub-route §2.3.1), reuse filter
  bar shared (wrapper `fin-bank-disbursements-filters.tsx`, "Bank Keluar"/"Bayar Ke")
  + `cashBankWorkflowActions`. Kolom list + **Cara Bayar**.
- Routing: `/finance/bank-disbursements` dipindah dari `ERP_PAGES` ke
  `TRX_FORM_PAGES` (shell-route-renderer) + `ERP_ROUTE_META` (`lib/nav.ts`).
- **Ditunda**: filter Cara Bayar di drawer (butuh ubah `cash-bank-filter-fields`
  shared); pass ini cukup **kolom** Cara Bayar di list. Backend filter
  `paymentMethod` sudah siap → tinggal wire field saat melanjutkan.

## § Form Builder — pengaturan field per-jenis transaksi (placeholder, nilai default, read-only) (2026-06-01)

Form Builder (`/admin/form-builder`, GET/PUT `/api/erp/transaction-forms/:code/fields`)
mengonfigurasi field header form transaksi (CR/CD/BD/RM). Sebelumnya bisa atur:
label, tipe, visible, wajib, kolom (slot), urutan, dan untuk lookup → sumber +
filter + urutan default. **Ditambah (2026-06-01)** tiga atribut per-field, berlaku
untuk **semua** tipe field:

- **`placeholder`** (`String?`) — teks petunjuk saat kosong. `null`/`''` → form
  pakai placeholder bawaannya (mis. "Pilih partner…"). Gantikan hardcode di form.
- **`defaultValue`** (`String?`) — nilai prefilled saat **tambah baru** (record
  tanpa `id`). Lookup menyimpan **id**; tipe lain menyimpan string mentah.
- **`isReadonly`** (`Boolean @default(false)`) — field selalu non-edit, **terlepas**
  dari status workflow. Berbeda dari `locked` (yang diturunkan dari status dokumen).

**DB/Backend:** kolom `placeholder` / `default_value` / `is_readonly` di
`sys_form_fields` (migrasi hand-written `20260601_004_form_fields_field_settings`,
`prisma migrate deploy` + `generate` di dalam container — §2.32/§2.34). DTO
`FormFieldInputDto` + `ErpFormFieldsService.saveFields` persist ketiganya.

**UI (atomic, reusable):**
- `components/pages/form-builder-field-settings.tsx` → `FieldSettingsPopover`:
  **satu** gear per baris, muncul untuk **semua** tipe field. Isi: Placeholder,
  Nilai default (editor type-aware: SearchSelect untuk lookup, DateInput/NumInput/
  Input untuk DATE/NUMBER/lainnya), dan **Kunci (read-only)** = `BooleanRadio`
  (§2.6 — pilihan biner = radio).
- `form-builder-lookup-config.tsx` di-refactor: body lookup (sumber/sort/filter)
  diekspor sebagai `LookupConfigSection` (tanpa popover wrapper) + helper
  `hasLookupConfig`. `FieldSettingsPopover` me-render section ini untuk tipe lookup
  → **satu** popover gabungan, bukan dua gear.
- **Footer dialog = Tutup + Simpan-ke-DB (2026-06-02):** dialog meng-edit draft
  *live* via `onUpdate` (propagasi ke state `fields`), tapi footer punya tombol
  **Simpan** yang memanggil `onSave` (= `handleSave` halaman → `saveFormFields`)
  lalu menutup dialog (`DialogClose`) — jadi user bisa langsung persist ke DB
  dari dalam dialog tanpa harus cari tombol Simpan di toolbar. `onSave`/`saving`
  di-thread `form-builder-page` → `FormBuilderFields` → `FieldRow` →
  `FieldSettingsPopover` (opsional; fallback tombol **Tutup** saja bila tak ada).
  Tetap satu endpoint bulk (`saveFormFields(code, fields)`) — Simpan dialog
  menyimpan **seluruh** konfigurasi field, identik dgn tombol Simpan toolbar
  (Undo/Redo tetap berlaku). Tombol toolbar tidak dihapus.

**Konsumsi form (`cash-bank-transaction-form.tsx` + `cash-bank-custom-fields.tsx`):**
- Placeholder: `ph(key, fallback)` = `config.placeholder || fallback`.
- Read-only: `ro(key)` = `locked || config.isReadonly`.
- Default: `formDefaultsPatch(data, config)` (`cash-bank-form-model.ts`) menghitung
  patch nilai default → diterapkan **sekali** via effect saat record baru & config
  sudah load (guard `useRef`); **hanya** mengisi field yang masih kosong (tidak
  pernah menimpa input user). Structural keys map langsung ke `CashBankFormData`;
  custom keys masuk `customFields`. Untuk structural **lookup** (partner/account/
  branch/location), patch **ikut mengisi `*Label`** dari `defaultValueLabel` config
  → picker langsung tampil label benar tanpa round-trip.
- **Default tanggal: "Hari ini" dinamis + fixed (2026-06-02):** editor nilai default
  field DATE = segmented `Kosong / Hari ini / Tanggal tetap` (`DateDefaultEditor` di
  `form-builder-field-settings.tsx`). "Hari ini" simpan sentinel **`@today`**
  (`TODAY_DEFAULT` di `lib/api/form-fields.ts`); `formDefaultsPatch` resolve `@today`
  → `todayIso()` saat apply. **Tanggal transaksi 100% config-driven (2026-06-02):**
  baseline hardcode `today` di `defaultCashBankForm` **dihapus** (`transactionDate: ''`)
  — keputusan user "benar-benar kosong". Jadi: **Kosong** → field blank (user isi
  manual; tetap `required`), **Hari ini** → today, **Tanggal tetap** → fix. Semua
  via `formDefaultsPatch` fill-empty biasa (tak perlu override lagi). Konsekuensi:
  jenis transaksi tanpa default tanggal terkonfigurasi → form mulai blank (bukan
  today). Backend simpan `@today` apa adanya (string); resolver label lookup tak
  menyentuhnya (DATE bukan lookup).
- **Currency default = config-driven (2026-06-02, FIXED):** dulu effect mount
  meng-hardcode `currencyId = find(IDR) ?? currencies[0]` dengan closure `data`
  basi → **menimpa** default Form Builder; karena IDR (id=1) di luar 100 baris
  pertama (`createdAt desc`, 172 currency) malah men-set currency acak + label
  kosong. Sekarang effect mount **hanya** memuat list currency; default currency
  diputuskan effect `formDefaultsPatch` (config = sumber kebenaran), fallback base
  currency (IDR) **hanya** bila config tak punya default. Effect default menunggu
  **config + currencies** ter-load (deterministik). `currencyLabel` fallback ke
  `config.byKey['currencyId'].defaultValueLabel` saat currency tak ada di list.
- **`SearchSelect` hormati `initialLabel` untuk value async (2026-06-02, ROOT CAUSE):**
  bug field Uang "Kosong" yang membandel ternyata di primitif `SearchSelect`
  (`use-search-select.ts`), bukan alur data. `initialLabel` dulu **hanya** dipakai
  di effect mount yang ber-deps `[]` — saat default form di-apply **setelah** mount
  (async), value berubah jadi `'1'` tapi effect mount sudah lewat, dan effect
  `[props.value, options]` cuma cek `options` (IDR tak ada di halaman pertama) →
  `displayLabel` kosong walau `value` & `initialLabel` benar. Fix: effect itu kini
  fallback ke `initialLabel` saat value tak ketemu di `options` (deps tambah
  `initialLabel`). Berlaku **semua** picker (partner/akun/cabang/lokasi/uang) yang
  value-nya di-set async — diverifikasi via console log: data benar, render primitif
  yang putus.

**Label nilai default lookup di-resolve server-side (2026-06-02, FIXED):** dulu
`defaultValue` lookup hanya menyimpan id; saat dialog dibuka ulang `SearchSelect`
me-resolve label dari `loadOptions('', 1, limit)` (halaman pertama). Bila row
tersimpan ada di luar halaman 1 (mis. IDR id=1 di antara 172 mata uang yang
di-sort `createdAt desc`), label **tidak ketemu → field tampak kosong** → user
mengira "tidak tersimpan" (padahal id tersimpan benar). **Fix:** backend
`GET /transaction-forms/:code/fields` kini mengembalikan `defaultValueLabel`
(`{code} - {name}`) untuk tiap field lookup ber-`defaultValue`, di-resolve di
`erp-form-fields/lookup-label-resolver.ts` (`withDefaultValueLabels` — group id
per slug, 1 query/slug, mendukung 10 sumber + alias slug lama). FE: tipe
`ErpFormField.defaultValueLabel` + `DefaultValueEditor` oper `initialLabel` ke
`SearchSelect`. Tanpa kolom DB baru (derived). **Catatan:** Kustomisasi Grid
punya pola serupa untuk default lookup kolom — belum diberi resolver yang sama
(belum dilaporkan bermasalah).

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

## § Setup config Form Builder + Kustomisasi Grid — transaksi non-kas/bank (2026-06-02)

Atas permintaan user ("kerjakan ini semua kecuali cash/bank, kerjakan di form build
dan grid custom dulu"): **config-only** — siapkan default Form Builder (header
`sys_form_fields`) + Kustomisasi Grid (`sys_transaction_grids` + kolom) untuk **9
transaksi Finance non-kas/bank**, mirip setup FIN.CD. **Halaman TIDAK direfactor
pass ini** (jurnal/giro masih hardcoded `JournalLinesEditor` — lihat follow-up).

**Cakupan (9 kode, keputusan user "7 menu + JM & RV"):** General Journal `FIN.GJ`,
Adjustment Journal `FIN.AJ`, Journal Memorial `FIN.JM`, Receipt Giro `FIN.RG`, Send
Giro `FIN.SG`, **Receipt Giro Clearing `FIN.RGC`** (baru), **Send Giro Clearing
`FIN.SGC`** (baru), Revaluasi Valas `FIN.RV`, Opening Balance (CoA) `FIN.BB`.
Catalog `sys_transaction_types` diselaraskan ke label menu: RG/SG = "Receipt/Send
Giro" (dari "Receive/Spend"), BB = "Opening Balance (CoA)" (dari "Beginning
Balance"); RGC/SGC ditambahkan.

**3 famili grid (`seed-erp-transaction-grids.ts`, `GridFamily` + `LINE_TABLE_BY_FAMILY`):**
- **journal** (GJ/AJ/JM/RV/BB) → `fin_journal_lines`. Kolom default: No (rownum) ·
  Akun (lookup `accounts`, wajib) · Debit · Kredit · Catatan · Cost Center · Divisi /
  Sub Divisi / Proyek (lookup, hidden).
- **giro** (RG/SG) → `fin_giros`. **Keputusan user: grid = instrumen giro
  (domain-benar), BUKAN baris jurnal Debit/Kredit** (halaman hardcoded lama yang
  memakai JournalLinesEditor = scaffolding sementara, bukan acuan). Kolom: No ·
  No Giro/Cek (`giroNumber`, wajib) · Bank Penerbit (`bankName`) · Jatuh Tempo
  (`dueDate`) · Nominal (`amount`) · Catatan.
- **giroClearing** (RGC/SGC) → `fin_giros`. Kolom: No · No Giro/Cek · Jatuh Tempo ·
  Nominal · **Tgl Cair (`clearedDate`, wajib)** · **Akun Bank (`bankAccountId`,
  lookup accounts)** · Catatan.
- Tanpa slot kustom hidden (beda dari cash/bank) — mengikuti hasil kurasi cash/bank
  (custom slot ditambah via UI bila perlu). `rownum` pakai preset
  `cellEditor=ROWNUM` (center). Grid `main` primary per transaksi.

**Form Builder header default (`erp-form-fields.service.ts` → `DEFAULTS_BY_CODE`):**
field-key **native model jurnal/giro** (mis. `entryDate`, bukan `transactionDate`
ala kas/bank) — di-bind form config-driven jurnal/giro di masa depan.
- journal (GJ/JM/RV): Uraian + Catatan (LEFT) · Cabang (CENTER) · Tanggal · No
  Transaksi · Uang (RIGHT). **BB** = sama, label tanggal "Tanggal Saldo Awal".
  **AJ** = + Partner (opsional, LEFT).
- giro (RG): Terima Dari (partner) + Uraian (LEFT) · Cabang · Tanggal · No
  Transaksi · Uang. **SG** = partner "Bayar Ke". Instrumen (No Giro/Bank/Jatuh
  Tempo/Nominal) ada di **grid**, bukan header.
- giroClearing (RGC/SGC): Akun Bank (account, filter kas/bank) + Uraian · Cabang ·
  Tanggal Cair · No Transaksi · Uang.

**Di mana config hidup (penting):** beda dari kurasi cash/bank yang live-DB-only,
9 transaksi ini belum punya baseline di code → baseline **ditulis version-controlled**
(grid kolom di `seed-erp-transaction-grids.ts`; header default di `DEFAULTS_BY_CODE`),
konsisten dgn tempat baseline cash/bank berada. Karena container `nest --watch`
(bind-mount) tidak selalu recompile, config **juga di-apply langsung ke DB live**
(idempotent): grid kolom + **baris `sys_form_fields` di-seed langsung** supaya
`getFields()` mengembalikan default benar tanpa bergantung lazy-seed (mencegah
fallback `CR_DEFAULTS` salah ter-persist bila Form Builder dibuka sebelum recompile).

**Follow-up (belum, di luar pass ini):**
- Halaman jurnal/giro/clearing **belum** config-driven (masih hardcoded; tidak oper
  `transactionCode`). Wiring = fase terpisah (perlu komponen grid jurnal Debit/Kredit
  & grid instrumen giro yang baca config, + form header render dari Form Builder).
- **Opening Balance** belum punya halaman frontend tersendiri (kini = `journalType`
  di General Journal). Catalog/config `FIN.BB` sudah siap.
- Posting GL & line-table backend untuk giro/clearing (`fin_giros` clearing flow)
  belum dibangun; instrumen giro saat ini = pencatatan, dasar modul clearing.

## § Sales Order (SO) — transaksi sales pertama + grid engine generik (2026-06-02)

Build transaksi **item-based** pertama di m5 Sales — pola **persis cash/bank**
(config-driven header via Form Builder + grid baris via Kustomisasi Grid + §2.36
layout + state machine §2.7 + sub-route URL §2.3.1), tapi baris = **item**
(Item·Qty·Satuan·Harga·Disc·Pajak·Total), bukan kontra-akun + Total tunggal.
Keputusan user: pilot **Sales Order**, **full backend incl. posting**, dan **semua
16 transaksi sales terdaftar** di Form Builder + Kustomisasi Grid.

**Grid engine digeneralkan (bukan fork).** Mesin grid spreadsheet (navigasi cell,
edit-state, render kolom config-driven) diekstrak jadi **generik model-agnostik**:
- [`grid-line-core.ts`](components/organisms/grid-line-core.ts) — `GridCol`/`GridDataType`/
  `GridRowBase` + interface adapter **`GridModel<Row>`** (`newRow`/`getCellRaw`/
  `buildCellPatch`) + helper generik (`isLookupCol`/`isCellFilled`/`rowRequiredMissing`/
  `linesRequiredMissing`).
- [`use-grid-nav.ts`](components/organisms/use-grid-nav.ts) — `useGridNav<Row>` generik
  (logika dipindah dari `use-cash-grid-nav`). `LineCell` ([cash-bank-line-cell.tsx](components/organisms/cash-bank-line-cell.tsx))
  sudah row-agnostic → dipakai apa adanya (semua editor: ROWNUM/LOOKUP/NUMBER/
  DISCOUNT/STEPPER/DATE/TEXTAREA/CHECKBOX/…).
- Cash/bank **tidak berubah perilaku**: `cash-bank-line-model.ts` kini mendefinisikan
  `cashBankGridModel` + delegasi helper ke core; `use-cash-grid-nav.ts` = wrapper tipis
  (`useGridNav` + `cashBankGridModel`). `cash-bank-lines.tsx` tak disentuh. **Jangan
  fork mesin grid** — bikin `GridModel<Row>` baru + organism konsumen.
- Sales: [`sls-item-line-model.ts`](components/organisms/sls-item-line-model.ts)
  (`SlsItemLineRow` + `slsItemGridModel`; `lineTotal` = derived qty×harga−disc, read-only/skip)
  + organism [`sls-item-lines.tsx`](components/organisms/sls-item-lines.tsx).

**Backend = modul baru `erp-sls-orders`** (`/erp/sls/orders`, `ErpJwtAuthGuard`),
mirror cash/bank: create/list/get/update/remove + `transition` (state machine
DRAFT→NEED_APPROVE→APPROVED→POSTED + REJECT/REOPEN), `docNumber` auto via
`sys_document_numberings` code **`SO`** (tabel `sls_orders` punya **dua** kolom unik
NOT NULL `code` + `doc_number` → di-set sama), `fiscalPeriodId` diturunkan dari
`docDate`, `subtotal`/`grandTotal` dihitung server-side (`lineNet = qty×harga −
disc`; `grandTotal = subtotal + Σpajak baris + pajak/biaya header − disc header`).
Enrich cross-domain (customer/branch/location/warehouse/currency/paymentTerm/
salesDept + per-baris item/unit/tax/warehouse) = scalar FK tanpa `@relation`,
di-resolve code+name server-side.

**SO TIDAK posting GL (penting).** Sales Order = dokumen komitmen, bukan peristiwa
finansial → `SlsOrderPostingService.postToLedger` = **no-op terdokumentasi** (POST
hanya cek periode tak CLOSED + set POSTED/postedAt; **0 baris `fin_ledger_entries`**).
Signature paralel `CashBankPostingService` agar **Sales Invoice (SI)** nanti isi
posting AR/revenue sungguhan. **E2E terverifikasi (2026-06-02):** create (subtotal=
grandTotal 202.500 dari 2 baris) → submit → approve → post (POSTED, 0 ledger) → list.

**Frontend (adopter §2.3.1).** API client [`lib/api/sls-orders.ts`](lib/api/sls-orders.ts);
form shared [`sales-transaction-form.tsx`](components/pages/sales-transaction-form.tsx)
(header 100% dari Form Builder via `useFormFields('SLS.SO')`, dispatch struktural
→ [`sls-structural-field.tsx`](components/molecules/sls-structural-field.tsx), custom →
reuse `CashBankCustomField`; Detail = `sls-item-lines`; fallback `DEFAULT_SLS_FORM_FIELDS`)
+ wrapper tipis [`sls-order-form.tsx`](components/pages/sls-order-form.tsx) +
model [`sls-order-form-model.ts`](components/pages/sls-order-form-model.ts). List/router
[`sls-orders-page.tsx`](components/pages/sls-orders-page.tsx) (reuse `cashBankWorkflowActions`
= mesin §2.7 yang sama) + filter slim [`sls-orders-filters.tsx`](components/pages/sls-orders-filters.tsx).
Daftar `/sales/orders` di `TRX_FORM_PAGES` (shell-route-renderer) + `ERP_ROUTE_META`.

**Lookup source baru (registry).** [`lookup-source-registry.ts`](lib/lookup-source-registry.ts)
ditambah `items`/`units`/`taxes`/`payment-terms` (selain 10 lama) → dipakai grid item
(Item/Satuan/Pajak) & header (Termin). `items` dikecualikan dari eager label-fetch di
`LineCell` (katalog besar — pakai label tersimpan/enriched, sama spt `accounts`).

**Katalog + config 16 transaksi sales (`seed-erp-transaction-grids.ts` +
`seed-erp-sales-forms.ts`).** Famili grid baru **`salesItem`** (kolom item-based default,
`lineTable` per-txn karena tiap dokumen sales punya tabel baris sendiri). 16 type
`SLS.*` di `sys_transaction_types` (kode lama `SLS.INV`/`SLS.RET` **di-prune** →
diganti `SLS.SI`/`SLS.SR`). 9 dokumen item-based (SQ/SO/PI/PL/DO/DR/SI/RNR/SR) dapat
grid + 12 field header (`sys_form_fields`); 7 dokumen pembayaran/alokasi (AS/IP/RP/IC/
PV/SIE/BB) = katalog saja (belum item-based). **Form kerja penuh baru SLS.SO.**

**Kontrak dataField/fieldKey (jangan rename).** Grid SLS.SO: `rowNo`,`itemId`,`quantity`,
`unitId`,`unitPrice`,`discountPercent`,`discountAmount`(hidden),`tax1Id`,`lineTotal`
(skip/derived),`warehouseId`(hidden),`notes`,`costCenterId`/`divisionId`/`subdivisionId`/
`projectId`(hidden). Form SLS.SO: `customerId`/`description`/`referenceNo` (LEFT) ·
`branchId`/`locationId`/`warehouseId`/`salesDeptId` (CENTER) · `docDate`(@today)/`docNumber`/
`currencyId`(default 1)/`paymentTermId`/`dueDate` (RIGHT).

**Follow-up:**
- Kolom **custom** baris sales belum persist (`sls_*_lines` tak punya `custom_fields`
  JSONB; cash/bank punya). Tambah kolom JSONB additif bila perlu custom line column.
- SI/DO/dst belum punya backend/form (hanya katalog + grid/form config). SI = isi
  posting AR/revenue di `SlsOrderPostingService` pola.
- **REOPEN dari POSTED → 400** (mesin `NEXT` tak punya transisi POSTED→REOPEN; **sama
  persis** dgn cash/bank — gap warisan, bukan regresi). UI kebab menawarkan "Reopen"
  di POSTED tapi backend menolak. Perbaiki serempak dgn cash/bank di pass terpisah.

---

## M3 Warehouse & Inventory — 10 Transaksi (2026-06-03)

Semua 10 transaksi M3 dibangun end-to-end: config layer (Form Builder + Kustomisasi Grid)
untuk semua 10, backend + frontend untuk 7 transaksi utama; PA/RW/DC punya bentuk
khusus (diuraikan di bawah).

### Pola umum (MR/TS/RS/RF/SA/IB/SP)

- **Backend**: satu module per tabel (`erp-inv-stock-movements` shared untuk 4 dokumen
  via `movementType` discriminator; module terpisah untuk SA/IB/SP). Semua: guard
  `ErpJwtAuthGuard`, state machine §2.7, penomoran via `sys_document_numberings`,
  fiscal period dari tanggal transaksi, posting NO-OP seam (stock balance = derived
  view `inv_stock_balances` dari status POSTED), enrich FK batched tanpa @relation.
- **Frontend**: 1 page per kode transaksi (thin wrapper di atas shared core atau
  standalone); form header 100% dari `useFormFields(transactionCode)`; grid detail
  dari `getGridColumns(transactionCode)`; generic grid engine reuse
  (`grid-line-core`/`use-grid-nav`/`LineCell`). URL sub-route `<base>/new` + `<base>/:id`
  via `TRX_FORM_PAGES` (§2.3.1).

### Keputusan desain per transaksi

**MR/TS/RS/RF** → `inv_stock_movements` shared, `movementType` enum discriminator.
Penomoran per-kode (MR/TS/RS/RF). Source/dest warehouse per-baris (TS/RS) atau
header-only (MR). RF = fuel refill, kolom Harga/Liter tampil default.

**SA (Stock Adjustment)** → `inv_stock_adjustments`. Arah INCREASE/DECREASE per-baris.
**Akun GL server-side:** `inventoryAccountId = line ?? item.inventoryAccountId ??
Setting(inventory.accounts.defaultInventoryAccountId)` — error eksplisit bila tidak
diset. `contraAccountId = line ?? Setting(inventory.accounts.defaultAdjustmentContraAccountId)`.
Frontend grid TIDAK tampilkan kolom akun (diturunkan otomatis).

**IB (Opening Stock)** → `inv_opening_stocks`. Header wajib `currencyId`+`exchangeRate`.
Akun persediaan server-side sama dengan SA. Header warehouse = default untuk baris;
baris boleh override per baris.

**SP (Stock Count)** → `inv_stock_counts`. Qty sistem/fisik/baik/rusak + varianceQty
= fisik − sistem (dihitung server-side). Tanpa akun GL (penyesuaian terpisah via SA).
Model TIDAK punya `postedAt` — POST hanya flip `status`+`postingStatus`.

**PA (Price Adjustment)** → `inv_cost_recalculations`. Bukan dokumen hand-keyed;
ini trigger proses recalc. Create = scope (item?/gudang?, dateRange, costingMethod)
→ status PENDING; kalkulasi async out-of-scope. Tanpa workflow approval.
Frontend: form header-only, create-only (tanpa edit), status badge job.

**RW (Receipt Weigher)** → `inv_weighbridge_tickets`. Header-only (tanpa grid baris).
`netWeight = grossWeight − tareWeight` dihitung server-side, tampil read-only di form.
Workflow §2.7 penuh.

**DC (Daily Check / Time Sheet)** → `inv_daily_checks` + `inv_daily_check_lines` (**tabel
BARU**, migrasi `20260603_003`). Header: branchId/checkDate/machineRef/operatorRef.
Baris: item+qty+unit+gudang. Workflow §2.7 penuh.

### Kode transaksi ↔ route ↔ backend ↔ tabel

| Kode    | Route FE                         | Backend endpoint                | Tabel header                  |
|---------|----------------------------------|---------------------------------|-------------------------------|
| INV.MR  | /warehouse/material-requests     | /erp/inv/stock-movements        | inv_stock_movements           |
| INV.TS  | /warehouse/transfers             | /erp/inv/stock-movements        | inv_stock_movements           |
| INV.RS  | /warehouse/transfer-receipts     | /erp/inv/stock-movements        | inv_stock_movements           |
| INV.RF  | /warehouse/fuel-refills          | /erp/inv/stock-movements        | inv_stock_movements           |
| INV.SA  | /warehouse/stock-adjustments     | /erp/inv/stock-adjustments      | inv_stock_adjustments         |
| INV.IB  | /warehouse/opening-stocks        | /erp/inv/opening-stocks         | inv_opening_stocks            |
| INV.SP  | /warehouse/stock-counts          | /erp/inv/stock-counts           | inv_stock_counts              |
| INV.PA  | /warehouse/price-adjustments     | /erp/inv/price-adjustments      | inv_cost_recalculations       |
| INV.RW  | /warehouse/receipt-weighers      | /erp/inv/weighbridge-tickets    | inv_weighbridge_tickets       |
| INV.DC  | /warehouse/daily-checks          | /erp/inv/daily-checks           | inv_daily_checks              |

### Deploy & verifikasi (2026-06-03)
- Migrasi `20260603_003_erp_inv_daily_checks` → `prisma migrate deploy` (Postgres :3208). 79 migrasi sinkron.
- **Fix gap grid-custom DC:** INV.DC awalnya terdaftar **tanpa** `grid:` family di
  `seed-erp-transaction-grids.ts` → `sys_transaction_grids` 0 baris, sehingga DC selalu
  jatuh ke `defaultInvDailyCheckCols()` dan **tak bisa** dikustomisasi via Kustomisasi Grid
  (beda dengan 8 transaksi lain). Ditambah famili `invDailyCheck` →
  `inv_daily_check_lines` + `INV_DAILY_CHECK_COLUMNS` (mirror default cols). Re-seed →
  INV.DC kini 1 grid + 11 kolom (parity). RW tetap 0/0 (header-only, benar).
- Container `sentient-infra-api-gateway` (`nest --watch`) tidak otomatis recompile modul
  PA/RW/DC yang di-commit belakangan (route 404). Prosedur §2.34: `prisma generate` di
  dalam container (DC model baru) → `docker restart`. Semua 7 route inv kini 401
  (terdaftar + guarded): stock-movements, stock-adjustments, opening-stocks, stock-counts,
  price-adjustments, weighbridge-tickets, daily-checks.

### Follow-up — RESOLVED (2026-06-03)

Keempat follow-up M3 dikerjakan atas permintaan user ("ke-4 follow-up sekarang"):

1. **DC field line mesin/jam** ✅ — `inv_daily_check_lines` dapat kolom `machineRef`
   (TEXT) + `workHours` Decimal(19,4). Migrasi additif `20260603_004`. Distinct dari
   `machineRef`/`operatorRef` yang sudah ada di HEADER. DTO/mapper/FE row model/default
   cols/grid seed sinkron. Commit `c457e073`.

2. **Kolom akun GL override (SA/IB)** ✅ — kolom `inventoryAccountId` (SA: + `contraAccountId`)
   di-set `visible` di Kustomisasi Grid. FE row model bawa field typed (masuk
   `STANDARD_FIELDS`+`LABEL_KEYS`) + serializer kirim/hydrate. Backend sudah resolve
   line→item→Setting; sebelumnya nilai jatuh ke `customFields` & di-drop. Live DB:
   3 kolom di-flip `is_visible=true` via SQL (seed `update:{}` tak meng-update baris
   existing — baseline seed sudah benar untuk env baru). Commit `c457e073`.

3. **PA processor** ✅ — `process()` kini **generate baris server-side** dari moving-average
   (PA form header-only/trigger, tak ada line grid). Hitung `oldUnitCost` (item.averageCost
   ?? standardCost) vs `newUnitCost` (moving-avg) × `affectedQty` (qty on-hand) →
   `deltaAmount` + `totalDelta`, update `item.averageCost`, status `COMPLETED`/`FAILED`.
   Endpoint `POST /erp/inv/price-adjustments/:id/process` + aksi "Proses" (status
   PENDING/FAILED). **Catatan:** `inv_cost_recalculation_lines.warehouseId` NOT NULL →
   PA wajib `warehouseId` di header (PA company-wide → BadRequest jelas). Commit `595b555f`.

4. **GL valuation persediaan** ✅ (gated) — modul `erp-inv-gl`: `InvMovingAverageCostService`
   (moving-average on-the-fly dari POSTED stock-movement lines UNION opening-stock lines,
   sign per `movementType`; REQUEST/TRANSFER internal di-skip; `averageCost` field lama
   tak dipakai/unmaintained) + helpers (`buildLedgerRows` assert debit==kredit,
   `reverseInvLedger`) posting ke **`fin_ledger_entries`** (target nyata, mirror
   `CashBankPostingService` — BUKAN `fin_journal_entries`). Wire ke 3 seam NO-OP:
   - **SA**: Dr/Cr per arah `INCREASE`/`DECREASE` dari akun line.
   - **IB**: N Dr persediaan + 1 Cr ekuitas pembukaan (Setting `defaultOpeningEquityAccountId`).
   - **Movement valued**: `ISSUE` Dr COGS/Cr persediaan; `RETURN` kebalikan; TRANSFER/
     TRANSFER_RECEIPT/REQUEST tanpa GL. Cost = line.unitCost ?? moving-avg ?? item cost.
   - `reverseX` simetris (REOPEN/re-post unwind), assert balance tiap jurnal.

   **AMAN BY DEFAULT:** seluruh posting **gated** di belakang Setting
   `inventory/accounts/glPostingEnabled` (default **false** → seam tetap NO-OP persis
   perilaku lama; status POST tetap flip, 0 baris ledger). Aktifkan via:
   ```sql
   INSERT INTO sys_settings(module,"group",key,value) VALUES
     ('inventory','accounts','glPostingEnabled','true'),
     ('inventory','accounts','defaultOpeningEquityAccountId','<id>'),
     ('inventory','accounts','defaultCogsAccountId','<id>'),
     ('inventory','accounts','defaultInventoryAccountId','<id>');
   ```
   Saat enabled tapi akun kurang → `BadRequestException` jelas (tidak silent).

### Follow-up (sisa, butuh keputusan/di luar inventory)
- **Sales COGS (DO/SI)**: posting COGS sisi penjualan dimiliki modul `erp-sls-*` yang
  sedang dibangun sesi lain (seam `SlsDeliveryReportPostingService` NO-OP; modul SI/DO
  masih stub). Ditunda agar tak bentrok; pola sama (`buildLedgerRows` reusable).
- **IB exchangeRate** di posting GL: header `currencyId`/`exchangeRate` IB sudah dipakai;
  SA/Movement tak punya kolom currency → default `currencyId=1`, `exchangeRate=1`.
- **Snapshot `item.averageCost`**: kini diupdate hanya oleh PA process; movement POST belum
  menyetel ulang average (moving-avg dihitung on-the-fly). Bila perlu snapshot konsisten,
  tambah update saat movement POST.

---

## § Purchasing (M4) — config baseline + forms

**Pola = persis Sales (M5).** Purchasing dibangun meniru Sales Order item-based
(header config-driven Form Builder + grid baris config-driven Kustomisasi Grid +
state machine §2.7 + sub-route URL §2.3.1). Tiap dokumen `pur_*` punya line table
sendiri (per-txn `lineTable` override, bukan satu tabel bersama).

**13 tipe transaksi PUR** (paritas menu M4.TX) terdaftar di `sys_transaction_types`
(`seed-erp-transaction-grids.ts`). Kode stub lama (`PUR.GR`/`PUR.INV`/`PUR.RET`)
**di-prune** → diganti kode kanonik per menu:

| Kode | Dokumen | Grid family | Line table |
| --- | --- | --- | --- |
| `PUR.PR` | Purchase Requisition | `purchaseItem` | `pur_requisition_lines` |
| `PUR.RFQ` | Request for Quotation | `purchaseRfq` | `pur_rfq_suppliers` (baris = supplier diundang) |
| `PUR.BS` | Bid Comparison | `purchaseBid` | `pur_bid_selection_lines` |
| `PUR.PO` | Purchase Order | `purchaseItem` | `pur_order_lines` |
| `PUR.GRN` | Goods Receipt | `purchaseReceipt` | `pur_goods_receipt_lines` (+ QC: accepted/rejected/quarantine) |
| `PUR.PI` | Purchase Invoice | `purchaseItem` | `pur_invoice_lines` |
| `PUR.DNR` | Return Shipment | `purchaseItem` | `pur_return_lines` (returnType=DEBIT_NOTE) |
| `PUR.PRT` | Purchase Return | `purchaseItem` | `pur_return_lines` (returnType=RETURN_TO_VENDOR) |
| `PUR.AP` · `PUR.PP` · `PUR.VPP` · `PUR.VP` · `PUR.OB` | Vendor Advance / Freight Payable / Payment Schedule / Vendor Payment / Opening AP | — (reuse finance domain) | — (katalog + header saja) |

8 dokumen item-based dapat grid kolom default; 5 dokumen pembayaran/saldo-awal
(`AP/PP/VPP/VP/OB` reuse `fin_ap_payments`/`fin_settlement_allocations`) = katalog +
header form saja (grid menyusul saat desain reuse finance difinalisasi). Header field
default per kode di `seed-erp-purchasing-forms.ts` (`seedPurchasingForms`): supplier
**required** untuk PO/GRN/PI/DNR/PRT, **optional** untuk PR/RFQ/BS (pre-sourcing).

**Kontrak dataField/fieldKey (jangan rename).** Grid `purchaseItem`: `rowNo`,`itemId`,
`quantity`,`unitId`,`unitPrice`,`discountPercent`,`discountAmount`(hidden),`tax1Id`,
`lineTotal`(skip/derived),`warehouseId`(hidden),`notes`,dims(hidden). Grid
`purchaseReceipt` tambah `acceptedQty`/`rejectedQty`(hidden)/`quarantineQty`(hidden)/
`unitCost`(hidden). Header item-doc: `supplierId`/`description`/`referenceNo` (LEFT) ·
`branchId`/`locationId`/`warehouseId`/`payableAccountId` (CENTER) · `docDate`(@today)/
`docNumber`/`currencyId`(default 1)/`paymentTermId`/`dueDate` (RIGHT).

**Lookup registry** sudah punya `items`/`units`/`taxes`/`payment-terms` (ditambah saat
Sales). Reuse — jangan bikin slug baru.

**Form kerja penuh pertama = Purchase Order** (`/purchasing/purchase-orders`, code
`PUR.PO`) — mirror SO 1:1. Backend `erp-pur-orders` (CRUD + numbering `PO` + fiscal
period dari docDate + totals server-side + enrich cross-domain + workflow). **PO TIDAK
posting GL** (dokumen komitmen; GRN posting inventory+GR/IR, PI posting AP) — posting
service = no-op terdokumentasi. E2E verified: create→submit→approve→post (POSTED, 0
ledger entries; subtotal/grandTotal benar). FE: `lib/api/pur-orders`,
`purchase-transaction-form` (shared) + `pur-order-form`, `pur-item-lines`,
`pur-structural-field`, `pur-orders-page` + filters; route di `TRX_FORM_PAGES` +
`ERP_ROUTE_META`. **Beda dari SO:** `pur_orders` **tidak punya kolom `code`** (SO punya);
`customerId`→`supplierId`, `receivableAccountId`→`payableAccountId`, `salesDeptId` di-drop.
Reuse `cashBankWorkflowActions` + grid engine generik (sama spt SO).

Semua 13 transaksi Purchasing selesai. Replikasi selesai (PR/PI/GRN/DNR/PRT/RFQ/BS).
Payment docs (AP/PP/VPP/VP/OB): reuse pur_invoices / fin_ap_payments — lihat §§ payment docs di bawah.

**Follow-up (sama persis SO):** REOPEN dari POSTED → 400 (gap warisan); kolom custom
baris belum persist (`pur_*_lines` tak punya `custom_fields` JSONB).

**Payment docs (AP/PP/VPP/VP/OB)**: reuse Finance domain + `fin_ap_payments` +4 kolom
(migrasi `20260603_001`): `fx_gain_loss_amount/account_id` + `term_discount_amount/account_id`.
AP/PP/OB = list pur_invoices dari sisi pembelian; VPP/VP = fin_ap_payments DRAFT/ALL.
Form semua coming-soon — menunggu integrasi Finance AP payment form ke purchasing UI.
Routes: /purchasing/vendor-advances · /purchasing/freight-payables ·
/purchasing/payment-schedules · /purchasing/vendor-payments · /purchasing/opening-ap-balance.


---

## § Warehouse (M3) Reports — 23 laporan + export server-side (2026-06-03)

**Konteks:** menu REPORTS (`/warehouse/reports/*`, 23 path seeded) sebelumnya
render blank (ComingSoon — tak ada komponen terdaftar). Dibuat full: view +
export ke Excel/PDF/Word. Pilihan user: **semua report (A+B) sekaligus**,
**export server-side** (api-gateway).

**Pola = framework laporan uniform (1 kontrak).** Tiap report = `ReportDef`
(`{ key, title, group, columns, resolve(filters) }`) → `ReportDataset`
(`{ columns, rows, summary, total, generatedAt }`) yang **sama** dipakai view
JSON dan ketiga exporter, jadi tampilan & file selalu konsisten. Tambah report =
tambah satu `ReportDef`; tak ada plumbing per-report. Backend: `apps/api-gateway/
src/erp-inv-reports/` (registry `inv-reports.service.ts` compose `buildTxnReports`
+ `buildStockReports`). Endpoint (guard `ErpJwtAuthGuard`):
- `GET /erp/inv/reports` → katalog (key/title/group).
- `GET /erp/inv/reports/:key` → ReportDataset JSON (tabel layar).
- `GET /erp/inv/reports/:key/export?format=xlsx|pdf|docx&<filters>` → unduh file.
Filter query: dateFrom/dateTo/asOfDate/warehouseId/itemId/status/search/page/limit.

**23 report:**
- **11 transaksi** (`group:'transaction'`): MR/TS/RS/RF/Return (ErpInvStockMovement
  per `movementType`), SP/SA/PA/IB/DC/RW (header-level per modul). Filter status +
  tanggal + gudang + search.
- **4 item** (`group:'item'`): batch-items/batch-cards (ErpInvLot), serial-items/
  serial-cards (ErpInvSerial). (Lot tak punya qty on-hand per-lot → kolom qty 0;
  follow-up.)
- **8 agregasi stok** (`group:'stock'`, reuse `InvMovingAverageCostService` dari
  `erp-inv-gl` — saldo DERIVED dari POSTED movements ∪ opening, tak ada tabel
  stock-ledger): stock (saldo+nilai), stock-cards (kartu stok running balance,
  butuh itemId), stock-mutations (opening/in/out/closing), below-minimum
  (`md_items.minStock`), daily-stock (saldo harian), cogs-balance (cost recalc),
  stock-minus (saldo negatif), consignment (**kosong + note** — belum ada model
  konsinyasi; hanya `md_items.consignmentAccountId`).

**Export server-side** (`report-export.service.ts` + per-format): **exceljs**
(xlsx), **pdfkit** (pdf — font Helvetica built-in, tanpa Chromium; pdfmake
**tidak** dipakai, di-skip karena setup font 0.3.x ribet — pdfkit sudah jadi dep
& dipakai modul `erp-fin-reports`), **docx** (word). Format sel per `column.type`
(money/qty/number/percent/date/status) identik view & file. Filename
`${key}-YYYYMMDD.ext`.

**Frontend:** satu `InvReportPage` generik (`components/pages/inv-report-page.tsx`)
+ `ReportToolbar` (filter + tombol Excel/PDF/Word) + `ReportTable`, reuse
`ErpListLayout`/`Table`/format helpers + `lib/api/client.downloadFile` (cookie
`erp_token` + nama file dari `Content-Disposition`). Routing: `renderRoute`
dispatch `/warehouse/reports/:key` → `InvReportPage`; opsi per-key di
`lib/inv-report-options.ts` (status filter utk transaksi, item picker utk
stock-cards, as-of utk stock).

**Bug pre-eksis ditemukan & diperbaiki:** `inv_weighbridge_tickets.posted_at`
ada di schema (sejak build RW) tapi tak pernah dimigrasi → semua Prisma read RW
500 (P2022), termasuk list RW. Migrasi `20260603_005` menambah kolomnya.

**Verifikasi E2E:** login admin → katalog 23 report → 23/23 data http 200 →
export xlsx/pdf/docx magic bytes valid (PK/%PDF/PK). Typecheck api-gateway 0
error; file FE report clean.

**Follow-up:** (1) consignment butuh model transaksi konsinyasi. (2) batch/lot
on-hand qty per gudang (skema lot tak simpan qty). (3) Statistics group
(`/warehouse/stats/*`, 6 dashboard) masih ComingSoon — di luar scope ini.
(4) Finance reports (`/finance/*`: cash-flow, AR/AP card/aging, giro-maturity,
budget-realization) dibangun **paralel sesi lain** (modul `erp-fin-reports`,
pola berbeda) — jangan dobel.

---

### Initial Setup (M0.CFG) — 4 halaman kaya menggantikan settings generik (2026-06-03)

Sebelumnya 15 menu **INITIAL SETUP** (`M0.CFG`) ter-wire tapi 12 di antaranya
cuma editor key-value generik (`SettingsGroupPage`) dan **Import Data**
frontend-only (komentar di file: "backend import endpoint not yet implemented").
Atas keputusan user (kedalaman = "stub + upgrade halaman yang seharusnya kaya",
import = "bikin importer nyata"), 4 halaman di-upgrade jadi purpose-built;
sisanya (company/accounting/tax/description/format/defaults/report-defaults/
signature/options) **tetap** `SettingsGroupPage` (cukup sebagai key-value).

**Tabel baru (4, domain `sys`)** — migrasi `20260603_006_erp_initial_setup_pages`
(additive, 0 DROP; `migrate deploy` + `prisma generate` di container
`sentient-infra-api-gateway` + restart; Postgres :3208):
- `sys_bank_accounts` (`ErpBankAccount`) — rekening bank **perusahaan** (legacy
  0-31). Beda dari `md_partner_bank_accounts` (rekening partner). currency/GL =
  scalar BigInt FK + `@@index` tanpa `@relation` (domain decoupled).
- `sys_approval_rules` (`ErpApprovalRule`) — aturan persetujuan per jenis
  dokumen (legacy 0-46). Multi-level = beberapa baris per `documentType`
  (`@@unique([documentType, level])`); `minAmount` threshold; `approverRoleId`.
- `sys_home_widgets` (`ErpHomeWidget`) — konfigurasi widget beranda (legacy
  0-39): `widgetKey` unik, `enabled`, `sortOrder`, `colSpan` (1–4), `config` Json.
- `sys_import_jobs` (`ErpImportJob`) — riwayat impor (legacy 0-20): entity,
  fileName, status, rowsTotal/Ok/Failed, errors Json.

**Backend** (4 modul, pola `erp-currencies` 1:1, guard `ErpJwtAuthGuard`,
soft-delete, server-driven query): `erp-bank-accounts` (`/api/erp/bank-accounts`),
`erp-approval-rules` (`/api/erp/approval-rules`), `erp-home-widgets`
(`/api/erp/home-widgets`, bulk status toggle `enabled`), `erp-import`
(`/api/erp/import/:entity` upload via `FileInterceptor`@platform-express +
`/entities` + `/template/:entity` xlsx + `/jobs`). Import = registry adapter
per-entity di `erp-import.adapters.ts` (9 entitas: units, currencies,
item-categories, taxes, payment-terms, branches, partners, accounts, warehouses
— warehouse pakai header `locationCode`→`locationId`). Parse xlsx via `exceljs`
(`wb.xlsx.load(buffer)`) + CSV line-split; validasi per-baris try/catch
(duplikat/FK gagal = baris failed, batch jalan terus); job dicatat ke
`sys_import_jobs`. Verifikasi: 4 endpoint balas **401** (mapped + guarded).

**Frontend**: Bank Accounts / Approval / Home Layout reuse `SimpleMasterPage`
(Approval & Home meng-alias `code`/`name`/`isActive` di API client karena
field DB beda — `documentType`, `widgetKey`/`title`/`enabled`). Import =
rewrite `import-page.tsx` (fetch entities, unduh template, upload, ringkasan
hasil + tabel error baris, riwayat di `import-history.tsx`). Repoint 3 entri
`ERP_PAGES` (`shell-route-renderer.tsx`) dari `SettingsGroupPage` →
halaman baru; `/admin/import` sudah ke `ErpImportPage`. Tambah 4 entri
`ERP_ROUTE_META`. Menu `sys_menus` sudah ter-seed sebelumnya (tak diubah).

**Catatan ops:** dibangun **paralel sesi lain** yang sedang menggarap
Manufacturing Work Orders (commit `71201684`, edit `shell-trx-pages.ts`,
`erp-route-meta.ts §Produksi`, `MASTER-DATA-REPORT.md`) — file itu **bukan**
bagian build ini & sengaja tidak disentuh.

**Follow-up (tak di scope):** (1) seed default home widgets/approval rules
(halaman fungsional dalam keadaan kosong — user isi sendiri). (2) Approval
rules belum dipakai engine workflow transaksi (baru CRUD konfigurasi).
(3) Bank Accounts belum dipakai sebagai sumber kas/bank di form transaksi.

---

## Data Register Pages (2026-06-06)

**Keputusan:** Membangun semua 46 legacy DATA/STATS menu paths yang sebelumnya menampilkan `ComingSoon`.

### Register system

Dibuat `lib/registers/` — config-driven `DocumentRegisterPage` organism yang reusable: setiap dokumen didefinisikan sebagai `DocumentRegisterConfig<Row>` (list fn, kolom, editBase, status options). Renderer `shell-route-renderer.tsx` lookup REGISTER_CONFIGS sebelum TRX dispatch. Route meta di-merge otomatis dari register configs ke `ERP_ROUTE_META`. Register = read-only (tidak ada create/delete di halaman data).

**35 Data registers** (inv 10 + pur 10 + sls 15): pakai endpoint list TX yang sudah ada.

**Opening AP Balance:** filter `isOpeningBalance: true` ditambahkan ke `QueryPurInvoicesDto` + `buildPurInvoiceWhere`. Field sudah ada di `pur_invoices`.

### Warehouse Statistics (6 halaman)

Backend: modul baru `erp-inv-stats` — 6 GET-only endpoints (`/erp/inv/stats/*`): top-revenue, best-selling, most-profitable (COGS dari `sls_invoice_lines.unit_cost`), below-minimum (moving-average on-hand vs `md_items.min_stock`), approvals (NEED_APPROVE count per inv doc type), kpi. Tidak ada migrasi. FE: `lib/api/inv-stats.ts` + 6 halaman + `StatPageShell` organism.

### Group B — 4 dokumen baru tanpa tabel baru (2026-06-06)

**Keputusan (user, 2026-06-06):** vendor-advances (AP), freight-payables (PP), payment-schedules (VPP), ar-collections (IC) REUSE `fin_ap_payments` / `fin_ar_receipts` dengan discriminator `source` field — tidak membuat tabel baru. Schema comment di `fin_ap_payments` sudah mencatat reuse ini untuk VP/VPP.

- AP, PP, VPP → `fin_ap_payments` (source='AP'/'PP'/'VPP')
- IC → `fin_ar_receipts` (source='IC')

Modules: `erp-pur-vendor-advances`, `erp-pur-freight-payables`, `erp-pur-payment-schedules`, `erp-sls-ar-collections`. Tiap modul: full CRUD + list (filter by source) + workflow DRAFT→NEED_APPROVE→APPROVED→POSTED + auto-number dari `sys_document_numberings`. GL posting = UNPOSTED + TODO (post-MVP).

`source`/`partner`/`date` filter ditambahkan ke `QueryApPaymentDto` + `QueryArReceiptDto` (source, partnerId, dateFrom, dateTo). `sortBy`/`sortDir` ditambahkan ke `QueryArReceiptDto`.

4 docNumber codes seeded: `AP`/`PP`/`VPP`/`IC` (via direct SQL karena `seed-erp.ts` punya pre-existing TS error `bulkUpsertMenuItems` undefined yang mencegah `ts-node seed-erp.ts`).

**Follow-up (post-MVP):** GL posting untuk AP/PP/VPP/IC; line detail (invoice allocation) untuk VP/VPP/IC.

### Report Designer — UX 3-dock + drag-to-bind + undo/redo (2026-06-07)

Designer (`/admin/report-designer`) dirombak dari model **tab mutually-exclusive**
(`activePanel` = dataSources|bands|preview, hanya satu kelihatan) ke **3 dock
simultan + collapsible**:

- **Dock kiri** (380px, toggle): tab **Data Sources** (SQL editor) / **Fields**
  (palette kolom hasil query). Test Query mem-publish kolom via `onSchema(alias,
  columns)` → di-hold di state page `schemas` → tab Fields render chip kolom.
- **Center**: canvas selalu tampil; **Preview** kini split di kanan canvas
  (toggle, bukan menelan canvas).
- **Dock kanan** (260px, toggle): Properties.

Fitur baru:
- **Drag-to-bind**: field dari palette di-drag (HTML5 DnD, MIME
  `application/x-rpt-field`) ke band → auto-buat text component `{kolom}` di posisi
  cursor. Klik field = sisip ke band terpilih. Factory di
  `lib/report-component-factory.ts` (`makeBoundText`, `resolveTargetBand`,
  `MM_TO_PX` — sumber tunggal skala mm→px, dipakai canvas+overlay+preview).
- **Toolbar komponen terpusat** di atas canvas (Text/Garis/Gambar) — gantikan
  chip `T/—/IMG` per-gutter band. Gutter band kini hanya identitas + reorder/hapus.
- **Resize handle** 8-arah pada komponen terpilih (line = 2 handle horizontal);
  drag pakai update `transient` (tak menambah history per-frame).
- **Undo/redo**: `past`/`future` di `DesignerState` (limit 50), tombol toolbar +
  Ctrl+Z / Ctrl+Shift+Z / Ctrl+Y; Ctrl+S simpan. Operasi transient (drag/resize)
  snapshot sekali via `PUSH_HISTORY` saat mousedown.

File: `report-store.ts` (history `commit()`), `report-types.ts` (state baru),
organism `report-designer/` dipecah: `designer-canvas` (shell+toolbar),
`band-row`, `component-overlay`, `component-toolbar`, `field-palette`. Semua < 400
baris. Catatan: HTML5 DnD dipakai di sini (palette→canvas freeform) — di luar
larangan §2.14 yang khusus tab-strip/sortable list.

### Report Designer — iterasi 2: Properties tab, expression pintar, multi-select, snap/align (2026-06-07)

Lanjutan dari rombak 3-dock. Empat penambahan:

- **Properties bertab** (`PropTab` = layout/style/data): `properties-panel` jadi
  shell tab + sub-editor per tipe di `properties/` (`band/text/line/image-
  properties`, `controls`, `layout-fields`). Editor **Image** baru (src+fit).
- **ExpressionEditor** (`properties/expression-editor.tsx`): autocomplete kolom
  saat ketik `{`, picker Field / Agregat (`{{SUM(col)}}`…) + token PageNumber/
  TotalPageCount. Kolom = gabungan unik semua `schemas` hasil Test Query, dialir
  ke Properties via prop `columns`.
- **Multi-select + clipboard**: `DesignerSelection.componentIds` (dalam satu
  band). Shift/Ctrl-click = `TOGGLE_COMPONENT`; aksi batch via `PATCH_COMPONENTS`
  (group move = 1 undo step), `REMOVE_SELECTED`, `ADD_COMPONENTS`,
  `SELECT_COMPONENTS`. Keyboard di hook `lib/use-designer-shortcuts.ts`:
  Ctrl+C/V/D (clone via `cloneComponents`, offset 3mm), Del/Backspace,
  Ctrl+Z/Shift+Z/Y, Ctrl+S — di-skip saat fokus input/textarea.
- **Snap + align**: drag tunggal snap ke tepi/tengah komponen lain & batas band
  (`lib/report-snap.ts`, threshold 1.2mm) + garis bantu accent; group drag bebas.
  `AlignToolbar` (muncul di toolbar canvas saat ≥2 terpilih): align L/C/R · T/M/B,
  sebar H/V, samakan lebar/tinggi (`lib/report-align.ts`).

Resize handle = hanya saat seleksi tunggal (`resizable`). Semua file < 400 baris;
geometri `LayoutFields` pakai `GeometryPatch` agar editor per-tipe assignable.

### Report Designer — iterasi 3: reskin penuh ke model mock prototype (2026-06-07)

Atas permintaan user (UI/UX ikut screenshot prototype `report-designer.jsx`,
pilihan "reskin penuh + adopsi model mock" + 3 mode), **editor** designer
(`/admin/report-designer`) di-rombak ulang ke band-based canvas gaya
Stimulsoft + tag binding gaya Carbone `{d.x:formatter}`. **List page tetap
backend-connected** (`report-designer-list-page.tsx`); hanya editornya yang
diganti.

- **Model = mock** (`lib/report-designer-mock.ts`): `RD_DATA` sample (company/
  doc/items/totals), `rdResolve()` (resolver `{d.x:money|num}` + `{i.y}`),
  `rdInitialBands()` (Faktur Penjualan: ReportTitle/PageHeader/Data/ReportFooter/
  PageFooter), `RD_TOOLBOX`, `RD_DICT` (dictionary tree Carbone), `buildTemplate()`.
  Store SQL/undo-redo lama (reducer `report-store.ts`) **tidak dipakai** editor ini.
- **3 mode** (segmented control header): Desain (canvas band + ruler), Pratinjau
  (`RdPreview` dokumen terisi), Template (kode Carbone-ish read-only).
- **Layout**: header (judul + SRX + nama template + mode + Import/Jalankan/Export/
  Simpan) · **ribbon** (Font/Align/Bands/Insert/Page/Zoom) · body 3-kolom
  (Komponen+Sumber Data | canvas | Properti+Struktur) · footer pintasan.
- **Organisms** `components/organisms/report-designer-mock/`: `mock-designer`
  (orchestrator + state + keyboard Del/Esc/⌘P), `ribbon`, `left-panel`,
  `canvas`, `preview`, `right-panel`, `shared`. Semua < 400 baris.
- **CSS** `styles/report-designer.css` (di-import via `erp-components.css`),
  pakai token (`--panel`/`--bg`/`--border`/`--primary`/`--fg-muted`…). Kelas
  preview di-namespace `.rdv-*` untuk hindari bentrok `.rdp-*` react-day-picker.
- **Catatan**: organism SQL-designer lama (`report-designer/designer-*`,
  `datasource-panel`, `field-palette`, `properties*`, `preview-panel`,
  `band-row`, `component-*`, `align-toolbar`) + `lib/report-store.ts` +
  `lib/use-designer-shortcuts.ts` + `lib/report-align.ts` +
  `lib/report-component-factory.ts` jadi **orphan** (tak di-import page).
  `report-types.ts` & `report-template-dialog.tsx` **tetap** dipakai list page.
  Penghapusan file orphan ditunda (butuh konfirmasi user) — build hijau tanpanya.

## Data dummy transaksi finance — wajib POSTED + "terima dari" terisi (2026-06-07)

Keputusan (dengan user): semua data transaksi DUMMY_SEED di menu TRANSAKSI
finance harus **terposting** dan punya **"terima dari" (`partner_id`)**.

- Partner diisi **acak per arah**: RECEIPT / giro INCOMING → partner `is_customer`;
  DISBURSEMENT / giro OUTGOING → partner `is_supplier`; journal (tak berarah) →
  acak dari semua partner.
- Posting lewat **API state machine** (`/transition` SUBMIT→APPROVE→POST), bukan
  flip kolom langsung, supaya `fin_ledger_entries` (GL) ikut ter-generate dgn
  partner. Untuk doc yang sudah posted, `partner_id` dipropagasi ke ledger existing.
- Script repair: `scripts/fix-transactions-posted-and-partner.mjs` (idempotent,
  hanya menyentuh `source LIKE 'DUMMY%'`; giro tanpa kolom source = semua seed).
  Punya retry backoff utk throttle 429.
- Hasil: cash/bank 2219, giro 307, journal 636 → semua POSTED. Tanpa "terima dari"
  tersisa hanya OPENING_BALANCE (1, memang tanpa lawan transaksi) + 1 journal
  manual non-dummy. GL ledger 4693 → 6121.

## Receipt Memo / Send Memo = AR Receipt / AP Payment (2026-06-07)

Menu TRANSAKSI **Receipt Memo** → route `/finance/receipt-memos` → `ErpArReceiptsPage`
→ tabel **`fin_ar_receipts`** (endpoint `/fin/ar-receipts`). **Send Memo** →
`/finance/send-memos` → `ErpApPaymentsPage` → **`fin_ap_payments`**. (Komentar di
`scripts/seed-bank-out-1000.mjs` yg menyebut memo = cash-bank-txn sudah usang.)

AR Receipt = **skeleton CRUD**: `create` insert apa adanya, **tanpa** auto-number,
**tanpa** GL ledger, **tanpa** state machine `/transition`. Jadi "terposting" =
set `status='POSTED'`+`posting_status='POSTED'` langsung di payload/insert; `partner_id`
(customer) wajib = "terima dari".

Seed 2026-06-07: 1000 Receipt Memo POSTED, partner customer acak, tersebar merata
per bulan Jan 2025→Jun 2026 (56×10 + 55×8), doc `RM0000001..RM0001000`,
`source='DUMMY_SEED_RECEIPT_MEMO'`, bank acak 185–189, currency IDR. Insert via SQL
langsung (setara API krn create tak punya side-effect GL). Hapus: delete where
source='DUMMY_SEED_RECEIPT_MEMO'.

---

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

## Report Studio — desainer laporan band-based (2026-06-17)

Route `/admin/report-designer` (live `erp.fr-labs.my.id/app/admin/report-designer`)
sekarang menampilkan **Report Studio** — port React **fungsional penuh** dari
mockup `apps/web-erp/Report-Designer-ERP-Profesional/ReportStudio.dc.html`
(export dc-runtime). Menggantikan `MockReportDesigner` lama (whole-route,
keputusan user 2026-06-17). UI gaya ribbon MS-Office: quickbar (undo/redo, nama
laporan, pilih template, lang/tema, Ekspor) + ribbon Home/Page/Layout/View +
doc-tabs (Design/Preview) + data rail (Data/Relasi/Parameter/Fungsi, drag field
ke kanvas) + kanvas band (drag/move/resize elemen, ruler/grid/guides) + panel
kanan (Properti/Pohon/Kamus) + status bar. Fungsi nyata: undo/redo, clipboard,
group-by, 5 template (invoice/sales/purchasing/finance/customers) + data dummy,
ekspor PDF/Print (iframe), Excel/Word/HTML (Blob download), live preview
berpaginasi, evaluator ekspresi (Sum/Avg/Count/Max/Min/Today/PageNumber/…).

Keputusan & catatan:
- **Pixel-faithful (pilihan user)**: styling memakai inline-style verbatim dari
  mockup sebagai **"designer surface"** — **pengecualian sadar** atas §2 (token-
  only). Tema (light/dark) + warna aksen tetap lewat CSS var palette (`--accent`
  /`--panel`/`--border`/…) yang di-set di root via `rootStyle`. Helper `s()`
  (`lib/report-studio/css.ts`) mem-parse string CSS → objek style React;
  hover/focus lewat komponen `Hov` (`rs-shared.tsx`).
- **Struktur** (semua ≤400 baris, §3): logika murni di
  `lib/report-studio/*` (types, constants, i18n, format, templates, data,
  el-style, palette, pagination, export, css). Controller =
  hook `useReportStudio` (single merged-state + refs untuk scratch non-render;
  `stRef` disinkron via effect, uid = counter modul — hindari akses ref saat
  render / aturan `react-hooks/refs`). Port `renderVals()` → `vals/*` builders
  (`buildVals`). UI = organisms `components/organisms/report-studio/*` + page
  `components/pages/report-studio-page.tsx` (daftar di `ERP_PAGES`
  `shell-route-renderer.tsx`).
- **Template wired ke backend (2026-06-17)**: dropdown template = **list asli**
  dari `lib/api/reports` (`listReportTemplates`, filter `isActive`). Pilih →
  `getReportTemplate` → `templateJson` di-load via
  `lib/report-studio/template-io.ts` `reportFromTemplateJson()`: kalau JSON
  **native RsReport** (discriminator: tiap band punya `els[]` + `type`) dipakai
  apa adanya; kalau **legacy/empty** (template lama pakai skema band beda, tanpa
  `els`) → buka **starter layout per-module** (SALES/PUR/FIN/INV→builtin).
  Tombol **Simpan** (quickbar) → `updateReportTemplate(currentId,{templateJson:
  report, name})` — RsReport JSON-serializable, round-trip. Offline/API gagal →
  fallback builtin 5 template (`isBuiltinKey`).
- **Rows preview wired ke SQL nyata (2026-06-17)**: `RsReport.sql` opsional
  (persist di templateJson). Editor SQL di Data tab (textarea, debounce 500ms) →
  `executeSqlQuery(sql,{},200)` → `state.sqlRows/sqlCols/sqlErr`. Saat SQL aktif:
  `effectiveData()` pakai rows nyata (bukan `buildData`), **dictionary** (Data
  tab + Kamus) menampilkan **kolom hasil query** (datasource "Query") sebagai
  field draggable, bind = nama kolom (`resolveField(bind,row)`=`row[bind]` sudah
  generik); `bindOptions` property-grid + status bar (`Query · SQL · N baris`)
  ikut. SQL kosong → balik ke data contoh. Keyboard handler skip `<textarea>`
  jadi ngetik SQL aman.
- **Cleanup (2026-06-17)**: cluster lama **DIHAPUS** (superseded, self-contained,
  tak direferensikan): `report-designer-page.tsx`, `report-designer-list-page.tsx`,
  `organisms/report-designer/*`, `organisms/report-designer-mock/*`,
  `lib/report-designer-mock.ts`, `lib/report-api.ts`. `lib/api/reports.ts` tetap
  (dipakai Report Studio). Route path `/admin/report-designer` + `erp-route-meta`
  tetap (canonical `sys_menus.path`).
- **Integrasi chrome ke template ERP (2026-06-22, keputusan user)**: scope =
  *chrome integration* (kanvas/designer surface **tetap** pixel-faithful, §2
  exception tak berubah). Dua keluhan yang disasar: **double header** & **alur
  simpan/load**.
  - **Quickbar tidak lagi "header gelap kedua"**: `rs-quickbar.tsx` diubah dari
    titlebar gelap (`--titlebar:#11161f`, font IBM Plex, wordmark "R ReportStudio"
    + label `.rdl — Designer`) → **toolbar terang ber-token ERP** (`--panel`/
    `--border`/`--radius`, tinggi 44px = `--topbar-h`). Wordmark + `.rdl`
    **dihapus** (redundan dgn breadcrumb ERP "Report Designer"). Doctabs + ribbon
    di bawahnya sudah terang → seluruh area atas jadi kohesif dgn shell ERP.
  - **Font chrome native ERP**: `rootStyle` (`palette.ts`) `font-family` →
    `var(--font-sans, …)` (Geist saat tersedia, fallback IBM Plex/system). `@import`
    Google Fonts dipangkas ke **IBM Plex Mono saja** (masih dipakai status bar +
    label ekstensi ekspor).
  - **Alur simpan/load lebih jelas**: dropdown template diberi label eksplisit
    **"Template"** (terbaca sbg buka/ganti template); tombol **Simpan = primary
    (accent)** — aksi harian; **Ekspor = secondary outline** (sebelumnya kebalik:
    Ekspor accent, Simpan ghost samar). Undo/redo pakai gaya ikon terang
    (`qIconLight` di `vals/styles.ts`), disabled → muted + `not-allowed`.
  - **Shortcut Ctrl/Cmd+S → simpan** ditambah di keyboard handler `useReportStudio`
    (di-handle sebelum guard input, jadi tetap jalan saat fokus di field nama).
  - Tak ada perubahan fungsi designer/kanvas; semua file ≤400 baris, typecheck OK.
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

## Lampiran Transaksi — per id transaksi, 4 domain (2026-06-13)

Lanjutan dari Item Lampiran: lampiran ke **transaksi** Finance / Warehouse
(Inventory) / Purchasing / Sales. Keputusan user (2026-06-13): **arsitektur
per-domain** (4 tabel generik, bukan 1 tabel global `sys_attachments`; bukan
juga per-jenis-transaksi ~40 tabel), **cakupan semua jenis transaksi 4 domain**.
Frasa user "**per id transaksi**" → tabel berkunci `(doc_type, doc_id)`.

**DB & backend (api-gateway):**
- 4 tabel generik (migrasi `20260613_002_erp_transaction_attachments`):
  `fin_transaction_attachments`, `inv_transaction_attachments`,
  `pur_transaction_attachments`, `sls_transaction_attachments`
  (`ErpFin/Inv/Pur/SlsTransactionAttachment`). Kolom: `doc_type`/`doc_id`
  (polymorphic lintas tabel transaksi domain — **tanpa FK**, index
  `(doc_type, doc_id)`), `file_name`/`stored_name`(unique)/`mime_type`/
  `size_bytes`/`note?`/`sort_order`/`created_at`/`created_by_id?`.
- **Satu** module `erp-attachments` (1 service + 1 controller) melayani keempat
  domain. Controller `@Controller('erp/:domain/attachments')` ber-guard
  `ErpJwtAuthGuard`; service `delegate(domain)` memilih tabel Prisma yang tepat
  (fin/inv/pur/sls). Endpoint per record:
  `GET/POST /erp/:domain/attachments/:docType/:docId` (list/upload) ·
  `PATCH/DELETE …/:attachmentId` (note/hapus) · `GET …/:attachmentId/file`
  (stream `inline`). Whitelist mime (PDF/gambar/Office/CSV/teks/ZIP) max
  **30×10MB**; `storedName` = `<domain>-<docType>-<docId>-<uuid>.<ext>`.
- File di `uploads/erp-transactions/` (terpisah dari `uploads/erp-items/`;
  env `ERP_TXN_UPLOAD_DIR`). Verifikasi live: keempat domain 401 saat
  unauthenticated (route ter-register, guard aktif).

**Frontend (web-erp):**
- API generik `lib/api/transaction-attachments.ts` (`TransactionAttachment` +
  list/upload/updateNote/delete + `transactionAttachmentFileUrl`),
  parameter `(domain, docType, docId)`.
- Organism generik [`transaction-attachment-upload.tsx`](components/organisms/transaction-attachment-upload.tsx)
  — props `{ domain, docType, docId }`; UI identik item-attachment (dropzone +
  catatan editable + buka/unduh/hapus), empty state bila `docId` null.
- **Tab "Lampiran"** dipasang di **shared transaction form** (sekali per form,
  meng-cover semua jenis dokumen yang lewat form itu): `domain` hardcoded per
  form, `docType = transactionCode`, `docId = data.id ?? null`. Form ter-wire:
  - **fin:** `cash-bank-transaction-form` (CR/CD/BD), `journal-transaction-form`
    (GJ/AJ/JM/BB/RV), `giro-transaction-form` (RG/SG/RGC/SGC).
  - **sls:** `sales-transaction-form` (semua 16 tipe SLS.*).
  - **pur:** `purchase-transaction-form` (PR/PO/RFQ/GRN/PI/retur).
  - **inv:** `inv-stock-adjustment/stock-count/stock-movement/opening-stock/daily-check-form`.

**Sisa (follow-up):** form transaksi yang TIDAK lewat shared form di atas (mis.
`inv-price-adjustment`, dokumen pur/sls dengan form bespoke, pembayaran
AP/AR) — tinggal pasang `<TransactionAttachmentUpload domain docType docId>`
dengan resep yang sama. Backend keempat domain sudah siap menerima docType
apa pun. **Impor file bulk CSV/XLSX tetap fase berikutnya.**

## § Desainer Laporan — drag-and-drop + edit konten report asli (2026-06-13)

Editor `/admin/report-designer` (organism `report-designer-mock/`, kode "SRX")
diberi dua kemampuan:

**1. Drag-and-drop di kanvas (mode Desain).** Komponen (text/field/line/columns/
datarow/totalrow) bisa di-drag untuk pindah posisi, ter-clamp di dalam band-nya
(tak bisa keluar area band). Komponen text/line/field punya **resize handle** di
tepi kanan saat terpilih untuk ubah lebar. Implementasi:
- Hook `use-rd-drag.ts` (`useRdDrag`) — pointer-drag berbasis `window`
  listener + `setPointerCapture`-style tracking, konversi px layar → % band
  (untuk x/w) & px unzoomed (untuk y), clamp ke bounds band. Toggle class
  `body.rd-dragging` (cursor grabbing global).
- `canvas.tsx` `onPointerDown` per komponen → `moveComp(bandId, compId, patch)`
  di `mock-designer.tsx`. CSS `.rd-draggable`/`.rd-resize-handle` di
  `styles/report-designer.css`.

**2. Template report-engine kini EDITABLE (bukan lagi fallback mock).** Sebelumnya
template seed (Buku Besar/Neraca Saldo/Neraca) berformat report-engine
(`dataSources` + SQL + `bands[].components`, geometri mm) ditolak `parseTemplateJson`
→ editor menampilkan mock "FAKTUR PENJUALAN" generik, bukan konten report asli.
Sekarang **adapter dua-arah** `lib/report-engine-adapter.ts`:
- `reToBands(json)` — report-engine → model editor (`comps`, x/w %, y px).
  Konversi geometri pakai `contentWidth` (dari pageSize+orientation+margins) &
  `vScale = 760/contentWidth` agar proporsional dengan halaman nyata.
- `reApplyGeometry(source, bands, paper)` — tulis-balik **hanya** geometri
  (x/y/width/height), expression teks, dan style dasar (fontSize/bold/align) ke
  JSON report-engine asli; **SQL, dataSources, groupBy, border, background, dan
  field lain dipertahankan verbatim**. Komponen dicocokkan via `id`; yang dihapus
  di-drop, yang baru di-append. **Tanpa data loss** pada query laporan.
- `loadBands` (report-designer-io.ts) coba format editor → report-engine →
  fallback mock; bawa `engineSource` untuk round-trip. `isForeignTemplate`
  kini `false` untuk report-engine (jadi tak ada banner "tak bisa diedit" &
  simpan tanpa confirm overwrite). `mock-designer.persist()` pakai
  `reApplyGeometry(engineSource, …)` bila `engineSource` ada, else
  `serializeTemplate`.
- Struktur-tree (right panel) judul root = nama template asli (prop `title`),
  bukan hardcode "Faktur Penjualan".

**Catatan:** mode Pratinjau untuk data band report-engine (field individual,
tanpa `cols` datarow) kini render seperti band biasa (tak meng-ulang sample
rows). Penyempurnaan preview band data (bind ke dataSource nyata) = follow-up.

---

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

### API error toast — tampilkan `message` Nest, bukan "Bad Request" (2026-07-14)

Saat delete/akses API 400/500 di master (contoh partner-types:
`Cannot delete a protected partner type.`), toast hanya menampilkan
"Bad Request" / statusText generik.

**Akar:** `AllExceptionsFilter` Nest mengembalikan
`{ success:false, statusCode, error: string, message: string, ... }`.
`packages/ui-kit` `toApiError` menganggap `error` object `{ code, message }`
→ string `"Bad Request"` jadi `error.message` undefined → fallback statusText.

**Perbaikan:**
- `packages/ui-kit/src/api/client.ts` — parse kedua envelope (Nest string
  `error` + nested `ApiError`); prioritaskan `message` (string|string[]).
- `apps/web-erp/lib/api/import.ts` — mirror parse yang sama.
- `apps/web-erp/lib/error-message.ts` `toToastMessage` — untuk pesan bisnis
  (fallback branch) tampilkan raw message API, bukan title generik.

Berlaku global untuk semua app yang pakai `createApiClient` (ERP/HR/MDP).

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

