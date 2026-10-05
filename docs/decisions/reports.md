# DECISIONS — Report Engine, Report Designer & Report Studio

> Bagian dari decision log Web-ERP. Dipindahkan dari `DECISIONS.md`
> (2026-10-04) agar file indeks ramping; **isi entri tidak diubah** dan
> nomor `§` dipertahankan sebagai anchor stabil. Indeks semua entri:
> [`DECISIONS.md`](../../DECISIONS.md).

---

### Report Engine — Custom PDF Report Engine (2026-06-06)

Keputusan: Senti ERP membangun **custom report engine sendiri** — tanpa 3rd-party
(Carbone.io, Stimulsoft, pdfme, LibreOffice, dll).

**Riset dilakukan** dengan mempelajari 622+ file `.mrt` (Stimulsoft XML) dari legacy
MyERP+ di `opt/web-erp/preferensi/Backened - myerpplus/report/mrt/m2`, `m4`, `m5`:
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

**Dokumen lengkap:** `opt/web-erp/report-engine/README.md` — living doc, update di sana.

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

## Report Studio — desainer laporan band-based (2026-06-17)

Route `/admin/report-designer` (live `erp.fr-labs.my.id/app/admin/report-designer`)
sekarang menampilkan **Report Studio** — port React **fungsional penuh** dari
mockup `opt/web-erp/Report-Designer-ERP-Profesional/ReportStudio.dc.html`
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


### Registry m0_reports — katalog laporan hasil terjemahan .mrt (2026-10-05)

Keputusan user (2026-10-05): seluruh template Stimulsoft `.mrt` legacy
(1.560 file di `preferensi/Backened - myerpplus/report/mrt/`, folder `m0..m13`)
diinventarisasi per menu → jenis laporan → template, diterjemahkan ke standar
report engine ERP, dan registry-nya **di-insert ke tabel bernama `m0_reports`**
di DB ERP. Halaman laporan per modul (`/app/*/reports/*`) memakai satu menu
Reports + combo box jenis laporan; engine ditargetkan mampu export
PDF/Word/Excel/HTML dari template yang sama.

**Deviasi nama tabel (dicatat sadar):** `m0_reports` menyimpang dari pola
`DOMAIN_snake_case_plural` di `CLAUDE.md` §1 (domain semantik `sys/adm/md/fin/…`,
tanpa prefix numerik legacy). Nama ini **instruksi eksplisit user** — mewarisi
registry legacy MySQL `m0_report` (`rid, rmoduleid, rmenuid, ritem, rtitle,
rreportname, rfilename, rdefault, rsql, rfrom, rfilter, rorderby, rgroupby,
rparam1..5, raktif, rurutan`) — dan dipertahankan apa adanya, bukan diganti
diam-diam. `m0_reports` adalah katalog lintas-modul, bukan tabel transaksi
satu domain.

**Isi & aturan registry (migrasi `20261005_032_erp_m0_reports`, model Prisma
`ErpM0Report` di `prisma/schema/erp-rpt.prisma`):**
- Satu baris per **jenis laporan**: dari ekstraksi 1.560 file → **1.326 jenis
  unik**; 218 file varian (per pelanggan/revisi/tanggal) dilipat ke kolom
  `variant_files` baris kanonisnya (`is_default=true`); 16 file junk/test
  (test*/coba/blank/datecoba) **tidak** di-register.
- Pemetaan modul legacy→ERP tersimpan di `legacy_module` + `erp_module`:
  m0→ADM, m1→M1, m2→FIN, m3→M3, m4→M4, m5→M5, m6→M6, m7→M7, m8→BI, m12→M12,
  m13→M13 (sekolah, dipertahankan), **m11→NULL (legacy-only — milik
  web-althea, tidak diserap ERP)**. `legacy_menu` NULL: registry legacy tidak
  termigrasi sehingga pemetaan per-menu tidak dapat diinferensikan.
- `sql_spec` (JSONB) menyimpan padanan `rsql/rfrom/rfilter/rgroupby/rorderby`
  + SQL lengkap seluruh data source `.mrt` sebagai **spesifikasi dataset**
  (dialek MySQL — tidak dieksekusi verbatim; builder per laporan dibuat dari
  spesifikasi ini). `params` = parameter DS + inferensi runtime
  (periode/dokumen/cabang/gudang/dll). `page_setup`/`bands`/`bindings`/
  `functions_used`/`flags` dari ekstraksi `.mrt`.
- Hubungan ke engine: `template_json` NULL dan `translation_status='PENDING'`
  saat seed; layout hasil terjemahan tetap hidup di `rpt_templates` dan
  dihubungkan via `rpt_template_id`/`report_key` pada fase terjemahan.
  `export_formats` = PDF/Word/Excel/HTML (semua true). `urutan` = urutan
  combo box per modul (alfabetis nama file kanonis).
- Seed bersifat idempoten (`ON CONFLICT (code) DO NOTHING`), dihasilkan oleh
  generator dari data ekstraksi — bukan bagian dari migrasi skema.

---

---

## Report engine .mrt v2 + Gelombang G0/G1 — 2026-10-05

**Konteks.** Registry `m0_reports` (1.326 jenis) sudah live dengan status
semua `PENDING`. User menyetujui eksekusi ("gas") gelombang sesuai blueprint
`engine-upgrade-design.md`: G0 = fondasi engine, G1 = Master Data (m1)
end-to-end sebagai pembuktian.

**Yang dibangun (web-erp, commit `b376b6c`, `7487f3d`, `e119f5f`, `7c1e436`,
`13e9598`):**
- **Template v2** di `rpt_templates.template_json` (`version: 2`): band plan
  lengkap (page/report/column header-footer, group ≤5 level, data, child,
  empty), ekspresi Stimulsoft disimpan **verbatim** dan dievaluasi oleh
  evaluator AST baru (`expr-parser`/`expr-functions`, tanpa `eval`): IIF
  bersarang, `Format` pola .NET + rantai `Replace` penukar separator,
  agregat ter-scope `Sum/SumIf/CountIf/SumRunning/Count/Last`, `Line`,
  `PageNumber/TotalPageCount`, terbilang (`f_nominal`). Dataset helper
  legacy (`formatNominal.fromat` — typo legacy dipertahankan — `formatMinus`,
  `formatTgl`, `formatQty`) menjadi pseudo-dataset dari **format service**
  terpusat (`sys_settings` group `company`, kunci `report_format_*`, seed
  migrasi `20261005_033`; design D7).
