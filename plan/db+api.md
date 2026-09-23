# Senti ERP — Recap DB + API & Rencana Selanjutnya

> Terakhir diperbarui: 2026-05-20 (wave 4 — m2 finance skeleton + polish + tests)
> Status: DB ✅ · API ✅ · Frontend F2+F3 ✅ · F4 m2 finance skeleton ✅ · sidebar dinamis ✅ · Playwright smoke 24/24 ✅ · vitest 27/27 ✅ · UAT browser manual ⬜

## Wave 4 (2026-05-20)

- **m2 Finance MVP (skeleton CRUD, 5 entitas)**: NestJS modules
  `erp-fin-{journal-entries,ar-receipts,ap-payments,giros,ledger}` di
  api-gateway + 5 halaman frontend dengan route `/keuangan/*`. Journal
  Entries: master/detail (header + nested lines). Ledger: read-only. Seed
  `sys_menus` ditambah dua group baru `M2.TX` + `M2.RPT`. Semua controller
  pakai `ErpJwtAuthGuard` (CLAUDE.md §2.5). **Skeleton, bukan posting
  engine** — belum: validasi debit==credit, auto-posting, hitung saldo CoA,
  lookup partner/branch jadi Select, dedup `docNumber`/`giroNumber` jadi
  pesan rapi.
- **Polish & convention**: `ErpListLayout.onAdd` jadi optional (page
  read-only seperti Permissions tak butuh workaround); sidebar flyout
  `maxHeight` di-clamp; `SelectContent` z-index di atas modal; command
  palette di-derive dari role-filtered nav (CLAUDE.md §2.4); 16 lint debt
  react-compiler di `app-shell.tsx` + `use-erp-list.ts` dibereskan (extract
  `useAppShellTabs` hook, `useErpList` pakai `useReducer`); Menu Manager
  pakai DnD sibling-only via `@dnd-kit`.
- **Playwright smoke** (`apps/web-erp/scripts/erp-smoke.mjs`): tools
  intercept fetch prod → local api-gateway + inject Bearer token; navigasi
  via mutasi localStorage workspace state + reload. 24 route ERP →
  **24/24 OK** (login + render + listX called, 0 console error).
- **Vitest 27/27** (`apps/web-erp/__tests__/`): 24 smoke render-test per
  halaman + 3 interaction test (units create, menus create, journal-entries
  create dengan 2 lines). Mocking pola `vi.hoisted` + `vi.mock('@/lib/api/...')`.
  Helper `render-page.tsx` + `feedback-mock.ts` di `__tests__/helpers/`.
- Gate akhir: typecheck clean · lint **0 error** · check:size OK · next
  build sukses · Playwright smoke 24/24 · vitest 27/27.
- ⬜ Belum: UAT manual klik-klik browser dari user (overnight run agent
  hanya bisa otomasi); ⬜ `dist.root-owned.bak` di api-gateway perlu
  `sudo rm -rf` dari user; ⬜ root `npm install` masih diblokir
  `workspace:0.2.0` di `apps/open-design` — DnD agent terpaksa materialisasi
  `@dnd-kit/*` langsung ke `apps/web-erp/node_modules/`, deklarasi
  `package.json` sudah benar dan akan kembali nyambung saat root install
  pulih.

## Wave 3 (2026-05-19)

- **11 halaman sisa dibangun** (client + page, pola `units` / `item-categories`):
  Location, Warehouse, Tax, Payment Terms, Partner Category, Currency (+rate
  history), Chart of Accounts (tree), Permissions (read-only), Document
  Numbering (+generate next), Fiscal Periods (+open/close), Menu Manager
  (`sys_menus` CRUD + tree + reorder). Total halaman ERP fungsional = 19.
