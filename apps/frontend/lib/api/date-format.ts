import { apiGet, apiPut } from './client';
import type { ApiResponse } from './types';

/** Global date display format (sys_settings `system/format/date_format`). */
export interface DateFormat {
  /** Moment-style token, e.g. "DD/MM/YYYY". */
  format: string;
  /** Rendered example for the sample date. */
  example: string;
}

export async function getDateFormat(): Promise<DateFormat> {
  const res = await apiGet<ApiResponse<DateFormat>>('/settings/date-format');
  return res.data;
}

export async function updateDateFormat(format: string): Promise<DateFormat> {
  const res = await apiPut<ApiResponse<DateFormat>>('/settings/date-format', { format });
  return res.data;
}
