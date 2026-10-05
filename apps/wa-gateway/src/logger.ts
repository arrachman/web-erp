import pino, { type Logger } from 'pino';
import { config } from './config';

/** Logger utama service. */
export const logger: Logger = pino({
  level: config.logLevel,
  base: { svc: 'wa-gateway' },
});

/**
 * Logger khusus Baileys — defaultnya `silent` karena Baileys sangat verbose
 * di level debug. Bisa dinaikkan via WA_GATEWAY_BAILEYS_LOG_LEVEL untuk debug
 * masalah koneksi.
 */
export const baileysLogger: Logger = pino({ level: config.baileysLogLevel });