- **Satu model pagination** (`layout-engine`) dikonsumsi 4 exporter:
  PDF (@react-pdf, font DejaVu Sans sebagai substitut metrik Lao UI),
  HTML (= pratinjau), DOCX (`docx` — rekonstruksi flow dari model),
  XLSX (exceljs, mode layout + mode data). Barcode via `bwip-js`.
- **Importer** `tools/report-import/`: worklist dari `m0_reports`, parser XML
  toleran (sanitasi `SqlCommand` + regex fallback), konversi ke v2 (mm,
  style, kondisi serial, relasi), normalisasi placeholder
  `PTNAMA/RTITLE/PARAM1..5` → konteks perusahaan/laporan, literal sesi
  (`idlogin/idmsmq`) dibuang + ditandai. Hasil m1: **55/55 CONVERTED**.
  Status hanya naik via `mark-verified.ts` (CONVERTED → VERIFIED).
- **Registry API** (`erp-report-registry`, `ErpJwtAuthGuard`):
  `GET /erp/report-registry?module=` (feed combo box, urut `urutan`),
  `GET /:code` (detail + `paramSchema`), `POST /:code/render`
  (html/pdf/docx/xlsx). Provider dataset per `report_key`
  (`ReportProviderRegistry`); modul domain mendaftar sendiri.
- **Builder M1** (`erp-md-reports`): satu builder generik mengeksekusi
  config SQL whitelist per laporan (identifiers hanya dari config, nilai
  parameter selalu bound). Field template tanpa padanan dipilih `NULL`;
  dataset tanpa padanan ERP (kategori pengecekan/produksi, barang hauling,
  selling point, poin pelanggan, label PCI2, barang khusus) render kosong
  secara jujur dengan catatan di config — tetap `CONVERTED`, tidak akan
  di-VERIFIED dengan data palsu. Stok (`bstok`) diturunkan dari movement
  POSTED + opening (konvensi `erp-inv-reports`); harga level 2–5 dari
  `md_item_prices` (hari ini kosong → NULL). `PRICEKATEGORI` sementara
  menampilkan kategori barang (tabel kategori harga legacy belum ada).
- **Frontend**: halaman generik `mrt-report-page` di `/master/reports`
  (menu `M1.RPT` + `M1.RPT.HUB` "Semua Laporan", migrasi 033, grants clone
  `M1.FIN`) — satu menu per modul + combo box; modul lain tinggal
  mendaftarkan rutenya ke `MRT_HUBS`.

**Verifikasi live.** 15 laporan sampel (ITEM, COA, WAREHOUSE, CONTACT, TAX,
UNIT, CURRENCY, CITY, PROVINCE, COUNTRY, BANK, AREA,
STOCKSADJUSTMENTTYPE, ITEMDETAIL, LABEL) dirender dari data ERP riil dalam
4 format: **60/60 pemeriksaan lolos** (marker data riil di HTML, magic
PDF/ZIP valid). Jumlah baris mode-data XLSX cocok persis dengan DB
(CITY 548, ITEM 65, COA 208, TAX 56, BANK 56). Ke-15 sampel kini
**VERIFIED**; 40 sisanya CONVERTED. Batas baris builder 2.000/laporan
(AREA 7.386 baris terpotong di batas ini — paginasi server menyusul bila
dibutuhkan).

**Catatan insiden deploy.** Controller render tidak boleh me-return objek
`res` (interceptor serializer global akan menserialisasikannya secara
rekursif → stack overflow); handler `@Res()` cukup memanggil `res.send()`.
## 2026-10-05 — Gelombang G2: Dokumen FIN (m2) — 237 tipe terkonversi, 34 dataset dokumen, hub Finance aktif

Konteks: gelombang kedua migrasi laporan .mrt (blueprint `engine-upgrade-design.md`). G2 = laporan kelas DOKUMEN modul FIN; laporan pernyataan/kartu/register/list (neraca, laba rugi, buku besar, kartu AR/AP, aging, trial balance, daftar jurnal/kas/bank/giro, dsb.) ditunda ke G3 sesuai rencana. Tipe analitik persediaan yang salah folder di m2 (kartu stok, HPP, costing persediaan murni) dicatat untuk G4.

Importer:
- Bug generik ditemukan & diperbaiki lebih dulu (`f702760`): (1) typo ter-commit `export export function parseDatasets` di `tools/report-import/mrt-datasets.ts` mematahkan kompilasi ts-node importer; (2) sanitizer `sanitizeMrtXml` belum menangani elemen komponen tanpa nama hasil konversi Stimulsoft lama — pola `< Ref="NN" type="Text" isKey="true">…</>` (tepat 1 per file, pada `receivegirocanceldetail1.mrt`, `bankkeluar_SIN.mrt`, `kaskeluar_SIN.mrt`) kini diganti tag `<Recovered …>…</Recovered>` setelah langkah escaping SqlCommand. Ketiga file terkonversi penuh (bukan regex fallback); cek DB: tidak ada template lain (termasuk M1) yang membawa warning fallback.
- Impor FIN: dry-run 237 converted / 0 skipped / 0 failed; run riil **237 converted, 0 skipped, 0 failed** — seluruh 237 baris FIN di `m0_reports` kini CONVERTED dengan `report_key` + `rpt_template_id`.

Klasifikasi 237 tipe FIN → dataset G2 dibangun untuk 34 report_key:
- Kas/Bank (12): cashreceiptdetail(+cb), bank (= voucher CR di kertas bank), cashdisbursementsdetail, receivemoneydetail(+cb; DS2 = giro terlampir), spendmoneydetail(+cb), bankmasuksin, bankkeluarsin, kaskeluarsin, kaskeluarkasbon (header-only — template legacy memang tanpa dataset).
- Jurnal (9): generaljournaldetail/1/2, jurnalmemorialdetail/2, adjustmentjournaldetail/2, saldoawalcoa/2 (DS1=DS2 baris jurnal + total header window-sum; DS3 = giro terlampir).
- Giro (4): receivegirodetail1/2, spendgirodetail1/2 (dokumen register giro dari fin_giro_entries + fin_giros).
- Faktur penjualan (1): salesinvoicedetail1newbatch (DS1 teragregasi per item+harga; DS2 baris mentah + batch/lot dari baris DO via source_line_id bila ada).
- Register pajak (2): pajakkeluaran (sls_invoices/lines, filter baris ber-tax1_id), pajakmasukan (pur_invoices/lines).
- Kosong-jujur (6): anggarandetail/2 — ERP tidak punya tabel dokumen anggaran (hanya agregat fin_budget_realizations); receivegirocanceldetail1/2 + spendgirocanceldetail1/2 — tidak ada entitas dokumen pembatalan giro di ERP (pembatalan = status per giro di fin_giros). Builder mengembalikan dataset kosong dengan catatan; laporan render header-only dan TIDAK diverifikasi.
- Sisa ±203 tipe (pernyataan, kartu, aging, register, list, rekap, analitik persediaan) → G3/G4, tetap CONVERTED dengan fallback generik.

