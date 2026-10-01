
-- 1. MEMBERSIHKAN TABEL LAMA DAN BARU
DROP TABLE IF EXISTS public."m0_auditlog" CASCADE;
DROP TABLE IF EXISTS public."m0_session" CASCADE;
DROP TABLE IF EXISTS public."m2_outbound_detail_batch" CASCADE;
DROP TABLE IF EXISTS public."m2_outbound_detail" CASCADE;
DROP TABLE IF EXISTS public."m2_outbound" CASCADE;
DROP TABLE IF EXISTS public."m1_contact" CASCADE;
DROP TABLE IF EXISTS public."m1_division" CASCADE;
DROP TABLE IF EXISTS public."m1_uom" CASCADE;
DROP TABLE IF EXISTS public."m1_province" CASCADE;
DROP TABLE IF EXISTS public."m1_city" CASCADE;
DROP TABLE IF EXISTS public."m1_city_sla" CASCADE;
DROP TABLE IF EXISTS public."m1_warehouse" CASCADE;
DROP TABLE IF EXISTS public."m1_item" CASCADE;
DROP TABLE IF EXISTS public."m0_master_data_contact" CASCADE;
DROP TABLE IF EXISTS public."m0_role_menu" CASCADE;
DROP TABLE IF EXISTS public."m0_role_permission" CASCADE;
DROP TABLE IF EXISTS public."m0_user_department" CASCADE;
DROP TABLE IF EXISTS public."m0_user_role" CASCADE;
DROP TABLE IF EXISTS public."m0_menu" CASCADE;
DROP TABLE IF EXISTS public."m0_users" CASCADE;
DROP TABLE IF EXISTS public."m0_department" CASCADE;
DROP TABLE IF EXISTS public."m0_role" CASCADE;
DROP TABLE IF EXISTS public."m0_permission" CASCADE;

DROP TABLE IF EXISTS public."auditlog" CASCADE;
DROP TABLE IF EXISTS public."session" CASCADE;
DROP TABLE IF EXISTS public."do_detail" CASCADE;
DROP TABLE IF EXISTS public."do" CASCADE;
DROP TABLE IF EXISTS public."master_data_contact" CASCADE;
DROP TABLE IF EXISTS public."master_data_uom" CASCADE;
DROP TABLE IF EXISTS public."master_data_province" CASCADE;
DROP TABLE IF EXISTS public."master_data_warehouse" CASCADE;
DROP TABLE IF EXISTS public."master_data_city" CASCADE;
DROP TABLE IF EXISTS public."master_data_item" CASCADE;
DROP TABLE IF EXISTS public."role_menu" CASCADE;
DROP TABLE IF EXISTS public."role_permission" CASCADE;
DROP TABLE IF EXISTS public."user_department" CASCADE;
DROP TABLE IF EXISTS public."user_role" CASCADE;
DROP TABLE IF EXISTS public."menu" CASCADE;
DROP TABLE IF EXISTS public."users" CASCADE;
DROP TABLE IF EXISTS public."user" CASCADE;
DROP TABLE IF EXISTS public."department" CASCADE;
DROP TABLE IF EXISTS public."role" CASCADE;
DROP TABLE IF EXISTS public."permission" CASCADE;

-- 2. FUNGSI TRIGGER UNTUK UPDATE OTOMATIS updated_at
CREATE OR REPLACE FUNCTION update_timestamp_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- 3. TABEL m0_users
CREATE TABLE public."m0_users" (
    id SERIAL PRIMARY KEY,
    email text NOT NULL,
    username text NOT NULL,
    password_hash text NOT NULL,
    warehouse_id text,
    full_name text,
    avatar_url text,
    is_active boolean DEFAULT true NOT NULL,
    last_login timestamp(3) without time zone,
    created_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by text,
    updated_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_by text,
    deleted_at timestamp(3) without time zone,
    deleted_by text,
    uuid text NOT NULL,
    CONSTRAINT chk_m0_users_active_requires_warehouse CHECK (NOT is_active OR warehouse_id IS NOT NULL)
);
CREATE INDEX m0_users_warehouse_id_idx ON public."m0_users"(warehouse_id);
CREATE TRIGGER tr_m0_users_updated_at BEFORE UPDATE ON public."m0_users" FOR EACH ROW EXECUTE FUNCTION update_timestamp_column();

