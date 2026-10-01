import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { randomUUID } from 'crypto';
import { promises as fs } from 'fs';
import * as path from 'path';
import { ErpItemMediaKind } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';

/** Shape of a multer in-memory upload (mirrors erp-import's UploadedFileLike). */
export interface UploadedMediaFile {
  originalname: string;
  mimetype: string;
  size: number;
  /** Present for memoryStorage uploads. */
  buffer?: Buffer;
  /** Present for diskStorage uploads (preferred — no full-file RAM). */
  path?: string;
  filename?: string;
}

// Whitelisted mime types → on-disk extension (extension derives from the mime,
// never from the user-supplied filename — prevents path tricks).
const IMAGE_MIME_EXT: Record<string, string> = {
  'image/jpeg': 'jpg',
  'image/png': 'png',
  'image/webp': 'webp',
  'image/gif': 'gif',
};
const VIDEO_MIME_EXT: Record<string, string> = {
  'video/mp4': 'mp4',
  'video/webm': 'webm',
  'video/quicktime': 'mov',
};

const MAX_IMAGE_BYTES = 5 * 1024 * 1024; // 5 MB
const MAX_VIDEO_BYTES = 50 * 1024 * 1024; // 50 MB (video pendek)
const MAX_IMAGES_PER_ITEM = 8; // satu video per item; video baru replace yang lama

/** Max upload accepted by multer before per-kind checks (= video cap). */
export const ITEM_MEDIA_MAX_UPLOAD_BYTES = MAX_VIDEO_BYTES;

@Injectable()
export class ErpItemMediaService {
  private readonly uploadDir =
    process.env.ERP_UPLOAD_DIR ?? path.join(process.cwd(), 'uploads', 'erp-items');

  constructor(private readonly prisma: PrismaService) {}

  private async assertItem(itemId: bigint) {
    const item = await this.prisma.erpItem.findFirst({
      where: { id: itemId, deletedAt: null },
      select: { id: true },
    });
    if (!item) throw new NotFoundException('Item tidak ditemukan');
  }

  private absPath(storedName: string): string {
    return path.join(this.uploadDir, storedName);
  }

  private async unlinkQuiet(storedName: string) {
    try {
      await fs.unlink(this.absPath(storedName));
    } catch {
      // file already gone — DB row is the source of truth, ignore
    }
  }

  private async unlinkPathQuiet(filePath?: string) {
    if (!filePath) return;
    try {
      await fs.unlink(filePath);
    } catch {
      // temporary upload already moved/removed
    }
  }

  async list(itemId: bigint) {
    await this.assertItem(itemId);
    const rows = await this.prisma.erpItemMedia.findMany({
      where: { itemId },
      orderBy: [{ isPrimary: 'desc' }, { sortOrder: 'asc' }, { id: 'asc' }],
    });
    return { success: true, data: rows };
  }