Builder (`160c9c8`, modul `erp-fin-doc-reports`, 5 file baru ≤400 baris):
- Provider `fin.` kedua di modul yang sama (layanan legacy FinDocReportsService tidak disentuh): konfigurasi whitelist SQL per report_key mengikuti pola erp-md-reports — identifier hanya dari konstanta, nilai selalu bound parameter, kolom spec tanpa padanan → NULL, LIMIT 2000, dukungan GROUP BY.
- Pemetaan legacy: m2_cr/cd/rm/sm → fin_cash_bank_transactions (kind CASH/BANK × direction RECEIPT/DISBURSEMENT) + fin_cash_bank_lines (nominal baris di-alias `kredit` sesuai dictionary legacy); gj/jm/aj/cb → fin_journal_entries journal_type GENERAL/MEMORIAL/ADJUSTMENT/OPENING_BALANCE; rg/sg → fin_giro_entries kind REGISTER type INCOMING/OUTGOING + fin_giros via giro_entry_id; giro terlampir DS2 → fin_giros.source_transaction_id = id transaksi kas/bank/jurnal; kode bank (ckodebank) → md_accounts.bank_id → md_banks.code; teks status diturunkan dari enum status dokumen (label Bahasa Indonesia, CASE di SQL, meniru FE lib/status.ts).
- Terbilang: engine `terbilang()` dievaluasi di TS oleh provider — nilai = Σ nominal baris per dokumen (mode sum) atau baris pertama (mode first-row), sufiks mata uang {IDR: 'Rupiah', USD: 'Dollar Amerika', …}, hanya dicap bila template mendeklarasikan kolom terbilang.

Deviasi dari SQL legacy (disengaja, dicatat):
1. `bankkeluar_SIN.mrt` — SQL legacy-nya secara harfiah menduplikasi query RM (bank MASUK; bug copy-paste di legacy). Builder memetakan sesuai nama file → BANK/DISBURSEMENT (spend money); nama kolom `rmuraian` dipertahankan sesuai dictionary template.
2. Pajak masukan bersumber dari pur_invoices (legacy m4_ri), total pajak = tax1_amount header per perilaku legacy; pajak keluaran kolom knama = nama divisi penjualan (legacy join sibagianpenjualan); register pajak hanya memuat baris/dokumen dengan tax1_id.
3. Filter status legacy `bank.mrt` (`Crstatus IN (2,3,4,7)`) tidak direplikasi — seleksi dokumen lewat document_no.
4. Filter `sumber` legacy kas/bank tidak direplikasi — diskriminator ERP adalah kind+direction.
5. Kolom batch DS2 faktur (nbt*) terisi hanya bila baris faktur punya source_line_id ke baris delivery order berlots; selain itu NULL.

Frontend (`d6665af`): `/app/finance/reports` didaftarkan di MRT_HUBS (module FIN, judul 'Laporan Keuangan') — cek MRT mendahului REPORT_HUBS legacy di renderer shell-routes, jadi route hub kini melayani halaman generik combo-box; route anak legacy per-laporan tetap hidup selama transisi. Tidak perlu migrasi menu (menu FIN.RPT.HUB + grant sudah ada dari gelombang FIN sebelumnya).

E2E live (2 ronde, fixture dibuat lewat API ERP sendiri lalu dibersihkan kembali ke baseline persis: cash_bank 0, jurnal 0, giro entries 0, giros 0, ledger 65):
- Ronde 1: 21 kode sampel × 4 format = 84 render, semua HTTP 201 + magic bytes benar; tanpa token 401; kode tak dikenal 404. Rekonsiliasi dataset (xlsx mode data) semua cocok: SI000001 Σ(jml×harga) 3.198.840 = grand_total DB; CR Σkredit 1.500.000; CD 750.000; RM 2.500.000 + DS2 giro BG-E2E-001 1.000.000; SM 1.250.000 + DS2 BG-E2E-002 500.000; GJ/JM/AJ/CB Σdebit=Σkredit 300.000/200.000/100.000/5.000.000 (jurnal saldo awal TERNYATA bisa dibuat & diposting via API — journal-opening-control tidak memblokir); RG Σjumlah 1.500.000 (2 baris); SG 900.000. Terbilang cocok di dataset & sebagian HTML. Namun hanya 7 kode lolos VERIFIED ronde ini — 13 kode HTML-nya tidak memuat nomor dokumen (akar masalah di engine, lihat perbaikan di bawah).
- Perbaikan engine & config dari temuan E2E (semua generik, test engine 17/17 + suite backend 43/43 hijau):
  1. `b8205df` — scope band header/footer = baris pertama semua dataset (`firstRowsScopeMap`): PageHeader/PageFooter sebelumnya dievaluasi dengan scope KOSONG dan header/footer lain hanya memetakan dataset primer, padahal pola voucher Stimulsoft menaruh no dokumen/tanggal di PageHeaderBand → render kosong & tanggal `NaN/NaN/NaN`. Ronde 2: 12/12 kode terdampak lolos semua kriteria (docno tampil di HTML, tanpa NaN) → VERIFIED.
  2. `de1d318` — DS1 spendmoneydetail: SM_FROM tidak punya join akun baris (cnomor/cnama terbaca akun bank header; debit = total header terulang per baris sehingga footer layout menjumlah 2×). Diganti LINE_FROM: akun dari baris, debit/debitvalas = nominal baris. Ronde 2 terkonfirmasi Σdebit 1.250.000 dan cnomor = akun baris (5101.01.001/6210.01.001).
  3. `646f4ca` — total faktur NULL bila diskon baris NULL: `SUM(qty*price) - SUM(discount_amount)` = NULL saat semua diskon NULL → Sum(DS1.total) footer 0/kosong. Dibungkus COALESCE pada DS1+DS2.
  4. `af1f955` — dataset helper kosong dari provider (`[]`) men-shadow `helperDatasets` engine sehingga pola `formatNominal.fromat` hilang dan angka render mentah tanpa pemisah ribuan di SEMUA laporan (termasuk M1 — tak terdeteksi di G1 karena verifikasi via xlsx-data). LayoutBase kini mengganti dataset helper yang absen ATAU kosong; helper dilengkapi kunci `pemisahDesimal`.
  5. `00e868c` — dataset satu baris (helper format, info perusahaan) kini selalu ter-bind sebagai row 0 di semua scope (`makeScope`); sebelumnya idiom Format di data band tak terformat (footer saja yang benar). Verifikasi akhir SI000001: baris `158.700`/`317.400`/`964.620` dan footer `3.198.840` semua terformat id-ID.
