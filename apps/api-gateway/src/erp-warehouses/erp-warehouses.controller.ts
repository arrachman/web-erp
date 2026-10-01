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
import { CreateErpWarehouseDto } from './dto/create-erp-warehouse.dto';
import { QueryErpWarehouseDto } from './dto/query-erp-warehouse.dto';
import { UpdateErpWarehouseDto } from './dto/update-erp-warehouse.dto';
import { ErpWarehousesService } from './erp-warehouses.service';

@ApiTags('ERP Warehouses')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/warehouses')
export class ErpWarehousesController {
  constructor(private readonly service: ErpWarehousesService) {}

  @Post()
  @ApiOperation({ summary: 'Create ERP warehouse' })
  @ApiResponse({ status: 201, description: 'ERP warehouse created' })
  create(@Body() dto: CreateErpWarehouseDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'Get ERP warehouse list' })
  @ApiResponse({ status: 200, description: 'List of ERP warehouses' })
  findAll(@Query() query: QueryErpWarehouseDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one ERP warehouse' })
  @ApiResponse({ status: 200, description: 'ERP warehouse detail' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update ERP warehouse' })
  @ApiResponse({ status: 200, description: 'ERP warehouse updated' })
  update(@Param('id') id: string, @Body() dto: UpdateErpWarehouseDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete ERP warehouse (soft delete)' })
  @ApiResponse({ status: 200, description: 'ERP warehouse deleted' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
