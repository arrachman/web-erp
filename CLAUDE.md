# Web-ERP — Aturan Baku untuk AI Agent (Claude)

Scope: **hanya** `opt/web-erp/**`. Berlaku di atas root `CLAUDE.md` repo
(tidak menggantikannya). Singkat, deklaratif, non-negosiabel.

Produk: **Senti ERP**. Legacy `opt/web-erp/preferensi/` = **referensi
fitur/business-logic/flow saja**, bukan sumber struktur kode/DB.

> 📒 **Decision log per-fitur** (appearance, item form, menu manager, geo, price
> tiers, akun GL, placements, dll) ada di [`DECISIONS.md`](DECISIONS.md). File ini
> = **rulebook** (invariant yang berlaku tiap sesi, selalu di-load); `DECISIONS.md`
> = **catatan build per-fitur** (dibaca on-demand saat menyentuh fitur terkait).
> Cross-ref `§2.x` yang tidak ada di sini → cari di `DECISIONS.md`.

---

## 1. Penamaan tabel (WAJIB)

Format baku **setiap** tabel fisik:

```
DOMAIN_NAMA-TABEL
```

- **DOMAIN** — segmen semantik **per-fungsi** (bukan numerik legacy
  `m0`/`m1`). **Tanpa** prefix produk `erp_`.
- **NAMA-TABEL** — nama entitas, `snake_case`, **plural**.

Pemisah antar segmen = underscore `_`. Semua lowercase.

Domain yang berlaku:

| Domain | Cakupan | Contoh tabel |
| --- | --- | --- |
| `sys` | Konfigurasi sistem/global — ubah → perilaku sistem berubah untuk semua | `sys_settings`, `sys_fiscal_periods`, `sys_document_numberings`, `sys_menus`, `sys_audit_logs` |
| `adm` | Identity & access — ubah → siapa bisa login/lihat/lakukan berubah | `adm_users`, `adm_roles`, `adm_permissions`, `adm_user_roles`, `adm_role_permissions`, `adm_role_menus`, `adm_user_branch_access` |
| `md` | Master Data | `md_items`, `md_partners`, `md_partner_addresses`, `md_accounts`, `md_cost_centers` |
| `fin` | Finance / GL (m2) | `fin_journal_entries`, `fin_journal_lines`, `fin_ledger_entries`, `fin_ar_receipts`, `fin_ap_payments`, `fin_giros` |
| `inv` | Inventory / stock movement (m3) | `inv_stock_movements`, `inv_stock_movement_lines`, `inv_opening_stocks`, `inv_stock_counts` |
| `pur` | Purchasing (m4) | `pur_orders`, `pur_goods_receipts`, `pur_invoices`, `pur_returns`, `pur_payments` |
| `sls` | Sales / AR (m5) | `sls_orders`, `sls_deliveries`, `sls_invoices`, `sls_returns`, `sls_receipts` |
| `mfg` | Manufacturing / production (m6) | `mfg_boms`, `mfg_work_orders`, `mfg_production_results` |
| `fa` | Fixed assets (m7) | `fa_assets`, `fa_asset_categories`, `fa_depreciations`, `fa_disposals` |
| `bi` | BI / dashboards (m8) | `bi_charts`, `bi_indicators`, `bi_chart_roles` |
| `pos` | POS / retail & promotions (m12) | `pos_areas`, `pos_contact_prices`, `pos_category_discounts` |
| `pln` | Planning / MRP-lite (new — no legacy equivalent) | `pln_reorder_policies`, `pln_demand_forecasts`, `pln_mrp_runs`, `pln_replenishment_suggestions` |

> Domain map legacy→modern + entitas inti per modul = `db-design/module-roadmap.md`
> (otoritatif). Legacy **m11 = vertical klinik** → milik `apps/web-althea`, **bukan**
> ERP (jangan diserap). m9 tidak ada; m10 perlu studi sebelum dipetakan.

Contoh: `User`→`adm_users`, `Account (CoA)`→`md_accounts`, `Document Numbering`→`sys_document_numberings`, `Partner Address`→`md_partner_addresses`.

Aturan turunan:

- **Batas `sys` vs `adm`:** definisi menu = `sys` (`sys_menus`); pemetaan
  role→menu = `adm` (`adm_role_menus`). FK lintas-domain diperbolehkan.
- "Administrator" legacy (m0) **dipecah** jadi `sys` (config sistem:
  setting, fiscal period, numbering, menu, audit log) + `adm` (identity &
  access: user, role, permission, pivot akses). Master Data (m1) → `md`.
  Modul fungsi baru → tambah domain semantik baru, **bukan** `m<n>`.
- Di Prisma: model `PascalCase` tetap ber-prefix `Erp` (hindari bentrok
  model `User`/`Menu` platform di schema yang sama) + `@@map("<domain>_...")`.
  Contoh: `model ErpItem { ... @@map("md_items") }`.
- Tabel pivot/junction: gabung kedua entitas —
  `adm_user_roles`, `adm_role_permissions`.
- Satu tabel = satu domain semantik tempat ia didefinisikan.
- **Tidak boleh** bentrok dengan tabel platform di Postgres bersama
  (`m0_*`, `m1_*`, `clinic_*` milik Althea/api-gateway). Namespace domain
  `sys_`/`adm_`/`md_` tidak beririsan dengan prefix platform — ERP tidak
  menumpang/reuse tabel platform.

> Dokumen desain DB otoritatif = `opt/web-erp/db-design/` (`README.md` hub +
> `entities-m*.md` per modul + `module-roadmap.md` + `legacy-mapping.md`).
> MVP pakai `sys_`/`adm_`/`md_`; modul pasca-MVP (m2–m12) dipetakan di
> `module-roadmap.md`. **Semua open decision RESOLVED (2026-05-17)** — log di
> `db-design/README.md §8`. Perubahan kunci: PK = **BigInt**, **`sys_audit_logs`
> masuk MVP**, **`CurrencyRate` bertanggal**, **`legacyCode` di tiap master**;
> `ErpUser` terpisah dari User klinik. **MVP = 31 tabel** (14 `sys_*`/`adm_*`
> + 17 `md_*`). **Prisma sudah ditulis & dimigrasi (2026-05-18):** 156 tabel
> ERP `fin`/`inv`/`pur`/`sls`/`mfg`/`fa`/`pos`/`pln` + 53 enum (additive, 0 DROP).
> Referensi lintas-domain = scalar `BigInt` FK + `@@index` **tanpa** `@relation`
> (domain decoupled); FK intra-domain ditegakkan. `bi`/m8 **dikecualikan**.

