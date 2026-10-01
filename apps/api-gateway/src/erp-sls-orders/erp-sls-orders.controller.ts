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
import { CreateSlsOrderDto } from './dto/create-sls-order.dto';
import { QuerySlsOrdersDto } from './dto/query-sls-orders.dto';
import { TransitionSlsOrderDto } from './dto/transition-sls-order.dto';
import { UpdateSlsOrderDto } from './dto/update-sls-order.dto';
import { ErpSlsOrdersService } from './erp-sls-orders.service';

@ApiTags('ERP Sls Orders')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/sls/orders')
export class ErpSlsOrdersController {
  constructor(private readonly service: ErpSlsOrdersService) {}

  @Post()
  @ApiOperation({ summary: 'Create sales order (header + item lines)' })
  create(@Body() dto: CreateSlsOrderDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List sales orders (filter by status/date/customer)' })
  findAll(@Query() query: QuerySlsOrdersDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get sales order by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update sales order (only DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdateSlsOrderDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow action: SUBMIT/APPROVE/REJECT/POST/REOPEN' })
  transition(@Param('id') id: string, @Body() dto: TransitionSlsOrderDto, @Request() req: any) {
    return this.service.transition(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete sales order (soft)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
