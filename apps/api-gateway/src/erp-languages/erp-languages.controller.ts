import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpLanguageDeleteDto, BulkErpLanguageStatusDto } from './dto/bulk-erp-language.dto';
import { CreateErpLanguageDto } from './dto/create-erp-language.dto';
import { QueryErpLanguageDto } from './dto/query-erp-language.dto';
import { UpdateErpLanguageDto } from './dto/update-erp-language.dto';
import { ErpLanguagesService } from './erp-languages.service';

@ApiTags('ERP Languages')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/languages')
export class ErpLanguagesController {
  constructor(private readonly service: ErpLanguagesService) {}

  @Post()
  @ApiOperation({ summary: 'Create Language' })
  create(@Body() dto: CreateErpLanguageDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List Languages (paginated)' })
  findAll(@Query() query: QueryErpLanguageDto) {
    return this.service.findAll(query);
  }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate languages' })
  bulkUpdateStatus(@Body() dto: BulkErpLanguageStatusDto, @Request() req: any) {
    return this.service.bulkUpdateStatus(dto, req.user?.id);
  }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete languages' })
  bulkDelete(@Body() dto: BulkErpLanguageDeleteDto, @Request() req: any) {
    return this.service.bulkDelete(dto, req.user?.id);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get Language by ID' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Language' })
  update(@Param('id') id: string, @Body() dto: UpdateErpLanguageDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Language' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
