-- Migration: 20260603_004_erp_inv_daily_check_line_machine_workhours
-- Additive: line-level machine reference + work hours on inv_daily_check_lines.
-- Idempotent (table already exists from 20260603_003).

ALTER TABLE "inv_daily_check_lines" ADD COLUMN IF NOT EXISTS "machine_ref" TEXT;
ALTER TABLE "inv_daily_check_lines" ADD COLUMN IF NOT EXISTS "work_hours" DECIMAL(19,4);
