# Senti Frontend Design System (SSOT)

> **Status:** Canonical standard for every Senti product frontend — `web-erp`, `web-mdp`,
> `web-hr`, and any future `web-*` app.
> **Reference implementation:** [`apps/web-erp`](../../apps/web-erp). When this document and
> `web-erp` disagree, `web-erp` wins and this document is the bug — open a fix.
> **Why it exists:** so that ERP, MDP, HR, … look, feel, and are wired identically. A user
> moving between products should not notice they changed apps.

---

## 0. TL;DR — the rule

1. **Copy the skeleton from `web-erp`, not from `web-mdp`.** `web-mdp` predates this doc and
   has drifted (see §9). New apps follow `web-erp` exactly.
2. **Same stack, same versions, same folders, same API-client contract, same tokens.**
3. **Differences between apps live in three places only:** the resource list (`lib/api/*`),
   the page components (`components/pages/*`), and the token *values* (`styles/*-tokens.css`).
   Everything else is identical boilerplate.

---

## 1. Canonical stack (pin these)

Taken from `apps/web-erp/package.json`. New apps copy these versions verbatim.

| Concern              | Choice                                  | Notes |
| -------------------- | --------------------------------------- | ----- |
| Framework            | **Next.js 16** (App Router)             | `app/` dir, RSC where it helps |
| React                | **React 19**                            | — |
| Language             | **TypeScript strict**                   | `tsc --noEmit` must pass |
| Styling              | **Tailwind v4** (`@tailwindcss/postcss`) + CSS-variable tokens | no Tailwind config theme; theme lives in CSS `@theme` |
| Primitives           | **Radix UI** + **shadcn**-style `components/ui/*` | wrapped, never imported raw into pages |
| Variants             | **class-variance-authority** + `cn()` (`clsx` + `tailwind-merge`) | — |
| Data fetching        | **TanStack Query v5**                   | one `QueryClient`, shared query keys |
| Tables               | **TanStack Table v8**                   | — |
| Forms                | **react-hook-form** + **zod** (`@hookform/resolvers`) | zod schema = validation SSOT |
| Theming              | **next-themes** (`attribute="class"`)   | light/dark + appearance switchers |
| Toasts               | **sonner**                              | one `<Toaster />` in root layout |
| Icons                | **lucide-react**                        | — |
| Charts               | **recharts**                            | wrapped in `components/ui/*-chart.tsx` |
| Dates                | **date-fns** + **react-day-picker**     | — |
| DnD                  | **@dnd-kit**                            | tree/grid reorder |
| Tests                | **Vitest** + Testing Library            | `test`, `test:watch` |

**Do not** introduce an alternative for anything in this table without changing this doc first.

---

## 2. Directory architecture (atomic design)

Every `web-*` app has this exact tree. Names are non-negotiable so cross-app muscle memory holds.

```
apps/web-<product>/
├── app/                      # Next.js App Router (routing only, thin)
│   ├── layout.tsx            # root: providers + appearance init (see §6)
│   ├── page.tsx              # entry / redirect
│   └── <segment>/[...route]/page.tsx   # catch-all → app-shell route renderer
├── components/
│   ├── ui/                   # PRIMITIVES — shadcn/Radix wrappers (button, input, dialog, …)
│   │                         #   ⚠ folder is named `ui/`, NOT `atoms/`
│   ├── molecules/            # small composites (search-select, form-field-row, num-input)
│   ├── organisms/            # complex composites (sidebar, topbar, table, line-editors, drawers)
│   ├── templates/            # app-shell, route renderer, keyboard layer
│   └── pages/                # one file per screen — the ONLY app-specific UI layer
├── lib/
│   ├── api/                  # HTTP layer — see §4 (one file per resource + client.ts + types.ts + hooks.ts)
│   ├── utils.ts              # cn() and tiny pure helpers
│   └── use-*.ts / *.ts       # hooks + domain logic (workflows, formatters, nav)
├── shared/
│   └── providers/            # query-provider.tsx (+ any cross-cutting provider)
├── styles/
│   ├── globals.css           # imports tokens, wires Tailwind v4 @theme
│   └── <product>-tokens.css  # design tokens (CSS variables) — see §5
├── scripts/check-file-size.mjs
└── package.json
```

### Layer rules (enforced in review)

- **`ui/` is the only place** that imports Radix/shadcn primitives directly. Pages never
  `import * from '@radix-ui/*'`.
- **Dependency direction is one-way:** `pages → organisms → molecules → ui`. A `ui` primitive
  never imports an organism. No upward imports, no sibling cycles.
