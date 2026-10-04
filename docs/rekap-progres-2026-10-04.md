# Rekap ERP CV Bahtera Madani — 4 Oktober 2026

Rekap status pekerjaan ERP (repo `web-erp` di `/opt/erp-bahtera-madani`, live di https://erp.fr-labs.my.id) per Minggu, 4 Oktober 2026 malam. Dokumen ini merangkum yang sudah selesai, yang masih kurang, dan data yang perlu diminta ke klien.

## 1. Status besar

**Seluruh PRD Fase 1 / MVP selesai & live**: A1 CRM Sekolah, A2 Order Hub, A3 Dokumen Pengadaan, A4 Pajak Pengadaan, D1 Katalog, D2 Pembelian, D3 Persediaan, E1 Keuangan. Kode web-erp sudah di-push ke origin sampai commit `f8134e0`.

## 2. Yang dikerjakan hari ini

- **Pembersihan data Tier A+B** — hanya data `data-client` CV Bahtera Madani yang dipertahankan (119 partner, 60 item awal, 11 invoice, 1 penerimaan AR, 57 baris ledger); backup lengkap di server `backups/cleanup-2026-10-04/`. Jurnal & periode non-2026 dihapus; hanya periode **Oktober 2026 OPEN**, sisanya CLOSED (kontrol posting).
- **Penomoran ulang invoice** — 11 invoice menjadi SI000001–SI000011 kronologis, penerimaan AR menjadi IP000001, snapshot ledger mengikuti (0 mismatch).
- **Penomoran dokumen menyeluruh** — 58 kode aktif di `sys_document_numberings`; Proforma Invoice dipisah ke kode sendiri `PFI`.
- **Menu MVP** — modul non-MVP (Produksi, Fixed Asset, POS) dan menu master/admin non-MVP disembunyikan via migrasi DB.
- **Sorting default** semua daftar transaksi = nomor dokumen menurun.
- **A1 Sekolah terisi** — profil CRM untuk 116 sekolah (TK 103, PAUD 13), pipeline dari invoice, 5 kontak Kepala Sekolah.
- **D1 katalog terisi** — profil katalog 60 item dari derivasi nama/kategori (berlabel derivasi, bukan data sumber).
- **Perbaikan live** — bug enrich `name` (2 endpoint), Freight Receivable (RP) & Freight Payable (PP) dihubungkan ke backend khusus, halaman Cash/Bank Transfer yang mati dihapus, fix translate Chrome + URL shell `/app/*`.

### Sesi malam (permintaan 5–8 + GL)

- **#5** Periode **September 2026 dibuka lagi** (REOPENED via API resmi) — invoice DRAFT SI000008 (10 Sep, Rp227.700) sekarang bisa diposting. Nov/Des tetap CLOSED, dibuka saat masuk bulannya.
- **#6** Unit systemd **`web-erp.service` diperbaiki** (sebelumnya rusak total): direktori kerja & mode production benar, sekarang service ini yang menjalankan frontend live, enabled + linger aktif (naik sendiri setelah reboot). Supervisi terbukti: proses dibunuh paksa → otomatis hidup lagi, situs 200.
- **#7** Working tree **sentient-factory ter-commit lokal** (5 commit, terakhir `3349668a`) — semua sinkronisasi A1–A4 + GL tidak bisa lagi hilang karena checkout/pull. Belum di-push. Yang sengaja tidak di-commit: `llm.py` (AI engine) dan `next-env.d.ts`.
- **Bug produksi ditemukan & diperbaiki**: mapper invoice di sentient-factory menulis `settlementStatus: 'UNSETTLED'` (bukan nilai enum valid) sehingga **API live tidak bisa membuat sales invoice sama sekali**. Sudah diperbaiki ke `'UNPAID'`, gateway di-rebuild + restart, terverifikasi.
- **Posting GL sales invoice LIVE** — implementasi lengkap web-erp (Dr Piutang / Cr Penjualan per baris / Cr PPN / Dr Diskon / uang muka / potong stok untuk invoice tanpa DO) di-sync ke sentient-factory dan terverifikasi live: invoice uji Rp20.000 menghasilkan jurnal balance (Dr 1120.01.001 / Cr 4104.01.001), `arLedgerEntryId` terisi, movement stok SII000001 terbit, dan reversal membersihkan semuanya. Invoice uji dihapus, counter direset.

## 3. Dokumen impor BENDERA — terhapus keliru, sudah dipulihkan

Impor `PESANAN BENDERA.xlsx` selesai dan diproses penuh (5 item master baru id 834–838; SO000001 ROSALIA Rp2.220.000; SO000002 TK AHMAD YANI Rp1.185.000; DO000001/DO000002 POSTED; SI000012/SI000013 POSTED — status Order Hub: DITAGIH).

Pada malam yang sama rantai dokumen ini **sempat terhapus karena kekeliruan audit**: audit A2 menyangka kedua order adalah sisa data uji (saat audit keduanya memang masih DRAFT buatan hari yang sama), dan penghapusan "order uji" disetujui atas dasar itu — padahal keduanya order klien asli dari file BENDERA. Setelah kekeliruan ditemukan dari memory, user memerintahkan pemulihan; rantai **dipulihkan dari backup pg_dump pra-penghapusan** dan seluruh hitungan terverifikasi identik dengan sebelum terhapus (2 order APPROVED, 2 DO POSTED, invoice 35/36 POSTED, counter SO 3 / DO 3 / SI 14, ledger tetap 57). Pelajaran: dokumen buatan hari yang sama bukan otomatis data uji — data klien yang diimpor dan diproses di hari yang sama memang berumur nol hari.

Catatan: SI000012/SI000013 diposting **sebelum** posting GL live, jadi keduanya belum punya baris jurnal di `fin_ledger_entries` (invoice lama hasil impor punya, karena ditulis langsung oleh script impor).

`CICILAN.xlsx` ternyata kosong (header + 1 baris kosong) — tidak ada data cicilan di dalamnya.

## 4. Data yang perlu diminta ke klien

1. Daftar supplier & penerbit buku: nama, kontak, alamat — termasuk supplier ATK, konsumsi, furnitur.
2. Harga beli per item dari masing-masing supplier.
3. Pemetaan item → supplier utama (vendor) untuk tiap produk.
4. Kebijakan stok minimum & maksimum per item (dasar Purchase Suggestion).
5. Persentase rabat per penerbit/supplier, dan skema konsinyasi bila ada.
6. Nama penerbit + HET tiap buku/item katalog; kelas dan kurikulum untuk buku yang belum terpetakan.
7. Profil resmi CV Bahtera Madani: alamat lengkap, NPWP, telepon, email, website (saat ini masih dummy Sentient Factory — tampil di kop semua PDF dokumen).
8. Data sekolah: NPSN, jumlah siswa, pagu BOS per sekolah per tahun/tahap (116 sekolah masih kosong).
9. Rincian cicilan per sekolah: total pembayaran, jumlah cicilan, yang sudah dibayar, sisa.
10. Saldo stok awal per gudang (hasil stok opname terakhir) untuk opening stock.
11. Penetapan akun penjualan untuk kategori LKS, Alat Peraga, Seragam, Cetak, Paket — kelimanya belum punya akun; posting GL menolak item di kategori itu sampai akun ditetapkan.

Catatan aturan (CLAUDE.md §6, commit `8c6e106`): data bisnis yang tidak ada di repo/DB/file klien **tidak boleh dikarang** — field dibiarkan NULL dan datanya diminta ke klien lewat user.

## 5. Kekurangan & catatan teknis tersisa

- **D2 belum bisa jalan riil** — sistem 100% siap (three-way match, rabat, suggestion, counter dokumen) tapi data bisnis 0%: nol supplier, vendor item 0/60, min/max stok 0/60. Terblokir poin 1–5 di atas.
- **Akun penjualan kategori baru** — lihat poin 11 di atas (PAKET termasuk item Paket Internet hasil impor).
- **VOID invoice POSTED** — ada di web-erp, belum ter-sync ke API live (invoice POSTED belum bisa dibatalkan via UI).
- **Tepi posting GL (gagal-aman)** — invoice TAX_INCLUSIVE dengan pajak per baris dan invoice dengan biaya lain (otherCost) di header akan ditolak oleh assertion balance upstream; data saat ini tidak ada yang seperti itu.
- **Mutasi stok dari DO** — jalur invoice standalone sudah menulis movement; jalur DO belum diaudit penuh setelah sync GL.
- **SIPLah** — masih impor file (sesuai mitigasi PRD); API merchant langsung menunggu akses.
- **SI000008** masih DRAFT — sudah bisa diposting kapan saja.
- **Push tertunda** — commit lokal sentient-factory (5) dan commit web-erp setelah `f8134e0` (antara lain aturan §6) belum di-push, menunggu instruksi.

## 6. Angka kunci (terakhir terverifikasi, 4 Okt 2026 ~21:15 WIB)

| Hal | Nilai |
|---|---|
| Partner | 119 (116 sekolah CUST-SCHOOL + 3 salesman) |
| Item | 65 (60 impor awal + 5 baru BENDERA) |
| Invoice penjualan aktif | 13 (SI000001–SI000013; 1 DRAFT = SI000008) |
| Penerimaan AR | 1 (IP000001) |
| Baris ledger | 57 |
| Sales order / DO aktif | 2 SO APPROVED, 2 DO POSTED (rantai BENDERA, lihat §3) |
| Periode fiskal 2026 | Okt OPEN, Sep REOPENED, lainnya CLOSED |
| Kode penomoran aktif | 58 |
| Frontend live | systemd `web-erp.service`, BUILD_ID `qbF8JSsLZmugNkkAz1dvc` |
| API live | container `sentient-infra-api-gateway` (copy sentient-factory) |
