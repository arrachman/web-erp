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
import { CreateInvStockMovementDto } from './dto/create-inv-stock-movement.dto';
import { QueryInvStockMovementsDto } from './dto/query-inv-stock-movements.dto';
import { TransitionInvStockMovementDto } from './dto/transition-inv-stock-movement.dto';
import { UpdateInvStockMovementDto } from './dto/update-inv-stock-movement.dto';
import { ErpInvStockMovementsService } from './erp-inv-stock-movements.service';

@ApiTags('ERP Inv Stock Movements')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/inv/stock-movements')
export class ErpInvStockMovementsController {
  constructor(private readonly service: ErpInvStockMovementsService) {}

  @Post()
  @ApiOperation({ summary: 'Create stock movement (header + item lines) — MR/TS/RS/RF' })
  create(@Body() dto: CreateInvStockMovementDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List stock movements (filter by type/status/date/warehouse)' })
  findAll(@Query() query: QueryInvStockMovementsDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get stock movement by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update stock movement (only DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdateInvStockMovementDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow action: SUBMIT/APPROVE/REJECT/POST/REOPEN' })
  transition(@Param('id') id: string, @Body() dto: TransitionInvStockMovementDto, @Request() req: any) {
    return this.service.transition(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete stock movement (soft)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
