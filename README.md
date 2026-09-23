# Sentient ERP — `web-erp`

Senti ERP. **Next.js 16 App Router + React 19**, stack mirror
`apps/web-althea` (Tailwind v4, shadcn/radix primitives, TanStack
Query/Table, react-hook-form + zod, Vitest). Anggota npm workspace
(`name: web-erp`).

## Keputusan (2026-05-17, dengan user)

> **Reversal keputusan terkunci sebelumnya.** Frontend TIDAK lagi
> "lanjutkan prototype standalone CDN-React". Sekarang: **app Next.js
> penuh di `apps/web-erp/`**, stack disamakan dengan web-althea.
> `prototype/` turun status jadi **referensi desain saja** (sumber port
> design system + shell). `preferensi/` tetap referensi legacy MyERP+.
> `db-design/` & `DB-DESIGN.md` tetap dokumen desain DB.

## Menjalankan (mode dev)

Port **3219** (`config/ports.json → apps.web-erp`, env `WEB_ERP_PORT`).
Dev server bind `0.0.0.0` (LAN) — butuh UFW dibuka (lihat root `CLAUDE.md §4.1`).

```bash
cd apps/web-erp
npm run dev          # next dev (Turbopack) :3219
npm run typecheck    # tsc --noEmit
npm run lint
npm run test
```

### Akses (semua URL berikut mengarah ke instance web-erp yang sama)

| URL | Konteks |
| --- | --- |
| `https://erp.fr-labs.my.id/` | Publik via domain (reverse proxy → port 3219) |
| `http://192.168.1.150:3219/` | LAN (host-ip langsung) |
| `http://202.59.200.26:3219/` | Public IP langsung |
| `http://localhost:3219/` | Lokal di host |

Keempatnya **satu aplikasi yang sama** (web-erp, port `3219`) — domain
`erp.fr-labs.my.id` hanyalah reverse-proxy/DNS di depan port 3219. Bukan
environment terpisah.

## Struktur

**Atomic design (hybrid — keputusan 2026-05-17, dengan user):**
`components/ui/` dipertahankan sebagai layer **atoms + molecules**
(konvensi shadcn/web-althea flat, tetap), ditambah `organisms/` &
`templates/`. Import langsung (tanpa barrel/index). `app/` (route Next.js)
= layer **pages**.

```
app/                  # App Router = layer "pages" (layout.tsx, page.tsx → AppShell)
components/ui/         # atoms + molecules (button/input/card/select/tabs/charts/…)
components/organisms/  # sidebar topbar tab-bar command-palette table modal filter-chips
components/templates/  # app-shell (shell layout + renderRoute dispatcher)
components/pages/       # halaman ter-port: dashboard, statistik, settings, appearance,
                       #   kas-masuk-list, generic-list, financial-report, data-list,
                       #   buku-besar, coming-soon, quick-action (+ *-parts split <400)
shared/providers/     # TanStack Query provider
styles/               # globals.css + erp-tokens.css + erp-components.css (port dari prototype/styles.css)
lib/                  # utils (cn), mock (port data.jsx), nav (model menu shell),
                       #   registry (REGISTRY/REPORTS/MODULES + generator data-driven),
                       #   feedback (notify/bulkAction → sonner), tab-context (useTabKey)
prototype/            # REFERENSI desain (CDN-React SPA) — bukan runtime
preferensi/           # REFERENSI legacy MyERP+ — read-only
db-design/, DB-DESIGN.md  # desain DB (dokumen)
```

`prototype/`, `preferensi/`, `db-design/` di-exclude dari tsconfig,
eslint, dan Tailwind v4 `@source` (globals.css pakai `source(none)` +
daftar source eksplisit — auto-crawl `preferensi/` 1.1 GB membekukan
compile).

## Status

Skeleton + design system ter-port (tokens, dark + 9 palette switcher,
primitif, app shell, dashboard).

**Batch data-driven core ter-port (2026-05-17, dengan user):** seluruh
item sidebar fungsional via `renderRoute` di `app-shell` —
`DataList` (REGISTRY: master data + dokumen inv/pur/sls/prd/fa, ~26 modul),
`GenericList` (MODULES: transaksi keuangan), `FinancialReport` (REPORTS),
`Statistik`, `SettingsPage`, `AppearancePage` (theme via next-themes +
data-* attr, tanpa global tweak store), `KasMasukList`, `BukuBesar`.
Data layer = `lib/registry.ts` (generator deterministik, mock).

**Batch interaksi + forms ter-port (2026-05-18, multi-agent paralel):**

- **Login** (`pages/login.tsx`) — gate UI-only, demo akun `adi.s/sentient`,
  state user disimpan di `localStorage['erp-user']` lewat shell (bukan
  NextAuth/JWT — keputusan UI-only).
- **Modal picker** (`organisms/lookup-modal.tsx`, `organisms/contact-picker*.tsx`)
  — LookupModal generik (kind `coa`/`lokasi`/`cc`), ContactPicker di-split
  4 file sibling tier-Organism (`contact-picker.tsx` orchestrator +
  `-data.tsx` IIFE seed + `-row.tsx` + `-preview.tsx`) karena single-file
  >400 baris.
- **Drawer slide-over** (`organisms/notification-drawer.tsx`,
  `organisms/activity-drawer.tsx`, `molecules/drawer-panel.tsx`) —
  listen `toggle-notif`/`toggle-activity` (dispatch dari topbar), drawer
  notifikasi re-broadcast `notif-count` ke topbar badge. Tanpa
  `window.__overlay` global; state lokal + ESC handler.
- **Confirm dialog** (`organisms/confirm-dialog.tsx`) — host mount sekali
  di shell, listen `app-confirm` CustomEvent. Types & API imperatif
  (`confirmAction`, refactor `bulkAction`) di `lib/feedback.ts`. Caller
  existing (`bulkAction(kind, count, clearSel)`) tetap kompat —
  parameter `items?` opsional ditambah.
- **Forms** (`pages/record-form.tsx`, `pages/trx-form*.tsx`) — RecordForm
  generik untuk master/dokumen di REGISTRY. TrxForm 7-file split
  (orchestrator + config + header + fields + summary + tabs + lines) =
  Pages tier, semua ≤400 baris (§3). Routing `<route>-new`: kalau
  `route ∈ MODULES` → `TrxForm`; kalau `route ∈ REGISTRY` → `RecordForm`;
  else `ComingSoon`.
- **Shared hook** `lib/drawer-toggle.ts` (`usePanelToggle(eventName)`).

## Catatan setup

- SWC native (`@next/swc-linux-x64-gnu`) di-install isolated ke
  `apps/web-erp/node_modules` — Turbopack tak jalan di WASM fallback.
- `npm install -w web-erp` gagal (`workspace:` protocol di paket lain
  monorepo); pakai install isolated di dir app bila perlu re-install.
- `config/ports.json` di-deny harness untuk Edit; port di-set via
  `node scripts/port-manager.js update web-erp 3219`. Field `name`/
  `type` entri masih "Web ERP (Prototype)"/"static" (stale, port benar).
