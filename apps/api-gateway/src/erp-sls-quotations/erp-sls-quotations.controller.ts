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
import { CreateSlsQuotationDto } from './dto/create-sls-quotation.dto';
import { QuerySlsQuotationsDto } from './dto/query-sls-quotations.dto';
import { TransitionSlsQuotationDto } from './dto/transition-sls-quotation.dto';
import { UpdateSlsQuotationDto } from './dto/update-sls-quotation.dto';
import { ErpSlsQuotationsService } from './erp-sls-quotations.service';

@ApiTags('ERP Sls Quotations')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/sls/quotations')
export class ErpSlsQuotationsController {
  constructor(private readonly service: ErpSlsQuotationsService) {}

  @Post()
  @ApiOperation({ summary: 'Create sales quotation (header + item lines)' })
  create(@Body() dto: CreateSlsQuotationDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List sales quotations (filter by status/date/customer)' })
  findAll(@Query() query: QuerySlsQuotationsDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get sales quotation by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update sales quotation (only DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdateSlsQuotationDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow action: SUBMIT/APPROVE/REJECT/POST/REOPEN' })
  transition(@Param('id') id: string, @Body() dto: TransitionSlsQuotationDto, @Request() req: any) {
    return this.service.transition(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete sales quotation (soft)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
