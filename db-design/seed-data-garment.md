# Senti ERP — Data Simulasi: Pabrik Garmen & Tekstil

> **Status:** Dokumentasi referensi (belum seed script). Dipakai sebagai acuan demo,
> pengujian UI, dan pengembangan fitur ERP di konteks industri garmen/tekstil.
>
> **Konteks bisnis:** PT Nusantara Garmen Sentosa — pabrik garmen skala menengah
> (500 karyawan) berbasis di Bandung. Memproduksi kemeja, celana, jaket untuk
> pasar ekspor dan lokal. Bahan baku utama dari supplier lokal & impor.

---

## 1. Struktur Organisasi

### 1.1 Branch (`md_branches`)

| code | name | city | isActive |
|------|------|------|----------|
| `HO` | Head Office Bandung | Bandung | true |
| `FAB-01` | Pabrik Utama Majalaya | Majalaya, Bandung | true |
| `FAB-02` | Pabrik Cabang Cimahi | Cimahi | true |
| `GDG-01` | Gudang Pusat Gedebage | Gedebage, Bandung | true |

### 1.2 Division (`md_divisions`)

| code | name | isActive |
|------|------|----------|
| `DIV-MFG` | Manufacturing | true |
| `DIV-MKT` | Sales & Marketing | true |
| `DIV-FIN` | Finance & Accounting | true |
| `DIV-OPS` | Operations | true |
| `DIV-QC` | Quality Control | true |

### 1.3 Department (`md_departments`)

| code | name | division | isActive |
|------|------|----------|----------|
| `DEPT-CUT` | Cutting | DIV-MFG | true |
| `DEPT-SEW` | Sewing / Assembly | DIV-MFG | true |
| `DEPT-FIN-OPS` | Finishing | DIV-MFG | true |
| `DEPT-EMB` | Embroidery & Printing | DIV-MFG | true |
| `DEPT-QC` | Quality Inspection | DIV-QC | true |
| `DEPT-WH` | Warehouse | DIV-OPS | true |
| `DEPT-PUR` | Purchasing | DIV-OPS | true |
| `DEPT-SLS` | Sales | DIV-MKT | true |
| `DEPT-ACC` | Accounting | DIV-FIN | true |
| `DEPT-HR` | Human Resources | DIV-OPS | true |

### 1.4 Sub-Department (`md_sub_departments`)

| code | name | department | isActive |
|------|------|------------|----------|
| `SUB-CUT-MKR` | Marker Making | DEPT-CUT | true |
| `SUB-CUT-SPR` | Spreading | DEPT-CUT | true |
| `SUB-CUT-CUT` | Cutting | DEPT-CUT | true |
| `SUB-SEW-LN` | Line Sewing 1 | DEPT-SEW | true |
| `SUB-SEW-L2` | Line Sewing 2 | DEPT-SEW | true |
| `SUB-SEW-L3` | Line Sewing 3 | DEPT-SEW | true |
| `SUB-FIN-BTN` | Button Attach | DEPT-FIN-OPS | true |
| `SUB-FIN-IRN` | Ironing | DEPT-FIN-OPS | true |
| `SUB-FIN-PKG` | Packing | DEPT-FIN-OPS | true |
| `SUB-EMB-EMB` | Embroidery | DEPT-EMB | true |
| `SUB-EMB-PRT` | Screen Printing | DEPT-EMB | true |
| `SUB-QC-IHK` | Inline Checking | DEPT-QC | true |
| `SUB-QC-FNL` | Final Inspection (AQL) | DEPT-QC | true |

### 1.5 Cost Center (`md_cost_centers`)

| code | name | department | isActive |
|------|------|------------|----------|
| `CC-CUT` | Cost Center Cutting | DEPT-CUT | true |
| `CC-SEW` | Cost Center Sewing | DEPT-SEW | true |
| `CC-FIN` | Cost Center Finishing | DEPT-FIN-OPS | true |
| `CC-EMB` | Cost Center Embroidery | DEPT-EMB | true |
| `CC-QC` | Cost Center QC | DEPT-QC | true |
| `CC-WH` | Cost Center Warehouse | DEPT-WH | true |
| `CC-OVH` | Factory Overhead | DIV-MFG | true |
| `CC-SLS` | Selling Cost | DIV-MKT | true |
| `CC-ADM` | General & Admin | DIV-FIN | true |

