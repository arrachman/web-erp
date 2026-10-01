import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { randomUUID } from 'crypto';
import { promises as fs } from 'fs';
import * as path from 'path';
import { PrismaService } from '../prisma/prisma.service';

/** Shape of a multer in-memory upload. */
export interface UploadedAttachmentFile {
  originalname: string;
  mimetype: string;
  size: number;
  buffer: Buffer;
}

/** Domains that own a `<domain>_transaction_attachments` table. */
export type AttachmentDomain = 'fin' | 'inv' | 'pur' | 'sls';
const DOMAINS: AttachmentDomain[] = ['fin', 'inv', 'pur', 'sls'];

// Minimal structural view of the 4 identical Prisma delegates — they share the
// same fields, so we operate through this narrow interface and pick the concrete
// delegate per domain (avoids a 4-way union at every call site).
interface AttachmentDelegate {
  findMany(args: unknown): Promise<unknown[]>;
  findFirst(args: unknown): Promise<Record<string, unknown> | null>;
  create(args: unknown): Promise<Record<string, unknown>>;
  update(args: unknown): Promise<Record<string, unknown>>;
  delete(args: unknown): Promise<unknown>;
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
const MAX_ATTACHMENTS_PER_DOC = 30;

/** Max upload accepted by multer before per-file checks. */
export const TRANSACTION_ATTACHMENT_MAX_UPLOAD_BYTES = MAX_ATTACHMENT_BYTES;

@Injectable()
export class ErpTransactionAttachmentsService {
  private readonly uploadDir =
    process.env.ERP_TXN_UPLOAD_DIR ?? path.join(process.cwd(), 'uploads', 'erp-transactions');

  constructor(private readonly prisma: PrismaService) {}

  private delegate(domain: string): AttachmentDelegate {
    switch (domain) {
      case 'fin': return this.prisma.erpFinTransactionAttachment as unknown as AttachmentDelegate;
      case 'inv': return this.prisma.erpInvTransactionAttachment as unknown as AttachmentDelegate;
      case 'pur': return this.prisma.erpPurTransactionAttachment as unknown as AttachmentDelegate;
      case 'sls': return this.prisma.erpSlsTransactionAttachment as unknown as AttachmentDelegate;
      default:
        throw new BadRequestException(`Domain "${domain}" tidak dikenal (fin/inv/pur/sls)`);
    }
  }

  private assertDocType(docType: string) {
    if (!docType || !/^[A-Za-z0-9._-]{1,32}$/.test(docType)) {
      throw new BadRequestException('docType tidak valid');
    }
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

  async list(domain: string, docType: string, docId: bigint) {
    this.assertDocType(docType);
    const rows = await this.delegate(domain).findMany({
      where: { docType, docId },
      orderBy: [{ sortOrder: 'asc' }, { id: 'asc' }],
    });
    return { success: true, data: rows };
  }

  async upload(
    domain: string,
    docType: string,
    docId: bigint,
    file: UploadedAttachmentFile | undefined,
    note: string | undefined,
    userId?: bigint,
  ) {
    this.assertDocType(docType);
    if (!file?.buffer?.length) throw new BadRequestException('File tidak ditemukan di request');

    const ext = ATTACHMENT_MIME_EXT[file.mimetype];
    if (!ext) {
      throw new BadRequestException(
        `Tipe file ${file.mimetype} tidak didukung (dukung: PDF, gambar, Word, Excel, PowerPoint, CSV, teks, ZIP)`,
      );
    }
    if (file.size > MAX_ATTACHMENT_BYTES) {
      throw new BadRequestException(
        `Ukuran file melebihi batas ${Math.round(MAX_ATTACHMENT_BYTES / 1024 / 1024)} MB`,
      );
    }

    const delegate = this.delegate(domain);
    const existing = (await delegate.findMany({
      where: { docType, docId },
      select: { sortOrder: true },
    })) as Array<{ sortOrder: number }>;
    if (existing.length >= MAX_ATTACHMENTS_PER_DOC) {
      throw new BadRequestException(`Maksimal ${MAX_ATTACHMENTS_PER_DOC} lampiran per transaksi`);
    }

    const storedName = `${domain}-${docType}-${docId}-${randomUUID()}.${ext}`;
    await fs.mkdir(this.uploadDir, { recursive: true });
    await fs.writeFile(this.absPath(storedName), file.buffer);

    try {
      const created = await delegate.create({
        data: {
          docType,
          docId,
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

  async updateNote(
    domain: string,
    docType: string,
    docId: bigint,
    attachmentId: bigint,
    note: string | undefined,
  ) {
    await this.findOwned(domain, docType, docId, attachmentId);
    const updated = await this.delegate(domain).update({
      where: { id: attachmentId },
      data: { note: note?.trim() || null },
    });
    return { success: true, data: updated };
  }

  async remove(domain: string, docType: string, docId: bigint, attachmentId: bigint) {
    const att = await this.findOwned(domain, docType, docId, attachmentId);
    await this.delegate(domain).delete({ where: { id: attachmentId } });
    await this.unlinkQuiet(att.storedName as string);
    return { success: true };
  }

  /** Resolve on-disk file for download (GET …/file). */
  async resolveFile(domain: string, docType: string, docId: bigint, attachmentId: bigint) {
    const att = await this.findOwned(domain, docType, docId, attachmentId);
    return {
      absPath: this.absPath(att.storedName as string),
      mimeType: att.mimeType as string,
      fileName: att.fileName as string,
    };
  }

  private async findOwned(
    domain: string,
    docType: string,
    docId: bigint,
    attachmentId: bigint,
  ): Promise<Record<string, unknown>> {
    this.assertDocType(docType);
    const att = await this.delegate(domain).findFirst({
      where: { id: attachmentId, docType, docId },
    });
    if (!att) throw new NotFoundException('Lampiran tidak ditemukan');
    return att;
  }

  static isDomain(value: string): value is AttachmentDomain {
    return (DOMAINS as string[]).includes(value);
  }
}
