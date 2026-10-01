// Senti shared API layer — public surface.
export type {
  ApiError,
  ApiResponse,
  PaginatedMeta,
  PaginatedResponse,
  PaginationParams,
} from './types';

export { SentiApiError, createApiClient } from './client';
export type { ApiClient, ApiClientConfig } from './client';