-- 4. TABEL m0_role
CREATE TABLE public."m0_role" (
    id SERIAL PRIMARY KEY,
    name text NOT NULL,
    description text,
    is_system boolean DEFAULT false NOT NULL,
    created_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by text,
    updated_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_by text,
    deleted_at timestamp(3) without time zone,
    deleted_by text,
    uuid text NOT NULL
);
CREATE TRIGGER tr_m0_role_updated_at BEFORE UPDATE ON public."m0_role" FOR EACH ROW EXECUTE FUNCTION update_timestamp_column();

-- 5. TABEL m0_permission
CREATE TABLE public."m0_permission" (
    id SERIAL PRIMARY KEY,
    name text NOT NULL,
    description text,
    module text NOT NULL,
    action text NOT NULL,
    created_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by text,
    updated_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_by text,
    deleted_at timestamp(3) without time zone,
    deleted_by text,
    uuid text NOT NULL
);
CREATE TRIGGER tr_m0_permission_updated_at BEFORE UPDATE ON public."m0_permission" FOR EACH ROW EXECUTE FUNCTION update_timestamp_column();

-- 6. TABEL m0_department
CREATE TABLE public."m0_department" (
    id SERIAL PRIMARY KEY,
    name text NOT NULL,
    code text NOT NULL,
    description text,
    parent_id text,
    manager_id text,
    created_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by text,
    updated_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_by text,
    deleted_at timestamp(3) without time zone,
    deleted_by text,
    uuid text NOT NULL
);
CREATE TRIGGER tr_m0_department_updated_at BEFORE UPDATE ON public."m0_department" FOR EACH ROW EXECUTE FUNCTION update_timestamp_column();

-- 7. TABEL m0_menu
CREATE TABLE public."m0_menu" (
    id SERIAL PRIMARY KEY,
    key text NOT NULL,
    title text NOT NULL,
    path text,
    icon text,
    type text DEFAULT 'ITEM' NOT NULL,
    parent_id text,
    sort_order integer DEFAULT 0 NOT NULL,
    is_visible boolean DEFAULT true NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    permission_name text,
    created_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by text,
    updated_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_by text,
    deleted_at timestamp(3) without time zone,
    deleted_by text,
    uuid text NOT NULL
);
CREATE TRIGGER tr_m0_menu_updated_at BEFORE UPDATE ON public."m0_menu" FOR EACH ROW EXECUTE FUNCTION update_timestamp_column();

-- 8. TABEL m0_role_permission
CREATE TABLE public."m0_role_permission" (
    id SERIAL PRIMARY KEY,
    role_id text NOT NULL,
    permission_id text NOT NULL,
    assigned_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by text,
    updated_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_by text,
    deleted_at timestamp(3) without time zone,
    deleted_by text,
    uuid text NOT NULL
);
CREATE TRIGGER tr_m0_role_permission_updated_at BEFORE UPDATE ON public."m0_role_permission" FOR EACH ROW EXECUTE FUNCTION update_timestamp_column();

-- 9. TABEL m0_role_menu
CREATE TABLE public."m0_role_menu" (
    id SERIAL PRIMARY KEY,
    role_id text NOT NULL,
    menu_id text NOT NULL,
    can_view boolean DEFAULT true NOT NULL,
    assigned_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by text,
    updated_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_by text,
    deleted_at timestamp(3) without time zone,
    deleted_by text,
    uuid text NOT NULL
);
CREATE TRIGGER tr_m0_role_menu_updated_at BEFORE UPDATE ON public."m0_role_menu" FOR EACH ROW EXECUTE FUNCTION update_timestamp_column();

-- 10. TABEL m0_user_department
CREATE TABLE public."m0_user_department" (
    id SERIAL PRIMARY KEY,
    user_id text NOT NULL,
    department_id text NOT NULL,
    joined_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by text,
    updated_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_by text,
    deleted_at timestamp(3) without time zone,
    deleted_by text,
    uuid text NOT NULL
);
CREATE TRIGGER tr_m0_user_department_updated_at BEFORE UPDATE ON public."m0_user_department" FOR EACH ROW EXECUTE FUNCTION update_timestamp_column();

