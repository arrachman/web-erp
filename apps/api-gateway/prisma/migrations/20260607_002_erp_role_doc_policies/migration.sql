-- Role Document Creation Policies
-- Maps (role, document_type) → allowed initial statuses when creating a new document.
-- Multi-role user: union all matching rows.

CREATE TABLE "adm_role_doc_policies" (
    "id"               BIGSERIAL    PRIMARY KEY,
    "role_id"          BIGINT       NOT NULL,
    "document_type"    VARCHAR(60)  NOT NULL,
    "allowed_statuses" TEXT[]       NOT NULL DEFAULT '{}',
    "is_active"        BOOLEAN      NOT NULL DEFAULT TRUE,
    "created_at"       TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    "updated_at"       TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    "deleted_at"       TIMESTAMPTZ,
    "created_by_id"    BIGINT,
    "updated_by_id"    BIGINT,

    CONSTRAINT "adm_role_doc_policies_role_id_document_type_key"
        UNIQUE ("role_id", "document_type")
);

CREATE INDEX "adm_role_doc_policies_role_id_idx" ON "adm_role_doc_policies" ("role_id");