---

## 2. Design system dulu, baru slicing frontend (WAJIB)

**DILARANG** mulai slicing/implementasi halaman atau fitur frontend sebelum
design system + komponen dasarnya siap.

Urutan wajib sebelum halaman pertama dibangun:

1. **Design tokens** — warna, tipografi, spacing, radius, shadow, breakpoint
   (sumber tunggal; bukan nilai hardcode tersebar).
2. **Komponen primitif** — Button, Input, Select, Checkbox/Radio, Modal,
   Table, Form field, Card, Badge, Toast/Alert, Tabs, Sidebar/Nav, Pagination.
3. **Pola layout** — shell aplikasi (sidebar + topbar), layout list/detail,
   layout form, state kosong/loading/error.
4. Baru setelah itu: slicing halaman modul (m0, m1, …) **memakai** komponen
   tersebut — tidak menulis ulang elemen UI ad-hoc per halaman.

---

### 2.1 Atomic Design (WAJIB saat membuat komponen frontend)

Setiap komponen frontend **wajib** dibangun mengikuti **atomic design**
(Brad Frost) — bukan komponen ad-hoc per halaman. Hierarki & pemetaan ke
urutan wajib di atas:

| Tingkat | Definisi | Contoh di ERP | Selaras langkah |
| --- | --- | --- | --- |
| **Atoms** | Elemen UI terkecil tak-terpecah | Button, Input, Label, Icon, Badge | §2 langkah 2 |
| **Molecules** | Gabungan beberapa atom = satu fungsi | Form field (label+input+error), search box, pagination | §2 langkah 2 |
| **Organisms** | Bagian UI kompleks gabungan molecule/atom | Tabel data + toolbar, sidebar nav, form section | §2 langkah 2–3 |
| **Templates** | Kerangka layout tanpa data nyata | Layout list/detail, layout form, app shell | §2 langkah 3 |
| **Pages** | Template + data nyata modul | Halaman m0/m1 (Item, Partner, User, …) | §2 langkah 4 |

Aturan:

- Komponen baru → tentukan tingkatnya; taruh di folder per tingkat (`atoms/`, `molecules/`, `organisms/`, `templates/`, `pages/`).
- Primitif dari `@sentient-factory/ui-kit` tetap bagian design system ERP; Tailwind source web-ERP **wajib** mencakup `packages/ui-kit/src` agar utility class primitive (Select, Dropdown, Tooltip, Dialog) ter-compile — jika tidak, flyout bisa transparan karena `bg-popover`/`border-*` tidak di-generate.
- **Dilarang** membangun tingkat lebih tinggi sebelum tingkat di bawahnya ada sebagai reusable (page tidak menulis ulang atom/molecule ad-hoc).
- Atom/molecule **tanpa** business logic & **tanpa** style hardcode — hanya token + props; logic naik ke organism/page.
- Konsisten batas 400 baris (§3): komponen besar dipecah per tingkat.

Konsekuensi:

- Butuh elemen UI belum ada di design system → **stop**, tambahkan dulu sebagai reusable di tingkat atomic yang tepat, baru lanjut.
- Tidak ada style/warna/spacing hardcode di halaman — selalu lewat token.
- Scope design system belum jelas → konfirmasi ke user, jangan asal mulai halaman.

---

### 2.2 Penamaan file frontend (WAJIB)

- **Dilarang** prefix `erp-` pada nama file di `opt/web-erp/**` (mis.
  `components/pages/erp-items-page.tsx`). Path sudah berada di bawah
  `web-erp`, jadi prefix produk pada filename = redundant noise.
- `kebab-case` + akhiran semantik per tingkat atomic: `-page.tsx`,
  `-form.tsx`, `-list.tsx`, dst. Contoh benar:
  `components/pages/items-page.tsx`, `components/pages/items-form.tsx`.
- Pengecualian: model Prisma tetap ber-prefix `Erp` (lihat §1) — itu untuk
  hindari bentrok kelas/identifier lintas-app dalam satu schema, **bukan**
  konvensi filename frontend.

---

### 2.3 Canonical route id = seeded `sys_menus.path` (2026-05-19)

Sidebar di-render dinamis dari `GET /api/erp/sys-menus/my-menus` (role-
filtered). Route id kanonik **= `sys_menus.path`** (mis. `/master/locations`,
`/admin/fiscal-periods`). `renderRoute` memetakan path→komponen via registry
`ERP_PAGES` (`shell-route-renderer.tsx`); short-id legacy (`adm-users`,
`md-items`) hanya **alias** untuk fallback `NAV` statis saat API down. Halaman
ERP baru: tambahkan entry di `ERP_PAGES` (key = path seeded di
`prisma/seed-erp.ts`) + `ERP_ROUTE_META` (`lib/nav.ts`) untuk breadcrumb.
Jangan bikin skema id baru — `sys_menus` adalah SSOT navigasi.

---

### 2.3.1 URL form transaksi = sub-route `<base>/new` & `<base>/:id` (2026-05-31)

Halaman **transaksi** (CR/CD/BD/giro/jurnal) wajib **URL-addressable** dalam
tiga bentuk, semua diturunkan dari `base` = canonical `sys_menus.path` (§2.3):

| URL | View |
| --- | --- |
| `<base>` (mis. `/finance/cash-receipts`) | list |
| `<base>/new` | form create |
| `<base>/:id` | form edit (deep-link/refresh → fetch by id) |

Route string tetap **single source of truth** untuk list-vs-form — tidak ada
lagi state `mode` internal di page. Mekanisme reusable:

