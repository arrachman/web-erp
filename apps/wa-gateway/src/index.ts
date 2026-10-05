import fs from 'node:fs';
import { config } from './config';
import { logger } from './logger';
import { createServer } from './server';
import { sessionManager } from './session-manager';

function main(): void {
  fs.mkdirSync(config.sessionsDir, { recursive: true });

  if (!config.accountToken) {
    logger.warn(
      'WA_GATEWAY_ACCOUNT_TOKEN kosong — endpoint level-akun (/get-devices, /add-device, /delete-device) akan tolak semua request. Set token sebelum pakai.',
    );
  }

  // Resume sesi yang sudah ter-pair sebelum restart.
  sessionManager.resumeAll();
  // Jaring pengaman: paksa reconnect sesi yang macet diam (insiden 29 Jul 2026).
  sessionManager.startWatchdog();

  const app = createServer();
  app.listen(config.port, () => {
    logger.info({ port: config.port, dataDir: config.dataDir }, 'wa-gateway listening');
  });
}

main();
