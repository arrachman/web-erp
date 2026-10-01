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
import { CreatePurOrderDto } from './dto/create-pur-order.dto';
import { QueryPurOrdersDto } from './dto/query-pur-orders.dto';
import { TransitionPurOrderDto } from './dto/transition-pur-order.dto';
import { UpdatePurOrderDto } from './dto/update-pur-order.dto';
import { ErpPurOrdersService } from './erp-pur-orders.service';

@ApiTags('ERP Pur Orders')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/pur/orders')
export class ErpPurOrdersController {
  constructor(private readonly service: ErpPurOrdersService) {}

  @Post()
  @ApiOperation({ summary: 'Create purchase order (header + item lines)' })
  create(@Body() dto: CreatePurOrderDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List purchase orders (filter by status/date/supplier)' })
  findAll(@Query() query: QueryPurOrdersDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get purchase order by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update purchase order (only DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdatePurOrderDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow action: SUBMIT/APPROVE/REJECT/POST/REOPEN' })
  transition(@Param('id') id: string, @Body() dto: TransitionPurOrderDto, @Request() req: any) {
    return this.service.transition(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete purchase order (soft)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