- Helper + builder di [`lib/trx-route.ts`](lib/trx-route.ts): `resolveTrxFormRoute()`,
  `trxNewRoute()`, `trxEditRoute()`, interface `TrxFormPageProps`
  (`formMode?`/`recordId?`/`onNavigate?`).
- Registry `TRX_FORM_PAGES` di `shell-route-renderer.tsx` (key = base path).
  **Page transaksi didaftar di sini, BUKAN di `ERP_PAGES`** — `renderRoute`
  meng-handle list + `/new` + `/:id` sekaligus dan meng-oper `onNavigate`.
- Page menerima `TrxFormPageProps`: derive `mode` dari `formMode`, muat form
  via effect (`create` → blank, `edit` → `getRecord(recordId)`), dan
  open/edit/back/save = panggil `onNavigate(...)` (replace-in-tab). `onNavigate`
  = `navigateInTab` → konsisten di Internal mode (ganti route tab) & Per-page
  URL mode (§2.19, URL jadi `/app<base>/new`).
- `pageMeta()` (`lib/nav.ts`) otomatis turunkan breadcrumb sub-route dari base
  meta + crumb `Baru`/`Edit` — tidak perlu daftar `/new`/`/:id` di `ERP_ROUTE_META`.

Adopter = **Kas Masuk** (`fin-cash-receipts-page.tsx`), **Kas Keluar**
(`fin-cash-disbursements-page.tsx`) & **Bank Masuk** (`fin-bank-receipts-page.tsx`).
Page transaksi baru: daftar di `TRX_FORM_PAGES`, implement `TrxFormPageProps`,
reuse helper — **jangan** fork skema URL sendiri. Workflow row-actions
(Ajukan/Setujui/…) shared di
[`lib/fin-cash-bank-workflow.ts`](lib/fin-cash-bank-workflow.ts). Form & filter
CR/CD beda **hanya label + arah** → komponen shared berparameter
([`cash-bank-transaction-form.tsx`](components/pages/cash-bank-transaction-form.tsx) +
[`cash-bank-filters.tsx`](components/pages/cash-bank-filters.tsx)); per-direction
= wrapper tipis (jangan duplikat). **Bank Masuk (RM) = twin Kas Masuk** (kind=BANK):
wrapper tipis yang oper `headerExtra` (Cara Bayar) + `extraTabs` (Giro,
organism [`cash-bank-giros.tsx`](components/organisms/cash-bank-giros.tsx)) ke form
shared; **jangan** fork form. Detail di `DECISIONS.md` § Kas Masuk / Kas Keluar /
Bank Masuk.

---

### 2.5 ERP controllers WAJIB pakai `ErpJwtAuthGuard` (2026-05-20)

Semua controller di `apps/api-gateway/src/erp-*/**` **harus** guard dengan
`ErpJwtAuthGuard` dari `../erp-auth/guards/erp-jwt-auth.guard`, **bukan**
`JwtAuthGuard` clinic dari `../auth/guards/jwt-auth.guard`. Cookie
`erp_token` ditandatangani oleh ErpAuthService dan hanya dikenali strategy
`erp-jwt`. Salah guard → semua endpoint ERP 401 padahal `/erp/auth/me` &
`/erp/sys-menus/my-menus` jalan. Sudah dibetulkan untuk 18 controller
(branches, warehouses, locations, items, units, item-categories, users,
roles, permissions, settings, currencies, taxes, payment-terms,
partner-categories, partners, fiscal-periods, document-numberings,
accounts).

---

### 2.6 Pilihan biner = radio button, bukan Select (2026-05-20)

Saat field form hanya punya **2 opsi** (mis. Aktif/Nonaktif, Ya/Tidak, Pria/
Wanita, Debit/Kredit) → **WAJIB** pakai radio button (atau segmented control),
**bukan** `Select`/dropdown. Alasan: dropdown untuk 2 opsi = 2 klik untuk
melihat & 2 klik untuk pilih, padahal radio = 1 klik dan kedua opsi terlihat
langsung tanpa harus dibuka. Juga lebih aksesibel (tab langsung, tanpa popover).

Berlaku untuk semua form ERP (filter list, dialog create/edit, settings).
`Select` tetap dipakai untuk ≥ 3 opsi atau saat opsi-nya dinamis dari API.
Saat vibe coding sebuah halaman: **baca dulu role/konteks field**, hitung
jumlah opsi, pilih kontrol yang sesuai sebelum nulis JSX.

Primitive tersedia di [`components/ui/radio-group.tsx`](components/ui/radio-group.tsx):

- `BooleanRadio` — helper untuk boolean field (Aktif/Nonaktif default,
  bisa override `trueLabel`/`falseLabel` untuk Ya/Tidak dll).
- `RadioGroup<T>` — generik untuk non-boolean (≥ 2 opsi terbatas yang
  bisa di-segmented). Pilih ini bila value bukan boolean.

Refactor awal (2026-05-20): 15 binary `Select` di 12 form (items, partners,
users, branches, warehouses, locations, taxes, units, currencies, payment-
terms, partner-categories, accounts, document-numberings) sudah diganti
`BooleanRadio` — jangan re-introduce pattern lama.

---

### 2.7 Standar baku setiap halaman list ERP (WAJIB, 2026-05-20)

**Setiap** halaman list master/transaksi di `opt/web-erp/**` **wajib**
menyediakan fitur-fitur berikut. Tidak ada list page yang boleh ship tanpa
ini — kalau salah satu hilang, halaman itu **belum** selesai. Implementasi
**harus** lewat organism reusable (`erp-list-layout.tsx` + turunannya),
bukan ditulis ulang ad-hoc per halaman.

**A. Header / Topbar (app shell, sudah disediakan `app-shell.tsx`)**
- Breadcrumb hierarkis: `Sentient / ERP → <Modul> → <Entitas>` — sumber
  label dari `ERP_ROUTE_META` (§2.3).
- Multi-tab workspace dengan tombol `+` buka tab baru + indikator jumlah tab.
- Global search "Cari semua…" shortcut **K** → buka `CommandPalette` (§2.4).
- Notifikasi, activity monitor, shortcut helper, user menu.

