import {
  Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards,
} from '@nestjs/common';
import { ApiBearerAuth, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpProjectDto, BulkStatusErpProjectDto } from './dto/bulk-erp-project.dto';
import { CreateErpProjectDto } from './dto/create-erp-project.dto';
import { QueryErpProjectDto } from './dto/query-erp-project.dto';
import { UpdateErpProjectDto } from './dto/update-erp-project.dto';
import { ErpProjectsService } from './erp-projects.service';

@ApiTags('ERP Projects')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/projects')
export class ErpProjectsController {
  constructor(private readonly service: ErpProjectsService) {}

  @Post()
  create(@Body() dto: CreateErpProjectDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }
  @Get()
  findAll(@Query() query: QueryErpProjectDto) { return this.service.findAll(query); }
  @Get(':id')
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }
  @Patch(':id')
  update(@Param('id') id: string, @Body() dto: UpdateErpProjectDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }
  @Patch('bulk/status')
  bulkUpdateStatus(@Body() dto: BulkStatusErpProjectDto, @Request() req: any) {
    return this.service.bulkUpdateStatus(dto, req.user?.id);
  }
  @Delete('bulk')
  bulkDelete(@Body() dto: BulkErpProjectDto, @Request() req: any) {
    return this.service.bulkDelete(dto, req.user?.id);
  }
  @Delete(':id')
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
