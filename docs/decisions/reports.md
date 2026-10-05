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