**B. Action bar (per halaman list)**
- Search lokal "Cari …" shortcut **`/`** (fokus ke input search list).
- Tombol **Export** data (CSV/XLSX, sesuai izin role).
- Tombol **Refresh** (re-fetch list).
- Tombol **`+ Tambah <entitas>`** shortcut **N**.

**C. Filter & summary bar**
- Filter **Status** approval (default "Semua") — chip/select.
- Tombol ikon filter lanjutan (kolom dinamis bila ada).
- **Summary agregat** kontekstual (mis. Σ piutang, Σ qty stok, jumlah baris
  difilter vs total) — diletakkan di kanan/atas tabel.
- Tombol **Reset filter** (visible begitu ada filter aktif).

**D. Tabel data**
- Checkbox select per-row + select-all di header.
- Kolom kanonik per entitas (kode, nama, atribut kunci, nilai numerik, status).
- **Kode** = link clickable → buka detail/edit, format kanonik per entitas
  (mis. `CUS-YYMM-NNNN`).
- Kolom numerik (uang/qty) **right-aligned** + format ribuan Indonesia.
- Badge status workflow **berwarna konsisten** mengikuti token design system:
  `Draft`, `Need Approve`, `Approved`, `Rejected`, `Posted` (warna dipetakan
  sekali di token, jangan hardcode per halaman).

**E. Footer / pagination**
- Indikator: `Halaman X dari Y · M dari N baris`.
- Pagination kontrol prev/next.
- Implementasi via organism reusable [`components/organisms/list-footer.tsx`](components/organisms/list-footer.tsx)
  (`ListFooter`) — mode `pagination` (TablePagination penuh) atau `summary`
  (count-only, dipakai halaman non-paginasi seperti tree menus §2.22).

**F. Keyboard-first navigation (WAJIB, listener di organism list)**
- `J` / `K` atau `↓` / `↑` → navigasi baris bawah/atas.
- `X` → toggle pilih baris aktif.
- `N` → tambah baru (sama dengan tombol di action bar).
- `←` / `→` → halaman prev / next.
- `/` → fokus search lokal · `K` → global palette · `?` → shortcut helper.

**G. Sidebar kiri (app shell)**
- Modul ikonik dinamis dari `GET /api/erp/sys-menus/my-menus` (§2.3).
- Toggle tema (matahari/bulan) di paling bawah; toggle bahasa ID/EN.

**Workflow approval**: setiap master/transaksi yang punya status workflow
**wajib** mengikuti state machine `Draft → Need Approve → Approved/Rejected
→ Posted`. State machine + transition rules hidup di backend; frontend list
hanya menampilkan badge + filter, tidak memutuskan transisi.

**Konsekuensi vibe coding:**
- List page baru → mulai dari organism `erp-list-layout.tsx` + turunan `generic-list*`/`data-list*`. **Dilarang** start dari blank `<table>`.
- Butuh fitur list belum di organism → **stop**, tambahkan ke organism reusable dulu (§2.1), baru pakai.
- Checklist A–G = **definition of done** list page; declare selesai = semua terpenuhi atau eskalasi pengecualian.

---

### 2.8 `cursor: pointer` wajib pada semua elemen interaktif (WAJIB)

Setiap elemen yang bisa diklik atau difokus **harus** menampilkan `cursor:
pointer`. Tidak ada pengecualian — elemen tanpa cursor pointer membingungkan
user karena tidak terbaca sebagai interaktif.

Elemen yang wajib:

| Elemen | Cara set |
| --- | --- |
| `<button>` (semua varian: primary, secondary, ghost, icon, dll) | Token/class global di design system |
| `<a>` / `<Link>` (Next.js) | Token/class global |
| `<input type="checkbox">` | Token/class global |
| `<input type="radio">` | Token/class global |
| Wrapper custom checkbox/radio (div/span klik) | `cursor-pointer` via Tailwind atau token |
| Label yang `htmlFor` ke input interaktif | `cursor-pointer` |
| Clickable table row / cell | `cursor-pointer` pada `<tr>` / `<td>` |
| Chip, badge, tag yang bisa diklik | `cursor-pointer` |

Cara implementasi:

- **Global CSS** (paling direkomendasikan): tambahkan satu rule di
  `styles/erp-components.css` (atau `globals.css`) agar semua elemen standar
  sudah di-cover tanpa perlu `className` per komponen:

  ```css
  button,
  a,
  [role="button"],
  input[type="checkbox"],
  input[type="radio"],
  label[for] {
    cursor: pointer;
  }
  ```

- **Wrapper custom**: tetap tambahkan `cursor-pointer` (Tailwind) atau
  `style={{ cursor: 'pointer' }}` bila elemen bukan tag HTML standar di atas.
- **Disabled state**: elemen `disabled` → `cursor: not-allowed` (override;
  jangan biarkan pointer di elemen tidak aktif).

Saat membuat komponen Atom baru (Button, Checkbox, RadioGroup, dll): **cek
dulu** apakah global CSS sudah di-cover — jika belum, tambahkan ke komponen
level atom (bukan ad-hoc di page). Jangan deklarasikan komponen selesai
sebelum cursor state-nya benar.

---

### 2.9 Spesifikasi detail behaviour halaman list (WAJIB, 2026-05-20)

Detail yang melengkapi checklist §2.7 — semua wajib konsisten di setiap list.

#### Row visual states (tidak boleh campur)

| State | Trigger | Visual |
| --- | --- | --- |
| Normal | Default | Latar default |
| Hovered | Cursor di atas row | `--bg-hover` |
| Focused (keyboard) | J/K aktif | Border kiri 2px `--accent` + `--bg-focus` |
| Selected (checkbox) | ✓ atau X keyboard | `--bg-selected`, checkbox terisi |
| Focused + Selected | Keduanya | Gabung: border kiri + latar selected |

Focused ≠ selected. J/K hanya memindahkan fokus visual; X / Space men-toggle selection (navigate tanpa sengaja memilih).

