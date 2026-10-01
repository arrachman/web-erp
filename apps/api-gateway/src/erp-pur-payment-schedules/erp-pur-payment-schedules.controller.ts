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
import { ErpPurPaymentSchedulesService } from './erp-pur-payment-schedules.service';
import { CreatePaymentScheduleDto } from './dto/create-payment-schedule.dto';
import { UpdatePaymentScheduleDto } from './dto/update-payment-schedule.dto';
import { QueryPaymentScheduleDto } from './dto/query-payment-schedule.dto';
import { TransitionPaymentScheduleDto } from './dto/transition-payment-schedule.dto';

@ApiTags('ERP Purchasing — Payment Schedules (VPP)')
@ApiBearerAuth('erp-jwt')
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/pur/payment-schedules')
export class ErpPurPaymentSchedulesController {
  constructor(private readonly service: ErpPurPaymentSchedulesService) {}

  @Post()
  @ApiOperation({ summary: 'Create Vendor Payment Plan (Jadwal Pembayaran Vendor)' })
  create(@Body() dto: CreatePaymentScheduleDto, @Request() req: any) {
    return this.service.create(dto, req.user?.sub ?? req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List Payment Schedules (VPP) with filters and pagination' })
  findAll(@Query() query: QueryPaymentScheduleDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get single Payment Schedule by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Payment Schedule (only when DRAFT or REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdatePaymentScheduleDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.sub ?? req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Payment Schedule (not allowed when POSTED)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.sub ?? req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow transition: SUBMIT / APPROVE / REJECT / POST / REOPEN' })
  transition(
    @Param('id') id: string,
    @Body() dto: TransitionPaymentScheduleDto,
    @Request() req: any,
  ) {
    return this.service.transition(BigInt(id), dto, req.user?.sub ?? req.user?.id);
  }
}
