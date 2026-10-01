import {
  BadRequestException,
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
import { ErpItemMediaKind } from '@prisma/client';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import {
  ErpItemMediaService,
  ITEM_MEDIA_MAX_UPLOAD_BYTES,
  UploadedMediaFile,
} from './erp-item-media.service';

@ApiTags('ERP Item Media')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/items/:itemId/media')
export class ErpItemMediaController {
  constructor(private readonly service: ErpItemMediaService) {}

  @Get()
  @ApiOperation({ summary: 'List media (gambar + video) milik satu item' })
  list(@Param('itemId') itemId: string) {
    return this.service.list(BigInt(itemId));
  }

  @Post()
  @ApiOperation({ summary: 'Upload gambar produk / video pendek (multipart "file" + "kind")' })
  @ApiConsumes('multipart/form-data')
  @UseInterceptors(
    FileInterceptor('file', {
      limits: { fileSize: ITEM_MEDIA_MAX_UPLOAD_BYTES },
      storage: makeDiskStorage({
        dest:
          process.env.ERP_UPLOAD_DIR ??
          path.join(process.cwd(), 'uploads', 'erp-items'),
        prefix: 'media',
      }),
    }),
  )
  upload(
    @Param('itemId') itemId: string,
    @Body('kind') kind: string,
    @UploadedFile() file: UploadedMediaFile,
    @Request() req: any,
  ) {
    if (kind !== ErpItemMediaKind.IMAGE && kind !== ErpItemMediaKind.VIDEO) {
      throw new BadRequestException('kind harus IMAGE atau VIDEO');
    }
    const userId = req.user?.id ? BigInt(req.user.id) : undefined;
    return this.service.upload(BigInt(itemId), kind, file, userId);
  }

  @Patch(':mediaId/primary')
  @ApiOperation({ summary: 'Jadikan gambar ini sebagai gambar utama item' })
  setPrimary(@Param('itemId') itemId: string, @Param('mediaId') mediaId: string) {
    return this.service.setPrimary(BigInt(itemId), BigInt(mediaId));
  }

  @Delete(':mediaId')
  @ApiOperation({ summary: 'Hapus satu media item (file + metadata)' })
  remove(@Param('itemId') itemId: string, @Param('mediaId') mediaId: string) {
    return this.service.remove(BigInt(itemId), BigInt(mediaId));
  }

  @Get(':mediaId/file')
  @ApiOperation({ summary: 'Stream file media (img/video src; support Range untuk video)' })
  async getFile(
    @Param('itemId') itemId: string,
    @Param('mediaId') mediaId: string,
    @Res() res: Response,
  ) {
    const { absPath, mimeType, fileName } = await this.service.resolveFile(
      BigInt(itemId),
      BigInt(mediaId),
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
