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
import { BulkErpLocationDto, BulkStatusErpLocationDto } from './dto/bulk-erp-location.dto';
import { CreateErpLocationDto } from './dto/create-erp-location.dto';
import { QueryErpLocationDto } from './dto/query-erp-location.dto';
import { UpdateErpLocationDto } from './dto/update-erp-location.dto';
import { ErpLocationsService } from './erp-locations.service';

@ApiTags('ERP Locations')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/locations')
export class ErpLocationsController {
  constructor(private readonly service: ErpLocationsService) {}

  @Post()
  @ApiOperation({ summary: 'Create ERP location' })
  @ApiResponse({ status: 201, description: 'ERP location created' })
  create(@Body() dto: CreateErpLocationDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'Get ERP location list' })
  @ApiResponse({ status: 200, description: 'List of ERP locations' })
  findAll(@Query() query: QueryErpLocationDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one ERP location' })
  @ApiResponse({ status: 200, description: 'ERP location detail' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update ERP location' })
  @ApiResponse({ status: 200, description: 'ERP location updated' })
  update(@Param('id') id: string, @Body() dto: UpdateErpLocationDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete ERP location (soft delete)' })
  @ApiResponse({ status: 200, description: 'ERP location deleted' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk update ERP location status' })
  @ApiResponse({ status: 200, description: 'Bulk status updated' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpLocationDto, @Request() req: any) {
    return this.service.bulkUpdateStatus(dto, req.user?.id);
  }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk delete ERP locations (soft delete)' })
  @ApiResponse({ status: 200, description: 'Bulk deleted' })
  bulkDelete(@Body() dto: BulkErpLocationDto, @Request() req: any) {
    return this.service.bulkDelete(dto, req.user?.id);
  }
}
