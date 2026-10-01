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
import { CreateInvStockCountDto } from './dto/create-inv-stock-count.dto';
import { QueryInvStockCountsDto } from './dto/query-inv-stock-counts.dto';
import { TransitionInvStockCountDto } from './dto/transition-inv-stock-count.dto';
import { UpdateInvStockCountDto } from './dto/update-inv-stock-count.dto';
import { ErpInvStockCountsService } from './erp-inv-stock-counts.service';

@ApiTags('ERP Inv Stock Counts')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/inv/stock-counts')
export class ErpInvStockCountsController {
  constructor(private readonly service: ErpInvStockCountsService) {}

  @Post()
  @ApiOperation({ summary: 'Create stock count / opname (header + item lines)' })
  create(@Body() dto: CreateInvStockCountDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List stock counts (filter by type/status/date/warehouse)' })
  findAll(@Query() query: QueryInvStockCountsDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get stock count by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update stock count (only DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdateInvStockCountDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow action: SUBMIT/APPROVE/REJECT/POST/REOPEN' })
  transition(@Param('id') id: string, @Body() dto: TransitionInvStockCountDto, @Request() req: any) {
    return this.service.transition(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete stock count (soft)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
