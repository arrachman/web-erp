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