- **Hasil akhir FIN: 19 VERIFIED** (dari 21 kode sampel dokumen; 2 register pajak render benar di 4 format tetapi dataset kosong secara jujur — belum ada faktur ber-PPN & pur_invoices kosong — sehingga TIDAK diverifikasi). 218 sisanya CONVERTED menunggu G3/G4.
- Kuirks kecil: template CR trio + AJ me-render tanggal MM/DD/YYYY (10/05/2026) mengikuti string format bawaan template legacy-nya sendiri — konversi setia, bukan bug. Komponen terbilang `cashreceiptdetail` di template terkonversi berekspresi kosong (satu komponen di .mrt asli memakai binding yang tak terbawa importer); nilai terbilang tetap benar di dataset & tampil di varian BANK/CB — kandidat perbaikan importer gelombang berikutnya. Unique index `fin_giros.giro_number` mencakup baris soft-deleted (temuan ops ronde 2, di luar scope laporan).

Status data riil saat G2: fin_cash_bank_transactions / fin_journal_entries / fin_giro_entries / fin_giros masih 0 dokumen (kelas kas/bank/jurnal/giro diverifikasi via dokumen fixture yang dibuat lewat API ERP sendiri lalu dibersihkan kembali ke baseline); sls_invoices 13 POSTED riil (faktur penjualan terverifikasi dari data riil); belum ada faktur ber-PPN (tax1_id semua baris kosong) dan pur_invoices kosong → register pajak render kosong secara jujur, belum VERIFIED.

## 2026-10-05 — Gelombang G3: Statements & Cards/Lists FIN (m2) — 161 builder, FIN VERIFIED 19 → 35

Konteks: gelombang ketiga migrasi laporan .mrt. G3 = 203 report_key FIN sisa G2 (pernyataan, buku besar, kartu AR/AP, aging, register/list). Klasifikasi akhir: **161 kunci berkonfigurasi builder** (150 builder riil + 11 honest-empty), **42 ditunda** tanpa builder (tetap CONVERTED).

Klasifikasi 203 kunci:
- Pernyataan (41): posisi keuangan & laba rugi global + varian cabang/lokasi/divisi/proyek/costcenter (pabrik pk*, mode balance kumulatif & mode movement jendela, T-form ROW_NUMBER, laba rugi tahunan pksaldo1..12, per-tahun), neraca mutasi (+varian dimensi; bankharianglobal/kasharianglobal = neraca mutasi terbatas akun kas/bank), anggaran & realisasi (6), laporan arus kas, mutasi keuangan, daily bank, neracat2.
- Buku besar & jurnal (26): buku besar global per-tanggal/tidak, varian dimensi, pekontak/pekontak2/kop1/kop2, per cost center (bp*), rekap per kontak/cost center, daftar data jurnal (fin_ledger_entries), rekap buku besar piutang/hutang per sumber, kas/bank harian akun lawan (+DS2 giro untuk bank), buku besar akun lawan, cashflow (kbh* tanpa filter kind), list jurnal umum/adjustment/memorial/saldo awal (daftar*), list kas/bank (8).
- AR/AP (54): kartu piutang/hutang (+cabang/lokasi), rekap (+split per mata uang, rekap detail), voucher piutang/hutang (+cabang/lokasi, voucher2/22), estimasi harian (3), analisa umur (12: ringkas/detail × cabang/lokasi), keluarga IP terima pembayaran (5), uang muka penjualan (daftar/detail/kartu/rekap/voucher), piutang ongkos kirim (5).
- Giro (14): daftar/data giro masuk/keluar, per tanggal, receive/spend giro list, analisa umur giro (4).
- Honest-empty (11): daftaranggaran/2 (dokumen anggaran m2_bd tidak ada padanan ERP — sama seperti anggarandetail G2); receivegirocancellist/2 + spendgirocancellist/2 (dokumen pembatalan giro tidak ada — status per giro); daftarumpembelian(+detail), kartuumpembelian, rekapumpembelian, voucherumpembelian (tidak ada dokumen/tabel uang muka pembelian di ERP; UM penjualan memakai sls_customer_advances).
- Ditunda (42): G4 analitik persediaan salah-folder m2 (25: kartustok average/fifo/khusus, nilaipersediaan*, persediaanbarang*, saldopersediaangudang*, mutasistok, covermonth, itemtransaction, laporanbarangserial); G6 analitik penjualan (6: labarugiinvoice detail/global, summarysalesreport, insentivepersalesman, rekappenjualanperbarangalamindo, salespurchase); G7 costing produksi (6: hpp, costing, materialcosting(+2025), materialused, perincianbiaya); customer-specific (5: harianalamindo — kolom per pelanggan ter-hardcode; bukubesarorang1/2/3 + bukubesarmesin1 — staging bb_divisi mem-pivot 7 divisi per pelanggan).

