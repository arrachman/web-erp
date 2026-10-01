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
import * as path from 'path';
import { makeDiskStorage } from '../common/upload/disk-upload';
import { ApiBearerAuth, ApiConsumes, ApiOperation, ApiTags } from '@nestjs/swagger';
import type { Response } from 'express';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import {
  ErpItemAttachmentsService,
  ITEM_ATTACHMENT_MAX_UPLOAD_BYTES,
  UploadedAttachmentFile,
} from './erp-item-attachments.service';

@ApiTags('ERP Item Attachments')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/items/:itemId/attachments')
export class ErpItemAttachmentsController {
  constructor(private readonly service: ErpItemAttachmentsService) {}

  @Get()
  @ApiOperation({ summary: 'List lampiran (dokumen pendukung) milik satu item' })
  list(@Param('itemId') itemId: string) {
    return this.service.list(BigInt(itemId));
  }

  @Post()
  @ApiOperation({ summary: 'Upload lampiran (multipart "file" + opsional "note")' })
  @ApiConsumes('multipart/form-data')
  @UseInterceptors(
    FileInterceptor('file', {
      limits: { fileSize: ITEM_ATTACHMENT_MAX_UPLOAD_BYTES },
      storage: makeDiskStorage({
        dest:
          process.env.ERP_UPLOAD_DIR ??
          path.join(process.cwd(), 'uploads', 'erp-items'),
        prefix: 'att',
      }),
    }),
  )
  upload(
    @Param('itemId') itemId: string,
    @Body('note') note: string | undefined,
    @UploadedFile() file: UploadedAttachmentFile,
    @Request() req: any,
  ) {
    const userId = req.user?.id ? BigInt(req.user.id) : undefined;
    return this.service.upload(BigInt(itemId), file, note, userId);
  }

  @Patch(':attachmentId')
  @ApiOperation({ summary: 'Ubah catatan (note) lampiran' })
  updateNote(
    @Param('itemId') itemId: string,
    @Param('attachmentId') attachmentId: string,
    @Body('note') note: string | undefined,
  ) {
    return this.service.updateNote(BigInt(itemId), BigInt(attachmentId), note);
  }

  @Delete(':attachmentId')
  @ApiOperation({ summary: 'Hapus satu lampiran item (file + metadata)' })
  remove(@Param('itemId') itemId: string, @Param('attachmentId') attachmentId: string) {
    return this.service.remove(BigInt(itemId), BigInt(attachmentId));
  }

  @Get(':attachmentId/file')
  @ApiOperation({ summary: 'Stream/download file lampiran' })
  async getFile(
    @Param('itemId') itemId: string,
    @Param('attachmentId') attachmentId: string,
    @Res() res: Response,
  ) {
    const { absPath, mimeType, fileName } = await this.service.resolveFile(
      BigInt(itemId),
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
