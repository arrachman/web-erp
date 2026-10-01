-- CreateEnum
CREATE TYPE "ErpUserLevel" AS ENUM ('POS', 'CENTRAL', 'POS_AND_CENTRAL', 'BI', 'BI_AND_CENTRAL');

-- CreateEnum
CREATE TYPE "ErpMenuType" AS ENUM ('MODULE', 'GROUP', 'ITEM');

-- CreateEnum
CREATE TYPE "ErpFiscalPeriodStatus" AS ENUM ('OPEN', 'SOFT_CLOSED', 'CLOSED', 'REOPENED');

-- CreateEnum
CREATE TYPE "ErpNumberingReset" AS ENUM ('NEVER', 'YEARLY', 'MONTHLY');

-- CreateEnum
CREATE TYPE "ErpItemType" AS ENUM ('INVENTORY', 'SERVICE', 'VOUCHER', 'ASSEMBLY');

-- CreateEnum
CREATE TYPE "ErpAccountType" AS ENUM ('ASSET', 'LIABILITY', 'EQUITY', 'REVENUE', 'EXPENSE');

-- CreateEnum
CREATE TYPE "ErpAccountKind" AS ENUM ('HEADER', 'POSTABLE');

-- CreateEnum
CREATE TYPE "ErpNormalBalance" AS ENUM ('DEBIT', 'CREDIT');

-- CreateEnum
CREATE TYPE "ErpCashFlowCategory" AS ENUM ('OPERATING', 'INVESTING', 'FINANCING');

-- CreateEnum
CREATE TYPE "ErpAddressType" AS ENUM ('BILLING', 'SHIPPING', 'OFFICE', 'OTHER');

-- CreateEnum
CREATE TYPE "ErpPartnerCategoryKind" AS ENUM ('CUSTOMER', 'SUPPLIER', 'SALESMAN', 'GENERAL');

-- CreateEnum
CREATE TYPE "ErpAuditAction" AS ENUM ('CREATE', 'UPDATE', 'DELETE', 'RESTORE', 'LOGIN', 'LOGOUT');

-- CreateEnum
CREATE TYPE "ErpNotificationChannel" AS ENUM ('EMAIL', 'WHATSAPP', 'IN_APP', 'SMS');