Arsitektur builder (modul sama `erp-fin-doc-reports`, 9 file konfigurasi baru ≤400 baris):
- Service diperluas `bindParams`: placeholder `?` di dalam from/where/groupBy/orderBy di-bind berurutan dari konfigurasi (nilai absen → NULL, default via COALESCE di SQL). Diperlukan untuk saldo awal & jendela berjalan di dalam subquery SELECT — paramFilters (WHERE luar saja) tidak cukup. Konvensi: `?` tidak pernah di ekspresi select (urutan kunjungan kolom = urutan kamus template); seluruh logika periode di CTE `prm` dalam derived table beralias `s`.
- Sumber pernyataan/buku besar = `fin_ledger_entries` (proyeksi posting) + hierarki `md_accounts` — sama persis dengan semantik endpoint `erp-fin-reports` yang live (POSTABLE saja, opening_balance sisi normal, header = rollup turunan via CTE rekursif, baris sintetis Laba/(Rugi) Tahun Berjalan di grup ekuitas, trial balance/neraca mutasi konvensi debit-positif, saldo berjalan buku besar konvensi sisi normal). Nama tabel riil: periode fiskal = `sys_fiscal_periods` (bukan fin_), ongkos kirim = `sls_freight_receivables` (bukan receipts), `fin_settlement_allocations` tanpa deleted_at.

Keputusan semantik (disengaja, dicatat):
1. Kartu AR/AP berbasis DOKUMEN POSTED (sls/pur_invoices + fin_ar_receipts/fin_ap_payments + retur), bukan staging jurnal legacy; totalnya rekonsiliasi ke saldo kontrol ledger karena setiap dokumen POSTED terproyeksi ke ledger (dibuktikan di E2E). Saldo awal per kontak ditanam per baris; kolom saldo per baris = saldo berjalan setelah baris itu (footer grup template kosong).
2. Bucket analisa umur dibaca dari LABEL HEADER template (umur1 = jatuh tempo >60 hari lagi; pita 15 hari: 60–46/45–31/30–16/15–1 sebelum tempo; 0–30, 31–60, >60 lewat tempo). Setting legacy `m0_setting` UmurPiutang1..10 tidak ikut termigrasi ke ERP. Faktur tanpa due_date diukur dari doc_date. Kolom umur_1/umur9/umur10 tidak terpakai template ringkas → 0.
3. Laporan arus kas = metode tidak langsung dari mutasi ledger akun non-kas berkategori `cash_flow_category` (EQUITY tanpa kategori → Pendanaan; lainnya tanpa kategori → Operasi), tanda dibalik untuk ASSET/EXPENSE; Σ baris = Δkas periode.
4. Anggaran & realisasi dari `fin_budget_realizations` × `sys_fiscal_periods` yang tumpang-tindih jendela; realisasi = mutasi sisi normal (debit_total−kredit_total), variasi = anggaran − realisasi (meniru endpoint budget yang ada).
5. Daftar giro: LEFT JOIN fin_giro_entries + fallback ke transaksi kas/bank sumber (`source_transaction_id`) — giro dari kas/bank tidak punya baris register; receive/spendgirolist tetap khusus baris REGISTER.
6. UM penjualan: baris aplikasi kartu memakai tanggal dokumen UM (ERP tidak menyimpan tanggal aplikasi terpisah; applied_amount agregat). Ongkos kirim: pelunasan hanya dari settlement_status (PAID = lunas penuh), kolom pv* template detail tidak ada padanan dokumen pembayaran → NULL.
7. neracat2 dibangun sebagai rekonstruksi (pasangan T level≤2 dari staging posisi keuangan; sql_spec legacy kosong) — render lolos sweep tetapi TIDAK diverifikasi.
8. Periode default: pernyataan = bulan berjalan; kartu/rekap/voucher/aging = 1900-01-01 s.d. hari ini.

Perbaikan engine generik dari temuan E2E: exporter xlsx mode data kini men-dedupe nama worksheet secara case-insensitive (template legacy mendeklarasikan helper `FormatMinus`/`FormatTgl` kapital yang bertabrakan dengan helper engine `formatMinus`/`formatTgl` → "Worksheet name already exists"). Suite backend 43/43 + engine specs hijau, tsc bersih.

E2E live (fixture 13 dokumen via API ERP sendiri, dua periode Sep+Okt 2026: 2 jurnal umum, 4 kas/bank — 2 ber-giro, 3 SI, 2 PI, 1 penerimaan piutang teralokasi sebagian, 1 pembayaran hutang; lalu dibersihkan kembali ke baseline persis: ledger 65 baris Σ 14.809.963, jurnal/kas-bank/giro/piutang-hutang non-aktif 0, sls_invoices 13, pur_invoices 0):
- Sweep render mode data: **161/161 kunci HTTP 201**. Sampel 16 kunci × 4 format layout + data: 80/80 HTTP 201, magic bytes benar, marker HTML terverifikasi (angka terformat id-ID: 22.659.963, 6.618.640, 16.809.963, 12.505.000, 2.000.000; nomor dokumen SI000014, CR000003, JV000003, BG-G3-001).
- Rekonsiliasi dataset vs SQL independen (semua cocok persis): TB Okt Σdebit = Σkredit = 12.505.000 dan Σ saldo akhir debit-bertanda = 0; Neraca per 31 Okt 2026: Aset 22.659.963 = Kewajiban 1.800.000 + Ekuitas 0 + Laba Berjalan 20.859.963; Laba rugi Okt: pendapatan 9.405.000 − beban 600.000 = laba bersih 8.805.000; Arus kas Okt Σ baris = Δkas 4.900.000; Buku besar akun 1120.01.001 saldo penutup 16.809.963 = saldo TB; List jurnal umum Okt Σ 600.000/600.000; Kas harian Okt neto +2.000.000; Kartu piutang kontak 1025 saldo akhir 6.618.640 = rekap piutang = total aging piutang kontak yang sama; Kartu hutang kontak 1026: 1.800.000 = rekap = aging, dengan bucket aging terverifikasi per pita (PI_A 600.000 di pita 31–60; PI_B 1.200.000 di pita 15–1 sebelum tempo).
- 16 kode sampel di atas ditandai VERIFIED via mark-verified.ts → **FIN VERIFIED 19 → 35**, CONVERTED 202. Tanpa token 401; kode tak dikenal 404; detail registry G3 `hasDataProvider: true` (hub /app/finance/reports generik langsung melayani kunci baru).
- Kuirks: throttler registry menahan burst render (~429 setelah >100 render cepat berturut-turut) — sweep final memakai jeda antar-render; bukan cacat laporan.

Status data riil saat G3: ledger 65 baris riil (SI + 1 penerimaan), selebihnya kelas FIN masih tanpa transaksi riil — builder terverifikasi terhadap data riil yang ada (neraca/laba rugi/buku besar/kartu piutang membaca baris riil tsb.) plus fixture dua periode untuk kelas yang kosong.

---

## G4 — Inventory M3 + analitik persediaan salah-folder m2 (2026-10-05)