#### Column alignment & format angka

| Tipe data | Alignment | Format |
| --- | --- | --- |
| Teks (kode, nama, kota, NPWP) | Kiri | — |
| Numerik (uang, qty, %) | **Kanan** + `tabular-nums` | `46.666.000,00` |
| Badge / status | Kiri | `● Approved` |
| Aksi inline | Kanan | `Edit Hapus` |
| Checkbox | Tengah | ☐ |

Format Rupiah: `Intl.NumberFormat('id-ID', { minimumFractionDigits: 2 })` (titik ribuan, koma desimal). **Tidak ada simbol Rp di kolom tabel.** Implementasi sekali di `lib/format.ts` — tidak boleh inline per halaman.

#### Tinggi baris/tabel = density token (WAJIB, 2026-05-20)

Tabel list **wajib** ikut knob `density` Setting → Tampilan (`compact`/`comfortable`) via [`components/organisms/table.tsx`](components/organisms/table.tsx): `TableHead` `h-[var(--header-h)]`, `TableCell` `h-[var(--row-h)]`, token di [`styles/erp-tokens.css`](styles/erp-tokens.css) (`[data-density=…]`). App-shell hydrate `data-density` (localStorage `erp-appearance` lalu override server `getMyPreferences()`). **DILARANG** hardcode `data-density` di mount effect. List baru: pakai organism `Table*` — density otomatis.

#### Approval status — token color mapping (jangan hardcode)

| Status | Badge variant | Label |
| --- | --- | --- |
| `DRAFT` | `default` | Draft |
| `NEED_APPROVE` | `warning` | Need Approve |
| `APPROVED` | `success` | Approved |
| `REJECTED` | `danger` | Rejected |
| `POSTED` | `info` | Posted |

Mapping **sekali** di `lib/status.ts` (`statusBadgeVariant`/`statusLabel`); import, jangan switch/if per halaman.

#### H. Bulk action toolbar (WAJIB bila ada operasi batch)

Tampil di atas tabel **hanya saat ≥1 baris dipilih**; hilang saat 0. Slide-in. Teks `X baris dipilih` + tombol batch sesuai entitas + **Batal pilihan** (kanan). Aksi destruktif → confirmation dialog. Setelah batch → reload + clear selection + toast. Implementasi via slot `toolbar` `ErpListLayout` / organism `bulk-action-bar.tsx` — **dilarang** inline.

#### Post-action feedback (toast via `notify()`)

| Operasi | Variant | Contoh |
| --- | --- | --- |
| Create/Update/Delete | `success` | `"Customer dibuat"` |
| Batch | `success` | `"3 customer diaktifkan"` |
| Error API | `danger` | Pesan dari `error.message` |
| Fitur belum tersedia | `warn` | `"Export belum tersedia"` |

Tidak ada operasi **silent**. Error wajib meneruskan pesan asli API, bukan generik.

#### Confirmation dialog (aksi destruktif / undo)

Aksi tidak-undo (hapus, batch-hapus, reject) **wajib** `confirmAction()`: title `Hapus <Entitas>?`, pesan `<kode> — <nama> akan dihapus permanen` (batch: `X <entitas>…`), tombol confirm `danger` label eksplisit (`Hapus`), batal ghost/secondary.

#### Empty / loading / error state

| State | Tampilan |
| --- | --- |
| Loading | `Memuat...` di tengah area tabel |
| Empty (no data) | `TableEmpty` colspan: `Tidak ada data` |
| Empty (filtered) | `TableEmpty`: `Tidak ada hasil untuk filter ini` |

`TableEmpty` (`components/organisms/table.tsx`) untuk semua empty state — jangan biarkan `<tbody>` kosong. **Error = molecule `ErrorState` (2026-05-20):** pesan backend mentah (`Not Found`, `Failed to fetch`, `Unauthorized`) **dilarang** ditampilkan apa adanya; `ErpListLayout` render [`components/molecules/error-state.tsx`](components/molecules/error-state.tsx) (kategori: tidak terhubung/tidak ditemukan/akses ditolak/server/fallback + tombol Coba lagi). Cukup oper `error` dari `useErpList`.

---

### 2.10.1 Modal master = body scroll terpisah (WAJIB, 2026-07-14)

Modal create/edit via organism `SimpleMasterPage` (dan semua dialog master
`code+name+isActive`) **wajib** tetap muat viewport tanpa konten terpotong:

- `ModalContent` dibatasi `max-h-[84vh]` + tetap `flex flex-col`.
- Header/Footer `shrink-0`; region tengah (`FormErrorSummary` + `FormFields`)
  dibungkus wrapper `min-h-0 flex-1 overflow-y-auto`.
- Tujuan: form kaya (mis. Partner dengan tab Kontak/Alamat & nested kebab
  §2.11) tetap bisa di-scroll ke bawah; footer (Batal/Simpan) selalu visible.

Jangan biarkan region form tumpah ke luar dialog tanpa scroll — konten bawah
menjadi tak terjangkau (bug Partner, 2026-07-14).

---

### 2.10 Status boolean = kolom badge `Aktif/Nonaktif` (2026-05-20)

Untuk entitas dengan status biner (`isActive`) di list page: tetap kolom
sendiri `STATUS` berisi `<Badge variant="success|default" dot>` —
`Aktif` (hijau) / `Nonaktif` (abu). Eksperimen "dot di depan Nama + mute
baris nonaktif" dicoba lalu **di-rollback** atas keputusan user
(2026-05-20): kolom badge lebih konsisten dengan workflow status multi-state
dan lebih mudah dipindai saat semua baris perlu kelihatan "setara".

Workflow multi-state (`Draft/Need Approve/Approved/Rejected/Posted`) tetap
pakai `StatusBadge` (§2.9).

---

### 2.11 Inline row actions = semua di kebab menu (WAJIB, 2026-05-20)

Pola wajib kolom action di list page: **semua aksi (Edit, Riwayat, Hapus, …)
masuk kebab menu** (icon `more-vertical`). Tidak ada tombol aksi yang visible
inline — kolom action hanya berisi satu icon `⋮` per baris.