### 1.6 Warehouse (`md_warehouses`)

| code | name | branch | isActive |
|------|------|--------|----------|
| `WH-RM` | Gudang Bahan Baku | GDG-01 | true |
| `WH-WIP` | Gudang WIP | FAB-01 | true |
| `WH-FG` | Gudang Barang Jadi | GDG-01 | true |
| `WH-ACC` | Gudang Aksesori & Trims | GDG-01 | true |
| `WH-PKG` | Gudang Packaging | GDG-01 | true |
| `WH-RJT` | Gudang Barang Reject | FAB-01 | true |

### 1.7 Location (`md_locations`)

| code | name | warehouse | isActive |
|------|------|-----------|----------|
| `LOC-RM-A1` | Rak A-1 Kain Katun | WH-RM | true |
| `LOC-RM-A2` | Rak A-2 Kain Polyester | WH-RM | true |
| `LOC-RM-B1` | Rak B-1 Kain Denim | WH-RM | true |
| `LOC-ACC-C1` | Rak C-1 Kancing | WH-ACC | true |
| `LOC-ACC-C2` | Rak C-2 Resleting | WH-ACC | true |
| `LOC-FG-D1` | Rak D-1 Kemeja | WH-FG | true |
| `LOC-FG-D2` | Rak D-2 Celana | WH-FG | true |
| `LOC-FG-D3` | Rak D-3 Jaket | WH-FG | true |

---

## 2. Master Data Item

### 2.1 Unit (`md_units`)

| code | name | isActive |
|------|------|----------|
| `MTR` | Meter | true |
| `YRD` | Yard | true |
| `KG` | Kilogram | true |
| `GRM` | Gram | true |
| `PCS` | Pcs | true |
| `DZN` | Dozen | true |
| `ROLL` | Roll | true |
| `BOX` | Box | true |
| `BAL` | Bal | true |
| `SET` | Set | true |

### 2.2 Item Category (`md_item_categories`)

| code | name | isActive |
|------|------|----------|
| `CAT-RM-FB` | Bahan Baku - Kain | true |
| `CAT-RM-TR` | Bahan Baku - Trims & Aksesori | true |
| `CAT-WIP` | Barang Setengah Jadi (WIP) | true |
| `CAT-FG-CM` | Barang Jadi - Kemeja | true |
| `CAT-FG-TR` | Barang Jadi - Celana & Rok | true |
| `CAT-FG-JK` | Barang Jadi - Jaket & Outerwear | true |
| `CAT-PKG` | Packaging | true |
| `CAT-SVC` | Jasa | true |

### 2.3 Brand (`md_brands`)

| code | name | isActive |
|------|------|----------|
| `BRD-NSG` | Nusantara Garmen (own label) | true |
| `BRD-CLT` | CraftLite (private label buyer A) | true |
| `BRD-URB` | Urbanwear Co. (buyer B) | true |
| `BRD-EXP` | Export Generic (unlabeled) | true |

### 2.4 Material (`md_materials`)

| code | name | isActive |
|------|------|----------|
| `MAT-CTN` | Cotton 100% | true |
| `MAT-PLY` | Polyester | true |
| `MAT-TC` | TC (Teteron Cotton 65/35) | true |
| `MAT-DNM` | Denim | true |
| `MAT-RYN` | Rayon | true |
| `MAT-LNN` | Linen | true |
| `MAT-NYL` | Nylon | true |
| `MAT-SPX` | Spandex/Lycra | true |

### 2.5 Size (`md_sizes`)

| code | name | isActive |
|------|------|----------|
| `SZ-XS` | XS | true |
| `SZ-S` | S | true |
| `SZ-M` | M | true |
| `SZ-L` | L | true |
| `SZ-XL` | XL | true |
| `SZ-2XL` | 2XL | true |
| `SZ-3XL` | 3XL | true |
| `SZ-28` | 28 (celana) | true |
| `SZ-30` | 30 | true |
| `SZ-32` | 32 | true |
| `SZ-34` | 34 | true |
| `SZ-36` | 36 | true |

