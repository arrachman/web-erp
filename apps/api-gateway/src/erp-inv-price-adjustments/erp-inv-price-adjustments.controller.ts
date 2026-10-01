import {
  Body,
  Controller,
  Delete,
  Get,
  Param,
  Post,
  Query,
  Request,
  UseGuards,
} from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { CreateInvPriceAdjustmentDto } from './dto/create-inv-price-adjustment.dto';
import { QueryInvPriceAdjustmentsDto } from './dto/query-inv-price-adjustments.dto';
import { ErpInvPriceAdjustmentsService } from './erp-inv-price-adjustments.service';

@ApiTags('ERP Inv Price Adjustments')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/inv/price-adjustments')
export class ErpInvPriceAdjustmentsController {
  constructor(private readonly service: ErpInvPriceAdjustmentsService) {}

  @Post()
  @ApiOperation({ summary: 'Trigger a new cost recalculation (creates PA record with status=PENDING)' })
  create(@Body() dto: CreateInvPriceAdjustmentDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Post(':id/process')
  @ApiOperation({ summary: 'Process a PA: derive deltas server-side, write lines, set COMPLETED/FAILED' })
  process(@Param('id') id: string, @Request() req: any) {
    return this.service.process(BigInt(id), req.user?.id ?? null);
  }

  @Get()
  @ApiOperation({ summary: 'List price adjustments (filter by status/item/warehouse/date)' })
  findAll(@Query() query: QueryInvPriceAdjustmentsDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get price adjustment by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete price adjustment (soft-delete; only PENDING or FAILED)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
