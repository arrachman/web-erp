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
import { CreateSlsDeliveryReportDto } from './dto/create-sls-delivery-report.dto';
import { QuerySlsDeliveryReportsDto } from './dto/query-sls-delivery-reports.dto';
import { TransitionSlsDeliveryReportDto } from './dto/transition-sls-delivery-report.dto';
import { UpdateSlsDeliveryReportDto } from './dto/update-sls-delivery-report.dto';
import { ErpSlsDeliveryReportsService } from './erp-sls-delivery-reports.service';

@ApiTags('ERP Sls Delivery Reports')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/sls/delivery-reports')
export class ErpSlsDeliveryReportsController {
  constructor(private readonly service: ErpSlsDeliveryReportsService) {}

  @Post()
  @ApiOperation({ summary: 'Create delivery report (header + item lines)' })
  create(@Body() dto: CreateSlsDeliveryReportDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List delivery reports (filter by status/date/customer/deliveryOrderId)' })
  findAll(@Query() query: QuerySlsDeliveryReportsDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get delivery report by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update delivery report (only DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdateSlsDeliveryReportDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow action: SUBMIT/APPROVE/REJECT/POST/REOPEN' })
  transition(@Param('id') id: string, @Body() dto: TransitionSlsDeliveryReportDto, @Request() req: any) {
    return this.service.transition(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete delivery report (soft)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
