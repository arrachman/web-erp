-- Fase 3 W7: harga kontrak per sekolah (berlapis) + bundling/paket.
-- Resolusi harga: harga item kontrak > diskon kategori kontrak > diskon
-- sekolah kontrak > harga jual standar. Yayasan TIDAK dibangun (keputusan
-- 2026-10-05: per-sekolah saja), jadi kontrak selalu per partner sekolah.
CREATE TABLE IF NOT EXISTS md_school_contract_prices (
  id BIGSERIAL PRIMARY KEY,
  partner_id BIGINT NOT NULL REFERENCES md_partners(id),
  item_id BIGINT REFERENCES md_items(id),
  category_id BIGINT REFERENCES md_item_categories(id),
  price NUMERIC(19,4),
  discount_percent NUMERIC(9,4),
  valid_from DATE,
  valid_to DATE,
  is_active BOOLEAN NOT NULL DEFAULT true,
  notes TEXT,
  metadata JSONB,
  legacy_code TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),
  created_by_id BIGINT,
  updated_by_id BIGINT,
  deleted_at TIMESTAMPTZ
);
CREATE INDEX IF NOT EXISTS md_school_contract_prices_partner_idx ON md_school_contract_prices (partner_id);
CREATE INDEX IF NOT EXISTS md_school_contract_prices_item_idx ON md_school_contract_prices (item_id);

-- Paket/bundle: satu item paket memiliki banyak baris komponen.
CREATE TABLE IF NOT EXISTS md_item_bundles (
  id BIGSERIAL PRIMARY KEY,
  item_id BIGINT NOT NULL UNIQUE REFERENCES md_items(id),
  name TEXT,
  notes TEXT,
  is_active BOOLEAN NOT NULL DEFAULT true,
  metadata JSONB,
  legacy_code TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),
  created_by_id BIGINT,
  updated_by_id BIGINT,
  deleted_at TIMESTAMPTZ
);
CREATE TABLE IF NOT EXISTS md_item_bundle_lines (
  id BIGSERIAL PRIMARY KEY,
  bundle_id BIGINT NOT NULL REFERENCES md_item_bundles(id),
  component_item_id BIGINT NOT NULL REFERENCES md_items(id),
  quantity NUMERIC(19,4) NOT NULL DEFAULT 1,
  line_no INT NOT NULL DEFAULT 1,
  notes TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),
  created_by_id BIGINT,
  updated_by_id BIGINT,
  deleted_at TIMESTAMPTZ
);
CREATE INDEX IF NOT EXISTS md_item_bundle_lines_bundle_idx ON md_item_bundle_lines (bundle_id);

-- Fase 3 W4: peran orang tua di portal (akun tertaut ke partner sekolah yang
-- sudah ada; data siswa di metadata akun).
ALTER TYPE "ErpPortalAccountRole" ADD VALUE IF NOT EXISTS 'ORANG_TUA';

-- Menu admin: Harga Kontrak & Paket (kloning baris Akun Portal Sekolah).
INSERT INTO sys_menus (code, title, path, icon, type, parent_id, sort_order, is_active, created_at, updated_at)
SELECT 'M1.CONTRACTS', 'Harga Kontrak & Paket', '/master/contract-prices', icon, type, parent_id, sort_order + 1, true, now(), now()
FROM sys_menus WHERE code = 'M1.PARTNER.PORTAL'
  AND NOT EXISTS (SELECT 1 FROM sys_menus WHERE code = 'M1.CONTRACTS');
INSERT INTO adm_role_menus (role_id, menu_id, can_view, can_create, can_edit, can_delete, can_approve, can_print, can_export, can_import, is_favorite)
SELECT rm.role_id, nm.id, rm.can_view, rm.can_create, rm.can_edit, rm.can_delete, rm.can_approve, rm.can_print, rm.can_export, rm.can_import, rm.is_favorite
FROM adm_role_menus rm
JOIN sys_menus om ON om.id = rm.menu_id AND om.code = 'M1.PARTNER.PORTAL'
JOIN sys_menus nm ON nm.code = 'M1.CONTRACTS'
WHERE NOT EXISTS (SELECT 1 FROM adm_role_menus x WHERE x.role_id = rm.role_id AND x.menu_id = nm.id);