Alasan: deretan tombol per-baris × 10–20 baris bikin tabel sangat "ribut",
dan tombol Hapus merah yang visible selalu rawan salah klik. Dengan semua
aksi di kebab, tabel jauh lebih tenang dan setiap aksi butuh klik sengaja
(termasuk Edit) — trade-off: Edit jadi 2 klik, tapi user tetap bisa klik
kode di kolom KODE (`CodeLinkCell`) sebagai jalur cepat ke detail/edit
(satu klik).

Konvensi item kebab:
- Urutan: aksi navigasi/baca dulu (Edit, Riwayat, Duplikat, …), aksi
  destruktif terakhir.
- **Hapus selalu paling bawah** + `separatorBefore: true` + `danger: true`.
- Workflow approval (Approve/Reject/Post) ikut masuk kebab — bukan bikin
  tombol baru.

Implementasi wajib lewat molecule reusable
[`components/molecules/row-actions-menu.tsx`](components/molecules/row-actions-menu.tsx)
(`RowActionsMenu`) — **dilarang** rakit ad-hoc per halaman. Primitif Radix di
[`components/ui/dropdown-menu.tsx`](components/ui/dropdown-menu.tsx). **Menu row-action
harus memakai z-index di atas modal (`z-[800]`)** agar kebab tetap clickable saat
dipakai di tabel nested dalam dialog (bug Partner kontak/alamat, 2026-07-14).

**Paritas right-click (WAJIB, 2026-05-20):** setiap baris list **wajib** juga
membuka menu yang sama via klik-kanan (context menu). Bungkus `<TableRow>`
dengan `<RowContextMenu items={rowActions}>` dari molecule yang sama; primitif
Radix di [`components/ui/context-menu.tsx`](components/ui/context-menu.tsx).
Items **harus** array yang sama (referensi sama) dengan `<RowActionsMenu>`
agar opsi & urutannya garanteed sinkron — jangan duplikasi literal. Contoh:

```tsx
const rowActions: RowActionItem[] = [
  { label: 'Edit', onSelect: () => openEdit(row) },
  { label: 'Riwayat', onSelect: () => setAuditTarget(row) },
  { label: 'Hapus', onSelect: () => handleDelete(row), danger: true, separatorBefore: true },
];
return (
  <RowContextMenu items={rowActions}>
    <TableRow ...>
      ...
      <TableCell><RowActionsMenu items={rowActions} /></TableCell>
    </TableRow>
  </RowContextMenu>
);
```

---

### 2.12 Server-driven pagination + search + filter + sort (WAJIB, 2026-05-20)

**Setiap** list page **wajib** kirim `page`, `limit`, `search`, `sortBy`, `sortDir` (+`isActive` bila ada) ke API. **DILARANG** filter/slice di klien — backend default `limit=10` → client-side filter bikin user cuma lihat 10 baris walau footer bilang "Tampilkan 25".

> Bug history (2026-05-20): branches cuma tampil 10 baris (FE kirim tanpa `limit` + `rows.filter().slice()`); lanjutan: DTO `warehouses`/`partners`/`locations` belum punya `sortBy`/`sortDir` → `forbidNonWhitelisted` lempar 400. Pelajaran: **sync DTO query dulu sebelum FE server-side sort/pagination**.

**Pola kanonik** (lihat [components/pages/branches-page.tsx](components/pages/branches-page.tsx)):

```tsx
const [sortBy, setSortBy] = useState('createdAt');
const [sortDir, setSortDir] = useState<'asc'|'desc'>('desc');
const [search, setSearch] = useState('');
const [statusFilter, setStatusFilter] = useState('active'); // default: aktif saja
const { page, pageSize, setPage, setPageSize } = useListPagination('branches');

// Debounce search 300ms
const [debouncedSearch, setDebouncedSearch] = useState(search);
useEffect(() => {
  const t = setTimeout(() => setDebouncedSearch(search), 300);
  return () => clearTimeout(t);
}, [search]);

const isActiveParam = statusFilter === 'active' ? true
  : statusFilter === 'inactive' ? false : undefined;

const { rows, meta, loading, error, reload } = useErpList(
  () => listBranches({ page, limit: pageSize, search: debouncedSearch || undefined, sortBy, sortDir, isActive: isActiveParam }),
  [page, pageSize, debouncedSearch, sortBy, sortDir, isActiveParam],
);

useEffect(() => { setPage(1); }, [debouncedSearch, statusFilter, sortBy, sortDir, pageSize]); // reset page 1

const paged = rows, totalRows = meta?.total ?? 0, pageCount = meta?.totalPages ?? 1;
```

Aturan turunan:

- `useErpList` (`lib/use-erp-list.ts`) **harus** dipanggil dengan **deps array** kedua (fetcher closure di-cache via ref).
- `meta.total`/`meta.totalPages` backend = SSOT pagination footer. **Dilarang** memo `filtered = rows.filter(...)`.
- Backend DTO **wajib** support `page`/`limit`/`sortBy`/`sortDir` + `search`; tambah `isActive` bila perlu. DTO belum lengkap → tambah dulu di `apps/api-gateway/src/erp-<feature>/dto/query-*.dto.ts`.
- Pengecualian (list kecil enum-like, tanpa paginasi): `settings`, `fiscal-periods`, `menus`, `permissions`. Tumbuh → tambah paginasi backend dulu.

---

### Aturan turunan lintas-fitur (ringkas — detail di `DECISIONS.md`)

Index rule lintas-fitur: satu baris = invariant + ref; detail + rasional di
section ber-`§` [`DECISIONS.md`](DECISIONS.md).