-- CreateTable
CREATE TABLE "sys_menus" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "title" TEXT NOT NULL,
    "path" TEXT,
    "icon" TEXT,
    "type" "ErpMenuType" NOT NULL,
    "parent_id" BIGINT,
    "sort_order" INTEGER NOT NULL,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "sys_menus_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sys_settings" (
    "id" BIGSERIAL NOT NULL,
    "module" TEXT,
    "group" TEXT NOT NULL,
    "key" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "value" TEXT,
    "data_type" TEXT NOT NULL,
    "sort_order" INTEGER NOT NULL DEFAULT 0,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "sys_settings_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sys_document_numberings" (
    "id" BIGSERIAL NOT NULL,
    "document_code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "prefix" TEXT NOT NULL,
    "digit_count" INTEGER NOT NULL,
    "reset_policy" "ErpNumberingReset" NOT NULL,
    "next_number" INTEGER NOT NULL DEFAULT 1,
    "menu_id" BIGINT,
    "affects_ledger" BOOLEAN NOT NULL DEFAULT false,
    "affects_inventory" BOOLEAN NOT NULL DEFAULT false,
    "affects_cost" BOOLEAN NOT NULL DEFAULT false,
    "notes" TEXT,
    "legacy_code" TEXT,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "sys_document_numberings_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sys_fiscal_periods" (
    "id" BIGSERIAL NOT NULL,
    "year" INTEGER NOT NULL,
    "period_no" INTEGER NOT NULL,
    "name" TEXT NOT NULL,
    "start_date" DATE NOT NULL,
    "end_date" DATE NOT NULL,
    "status" "ErpFiscalPeriodStatus" NOT NULL DEFAULT 'OPEN',
    "closed_at" TIMESTAMPTZ(6),
    "closed_by_id" BIGINT,
    "soft_closed_at" TIMESTAMPTZ(6),
    "reopened_at" TIMESTAMPTZ(6),
    "reopened_by_id" BIGINT,
    "reopen_reason" TEXT,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "sys_fiscal_periods_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sys_audit_logs" (
    "id" BIGSERIAL NOT NULL,
    "action" "ErpAuditAction" NOT NULL,
    "entity_name" TEXT NOT NULL,
    "entity_id" BIGINT,
    "summary" TEXT,
    "changes" JSONB,
    "actor_id" BIGINT,
    "actor_ip" TEXT,
    "occurred_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT "sys_audit_logs_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sys_notifications" (
    "id" BIGSERIAL NOT NULL,
    "recipient_id" BIGINT NOT NULL,
    "type" TEXT NOT NULL,
    "title" TEXT NOT NULL,
    "body" TEXT NOT NULL,
    "reference_entity" TEXT,
    "reference_id" BIGINT,
    "action_url" TEXT,
    "read_at" TIMESTAMPTZ(6),
    "archived_at" TIMESTAMPTZ(6),
    "expires_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT "sys_notifications_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sys_languages" (
    "id" BIGSERIAL NOT NULL,
    "code" VARCHAR(10) NOT NULL,
    "name" TEXT NOT NULL,
    "native_name" TEXT NOT NULL,
    "is_rtl" BOOLEAN NOT NULL DEFAULT false,
    "is_default" BOOLEAN NOT NULL DEFAULT false,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "sys_languages_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sys_email_templates" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "description" TEXT,
    "channel" "ErpNotificationChannel" NOT NULL,
    "language_code" VARCHAR(10),
    "subject" TEXT,
    "body" TEXT NOT NULL,
    "available_placeholders" JSONB,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "sys_email_templates_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "adm_users" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "email" TEXT,
    "password_hash" TEXT NOT NULL,
    "level" "ErpUserLevel" NOT NULL,
    "language" VARCHAR(2) NOT NULL DEFAULT 'id',
    "default_menu_id" BIGINT,
    "home_branch_id" BIGINT,
    "home_warehouse_id" BIGINT,
    "salesman_partner_id" BIGINT,
    "expires_at" TIMESTAMPTZ(6),
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "adm_users_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "adm_roles" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "description" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "adm_roles_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "adm_permissions" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "group" TEXT,
    "description" TEXT,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "adm_permissions_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "adm_user_roles" (
    "user_id" BIGINT NOT NULL,
    "role_id" BIGINT NOT NULL,

    CONSTRAINT "adm_user_roles_pkey" PRIMARY KEY ("user_id","role_id")
);

-- CreateTable
CREATE TABLE "adm_role_permissions" (
    "role_id" BIGINT NOT NULL,
    "permission_id" BIGINT NOT NULL,

    CONSTRAINT "adm_role_permissions_pkey" PRIMARY KEY ("role_id","permission_id")
);

-- CreateTable
CREATE TABLE "adm_role_menus" (
    "role_id" BIGINT NOT NULL,
    "menu_id" BIGINT NOT NULL,
    "can_view" BOOLEAN NOT NULL DEFAULT false,
    "can_create" BOOLEAN NOT NULL DEFAULT false,
    "can_edit" BOOLEAN NOT NULL DEFAULT false,
    "can_delete" BOOLEAN NOT NULL DEFAULT false,
    "can_approve" BOOLEAN NOT NULL DEFAULT false,
    "can_print" BOOLEAN NOT NULL DEFAULT false,
    "can_export" BOOLEAN NOT NULL DEFAULT false,
    "can_import" BOOLEAN NOT NULL DEFAULT false,
    "is_favorite" BOOLEAN NOT NULL DEFAULT false,

    CONSTRAINT "adm_role_menus_pkey" PRIMARY KEY ("role_id","menu_id")
);

-- CreateTable
CREATE TABLE "adm_user_branch_access" (
    "user_id" BIGINT NOT NULL,
    "branch_id" BIGINT NOT NULL,

    CONSTRAINT "adm_user_branch_access_pkey" PRIMARY KEY ("user_id","branch_id")
);

-- CreateTable
CREATE TABLE "adm_user_location_access" (
    "user_id" BIGINT NOT NULL,
    "location_id" BIGINT NOT NULL,

    CONSTRAINT "adm_user_location_access_pkey" PRIMARY KEY ("user_id","location_id")
);

-- CreateTable
CREATE TABLE "adm_user_warehouse_access" (
    "user_id" BIGINT NOT NULL,
    "warehouse_id" BIGINT NOT NULL,

    CONSTRAINT "adm_user_warehouse_access_pkey" PRIMARY KEY ("user_id","warehouse_id")
);

-- CreateTable
CREATE TABLE "adm_user_sessions" (
    "id" BIGSERIAL NOT NULL,
    "user_id" BIGINT NOT NULL,
    "refresh_token_hash" TEXT NOT NULL,
    "device_name" TEXT,
    "ip_address" TEXT,
    "user_agent" TEXT,
    "last_active_at" TIMESTAMPTZ(6) NOT NULL,
    "expires_at" TIMESTAMPTZ(6) NOT NULL,
    "revoked_at" TIMESTAMPTZ(6),
    "revoked_reason" TEXT,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT "adm_user_sessions_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "adm_password_policies" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "min_length" INTEGER NOT NULL DEFAULT 8,
    "require_uppercase" BOOLEAN NOT NULL DEFAULT true,
    "require_lowercase" BOOLEAN NOT NULL DEFAULT true,
    "require_number" BOOLEAN NOT NULL DEFAULT true,
    "require_symbol" BOOLEAN NOT NULL DEFAULT false,
    "max_age_days" INTEGER,
    "history_count" INTEGER NOT NULL DEFAULT 3,
    "max_failed_attempts" INTEGER NOT NULL DEFAULT 5,
    "lockout_duration_min" INTEGER NOT NULL DEFAULT 30,
    "max_concurrent_sessions" INTEGER,
    "session_timeout_min" INTEGER NOT NULL DEFAULT 480,
    "is_default" BOOLEAN NOT NULL DEFAULT false,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "adm_password_policies_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "adm_user_preferences" (
    "user_id" BIGINT NOT NULL,
    "theme" TEXT,
    "language" VARCHAR(10),
    "timezone" TEXT,
    "date_format" TEXT,
    "number_format" TEXT,
    "table_page_size" INTEGER,
    "sidebar_collapsed" BOOLEAN NOT NULL DEFAULT false,
    "default_branch_id" BIGINT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "adm_user_preferences_pkey" PRIMARY KEY ("user_id")
);

-- CreateTable
CREATE TABLE "md_branches" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "address_line1" TEXT,
    "address_line2" TEXT,
    "city" TEXT,
    "postal_code" TEXT,
    "phone" TEXT,
    "fax" TEXT,
    "notes" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_branches_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_locations" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "branch_id" BIGINT NOT NULL,
    "address_line1" TEXT,
    "city" TEXT,
    "postal_code" TEXT,
    "phone" TEXT,
    "notes" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_locations_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_warehouses" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "location_id" BIGINT NOT NULL,
    "allow_negative_stock" BOOLEAN NOT NULL DEFAULT false,
    "notes" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_warehouses_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_units" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "conversion_factor" DECIMAL(19,4) NOT NULL DEFAULT 1,
    "notes" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_units_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_item_categories" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "parent_id" BIGINT,
    "inventory_account_id" BIGINT,
    "cogs_account_id" BIGINT,
    "sales_account_id" BIGINT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_item_categories_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_items" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "barcode" TEXT,
    "type" "ErpItemType" NOT NULL,
    "category_id" BIGINT NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "standard_cost" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "average_cost" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "purchase_price" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "sale_price" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "min_stock" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "max_stock" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "reorder_qty" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "tracks_serial" BOOLEAN NOT NULL DEFAULT false,
    "tracks_batch" BOOLEAN NOT NULL DEFAULT false,
    "tracks_bin" BOOLEAN NOT NULL DEFAULT false,
    "inventory_account_id" BIGINT,
    "sales_account_id" BIGINT,
    "cogs_account_id" BIGINT,
    "purchase_tax_id" BIGINT,
    "sale_tax_id" BIGINT,
    "primary_supplier_id" BIGINT,
    "weight" DECIMAL(19,4),
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_items_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_partner_categories" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "kind" "ErpPartnerCategoryKind" NOT NULL,
    "sales_tier" INTEGER,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_partner_categories_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_partners" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "is_customer" BOOLEAN NOT NULL DEFAULT false,
    "is_supplier" BOOLEAN NOT NULL DEFAULT false,
    "is_salesman" BOOLEAN NOT NULL DEFAULT false,
    "category_id" BIGINT,
    "tax_number" TEXT,
    "is_taxable" BOOLEAN NOT NULL DEFAULT false,
    "currency_id" BIGINT,
    "receivable_account_id" BIGINT,
    "payable_account_id" BIGINT,
    "ar_credit_limit" DECIMAL(19,4),
    "ap_credit_limit" DECIMAL(19,4),
    "sale_term_id" BIGINT,
    "purchase_term_id" BIGINT,
    "salesman_id" BIGINT,
    "commission_rate" DECIMAL(9,4),
    "branch_id" BIGINT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_partners_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_partner_addresses" (
    "id" BIGSERIAL NOT NULL,
    "partner_id" BIGINT NOT NULL,
    "type" "ErpAddressType" NOT NULL,
    "is_default" BOOLEAN NOT NULL DEFAULT false,
    "address_line1" TEXT NOT NULL,
    "address_line2" TEXT,
    "city" TEXT,
    "province" TEXT,
    "country" TEXT,
    "postal_code" TEXT,
    "phone" TEXT,
    "fax" TEXT,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_partner_addresses_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_partner_contacts" (
    "id" BIGSERIAL NOT NULL,
    "partner_id" BIGINT NOT NULL,
    "name" TEXT NOT NULL,
    "title" TEXT,
    "phone" TEXT,
    "email" TEXT,
    "is_default" BOOLEAN NOT NULL DEFAULT false,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_partner_contacts_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_partner_bank_accounts" (
    "id" BIGSERIAL NOT NULL,
    "partner_id" BIGINT NOT NULL,
    "bank_name" TEXT NOT NULL,
    "account_number" TEXT NOT NULL,
    "account_holder" TEXT,
    "is_default" BOOLEAN NOT NULL DEFAULT false,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_partner_bank_accounts_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_currencies" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "symbol" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_currencies_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_currency_rates" (
    "id" BIGSERIAL NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "rate_date" DATE NOT NULL,
    "rate" DECIMAL(19,6) NOT NULL,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "md_currency_rates_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_accounts" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "alias" TEXT,
    "type" "ErpAccountType" NOT NULL,
    "kind" "ErpAccountKind" NOT NULL,
    "normal_balance" "ErpNormalBalance" NOT NULL,
    "parent_id" BIGINT,
    "level" INTEGER NOT NULL,
    "cash_flow_category" "ErpCashFlowCategory",
    "currency_id" BIGINT,
    "is_control_account" BOOLEAN NOT NULL DEFAULT false,
    "bank_name" TEXT,
    "bank_account_no" TEXT,
    "opening_balance" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "notes" TEXT,
    "legacy_code" TEXT,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_accounts_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_taxes" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "rate" DECIMAL(9,4) NOT NULL,
    "sale_account_id" BIGINT,
    "purchase_account_id" BIGINT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_taxes_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_payment_terms" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "net_days" INTEGER NOT NULL,
    "discount_percent1" DECIMAL(9,4),
    "discount_days1" INTEGER,
    "discount_percent2" DECIMAL(9,4),
    "discount_days2" INTEGER,
    "penalty_percent" DECIMAL(9,4),
    "penalty_period" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_payment_terms_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE UNIQUE INDEX "sys_menus_code_key" ON "sys_menus"("code");

-- CreateIndex
CREATE INDEX "sys_menus_parent_id_idx" ON "sys_menus"("parent_id");

-- CreateIndex
CREATE INDEX "sys_menus_legacy_code_idx" ON "sys_menus"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "sys_settings_module_group_key_key" ON "sys_settings"("module", "group", "key");

-- CreateIndex
CREATE UNIQUE INDEX "sys_document_numberings_document_code_key" ON "sys_document_numberings"("document_code");

-- CreateIndex
CREATE INDEX "sys_document_numberings_menu_id_idx" ON "sys_document_numberings"("menu_id");

-- CreateIndex
CREATE INDEX "sys_document_numberings_legacy_code_idx" ON "sys_document_numberings"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "sys_fiscal_periods_year_period_no_key" ON "sys_fiscal_periods"("year", "period_no");

-- CreateIndex
CREATE INDEX "sys_audit_logs_entity_name_entity_id_idx" ON "sys_audit_logs"("entity_name", "entity_id");

-- CreateIndex
CREATE INDEX "sys_audit_logs_actor_id_idx" ON "sys_audit_logs"("actor_id");

-- CreateIndex
CREATE INDEX "sys_audit_logs_occurred_at_idx" ON "sys_audit_logs"("occurred_at");

-- CreateIndex
CREATE INDEX "sys_notifications_recipient_id_read_at_idx" ON "sys_notifications"("recipient_id", "read_at");

-- CreateIndex
CREATE INDEX "sys_notifications_created_at_idx" ON "sys_notifications"("created_at");

-- CreateIndex
CREATE INDEX "sys_notifications_expires_at_idx" ON "sys_notifications"("expires_at");

-- CreateIndex
CREATE UNIQUE INDEX "sys_languages_code_key" ON "sys_languages"("code");

-- CreateIndex
CREATE UNIQUE INDEX "sys_email_templates_code_channel_language_code_key" ON "sys_email_templates"("code", "channel", "language_code");

-- CreateIndex
CREATE UNIQUE INDEX "adm_users_code_key" ON "adm_users"("code");

-- CreateIndex
CREATE UNIQUE INDEX "adm_users_email_key" ON "adm_users"("email");

-- CreateIndex
CREATE INDEX "adm_users_home_branch_id_idx" ON "adm_users"("home_branch_id");

-- CreateIndex
CREATE INDEX "adm_users_legacy_code_idx" ON "adm_users"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "adm_roles_code_key" ON "adm_roles"("code");

-- CreateIndex
CREATE UNIQUE INDEX "adm_permissions_code_key" ON "adm_permissions"("code");

-- CreateIndex
CREATE INDEX "adm_user_sessions_user_id_idx" ON "adm_user_sessions"("user_id");

-- CreateIndex
CREATE INDEX "adm_user_sessions_refresh_token_hash_idx" ON "adm_user_sessions"("refresh_token_hash");

-- CreateIndex
CREATE UNIQUE INDEX "adm_password_policies_code_key" ON "adm_password_policies"("code");

-- CreateIndex
CREATE UNIQUE INDEX "md_branches_code_key" ON "md_branches"("code");

-- CreateIndex
CREATE INDEX "md_branches_legacy_code_idx" ON "md_branches"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_locations_code_key" ON "md_locations"("code");

-- CreateIndex
CREATE INDEX "md_locations_branch_id_idx" ON "md_locations"("branch_id");

-- CreateIndex
CREATE INDEX "md_locations_legacy_code_idx" ON "md_locations"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_warehouses_code_key" ON "md_warehouses"("code");

-- CreateIndex
CREATE INDEX "md_warehouses_location_id_idx" ON "md_warehouses"("location_id");

-- CreateIndex
CREATE INDEX "md_warehouses_legacy_code_idx" ON "md_warehouses"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_units_code_key" ON "md_units"("code");

-- CreateIndex
CREATE INDEX "md_units_legacy_code_idx" ON "md_units"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_item_categories_code_key" ON "md_item_categories"("code");

-- CreateIndex
CREATE INDEX "md_item_categories_parent_id_idx" ON "md_item_categories"("parent_id");

-- CreateIndex
CREATE INDEX "md_item_categories_legacy_code_idx" ON "md_item_categories"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_items_code_key" ON "md_items"("code");

-- CreateIndex
CREATE INDEX "md_items_category_id_idx" ON "md_items"("category_id");

-- CreateIndex
CREATE INDEX "md_items_base_unit_id_idx" ON "md_items"("base_unit_id");

-- CreateIndex
CREATE INDEX "md_items_barcode_idx" ON "md_items"("barcode");

-- CreateIndex
CREATE INDEX "md_items_legacy_code_idx" ON "md_items"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_partner_categories_code_key" ON "md_partner_categories"("code");

-- CreateIndex
CREATE INDEX "md_partner_categories_legacy_code_idx" ON "md_partner_categories"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_partners_code_key" ON "md_partners"("code");

-- CreateIndex
CREATE INDEX "md_partners_category_id_idx" ON "md_partners"("category_id");

-- CreateIndex
CREATE INDEX "md_partners_currency_id_idx" ON "md_partners"("currency_id");

-- CreateIndex
CREATE INDEX "md_partners_salesman_id_idx" ON "md_partners"("salesman_id");

-- CreateIndex
CREATE INDEX "md_partners_legacy_code_idx" ON "md_partners"("legacy_code");

-- CreateIndex
CREATE INDEX "md_partner_addresses_partner_id_idx" ON "md_partner_addresses"("partner_id");

-- CreateIndex
CREATE INDEX "md_partner_contacts_partner_id_idx" ON "md_partner_contacts"("partner_id");

-- CreateIndex
CREATE INDEX "md_partner_bank_accounts_partner_id_idx" ON "md_partner_bank_accounts"("partner_id");

-- CreateIndex
CREATE UNIQUE INDEX "md_currencies_code_key" ON "md_currencies"("code");

-- CreateIndex
CREATE INDEX "md_currencies_legacy_code_idx" ON "md_currencies"("legacy_code");

-- CreateIndex
CREATE INDEX "md_currency_rates_currency_id_rate_date_idx" ON "md_currency_rates"("currency_id", "rate_date");

-- CreateIndex
CREATE UNIQUE INDEX "md_currency_rates_currency_id_rate_date_key" ON "md_currency_rates"("currency_id", "rate_date");

-- CreateIndex
CREATE UNIQUE INDEX "md_accounts_code_key" ON "md_accounts"("code");

-- CreateIndex
CREATE INDEX "md_accounts_parent_id_idx" ON "md_accounts"("parent_id");

-- CreateIndex
CREATE INDEX "md_accounts_currency_id_idx" ON "md_accounts"("currency_id");

-- CreateIndex
CREATE INDEX "md_accounts_legacy_code_idx" ON "md_accounts"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_taxes_code_key" ON "md_taxes"("code");

-- CreateIndex
CREATE INDEX "md_taxes_legacy_code_idx" ON "md_taxes"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_payment_terms_code_key" ON "md_payment_terms"("code");

-- CreateIndex
CREATE INDEX "md_payment_terms_legacy_code_idx" ON "md_payment_terms"("legacy_code");

ALTER TABLE "sys_menus" ADD CONSTRAINT "sys_menus_parent_id_fkey" FOREIGN KEY ("parent_id") REFERENCES "sys_menus"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sys_document_numberings" ADD CONSTRAINT "sys_document_numberings_menu_id_fkey" FOREIGN KEY ("menu_id") REFERENCES "sys_menus"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sys_audit_logs" ADD CONSTRAINT "sys_audit_logs_actor_id_fkey" FOREIGN KEY ("actor_id") REFERENCES "adm_users"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sys_notifications" ADD CONSTRAINT "sys_notifications_recipient_id_fkey" FOREIGN KEY ("recipient_id") REFERENCES "adm_users"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sys_email_templates" ADD CONSTRAINT "sys_email_templates_language_code_fkey" FOREIGN KEY ("language_code") REFERENCES "sys_languages"("code") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "adm_user_roles" ADD CONSTRAINT "adm_user_roles_user_id_fkey" FOREIGN KEY ("user_id") REFERENCES "adm_users"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "adm_user_roles" ADD CONSTRAINT "adm_user_roles_role_id_fkey" FOREIGN KEY ("role_id") REFERENCES "adm_roles"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "adm_role_permissions" ADD CONSTRAINT "adm_role_permissions_role_id_fkey" FOREIGN KEY ("role_id") REFERENCES "adm_roles"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "adm_role_permissions" ADD CONSTRAINT "adm_role_permissions_permission_id_fkey" FOREIGN KEY ("permission_id") REFERENCES "adm_permissions"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "adm_role_menus" ADD CONSTRAINT "adm_role_menus_role_id_fkey" FOREIGN KEY ("role_id") REFERENCES "adm_roles"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "adm_role_menus" ADD CONSTRAINT "adm_role_menus_menu_id_fkey" FOREIGN KEY ("menu_id") REFERENCES "sys_menus"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "adm_user_branch_access" ADD CONSTRAINT "adm_user_branch_access_user_id_fkey" FOREIGN KEY ("user_id") REFERENCES "adm_users"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "adm_user_branch_access" ADD CONSTRAINT "adm_user_branch_access_branch_id_fkey" FOREIGN KEY ("branch_id") REFERENCES "md_branches"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "adm_user_location_access" ADD CONSTRAINT "adm_user_location_access_user_id_fkey" FOREIGN KEY ("user_id") REFERENCES "adm_users"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "adm_user_location_access" ADD CONSTRAINT "adm_user_location_access_location_id_fkey" FOREIGN KEY ("location_id") REFERENCES "md_locations"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "adm_user_warehouse_access" ADD CONSTRAINT "adm_user_warehouse_access_user_id_fkey" FOREIGN KEY ("user_id") REFERENCES "adm_users"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "adm_user_warehouse_access" ADD CONSTRAINT "adm_user_warehouse_access_warehouse_id_fkey" FOREIGN KEY ("warehouse_id") REFERENCES "md_warehouses"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "adm_user_sessions" ADD CONSTRAINT "adm_user_sessions_user_id_fkey" FOREIGN KEY ("user_id") REFERENCES "adm_users"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "adm_user_preferences" ADD CONSTRAINT "adm_user_preferences_user_id_fkey" FOREIGN KEY ("user_id") REFERENCES "adm_users"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_locations" ADD CONSTRAINT "md_locations_branch_id_fkey" FOREIGN KEY ("branch_id") REFERENCES "md_branches"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_warehouses" ADD CONSTRAINT "md_warehouses_location_id_fkey" FOREIGN KEY ("location_id") REFERENCES "md_locations"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_item_categories" ADD CONSTRAINT "md_item_categories_parent_id_fkey" FOREIGN KEY ("parent_id") REFERENCES "md_item_categories"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_item_categories" ADD CONSTRAINT "md_item_categories_inventory_account_id_fkey" FOREIGN KEY ("inventory_account_id") REFERENCES "md_accounts"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_item_categories" ADD CONSTRAINT "md_item_categories_cogs_account_id_fkey" FOREIGN KEY ("cogs_account_id") REFERENCES "md_accounts"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_item_categories" ADD CONSTRAINT "md_item_categories_sales_account_id_fkey" FOREIGN KEY ("sales_account_id") REFERENCES "md_accounts"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_items" ADD CONSTRAINT "md_items_category_id_fkey" FOREIGN KEY ("category_id") REFERENCES "md_item_categories"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_items" ADD CONSTRAINT "md_items_base_unit_id_fkey" FOREIGN KEY ("base_unit_id") REFERENCES "md_units"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_items" ADD CONSTRAINT "md_items_inventory_account_id_fkey" FOREIGN KEY ("inventory_account_id") REFERENCES "md_accounts"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_items" ADD CONSTRAINT "md_items_sales_account_id_fkey" FOREIGN KEY ("sales_account_id") REFERENCES "md_accounts"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_items" ADD CONSTRAINT "md_items_cogs_account_id_fkey" FOREIGN KEY ("cogs_account_id") REFERENCES "md_accounts"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_items" ADD CONSTRAINT "md_items_purchase_tax_id_fkey" FOREIGN KEY ("purchase_tax_id") REFERENCES "md_taxes"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_items" ADD CONSTRAINT "md_items_sale_tax_id_fkey" FOREIGN KEY ("sale_tax_id") REFERENCES "md_taxes"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_items" ADD CONSTRAINT "md_items_primary_supplier_id_fkey" FOREIGN KEY ("primary_supplier_id") REFERENCES "md_partners"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_partners" ADD CONSTRAINT "md_partners_category_id_fkey" FOREIGN KEY ("category_id") REFERENCES "md_partner_categories"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_partners" ADD CONSTRAINT "md_partners_currency_id_fkey" FOREIGN KEY ("currency_id") REFERENCES "md_currencies"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_partners" ADD CONSTRAINT "md_partners_receivable_account_id_fkey" FOREIGN KEY ("receivable_account_id") REFERENCES "md_accounts"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_partners" ADD CONSTRAINT "md_partners_payable_account_id_fkey" FOREIGN KEY ("payable_account_id") REFERENCES "md_accounts"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_partners" ADD CONSTRAINT "md_partners_sale_term_id_fkey" FOREIGN KEY ("sale_term_id") REFERENCES "md_payment_terms"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_partners" ADD CONSTRAINT "md_partners_purchase_term_id_fkey" FOREIGN KEY ("purchase_term_id") REFERENCES "md_payment_terms"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_partners" ADD CONSTRAINT "md_partners_salesman_id_fkey" FOREIGN KEY ("salesman_id") REFERENCES "md_partners"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_partners" ADD CONSTRAINT "md_partners_branch_id_fkey" FOREIGN KEY ("branch_id") REFERENCES "md_branches"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_partner_addresses" ADD CONSTRAINT "md_partner_addresses_partner_id_fkey" FOREIGN KEY ("partner_id") REFERENCES "md_partners"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_partner_contacts" ADD CONSTRAINT "md_partner_contacts_partner_id_fkey" FOREIGN KEY ("partner_id") REFERENCES "md_partners"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_partner_bank_accounts" ADD CONSTRAINT "md_partner_bank_accounts_partner_id_fkey" FOREIGN KEY ("partner_id") REFERENCES "md_partners"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_currency_rates" ADD CONSTRAINT "md_currency_rates_currency_id_fkey" FOREIGN KEY ("currency_id") REFERENCES "md_currencies"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_accounts" ADD CONSTRAINT "md_accounts_parent_id_fkey" FOREIGN KEY ("parent_id") REFERENCES "md_accounts"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_accounts" ADD CONSTRAINT "md_accounts_currency_id_fkey" FOREIGN KEY ("currency_id") REFERENCES "md_currencies"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_taxes" ADD CONSTRAINT "md_taxes_sale_account_id_fkey" FOREIGN KEY ("sale_account_id") REFERENCES "md_accounts"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_taxes" ADD CONSTRAINT "md_taxes_purchase_account_id_fkey" FOREIGN KEY ("purchase_account_id") REFERENCES "md_accounts"("id") ON DELETE SET NULL ON UPDATE CASCADE;

