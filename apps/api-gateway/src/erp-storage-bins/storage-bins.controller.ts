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
import { BulkErpStorageBinDto, BulkStatusErpStorageBinDto } from './dto/bulk-storage-bins.dto';
import { CreateErpStorageBinDto } from './dto/create-storage-bin.dto';
import { QueryErpStorageBinDto } from './dto/query-storage-bin.dto';
import { UpdateErpStorageBinDto } from './dto/update-storage-bin.dto';
import { ErpStorageBinsService } from './storage-bins.service';

@ApiTags('ERP Storage Bins')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/storage-bins')
export class ErpStorageBinsController {
  constructor(private readonly service: ErpStorageBinsService) {}

  @Post()
  @ApiOperation({ summary: 'Create Storage Bin' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpStorageBinDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List Storage Bins' })
  findAll(@Query() query: QueryErpStorageBinDto) {
    return this.service.findAll(query);
  }

  @Get('tree/:warehouseId')
  @ApiOperation({ summary: 'All bins of one warehouse (flat, sorted by code)' })
  tree(@Param('warehouseId') warehouseId: string) {
    return this.service.tree(BigInt(warehouseId));
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get Storage Bin' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpStorageBinDto, @Request() req: any) {
    return this.service.bulkUpdateStatus(dto, req.user?.id);
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Storage Bin' })
  update(@Param('id') id: string, @Body() dto: UpdateErpStorageBinDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpStorageBinDto, @Request() req: any) {
    return this.service.bulkDelete(dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Storage Bin' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
