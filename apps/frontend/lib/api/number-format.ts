import { apiGet, apiPut } from './client';
import type { ApiResponse } from './types';

export interface NumberFormat {
  thousandsSep: string;
  decimalSep: string;
  decimals: number;
  example: string;
}

export async function getNumberFormat(): Promise<NumberFormat> {
  const res = await apiGet<ApiResponse<NumberFormat>>('/settings/number-format');
  return res.data;
}

export async function updateNumberFormat(payload: {
  thousandsSep: string;
  decimalSep: string;
  decimals: number;
}): Promise<NumberFormat> {
  const res = await apiPut<ApiResponse<NumberFormat>>('/settings/number-format', payload);
  return res.data;
}
