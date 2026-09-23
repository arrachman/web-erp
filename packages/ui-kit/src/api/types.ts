// Base response and pagination types for every Senti product API.
// Backend contract (NestJS): list endpoints return { success, data, meta },
// single endpoints return { success, data }. BigInt IDs are serialised to
// strings — model entity ids as `string`, never `number`.

export interface ApiError {
  code: string;
  message: string;
  details?: unknown;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
}

export interface PaginatedMeta {
  page: number;
  limit: number;
  /**
   * Exact row count when totalExact !== false.
   * When COUNT(*) was skipped (includeTotal=false), this is a lower-bound
   * hint (page*limit or page*limit-1) — prefer `hasMore` for navigation.
   */
  total: number;
  totalPages: number;
  /** True when another page likely exists. */
  hasMore?: boolean;
  /** false when the server skipped COUNT(*) (total is approximate). */
  totalExact?: boolean;
}

export interface PaginatedResponse<T> {
  success: boolean;
  data: T[];
  meta: PaginatedMeta;
}

export interface PaginationParams {
  page?: number;
  limit?: number;
  search?: string;
  sortBy?: string;
  sortDir?: 'asc' | 'desc';
  /**
   * Optional active-state filter — supported by most master-data list
   * endpoints. Backends using `forbidNonWhitelisted: true` reject this for
   * resources without an `isActive` column, so strip it in the resource
   * wrapper before forwarding when the endpoint does not support it.
   */
  isActive?: boolean;
  /** When false, server may skip COUNT(*) and set hasMore/totalExact. */
  includeTotal?: boolean;
}