- **`pages/` is the app's identity.** ~90% of per-product work is new files here + new
  `lib/api/*` resources. If you find yourself rewriting an organism per app, it belongs in
  `ui-kit` instead (see §8).
- **File size ≤ 400 lines** (repo rule, `CLAUDE.md §5`; `npm run check:size` enforces). Split
  large pages into `*-form.tsx`, `*-filters.tsx`, `*-model.ts`.

---

## 3. Routing & the App Shell

The shell is the persistent chrome (sidebar, topbar, multi-tab bar) that every product shares.

- `app/<segment>/[...route]/page.tsx` is a **thin catch-all** that renders the shell.
- `components/templates/app-shell.tsx` owns layout + tabs + keyboard.
- `components/templates/shell-route-renderer.tsx` maps a route string → a `components/pages/*`
  component via a **route registry** (`lib/registry.ts` / `lib/nav.ts`).
- Menus/permissions come from the backend (`useErpMyMenus()` pattern) — the sidebar is
  **data-driven**, never hardcoded per app.

> A new product reuses the entire shell. It only registers its own routes → pages.

---

## 4. API client contract (the most important section)

This is where `web-mdp` drifted hardest. **HR and all future apps follow the `web-erp` shape.**

### 4.1 Layout — `lib/api/` is a folder, not a single file

```
lib/api/
├── client.ts        # the ONE fetch wrapper (apiGet/Post/Patch/Put/Delete + upload/download)
├── types.ts         # ApiResponse<T>, PaginatedResponse<T>, PaginationParams, ApiError
├── hooks.ts         # shared TanStack Query hooks + query-key factory
├── index.ts         # barrel re-exports
└── <resource>.ts    # one file per backend resource (branches, items, employees, …)
```

`web-mdp` originally put everything in one `lib/api.ts` (realigned 2026-06-30 to `lib/api/*` —
per-domain modules + barrel). Do **not** reintroduce a monolith. One file per resource keeps
each under 400 lines and matches the backend's one-module-per-resource layout.

### 4.2 `client.ts` — copy verbatim, change only the base URL

The reference client (`apps/web-erp/lib/api/client.ts`) provides:

- `request<T>()` private helper: `credentials: 'include'`, JSON headers, query-string builder,
  `204 → undefined`, and a **uniform error envelope** decode into `ErpApiError { code, message, details }`.
- Public verbs: `apiGet`, `apiPost`, `apiPatch`, `apiPut`, `apiDelete`.
- File helpers: `buildApiUrl`, `downloadFile` (Content-Disposition aware), `apiUpload` (multipart;
  never set Content-Type manually).

**Base URL** — the single intended per-app difference. Two valid strategies; pick one per app and
document it at the top of `client.ts`:

| Strategy | Used by | When |
| -------- | ------- | ---- |
| Absolute env URL: `process.env.NEXT_PUBLIC_<APP>_API_URL` | `web-erp` (`…/api/erp`) | app deployed on its own origin |
| Same-origin Next rewrite: `/api/<app>/*` → api-gateway | `web-mdp` (`/api/mdp`) | app served behind the same gateway/host |

Rename the error class per product (`ErpApiError` → `HrApiError`) but keep the shape identical.

### 4.3 Error envelope — backend contract (matches `CLAUDE.md §5`)

```ts
// lib/api/types.ts
export interface ApiError { code: string; message: string; details?: unknown }
export interface ApiResponse<T> { data: T; error?: ApiError }
export interface PaginatedResponse<T> { data: T[]; meta: { page; limit; total; totalPages } }
export interface PaginationParams { page?: number; limit?: number; search?: string; /* … */ }
```

All backend responses use `{ data, error }`. The client unwraps `.data`; resource functions
return the inner type.

### 4.4 Resource module shape (the template every `<resource>.ts` follows)

From `apps/web-erp/lib/api/branches.ts` — copy this skeleton for every entity:

```ts
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface <Entity> { id: string; code: string; name: string; isActive: boolean;
  createdAt: string; updatedAt: string; /* … */ }
export interface Create<Entity>Payload { /* required + optional */ }
export interface Update<Entity>Payload { /* all optional */ }

export async function list<Entity>(params?: PaginationParams): Promise<PaginatedResponse<<Entity>>>;
export async function create<Entity>(p: Create<Entity>Payload): Promise<<Entity>>;
export async function update<Entity>(id: string, p: Update<Entity>Payload): Promise<<Entity>>;
export async function delete<Entity>(id: string): Promise<void>;
export async function bulkUpdate<Entity>Status(ids: string[], isActive: boolean): Promise<{ affected: number }>;
export async function bulkDelete<Entity>(ids: string[]): Promise<{ affected: number }>;
```

