# SQL Templates - Domain m2r

Template SQL ini adalah draft awal dari hasil dashboard mapping otomatis.

## Files
- summary.sql
- trends.sql
- breakdown.sql
- table.sql

## Placeholder Params
- :from_date (DATE)
- :to_date (DATE)
- :group_by (dimension column)
- :limit (INT)
- :offset (INT)

## Current Auto Picks
- primary_table: m2r_hppglobalsk
- metric_source: m2r_anggaran.nmsaldo
- trend_source: m2r_mutasi_stok_custom.ccustomint1
- filter_source: m2r_ap_card.apstatuslunas
- breakdown_metric_source: m2r_ap_card.apsaldoawal

Template sudah dibuat valid secara default (tanpa filter tanggal). Tambahkan kolom tanggal aktual pada baris komentar DATE(<date_column>) sebelum dipakai di production query.