- **Keputusan terkunci — canonical route id = seeded `sys_menus.path`.**
  Sidebar dinamis (`fetchMyMenus` → `app-shell`) meng-emit `node.path` sebagai
  route id (mis. `/master/locations`, `/admin/fiscal-periods`). `renderRoute`
  kini memetakan path → komponen via registry `ERP_PAGES`; short-id legacy
  (`adm-users`, `md-items`) dipertahankan sebagai alias agar fallback `NAV`
  statis (saat API down) tetap jalan. `pageMeta` punya `ERP_ROUTE_META` agar
  breadcrumb/tab tidak menampilkan path mentah. Alasan: sys_menus jadi SSOT
  navigasi per-role; sebelumnya sidebar dinamis menghasilkan ComingSoon untuk
  semua karena mismatch path↔short-id.
- Verifikasi: `tsc --noEmit` clean, `next build` sukses, `check:size` OK
  (semua file ≤400 baris). Lint 16 error pre-existing di `app-shell.tsx` +
  `use-erp-list.ts` (react-compiler, file tak disentuh sesi ini) — bukan
  regресi wave 3. Smoke API: 19 endpoint `/api/erp/*` balas 200.
- ⬜ Belum: UAT manual di browser (`:3219`, login `admin`/`Admin123!`) —
  agent tidak bisa drive browser. Lint debt pre-existing perlu dibereskan
  terpisah.

---

## 1. Apa yang sudah selesai

### 1.1 Database — Prisma Schema + Migration

**Commit:** `02961c0` — `feat(web-erp): tambah Prisma models + migration ERP MVP m0+m1`

**36 tabel ERP** ditambahkan ke `apps/api-gateway/prisma/schema.prisma` (shared Postgres
dengan platform Althea). Semua model ber-prefix `Erp` di Prisma + `@@map("domain_table")`.

| Grup | Tabel | Keterangan |
|---|---|---|
| `sys_*` (8) | `sys_settings`, `sys_menus`, `sys_document_numberings`, `sys_fiscal_periods`, `sys_audit_logs`, `sys_notifications`, `sys_email_templates`, `sys_languages` | Konfigurasi sistem |
| `adm_*` (12) | `adm_users`, `adm_roles`, `adm_permissions`, `adm_user_roles`, `adm_role_permissions`, `adm_role_menus`, `adm_user_branch_access`, `adm_user_location_access`, `adm_user_warehouse_access`, `adm_user_sessions`, `adm_password_policies`, `adm_user_preferences` | Identity & access |
| `md_*` (16) | `md_branches`, `md_locations`, `md_warehouses`, `md_units`, `md_item_categories`, `md_items`, `md_partner_categories`, `md_partners`, `md_partner_addresses`, `md_partner_contacts`, `md_partner_bank_accounts`, `md_currencies`, `md_currency_rates`, `md_accounts`, `md_taxes`, `md_payment_terms` | Master data |

**Migration applied:** `apps/api-gateway/prisma/migrations/20260518_001_erp_mvp_m0_m1_init/migration.sql`  
**Enum:** 13 enum Prisma (`ErpUserLevel`, `ErpMenuType`, `ErpItemType`, `ErpAccountType`, dll)  
**PK:** BigInt `@default(autoincrement())` di semua tabel ERP  

**Seed dijalankan** (`prisma/seed-erp.ts`):

| Data | Isi |
|---|---|
| `sys_languages` | id (Indonesia), en (English) |
| `sys_settings` | 7 setting default (timezone, currency, decimal places, dll) |
| `sys_menus` | 28 menu entry M0+M1 |
| `sys_document_numberings` | 7 tipe dokumen (GRN, PO, SO, INV, JV, RCP, PAY) |
| `md_currencies` | IDR (base currency) |
| `md_branches` | 1 cabang HQ |
| `adm_password_policies` | 1 policy DEFAULT |
| `adm_permissions` | 34 permission (CRUD × resource) |
| `adm_roles` | 1 role SUPERADMIN |
| `adm_users` | 1 user admin (`admin` / `Admin123!`) |
| `adm_role_permissions` | 34 rows (SUPERADMIN dapat semua) |
| `adm_role_menus` | 28 rows (SUPERADMIN dapat semua menu) |

