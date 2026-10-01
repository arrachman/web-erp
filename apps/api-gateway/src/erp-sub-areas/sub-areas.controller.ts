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
  UseGuards,
} from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpSubAreaDto, BulkStatusErpSubAreaDto } from './dto/bulk-sub-areas.dto';
import { CreateErpSubAreaDto } from './dto/create-sub-areas.dto';
import { QueryErpSubAreaDto } from './dto/query-sub-areas.dto';
import { UpdateErpSubAreaDto } from './dto/update-sub-areas.dto';
import { ErpSubAreasService } from './sub-areas.service';

@ApiTags('ERP Sub-Areas')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/sub-areas')
export class ErpSubAreasController {
  constructor(private readonly service: ErpSubAreasService) {}

  @Post()
  @ApiOperation({ summary: 'Create Sub-Area (Kelurahan)' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpSubAreaDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List Sub-Areas (Kelurahan)' })
  findAll(@Query() query: QueryErpSubAreaDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get Sub-Area' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Sub-Area' })
  update(@Param('id') id: string, @Body() dto: UpdateErpSubAreaDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpSubAreaDto, @Request() req: any) {
    return this.service.bulkUpdateStatus(dto, req.user?.id);
  }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpSubAreaDto, @Request() req: any) {
    return this.service.bulkDelete(dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Sub-Area' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
