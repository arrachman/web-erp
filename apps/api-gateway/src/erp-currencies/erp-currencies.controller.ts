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
import { BulkErpCurrencyDto, BulkStatusErpCurrencyDto } from './dto/bulk-erp-currency.dto';
import { CreateErpCurrencyDto } from './dto/create-erp-currency.dto';
import { QueryErpCurrencyDto } from './dto/query-erp-currency.dto';
import { UpdateErpCurrencyDto } from './dto/update-erp-currency.dto';
import { CreateErpCurrencyRateDto } from './dto/create-erp-currency-rate.dto';
import { ErpCurrenciesService } from './erp-currencies.service';

@ApiTags('ERP Currencies')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/currencies')
export class ErpCurrenciesController {
  constructor(private readonly service: ErpCurrenciesService) {}

  @Post()
  @ApiOperation({ summary: 'Create ERP currency' })
  @ApiResponse({ status: 201, description: 'Currency created' })
  create(@Body() dto: CreateErpCurrencyDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List ERP currencies' })
  @ApiResponse({ status: 200, description: 'List of currencies' })
  findAll(@Query() query: QueryErpCurrencyDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one ERP currency' })
  @ApiResponse({ status: 200, description: 'Currency detail' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update ERP currency' })
  @ApiResponse({ status: 200, description: 'Currency updated' })
  update(@Param('id') id: string, @Body() dto: UpdateErpCurrencyDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate ERP currencies' })
  @ApiResponse({ status: 200, description: 'Bulk status updated' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpCurrencyDto, @Request() req: any) {
    return this.service.bulkUpdateStatus(dto, req.user?.id);
  }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete ERP currencies' })
  @ApiResponse({ status: 200, description: 'Bulk deleted' })
  bulkDelete(@Body() dto: BulkErpCurrencyDto, @Request() req: any) {
    return this.service.bulkDelete(dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft delete ERP currency' })
  @ApiResponse({ status: 200, description: 'Currency deleted' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }

  @Post(':id/rates')
  @ApiOperation({ summary: 'Add or upsert currency rate for a specific date' })
  @ApiResponse({ status: 201, description: 'Currency rate added' })
  addRate(
    @Param('id') id: string,
    @Body() dto: CreateErpCurrencyRateDto,
    @Request() req: any,
  ) {
    return this.service.addRate(BigInt(id), dto, req.user?.id);
  }

  @Get(':id/rates')
  @ApiOperation({ summary: 'Get currency rates' })
  @ApiResponse({ status: 200, description: 'Currency rate list' })
  getRates(
    @Param('id') id: string,
    @Query('page') page?: string,
    @Query('limit') limit?: string,
  ) {
    return this.service.getRates(BigInt(id), {
      page: page ? parseInt(page, 10) : undefined,
      limit: limit ? parseInt(limit, 10) : undefined,
    });
  }
}
