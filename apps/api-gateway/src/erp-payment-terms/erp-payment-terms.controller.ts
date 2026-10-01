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
import { BulkErpPaymentTermDto, BulkStatusErpPaymentTermDto } from './dto/bulk-erp-payment-term.dto';
import { CreateErpPaymentTermDto } from './dto/create-erp-payment-term.dto';
import { QueryErpPaymentTermDto } from './dto/query-erp-payment-term.dto';
import { UpdateErpPaymentTermDto } from './dto/update-erp-payment-term.dto';
import { ErpPaymentTermsService } from './erp-payment-terms.service';

@ApiTags('ERP Payment Terms')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/payment-terms')
export class ErpPaymentTermsController {
  constructor(private readonly service: ErpPaymentTermsService) {}

  @Post()
  @ApiOperation({ summary: 'Create ERP payment term' })
  @ApiResponse({ status: 201, description: 'Payment term created' })
  create(@Body() dto: CreateErpPaymentTermDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List ERP payment terms' })
  @ApiResponse({ status: 200, description: 'List of payment terms' })
  findAll(@Query() query: QueryErpPaymentTermDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one ERP payment term' })
  @ApiResponse({ status: 200, description: 'Payment term detail' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update ERP payment term' })
  @ApiResponse({ status: 200, description: 'Payment term updated' })
  update(@Param('id') id: string, @Body() dto: UpdateErpPaymentTermDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate ERP payment terms' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpPaymentTermDto, @Request() req: any) {
    return this.service.bulkUpdateStatus(dto, req.user?.id);
  }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete ERP payment terms' })
  bulkDelete(@Body() dto: BulkErpPaymentTermDto, @Request() req: any) {
    return this.service.bulkDelete(dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft delete ERP payment term' })
  @ApiResponse({ status: 200, description: 'Payment term deleted' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
