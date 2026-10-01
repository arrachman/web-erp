import { listCountries } from '@/lib/api/countries';
import { listProvinces } from '@/lib/api/provinces';
import { listCities } from '@/lib/api/cities';
import { listAreas } from '@/lib/api/areas';
import { listSubAreas } from '@/lib/api/sub-areas';
import type { ErpAddressType, ErpPartnerAddress } from '@/lib/api/partners';

export const ADDRESS_TYPE_LABELS: Record<ErpAddressType, string> = {
  BILLING: 'Penagihan',
  SHIPPING: 'Pengiriman',
  OFFICE: 'Kantor',
  OTHER: 'Lainnya',
};

export async function loadCountryOptions(search: string, page: number, limit: number) {
  const res = await listCountries({ search: search || undefined, page, limit, isActive: true } as Parameters<typeof listCountries>[0]);
  return { data: res.data.map((c) => ({ value: c.id, label: c.name })), total: res.meta.total };
}

export function makeProvinceLoader(countryId: string) {
  return async (search: string, page: number, limit: number) => {
    const res = await listProvinces({ search: search || undefined, page, limit, isActive: true, countryId: countryId || undefined } as Parameters<typeof listProvinces>[0]);
    return { data: res.data.map((p) => ({ value: p.id, label: p.name })), total: res.meta.total };
  };
}

export function makeCityLoader(provinceId: string) {
  return async (search: string, page: number, limit: number) => {
    const res = await listCities({ search: search || undefined, page, limit, isActive: true, provinceId: provinceId || undefined } as Parameters<typeof listCities>[0]);
    return { data: res.data.map((c) => ({ value: c.id, label: c.name })), total: res.meta.total };
  };
}

export function makeAreaLoader(cityId: string) {
  return async (search: string, page: number, limit: number) => {
    const res = await listAreas({ search: search || undefined, page, limit, isActive: true, cityId: cityId || undefined } as Parameters<typeof listAreas>[0]);
    return { data: res.data.map((a) => ({ value: a.id, label: a.name, meta: a.postalCode ?? '' })), total: res.meta.total };
  };
}

export function makeSubAreaLoader(areaId: string) {
  return async (search: string, page: number, limit: number) => {
    const res = await listSubAreas({ search: search || undefined, page, limit, isActive: true, areaId: areaId || undefined } as Parameters<typeof listSubAreas>[0]);
    return { data: res.data.map((s) => ({ value: s.id, label: s.name, meta: s.postalCode ?? '' })), total: res.meta.total };
  };
}

export function addressLocationLabel(a: ErpPartnerAddress): string {
  const parts = [a.subArea?.name, a.area?.name, a.city?.name, a.province?.name, a.country?.name].filter(Boolean);
  return parts.join(', ') || '—';
}
