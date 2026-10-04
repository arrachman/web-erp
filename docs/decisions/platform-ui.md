# DECISIONS — Platform & UI — shell, grid, form builder, register, lampiran transaksi

> Bagian dari decision log Web-ERP. Dipindahkan dari `DECISIONS.md`
> (2026-10-04) agar file indeks ramping; **isi entri tidak diubah** dan
> nomor `§` dipertahankan sebagai anchor stabil. Indeks semua entri:
> [`DECISIONS.md`](../../DECISIONS.md).

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

**Posisi Menu Vertical/Horizontal (2026-09-23).** Kartu "Menu Sidebar" di
Tampilan direstrukturisasi: knob utama = **Posisi Menu** (`Vertical` /
`Horizontal`). Nilai persist tetap satu field `metadata.sidebar` =
`'icon' | 'label' | 'horizontal'` (tidak ada migrasi): Vertical = `icon`/
`label`, Horizontal = `horizontal` — kartu hanya mempresentasikan ulang knob
lama. Saat pindah Horizontal → Vertical kembali, template default ke `icon`
(nilai template sebelumnya tidak disimpan terpisah).

**Template & Mode Menu berlaku di KEDUA orientasi (2026-09-23).** Row
**Template** (`Ikon` / `Ikon + Label`) dan **Mode Menu** (`Flyout` /
`Accordion`) tetap tampil saat Horizontal dan fungsinya aktif:

- **Template saat Vertical** = `metadata.sidebar` (`icon`/`label`) seperti
  semula. **Template saat Horizontal** = field baru
  **`metadata.menubarTemplate`** (`icon`/`label`, default `label`) → attr
  `data-menubar` di `<html>`; CSS `html[data-sidebar='horizontal'][data-
  menubar='icon'] .sidebar .nav-item` menyembunyikan label + center ikon.
  Field murni CSS-driven — tanpa state React; di-set di blocking script
  (`app/layout.tsx`), `use-appearance.ts` (applyTweak/hydrate/reset/save),
  dan `apply-server-prefs.ts` (attr + localStorage). `metadata` backend =
  free-form Json (`Record<string, unknown>`) — tanpa perubahan API/DB.
- **Mode Menu saat Vertical** = `sidebarMenuMode` seperti semula (flyout
  panel kanan vs accordion expand inline). **Mode Menu saat Horizontal**
  memakai state `sidebarMenuMode` yang sama: `accordion` = dropdown dengan
  grup bisa expand/collapse (`renderAccordionChildren`), `flyout` = dropdown
  daftar rata semua item per grup tanpa collapse (`renderFlyoutChildren`) —
  keduanya di panel fixed-position yang sama di `sidebar.tsx`.

**Layout horizontal diperbaiki (2026-09-23).** Sebelumnya `html[data-
sidebar='horizontal'] .app` hanya punya 1 row `'topbar' 'main'` dan
`.sidebar` **menumpuk** `.topbar` di `grid-area: topbar` yang sama —
breadcrumb/search/notif/user menu tertutup di balik menu bar. Sekarang grid =
3 row `'menubar' 'topbar' 'main'` (token baru `--menubar-h: 52px`;
`--topbar-h` kembali default 44px untuk row topbar), `.sidebar` → area
`menubar`. Submenu dropdown mode horizontal = `position: fixed` pada koordinat
viewport (lolos clip `overflow-x: auto` nav bar; lihat `sidebar.tsx`) dengan
clamp kiri/lebar ke viewport, dan tidak lagi auto-expand modul aktif saat
load (hover-driven). `applyServerPrefs` kini juga dispatch
`erp-set-sidebar` / `erp-set-sidebar-menu` agar state React AppShell
(`sidebarMode`/`sidebarMenuMode`) sinkron dengan server prefs — sebelumnya
hanya atribut DOM yang di-set (divergensi saat localStorage usang/cross-device).
Listener event = `use-app-shell-keyboard.ts` (validasi nilai di listener).

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
- `opt/web-erp/lib/api/import.ts` — mirror parse yang sama.
- `opt/web-erp/lib/error-message.ts` `toToastMessage` — untuk pesan bisnis
  (fallback branch) tampilkan raw message API, bukan title generik.

Berlaku global untuk semua app yang pakai `createApiClient` (ERP/HR/MDP).

