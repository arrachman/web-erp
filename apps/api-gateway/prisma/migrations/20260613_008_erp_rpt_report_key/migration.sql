-- Bind report templates to reports via a `report_key` (`<module>.<report>`, e.g. `fin.trial-balance`).
-- The matching active template drives that report's PDF render; NULL = unbound (ad-hoc template).
-- Additive + nullable → non-destructive.

ALTER TABLE "rpt_templates" ADD COLUMN "report_key" TEXT;

CREATE INDEX "rpt_templates_report_key_idx" ON "rpt_templates"("report_key");
