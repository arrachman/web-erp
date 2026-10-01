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
import { CreateSlsCustomerAdvanceDto } from './dto/create-sls-customer-advance.dto';
import { QuerySlsCustomerAdvancesDto } from './dto/query-sls-customer-advances.dto';
import { TransitionSlsCustomerAdvanceDto } from './dto/transition-sls-customer-advance.dto';
import { UpdateSlsCustomerAdvanceDto } from './dto/update-sls-customer-advance.dto';
import { ErpSlsCustomerAdvancesService } from './erp-sls-customer-advances.service';

@ApiTags('ERP Sls Customer Advances')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/sls/customer-advances')
export class ErpSlsCustomerAdvancesController {
  constructor(private readonly service: ErpSlsCustomerAdvancesService) {}

  @Post()
  @ApiOperation({ summary: 'Create customer advance (uang muka pelanggan)' })
  create(@Body() dto: CreateSlsCustomerAdvanceDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List customer advances (filter by status/date/customer/settlementStatus)' })
  findAll(@Query() query: QuerySlsCustomerAdvancesDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get customer advance by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update customer advance (only DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdateSlsCustomerAdvanceDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow action: SUBMIT/APPROVE/REJECT/POST/REOPEN' })
  transition(
    @Param('id') id: string,
    @Body() dto: TransitionSlsCustomerAdvanceDto,
    @Request() req: any,
  ) {
    return this.service.transition(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete customer advance (soft)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
