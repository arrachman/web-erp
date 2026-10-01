import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { randomUUID } from 'crypto';
import { promises as fs } from 'fs';
import * as path from 'path';
import { PrismaService } from '../prisma/prisma.service';

/** Shape of a multer in-memory upload (mirrors erp-item-media's UploadedMediaFile). */
export interface UploadedAttachmentFile {
  originalname: string;
  mimetype: string;
  size: number;
  buffer?: Buffer;
  path?: string;
  filename?: string;
}

// Whitelisted document/image mime types → on-disk extension. Extension derives
// from the mime, never from the user-supplied filename (prevents path tricks).
const ATTACHMENT_MIME_EXT: Record<string, string> = {
  'application/pdf': 'pdf',
  'image/jpeg': 'jpg',
  'image/png': 'png',
  'image/webp': 'webp',
  'image/gif': 'gif',
  'application/msword': 'doc',
  'application/vnd.openxmlformats-officedocument.wordprocessingml.document': 'docx',
  'application/vnd.ms-excel': 'xls',
  'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet': 'xlsx',
  'application/vnd.ms-powerpoint': 'ppt',
  'application/vnd.openxmlformats-officedocument.presentationml.presentation': 'pptx',
  'text/csv': 'csv',
  'text/plain': 'txt',
  'application/zip': 'zip',
  'application/x-zip-compressed': 'zip',
};

const MAX_ATTACHMENT_BYTES = 10 * 1024 * 1024; // 10 MB per file
const MAX_ATTACHMENTS_PER_ITEM = 20;

/** Max upload accepted by multer before per-file checks. */
export const ITEM_ATTACHMENT_MAX_UPLOAD_BYTES = MAX_ATTACHMENT_BYTES;

@Injectable()
export class ErpItemAttachmentsService {
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
    const rows = await this.prisma.erpItemAttachment.findMany({
      where: { itemId },
      orderBy: [{ sortOrder: 'asc' }, { id: 'asc' }],
    });
    return { success: true, data: rows };
  }

  async upload(
    itemId: bigint,
    file: UploadedAttachmentFile | undefined,
    note: string | undefined,
    userId?: bigint,
  ) {
    if (!file || (!file.buffer?.length && !file.path)) {
      throw new BadRequestException('File tidak ditemukan di request');
    }
    await this.assertItem(itemId);

    const ext = ATTACHMENT_MIME_EXT[file.mimetype];
    if (!ext) {
      await this.unlinkPathQuiet(file.path);
      throw new BadRequestException(
        `Tipe file ${file.mimetype} tidak didukung (dukung: PDF, gambar, Word, Excel, PowerPoint, CSV, teks, ZIP)`,
      );
    }
    if (file.size > MAX_ATTACHMENT_BYTES) {
      await this.unlinkPathQuiet(file.path);
      throw new BadRequestException(
        `Ukuran file melebihi batas ${Math.round(MAX_ATTACHMENT_BYTES / 1024 / 1024)} MB`,
      );
    }

    const existing = await this.prisma.erpItemAttachment.findMany({
      where: { itemId },
      select: { sortOrder: true },
    });
    if (existing.length >= MAX_ATTACHMENTS_PER_ITEM) {
      await this.unlinkPathQuiet(file.path);
      throw new BadRequestException(`Maksimal ${MAX_ATTACHMENTS_PER_ITEM} lampiran per item`);
    }

    const storedName = `att-${itemId}-${randomUUID()}.${ext}`;
    await fs.mkdir(this.uploadDir, { recursive: true });
    const dest = this.absPath(storedName);
    if (file.path) {
      await fs.rename(file.path, dest).catch(async () => {
        await fs.copyFile(file.path!, dest);
        await fs.unlink(file.path!).catch(() => undefined);
      });
    } else {
      await fs.writeFile(dest, file.buffer!);
    }

    try {
      const created = await this.prisma.erpItemAttachment.create({
        data: {
          itemId,
          fileName: file.originalname || storedName,
          storedName,
          mimeType: file.mimetype,
          sizeBytes: file.size,
          note: note?.trim() || null,
          sortOrder: existing.reduce((max, m) => Math.max(max, m.sortOrder), 0) + 1,
          createdById: userId ?? null,
        },
      });
      return { success: true, data: created };
    } catch (err) {
      await this.unlinkQuiet(storedName); // DB gagal → jangan tinggalkan file yatim
      throw err;
    }
  }

  async updateNote(itemId: bigint, attachmentId: bigint, note: string | undefined) {
    await this.findOwned(itemId, attachmentId);
    const updated = await this.prisma.erpItemAttachment.update({
      where: { id: attachmentId },
      data: { note: note?.trim() || null },
    });
    return { success: true, data: updated };
  }

  async remove(itemId: bigint, attachmentId: bigint) {
    const att = await this.findOwned(itemId, attachmentId);
    await this.prisma.erpItemAttachment.delete({ where: { id: attachmentId } });
    await this.unlinkQuiet(att.storedName);
    return { success: true };
  }

  /** Resolve on-disk file for download (GET …/file). */
  async resolveFile(itemId: bigint, attachmentId: bigint) {
    const att = await this.findOwned(itemId, attachmentId);
    return {
      absPath: this.absPath(att.storedName),
      mimeType: att.mimeType,
      fileName: att.fileName,
    };
  }

  private async findOwned(itemId: bigint, attachmentId: bigint) {
    const att = await this.prisma.erpItemAttachment.findFirst({
      where: { id: attachmentId, itemId },
    });
    if (!att) throw new NotFoundException('Lampiran tidak ditemukan');
    return att;
  }
}
