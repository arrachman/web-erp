// HTTP client for the Senti ERP backend.
//
// The transport (fetch wrapper, error envelope, upload/download) now lives in
// the shared @sentient-factory/ui-kit (Tier 1) via `createApiClient`. This file
// only binds the ERP base URL and re-exports the verbs under the existing names,
// so every `@/lib/api/client` import across the app stays stable.
//
// Base URL: NEXT_PUBLIC_ERP_API_URL env (required in production)
// Cookies (erp_token) are sent automatically via credentials: 'include'
// BigInt IDs are expected as strings (backend serialises BigInt → string)

import { createApiClient, SentiApiError } from '@sentient-factory/ui-kit';

const BASE_URL =
  process.env.NEXT_PUBLIC_ERP_API_URL ?? 'https://erp.fr-labs.my.id/api/erp';

const client = createApiClient({ baseUrl: BASE_URL });

/** ERP-named alias of the shared error class (identical shape + behaviour). */
export const ErpApiError = SentiApiError;

export const {
  apiGet,
  apiPost,
  apiPatch,
  apiPut,
  apiDelete,
  apiUpload,
  downloadFile,
  buildApiUrl,
} = client;