Importer m3: **129/129 CONVERTED, 0 skipped, 0 gagal** (semua `inv.*`). Builder baru `ErpInvMrtReportsService` di modul `erp-inv-reports` (9 file baru ≤400 baris): **149 report_key** = 118 konfigurasi SQL (termasuk jujur-kosong) + 31 komputasi TS (mesin replay peristiwa stok). Provider FIN dipersempit: `canHandle` kini hanya mengklaim key yang ada di `FIN_DOC_MRT_CONFIGS` (sebelumnya prefix `fin.`) agar 21 key analitik `fin.*` (kartu stok/nilai persediaan/saldo persediaan/mutasi stok) dilayani provider inventory; key `fin.*` tanpa builder tetap fallback generik kosong seperti sebelumnya. Frontend: hub MRT generik ditambah rute `/app/warehouse/reports` (M3, grup menu M3.RPT yang sudah ada; 27 menu legacy tidak diubah).

Klasifikasi:
- **Dibangun SQL (dataset riil)**: register/dokumen movement (MR/TS/RS per prefix mr/ts/rs), saldo awal (IB), penyesuaian stok (SA), kalkulasi ulang biaya (PA ← `inv_cost_recalculations`), opname (list/detail/blanko/hasil/selisih/outstanding/step), posisi & mutasi register stok, batch/lot & serial, stok minimum, daily check/timesheet.
- **Komputasi TS (replay)**: kartu stok average/fifo/khusus + rekap, mutasi stok (+per sumber), nilai/saldo persediaan `fin.*`, persediaanbarangdetail, pivot `inv.stok` (g1..g10/j1..j10), covermonth, itemtransaction. Replay memainkan ulang opening POSTED + movement POSTED per item×gudang; saldo penutup kartu stok = saldo derived ERP menurut konstruksi.
- **Jujur-kosong** (tidak ada entitas ERP; render header-only, tetap CONVERTED): refuel ×5 (ERP tidak punya entitas pengisian BBM — BBM adalah ISSUE biasa; template butuh hourmeter/jam pengisian yang tidak disimpan), daily-available staging ×4 (semantik ETL pelanggan BR/BW), rekap konsinyasi ×1.
- **Ditunda**: `inv.materialused` → G7 (costing produksi); 4 key customer-specific (`fin.nilaipersediaansatuan`, `...satuan25`, `...satuanhj`, `fin.nilaipersediaangudangdetailrswijaya`) tanpa builder. `md.barangkhusus` (M1, ditunda G1) kini terisi dari baris movement POSTED.

Keputusan semantik (disengaja, dicatat):
1. **Costing = moving average derived, BUKAN FIFO.** ERP tidak menyimpan layer FIFO; template bernama fifo/average sama-sama menerima data moving-average. Formula mengikuti `ErpInvMovingAverageCostService`: rata-rata tertimbang Σnilai/Σqty baris masuk POSTED (saldo awal + TRANSFER_RECEIPT/RETURN ber-unit_cost; baris masuk tanpa biaya gugur dari pembilang & penyebut). Stamp `md_items.average_cost` TIDAK terawat (doc service sendiri menyatakannya; terbukti di E2E: GRN men-stamp harga terakhir 13.000, saldo awal tidak men-stamp) → ekspresi `AVG_COST` di SQL family diganti subquery derived yang sama, fallback `last_hpp` → `purchase_price`. Konsekuensi: total saldo stok = Σ penutup kartu stok menurut konstruksi.
2. **Atribusi gudang baris** = `COALESCE(destination, source)` persis konvensi `erp-inv-reports` yang live. Baris movement kanonis membawa TEPAT SATU gudang (yang dipengaruhi): GRN/DO posting men-stamp demikian; API movement generik membiarkan NULL kecuali caller mengisi — laporan per-gudang mengikuti apa adanya.
3. **Penyesuaian stok & stock count TIDAK menulis movement** (perilaku ERP saat ini: posting SA/SP hanya jurnal + dokumennya sendiri) → saldo derived tidak berubah oleh SA; varian opname dibaca dari dokumen SP (system vs physical), bukan dari mutasi saldo.
4. **PA legacy "price list adjustment" → `inv_cost_recalculations`** (modul erp-inv-price-adjustments): dokumen run rekalkulasi (from/to date, costing method) + baris old/new unit cost — padanan terdekat yang ada; bukan daftar harga jual.
5. **Mutasi per sumber**: bucket `msm*` dipetakan dari `inv_stock_movements.source` + tipe (OPENING→msm3ib, PUR_GOODS_RECEIPT→msm4grn, TRANSFER→msm3ts, SLS_DELIVERY_ORDER→msm5do, SLS invoice→msm5si, retur→msm5sr/msm4prt, receipt lain→msm3rs) — heuristik terdokumentasi, bukan staging legacy per-modul yang sesungguhnya.
6. **covermonth** didefinisikan ulang: arus keluar 30 hari terakhir per item×gudang; label fast/slow/non-moving diturunkan dari cakupan bulan stok (bukan setting legacy).
7. `inv.komisilimoplast` dibangun sebagai daftar posisi stok — DS1 template-nya memang hanya kolom stok walau namanya "komisi".

Perbaikan dari temuan E2E:
- `statusLabel()` meng-cast enum ke text (`CASE col::text WHEN ... ELSE col::text END`) — CASE campuran enum/text melempar 22P02 di Postgres; menimpa 53 konfigurasi berkolom status.
- `movementDocConfig`: kolom `<prefix>notransaksi` tertimpa entri relasi (`tsnotransaksi`/`mrnotransaksi` = `rel.doc_number`) — nomor dokumen sendiri hilang di print TS/MR. Kini kondisional per prefix (di RS, `tsnotransaksi` memang nomor TS sumber).

