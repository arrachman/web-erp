-- Faktor konversi satuan jual dipindah ke md_units.conversion_factor (sudah ada sejak awal).
-- Hapus kolom redundan di md_items.
ALTER TABLE "md_items" DROP COLUMN IF EXISTS "field_unit_factor";
