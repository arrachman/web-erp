# Senti ERP — Report Engine

> Status: **IMPLEMENTED (MVP) — Finance pilot live**
> Phase: Implementation (engine + Finance wiring); Sales/Purchasing/Inventory rollout + designer alignment pending
> Last updated: 2026-06-13
> Author: Claude (dari riset bersama user)
>
> **2026-06-13 update:** render engine dibangun (`apps/api-gateway/src/erp-report-engine/`,
> stack = **@react-pdf/renderer v3**). Arsitektur final = *template owns LAYOUT, report
> builder owns DATA* (template SQL/`dataSources` diabaikan untuk laporan ber-`reportKey`).
> Binding via kolom `rpt_templates.report_key` (`<module>.<report>`). Finance: 14 report
> ter-wire ke engine (fallback pdfkit bila tak ada template aktif). Detail keputusan:
> `apps/web-erp/DECISIONS.md` §"Report Engine — Wiring reports → Report Designer templates".

Dokumen ini adalah **single source of truth** untuk desain custom report engine Senti ERP.
Baca ini sebelum menyentuh apapun di `/report-engine/`.

---

## Daftar Isi

1. [Keputusan Terkunci](#1-keputusan-terkunci)
2. [Temuan Riset — Analisis MRT Legacy](#2-temuan-riset--analisis-mrt-legacy)
3. [Arsitektur Engine](#3-arsitektur-engine)
4. [Spesifikasi Template JSON](#4-spesifikasi-template-json)
5. [Spesifikasi Expression Engine](#5-spesifikasi-expression-engine)
6. [Fitur Terbilang](#6-fitur-terbilang-amount-in-words)
7. [Roadmap Implementasi](#7-roadmap-implementasi)
8. [Open Questions](#8-open-questions)

---

## 1. Keputusan Terkunci

| Topik | Keputusan | Tanggal |
|---|---|---|
| Pendekatan | **Custom engine, tanpa 3rd-party** (Carbone.io, Stimulsoft, pdfme, dll ditolak) | 2026-06-05 |
| Format template | **JSON** — bukan XML seperti Stimulsoft .mrt, bukan DOCX/XLSX | 2026-06-05 |
| Sintaks marker | **Gaya Carbone `{d.x:formatter}`** (ATM dari carbone.io) — `d` = baris data, `c` = complement (company/settings/params/summary), formatter suffix `:formatN`/`:formatD`/`:html`/`:terbilang`. Tetap **band-based** (bukan flat-loop Carbone). Spec di §5 | 2026-06-07 |
| Sumber data | **API endpoint JSON** per report — tidak embed SQL di template | 2026-06-05 |
| Output | **PDF** sebagai output utama; HTML preview sebagai byproduct | 2026-06-05 |
| Rendering stack | **@react-pdf/renderer v3** (CJS, React 18 lokal di api-gateway) — Puppeteer ditolak | 2026-06-13 |
| Sumber data (revisi) | **Builder owns data** — engine bind hasil `ReportDocument`/`ReportDataset` sebagai `{d}`; SQL di template diabaikan untuk laporan ber-`reportKey` | 2026-06-13 |
| Binding laporan→template | Kolom `rpt_templates.report_key` (`<module>.<report>`); template aktif yang cocok dipakai saat cetak PDF | 2026-06-13 |
| Designer UI | **Fase berikutnya** — MVP pakai template JSON manual dulu | 2026-06-05 |
| Terbilang | Implement sendiri sebagai fungsi TS, **bukan** MySQL stored procedure | 2026-06-05 |
| Legacy MRT | Referensi feature/logic/flow SAJA — **tidak diport 1:1** | 2026-06-05 |

---

## 2. Temuan Riset — Analisis MRT Legacy

### 2.1 File yang Dipelajari

| File | Modul | Ukuran | Tipe | Hal Baru Ditemukan |
|---|---|---|---|---|
| `m2/AdjustmentJournaldetail.mrt` | Finance | 1037 baris | Ledger/List | Pola dasar: band, expressions, number format |
| `m2/bukubesarpekontak2_kop2.mrt` | Finance | 3548 baris (163 KB) | GL per Contact | 2-level group, PrintOnAllPages, CanGrow, conditional formatting, 122-elemen GroupHeader |
| `m4/PO.mrt` | Purchasing | ~97 KB | Form Dokumen | EmptyBand, CanShrink, Image (logo), multi-DS settings |
| `m4/ListPurchaseOrder_product.mrt` | Purchasing | 36 KB | List | Pattern list sederhana |
| `m4/pembelian.mrt` | Purchasing | 32 KB | List | Konfirmasi pattern 2-group list |
| `m5/FAKTURPENJUALAN.mrt` | Sales | 128 KB | Form Faktur | SumIf lintas-band, f_nominal (terbilang), 63-elemen GroupFooter, multi-currency |
| `m5/DailySales2.mrt` | Sales | 123 KB | Sales Report | View `m2r_daily_sales_new`, CASE pivot di SQL |

**Skala file MRT:**
- m4 Purchasing: **211 file**
- m5 Sales: **411 file**
- Scan seluruh 622 file m4+m5: **nol** Chart, CrossTab, Barcode. SubReport hanya 1 file di m7.

---

### 2.2 Anatomi Stimulsoft MRT (untuk referensi)

```
<StiSerializer>
  <Dictionary>
    <Databases>           ← koneksi ODBC ke MySQL
    <DataSources>         ← SQL queries (DS1, formatTgl, formatNominal, ...)
    <Variables>           ← parameter laporan (tanggal awal/akhir, dll)
  <Pages>
    <Page1>
      <Components>        ← list band (urutan = urutan render atas-bawah)
        <PageHeaderBand>  ← isi komponen (Text, Image, primitives)
        <GroupHeaderBand> ← level 1 dan 2
        <DataBand>
        <EmptyBand>
        <GroupFooterBand> ← level 1 dan 2
        <PageFooterBand>
```

---

### 2.3 Katalog Band

| Band | Frekuensi | Properties Kunci | Catatan |
|---|---|---|---|
| `PageHeaderBand` | Semua file | `height` | Judul laporan, nama perusahaan, parameter filter |
| `PageFooterBand` | Semua file | `height` | `{PageNumber}/{TotalPageCount}`, `{Time}` |
| `GroupHeaderBand` level 1 | Semua file | `Condition`, `PrintOnAllPages`, `NewPageBefore` | Bisa 0 elemen (trigger) hingga 122 elemen (kartu entitas penuh) |
| `GroupHeaderBand` level 2 | File kompleks | `Condition`, `NewPageBefore` | Outer group; biasanya `height=0` (invisible) |
| `DataBand` | Semua file | `DataSourceName`, `CanGrow`, `CanShrink` | Baris data berulang |
| `EmptyBand` | Form dokumen | — | Filler baris kosong saat DataBand < minRows. Kolom sama dengan DataBand, text kosong |
| `GroupFooterBand` level 1 | Semua file | — | Subtotal per group. Bisa 63 elemen (faktur penjualan) |
| `GroupFooterBand` level 2 | File kompleks | — | Grand total per outer group |

---

### 2.4 Katalog Komponen

| Tipe | Deskripsi | Properties Kritis |
|---|---|---|
| `Text` | Text statis atau ekspresi | `Text`, `Font`, `HorAlignment`, `VertAlignment`, `CanGrow`, `CanShrink`, `WordWrap`, `Conditions` |
| `Image` | Gambar (logo perusahaan) | `ImageURL` (key dari settings), `ClientRectangle` |
| `HorizontalLinePrimitive` | Garis horizontal | `ClientRectangle`, border style |
| `StartPointPrimitive` + `EndPointPrimitive` | Garis vertikal (berpasangan via GUID) | `Guid` sama di kedua ujung |

**Tidak ditemukan** di seluruh 622 file m4+m5: Chart, CrossTab/Matrix, Barcode, SubReport, Gauge.
Engine MVP tidak perlu support ini.

---

### 2.5 Expression Language

Semua ekspresi dibungkus `{...}` di dalam properti `Text` komponen.

#### Field & DataSource
```
{DS1.fieldname}           ← nilai field dari DataSource aktif
{formatTgl.format}        ← nilai dari DataSource lain (lookup)
```

#### Aggregates
```
{Sum(DS1.field)}
{SumIf(GroupHeaderBand1, DS1.field, Line == 1)}   ← baca nilai header dari footer
```
`SumIf` lintas-band dipakai karena JOIN denormalisasi menyimpan header field di setiap baris.
Di Senti ERP → **eliminasi dengan pre-compute di API response** (lihat §3.3).

#### Conditional & Format
```
{IIF(condition, trueValue, falseValue)}
{Format(formatTgl.format, DS1.gltgl)}
{Replace(str, ",", "RB")}
```

#### System Variables
```
{PageNumber}        ← halaman saat ini
{TotalPageCount}    ← total halaman
{Time}              ← datetime saat cetak
{Line}              ← nomor baris di DataBand
```

---

### 2.6 Layout Properties

| Property | Tipe | Perilaku |
|---|---|---|
| `CanGrow` | Boolean | Tinggi cell/band bertambah jika konten melebihi `height` |
| `CanShrink` | Boolean | Tinggi cell/band mengecil jika konten lebih pendek |
| `GrowToHeight` | Boolean | Tumbuh ke tinggi parent band |
| `PrintOnAllPages` | Boolean | GroupHeader diulang tiap halaman (sticky) |
| `NewPageBefore` | Boolean | Halaman baru sebelum band dirender |
| `WordWrap` | Boolean | Teks wrap ke baris berikutnya |

---

### 2.7 Pola DataSource (Legacy → Senti ERP)

Setiap MRT selalu punya 4–7 DataSource:

| Legacy DataSource | Query ke | Di Senti ERP |
|---|---|---|
| `DS1` | JOIN flat semua tabel transaksi+master | `data[]` array di JSON payload |
| `formatTgl` | `m0_setting WHERE skode="FormatTanggalReport"` | `settings.dateFormat` |
| `formatNominal` | `m0_setting WHERE skode="FormatNominalReport"` | `settings.groupSep / decimalSep / decimalDigits` |
| `formatQty` | `m0_setting WHERE skode="FormatJmlReport"` | `settings.qtyDigits` |
| `formatMinus` | `m0_setting WHERE skode="FormatMinusReport"` | `settings.negativePrefix / negativeSuffix` |
| `logo` | `m0_setting WHERE skode="LogoPerusahaan"` | `company.logoUrl` |
| `AlamatPerusahaan` | `m0_setting WHERE skode="AlamatPerusahaan"` | `company.address` |

**Format `FormatNominalReport`** disimpan sebagai `".|,|0"` (pipe-separated):
`digitGrup | pemisahDesimal | digitDesimal`.
Contoh: `".|,|0"` = titik ribuan, koma desimal, 0 digit desimal.

**Format `FormatMinusReport`** = 2 karakter: prefix + suffix.
Contoh: `"()"` → `(1.000)` | `"- "` → `-1.000 `.

---

### 2.8 Pattern Format Angka (Standard di Semua Report)

```
{IIF((val) < 0, formatMinus.kiri, "")}
{IIF((val) < 0,
  Replace(Replace(Replace(Replace(Replace(
    Format(formatNominal.fromat, val),
    ",","RB"),".","DS"),"RB",formatNominal.digitGrup),"DS",formatNominal.pemisahDesimal),"-",""),
  Replace(Replace(Replace(Replace(
    Format(formatNominal.fromat, val),
    ",","RB"),".","DS"),"RB",formatNominal.digitGrup),"DS",formatNominal.pemisahDesimal)
)}
{IIF((val) < 0, formatMinus.kanan, "")}
```

Di Senti ERP → cukup `{d.field:formatN(2)}` (§5.2). Template tidak perlu tulis
5-lapis Replace; grup ribuan/desimal/minus diambil otomatis dari `settings`.

---

### 2.9 Klasifikasi Tipe Report

#### Tipe 1 — Form Dokumen Transaksi
1 dokumen = 1 grup; GroupHeader = kartu entitas; EmptyBand filler rows.
```
PageHeader → GroupHeader1 (header dok + col headers) → Data → Empty → GroupFooter1 (totals + TTD) → PageFooter
GroupBy: nomor transaksi
Contoh: PO, Sales Invoice, GRN, Kwitansi
```

#### Tipe 2 — Laporan List / Tabulasi
Banyak record; 1–2 level grouping; GroupHeader = column labels saja.
```
PageHeader (col headers) → GroupHeader2 (outer, invisible) → GroupHeader1 → Data → GroupFooter1 → GroupFooter2 → PageFooter
GroupBy: kategori / salesman / supplier / periode
Contoh: Daftar PO, Laporan Pembelian, Daily Sales
```

#### Tipe 3 — Buku Besar / Ledger
GroupHeader kompleks (PrintOnAllPages); running balance di footer (Last, bukan Sum).
```
PageHeader → GroupHeader2 (per contact, NewPageBefore) → GroupHeader1 (per COA, PrintOnAllPages, 122 elemen) → Data (CanGrow) → GroupFooter1 (sum debit/kredit + Last(saldo)) → GroupFooter2 → PageFooter
Contoh: Buku Besar, Buku Besar per Kontak
```

---

### 2.10 Fitur yang TIDAK Dipakai di Produksi (scope keluar MVP)

| Fitur | Status di 622 file m4+m5 |
|---|---|
| Chart / Grafik | Nol — hanya di `m11/COBA.mrt` (percobaan) |
| CrossTab / Matrix | Nol |
| Barcode / QR Code | Nol |
| SubReport | 1 file di m7 (`pembayaranpiutanggiro.mrt`) saja |

---

## 3. Arsitektur Engine

### 3.1 Stack Teknologi

**Pilihan rendering (PENDING keputusan user):**

| Opsi | Pro | Kontra |
|---|---|---|
| **@react-pdf/renderer** | Pure TS, no browser dep, bundle kecil, cepat | Layout system terbatas, custom fonts ribet, CSS terbatas |
| **HTML → PDF via Puppeteer** | Pixel-perfect, full CSS, mudah debug via browser preview, font mudah | Perlu Chromium, berat di server, startup lambat |
| **HTML → PDF via Playwright** | API lebih modern, sama dengan Puppeteer | Sama dengan Puppeteer |

**Rekomendasi:** Puppeteer/Playwright. Alasan: laporan ERP butuh layout presisi
(border tabel, vertical lines, CanGrow per-cell). HTML/CSS jauh lebih mudah di-debug.
Chromium bisa di-cache di Docker layer.

**Lokasi module:**
- MVP: NestJS module di `apps/api-gateway/src/reports/`
- Nanti: pisah ke `apps/report-engine/` jika load tinggi

### 3.2 Pipeline Rendering

```
User klik "Cetak PDF"
  ↓
[1] Request ke API: GET /api/reports/{templateId}?param1=x&param2=y
  ↓
[2] Load template JSON dari template store (file/DB)
  ↓
[3] Fetch data payload dari endpoint yang di-define di template
  ↓
[4] Expression Engine: evaluate {field}, {Sum}, {IIF}, {formatNumber}, dll
  ↓
[5] Layout Engine: hitung posisi band, CanGrow, pagination, group breaks
  ↓
[6] Renderer: generate HTML string (dengan CSS inline)
  ↓
[7] Puppeteer: HTML → PDF buffer (A4, margin sesuai template)
  ↓
[8] Response: stream PDF ke browser (Content-Type: application/pdf)
```

**Template store:**
- MVP: JSON files di `apps/api-gateway/src/reports/templates/`
- Phase 4: tabel `rpt_templates` di DB (user upload/edit via designer)

### 3.3 Kontrak API Payload

Setiap endpoint report mereturn **satu** JSON dengan struktur standar:

```jsonc
{
  "company": {
    "name": "PT. Senti Factory",
    "address": "Jl. Industri No. 1, Surabaya",
    "phone": "031-xxxxxxx",
    "logoUrl": "/static/logo.png"
  },
  "settings": {
    "dateFormat": "DD/MM/YYYY",
    "groupSeparator": ".",
    "decimalSeparator": ",",
    "decimalDigits": 0,
    "negativePrefix": "(",
    "negativeSuffix": ")",
    "currencySymbol": "Rp",
    "qtyDigits": 2
  },
  "params": {
    "dateFrom": "2026-01-01",
    "dateTo": "2026-12-31"
  },
  "data": [
    {
      "ponotransaksi": "PO/2026/001",
      "potgl": "2026-01-15T00:00:00.000Z",
      "posupplier": "PT. Supplier A",
      "bkode": "BRG-001",
      "namabarang": "Besi Beton 10mm",
      "jml": 100,
      "satuan": "batang",
      "harga": 25000,
      "diskon": 0,
      "total": 2500000,
      "pototaltransaksi": 2500000
    }
  ],
  "summary": {
    "grandTotal": 2500000,
    "totalDiskon": 0,
    "totalPajak": 275000,
    "terbilang": "DUA JUTA LIMA RATUS RIBU RUPIAH"
  }
}
```

**Kenapa `summary` pre-computed di API:**
Legacy pakai `SumIf(GroupHeaderBand1, DS1.field, Line==1)` karena JOIN denormalisasi
menduplikasi header field di setiap baris. Di Senti ERP, API return data bersih —
aggregates header-level (diskon, pajak, biayalain) sudah ada di `summary`, bukan
diulang di setiap baris `data[]`. Ini menghilangkan kebutuhan `SumIf` lintas-band.

---

## 4. Spesifikasi Template JSON

### 4.1 Root Template

```jsonc
{
  "id": "purchase-order-v1",
  "name": "Purchase Order",
  "module": "pur",
  "version": 1,
  "pageSize": "A4",            // A4 | A5 | Letter | Legal
  "orientation": "portrait",   // portrait | landscape
  "margins": { "top": 10, "right": 10, "bottom": 10, "left": 10 },  // mm
  "dataEndpoint": "/api/reports/purchase-orders",
  "params": [
    { "name": "id", "type": "string", "label": "Nomor PO", "required": true }
  ],
  "fonts": ["Arial"],
  "bands": []
}
```

### 4.2 Band Definitions

**PageHeaderBand:**
```jsonc
{
  "type": "pageHeader",
  "height": 30,
  "components": []
}
```

**PageFooterBand:**
```jsonc
{
  "type": "pageFooter",
  "height": 8,
  "components": []
}
```

**GroupHeaderBand:**
```jsonc
{
  "type": "groupHeader",
  "level": 1,                       // 1 = inner, 2 = outer
  "groupBy": "ponotransaksi",       // field di data[]
  "printOnAllPages": false,
  "newPageBefore": false,
  "height": 12,
  "components": []
}
```

**DataBand:**
```jsonc
{
  "type": "data",
  "height": 6,
  "canGrow": false,
  "minRows": 0,                     // jika > 0: pad dengan baris kosong (EmptyBand effect)
  "components": []
}
```

**GroupFooterBand:**
```jsonc
{
  "type": "groupFooter",
  "level": 1,
  "height": 20,
  "components": []
}
```

### 4.3 Component Definitions

**Text:**
```jsonc
{
  "type": "text",
  "name": "LBLTOTAL",
  "x": 120, "y": 0, "width": 30, "height": 6,
  "expression": "{d.total:formatN(2)}",
  "style": {
    "fontSize": 9,
    "fontFamily": "Arial",
    "bold": false,
    "italic": false,
    "color": "#000000",
    "background": "transparent",
    "align": "right",
    "vertAlign": "middle",
    "wordWrap": false,
    "border": {
      "sides": ["bottom"],           // top | right | bottom | left | all
      "style": "solid",              // solid | dashed | dotted
      "width": 0.5,
      "color": "#000000"
    }
  },
  "canGrow": false,
  "canShrink": false,
  "conditions": [
    {
      "when": "d.total < 0",
      "style": { "color": "#FF0000" }
    }
  ]
}
```

**Image:**
```jsonc
{
  "type": "image",
  "name": "CompanyLogo",
  "x": 0, "y": 0, "width": 30, "height": 15,
  "src": "{c.company.logoUrl}",
  "fit": "contain"
}
```

**Line:**
```jsonc
{
  "type": "line",
  "x": 0, "y": 5,
  "width": 170, "height": 0,       // height=0 = horizontal; width=0 = vertical
  "style": { "color": "#000000", "width": 0.5, "style": "solid" }
}
```

---

## 5. Spesifikasi Expression Engine — sintaks gaya Carbone (ATM)

> **Keputusan 2026-06-07:** sintaks marker = **Amati-Tiru-Modifikasi dari carbone.io**.
> Kita pinjam gaya terse `{d.field}` + formatter suffix `:name(args)`; **tetap
> band-based** (bukan flat-loop `{d.items[i].x}` ala Carbone — band model dibutuhkan
> untuk Buku Besar 2-level / Tipe 3, lihat §2.9). Iterasi baris ditangani oleh
> **DataBand** atas `data[]`, bukan marker `[i]` inline.

Expression engine meng-evaluate string `"..."` yang mengandung `{...}` blocks.
Multiple ekspresi bisa digabung: `"Halaman {$page} dari {$pages}"`.

### 5.1 Namespace (root data)

ATM Carbone yang punya `d` (data) + `c` (complement). Di Senti:

| Root | Isi | Sumber payload (§3.3) | Contoh |
|---|---|---|---|
| `d` | Baris data **scope band saat ini** (DataBand) | `data[]` per-row | `{d.ponotransaksi}`, `{d.namabarang}` |
| `c` | **Complement** — semua data non-baris (company, settings, params, summary) | `company` / `settings` / `params` / `summary` | `{c.company.name}`, `{c.settings.dateFormat}`, `{c.params.dateFrom}`, `{c.summary.grandTotal}` |

Nested OK: `{d.partner.name}`, `{c.company.address}`.

### 5.2 Formatter (suffix `:name(args)`, bisa di-chain)

ATM langsung dari Carbone (`{d.amount:formatN(2)}`, `{d.notes:html}`):

| Formatter | Fungsi | Contoh |
|---|---|---|
| `:formatN(decimals?)` | Angka + grup ribuan dari `settings`. Default `decimalDigits` | `{d.total:formatN(2)}` |
| `:formatC(currency?)` | Mata uang (+ simbol dari `settings.currencySymbol`) | `{d.total:formatC}` |
| `:formatQ` | Kuantitas (`settings.qtyDigits`) | `{d.jml:formatQ}` |
| `:formatD(pattern?)` | Tanggal; default `settings.dateFormat` | `{d.tgl:formatD(DD/MM/YYYY)}` |
| `:terbilang(currency?)` | Nominal → huruf (§6) | `{c.summary.grandTotal:terbilang}` |
| `:html` | Render string sebagai HTML (rich text) | `{d.notes:html}` |
| `:upperCase` / `:lowerCase` / `:ucFirst` | Transform teks | `{d.status:upperCase}` |
| `:padLeft(n,ch?)` / `:padRight(n,ch?)` | Padding | `{d.kode:padLeft(6,0)}` |
| `:default(text)` | Fallback bila null/undefined/empty | `{d.npwp:default(-)}` |
| `:ifLT(x):show(a):elseShow(b)` | Conditional tampil (Carbone `:ifEQ/:ifGT/:show`) | `{d.total:ifLT(0):show((-)):elseShow()}` |

Chain dieval kiri→kanan: `{d.total:formatN(2):default(0,00)}`.

### 5.3 Aggregate (band-scope, gaya fungsi)

Carbone `:aggSum` tak cocok dengan band model → tetap fungsi, namespace `d`:

| Syntax | Deskripsi |
|---|---|
| `{sum(d.field)}` | Sum di scope group aktif |
| `{count()}` | Jumlah baris di group |
| `{min(d.field)}` / `{max(d.field)}` | Min / max |
| `{last(d.field)}` | Nilai baris terakhir di group (running balance) |

Formatter bisa ditempel: `{sum(d.total):formatN(2)}`.

**Scope:** GroupFooter level 1 → agregat baris group level 1; level 2 → group
level 2; di DataBand → running total seluruh data.

### 5.4 System variable (prefix `$`)

| Syntax | Deskripsi |
|---|---|
| `{$page}` | Halaman saat ini |
| `{$pages}` | Total halaman |
| `{$line}` | Nomor baris di DataBand |
| `{$now}` | Datetime cetak — `{$now:formatD(DD/MM/YYYY HH:mm)}` |

### 5.5 Conditional formatting (component `conditions[]`)

Conditional **style** (warna/font) tetap di array `conditions` komponen (§4.3),
bukan formatter. Ekspresi `when` = boolean JS sederhana atas namespace `d`/`c`:
```
d.total < 0
d.status == 'CANCELLED'
d.glid == 0
d.qty > d.qtyMax
```
Operator: `==`, `!=`, `<`, `>`, `<=`, `>=`, `&&`, `||`, `!`

---

## 6. Fitur Terbilang (Amount in Words)

Legacy MyERP+ pakai MySQL stored function `f_nominal(amount, currency)`.
Di Senti ERP → implementasi TypeScript, dipanggil di API layer (bukan di template).

**Kontrak fungsi:**
```typescript
// apps/api-gateway/src/reports/lib/terbilang.ts
export function toTerbilang(amount: number, currency: 'IDR' | 'USD' = 'IDR'): string

// toTerbilang(150000)          → "SERATUS LIMA PULUH RIBU RUPIAH"
// toTerbilang(1500000.5)       → "SATU JUTA LIMA RATUS RIBU RUPIAH LIMA PULUH SEN"
// toTerbilang(1000, 'USD')     → "ONE THOUSAND US DOLLAR"
```

**Cara pakai di template:** `{c.summary.grandTotal:terbilang}` (formatter suffix, §5.2) —
nilai biasanya sudah pre-computed di `summary.terbilang`, atau dihitung on-the-fly.

**Belum dibuat** — pending phase implementasi.

---

## 7. Roadmap Implementasi

### Phase 1 — Foundation (NEXT)

- [ ] Putuskan rendering stack: @react-pdf/renderer vs Puppeteer (tanya user)
- [ ] Buat struktur `apps/api-gateway/src/reports/`
- [ ] Implement Expression Engine dasar: `{field}`, `{IIF}`, `{Format}`, `{PageNumber}`
- [ ] Implement `formatNumber()`, `formatDate()`, `formatQty()`
- [ ] Implement `toTerbilang()` (IDR)
- [ ] Renderer: PageHeader + Data + PageFooter saja
- [ ] Test dengan 1 template: Daftar PO (Tipe 2 — list sederhana)

### Phase 2 — Complete Band Support

- [ ] GroupHeader + GroupFooter n-level
- [ ] EmptyBand / `minRows` padding
- [ ] `Sum()`, `Count()`, `Last()` aggregates
- [ ] `CanGrow` / `CanShrink` pada text
- [ ] `PrintOnAllPages` pada GroupHeader
- [ ] `NewPageBefore` pagination
- [ ] Image component (logo)
- [ ] Line component (horizontal + vertical)
- [ ] Test dengan PO form + Sales Invoice (Tipe 1 — form dokumen)

### Phase 3 — Complex Reports + Conditional Formatting

- [ ] Per-cell conditional formatting (color, font berdasarkan ekspresi)
- [ ] `{Last(field)}` untuk running balance
- [ ] `{LineNumber}` row numbering
- [ ] Test dengan Buku Besar Per Kontak (Tipe 3 — 2-level group, 122-elemen GroupHeader)
- [ ] Multi-currency display

### Phase 4 — Designer UI (Future)

- [ ] Tabel `rpt_templates` di DB
- [ ] Template store API (CRUD)
- [ ] Drag-and-drop designer (React)
- [ ] Preview real-time di browser
- [ ] Permission: role yang bisa create/edit template

---

## 8. Open Questions

| # | Pertanyaan | Impact | Status |
|---|---|---|---|
| Q1 | Rendering stack: @react-pdf/renderer vs Puppeteer? | Arsitektur dasar | **PENDING — tanya user** |
| Q2 | Template storage fase pertama: files atau tabel DB? | Deployment | Sementara: JSON files |
| Q3 | Perlu scan m3 (Inventory) atau m7 (AR/AP) sebelum mulai? | Feature coverage | Bisa parallel dengan implementasi |
| Q4 | `SubReport` (ditemukan di m7) perlu di-scope ke MVP? | Complexity | Rekomendasi: skip MVP |
| Q5 | Format terbilang untuk USD dan mata uang lain? | Fungsi terbilang | IDR dulu, extend later |
| Q6 | Parameter report: date range picker — URL query atau modal form? | UX | Belum diputuskan |
| Q7 | Report scheduling / kirim email otomatis? | Feature scope | Out of scope MVP |

---

*File ini adalah living document. Setiap keputusan baru → update section terkait + catat tanggal.
Jangan tulis log percakapan — hanya fakta dan keputusan.*
