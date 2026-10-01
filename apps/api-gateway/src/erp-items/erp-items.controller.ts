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
import { BulkErpItemDto, BulkStatusErpItemDto } from './dto/bulk-erp-item.dto';
import { CreateErpItemDto } from './dto/create-erp-item.dto';
import { QueryErpItemDto } from './dto/query-erp-item.dto';
import { UpdateErpItemDto } from './dto/update-erp-item.dto';
import { ErpItemsService } from './erp-items.service';

@ApiTags('ERP Items')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/items')
export class ErpItemsController {
  constructor(private readonly service: ErpItemsService) {}

  @Post()
  @ApiOperation({ summary: 'Create ERP item' })
  @ApiResponse({ status: 201, description: 'ERP item created' })
  create(@Body() dto: CreateErpItemDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'Get ERP item list' })
  @ApiResponse({ status: 200, description: 'List of ERP items' })
  findAll(@Query() query: QueryErpItemDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one ERP item' })
  @ApiResponse({ status: 200, description: 'ERP item detail' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update ERP item' })
  @ApiResponse({ status: 200, description: 'ERP item updated' })
  update(@Param('id') id: string, @Body() dto: UpdateErpItemDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate ERP items' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpItemDto, @Request() req: any) {
    return this.service.bulkUpdateStatus(dto, req.user?.id);
  }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete ERP items' })
  bulkDelete(@Body() dto: BulkErpItemDto, @Request() req: any) {
    return this.service.bulkDelete(dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete ERP item (soft delete)' })
  @ApiResponse({ status: 200, description: 'ERP item deleted' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
