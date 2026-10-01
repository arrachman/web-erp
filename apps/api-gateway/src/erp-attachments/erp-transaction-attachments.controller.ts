import {
  Body,
  Controller,
  Delete,
  Get,
  Param,
  Patch,
  Post,
  Request,
  Res,
  UploadedFile,
  UseGuards,
  UseInterceptors,
} from '@nestjs/common';
import { FileInterceptor } from '@nestjs/platform-express';
import { ApiBearerAuth, ApiConsumes, ApiOperation, ApiTags } from '@nestjs/swagger';
import type { Response } from 'express';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import {
  ErpTransactionAttachmentsService,
  TRANSACTION_ATTACHMENT_MAX_UPLOAD_BYTES,
  UploadedAttachmentFile,
} from './erp-transaction-attachments.service';

// Lampiran transaksi generik. `domain` = fin|inv|pur|sls memilih tabel
// `<domain>_transaction_attachments`; (docType, docId) mengunci ke satu record
// transaksi. Mirip item-attachments tapi key polymorphic, bukan FK item.
@ApiTags('ERP Transaction Attachments')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/:domain/attachments')
export class ErpTransactionAttachmentsController {
  constructor(private readonly service: ErpTransactionAttachmentsService) {}

  @Get(':docType/:docId')
  @ApiOperation({ summary: 'List lampiran satu transaksi (docType + docId)' })
  list(
    @Param('domain') domain: string,
    @Param('docType') docType: string,
    @Param('docId') docId: string,
  ) {
    return this.service.list(domain, docType, BigInt(docId));
  }

  @Post(':docType/:docId')
  @ApiOperation({ summary: 'Upload lampiran transaksi (multipart "file" + opsional "note")' })
  @ApiConsumes('multipart/form-data')
  @UseInterceptors(FileInterceptor('file', { limits: { fileSize: TRANSACTION_ATTACHMENT_MAX_UPLOAD_BYTES } }))
  upload(
    @Param('domain') domain: string,
    @Param('docType') docType: string,
    @Param('docId') docId: string,
    @Body('note') note: string | undefined,
    @UploadedFile() file: UploadedAttachmentFile,
    @Request() req: any,
  ) {
    const userId = req.user?.id ? BigInt(req.user.id) : undefined;
    return this.service.upload(domain, docType, BigInt(docId), file, note, userId);
  }

  @Patch(':docType/:docId/:attachmentId')
  @ApiOperation({ summary: 'Ubah catatan (note) lampiran transaksi' })
  updateNote(
    @Param('domain') domain: string,
    @Param('docType') docType: string,
    @Param('docId') docId: string,
    @Param('attachmentId') attachmentId: string,
    @Body('note') note: string | undefined,
  ) {
    return this.service.updateNote(domain, docType, BigInt(docId), BigInt(attachmentId), note);
  }

  @Delete(':docType/:docId/:attachmentId')
  @ApiOperation({ summary: 'Hapus satu lampiran transaksi (file + metadata)' })
  remove(
    @Param('domain') domain: string,
    @Param('docType') docType: string,
    @Param('docId') docId: string,
    @Param('attachmentId') attachmentId: string,
  ) {
    return this.service.remove(domain, docType, BigInt(docId), BigInt(attachmentId));
  }

  @Get(':docType/:docId/:attachmentId/file')
  @ApiOperation({ summary: 'Stream/download file lampiran transaksi' })
  async getFile(
    @Param('domain') domain: string,
    @Param('docType') docType: string,
    @Param('docId') docId: string,
    @Param('attachmentId') attachmentId: string,
    @Res() res: Response,
  ) {
    const { absPath, mimeType, fileName } = await this.service.resolveFile(
      domain,
      docType,
      BigInt(docId),
      BigInt(attachmentId),
    );
    res.sendFile(absPath, {
      headers: {
        'Content-Type': mimeType,
        'Content-Disposition': `inline; filename="${encodeURIComponent(fileName)}"`,
        'Cache-Control': 'private, max-age=3600',
      },
      acceptRanges: true,
    });
  }
}
