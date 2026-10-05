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
import { CreateSlsDeliveryOrderDto } from './dto/create-sls-delivery-order.dto';
import { QuerySlsDeliveryOrdersDto } from './dto/query-sls-delivery-orders.dto';
import { TransitionSlsDeliveryOrderDto } from './dto/transition-sls-delivery-order.dto';
import { UpdateSlsDeliveryOrderDto } from './dto/update-sls-delivery-order.dto';
import { ErpSlsDeliveryOrdersService } from './erp-sls-delivery-orders.service';
import { ErpOutboundNotificationsService } from '../erp-outbound-notifications/erp-outbound-notifications.service';

@ApiTags('ERP Sls Delivery Orders')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/sls/delivery-orders')
export class ErpSlsDeliveryOrdersController {
  constructor(
    private readonly service: ErpSlsDeliveryOrdersService,
    private readonly notifications: ErpOutboundNotificationsService,
  ) {}

  @Post()
  @ApiOperation({ summary: 'Create delivery order (header + item lines)' })
  create(@Body() dto: CreateSlsDeliveryOrderDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List delivery orders (filter by status/date/customer)' })
  findAll(@Query() query: QuerySlsDeliveryOrdersDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get delivery order by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update delivery order (only DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdateSlsDeliveryOrderDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow action: SUBMIT/APPROVE/REJECT/POST/REOPEN' })
  async transition(@Param('id') id: string, @Body() dto: TransitionSlsDeliveryOrderDto, @Request() req: any) {
    const result = await this.service.transition(BigInt(id), dto, req.user?.id);
    // W6: barang dikirim → notifikasi portal (tidak pernah menggagalkan request).
    if (String(dto.action) === 'POST') void this.notifications.notifyDeliveryPosted(BigInt(id));
    return result;
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete delivery order (soft)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
