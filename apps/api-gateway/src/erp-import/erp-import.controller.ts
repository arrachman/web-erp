import {
  Controller,
  Get,
  Param,
  Post,
  Request,
  Res,
  UploadedFile,
  UseGuards,
  UseInterceptors,
} from '@nestjs/common';
import { FileInterceptor } from '@nestjs/platform-express';
import { ApiBearerAuth, ApiConsumes, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import type { Response } from 'express';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { ErpImportService, UploadedFileLike } from './erp-import.service';

@ApiTags('ERP Import')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/import')
export class ErpImportController {
  constructor(private readonly service: ErpImportService) {}

  @Get('entities')
  @ApiOperation({ summary: 'List supported import entities' })
  @ApiResponse({ status: 200, description: 'Supported entities' })
  entities() {
    return this.service.getEntities();
  }

  @Get('jobs')
  @ApiOperation({ summary: 'Recent import jobs (last 50)' })
  @ApiResponse({ status: 200, description: 'Import job history' })
  jobs() {
    return this.service.listJobs();
  }

  @Get('template/:entity')
  @ApiOperation({ summary: 'Download an XLSX template for an entity' })
  @ApiResponse({ status: 200, description: 'XLSX template stream' })
  async template(@Param('entity') entity: string, @Res() res: Response) {
    const { buffer, fileName } = await this.service.buildTemplate(entity);
    res.setHeader(
      'Content-Type',
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    );
    res.setHeader('Content-Disposition', `attachment; filename="${fileName}"`);
    res.send(buffer);
  }

  @Post(':entity')
  @ApiOperation({ summary: 'Import a file (XLSX/CSV) for an entity' })
  @ApiConsumes('multipart/form-data')
  @ApiResponse({ status: 201, description: 'Import summary' })
  @UseInterceptors(FileInterceptor('file'))
  import(
    @Param('entity') entity: string,
    @UploadedFile() file: UploadedFileLike,
    @Request() req: any,
  ) {
    return this.service.import(entity, file, req.user?.id);
  }
}