- **Master `code+name+isActive`** → organism `SimpleMasterPage` (jangan fork); validation WAJIB (`validate`, `aria-invalid`, `error`, auto-focus error pertama). → §2.15/§2.16
- **Tree + DnD** (CoA/kategori/menu) → `TreeDndMasterPage`; jangan bikin organism tree baru. → §2.22
- **Tab strip / sortable** → `@dnd-kit/core`+`sortable`; dilarang HTML5 DnD / react-beautiful-dnd. → §2.14
- **Master `code`** = bare semantic, **tanpa** prefix entity-scope (`CAT-`/`BRD-`/`UNT-`). → §2.27
- **Search endpoint** = `code` exact-match (insensitive) + `name` `contains`; dilarang `code: { contains }`. SearchSelect: exact-code auto-pick saat commit. → §2.29/§2.30
- **`SearchSelect` modal** = stale-while-loading saat ganti halaman; multi: Enter = submit Pilih, Space/klik = toggle. → §2.28
- **Input numerik** = `<NumInput>` (bukan `<Input type=number>`); display = `formatNumber/formatRupiah/formatQty` (`lib/format.ts`). → §2.31
- **Input tanggal** = `<DateInput>` (bukan `<Input type=date>` mentah; caption bulan/tahun = dropdown, rentang `calendarNavBounds()`); display = `formatDate()` (`lib/date-format.ts`, dinamis dari `sys_settings`). Pengecualian: grid-cell editor & date-range-picker. → §2.39
- **Format kode akun & angka** = dinamis dari `sys_settings`; account-code = lock-after-data. → §2.24/§2.31
- **Enum business-logic** (≥3 nilai) → info-icon label + Radix Popover comparison. → §2.26
- **Migrasi ERP** = hand-written SQL + `prisma migrate deploy` (bukan `migrate dev`) + `prisma generate` **di container** lalu restart. → §2.32/§2.34
- **Preferensi user** (theme/lang/density/font/sidebar/primary) → `adm_user_preferences`; 3 bahasa UI `id/en/ja`. → §2.13
- **Command palette & sidebar** = derived `sys_menus` role-filtered (`my-menus`); dilarang hardcode. Seed menu SSOT = `prisma/seed-erp.ts` (jangan seed ERP menu di `seed.ts`). → §2.4/§2.17
- **Mode URL routing** (`urlRoutingEnabled`) → ganti mode wajib `confirmAction`. → §2.19
- **URL form transaksi** → `<base>/new` (create) & `<base>/:id` (edit); route = SSOT list-vs-form (tanpa state `mode`). Reuse `lib/trx-route.ts` + registry `TRX_FORM_PAGES`. → §2.3.1
- **Layout form transaksi** → kanan-atas urutan: **Tanggal → No Transaksi → Uang/Kurs**; identitas (partner/akun/uraian) kiri, dimensi (cabang/lokasi) tengah. Label rata kiri, asterisk di belakang. Berlaku CR/CD/BD/giro/jurnal. → §2.36
- **Kas/bank (CR/CD/BD)** → backend shared `erp-fin-cash-bank-transactions` (`direction`, `docNumber` auto, `fiscalPeriodId` dari tanggal, posting GL balanced saat POST). Baris = organism `cash-bank-lines.tsx` (satu kolom Total, bukan debit/kredit). Status read-only + transisi via aksi. → § Kas Masuk
- **Status dokumen transaksi** = enum `ErpDocumentStatus` 7-nilai (`DRAFT/NEED_APPROVE/APPROVED/REJECTED/POSTED/VOID/CANCELLED`), sejalan `lib/status.ts`; jangan reintroduce varian 4-nilai lama. → § Kas Masuk
- **Filter list transaksi** = 1 baris via slot `toolbar` `ErpListLayout` (gabung summary `Σ`); inline = Status + Tanggal; tombol **Filter** (badge jumlah aktif) → drawer kanan staged (`organisms/drawer.tsx`). Tanpa chip terpisah. → §2.40
- **Tipe Partner** = master `md_partner_types` (`code+name+isActive`; `kind` derived dari code: `CUST`/`SUP`/`SLS`/lain → `CUSTOMER`/`SUPPLIER`/`SALESMAN`/`GENERAL`) + FK `md_partners.partner_type_id`; **jangan** reintroduce boolean `isCustomer`/dll. Kategori/sub-segmen = `md_partner_categories` (`salesTier`). → DECISIONS.md "Tipe Partner"
- **Atribut item** (Nozzle/OEM/dll) → mirror `md_colors` (code+name+isActive) + FK `md_items` + modul ber-guard `ErpJwtAuthGuard` + seed `seed-erp.ts` + daftar `ERP_PAGES`/`NAV`/`ERP_ROUTE_META`. Reuse master existing; Vendor→`md_partners`, Satuan Lapangan→`md_units`. **Jangan** tabel atribut generik. → §2.35
- **Sumber lookup** = slug kanonik `lib/lookup-source-registry.ts` (14 sumber); resolve via `lib/grid-lookup-loaders.ts` (slug lama di-alias). **Jangan** bikin slug baru. → § Kustomisasi Grid
- **Field settings kolom grid** = `GridColumnSettings` gear → Placeholder + Nilai default (type-aware) + Lookup config. DB: `sys_transaction_grid_columns.placeholder/default_value/default_value_label`. Live: `applyColumnDefaults` + `useSeedLineDefaults`. → § Kustomisasi Grid
- **Form Builder field** → config per-field di `sys_form_fields` (label/tipe/visible/wajib/slot/urutan + lookup + placeholder/`defaultValue`/`isReadonly`). UI = satu `FieldSettingsPopover`; form konsumsi `ph()`/`ro()` + `formDefaultsPatch()`. → § Form Builder
- **Header form transaksi = render 100% config** (no hardcoded `<Field>`): loop config-ordered per slot, dispatch struktural→`CashBankStructuralField` vs custom→`CashBankCustomField`. Tipe sistem di-GUARD (Select disabled di builder); custom bebas. → § Header form transaksi
- **Config transaksi baru = baseline version-controlled** (bukan live-DB-only): grid kolom + `lineTable` per famili di `seed-erp-transaction-grids.ts` (`GridFamily`: cashbank/journal/giro/giroClearing/inv*), header default per kode di `DEFAULTS_BY_CODE`. Finance non-kas/bank sudah config-only; **giro = grid instrumen (`fin_giros`), bukan baris jurnal**. → § Setup config
- **Grid baris transaksi** = mesin generik (`grid-line-core.ts` `GridModel<Row>` + `use-grid-nav.ts` `useGridNav<Row>` + `LineCell`). Transaksi baru → bikin `GridModel<Row>` + organism; **jangan fork** mesin. Cash/bank=`cashBankGridModel`; sales=`slsItemGridModel`. → § Sales Order
- **Sales item-based (SO/SI/DO)** → pola cash/bank, baris=item (Item·Qty·Satuan·Harga·Disc·Pajak·Total): shared `sales-transaction-form.tsx` + `sls-item-lines.tsx` + wrapper tipis. **SO tidak posting GL** (SI yang posting AR/revenue). → § Sales Order
- **Biaya item (tab Harga)** = system-managed dari pembelian: visible hanya `purchasePrice`+`lastHpp` (read-only, di-stamp GRN POST); `standardCost` dihapus dari UI. Pajak = 4 slot (Beli 1/2 + Jual 1/2). Tingkat Harga/Diskon = dinamis/unlimited via `md_partner_categories.sales_tier`. → § Item Harga tab
- **Media item** = galeri `md_item_media` (max 8 img + 1 video, satu primary); file `uploads/erp-items/`, stream via endpoint ber-guard; upload `apiUpload()` + organism `item-media-upload.tsx`. Butuh item tersimpan. → § Item Media
- **Lampiran file = per-modul** (bukan `sys_attachments` generik). Pilot Item: `md_item_attachments` + `erp-item-attachments.*`. Pola: tabel generik + modul ber-guard + organism dropzone + section form. Salin untuk entitas lain. Bulk CSV/XLSX = follow-up. → § Item Lampiran
- **Lampiran transaksi = per-domain**: 4 tabel `<domain>_transaction_attachments` polymorphic + **satu** modul `erp-attachments` (`:domain/attachments/:docType/:docId`). FE `transaction-attachment-upload.tsx`, tab Lampiran per shared form (fin/sls/pur/inv). → § Lampiran Transaksi
- **Dimensi multi-select master** (Cabang/Gudang/Lokasi) → junction `md_<entity>_dim_*` + molecule `MultiLookupField` + loader `loadBranch/Warehouse/LocationOptions`. Sudah dipakai item & partner; **jangan fork**. → § Partner dimensi
- **Report Studio** (`/admin/report-designer`) = desainer laporan band-based; logika di `lib/report-studio/*` + organisms `components/organisms/report-studio/*`. Template via `lib/api/reports` (`templateJson`↔RsReport). **Pixel-faithful = pengecualian §2** (inline-style "designer surface", aksen tetap CSS var). → § Report Studio