---

### 1.2 API — NestJS Modules

**Commit:** `9ad28a8` — `feat(api-gateway): tambah 20 ERP NestJS modules`  
**124 files, 7235 baris baru, typecheck bersih (0 error)**

Semua route di bawah prefix global `/api/erp/...`:

| Module | Route | Isi |
|---|---|---|
| `erp-auth` | `/api/erp/auth` | login, logout, GET me — JWT cookie `erp_token` |
| `erp-users` | `/api/erp/users` | CRUD `adm_users` |
| `erp-roles` | `/api/erp/roles` | CRUD `adm_roles` + assign perms + assign menus |
| `erp-permissions` | `/api/erp/permissions` | List `adm_permissions` |
| `erp-branches` | `/api/erp/branches` | CRUD `md_branches` |
| `erp-locations` | `/api/erp/locations` | CRUD `md_locations` |
| `erp-warehouses` | `/api/erp/warehouses` | CRUD `md_warehouses` |
| `erp-units` | `/api/erp/units` | CRUD `md_units` |
| `erp-item-categories` | `/api/erp/item-categories` | CRUD `md_item_categories` (tree) |
| `erp-items` | `/api/erp/items` | CRUD `md_items` |
| `erp-partner-categories` | `/api/erp/partner-categories` | CRUD `md_partner_categories` |
| `erp-partners` | `/api/erp/partners` | CRUD `md_partners` + nested address/contact/bank |
| `erp-currencies` | `/api/erp/currencies` | CRUD `md_currencies` + sub-resource rates |
| `erp-accounts` | `/api/erp/accounts` | CRUD `md_accounts` (CoA tree) |
| `erp-taxes` | `/api/erp/taxes` | CRUD `md_taxes` |
| `erp-payment-terms` | `/api/erp/payment-terms` | CRUD `md_payment_terms` |
| `erp-settings` | `/api/erp/settings` | GET + PATCH `sys_settings` |
| `erp-sys-menus` | `/api/erp/sys-menus` | CRUD `sys_menus` + GET tree |
| `erp-document-numberings` | `/api/erp/document-numberings` | CRUD + generate next number |
| `erp-fiscal-periods` | `/api/erp/fiscal-periods` | CRUD + open/close period |

**Konvensi berlaku:**
- Auth: `JwtAuthGuard` (shared dengan platform) — token dari `/api/erp/auth/login`
- BigInt IDs: diserialisasi jadi string di response (global `BigIntSerializerInterceptor`)
- Swagger: tersedia di `http://localhost:3203/api/docs` setelah server jalan
- Soft delete: `deletedAt` timestamp (bukan hard delete)
- Pagination: `{ success, data, meta: { page, limit, total, totalPages } }`

---

## 1.3 Frontend — Design System + Auth + F2 Admin + F3 Master Data

**Commits:** `b027825`, `7653ed1`, `4345dc0`, `8628f80` (Wave 1+2, 2026-05-18)

| Sub-fase | Status | Keterangan |
|---|---|---|
| F0 — Design tokens | ✅ | `styles/erp-tokens.css` — warna, tipografi, spacing, radius |
| F0 — Atoms | ✅ | `components/ui/` — button, input, select, badge, label, checkbox, tooltip, dll |
| F0 — Organisms | ✅ | sidebar, topbar, table, modal, confirm-dialog, tab-bar, dll |
| F0 — App shell | ✅ | `app-shell.tsx` dipecah 434→334 baris + `shell-route-renderer.tsx` |
| F0 — API client | ✅ | `lib/api/` — client, auth, types + 8 resource files (users, roles, branches, dll) |
| F1 — Login | ✅ | Terhubung ke `POST /api/erp/auth/login`, logout clear cookie, stale-session guard |
| F2 — Admin pages | ✅ | Users, Roles, Branches, Settings — CRUD real API |
| F3 — Master data | ✅ | Items, Units, Partners, Item Categories — CRUD real API |

