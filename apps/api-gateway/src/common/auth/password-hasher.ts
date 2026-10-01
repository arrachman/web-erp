import { pbkdf2Sync, randomBytes, timingSafeEqual } from 'crypto';

const PBKDF2_PREFIX = 'pbkdf2$v1';
const PBKDF2_DIGEST = 'sha512';
const PBKDF2_ITERATIONS = 210000;
const KEYLEN = 64;

export async function hashPassword(password: string): Promise<string> {
  const salt = randomBytes(16);
  const derived = pbkdf2Sync(password, salt, PBKDF2_ITERATIONS, KEYLEN, PBKDF2_DIGEST);

  return `${PBKDF2_PREFIX}$${PBKDF2_DIGEST}$${PBKDF2_ITERATIONS}$${salt.toString('base64')}$${derived.toString('base64')}`;
}

export async function verifyPassword(password: string, storedHash: string): Promise<boolean> {
  if (!storedHash) {
    return false;
  }

  if (!storedHash.startsWith(`${PBKDF2_PREFIX}$`)) {
    return false;
  }

  const parts = storedHash.split('$');
  if (parts.length !== 6) {
    return false;
  }

  const digest = parts[2];
  const iterations = Number(parts[3]);
  const saltB64 = parts[4];
  const expectedB64 = parts[5];

  if (!Number.isFinite(iterations) || iterations <= 0) {
    return false;
  }

  const salt = Buffer.from(saltB64, 'base64');
  const expected = Buffer.from(expectedB64, 'base64');
  const derived = pbkdf2Sync(password, salt, iterations, expected.length, digest);

  if (derived.length !== expected.length) {
    return false;
  }

  return timingSafeEqual(derived, expected);
}
