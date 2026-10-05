import path from 'node:path';

/** Ambil env string; kembalikan fallback bila undefined / kosong. */
function env(key: string, fallback = ''): string {
  const v = process.env[key];
  return v === undefined || v.trim() === '' ? fallback : v;
}

const dataDir = path.resolve(env('WA_GATEWAY_DATA_DIR', path.join(process.cwd(), 'data')));

export const config = {
  /** Port HTTP service. */
  port: Number(env('WA_GATEWAY_PORT', '3204')),
  /** Token level-akun (harus sama dengan SENTIWA_ACCOUNT_TOKEN di api-gateway). */
  accountToken: env('WA_GATEWAY_ACCOUNT_TOKEN'),
  /** URL webhook tujuan event delivery/read (dipakai di langkah 3). */
  webhookUrl: env('WA_GATEWAY_WEBHOOK_URL'),
  /** Shared-secret header webhook (opsional). */
  webhookSecret: env('WA_GATEWAY_WEBHOOK_SECRET'),
  /** Dir data persisten. */
  dataDir,
  /** sessions/<token> → auth creds Baileys per device. */
  sessionsDir: path.join(dataDir, 'sessions'),
  /** registry.json → metadata device (token ↔ nama/nomor). */
  registryFile: path.join(dataDir, 'registry.json'),
  /** Level log service. */
  logLevel: env('WA_GATEWAY_LOG_LEVEL', 'info'),
  /** Level log internal Baileys (chatty di debug). */
  baileysLogLevel: env('WA_GATEWAY_BAILEYS_LOG_LEVEL', 'silent'),
} as const;

export type AppConfig = typeof config;
