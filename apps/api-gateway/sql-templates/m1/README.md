# SQL Templates - Domain m1

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
- primary_table: m1_cogs_fifo_in
- metric_source: m1_item_transaction.saldojml
- trend_source: m1_cogs_fifo_in.cfiid
- filter_source: m1_contact.kkategorisalesman
- breakdown_metric_source: m1_contact.ktotalpiutang

Template sudah dibuat valid secara default (tanpa filter tanggal). Tambahkan kolom tanggal aktual pada baris komentar DATE(<date_column>) sebelum dipakai di production query.
