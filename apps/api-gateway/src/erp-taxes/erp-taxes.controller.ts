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
import { BulkErpTaxDto, BulkStatusErpTaxDto } from './dto/bulk-erp-tax.dto';
import { CreateErpTaxDto } from './dto/create-erp-tax.dto';
import { QueryErpTaxDto } from './dto/query-erp-tax.dto';
import { UpdateErpTaxDto } from './dto/update-erp-tax.dto';
import { ErpTaxesService } from './erp-taxes.service';

@ApiTags('ERP Taxes')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/taxes')
export class ErpTaxesController {
  constructor(private readonly service: ErpTaxesService) {}

  @Post()
  @ApiOperation({ summary: 'Create ERP tax' })
  @ApiResponse({ status: 201, description: 'Tax created' })
  create(@Body() dto: CreateErpTaxDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List ERP taxes' })
  @ApiResponse({ status: 200, description: 'List of taxes' })
  findAll(@Query() query: QueryErpTaxDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one ERP tax' })
  @ApiResponse({ status: 200, description: 'Tax detail' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update ERP tax' })
  @ApiResponse({ status: 200, description: 'Tax updated' })
  update(@Param('id') id: string, @Body() dto: UpdateErpTaxDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate ERP taxes' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpTaxDto, @Request() req: any) {
    return this.service.bulkUpdateStatus(dto, req.user?.id);
  }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete ERP taxes' })
  bulkDelete(@Body() dto: BulkErpTaxDto, @Request() req: any) {
    return this.service.bulkDelete(dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft delete ERP tax' })
  @ApiResponse({ status: 200, description: 'Tax deleted' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