**Infrastruktur baru:**
- `lib/use-erp-list.ts` — generic hook untuk list + CRUD + pagination
- `components/organisms/erp-list-layout.tsx` — shell reusable untuk halaman list ERP

---

## 2. Yang belum selesai / perlu verifikasi

| Item | Status | Catatan |
|---|---|---|
| Start api-gateway | ⚠️ Blocked | `dist/` dimiliki root — jalankan dulu: `sudo chown -R $USER:$USER apps/api-gateway/dist/` kemudian `cd apps/api-gateway && npm run dev` |
| Test login endpoint | ⬜ Pending | Setelah api-gateway jalan: `POST /api/erp/auth/login` body `{ login: "admin", password: "Admin123!" }` |
| Swagger verify | ⬜ Pending | Buka `http://localhost:3203/api/docs` setelah server jalan |
| Test halaman F2/F3 di browser | ⬜ Pending | web-erp jalan di `:3219`. Login → sidebar Setting (adm-users, dll) + Data Master (md-items, dll) |

---

## 3. Rencana selanjutnya — Frontend web-erp

DB dan API sudah siap. Langkah berikut adalah membangun frontend `apps/web-erp`.
Urutan ini mengikuti **CLAUDE.md §2** (design system dulu, baru slicing) +
**§2.1** (atomic design wajib).

### Fase F0 — Design System & Shell (WAJIB sebelum halaman apapun)

> Prerequisite: semua halaman dibangun dari token + komponen reusable ini.

**F0.1 — Design Tokens**
- Warna (primary, secondary, neutral, semantic: success/warning/error/info)
- Tipografi (font family, size scale, weight)
- Spacing scale (4px base grid)
- Border radius, shadow, breakpoint
- File: `apps/web-erp/src/styles/tokens.css` (atau Tailwind config extend)

**F0.2 — Atoms** (komponen terkecil, tanpa business logic)
- `Button` (variant: primary/secondary/ghost/danger, size: sm/md/lg, state: loading/disabled)
- `Input`, `Textarea` (dengan error state)
- `Select`, `MultiSelect`
- `Checkbox`, `Radio`, `Toggle`
- `Badge` (variant: status, level)
- `Icon` (wrapper Lucide/Heroicons)
- `Spinner` / `Skeleton`
- `Tooltip`

**F0.3 — Molecules**
- `FormField` (Label + Input + Error message)
- `SearchBox` (Input + icon + clear)
- `Pagination` (prev/next + page info)
- `ConfirmDialog` (modal konfirmasi sebelum aksi destruktif)
- `Toast` / `Alert` (success, warning, error, info)
- `DatePicker`, `DateRangePicker`

**F0.4 — Organisms**
- `DataTable` (kolom konfigurabel, sort, filter, pagination, row actions)
- `SidebarNav` (tree menu dari `sys_menus`, highlight aktif, collapse)
- `TopBar` (user avatar, notif, breadcrumb)
- `FormSection` (card + title + fields grid)
- `EmptyState`, `ErrorState`, `LoadingState`

**F0.5 — App Shell Template**
- Layout: `Sidebar (240px) + Main area`
- Topbar sticky
- Slot: `<Outlet />` untuk konten halaman
- Route guard: redirect ke `/erp/login` jika belum auth

---

### Fase F1 — Auth Pages

- `/erp/login` — form login → `POST /api/erp/auth/login` → set cookie → redirect dashboard
- `/erp/logout` — clear session
- Route guard: semua `/erp/*` (kecuali login) memerlukan `erp_token` valid

---

### Fase F2 — Dashboard M0 (Admin)

> Menggunakan design system F0. Satu halaman = satu page component.

