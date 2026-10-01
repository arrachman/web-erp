-- Migration: erp_partner_address_location_fk
-- Replace free-text country/province/city with FK references to md_countries/md_provinces/md_cities/md_areas

ALTER TABLE md_partner_addresses
  DROP COLUMN IF EXISTS country,
  DROP COLUMN IF EXISTS province,
  DROP COLUMN IF EXISTS city,
  ADD COLUMN country_id  BIGINT,
  ADD COLUMN province_id BIGINT,
  ADD COLUMN city_id     BIGINT,
  ADD COLUMN area_id     BIGINT;

ALTER TABLE md_partner_addresses
  ADD CONSTRAINT fk_partner_addr_country  FOREIGN KEY (country_id)  REFERENCES md_countries(id),
  ADD CONSTRAINT fk_partner_addr_province FOREIGN KEY (province_id) REFERENCES md_provinces(id),
  ADD CONSTRAINT fk_partner_addr_city     FOREIGN KEY (city_id)     REFERENCES md_cities(id),
  ADD CONSTRAINT fk_partner_addr_area     FOREIGN KEY (area_id)     REFERENCES md_areas(id);

CREATE INDEX idx_partner_addr_country  ON md_partner_addresses(country_id);
CREATE INDEX idx_partner_addr_province ON md_partner_addresses(province_id);
CREATE INDEX idx_partner_addr_city     ON md_partner_addresses(city_id);
CREATE INDEX idx_partner_addr_area     ON md_partner_addresses(area_id);
