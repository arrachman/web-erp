// Base response/pagination types — now sourced from the shared
// @sentient-factory/ui-kit (Tier 1). Re-exported so the ~140 resource modules
// importing from `./types` stay unchanged.
export type {
  ApiError,
  ApiResponse,
  PaginatedMeta,
  PaginatedResponse,
  PaginationParams,
} from '@sentient-factory/ui-kit';