| Halaman | Route | API |
|---|---|---|
| Dashboard utama | `/erp/dashboard` | stats ringkas (jumlah user, branch, item) |
| User Management | `/erp/admin/users` | `GET/POST/PATCH/DELETE /api/erp/users` |
| Role Management | `/erp/admin/roles` | CRUD roles + assign permissions + assign menus |
| Permission List | `/erp/admin/permissions` | `GET /api/erp/permissions` (readonly) |
| Branch | `/erp/admin/branches` | CRUD branches |
| Location | `/erp/admin/locations` | CRUD locations |
| Warehouse | `/erp/admin/warehouses` | CRUD warehouses |
| System Settings | `/erp/admin/settings` | `GET/PATCH /api/erp/settings` |
| Menu Config | `/erp/admin/menus` | CRUD `sys_menus` + drag-reorder tree |
| Document Numbering | `/erp/admin/doc-numberings` | CRUD + preview format |
| Fiscal Period | `/erp/admin/fiscal-periods` | CRUD + open/close actions |

---

### Fase F3 — Dashboard M1 (Master Data)

| Halaman | Route | API |
|---|---|---|
| Item | `/erp/master/items` | CRUD items + filter by type/category |
| Item Category | `/erp/master/item-categories` | CRUD + tree view |
| Unit of Measure | `/erp/master/units` | CRUD |
| Partner | `/erp/master/partners` | CRUD + nested tabs (address, contact, bank) |
| Partner Category | `/erp/master/partner-categories` | CRUD |
| Chart of Accounts | `/erp/master/accounts` | CRUD + tree CoA |
| Currency | `/erp/master/currencies` | CRUD + rate history |
| Tax | `/erp/master/taxes` | CRUD |
| Payment Terms | `/erp/master/payment-terms` | CRUD |

---

### Fase F4 (Post-MVP) — Modul Transaksional

Setelah F0–F3 solid, lanjut ke modul berikutnya sesuai roadmap:

| Prioritas | Modul | Domain DB | Commit prerequisite |
|---|---|---|---|
| 1 | Finance / GL | `fin_*` | Schema m2 sudah di db-design, perlu Prisma write |
| 2 | Inventory | `inv_*` | Schema m3 sudah di db-design |
| 3 | Purchasing | `pur_*` | Schema m4 sudah di db-design |
| 4 | Sales / AR | `sls_*` | Schema m5 sudah di db-design |
| 5 | Manufacturing | `mfg_*` | Schema m6 sudah di db-design |
| 6 | Fixed Assets | `fa_*` | Schema m7 sudah di db-design |
| 7 | POS | `pos_*` | Schema m12 sudah di db-design |
| 8 | Planning/MRP | `pln_*` | Schema pln sudah di db-design |
| 9 | BI/Dashboard | `bi_*` | Schema m8 belum dikatalog |

Setiap modul transaksional: **schema design review → Prisma write + migration → API → Frontend**.

---

## 4. Urutan kerja yang disarankan sekarang

```
1. Fix dist/ ownership → start api-gateway → test login       [~5 menit]
   sudo chown -R $USER:$USER apps/api-gateway/dist/
   cd apps/api-gateway && npm run dev

2. Test F2/F3 pages di browser (:3219) — confirm CRUD bekerja  [~30 menit]

3. Review + keputusan: polish F2/F3 atau mulai F4 (m2 Finance)
```

---

## 5. Referensi

| Dokumen | Path | Isi |
|---|---|---|
| DB Design (otoritatif) | `apps/web-erp/db-design/README.md` | Hub semua keputusan DB, ERD, enum catalog |
| Prisma Schema | `apps/api-gateway/prisma/schema.prisma` | Source of truth model DB |
| Migration ERP | `apps/api-gateway/prisma/migrations/20260518_001_erp_mvp_m0_m1_init/` | SQL migration yang sudah applied |
| Seed ERP | `apps/api-gateway/prisma/seed-erp.ts` | Data awal idempotent |
| API Swagger | `http://localhost:3203/api/docs` | Auto-generated dari kode (perlu server jalan) |
| Module Roadmap | `apps/web-erp/db-design/module-roadmap.md` | Peta domain m2–m12 |
| CLAUDE.md web-erp | `apps/web-erp/CLAUDE.md` | Aturan baku: naming, atomic design, 400 baris |