### 2.6 Item — Bahan Baku Kain (`md_items`, type=INVENTORY, category=CAT-RM-FB)

| code | name | unit | material | isActive |
|------|------|------|----------|----------|
| `RM-CTN-30S` | Kain Katun 30s (putih) | YRD | MAT-CTN | true |
| `RM-CTN-40S` | Kain Katun 40s (putih) | YRD | MAT-CTN | true |
| `RM-CTN-STRP` | Kain Katun Stripe | YRD | MAT-CTN | true |
| `RM-CTN-CHK` | Kain Katun Kotak-kotak | YRD | MAT-CTN | true |
| `RM-TC-HS` | Kain TC Hard Shirt | YRD | MAT-TC | true |
| `RM-DNM-12` | Kain Denim 12oz | YRD | MAT-DNM | true |
| `RM-DNM-14` | Kain Denim 14oz (heavy) | YRD | MAT-DNM | true |
| `RM-PLY-MSH` | Kain Polyester Mesh | MTR | MAT-PLY | true |
| `RM-RYN-VIS` | Kain Rayon Viscose | MTR | MAT-RYN | true |
| `RM-LNN-WV` | Kain Linen Woven | MTR | MAT-LNN | true |
| `RM-NYL-RPS` | Kain Nylon Ripstop | MTR | MAT-NYL | true |

### 2.7 Item — Aksesori & Trims (`md_items`, type=INVENTORY, category=CAT-RM-TR)

| code | name | unit | isActive |
|------|------|------|----------|
| `TR-BTN-PLY-S` | Kancing Polyester 4L (besar) | PCS | true |
| `TR-BTN-PLY-K` | Kancing Polyester 4L (kecil) | PCS | true |
| `TR-BTN-MTL` | Kancing Metal (jeans) | PCS | true |
| `TR-ZPR-NL-30` | Resleting Nylon 30cm | PCS | true |
| `TR-ZPR-NL-50` | Resleting Nylon 50cm | PCS | true |
| `TR-ZPR-MTL` | Resleting Metal (jaket) | PCS | true |
| `TR-INTLN` | Kain Keras / Interlining | MTR | true |
| `TR-THRD-CTN` | Benang Jahit Cotton (putih) | ROLL | true |
| `TR-THRD-PLY` | Benang Jahit Polyester (hitam) | ROLL | true |
| `TR-ELST-2` | Karet Elastik 2cm | MTR | true |
| `TR-ELST-4` | Karet Elastik 4cm | MTR | true |
| `TR-LBL-MN` | Label Merek Woven | PCS | true |
| `TR-LBL-SZ` | Label Ukuran | PCS | true |
| `TR-LBL-CR` | Label Care Instruction | PCS | true |
| `TR-RVT` | Rivet Jeans (metal) | PCS | true |
| `TR-SNAP` | Snap Button | PCS | true |
| `TR-VELC` | Velcro 2.5cm | MTR | true |

### 2.8 Item — Packaging (`md_items`, type=INVENTORY, category=CAT-PKG)

| code | name | unit | isActive |
|------|------|------|----------|
| `PKG-POLY-S` | Plastik Polybag (S) | PCS | true |
| `PKG-POLY-M` | Plastik Polybag (M) | PCS | true |
| `PKG-POLY-L` | Plastik Polybag (L) | PCS | true |
| `PKG-BOX-12` | Karton Box (12 pcs) | PCS | true |
| `PKG-BOX-24` | Karton Box (24 pcs) | PCS | true |
| `PKG-HNG` | Hanger Plastik | PCS | true |
| `PKG-TAG` | Price Tag + String | PCS | true |
| `PKG-TAPE` | Selotip Packing | ROLL | true |

### 2.9 Item — Barang Jadi (`md_items`, type=INVENTORY)

