import {
  Body,
  Controller,
  Delete,
  Get,
  Param,
  Patch,
  Post,
  Query,
  Request,
  Res,
  UseGuards,
} from '@nestjs/common';
import type { Response } from 'express';
import { ApiBearerAuth, ApiOperation, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { CreateErpReportDto } from './dto/create-report.dto';
import { ExecuteSqlDto } from './dto/execute-sql.dto';
import { QueryErpReportDto } from './dto/query-report.dto';
import { UpdateErpReportDto } from './dto/update-report.dto';
import { ErpReportsService } from './erp-reports.service';

@ApiTags('ERP Reports')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/reports')
export class ErpReportsController {
  constructor(private readonly service: ErpReportsService) {}

  @Get()
  @ApiOperation({ summary: 'List report templates' })
  findAll(@Query() query: QueryErpReportDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one report template' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Post()
  @ApiOperation({ summary: 'Create report template' })
  create(@Body() dto: CreateErpReportDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update report template' })
  update(@Param('id') id: string, @Body() dto: UpdateErpReportDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete report template' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }

  @Post('execute-sql')
  @ApiOperation({ summary: 'Execute a read-only SQL query (SELECT only)' })
  executeSql(@Body() dto: ExecuteSqlDto) {
    return this.service.executeSql(dto);
  }

  @Post(':id/materialize')
  @ApiOperation({ summary: 'Generate editable bands from the bound report columns' })
  materialize(@Param('id') id: string, @Request() req: any) {
    return this.service.materializeTemplate(BigInt(id), req.user?.id);
  }

  @Post('preview')
  @ApiOperation({ summary: 'Render a template to PDF with sample data (designer preview)' })
  async preview(
    @Body() body: { templateJson?: Record<string, unknown> },
    @Res() res: Response,
  ) {
    const buffer = await this.service.previewTemplate(body?.templateJson ?? {});
    res.set({
      'Content-Type': 'application/pdf',
      'Content-Disposition': 'inline; filename="preview.pdf"',
      'Content-Length': String(buffer.length),
    });
    res.send(buffer);
  }
}
