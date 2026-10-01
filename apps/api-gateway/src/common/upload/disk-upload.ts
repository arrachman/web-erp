import { randomUUID } from 'crypto';
import * as fs from 'fs';
import * as path from 'path';

/**
 * Multer disk storage — streams upload to disk instead of buffering the whole
 * file in memory (critical for video up to 50 MB).
 *
 * Uses require('multer') so we don't depend on @types/multer.
 */
// eslint-disable-next-line @typescript-eslint/no-require-imports
const multer = require('multer') as {
  diskStorage: (opts: {
    destination: (
      req: unknown,
      file: { originalname: string },
      cb: (err: Error | null, dest: string) => void,
    ) => void;
    filename: (
      req: unknown,
      file: { originalname: string },
      cb: (err: Error | null, name: string) => void,
    ) => void;
  }) => unknown;
};

export function makeDiskStorage(opts: {
  dest: string;
  /** Filename prefix, e.g. "media" or "att". */
  prefix?: string;
}) {
  const dest = opts.dest;
  const prefix = opts.prefix ?? 'up';
  fs.mkdirSync(dest, { recursive: true });

  return multer.diskStorage({
    destination: (_req, _file, cb) => {
      cb(null, dest);
    },
    filename: (_req, file, cb) => {
      const ext = path.extname(file.originalname || '').slice(0, 12).toLowerCase() || '';
      const safeExt = /^\.[a-z0-9]+$/.test(ext) ? ext : '';
      cb(null, `${prefix}-${randomUUID()}${safeExt}`);
    },
  });
}

/** Shape after FileInterceptor + diskStorage (path set, buffer usually absent). */
export interface DiskUploadedFile {
  originalname: string;
  mimetype: string;
  size: number;
  filename: string;
  path: string;
  destination?: string;
  buffer?: Buffer;
}

export async function readUploadBuffer(file: DiskUploadedFile): Promise<Buffer> {
  if (file.buffer?.length) return file.buffer;
  if (file.path) return fs.promises.readFile(file.path);
  throw new Error('Upload file has neither buffer nor path');
}

export async function unlinkQuiet(filePath?: string | null): Promise<void> {
  if (!filePath) return;
  try {
    await fs.promises.unlink(filePath);
  } catch {
    // ignore
  }
}