-- 11. TABEL m0_user_role
CREATE TABLE public."m0_user_role" (
    id SERIAL PRIMARY KEY,
    user_id text NOT NULL,
    role_id text NOT NULL,
    assigned_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by text,
    updated_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_by text,
    deleted_at timestamp(3) without time zone,
    deleted_by text,
    uuid text NOT NULL
);
CREATE TRIGGER tr_m0_user_role_updated_at BEFORE UPDATE ON public."m0_user_role" FOR EACH ROW EXECUTE FUNCTION update_timestamp_column();

-- 12. TABEL m0_auditlog
CREATE TABLE public."m0_auditlog" (
    id SERIAL PRIMARY KEY,
    user_id text,
    action text NOT NULL,
    entity_type text NOT NULL,
    entity_id text,
    old_data jsonb,
    new_data jsonb,
    ip_address text,
    user_agent text,
    created_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by text,
    updated_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_by text,
    deleted_at timestamp(3) without time zone,
    deleted_by text,
    uuid text NOT NULL
);
CREATE TRIGGER tr_m0_auditlog_updated_at BEFORE UPDATE ON public."m0_auditlog" FOR EACH ROW EXECUTE FUNCTION update_timestamp_column();

-- 13. TABEL m0_session
CREATE TABLE public."m0_session" (
    id SERIAL PRIMARY KEY,
    user_id text NOT NULL,
    token text NOT NULL,
    expires_at timestamp(3) without time zone NOT NULL,
    ip_address text,
    user_agent text,
    created_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by text,
    updated_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_by text,
    deleted_at timestamp(3) without time zone,
    deleted_by text,
    uuid text NOT NULL
);
CREATE TRIGGER tr_m0_session_updated_at BEFORE UPDATE ON public."m0_session" FOR EACH ROW EXECUTE FUNCTION update_timestamp_column();

-- 14. TABEL m1_contact
CREATE TABLE public."m1_contact" (
    id SERIAL PRIMARY KEY,
    code text NOT NULL,
    name text NOT NULL,
    tax text,
    website text,
    address text,
    street text,
    city text,
    province text,
    zip_code text,
    type text NOT NULL,
    contact_first_name text,
    contact_email text,
    contact_phone text,
    created_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by text,
    updated_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_by text,
    deleted_at timestamp(3) without time zone,
    deleted_by text,
    uuid text NOT NULL,
    CONSTRAINT m1_contact_code_key UNIQUE (code),
    CONSTRAINT m1_contact_uuid_key UNIQUE (uuid)
);
CREATE TRIGGER tr_m1_contact_updated_at BEFORE UPDATE ON public."m1_contact" FOR EACH ROW EXECUTE FUNCTION update_timestamp_column();

-- 15. TABEL m1_uom
CREATE TABLE public."m1_uom" (
    id SERIAL PRIMARY KEY,
    code text NOT NULL,
    name text NOT NULL,
    type text NOT NULL,
    created_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by text,
    updated_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_by text,
    deleted_at timestamp(3) without time zone,
    deleted_by text,
    uuid text NOT NULL,
    CONSTRAINT m1_uom_code_key UNIQUE (code),
    CONSTRAINT m1_uom_uuid_key UNIQUE (uuid)
);
CREATE TRIGGER tr_m1_uom_updated_at BEFORE UPDATE ON public."m1_uom" FOR EACH ROW EXECUTE FUNCTION update_timestamp_column();

-- 16. TABEL m1_division
CREATE TABLE public."m1_division" (
    id SERIAL PRIMARY KEY,
    code text NOT NULL,
    name text NOT NULL,
    description text,
    is_active boolean DEFAULT true NOT NULL,
    created_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by text,
    updated_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_by text,
    deleted_at timestamp(3) without time zone,
    deleted_by text,
    uuid text NOT NULL,
    CONSTRAINT m1_division_code_key UNIQUE (code),
    CONSTRAINT m1_division_uuid_key UNIQUE (uuid)
);
CREATE TRIGGER tr_m1_division_updated_at BEFORE UPDATE ON public."m1_division" FOR EACH ROW EXECUTE FUNCTION update_timestamp_column();

-- 17. TABEL m1_province
CREATE TABLE public."m1_province" (
    id SERIAL PRIMARY KEY,
    name text NOT NULL,
    iso_code text NOT NULL,
    created_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by text,
    updated_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_by text,
    deleted_at timestamp(3) without time zone,
    deleted_by text,
    uuid text NOT NULL,
    CONSTRAINT m1_province_iso_code_key UNIQUE (iso_code),
    CONSTRAINT m1_province_uuid_key UNIQUE (uuid)
);
CREATE TRIGGER tr_m1_province_updated_at BEFORE UPDATE ON public."m1_province" FOR EACH ROW EXECUTE FUNCTION update_timestamp_column();