Conventions baked in:
- **IDs are strings** (backend serialises BigInt → string). Never `number`.
- **Soft-delete + bulk** ops are standard for master data.
- Timestamps are ISO strings.

### 4.5 TanStack Query conventions

- **One** `QueryClient` from `shared/providers/query-provider.tsx`. Defaults:
  `staleTime: 30_000`, `refetchOnWindowFocus: false`, `retry: 1`.
- **Query-key factory** per app (`erpQueryKeys` → `hrQueryKeys`), namespaced by product:
  `['hr', 'employees', params]`. Namespacing prevents cross-app cache collisions if apps ever
  share a client.
- Reference/session data (me, menus) → `staleTime: 5 * 60 * 1000`, `retry: false`.
- Mutations invalidate the matching key; no manual cache surgery unless necessary.

---

## 5. Design tokens & theming

Tokens are **CSS variables**, not Tailwind config. Tailwind v4 consumes them via `@theme` in
`styles/globals.css`.

- **File:** `styles/<product>-tokens.css` defines the palette under `:root`/`[data-theme='light']`
  and `.dark`/`[data-theme='dark']`.
- **Required token groups** (see `apps/web-erp/styles/erp-tokens.css`): Surfaces (`--bg`,
  `--panel`, `--border`), Text (`--fg`, `--fg-muted`, …), Primary palette (`--primary*`),
  Status (`--success/danger/warn/info` + `-soft`), Metrics (`--row-h`, `--header-h`, `--topbar-h`,
  `--sidebar-w`, `--radius`), Elevation (`--shadow-*`), Type families (`--font-*`).
- **shadcn semantic bridge:** alias `--background`, `--foreground`, `--border`, … onto the product
  palette so the `ui/` primitives resolve correctly. This bridge is what lets primitives be shared.
- **Appearance switchers** (data-attributes on `<html>`): `data-theme`, `data-primary`,
  `data-density` (compact/comfortable), `data-fontscale`. The blocking init script in
  `app/layout.tsx` sets them before first paint to avoid FOUC — copy it per app.

> **Per-product latitude:** token *values* (brand primary, density default) may differ.
> Token *names* and *groups* may not. A button in HR and a button in ERP are the same component
> with different variable values.

---

## 5.5. Semantic class layer — the `web-erp` visual contract (SSOT)

There are **two** styling layers, and every `web-*` app uses both:

1. **Token-driven Tailwind primitives** (`components/ui/*`) — buttons inside dialogs, badges,
   inputs in forms built from `ui/`. These resolve via the shadcn bridge (§5).
2. **Semantic ERP classes** — the *chrome and dense data surfaces* (page header, toolbar, data
   table, status pills, pager, line editors). These are **plain CSS classes**, ported from the
   prototype with names kept identical, living in `styles/erp-*.css`. Pages compose them with
   `className="page-header"`, `className="btn primary"`, etc. — **not** ad-hoc Tailwind.

> **Rule:** the dense "ERP look" comes from this semantic class layer, not from sprinkling
> Tailwind utilities on chrome. A new app **copies the whole `styles/erp-*.css` set**, renames the
> file prefix if desired, and changes only token *values* (§5). The class *names and structure*
> below are non-negotiable so cross-app muscle memory and pixel parity hold.

### 5.5.1 The CSS files (copy the whole set)

`styles/erp-components.css` is an index that `@import`s the sub-files; `globals.css` imports it.
Reference: [`apps/web-erp/styles/`](../../apps/web-erp/styles).

| File | Owns |
| ---- | ---- |
| `erp-shell.css`     | base layout, app shell, topbar, sidebar, breadcrumb, **page chrome** (`.page`, `.page-header`, `.page-title`, `.code-tag`, `.page-actions`) |
| `erp-controls.css`  | **buttons** (`.btn*`), **toolbar/filters** (`.toolbar`, `.chip`, `.search-input`), **forms** (`.field`, `.form-section`), **page body** (`.page-body`), tabs, detail **line editor** (`.lines`) |
| `erp-table.css`     | **data table** (`.tbl-wrap`, `.tbl`), **status pill** (`.pill*`), **pager** (`.pager`), **bulk bar** (`.bulk-bar`), empty/loading states |
| `erp-panels.css`    | record drawer, slide-over panels, lookup modal, settings sub-nav |
| `erp-dialogs.css`   | confirm dialog, toasts, user menu, workspace switcher |
| `erp-dashboard.css` · `erp-contact-picker.css` · `erp-multitab.css` · `erp-login.css` | dashboard/cmd-palette, contact picker, multi-tab workspace, auth screens |