| code | name | category | brand | unit | isActive |
|------|------|----------|-------|------|----------|
| `FG-SHT-CTN-S` | Kemeja Katun Lengan Panjang (S) | CAT-FG-CM | BRD-NSG | PCS | true |
| `FG-SHT-CTN-M` | Kemeja Katun Lengan Panjang (M) | CAT-FG-CM | BRD-NSG | PCS | true |
| `FG-SHT-CTN-L` | Kemeja Katun Lengan Panjang (L) | CAT-FG-CM | BRD-NSG | PCS | true |
| `FG-SHT-CTN-XL` | Kemeja Katun Lengan Panjang (XL) | CAT-FG-CM | BRD-NSG | PCS | true |
| `FG-SHT-STR-M` | Kemeja Katun Stripe (M) | CAT-FG-CM | BRD-CLT | PCS | true |
| `FG-SHT-STR-L` | Kemeja Katun Stripe (L) | CAT-FG-CM | BRD-CLT | PCS | true |
| `FG-POLO-M` | Polo Shirt TC (M) | CAT-FG-CM | BRD-NSG | PCS | true |
| `FG-POLO-L` | Polo Shirt TC (L) | CAT-FG-CM | BRD-NSG | PCS | true |
| `FG-POLO-XL` | Polo Shirt TC (XL) | CAT-FG-CM | BRD-NSG | PCS | true |
| `FG-JNS-30` | Celana Jeans Denim 30 | CAT-FG-TR | BRD-URB | PCS | true |
| `FG-JNS-32` | Celana Jeans Denim 32 | CAT-FG-TR | BRD-URB | PCS | true |
| `FG-JNS-34` | Celana Jeans Denim 34 | CAT-FG-TR | BRD-URB | PCS | true |
| `FG-CHNO-30` | Celana Chino (30) | CAT-FG-TR | BRD-NSG | PCS | true |
| `FG-CHNO-32` | Celana Chino (32) | CAT-FG-TR | BRD-NSG | PCS | true |
| `FG-JKT-NYL-M` | Jaket Nylon Ripstop (M) | CAT-FG-JK | BRD-CLT | PCS | true |
| `FG-JKT-NYL-L` | Jaket Nylon Ripstop (L) | CAT-FG-JK | BRD-CLT | PCS | true |
| `FG-JKT-DNM-M` | Jaket Denim (M) | CAT-FG-JK | BRD-URB | PCS | true |
| `FG-JKT-DNM-L` | Jaket Denim (L) | CAT-FG-JK | BRD-URB | PCS | true |

---

## 3. Master Data Partner

### 3.1 Kategori Partner (`md_partner_categories`)

| code | name | isActive |
|------|------|----------|
| `PC-SUP-KN` | Supplier Kain | true |
| `PC-SUP-TR` | Supplier Trims & Aksesori | true |
| `PC-SUP-PKG` | Supplier Packaging | true |
| `PC-SUP-CHM` | Supplier Chemical & Dye | true |
| `PC-CUS-EXP` | Customer Ekspor | true |
| `PC-CUS-DOM` | Customer Lokal/Domestik | true |
| `PC-CUS-DIS` | Distributor | true |
| `PC-EXP-LOG` | Ekspedisi / Freight Forwarder | true |

### 3.2 Partner Supplier (`md_partners`, type=SUPPLIER)

| code | name | NPWP | category | city | isActive |
|------|------|------|----------|------|----------|
| `SUP-001` | PT Indah Tekstil Nusantara | 01.234.567.8-001.000 | PC-SUP-KN | Bandung | true |
| `SUP-002` | CV Makmur Sandang Jaya | 02.345.678.9-002.000 | PC-SUP-KN | Pekalongan | true |
| `SUP-003` | PT Primissima Textile | 03.456.789.0-003.000 | PC-SUP-KN | Yogyakarta | true |
| `SUP-004` | PT Star Denim Indonesia | 04.567.890.1-004.000 | PC-SUP-KN | Surabaya | true |
| `SUP-005` | CV Berkah Trims Sejahtera | 05.678.901.2-005.000 | PC-SUP-TR | Bandung | true |
| `SUP-006` | PT Ching Fong Button | 06.789.012.3-006.000 | PC-SUP-TR | Jakarta | true |
| `SUP-007` | CV Maju Zipper Indonesia | 07.890.123.4-007.000 | PC-SUP-TR | Tangerang | true |
| `SUP-008` | PT Surya Packaging Prima | 08.901.234.5-008.000 | PC-SUP-PKG | Bekasi | true |
| `SUP-009` | CV Sejati Benang Kencana | 09.012.345.6-009.000 | PC-SUP-TR | Bandung | true |