E2E live (fixture via API ERP sendiri, Sep 2026: saldo awal IB000001 3 item, PO→GRN000008 ber-lot LOT-G4-001 acceptedQty 40 @13.000, transfer TS000001 30 antar-gudang + penerimaan RS000001, issue RF000001, MR000001, opname SP000001, penyesuaian SA000001, rekalkulasi PA000002 COMPLETED; gudang kedua G4-GDG-02 dibuat via API):
- Smoke provider: **149/149 build OK**; sweep sampel 19 kode × format (magic bytes PDF/DOCX/XLSX + marker HTML id-ID) lolos; 401 tanpa token, 404 kode tak dikenal, `hasDataProvider: true` untuk key inv + fin analitik; hub /app/warehouse/reports, /app/master/reports, /app/finance/reports 200.
- Rekonsiliasi (semua cocok persis): kartu stok BUKU-BAHASA-INGGRIS-A @Gudang Utama penutup **qty 85** (= 100+40−30−25, sama dengan SQL derived independen) dan **nilai 922.857,14** (= 1.520.000 − 325.714,29 − 271.428,57 pada rata-rata bergerak 10.857,142857); saldo per item global 115 qty / **1.248.571,43** (= 115 × 10.857,142857) = Σ kartu stok dua gudang; valuasi `fin.nilaipersediaan` konsisten dengan replay yang sama; mutasi berjalan 100→140→110→85 dengan nilai 1.000.000→1.520.000→1.194.285,71→922.857,14; selisih opname **−3** (sistem 85 vs fisik 82) bernilai **(32.571)** = −3 × 10.857,14; lot LOT-G4-001 saldo 40 @2309 kedaluwarsa 2027-06-30; stok bawah minimum menangkap item bersaldo 10 < min 50; rekalkulasi PA item B old=new **12.000** = rata-rata derived.
- **19 kode ditandai VERIFIED** via mark-verified.ts → **M3 VERIFIED 0 → 16** (113 CONVERTED), **FIN VERIFIED 35 → 37** (+kartustokfifo, +nilaipersediaan), **M1 VERIFIED 15 → 16** (+barangkhusus). `inv.beritaacarastokopnamesin` TIDAK diverifikasi: baris SP ber-lot tidak bisa dibuat via API (DTO stock-count tidak mengekspos lotId) sehingga jalur data lot-opname belum terbukti dengan baris riil.
- Fixture dibersihkan kembali: movements 2 / lines 6, saldo awal/opname/penyesuaian/lot/PO/GRN/recalc aktif 0, gudang 1, item 771 min_stock kembali 2, field biaya item 777/778 yang ter-stamp posting fixture dikembalikan ke nilai awal (avg/hpp 0, purchase 15.000). Jurnal posting fixture berada di `fin_journal_entries` (8 baris, ter-soft-delete oleh alur reopen/delete API sendiri). Catatan: `fin_ledger_entries` berisi 46 baris Σ 13.593.163 saat G4 selesai — seluruhnya ber-created_at 2–4 Okt dan TIDAK PERNAH disentuh G4 (posting inventory/purchasing tidak menulis tabel ini); angka baseline G3 (65 baris) berubah oleh aktivitas sesi paralel di DB bersama, bukan oleh gelombang ini.
- Suite backend 43/43 hijau, tsc bersih.

---

## G5 — Purchasing M4 (2026-10-05)

Importer m4: **169/169 CONVERTED, 0 skipped, 0 gagal** (semua `pur.*`). Koreksi penting: nama-nama di daftar menu (`list-laporan-per-menu.md`) TIDAK sama dengan set report_key registry yang sebenarnya — seluruh konfigurasi ditulis ulang terhadap ground truth registry (169 key + params + bindings) dan dataset terdeklarasi hasil parse template (DS1/DS2/DS3 per key). Builder baru `ErpPurMrtReportsService` di modul `erp-pur-reports` (12 file ≤400 baris; konfigurasi SQL whitelist per report_key mengikuti pola FIN/INV: identifier hanya dari konstanta, parameter ter-bind, kolom terdeklarasi tak terpetakan → NULL, cap 2.000 baris, terbilang multi-spesifikasi di-stamp di TS). Cakupan: **167 dari 169 key diklaim provider** (162 builder riil + 5 jujur-kosong); `canHandle` hanya mengklaim key yang ada konfigurasinya. Frontend: hub MRT generik ditambah rute `/app/purchasing/reports` (grup menu M4.RPT yang sudah ada; menu legacy tidak diubah). Tidak ada migrasi baru (terakhir tetap `20261005_033`).

Klasifikasi:
- **Dibangun (162)**: dokumen PR/RQ/RFQ/bid-selection, dokumen PO (termasuk form PO ber-terbilang), dokumen GRN (+ varian batch/serial), dokumen purchase invoice/RI (+ kwitansi, saldo awal RI, laporan PPN), dokumen retur PRT & debit note DNR (+ varian batch/serial), register & daftar (pembelian per header/baris/per produk/per kategori/per proyek, list PR/PO/GRN/RI/retur), keluarga outstanding (PO/GRN/RI/PR/RQ/BS/VP/VPP/AP/baris retur), analitik (potracking, pokawatagrnri, stockbackorderreportglobal, price history & analisis per supplier/produk/periode), pembayaran vendor (dokumen VP & VPP — DS1=DS2 alokasi, DS3 instrumen — daftar VP/VPP, daftar uang muka & list advance).
- **Jujur-kosong (5)** (tidak ada entitas ERP; render header-only, tetap CONVERTED): `pur.kontrakbeli` + `pur.pfdetail` (kontrak beli legacy m4_pf), `pur.piexchange` + `pur.piexchange2` (penyesuaian tukar PI), `pur.poambilsi` (kaitan premium PO↔SI).
- **Ditunda (2)**: `pur.labelpci2` → G8 (label); `pur.voucherpiutangpersalesman` → G6 (penjualan). Keduanya tanpa builder (`hasDataProvider: false`).
- Laporan uang muka pembelian (`daftarum`, `listadvance*`) terbangun penuh tetapi **tidak bisa berisi data lewat API saat ini** (lihat bug/catatan API di bawah) → render kosong secara jujur, TIDAK diverifikasi (preseden register pajak G2).

