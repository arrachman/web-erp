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
import { CreateApPaymentDto } from './dto/create-ap-payment.dto';
import { QueryApPaymentDto } from './dto/query-ap-payment.dto';
import { UpdateApPaymentDto } from './dto/update-ap-payment.dto';
import { ErpFinApPaymentsService } from './erp-fin-ap-payments.service';

@ApiTags('ERP Fin AP Payments')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/fin/ap-payments')
export class ErpFinApPaymentsController {
  constructor(private readonly service: ErpFinApPaymentsService) {}

  @Post()
  @ApiOperation({ summary: 'Create AP payment' })
  create(@Body() dto: CreateApPaymentDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List AP payments' })
  findAll(@Query() query: QueryApPaymentDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get AP payment' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update AP payment' })
  update(
    @Param('id') id: string,
    @Body() dto: UpdateApPaymentDto,
    @Request() req: any,
  ) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete AP payment' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