-- 18. TABEL m1_city
CREATE TABLE public."m1_city" (
    id SERIAL PRIMARY KEY,
    province_id text NOT NULL,
    name text NOT NULL,
    postal_code text NOT NULL,
    created_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by text,
    updated_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_by text,
    deleted_at timestamp(3) without time zone,
    deleted_by text,
    uuid text NOT NULL,
    CONSTRAINT m1_city_uuid_key UNIQUE (uuid),
    CONSTRAINT m1_city_province_id_fkey FOREIGN KEY (province_id) REFERENCES public."m1_province"(uuid) ON UPDATE CASCADE ON DELETE RESTRICT
);
CREATE TRIGGER tr_m1_city_updated_at BEFORE UPDATE ON public."m1_city" FOR EACH ROW EXECUTE FUNCTION update_timestamp_column();

-- 19. TABEL m1_city_sla
CREATE TABLE public."m1_city_sla" (
    id SERIAL PRIMARY KEY,
    city_id text NOT NULL,
    std_lead_time_days integer DEFAULT 0 NOT NULL,
    std_return_do_days integer DEFAULT 0 NOT NULL,
    created_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by text,
    updated_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_by text,
    deleted_at timestamp(3) without time zone,
    deleted_by text,
    uuid text NOT NULL,
    CONSTRAINT m1_city_sla_uuid_key UNIQUE (uuid),
    CONSTRAINT m1_city_sla_city_id_fkey FOREIGN KEY (city_id) REFERENCES public."m1_city"(uuid) ON UPDATE CASCADE ON DELETE RESTRICT
);
CREATE INDEX m1_city_sla_city_id_idx ON public."m1_city_sla"(city_id);
CREATE TRIGGER tr_m1_city_sla_updated_at BEFORE UPDATE ON public."m1_city_sla" FOR EACH ROW EXECUTE FUNCTION update_timestamp_column();

-- 20. TABEL m1_warehouse
CREATE TABLE public."m1_warehouse" (
    id SERIAL PRIMARY KEY,
    name text NOT NULL,
    city_id text NOT NULL,
    location_name text,
    address_detail text,
    created_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by text,
    updated_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_by text,
    deleted_at timestamp(3) without time zone,
    deleted_by text,
    uuid text NOT NULL,
    CONSTRAINT m1_warehouse_uuid_key UNIQUE (uuid),
    CONSTRAINT m1_warehouse_city_id_fkey FOREIGN KEY (city_id) REFERENCES public."m1_city"(uuid) ON UPDATE CASCADE ON DELETE RESTRICT
);
CREATE TRIGGER tr_m1_warehouse_updated_at BEFORE UPDATE ON public."m1_warehouse" FOR EACH ROW EXECUTE FUNCTION update_timestamp_column();

-- Relasi user -> warehouse
ALTER TABLE public."m0_users"
ADD CONSTRAINT m0_users_warehouse_id_fkey
FOREIGN KEY (warehouse_id) REFERENCES public."m1_warehouse"(uuid)
ON UPDATE CASCADE
ON DELETE SET NULL;

-- 21. TABEL m1_item
CREATE TABLE public."m1_item" (
    id SERIAL PRIMARY KEY,
    code text NOT NULL,
    name text NOT NULL,
    category text NOT NULL,
    uom_id text NOT NULL,
    item_type text NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    created_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by text,
    updated_at timestamp(3) without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_by text,
    deleted_at timestamp(3) without time zone,
    deleted_by text,
    uuid text NOT NULL,
    CONSTRAINT m1_item_code_key UNIQUE (code),
    CONSTRAINT m1_item_uuid_key UNIQUE (uuid),
    CONSTRAINT m1_item_uom_id_fkey FOREIGN KEY (uom_id) REFERENCES public."m1_uom"(uuid) ON UPDATE CASCADE ON DELETE RESTRICT
);
CREATE TRIGGER tr_m1_item_updated_at BEFORE UPDATE ON public."m1_item" FOR EACH ROW EXECUTE FUNCTION update_timestamp_column();
