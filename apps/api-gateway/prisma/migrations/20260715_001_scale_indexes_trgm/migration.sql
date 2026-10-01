-- Scale indexes: pg_trgm for ILIKE contains + partial indexes for active rows.
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Accounts (CoA)
CREATE INDEX IF NOT EXISTS md_accounts_name_trgm
  ON md_accounts USING gin (name gin_trgm_ops)
  WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS md_accounts_code_lower
  ON md_accounts (lower(code))
  WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS md_accounts_parent_active
  ON md_accounts (parent_id, code)
  WHERE deleted_at IS NULL;

-- Items
CREATE INDEX IF NOT EXISTS md_items_name_trgm
  ON md_items USING gin (name gin_trgm_ops)
  WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS md_items_code_lower
  ON md_items (lower(code))
  WHERE deleted_at IS NULL;

-- Partners
CREATE INDEX IF NOT EXISTS md_partners_name_trgm
  ON md_partners USING gin (name gin_trgm_ops)
  WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS md_partners_code_lower
  ON md_partners (lower(code))
  WHERE deleted_at IS NULL;

-- Journal entries
CREATE INDEX IF NOT EXISTS fin_journal_entries_doc_trgm
  ON fin_journal_entries USING gin (doc_number gin_trgm_ops)
  WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS fin_journal_entries_entry_date_active
  ON fin_journal_entries (entry_date DESC, id DESC)
  WHERE deleted_at IS NULL;

-- Ledger
CREATE INDEX IF NOT EXISTS fin_ledger_entries_doc_trgm
  ON fin_ledger_entries USING gin (doc_number gin_trgm_ops)
  WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS fin_ledger_entries_entry_date_active
  ON fin_ledger_entries (entry_date DESC, id DESC)
  WHERE deleted_at IS NULL;
