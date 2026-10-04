-- MVP navigation scope (Admin): hide Admin menus outside the Bahtera Madani
-- MVP (Fase 1: A1, D1, D2, D3, E1) from the live ERP sidebar under
-- /app/admin/*. Hidden: duplicate menus (Document Numbering and Menu Manager
-- under System, User Log which duplicates Audit Log), cosmetic/format and
-- advanced menus (Form Builder, Grid Customization, Appearance, Number
-- Format, Date Format, Account Code Format, Language, Online Users, Home
-- Layout, Format Settings, Description Presets, Options, Report Defaults,
-- Preferences), and — by owner decision 2026-10-04, slimmer variant —
-- Report Designer, Signature Settings, Approval Settings, Doc Creation
-- Policy, Recalculate COGS, and Repost Journals.
-- Deactivated, not deleted, so they can be re-enabled when needed.
-- Data, endpoints, and role mappings are untouched; only `is_active` flips,
-- which the `getMyMenus` tree builder already filters out.
-- Kept for MVP: Company, Accounting, Tax and Bank Account settings,
-- Document Numbering, Import Data, Default Settings, Users, Roles,
-- Permissions, Menu Management, Close Fiscal Period, Data Validity Check,
-- Fiscal Periods, Audit Log, and Settings Manager.
-- See docs/bahtera-madani-mvp-scope.md § "Navigasi deployment MVP".

UPDATE "sys_menus"
SET "is_active" = false,
    "updated_at" = NOW()
WHERE "code" IN (
    'M0.SYS.DOCNUM',
    'M0.SYS.MENUS',
    'M0.ADM.USER-LOG',
    'M0.SYS.FORM',
    'M0.SYS.GRID',
    'M0.SYS.APPEARANCE',
    'M0.SYS.NUM-FORMAT',
    'M0.SYS.DATE-FORMAT',
    'M0.SYS.ACCT-CODE',
    'M0.ADM.LANG',
    'M0.ADM.ONLINE-USERS',
    'M0.CFG.HOME',
    'M0.CFG.FORMAT',
    'M0.CFG.DESCRIPTION',
    'M0.CFG.OPTIONS',
    'M0.CFG.REPORT-DEFAULT',
    'PREFERENCES',
    'M0.SYS.REPORT',
    'M0.CFG.SIGNATURE',
    'M0.CFG.APPROVAL',
    'M0.CFG.DOC-POLICY',
    'M0.ADM.RECALC-COGS',
    'M0.ADM.REPOST-JOURNAL'
  )
  AND "deleted_at" IS NULL
  AND "is_active" = true;
