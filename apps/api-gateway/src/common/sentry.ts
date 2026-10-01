import * as Sentry from '@sentry/node';

/**
 * Initialize Sentry untuk error tracking di api-gateway.
 * Called once di main.ts BEFORE NestFactory.create().
 *
 * Required env: SENTRY_DSN (optional — kalau kosong, init di-skip).
 */
export function initSentry() {
  const dsn = process.env.SENTRY_DSN;
  if (!dsn) {
     
    console.log('[sentry] SENTRY_DSN not set — error tracking disabled');
    return;
  }
  Sentry.init({
    dsn,
    environment: process.env.NODE_ENV ?? 'development',
    release: process.env.APP_VERSION,
    tracesSampleRate: process.env.NODE_ENV === 'production' ? 0.1 : 1.0,
    profilesSampleRate: 0.0,
    beforeSend(event) {
      // Filter out 401/403 (expected, not actionable)
      if (event.contexts?.response) {
        const status = (event.contexts.response as { status_code?: number }).status_code;
        if (status === 401 || status === 403) return null;
      }
      return event;
    },
  });
   
  console.log('[sentry] initialized');
}