  async upload(
    itemId: bigint,
    kind: ErpItemMediaKind,
    file: UploadedMediaFile | undefined,
    userId?: bigint,
  ) {
    if (!file || (!file.buffer?.length && !file.path)) {
      throw new BadRequestException('File tidak ditemukan di request');
    }
    await this.assertItem(itemId);

    const isImage = kind === ErpItemMediaKind.IMAGE;
    const extMap = isImage ? IMAGE_MIME_EXT : VIDEO_MIME_EXT;
    const ext = extMap[file.mimetype];
    if (!ext) {
      await this.unlinkPathQuiet(file.path);
      const allowed = Object.keys(extMap).join(', ');
      throw new BadRequestException(
        `Tipe file ${file.mimetype} tidak didukung untuk ${isImage ? 'gambar' : 'video'} (dukung: ${allowed})`,
      );
    }

    const maxBytes = isImage ? MAX_IMAGE_BYTES : MAX_VIDEO_BYTES;
    if (file.size > maxBytes) {
      await this.unlinkPathQuiet(file.path);
      throw new BadRequestException(
        `Ukuran file melebihi batas ${Math.round(maxBytes / 1024 / 1024)} MB`,
      );
    }

    const existing = await this.prisma.erpItemMedia.findMany({
      where: { itemId, kind },
      select: { id: true, storedName: true, sortOrder: true },
    });
    if (isImage && existing.length >= MAX_IMAGES_PER_ITEM) {
      await this.unlinkPathQuiet(file.path);
      throw new BadRequestException(`Maksimal ${MAX_IMAGES_PER_ITEM} gambar per item`);
    }

    const storedName = `${itemId}-${randomUUID()}.${ext}`;
    await fs.mkdir(this.uploadDir, { recursive: true });
    const dest = this.absPath(storedName);
    if (file.path) {
      // diskStorage already streamed to temp path — move into final name
      await fs.rename(file.path, dest).catch(async () => {
        await fs.copyFile(file.path!, dest);
        await fs.unlink(file.path!).catch(() => undefined);
      });
    } else {
      await fs.writeFile(dest, file.buffer!);
    }

    try {
      const created = await this.prisma.$transaction(async (tx) => {
        // Video baru menggantikan video lama (satu video per item).
        if (!isImage && existing.length > 0) {
          await tx.erpItemMedia.deleteMany({ where: { id: { in: existing.map((m) => m.id) } } });
        }
        return tx.erpItemMedia.create({
          data: {
            itemId,
            kind,
            fileName: file.originalname || storedName,
            storedName,
            mimeType: file.mimetype,
            sizeBytes: file.size,
            sortOrder: existing.reduce((max, m) => Math.max(max, m.sortOrder), 0) + 1,
            isPrimary: isImage && existing.length === 0,
            createdById: userId ?? null,
          },
        });
      });
      if (!isImage) {
        await Promise.all(existing.map((m) => this.unlinkQuiet(m.storedName)));
      }
      return { success: true, data: created };
    } catch (err) {
      await this.unlinkQuiet(storedName); // DB gagal → jangan tinggalkan file yatim
      throw err;
    }
  }

  async setPrimary(itemId: bigint, mediaId: bigint) {
    const media = await this.findOwned(itemId, mediaId);
    if (media.kind !== ErpItemMediaKind.IMAGE) {
      throw new BadRequestException('Hanya gambar yang bisa dijadikan utama');
    }
    const [, updated] = await this.prisma.$transaction([
      this.prisma.erpItemMedia.updateMany({
        where: { itemId, kind: ErpItemMediaKind.IMAGE },
        data: { isPrimary: false },
      }),
      this.prisma.erpItemMedia.update({ where: { id: mediaId }, data: { isPrimary: true } }),
    ]);
    return { success: true, data: updated };
  }

  async remove(itemId: bigint, mediaId: bigint) {
    const media = await this.findOwned(itemId, mediaId);
    await this.prisma.erpItemMedia.delete({ where: { id: mediaId } });
    await this.unlinkQuiet(media.storedName);
    // Gambar utama dihapus → promosikan gambar tersisa pertama.
    if (media.isPrimary) {
      const next = await this.prisma.erpItemMedia.findFirst({
        where: { itemId, kind: ErpItemMediaKind.IMAGE },
        orderBy: [{ sortOrder: 'asc' }, { id: 'asc' }],
      });
      if (next) {
        await this.prisma.erpItemMedia.update({
          where: { id: next.id },
          data: { isPrimary: true },
        });
      }
    }
    return { success: true };
  }

  /** Resolve on-disk file for streaming (GET …/file). */
  async resolveFile(itemId: bigint, mediaId: bigint) {
    const media = await this.findOwned(itemId, mediaId);
    return {
      absPath: this.absPath(media.storedName),
      mimeType: media.mimeType,
      fileName: media.fileName,
    };
  }

  private async findOwned(itemId: bigint, mediaId: bigint) {
    const media = await this.prisma.erpItemMedia.findFirst({
      where: { id: mediaId, itemId },
    });
    if (!media) throw new NotFoundException('Media tidak ditemukan');
    return media;
  }
}