### 5.5.2 Canonical page skeleton (list screen)

Every list/master page renders this exact structure (see
[`generic-list-parts.tsx`](../../apps/web-erp/components/pages/generic-list-parts.tsx)):

```tsx
<div className="page">
  <div className="page-header">
    <h1 className="page-title">
      {title}
      <span className="code-tag">M-101</span>          {/* mono doc/menu code */}
    </h1>
    <div className="page-actions">                      {/* margin-left:auto */}
      <div className="search-input"><Icon name="search"/><input/><Kbd>/</Kbd></div>
      <button className="btn"><Icon name="download"/> Export</button>
      <button className="btn"><Icon name="refresh"/></button>
      <div className="btn-split">                        {/* primary + dropdown caret */}
        <button className="btn primary"><Icon name="plus"/> Tambah <Kbd>N</Kbd></button>
        <button className="btn primary"><Icon name="chevdown"/></button>
      </div>
    </div>
  </div>

  <div className="filter-bar"> … filter button + chips + summary … </div>   {/* pinned */}

  <div className="page-body">                            {/* flex:1; min-h-0; scrolls */}
    <div className="tbl-wrap scrollbar">
      <table className="tbl"> … </table>
    </div>
  </div>

  <ListFooter />                                          {/* row count + kbd hints + pager */}
</div>
```

This is the **canonical modern organism** ([`erp-list-layout.tsx`](../../apps/web-erp/components/organisms/erp-list-layout.tsx),
mirrored by web-hr's `list-layout.tsx` and web-mdp's `master-crud-page.tsx`): `.page-header` →
`.filter-bar` → `.page-body` (the scroller) → footer. A legacy prototype-port variant
([`generic-list.tsx`](../../apps/web-erp/components/pages/generic-list.tsx)) uses `.toolbar` +
`.chip` filters and a `.pager` footer instead, with `.tbl-wrap scrollbar` self-scrolling (no
`.page-body`). Both are valid; **new apps follow `erp-list-layout`** (`.filter-bar` + `.page-body`).

Detail/form pages keep `.page` + `.page-header`, then use a scroll body —
`<div className="page-body overflow-auto p-4">` — wrapping `.form-section` / `.field` / `.lines`.
The `.lines` modifier is for **detail line-editors only** — never on a list `.tbl-wrap` (use
`scrollbar`).

**Structural rules:**
- `.page` is `flex-column; height:100%`. `.page-header` and `.toolbar` stay pinned;
  **only `.page-body` scrolls** (`flex:1; min-height:0; overflow-y:auto`).
- `.page-actions` floats right (`margin-left:auto`); search box first, then secondary `.btn`s,
  primary action last in a `.btn-split`.
- The `.code-tag` (mono menu/doc code) sits inside `.page-title`.

### 5.5.3 Button vocabulary (`.btn`)

26px tall, 12px/500 text, `var(--radius)`, token-driven. Combine the base with one modifier:

| Class | Use |
| ----- | --- |
| `btn`            | default / secondary action |
| `btn primary`    | the one main action (filled `--primary`) |
| `btn ghost`      | tertiary / icon-only toolbar action (transparent) |
| `btn danger`     | destructive (red text, red-soft hover) |
| `btn sm`         | 22px compact (inside dense rows/bulk bar) |
| `btn-split`      | wrapper: primary button + caret button fused |
| `:disabled`      | `opacity:.45; not-allowed` |

Embedded shortcut hints use `<Kbd>` (the `.kbd` primitive). **Never** restyle `.btn` with Tailwind;
add a modifier or a token instead.

### 5.5.4 Forms (`.field`)

`.form-section` groups fields. Each control is:

```tsx
<div className="field">                 {/* add `err` class to show error state */}
  <label>Nama <span className="req">*</span></label>
  <div className="ctrl"><input type="text" /></div>      {/* or select / textarea / .lookup */}
  <div className="help">Helper / error text</div>
</div>
```

Inputs are flat, `var(--panel)` bg, focus ring from `--primary`. `.field.err` reddens border + help.
Lookup/picker fields use `.ctrl .lookup`.