### 3.3 Partner Customer (`md_partners`, type=CUSTOMER)

| code | name | category | country | isActive |
|------|------|----------|---------|----------|
| `CUS-001` | CraftLite International Ltd. | PC-CUS-EXP | United States | true |
| `CUS-002` | Urbanwear Co. GmbH | PC-CUS-EXP | Germany | true |
| `CUS-003` | Sunrise Fashion Japan K.K. | PC-CUS-EXP | Japan | true |
| `CUS-004` | PT Matahari Dept Store Tbk | PC-CUS-DOM | Indonesia | true |
| `CUS-005` | PT Ramayana Lestari Sentosa | PC-CUS-DOM | Indonesia | true |
| `CUS-006` | CV Bintang Mode Surabaya | PC-CUS-DIS | Indonesia | true |
| `CUS-007` | CV Jaya Fashion Medan | PC-CUS-DIS | Indonesia | true |

### 3.4 Partner Ekspedisi

| code | name | category | isActive |
|------|------|----------|----------|
| `EXP-001` | PT JNE Express | PC-EXP-LOG | true |
| `EXP-002` | PT Samudera Indonesia | PC-EXP-LOG | true |
| `EXP-003` | PT Ekspedisi Hasanah (freight forwarder) | PC-EXP-LOG | true |

---

## 4. Chart of Accounts (CoA)

> Mengikuti standar CoA garmen Indonesia (PSAK). Kode 4-digit dengan sub-akun.
> Semua akun masuk `md_accounts` (`type` = ASSET / LIABILITY / EQUITY / REVENUE / EXPENSE).

### 4.1 Aset Lancar (1xxx)

| code | name | type | isActive |
|------|------|------|----------|
| `1100` | Kas & Bank | ASSET | true |
| `1101` | Kas Kecil | ASSET | true |
| `1102` | Bank BCA - Rekening Operasional | ASSET | true |
| `1103` | Bank Mandiri - Rekening Giro | ASSET | true |
| `1200` | Piutang Usaha | ASSET | true |
| `1201` | Piutang Ekspor (AR Valas) | ASSET | true |
| `1202` | Piutang Lokal (AR IDR) | ASSET | true |
| `1290` | Cadangan Kerugian Piutang | ASSET | true |
| `1300` | Persediaan | ASSET | true |
| `1301` | Persediaan Bahan Baku (kain) | ASSET | true |
| `1302` | Persediaan Aksesori & Trims | ASSET | true |
| `1303` | Persediaan Barang Dalam Proses (WIP) | ASSET | true |
| `1304` | Persediaan Barang Jadi | ASSET | true |
| `1305` | Persediaan Packaging | ASSET | true |
| `1400` | Biaya Dibayar di Muka | ASSET | true |
| `1401` | Asuransi Dibayar di Muka | ASSET | true |
| `1402` | Sewa Dibayar di Muka | ASSET | true |

### 4.2 Aset Tetap (1xxx)

| code | name | type | isActive |
|------|------|------|----------|
| `1500` | Aset Tetap | ASSET | true |
| `1501` | Tanah & Bangunan Pabrik | ASSET | true |
| `1502` | Mesin Jahit & Obras | ASSET | true |
| `1503` | Mesin Potong (Cutting Machine) | ASSET | true |
| `1504` | Mesin Bordir (Embroidery Machine) | ASSET | true |
| `1505` | Mesin Setrika Uap (Steam Ironing) | ASSET | true |
| `1506` | Kendaraan Operasional | ASSET | true |
| `1507` | Peralatan Kantor & IT | ASSET | true |
| `1590` | Akumulasi Penyusutan — Mesin | ASSET | true |
| `1591` | Akumulasi Penyusutan — Bangunan | ASSET | true |
| `1592` | Akumulasi Penyusutan — Kendaraan | ASSET | true |

### 4.3 Kewajiban (2xxx)

