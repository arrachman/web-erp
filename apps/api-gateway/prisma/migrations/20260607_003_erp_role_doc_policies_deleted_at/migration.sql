-- Add deleted_at to adm_role_doc_policies (soft-delete support)
ALTER TABLE "adm_role_doc_policies" ADD COLUMN "deleted_at" TIMESTAMPTZ;