### 5.5.5 Data table (`.tbl`) & status (`.pill`)

- `.tbl-wrap` → scroll container; `.tbl` → the table (12px, sticky `thead`, hover/`.selected`/`.focused`
  rows). Column helpers: `.col-check` (checkbox col), `.col-num`/`.num` (right-align numerics),
  `.muted`, `.mono`. Sortable headers add `.sortable` + `.sort-ind`. Empty state: `.tbl-empty`.
- **Status pill** = `.pill` + one status modifier: `success` · `warn` · `danger` · `info` · `primary`
  (each with a leading `.dot`). This is the canonical way to render workflow/active status in tables.
  In React, the `StatusBadge` helper (per-app `badge.tsx`) maps the workflow enum → pill (§8).
- **Pager** `.pager` (page segments `.seg`), **bulk action bar** `.bulk-bar` (`.count`, `.divider`,
  `.ba-btn`) appears when rows are selected.

### 5.5.6 Filter toolbar

`.toolbar` row holds a filter icon + `.chip`s. A `.chip` shows `.label` + `.val`, gets `.active`,
has an `.x` remove affordance, and `.chip.add` adds a new filter. The free-text `.search-input`
(with `/` `Kbd`) lives in `.page-actions`, not the toolbar.

> **One-line summary for new screens:** wrap in `.page`; header = `.page-header`/`.page-title`+
> `.code-tag`/`.page-actions`; pinned `.toolbar` of `.chip`s; scrolling `.page-body` of
> `.tbl-wrap > .tbl` + `.pager`; actions are `.btn[ primary|ghost|danger|sm]`; status is `.pill`;
> forms are `.field` in `.form-section`. Pull the styling from the copied `styles/erp-*.css` — never
> re-invent chrome with raw Tailwind.

---

## 6. Root layout (`app/layout.tsx`) — provider order

Copy this order exactly (from `web-erp`); it is load-bearing:

```
<html suppressHydrationWarning>
  <head> blocking appearance-init script </head>
  <body class="text-foreground bg-background">
    <ThemeProvider attribute="class" storageKey="<app>-theme" enableSystem={false} disableTransitionOnChange>
      <AppQueryProvider>                 {/* TanStack Query */}
        <TooltipProvider delayDuration={0}>
          <Suspense>{children}</Suspense>
          <Toaster />                    {/* sonner */}
        </TooltipProvider>
      </AppQueryProvider>
    </ThemeProvider>
  </body>
</html>
```

Per-app: `metadata.title.template`, `storageKey`, `viewport.themeColor` (= brand primary).

---

## 7. Naming & code conventions

- Files: `kebab-case.tsx`. Components: `PascalCase`. Hooks: `useCamelCase`. Constants: `UPPER_SNAKE`.
  Booleans: `is/has/should/can` prefix. (Matches `CLAUDE.md §5` + global coding-style.)
- **Named exports** over default (except Next.js `page.tsx`/`layout.tsx` which must default-export).
- Immutable updates only — no in-place mutation (global coding-style rule).
- `'use client'` only where interactivity is needed; keep the shell/pages client, data-fetch via hooks.
- Path alias `@/` → app root (see `tsconfig.json`). Cross-app imports go through `packages/*`, never
  reach into a sibling app.

---

## 8. Shared package strategy (`packages/ui-kit`)

- **Tier 1 — ✅ EXTRACTED (2026-06-28).** `@sentient-factory/ui-kit` now owns the
  framework-agnostic foundation, consumed as **TS source** via Next.js `transpilePackages`
  (no `dist` build). `web-erp` consumes it through thin re-export adapters, so its public
  `@/lib/...` surface is unchanged. Package surface:

  | Import | Provides |
  | ------ | -------- |
  | `@sentient-factory/ui-kit` | `cn`, `createApiClient`, `SentiApiError`, all API types |
  | `@sentient-factory/ui-kit/api` | API client factory + types only |
  | `@sentient-factory/ui-kit/providers` | `AppQueryProvider` (`'use client'`) |
  | `@sentient-factory/ui-kit/utils` | `cn` only |
  | `@sentient-factory/ui-kit/styles/base-tokens.css` | base token contract (§5) |

  **New apps consume directly** — no adapters needed. Wire-up: add `"@sentient-factory/ui-kit": "*"`
  to deps, add `transpilePackages: ['@sentient-factory/ui-kit']` to `next.config.mjs`, then
  `const { apiGet, … } = createApiClient({ baseUrl })` in your `lib/api/client.ts`.