| code | name | type | isActive |
|------|------|------|----------|
| `2100` | Hutang Usaha | LIABILITY | true |
| `2101` | Hutang Supplier Kain | LIABILITY | true |
| `2102` | Hutang Supplier Trims | LIABILITY | true |
| `2200` | Hutang Bank | LIABILITY | true |
| `2201` | Kredit Modal Kerja — Bank BRI | LIABILITY | true |
| `2300` | Kewajiban Lancar Lainnya | LIABILITY | true |
| `2301` | Hutang Pajak PPN | LIABILITY | true |
| `2302` | Hutang PPh 21 (karyawan) | LIABILITY | true |
| `2303` | Hutang PPh 23 | LIABILITY | true |
| `2304` | Uang Muka dari Pelanggan | LIABILITY | true |
| `2305` | Hutang Gaji & Upah | LIABILITY | true |
| `2306` | Hutang BPJS Ketenagakerjaan | LIABILITY | true |
| `2307` | Hutang BPJS Kesehatan | LIABILITY | true |

### 4.4 Ekuitas (3xxx)

| code | name | type | isActive |
|------|------|------|----------|
| `3100` | Modal Disetor | EQUITY | true |
| `3200` | Laba Ditahan | EQUITY | true |
| `3300` | Laba / Rugi Tahun Berjalan | EQUITY | true |

### 4.5 Pendapatan (4xxx)

| code | name | type | isActive |
|------|------|------|----------|
| `4100` | Penjualan Ekspor | REVENUE | true |
| `4101` | Penjualan FOB (USD) | REVENUE | true |
| `4102` | Penjualan CNF (USD) | REVENUE | true |
| `4200` | Penjualan Lokal | REVENUE | true |
| `4201` | Penjualan Lokal — Distributor | REVENUE | true |
| `4202` | Penjualan Lokal — Ritel | REVENUE | true |
| `4300` | Pendapatan Jasa Makloon | REVENUE | true |
| `4900` | Pendapatan Lain-lain | REVENUE | true |

### 4.6 Harga Pokok Penjualan / COGS (5xxx)

| code | name | type | isActive |
|------|------|------|----------|
| `5100` | HPP Bahan Baku | EXPENSE | true |
| `5101` | Pemakaian Kain | EXPENSE | true |
| `5102` | Pemakaian Aksesori & Trims | EXPENSE | true |
| `5103` | Pemakaian Packaging | EXPENSE | true |
| `5200` | Biaya Tenaga Kerja Langsung | EXPENSE | true |
| `5201` | Upah Cutting | EXPENSE | true |
| `5202` | Upah Sewing (per pcs/lusinan) | EXPENSE | true |
| `5203` | Upah Finishing | EXPENSE | true |
| `5204` | Upah Embroidery | EXPENSE | true |
| `5205` | Lembur Pabrik | EXPENSE | true |
| `5300` | Overhead Pabrik | EXPENSE | true |
| `5301` | Penyusutan Mesin | EXPENSE | true |
| `5302` | Penyusutan Bangunan Pabrik | EXPENSE | true |
| `5303` | Listrik & Air Pabrik | EXPENSE | true |
| `5304` | Biaya Pemeliharaan Mesin | EXPENSE | true |
| `5305` | Biaya QC & Testing | EXPENSE | true |
| `5306` | Biaya Subkontrak (jahit luar) | EXPENSE | true |

### 4.7 Biaya Operasional (6xxx)

| code | name | type | isActive |
|------|------|------|----------|
| `6100` | Biaya Penjualan | EXPENSE | true |
| `6101` | Komisi Sales | EXPENSE | true |
| `6102` | Biaya Ekspedisi & Freight | EXPENSE | true |
| `6103` | Biaya Promosi & Pameran | EXPENSE | true |
| `6104` | Biaya Packaging Ekspor | EXPENSE | true |
| `6200` | Biaya Umum & Administrasi | EXPENSE | true |
| `6201` | Gaji Karyawan Staff | EXPENSE | true |
| `6202` | Biaya BPJS Ketenagakerjaan | EXPENSE | true |
| `6203` | Biaya BPJS Kesehatan | EXPENSE | true |
| `6204` | Sewa Gedung Kantor | EXPENSE | true |
| `6205` | Listrik & Air Kantor | EXPENSE | true |
| `6206` | Telekomunikasi & Internet | EXPENSE | true |
| `6207` | Perlengkapan Kantor | EXPENSE | true |
| `6208` | Biaya Perjalanan Dinas | EXPENSE | true |
| `6209` | Biaya Asuransi | EXPENSE | true |
| `6210` | Biaya Legal & Profesional | EXPENSE | true |
| `6211` | Penyusutan Peralatan Kantor | EXPENSE | true |
| `6300` | Biaya Keuangan | EXPENSE | true |
| `6301` | Bunga Pinjaman Bank | EXPENSE | true |
| `6302` | Biaya Administrasi Bank | EXPENSE | true |
| `6303` | Selisih Kurs (rugi) | EXPENSE | true |

