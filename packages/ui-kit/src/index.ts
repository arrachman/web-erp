// @sentient-factory/ui-kit — shared frontend foundation (Tier 1).
// Consumed as source via Next.js `transpilePackages`. See FRONTEND-DESIGN-SYSTEM.md.

export { cn } from './utils';

export type {
  ApiError,
  ApiResponse,
  PaginatedMeta,
  PaginatedResponse,
  PaginationParams,
  ApiClient,
  ApiClientConfig,
} from './api';
export { SentiApiError, createApiClient } from './api';

export { AppQueryProvider } from './providers/query-provider';
export type { AppQueryProviderProps } from './providers/query-provider';