- **Tier 2 — pure primitives ✅ EXTRACTED (2026-06-28).** 17 `components/ui/*` primitives now live
  in `@sentient-factory/ui-kit/ui/*`, imported as `@sentient-factory/ui-kit/ui/<name>`: `button`,
  `input`, `label`, `card`, `badge`, `checkbox`, `kbd`, `icons`, `dialog`, `popover`, `tooltip`,
  `tabs`, `sonner`, `pagination`, `select`, `dropdown-menu`, `context-menu`. web-erp's
  `components/ui/<name>.tsx` are now 2-line re-export stubs (so all `@/components/ui/*` imports are
  unchanged). **Split:** `StatusBadge` stays in web-erp's `badge.tsx` (it maps the ERP workflow
  enum via `lib/status`); only the value-agnostic `Badge`/`badgeVariants` moved.
  - **Still per-app (not extracted):** `radio-group` (`BooleanRadio` Aktif/Nonaktif locale default),
    `date-input` / `date-range-picker` ("Pilih tanggal"), `form-field`, and the charts
    (`bar-chart`, `donut-chart`, `sparkline`). Generalise label defaults → props before sharing.
  - Primitives reference Tailwind token classes (`bg-background`, `bg-secondary`, …); the **consuming
    app** must define those via its tokens (`base-tokens.css` + `<product>-tokens.css`).
- **Tier 2 — remaining:** app-shell template, search-select / table / form-field molecules.
- **Keep per-app:** `components/pages/*`, `lib/api/<resource>.ts`, token *values*, route registry.

> Types that cross the FE/BE boundary (and Python) belong in `packages/shared-types`, the existing
> cross-language SSOT — not in `ui-kit`.

---

## 9. Known drift in `web-mdp` (do NOT replicate)

`web-mdp` was built before this standard. When seeding `web-hr`, mirror `web-erp` instead:

| Aspect            | `web-erp` (canonical)            | `web-mdp` (drifted)        | HR should use |
| ----------------- | -------------------------------- | -------------------------- | ------------- |
| Primitives folder | `components/ui/`                 | `components/atoms/`        | `ui/`         |
| API layer         | `lib/api/*` (per-resource)       | `lib/api/*` (realigned 2026-06-30) | `lib/api/*`   |
| Query provider    | `shared/providers/query-provider`| `theme-provider` only      | both, erp-style |
| Error handling    | `ErpApiError {code,message,details}` envelope | `throw new Error(msg)` | typed envelope |
| Tokens file       | `erp-tokens.css` (full groups)   | `mdp-tokens.css` (subset)  | full groups   |

A follow-up task should realign `web-mdp` to this standard; until then it is the counter-example,
not the template.

---

## 10. New-app bootstrap checklist (`web-hr` and beyond)

1. `cp -r` the **skeleton** from `web-erp`: `app/layout.tsx`, `components/ui/*`,
   `components/templates/*`, `shared/providers/*`, `lib/api/{client,types,hooks,index}.ts`,
   `lib/utils.ts`, `styles/globals.css`, `scripts/check-file-size.mjs`, configs
   (`next.config.mjs`, `postcss.config.cjs`, `eslint.config.mjs`, `tsconfig.json`,
   `vitest.config.ts`). Strip ERP-specific resources/pages.
2. Rename: `Erp*` → `Hr*` (error class, query keys, storageKey, metadata, env var).
3. Create `styles/hr-tokens.css` from `erp-tokens.css`; change only brand values.
4. Set base URL strategy in `client.ts` (§4.2) and wire the gateway rewrite/env.
5. Add the app to `config/ports.json` via `npm run ports:*`, then **open the UFW port**
   (`CLAUDE.md §4.1`) — otherwise LAN clients time out.
6. Add `lib/api/<resource>.ts` per backend entity (§4.4) and `components/pages/*` per screen.
7. Register routes in the shell route registry.
8. `npm run check` (lint + typecheck + size + test) must pass before commit.

---

### Source references
- Reference app: `apps/web-erp`
- Client: `apps/web-erp/lib/api/client.ts` · Resource template: `apps/web-erp/lib/api/branches.ts`
- Query hooks: `apps/web-erp/lib/api/hooks.ts` · Provider: `apps/web-erp/shared/providers/query-provider.tsx`
- Layout: `apps/web-erp/app/layout.tsx` · Tokens: `apps/web-erp/styles/erp-tokens.css`
- Repo rules: `CLAUDE.md` (§5 conventions, §4.1 UFW) · cross-lang types: `packages/shared-types`
