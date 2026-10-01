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
import { BulkErpUnitDto, BulkStatusErpUnitDto } from './dto/bulk-erp-unit.dto';
import { CreateErpUnitDto } from './dto/create-erp-unit.dto';
import { QueryErpUnitDto } from './dto/query-erp-unit.dto';
import { UpdateErpUnitDto } from './dto/update-erp-unit.dto';
import { ErpUnitsService } from './erp-units.service';

@ApiTags('ERP Units')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/units')
export class ErpUnitsController {
  constructor(private readonly service: ErpUnitsService) {}

  @Post()
  @ApiOperation({ summary: 'Create ERP unit' })
  @ApiResponse({ status: 201, description: 'ERP unit created' })
  create(@Body() dto: CreateErpUnitDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'Get ERP unit list' })
  @ApiResponse({ status: 200, description: 'List of ERP units' })
  findAll(@Query() query: QueryErpUnitDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one ERP unit' })
  @ApiResponse({ status: 200, description: 'ERP unit detail' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update ERP unit' })
  @ApiResponse({ status: 200, description: 'ERP unit updated' })
  update(@Param('id') id: string, @Body() dto: UpdateErpUnitDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate ERP units' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpUnitDto, @Request() req: any) {
    return this.service.bulkUpdateStatus(dto, req.user?.id);
  }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete ERP units' })
  bulkDelete(@Body() dto: BulkErpUnitDto, @Request() req: any) {
    return this.service.bulkDelete(dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete ERP unit (soft delete)' })
  @ApiResponse({ status: 200, description: 'ERP unit deleted' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
