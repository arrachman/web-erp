-- Report Engine: rpt_templates
-- Menyimpan template laporan custom (JSON: bands, components, data sources SQL).

CREATE TABLE "rpt_templates" (
    "id"           BIGSERIAL PRIMARY KEY,
    "code"         TEXT        NOT NULL,
    "name"         TEXT        NOT NULL,
    "module"       TEXT        NOT NULL,
    "description"  TEXT,
    "template_json" JSONB      NOT NULL DEFAULT '{}',
    "is_active"    BOOLEAN     NOT NULL DEFAULT TRUE,
    "created_at"   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "updated_at"   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at"   TIMESTAMPTZ,
    CONSTRAINT "rpt_templates_code_key" UNIQUE ("code")
);

CREATE INDEX "rpt_templates_module_idx"    ON "rpt_templates" ("module");
CREATE INDEX "rpt_templates_is_active_idx" ON "rpt_templates" ("is_active");