Keputusan semantik (disengaja, dicatat):
1. **`fin_settlement_allocations.invoice_ref` menyimpan ID numerik dokumen sebagai teks** (posting AP menulis `invoiceRef: invoiceId`), bukan nomor dokumen. Semua join alokasi memakai `d.id::text = a.invoice_ref`; `notransaksi` tampil = COALESCE(nomor RI/VPP/AP/retur, invoice_ref). Berlaku juga untuk alokasi AR (invoice_ref = id invoice penjualan).
2. **`source_line_id` di baris pur_* adalah kolom legacy yang selalu NULL** (API tidak pernah menulisnya) → realisasi diturunkan dari relasi ERP-native: baris PO ← Σ accepted_qty baris GRN per `order_line_id`; baris GRN ← Σ baris RI per `goods_receipt_line_id`; baris RI ← Σ baris retur per `goods_receipt_line_id` (API retur menulis kolom itu, bukan `invoice_line_id`); baris PR ← Σ baris PO lewat header `requisition_id` dicocokkan per item; baris RQ ← Σ baris PO lewat header `quotation_id` per item. Outstanding baris retur (DNR) memakai `source_line_id` apa adanya → tampil 0 secara jujur.
3. **Deviasi formula legacy**: template RI-outstanding menghitung `sisa = jml * jmlrealisasi` (perkalian — bug template legacy); diimplementasikan `sisa = jml − jmlrealisasi` (preseden G2 bankkeluar_SIN).
4. `fin_ap_payments`: tanggal dokumen = `transaction_date`; `amount` = mata uang dokumen, `amount_fx` = basis (dipakai kolom *valas); tidak ada `warehouse_id`/`due_date` (aptgljatuhtempo/aptgllunas dipetakan ke `settled_date`). `source` string bebas, default 'VP'; nomor VPP berbagi seri awalan VP. Sumber alokasi (CASE): RI → VPP → AP → PRT → CA; daftar* menegasikan jmlbayar untuk sumber AP/PRT, dokumen vendorpayment tetap positif.
5. **potracking**: `paid` diselesaikan per dokumen RI; diprorata ke baris PO menurut nilai tertagih baris agar Σpaid per dokumen tetap eksak dan outstanding per baris tidak negatif semu.
6. Tabel baris pur_* (dan `fin_settlement_allocations`, `fin_payment_instruments`) **tidak punya `deleted_at`** — baris cascade mengikuti header; filter soft-delete hanya di header.

Dua bug API ERP ditemukan E2E dan diperbaiki (minimal, dua tree):
1. **PI dari GRN tidak pernah bisa posting via API**: posting mensyaratkan `pur_invoice_lines.accrued_payable_account_id` per baris, tetapi DTO create/update tidak punya field-nya dan mapper tidak pernah mengisinya. Fix: field DTO + mapper (web-erp `354a8bd`, SF `b54d79f1`).
2. **Retur DEBIT_NOTE tidak pernah bisa posting via API**: posting membaca header `return_purchase_account_id`, tetapi DTO/service retur tidak mengekspos/mem-petakannya. Fix: field di DTO create+update + pemetaan di service create/update (web-erp `ca253cf`, SF `9dc85eab`). Catatan sync SF: working tree SF memiliki modifikasi paralel pada file-file retur yang sama; diff menunjukkan salinan menyelaraskan SF ke keadaan kanonis web-erp (perbaikan paralel — increment penomoran atomik + validasi outstanding retur — sudah ter-commit di HEAD web-erp; SF sedang mid-sync). File paralel lain (query DTO, posting service, helpers) tidak disentuh.
3. Keterbatasan API (bukan bug fix): **uang muka AP tidak dapat dibuat** — POST alokasi mensyaratkan invoice yang sudah ada (min. 1 alokasi, tervalidasi invoice) sehingga dokumen AP murni-advance tidak bisa lahir via API; laporan advance kosong secara jujur (di atas).

E2E live (fixture via API ERP sendiri, docDate 2026-10-03, pembayaran/retur 2026-10-04; rantai SUBMIT→APPROVE→POST: PR000001 → PO000007 (header requisitionId=PR) → GRN000007 (lot LOT-G5-001, diterima parsial 6/10 item A, 4/4 item B) → PI000003 → PRT000001 + DNR000001 → VP000002 100.000 + VP000003 (VPP) 30.000 melunasi PI):
- Smoke provider: **167/167 build OK**; sweep render live **167/167 HTTP 201** (dijeda — throttler registry); sampel 21 kode × format: 74/75 pemeriksaan lolos (magic bytes PDF/DOCX/XLSX + marker HTML id-ID + terbilang "Seratus Lima Puluh Ribu"/"Seratus Tiga Puluh Ribu"). Satu-satunya catatan: varian GRN batch — `nbtkode` LOT-G5-001 terbukti ada di dataset provider dan di ekspor XLSX mode data, tetapi band layout template varian itu tidak menampilkan kolom lot DS2 di HTML (kuirk layout template legacy, bukan data).
- Rekonsiliasi (semua cocok persis vs SQL independen): total dokumen PO **150.000 = Σ baris** (50.000 + 100.000); outstanding PO baris A **sisa 4** (10 dipesan − 6 diterima), baris B (sisa 0) tersaring keluar; register listpurchaseorder Σ = grand total PO 150.000; pembelian per supplier Σ **130.000 = Σ baris RI** PI000003 = listreceiveinvoice; terbayar PI dari alokasi **130.000** (VP 100.000 + VPP 30.000); RI outstanding sisa **5 / 3** (retur via baris GRN: A 1, B 1); progress PR **100%** kedua baris (realisasi via requisition_id); potracking paid terprorata 30.000/100.000, outstanding 0.
- 401 tanpa token; 404 kode tak dikenal; detail registry `hasDataProvider: true` untuk key terbangun dan `false` untuk `pur.labelpci2` yang ditunda; hub https://erp.fr-labs.my.id/app/purchasing/reports **200**.
- **21 kode ditandai VERIFIED** via mark-verified.ts → **M4 VERIFIED 0 → 21** (148 CONVERTED). Suite backend 43/43 hijau, tsc bersih.
- Fixture dibersihkan kembali via API (REOPEN → DELETE, urutan terbalik): pur_orders/pur_requisitions/pur_goods_receipts/pur_invoices/pur_returns aktif **0**; fin_settlement_allocations kembali **1** baris baseline (orphan AR pra-ada); fin_ledger_entries **46** baris aktif — sama persis dengan hitungan awal G5 (posting fixture ter-reversal oleh alur reopen/delete API sendiri). Catatan jujur: (a) `fin_payment_instruments` 3 → 6 baris — tabel ini tanpa `deleted_at`, sehingga instrumen milik pembayaran fixture yang ter-soft-delete menjadi orphan inert (tidak pernah ter-render; query instrumen selalu lewat pembayaran aktif); (b) DRAFT VP000001 milik sesi paralel yang ada saat fixture dibuat kini tidak ada lagi — dihapus sesi pemiliknya sendiri, bukan oleh G5 (cleanup G5 hanya menyentuh id fixture 7/8).
