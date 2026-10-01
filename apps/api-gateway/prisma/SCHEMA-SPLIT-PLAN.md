# Rencana Split `schema.prisma` (multi-file via `prismaSchemaFolder`)

> Status: **✅ SELESAI (2026-05-31).** Dieksekusi di branch `refactor/prisma-schema-folder`.
> Hasil: 286 model + 67 enum dipecah ke 15 file di `prisma/schema/`; `prisma validate` OK,
> `migrate status` "up to date" (nol-migrasi), `db:generate` OK, `typecheck` hijau.
> Konteks awal: schema 9.388 baris / 286 model / 67 enum (4 domain tercampur).

## 1. Kenapa, dan kenapa BUKAN "max 400 baris"

Aturan max 400 baris ([root CLAUDE.md §5](../../../CLAUDE.md)) untuk **source code** (logika). Prisma
schema = **deklaratif, SSOT**, dan dikecualikan oleh skill `ref-audit`. Jadi target split
**bukan** angka baris, tapi **pemisahan per-domain** supaya 4 tim/area tidak saling tabrak di
satu file raksasa.

Satu-satunya cara split yang **didukung Prisma** adalah fitur `prismaSchemaFolder`
(pisah ke `prisma/schema/*.prisma`). Concat file manual TIDAK didukung `migrate`/`generate`.

## 2. Fakta kunci — ini refactor NOL-migrasi (alasan utama aman)

Memecah satu file jadi banyak file **tidak mengubah isi model satu byte pun** → tidak ada
diff skema → `prisma migrate dev` akan bilang *"already in sync, no migration created"*.
Tidak ada DDL, tidak sentuh `prisma/migrations/` yang sudah ada. Risiko utama = salah
ketik saat memindah blok (ketinggalan/dobel model) — ketangkap langsung oleh
`prisma validate` + `prisma generate`.

## 3. Komposisi saat ini (hasil scan)

| Pemilik | Prefix `@@map` | Jumlah |
| --- | --- | --- |
| Platform api-gateway (auth/menu/manager) | `m0_`, `m2_` | ~14 model |
| WMS/logistik (master+inbound/outbound/inventory legacy) | `m1_` | ~16 model |
| Clinic / Althea | `clinic_` | 14 model |
| **ERP (Senti ERP, `Erp*`)** | `sys_ adm_ md_ fin_ sls_ fa_ mfg_ pur_ pos_ inv_ pln_ erp_` | **242 model + ~65 enum** |

> Catatan: ERP **sudah** ada di schema (242 model — lihat commit §2.32–2.34), jadi rencana
> ini juga merapikan ERP yang sekarang mendominasi file. Skill `erp` yang masih bilang
> "Prisma model ditunda" sudah usang di titik ini.

## 4. Struktur folder usulan (`prisma/schema/`)

```
prisma/schema/
  datasource.prisma     # datasource db + generator (HARUS muncul tepat 1x, taruh di sini)
  platform.prisma       # User/Role/Menu/Permission/Department/Session/AuditLog/Manager*  (m0_,m2_)
  wms.prisma            # MasterData*/Delivery*/Inbound*/Outbound*/Inventory*             (m1_)
  clinic.prisma         # Clinic*                                                          (clinic_)
  erp-core.prisma       # ErpSetting/ErpMenu/ErpDocumentNumbering/ErpFiscalPeriod/ErpAuditLog
                        #  + adm_* (user/role/permission/access) + sys_*                  (sys_,adm_,erp_)
  erp-md.prisma         # md_* master data (item/partner/account/currency/dst — terbesar) (md_)
  erp-fin.prisma        # fin_*                                                            (fin_)
  erp-sls.prisma        # sls_*                                                            (sls_)
  erp-pur.prisma        # pur_*                                                            (pur_)
  erp-inv.prisma        # inv_*                                                            (inv_)
  erp-mfg.prisma        # mfg_*                                                            (mfg_)
  erp-fa.prisma         # fa_*                                                             (fa_)
  erp-pos.prisma        # pos_*                                                            (pos_)
  erp-pln.prisma        # pln_*                                                            (pln_)
  enums.prisma          # enum Erp* yang dipakai lintas-domain (DocumentStatus, dll)
```

**Enum:** enum yang hanya dipakai 1 domain → taruh di file domain itu. Enum lintas-domain
(`ErpDocumentStatus`, `ErpPostingStatus`, dll) → `enums.prisma`. Prisma meng-concat semua
file jadi satu namespace, jadi penempatan enum bebas asal tidak dobel.

## 5. Prasyarat & dampak konfigurasi

- Prisma **5.22** → `prismaSchemaFolder` masih **preview** (GA di Prisma 6). Wajib:
  ```prisma
  generator client {
    provider        = "prisma-client-js"
    binaryTargets   = ["native", "debian-openssl-3.0.x"]
    previewFeatures = ["prismaSchemaFolder"]
  }
  ```
- `package.json` `prisma: {}` kosong & tidak ada flag `--schema` → Prisma auto-detect
  `prisma/schema/` begitu folder ada. **Tidak perlu ubah script** `db:generate`/`db:migrate`.
- Seed (`prisma/seed*.ts`), `reset-db.ts`, migrations → **tidak terpengaruh** (mereka pakai
  `@prisma/client`, bukan baca file schema langsung).
- Update referensi dokumen yang menyebut "`prisma/schema.prisma`" sebagai single file
  ([api-gateway/CLAUDE.md](../CLAUDE.md), skill `erp`) → jadi "`prisma/schema/`".

## 6. Langkah eksekusi (saat di-approve)

1. `git checkout -b refactor/prisma-schema-folder` (jangan di `dev` langsung).
2. Tambah `previewFeatures = ["prismaSchemaFolder"]` di generator (sementara masih single file).
3. `mkdir prisma/schema`, pindah `datasource`+`generator` ke `prisma/schema/datasource.prisma`.
4. Pindah blok model/enum ke file per domain sesuai §4 (potong-tempel, jangan rewrite).
5. Hapus `prisma/schema.prisma` lama (sudah kosong/terpindah).
6. **Verifikasi (gerbang wajib):**
   ```bash
   npx prisma validate          # struktur valid, tak ada model dobel/hilang
   npx prisma format            # normalisasi
   npm run db:generate          # client regen sukses → TS service tak rusak
   npx prisma migrate status    # harus: up to date, NO pending migration
   npm run typecheck            # api-gateway hijau
   ```
   `migrate status` yang bilang "no pending" = bukti split = nol-perubahan-DB.
7. Sanity: hitung ulang `grep -c "^model " prisma/schema/*.prisma` = **286**, enum = **67**.
8. Commit `refactor(api-gateway): split prisma schema per-domain (prismaSchemaFolder)`.
9. Update [api-gateway/CLAUDE.md](../CLAUDE.md) + skill `erp` (ganti referensi single-file).

## 7. Rollback

`git checkout dev -- prisma/` (atau hapus branch). Karena nol-migrasi, tidak ada state DB
yang perlu dibalik.

## 8. Keputusan terbuka untuk user

- **Granularitas ERP:** pecah penuh 10 sub-file (§4) atau cukup 1 `erp.prisma` besar dulu?
  (Rekomendasi: penuh — `md_` saja 66 model.)
- **Gabung platform+WMS?** `m0/m2` dan `m1` bisa jadi satu `platform.prisma` (~30 model)
  kalau tak mau terlalu banyak file.