---

## 3. Clean code & batas 400 baris (WAJIB)

Saat vibe coding di `opt/web-erp/**`, kode **harus clean code** — dan
**tidak boleh > 400 baris per file**. Ini menguatkan root `CLAUDE.md §5`,
khusus web-erp tanpa pengecualian.

- **Maks 400 baris/file source.** Sebelum sebuah file tembus batas → stop,
  pecah jadi modul lebih kecil dengan tanggung jawab tunggal.
- **Clean code:** named exports, satu tanggung jawab per modul/komponen/fungsi,
  nama deskriptif, tanpa duplikasi, tanpa dead code, tanpa magic value
  (lewat token/konstanta). Komponen UI besar dipecah per bagian.
- Berlaku untuk source aplikasi/bisnis (`.tsx/.jsx/.ts/.js`). **Bukan**
  target: Prisma (schema/migrations/generated), log, dan data
  seed/feed/mock/fixture — sejalan skill `ref-audit`.
- Setelah split/refactor → `npm run typecheck` (atau verifikasi prototype
  tetap jalan) sebelum declare selesai.
- **Enforcement otomatis** (sejak 2026-05-19):
  - ESLint `max-lines` (error, 400) di-scope ke `app/**`, `components/**`,
    `lib/**`, `shared/**` (exclude test/spec/seed/mock/fixture). Jalan via
    `npm run lint` & `npm run check`.
  - Script `npm run check:size` (`scripts/check-file-size.mjs`) sebagai gate
    independen — `npm run check` chain: lint → typecheck → check:size → test.
- Refactor besar → spawn sub-agent per file (root `CLAUDE.md §11`) agar
  context utama tidak meledak.

---

## 4. Setelah vibe coding: commit + merge ke `dev` (WAJIB)

Setiap selesai satu unit kerja vibe coding di `opt/web-erp/**`, **wajib**
commit lalu merge ke branch `dev` — jangan tinggalkan kerja menggantung di
working tree atau feature branch.

- **Commit** conventional & atomik (`feat:`/`fix:`/`refactor:`/`docs:`/
  `chore:`), pesan jelas, scope `web-erp` bila relevan. Jangan tumpuk
  ratusan baris dalam satu commit (root `CLAUDE.md §7`).
- **Merge ke `dev`:** kalau kerja di feature branch pada checkout aktif → merge atau
  fast-forward/rebase ke `dev`. Kalau memang sedang di `dev` → cukup commit
  (sudah "di dev"); jangan biarkan perubahan uncommitted.
- **DILARANG** `--no-verify`, amend commit yang sudah dipush, atau
  `git push --force` (root `CLAUDE.md §5`). Sinkronisasi pakai rebase/
  fast-forward.
- Dokumen `.md` web-erp harus sudah sinkron **sebelum** commit penutup
  (lihat aturan sinkronisasi dokumen di skill `erp`).
- Belum commit + (jika perlu) merge ke `dev` → task **belum** boleh
  dideklarasikan selesai.

---

## 5. Saat ragu

Tanya user. Aturan-aturan di atas tidak punya pengecualian diam-diam —
kalau ada kebutuhan menyimpang, eskalasi dulu.

## Worktree Policy (VPS-wide)

- **Do not use Git worktrees on this VPS.** Work directly in the active workspace/checkout.
- Do not create, enter, recommend, or require a worktree for any task, including background jobs.
- Use the current branch, or create a normal Git branch in the same checkout when isolation is needed.
