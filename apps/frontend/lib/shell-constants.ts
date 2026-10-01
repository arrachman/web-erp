/** Maximum number of concurrent open tabs in the shell. */
export const MAX_TABS = 16;

/** localStorage key for persisting the active ShellUser across refreshes. */
export const USER_STORAGE_KEY = 'erp-user';

/** Default workspace id created automatically on first login. */
export const DEFAULT_WORKSPACE_ID = 'ws1';

/** Base path for global (no-workspace) mode. Static Next.js segment /app. */
export const GLOBAL_BASE_PATH = '/app';

/** Supported UI languages. */
export type Lang = 'id' | 'en' | 'ja';
