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

Aturan lengkapnya dipindah utuh ke `.claude/rules/frontend-slicing.md`
(path-scoped — otomatis dimuat saat bekerja di `apps/frontend/**`).
**WAJIB dibaca & dipatuhi sebelum menyentuh kode frontend apa pun.**
Intinya: design system & atomic design harus siap dulu, baru slicing;
halaman list standar mengikuti pola baku di file rules tersebut.

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

## 6. Data yang kurang: isi provisional dulu (WAJIB)

Keputusan user 2026-10-04 (mencabut aturan sebelumnya "jangan dikarang,
minta ke klien"): bila data yang dibutuhkan untuk membangun/mengisi fitur
**tidak ada** di repo, database, maupun `data-client/` — **isi dulu dengan
data provisional (karangan yang wajar)** agar implementasi dan pengujian
end-to-end bisa lanjut. Fitur tidak boleh tertahan hanya karena data klien
belum datang.

Syarat wajib data provisional:

- **Ditandai jelas dan bisa dicari**: baris yang dikarang wajib memakai
  penanda `legacy_code='provisional'` (dan/atau metadata
  `{"origin": "provisional"}`); untuk master yang namanya terlihat user
  (mis. nama supplier/penerbit) tambahkan awalan `[PROVISIONAL]` bila
  tidak ada field penanda yang lebih baik. Data provisional tidak boleh
  tercampur tanpa jejak dengan data asli klien.
- **Dicatat daftarnya**: setiap pengisian provisional dilaporkan ke user
  (field apa, nilai apa) dan dicatat di dokumen rekap progres, agar saat
  data klien asli tiba tinggal diganti — pergantian provisional -> data
  asli adalah pekerjaan lanjutan yang eksplisit, dan penandanya dicabut.
- **Wajar & konsisten**: harga beli < harga jual, persentase rabat dalam
  rentang umum, stok minimum < stok maksimum, dan seterusnya.

Pengecualian — **tetap jangan dikarang** (biarkan NULL):

- Identitas resmi/pemerintah: NPWP, NPSN.
- Nomor dokumen eksternal: nomor faktur pajak, nomor pesanan SIPLah, dsb.

Nilai turunan dari sumber eksplisit yang sudah ada (mis. jenjang dari
awalan nama sekolah) tetap boleh dipakai dan dicatat sebagai hasil
turunan, bukan data asli klien.

---

## Worktree Policy (VPS-wide)

- **Do not use Git worktrees on this VPS.** Work directly in the active workspace/checkout.
- Do not create, enter, recommend, or require a worktree for any task, including background jobs.
- Use the current branch, or create a normal Git branch in the same checkout when isolation is needed.
