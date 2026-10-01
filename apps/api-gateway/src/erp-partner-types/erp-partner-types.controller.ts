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
import { BulkErpPartnerTypeDto, BulkStatusErpPartnerTypeDto } from './dto/bulk-erp-partner-type.dto';
import { CreateErpPartnerTypeDto } from './dto/create-erp-partner-type.dto';
import { QueryErpPartnerTypeDto } from './dto/query-erp-partner-type.dto';
import { UpdateErpPartnerTypeDto } from './dto/update-erp-partner-type.dto';
import { ErpPartnerTypesService } from './erp-partner-types.service';

@ApiTags('ERP Partner Types')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/partner-types')
export class ErpPartnerTypesController {
  constructor(private readonly service: ErpPartnerTypesService) {}

  @Post()
  @ApiOperation({ summary: 'Create ERP partner type' })
  @ApiResponse({ status: 201, description: 'ERP partner type created' })
  create(@Body() dto: CreateErpPartnerTypeDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'Get ERP partner type list' })
  @ApiResponse({ status: 200, description: 'List of ERP partner types' })
  findAll(@Query() query: QueryErpPartnerTypeDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one ERP partner type' })
  @ApiResponse({ status: 200, description: 'ERP partner type detail' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update ERP partner type' })
  @ApiResponse({ status: 200, description: 'ERP partner type updated' })
  update(
    @Param('id') id: string,
    @Body() dto: UpdateErpPartnerTypeDto,
    @Request() req: any,
  ) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate ERP partner types' })
  @ApiResponse({ status: 200, description: 'Bulk status updated' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpPartnerTypeDto, @Request() req: any) {
    return this.service.bulkUpdateStatus(dto, req.user?.id);
  }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete ERP partner types' })
  @ApiResponse({ status: 200, description: 'Bulk deleted' })
  bulkDelete(@Body() dto: BulkErpPartnerTypeDto, @Request() req: any) {
    return this.service.bulkDelete(dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete ERP partner type (soft delete)' })
  @ApiResponse({ status: 200, description: 'ERP partner type deleted' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
