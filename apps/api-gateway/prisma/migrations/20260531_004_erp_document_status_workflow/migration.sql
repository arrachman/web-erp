-- Extend ErpDocumentStatus with the full Senti approval state machine (§2.7).
-- Additive only: existing DRAFT/POSTED/VOID/CANCELLED untouched; aligns the DB
-- enum with the frontend canonical set in apps/web-erp/lib/status.ts
-- (DRAFT → NEED_APPROVE → APPROVED/REJECTED → POSTED). No DROP, no data change.
ALTER TYPE "ErpDocumentStatus" ADD VALUE IF NOT EXISTS 'NEED_APPROVE' AFTER 'DRAFT';
ALTER TYPE "ErpDocumentStatus" ADD VALUE IF NOT EXISTS 'APPROVED' AFTER 'NEED_APPROVE';
ALTER TYPE "ErpDocumentStatus" ADD VALUE IF NOT EXISTS 'REJECTED' AFTER 'APPROVED';