---

## 5. Data Referensi Lain

### 5.1 Currency (`md_currencies`)

| code | name | symbol | isDefault | isActive |
|------|------|--------|-----------|----------|
| `IDR` | Indonesian Rupiah | Rp | true | true |
| `USD` | US Dollar | $ | false | true |
| `EUR` | Euro | € | false | true |
| `JPY` | Japanese Yen | ¥ | false | true |

### 5.2 Payment Term (`md_payment_terms`)

| code | name | days | isActive |
|------|------|------|----------|
| `COD` | Cash on Delivery | 0 | true |
| `NET-7` | Net 7 Hari | 7 | true |
| `NET-14` | Net 14 Hari | 14 | true |
| `NET-30` | Net 30 Hari | 30 | true |
| `NET-45` | Net 45 Hari | 45 | true |
| `NET-60` | Net 60 Hari | 60 | true |
| `LC-90` | L/C 90 Hari (ekspor) | 90 | true |
| `TT-ADV` | T/T Advance 30% + Pelunasan | 0 | true |

### 5.3 Tax (`md_taxes`)

| code | name | rate | isActive |
|------|------|------|----------|
| `PPN-11` | PPN 11% | 11.00 | true |
| `PPN-0-EXP` | PPN 0% (Ekspor) | 0.00 | true |
| `PPH-23-2` | PPh 23 (2% jasa) | 2.00 | true |
| `NOTAX` | Non-Taxable | 0.00 | true |

---

## 6. Catatan Implementasi

### Ketika membuat seed script (`seed-garment.ts`)

1. **Urutan insert wajib mengikuti dependency:**
   `currencies` → `countries` → `provinces` → `cities`
   → `units` → `item_categories` → `brands` → `materials` → `sizes`
   → `branches` → `divisions` → `departments` → `sub_departments` → `cost_centers`
   → `warehouses` → `locations`
   → `partner_categories` → `partners`
   → `accounts` → `payment_terms` → `taxes`
   → `items`

2. **Idempotent:** gunakan `upsert` dengan `where: { code }` agar seed aman
   dijalankan ulang tanpa duplikasi.

3. **BigInt PK:** semua FK skalar di schema ERP menggunakan `BigInt` —
   seed harus cast ID hasil `upsert` dengan benar sebelum dipakai sebagai FK.

4. **Legacy codes:** tiap item/partner/akun idealnya punya `legacyCode`
   (jika migrasi dari MyERP+ nantinya diperlukan).

5. **Volume dummy wajar untuk demo:**
   - Item bahan baku: ~30 SKU
   - Item barang jadi: ~25 SKU
   - Supplier: ~10 partner
   - Customer: ~8 partner (3 ekspor, 5 lokal/distributor)
   - CoA: ~80 akun

### Konteks bisnis untuk pengujian flow transaksi

Skenario tipikal yang bisa diuji dengan data ini:

| Skenario | Flow | Modul |
|----------|------|-------|
| Terima PO ekspor (kemeja katun L, 1.200 pcs) dari CraftLite | PO masuk → Work Order → kebutuhan bahan baku | M5 → M6 |
| Beli kain katun 40s dari PT Indah Tekstil | PO pembelian → GRN → invoice AP | M4 |
| Potong & jahit batch 500 kemeja | Work Order → BOM consumption → WIP → FG | M6 |
| QC final inspection (AQL 2.5) → reject 12 pcs | Inventory adjustment → reject stock | M3 |
| Kirim 480 pcs ke CraftLite via Ekspedisi Hasanah | Delivery → invoice AR → payment T/T | M5 → M2 |
| Rekonsiliasi kurs USD/IDR untuk invoice ekspor | Selisih kurs realisasi | M2 |
