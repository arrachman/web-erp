# Web-ERP — Aturan Baku untuk AI Agent (Claude)

Scope: **hanya** `apps/web-erp/**`. Berlaku di atas root `CLAUDE.md` repo
(tidak menggantikannya). Singkat, deklaratif, non-negosiabel.

Produk: **Senti ERP**. Legacy `apps/web-erp/preferensi/` = **referensi
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

Contoh benar:

| Entitas | Domain | Nama tabel |
| --- | --- | --- |
| User | adm | `adm_users` |
| Role | adm | `adm_roles` |
| Document Numbering | sys | `sys_document_numberings` |
| Item | md | `md_items` |
| Partner Address | md | `md_partner_addresses` |
| Account (CoA) | md | `md_accounts` |

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

> Dokumen desain DB otoritatif = `apps/web-erp/db-design/` (`README.md` hub +
> `entities-m0-administrator.md` + `entities-m1-master-data.md` +
> `entities-m2-finance.md` + `entities-m2-finance-enterprise.md` +
> `entities-m3-inventory.md` + `entities-m4-purchasing.md`
> + `entities-m5-sales.md` + `entities-m6-manufacturing.md` + `entities-m7-fixed-assets.md`
> + `entities-m12-pos.md` + `entities-pln-planning.md` + `legacy-mapping.md` + `module-roadmap.md`). MVP pakai
> `sys_`/`adm_`/`md_`; modul pasca-MVP (m2–m12) dipetakan di `module-roadmap.md`
> (decision §8 #14–17). **m2 `fin` + m3 `inv` + m4 `pur` + m5 `sls` + m6 `mfg`
> + m7 `fa` + m12 `pos` + `pln` (baru) sudah katalog field-level** (periode reuse
> `sys_fiscal_periods`; dimensi pakai master `md_*`; pembayaran AP/AR reuse
> `fin_ap_payments`/`fin_ar_receipts`; sale POS reuse `sls_invoices`).
> Sisa terakhir: m8 `bi`. Top-level
> `DB-DESIGN.md` lama sudah **dihapus** (redundan). **Semua open decision sudah
> RESOLVED dengan user (2026-05-17)** — log keputusan otoritatif di
> `db-design/README.md §8` (13 keputusan). Yang berubah dari draft: PK = **BigInt**,
> **audit-log (`sys_audit_logs`) masuk MVP**, **`CurrencyRate` bertanggal**,
> **`legacyCode` di tiap master**; `ErpUser` tetap terpisah dari User klinik.
> **MVP = 31 tabel** (14 `sys_*`/`adm_*` + 17 `md_*`). **PRISMA SUDAH DITULIS &
> DIMIGRASI (2026-05-18):** atas go-ahead user, seluruh katalog pasca-MVP
> diterjemahkan ke `apps/api-gateway/prisma/schema.prisma` + migrasi
> `20260518_003_erp_modules_fin_inv_pur_sls_mfg_fa_pos_pln` (additive: 156 tabel
> ERP `fin`/`inv`/`pur`/`sls`/`mfg`/`fa`/`pos`/`pln` + master GL-dim `md_*`, 53
> enum baru; 0 DROP, clinic/`m0_*`/`m1_*` aman). **Catatan desain:** referensi
> lintas-domain = scalar `BigInt` FK + `@@index` **tanpa** `@relation`/FK DB
> (domain decoupled); FK intra-domain ditegakkan. `inv_stock_balances` = derived
> view. `bi`/m8 **dikecualikan** — belum ada katalog field.

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

- Komponen baru → tentukan tingkatnya dulu; taruh di folder per tingkat
  (`atoms/`, `molecules/`, `organisms/`, `templates/`, `pages/` atau setara
  struktur prototype).
- Primitif yang di-reexport dari `@sentient-factory/ui-kit` tetap bagian dari
  design system ERP; Tailwind source web-ERP **wajib** mencakup
  `packages/ui-kit/src` agar utility class pada primitive (Select, Dropdown,
  Tooltip, Dialog, dll) ter-compile. Jika tidak, flyout/dropdown bisa tampil
  transparan karena `bg-popover`/`border-*` tidak pernah di-generate.
- **Dilarang** membangun tingkat lebih tinggi sebelum tingkat di bawahnya
  ada sebagai komponen reusable (page tidak menulis ulang atom/molecule
  ad-hoc).
- Atom/molecule **tanpa** business logic & **tanpa** style hardcode — hanya
  token + props. Logic naik ke organism/page.
- Konsisten dengan batas 400 baris (§3): komponen besar dipecah per tingkat,
  bukan jadi satu file gendut.

Konsekuensi:

- Kalau saat slicing butuh elemen UI yang belum ada di design system →
  **stop**, tambahkan dulu sebagai komponen reusable di tingkat atomic yang
  tepat, baru lanjut.
- Tidak ada style/warna/spacing hardcode di halaman; selalu lewat token.
- Konfirmasi ke user kalau scope design system belum jelas — jangan asal
  mulai halaman.

---

### 2.2 Penamaan file frontend (WAJIB)

- **Dilarang** prefix `erp-` pada nama file di `apps/web-erp/**` (mis.
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

**Setiap** halaman list master/transaksi di `apps/web-erp/**` **wajib**
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
- Bikin list page baru → mulai dari organism `erp-list-layout.tsx` + turunan
  `generic-list*` / `data-list*`. **Dilarang** start dari blank `<table>`.
- Butuh fitur list yang belum di organism (mis. grouping, pinned columns)
  → **stop**, tambahkan ke organism reusable dulu (§2.1 atomic design),
  baru pakai di halaman.
- Checklist di atas adalah **definition of done** untuk list page;
  declare selesai = semua poin A–G terpenuhi atau eskalasi alasan
  pengecualian ke user.

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

Detail implementasi yang melengkapi checklist §2.7. Semua poin ini **wajib
konsisten** di setiap halaman list — bukan opsional.

#### Row visual states (3 state, tidak boleh campur)

| State | Trigger | Visual |
| --- | --- | --- |
| **Normal** | Default | Latar default |
| **Hovered** | Cursor di atas row | Latar `--bg-hover` (subtle) |
| **Focused** (keyboard) | J/K navigation aktif | Border kiri 2px `--accent`, latar `--bg-focus` |
| **Selected** (checkbox) | Checkbox ✓ atau X keyboard | Latar `--bg-selected`, checkbox terisi |
| **Focused + Selected** | Keduanya aktif bersamaan | Gabung: border kiri + latar selected |

Row focused ≠ row selected. Navigasi J/K **tidak otomatis** men-select baris
— hanya memindahkan fokus visual. X / Space yang men-toggle selection. Ini
penting agar user bisa navigate tanpa sengaja memilih banyak baris.

#### Column alignment & format angka (standard)

| Tipe data | Alignment | Format |
| --- | --- | --- |
| Teks (kode, nama, kota, NPWP) | Kiri | — |
| Numerik (uang, qty, %) | **Kanan** + `tabular-nums` | `46.666.000,00` |
| Badge / status | Kiri (dalam cell) | `● Approved` |
| Tombol aksi inline | Kanan | `Edit Hapus` |
| Checkbox | Tengah | ☐ |

Format angka Rupiah: `Intl.NumberFormat('id-ID', { minimumFractionDigits: 2 })`
— ribuan titik, desimal koma, 2 digit desimal. **Tidak ada simbol Rp di kolom
tabel** (hanya di label header atau tooltip bila perlu). Implementasi sekali di
helper `lib/format.ts`, dipakai oleh semua halaman — tidak boleh inline per
halaman.

#### Tinggi baris/tabel = density token (WAJIB, 2026-05-20)

Tabel list **wajib** mengikuti knob `density` di Setting → Tampilan
(`compact` / `comfortable`). Implementasi sudah ada di
[`components/organisms/table.tsx`](components/organisms/table.tsx):

- `TableHead` pakai `h-[var(--header-h)]` → 28px compact / 34px comfortable.
- `TableCell` pakai `h-[var(--row-h)]` → 28px compact / 36px comfortable.
- Token didefinisikan di [`styles/erp-tokens.css`](styles/erp-tokens.css)
  pada selector `[data-density='compact']` / `[data-density='comfortable']`
  (data-attribute di-set di `<html>`).

App-shell (`components/templates/app-shell.tsx`) hydrate `data-density`
(juga `data-fontscale`/`data-sidebar`/`data-primary`) saat mount: pertama
dari localStorage `erp-appearance`, lalu di-override server SSOT lewat
`getMyPreferences()` (metadata Json). **DILARANG** hardcode `data-density`
ke literal di mount effect — dulu force-set `compact` membuat preferensi
user tidak persist saat reload. Pengubahan density di AppearancePage
otomatis apply ke semua tabel tanpa remount karena listener CSS variable.

Saat membuat halaman list baru: cukup pakai organism `Table*` dari
`components/organisms/table.tsx` — density mengikuti otomatis.

#### Approval status — token color mapping (WAJIB, jangan hardcode)

| Status value | Badge variant | Label tampil |
| --- | --- | --- |
| `DRAFT` | `default` (abu-abu) | Draft |
| `NEED_APPROVE` | `warning` (oranye) | Need Approve |
| `APPROVED` | `success` (hijau) | Approved |
| `REJECTED` | `danger` (merah) | Rejected |
| `POSTED` | `info` (biru) | Posted |

Mapping dipetakan **sekali** di `lib/status.ts` (fungsi `statusBadgeVariant` /
`statusLabel`). Setiap halaman import fungsi ini — **tidak boleh** ada
switch/if per halaman yang mendefinisikan warna sendiri.

#### H. Bulk action toolbar (WAJIB bila entitas punya operasi batch)

Tampil di atas tabel **hanya saat ≥ 1 baris dipilih**; hilang bila selection
kembali ke 0. Animasi slide-in (jangan langsung muncul tanpa transisi).

- Teks: `X baris dipilih` (X = jumlah selection)
- Tombol aksi batch sesuai entitas (contoh: Aktifkan, Nonaktifkan, Hapus)
- Tombol **Batal pilihan** di kanan — clear semua selection
- Aksi destruktif batch → **wajib** confirmation dialog sebelum eksekusi
- Setelah batch selesai → reload list + clear selection + toast notifikasi

Implementasi lewat slot `toolbar` di `ErpListLayout` atau organism
`bulk-action-bar.tsx`. **Dilarang** inline ad-hoc per halaman.

#### Post-action feedback (toast/notifikasi)

Setiap operasi CRUD dan batch **wajib** memberikan feedback via `notify()`:

| Operasi | Variant | Contoh pesan |
| --- | --- | --- |
| Create sukses | `success` | `"Customer dibuat"` |
| Update sukses | `success` | `"Customer diperbarui"` |
| Delete sukses | `success` | `"Customer dihapus"` |
| Batch sukses | `success` | `"3 customer diaktifkan"` |
| Error API | `danger` | Pesan dari `error.message` |
| Fitur belum tersedia | `warn` | `"Export belum tersedia"` |

Tidak ada operasi **silent** (tanpa feedback) — user harus selalu tahu apakah
aksi berhasil atau gagal. Toast error harus meneruskan pesan asli dari API,
bukan pesan generik "Terjadi kesalahan".

#### Confirmation dialog untuk aksi destruktif

Setiap aksi yang **tidak bisa di-undo** (hapus, batch-hapus, reject) **wajib**
menampilkan dialog konfirmasi via `confirmAction()` sebelum eksekusi:

- **Title**: `Hapus <Entitas>?`
- **Pesan single**: `<kode> — <nama> akan dihapus permanen.`
- **Pesan batch**: `X <entitas> akan dihapus permanen.`
- **Tombol confirm**: variant `danger`, label eksplisit (`Hapus`, bukan `OK`)
- **Tombol batal**: ghost/secondary

#### Empty / loading / error state

| State | Tampilan |
| --- | --- |
| **Loading** | Teks `Memuat...` di tengah area tabel |
| **Error** | Teks merah `Gagal memuat data: <pesan>` di atas tabel |
| **Empty (no data)** | `TableEmpty` colspan penuh: `Tidak ada data` |
| **Empty (filtered)** | `TableEmpty`: `Tidak ada hasil untuk filter ini` |

`TableEmpty` (`components/organisms/table.tsx`) adalah komponen standar untuk
semua empty state tabel — **jangan** biarkan `<tbody>` kosong tanpa keterangan.
Empty saat filter aktif harus dibedakan dari empty tanpa filter (pesan beda).

**Error state = molecule `ErrorState` (2026-05-20).** Pesan error mentah dari
backend (mis. HTTP `Not Found`, `Failed to fetch`, `Unauthorized`) **dilarang**
ditampilkan apa adanya. `ErpListLayout` me-render
[`components/molecules/error-state.tsx`](components/molecules/error-state.tsx)
yang mengkategorikan pesan jadi: tidak terhubung / data tidak ditemukan /
akses ditolak / server bermasalah / fallback — masing-masing dengan ikon,
judul, deskripsi user-friendly, dan tombol **Coba lagi** (memanggil
`onRefresh`). Saat membuat halaman list baru: cukup oper `error` dari
`useErpList` ke `ErpListLayout` — jangan rakit teks error ad-hoc.

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

**Setiap** list page **wajib** mengirim `page`, `limit`, `search`, `sortBy`,
`sortDir`, (+`isActive` bila ada filter status) ke API. **DILARANG** filter
atau slice di klien atas data hasil API — backend default `limit=10`, kalau
FE pakai client-side filter → user cuma lihat 10 baris pertama walau footer
bilang "Tampilkan 25".

Bug history (2026-05-20): branches page hanya menampilkan 10 baris walau
seed 500 dummy ada — penyebab: `listBranches({ sortBy, sortDir })` tidak
kirim `limit`, backend default 10, FE lalu `rows.filter(...).slice(page-1,
page)`. Quick fix-nya menaikkan limit hanya menunda masalah; refactor
proper diterapkan ke semua list page.

Bug history (2026-05-20, lanjutan): FE pakai server-driven pagination tapi
DTO `warehouses`/`partners`/`locations` belum punya `sortBy`/`sortDir` →
`ValidationPipe({ forbidNonWhitelisted: true })` lempar 400
`"property sortBy should not exist"`. Fix: tambah `sortBy`/`sortDir`
(whitelist `IsIn(SORTABLE_FIELDS)`) ke ketiga DTO + wire `orderBy: [{
[sortBy]: sortDir }]` di service. Setiap kali nambah list page baru, sync
DTO query dulu sebelum FE diarahkan ke server-side sort.

**Pola kanonik** (lihat [components/pages/branches-page.tsx](components/pages/branches-page.tsx)):

```tsx
const [sortBy, setSortBy] = useState('createdAt');
const [sortDir, setSortDir] = useState<'asc'|'desc'>('desc');
const [search, setSearch] = useState('');
const [statusFilter, setStatusFilter] = useState('active'); // default: tampilkan aktif saja
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
  () => listBranches({
    page, limit: pageSize, search: debouncedSearch || undefined,
    sortBy, sortDir, isActive: isActiveParam,
  }),
  [page, pageSize, debouncedSearch, sortBy, sortDir, isActiveParam],
);

// Reset page 1 saat filter/search/sort/pageSize berubah
useEffect(() => { setPage(1); }, [debouncedSearch, statusFilter, sortBy, sortDir, pageSize]);

const paged = rows;  // server sudah paginasi
const totalRows = meta?.total ?? 0;
const pageCount = meta?.totalPages ?? 1;
```

Aturan turunan:

- `useErpList` (`lib/use-erp-list.ts`) **harus** dipanggil dengan
  **deps array** kedua — fetcher closure di-cache via ref, hanya deps yang
  trigger refetch.
- `meta.total`/`meta.totalPages` dari backend = SSOT untuk pagination footer.
- **Dilarang** memo `filtered = rows.filter(...)` di list page lagi —
  filter ke backend.
- Backend DTO **wajib** support minimal `page`, `limit`, `sortBy`, `sortDir`
  + `search` (bila ada kolom teks). Filter status (`isActive`) bila entitas
  punya. Kalau DTO belum lengkap → tambahkan dulu di
  `apps/api-gateway/src/erp-<feature>/dto/query-*.dto.ts` sebelum FE
  menggantungkan diri ke server-side.
- Pengecualian sah saat ini (DTO memang tidak paginasi, list selalu kecil):
  `settings`, `fiscal-periods`, `menus`, `permissions` (enum-like). Kalau
  list-nya tumbuh, tambahkan paginasi backend dulu.

---

### Aturan turunan lintas-fitur (ringkas — detail di `DECISIONS.md`)

Rule yang lahir dari keputusan per-fitur **tapi berlaku saat membangun apa pun
yang baru**. Satu baris di sini = index; detail + rasional ada di section ber-`§`
di [`DECISIONS.md`](DECISIONS.md).

- **Halaman master `code+name+isActive`** → pakai organism `SimpleMasterPage`; jangan fork. Validation standard WAJIB (`validate` prop, `aria-invalid`, `error` prop, auto-focus error pertama). → §2.15/§2.16
- **Tree hierarkis + DnD** (CoA tree, kategori berjenjang, menu) → reuse `TreeDndMasterPage`; jangan bikin organism tree baru. → §2.22
- **Tab strip / sortable list** → `@dnd-kit/core`+`sortable`; dilarang HTML5 DnD / react-beautiful-dnd. → §2.14
- **Master `code`** = bare semantic, **tanpa** prefix entity-scope (`CAT-`/`BRD-`/`UNT-`). → §2.27
- **Search list endpoint** = `code` exact-match (insensitive) + `name` `contains`; dilarang `code: { contains }`. SearchSelect: exact-code auto-pick saat commit. → §2.29/§2.30
- **`SearchSelect` modal** = stale-while-loading saat ganti halaman (jangan replace `<tbody>` dgn loader row); mode multi: **Enter = submit tombol Pilih**, Space/klik = toggle baris. → §2.28
- **Input numerik** = `<NumInput>` (bukan `<Input type=number>`); **display angka** = `formatNumber/formatRupiah/formatQty` dari `lib/format.ts`. → §2.31
- **Input tanggal** = `<DateInput>` (bukan `<Input type=date>` mentah; placeholder "Pilih tanggal", popover day-picker dgn caption bulan/tahun = dropdown `captionLayout="dropdown"`, rentang dari `calendarNavBounds()`); **display tanggal** = `formatDate()` dari `lib/date-format.ts` (format dinamis dari `sys_settings`). Pengecualian: grid-cell editor & date-range-picker. → §2.39
- **Format kode akun & angka** = dinamis dari `sys_settings` (bukan hardcode locale); account-code = lock-after-data. → §2.24/§2.31
- **Enum business-logic** (≥3 nilai, konsekuensi sistem beda) → info-icon di label + Radix Popover comparison. → §2.26
- **Migrasi ERP** = hand-written SQL + `prisma migrate deploy` (bukan `migrate dev`) + `prisma generate` **di dalam container** lalu restart. → §2.32/§2.34
- **Preferensi user** (theme/lang/density/font/sidebar/primary) → tabel `adm_user_preferences`; 3 bahasa UI `id/en/ja`. → §2.13
- **Command palette & sidebar** = derived dari `sys_menus` role-filtered (`my-menus`); dilarang hardcode menu list. SSOT seed menu = `prisma/seed-erp.ts` (jangan seed ERP menu di `seed.ts`). → §2.4/§2.17
- **Mode URL routing off/on** (`urlRoutingEnabled`) → ganti mode wajib `confirmAction`. → §2.19
- **URL form transaksi** → sub-route `<base>/new` (create) & `<base>/:id` (edit) dari canonical list path; route = SSOT list-vs-form (tanpa state `mode`). Reuse `lib/trx-route.ts` + registry `TRX_FORM_PAGES`; jangan fork skema URL. → §2.3.1
- **Layout form input transaksi** → kolom **kanan-atas** urutan baku: **Tanggal → No Transaksi → Uang/Kurs**. Kurs read-only inline di sebelah mata uang; `No Transaksi` + checkbox `Auto` satu baris. Field identitas (partner/akun/uraian) di kiri, dimensi (cabang/lokasi) di tengah. Label rata kiri, asterisk required di belakang teks. Berlaku semua form transaksi (CR/CD/BD/giro/jurnal). → §2.36 (DECISIONS.md)
- **Transaksi kas/bank (CR/CD/BD)** → backend **shared** `erp-fin-cash-bank-transactions` (enum `direction`, `docNumber` auto + `fiscalPeriodId` diturunkan dari tanggal, posting GL balanced saat POST, state machine §2.7). Baris kontra = organism `cash-bank-lines.tsx` (**satu kolom Total**, bukan debit/kredit ala jurnal umum). Status read-only (badge) + transisi via aksi. → § Kas Masuk (DECISIONS.md)
- **Status dokumen transaksi** = enum `ErpDocumentStatus` 7-nilai (`DRAFT/NEED_APPROVE/APPROVED/REJECTED/POSTED/VOID/CANCELLED`), sejalan `lib/status.ts`; jangan reintroduce varian 4-nilai lama. → § Kas Masuk (DECISIONS.md)
- **Filter list transaksi** → **1 baris**: kontrol filter masuk slot `toolbar` `ErpListLayout` (gabung baris summary `Σ`). Inline = Status + Tanggal live; tombol **Filter** (badge jumlah filter lanjutan aktif) buka **drawer kanan** (staged draft → "Terapkan"); + Reset saat aktif. Tanpa chip terpisah. Drawer = organism reusable `components/organisms/drawer.tsx`; jangan rakit slide-over ad-hoc. → §2.40
- **Tipe Partner** = master data `md_partner_types` (`code+name+isActive`; `kind` **derived dari code**, bukan input form: `CUST`→`CUSTOMER`, `SUP`→`SUPPLIER`, `SLS`→`SALESMAN`, lain→`GENERAL`) + FK `md_partners.partner_type_id`; **jangan** reintroduce boolean `isCustomer/isSupplier/isSalesman`. Lookup Customer/Supplier/Salesman filter via `partnerType.kind` / query `typeKind`; `md_partner_categories` tetap kategori/sub-segmen terpisah (`salesTier`). → DECISIONS.md "Master Data Partner — Tipe Partner sebagai master data"
- **Master atribut item** (lookup spt Nozzle/OEM) → mirror `md_colors` (code+name+isActive) + FK di `md_items` + modul backend (guard `ErpJwtAuthGuard`) + seed `seed-erp.ts` + daftar di `ERP_PAGES`/`NAV`/`ERP_ROUTE_META`. Reuse master existing (Warna/Merk/Ukuran/Material/Section/Desainer); Vendor→`md_partners`, Satuan Lapangan→`md_units`. **Jangan** bikin tabel atribut generik. → §2.35
- **Sumber lookup (Form Builder & Kustomisasi Grid)** = slug kanonik registry `lib/lookup-source-registry.ts` (`LOOKUP_SOURCE_OPTIONS`, 14 sumber: ditambah `items`/`units`/`taxes`/`payment-terms`). Grid live resolve loader/label via `lib/grid-lookup-loaders.ts` (slug lama `account`/`costCenter`/… di-alias). **Jangan** bikin kosakata slug baru. → § Kustomisasi Grid (DECISIONS.md)
- **Field settings kolom grid (Kustomisasi Grid)** = **`GridColumnSettings`** gear di setiap kolom → dialog Placeholder + Nilai default (type-aware) + Lookup config (hanya tipe lookup). DB: `sys_transaction_grid_columns.placeholder/default_value/default_value_label`. Live grid: `applyColumnDefaults` (baris baru di `useGridNav`) + `useSeedLineDefaults` (baris awal saat form buka). Mirror `FieldSettingsPopover` Form Builder. → § Kustomisasi Grid (DECISIONS.md)
- **Form Builder field** → konfigurasi per-field di `sys_form_fields`: label/tipe/visible/wajib/slot/urutan + (lookup) sumber/filter/sort + **placeholder/`defaultValue`/`isReadonly`**. UI = **satu** `FieldSettingsPopover` (gear, semua tipe) yang me-render `LookupConfigSection` untuk tipe lookup; read-only = `BooleanRadio`. Form konsumsi: `ph()`/`ro()` + `formDefaultsPatch()` (isi default sekali saat record baru, hanya field kosong). → § Form Builder (DECISIONS.md)
- **Header form transaksi = render 100% dari config** (no hardcoded `<Field>`): satu loop config-ordered per slot (`slotFields` dari `useFormFields`), dispatch struktural→`CashBankStructuralField` (switch fieldKey = binding ke kolom DB, **tetap di source**) vs custom→`CashBankCustomField`; baris bersama `FormFieldRow`; fallback `DEFAULT_FORM_FIELDS`. **Tipe field sistem di-GUARD** (Select disabled di builder — terikat kolom DB+posting GL); label/sumber/slot/urutan/visible/wajib/placeholder/default/readonly editable. Custom = bebas penuh. → § Header form transaksi render dinamis (DECISIONS.md)
- **Config transaksi baru = baseline version-controlled** (BUKAN live-DB-only seperti kurasi cash/bank): grid kolom + `lineTable` per famili di `seed-erp-transaction-grids.ts` (`GridFamily`: cashbank/journal/giro/giroClearing/inv*), header default per kode di `DEFAULTS_BY_CODE` (`erp-form-fields.service.ts`). Famili tanpa kolom kurasi → catalog + grid `main` kosong (toleran). Finance non-kas/bank (GJ/AJ/JM/RV/BB jurnal · RG/SG giro · RGC/SGC clearing) sudah di-setup config-only — **giro = grid instrumen (`fin_giros`), bukan baris jurnal Debit/Kredit**; halaman belum config-driven (follow-up). → § Setup config Form Builder + Kustomisasi Grid (DECISIONS.md)
- **Grid baris transaksi = mesin generik model-agnostik** (`grid-line-core.ts` `GridModel<Row>` + `use-grid-nav.ts` `useGridNav<Row>` + `LineCell` row-agnostic). Transaksi baru → bikin `GridModel<Row>` + organism konsumen; **jangan fork** mesin nav/cell. Cash/bank = `cashBankGridModel`; sales item-based = `slsItemGridModel`. → § Sales Order (DECISIONS.md)
- **Transaksi sales item-based (SO/SI/DO/…)** → pola persis cash/bank tapi baris = item (Item·Qty·Satuan·Harga·Disc·Pajak·Total): shared `sales-transaction-form.tsx` (header config-driven Form Builder) + `sls-item-lines.tsx` (grid config-driven Kustomisasi Grid) + wrapper tipis per-dokumen + backend `erp-sls-*`. **SO tidak posting GL** (komitmen, bukan peristiwa finansial; SI yang posting AR/revenue). 16 type `SLS.*` terdaftar di katalog; form kerja penuh baru **Sales Order** (`/sales/orders`, code `SLS.SO`). → § Sales Order (DECISIONS.md)
- **Biaya item (tab Harga)** = system-managed dari pembelian: `purchasePrice` (Harga Beli Terakhir) + `lastHpp` (HPP Terakhir, kolom baru) di-stamp saat GRN POST (`PurGoodsReceiptPostingService`); `averageCost` seed bila 0 dan tetap data internal, tapi **tidak ditampilkan** di form (keputusan user 2026-07-11). Field biaya yang visible di tab Harga hanya `purchasePrice` + `lastHpp` (read-only). "HPP Update" (`standardCost`) **dihapus** dari UI. Diskon Pembelian item → **default baris PR/PO/RI/PRT** (editable, via `getItemForPurchaseAutoFill`). Pajak item punya 4 slot: **Pajak Beli 1/2** + **Pajak Jual 1/2** (`purchaseTaxId`/`purchaseTax2Id`/`saleTaxId`/`saleTax2Id`; slot 2 nullable additive, 2026-07-11). Tingkat Harga/Diskon Jual **dinamis/unlimited** (item baru mulai 1 tier, tambah/hapus hanya di akhir — keputusan user 2026-06-13) dipilih `md_partner_categories.sales_tier` (kategori pelanggan) → auto-fill harga+diskon baris jual. → § Item Harga tab (DECISIONS.md §2.32)
- **Media item** = galeri `md_item_media` (max 8 gambar + 1 video, satu primary); file di `uploads/erp-items/` (gitignored), stream via endpoint ber-guard (bukan static publik); upload multipart pakai `apiUpload()` + organism `item-media-upload.tsx`; section form "Media" butuh item tersimpan (`ItemFormData.id`). → § Item Media (DECISIONS.md)
- **Lampiran file (attachment)** = **per-modul** (keputusan user 2026-06-13: bukan `sys_attachments` generik). Pilot = Item: tabel `md_item_attachments` (`ErpItemAttachment`, generik tanpa kind/primary), modul `erp-item-attachments.*` ber-guard `ErpJwtAuthGuard` (`/erp/items/:itemId/attachments`, stream `Content-Disposition: inline`), whitelist mime (PDF/gambar/Office/CSV/teks/ZIP) max 20×10MB, file di `uploads/erp-items/` (`att-<itemId>-<uuid>`). FE: API di `lib/api/items.ts` + organism `item-attachment-upload.tsx` (dropzone + catatan editable + unduh/hapus) + section form **Lampiran** (butuh `ItemFormData.id`). Replikasi entitas lain = salin pola. **Impor file bulk CSV/XLSX = fase berikutnya (belum dikerjakan).** → § Item Lampiran (DECISIONS.md)
- **Lampiran transaksi (fin/inv/pur/sls)** = **per-domain** (keputusan user 2026-06-13): 4 tabel generik `<domain>_transaction_attachments` berkunci `(doc_type, doc_id)` (polymorphic, tanpa FK). **Satu** module backend `erp-attachments` (controller `erp/:domain/attachments/:docType/:docId`, guard `ErpJwtAuthGuard`, service `delegate(domain)`), whitelist mime max 30×10MB, file di `uploads/erp-transactions/`. FE: `lib/api/transaction-attachments.ts` + organism `transaction-attachment-upload.tsx` (props domain/docType/docId), tab **Lampiran** dipasang sekali per shared form (`docType=transactionCode`, `docId=data.id`): cash-bank/journal/giro (fin), sales (sls), purchase (pur), inv-stock-adjustment/count/movement/opening/daily-check (inv). Form bespoke / pembayaran AP-AR = follow-up (backend siap). → § Lampiran Transaksi (DECISIONS.md)
- **Dimensi multi-select master (Cabang/Gudang/Lokasi)** → pola junction `md_<entity>_dim_branches/_warehouses/_locations` + reuse molecule `MultiLookupField` (`items-form-parts.tsx`) & loader `loadBranchOptions/loadWarehouseOptions/loadLocationOptions` (`items-form-lookups.ts`); kolom tunggal lama (mis. `branch_id`) dipertahankan sebagai fallback = id pertama. Sudah dipakai **item** (`md_item_dim_*`) & **partner** (`md_partner_dim_*`, 2026-06-13). **Jangan** fork komponen/skema dim baru. → § Partner dimensi multi-select (DECISIONS.md)
- **Report Studio** (route `/admin/report-designer`) = desainer laporan band-based ribbon-style, port fungsional penuh dari `Report-Designer-ERP-Profesional/ReportStudio.dc.html`. Logika di `lib/report-studio/*`, controller hook + `vals/*` (port `renderVals`) + organisms `components/organisms/report-studio/*`. **Pixel-faithful = pengecualian §2** (inline-style "designer surface", tema/aksen tetap lewat CSS var palette); helper `s()` parse CSS string→style, `Hov` utk hover/focus. Template di-list/load/simpan dari `lib/api/reports` (`templateJson`↔RsReport via `lib/report-studio/template-io.ts`; legacy/empty→starter layout per-module; tombol Simpan); rows preview masih sample in-file (`executeSqlQuery` utk lanjutan). Cluster designer lama (report-designer*/report-designer-mock/report-api) **dihapus**. → § Report Studio (DECISIONS.md)

---

## 3. Clean code & batas 400 baris (WAJIB)

Saat vibe coding di `apps/web-erp/**`, kode **harus clean code** — dan
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

Setiap selesai satu unit kerja vibe coding di `apps/web-erp/**`, **wajib**
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
