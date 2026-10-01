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
import { ApiBearerAuth, ApiOperation, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { CreateInvOpeningStockDto } from './dto/create-inv-opening-stock.dto';
import { QueryInvOpeningStocksDto } from './dto/query-inv-opening-stocks.dto';
import { TransitionInvOpeningStockDto } from './dto/transition-inv-opening-stock.dto';
import { UpdateInvOpeningStockDto } from './dto/update-inv-opening-stock.dto';
import { ErpInvOpeningStocksService } from './erp-inv-opening-stocks.service';

@ApiTags('ERP Inv Opening Stocks')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/inv/opening-stocks')
export class ErpInvOpeningStocksController {
  constructor(private readonly service: ErpInvOpeningStocksService) {}

  @Post()
  @ApiOperation({ summary: 'Create opening stock (header + item lines) — IB' })
  create(@Body() dto: CreateInvOpeningStockDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List opening stocks (filter by status/date/warehouse)' })
  findAll(@Query() query: QueryInvOpeningStocksDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get opening stock by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update opening stock (only DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdateInvOpeningStockDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow action: SUBMIT/APPROVE/REJECT/POST/REOPEN' })
  transition(@Param('id') id: string, @Body() dto: TransitionInvOpeningStockDto, @Request() req: any) {
    return this.service.transition(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete opening stock (soft)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
