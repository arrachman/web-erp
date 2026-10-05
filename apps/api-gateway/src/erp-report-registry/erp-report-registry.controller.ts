import {
  Body,
  Controller,
  Get,
  Param,
  Post,
  Query,
  Req,
  Res,
  UseGuards,
} from '@nestjs/common';
import type { Response } from 'express';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { ErpReportRegistryService } from './erp-report-registry.service';
import { RenderRegistryReportDto } from './dto/render-report.dto';

/**
 * Report registry API (Wave G0) — feeds the per-module Reports hub combo
 * box and renders converted .mrt templates in HTML/PDF/DOCX/XLSX from
 * the single pagination model.
 */
@Controller('erp/report-registry')
@UseGuards(ErpJwtAuthGuard)
export class ErpReportRegistryController {
  constructor(private readonly service: ErpReportRegistryService) {}

  @Get()
  list(@Query('module') module?: string) {
    return this.service.list(module ?? '');
  }

  @Get(':code')
  detail(@Param('code') code: string) {
    return this.service.detail(code);
  }

  @Post(':code/render')
  async render(
    @Param('code') code: string,
    @Body() dto: RenderRegistryReportDto,
    @Req() req: { user?: { name?: string; fullName?: string; email?: string } },
    @Res() res: Response,
  ) {
    const userName = req.user?.fullName ?? req.user?.name ?? req.user?.email;
    const out = await this.service.render(code, dto, userName);
    res.setHeader('Content-Type', out.contentType);
    if (dto.format !== 'html') {
      res.setHeader('Content-Disposition', `attachment; filename="${out.filename}"`);
    }
    // NOTE: do not return the response object — the global bigint
    // serializer interceptor would try to serialize it (circular).
    res.send(out.body);
  }
}
