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
import { CreateInvStockAdjustmentDto } from './dto/create-inv-stock-adjustment.dto';
import { QueryInvStockAdjustmentsDto } from './dto/query-inv-stock-adjustments.dto';
import { TransitionInvStockAdjustmentDto } from './dto/transition-inv-stock-adjustment.dto';
import { UpdateInvStockAdjustmentDto } from './dto/update-inv-stock-adjustment.dto';
import { ErpInvStockAdjustmentsService } from './erp-inv-stock-adjustments.service';

@ApiTags('ERP Inv Stock Adjustments')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/inv/stock-adjustments')
export class ErpInvStockAdjustmentsController {
  constructor(private readonly service: ErpInvStockAdjustmentsService) {}

  @Post()
  @ApiOperation({ summary: 'Create stock adjustment (header + item lines)' })
  create(@Body() dto: CreateInvStockAdjustmentDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List stock adjustments (filter by status/date/warehouse)' })
  findAll(@Query() query: QueryInvStockAdjustmentsDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get stock adjustment by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update stock adjustment (only DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdateInvStockAdjustmentDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow action: SUBMIT/APPROVE/REJECT/POST/REOPEN' })
  transition(@Param('id') id: string, @Body() dto: TransitionInvStockAdjustmentDto, @Request() req: any) {
    return this.service.transition(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete stock adjustment (soft)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
