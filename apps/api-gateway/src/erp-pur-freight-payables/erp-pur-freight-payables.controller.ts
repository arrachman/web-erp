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
import { ErpPurFreightPayablesService } from './erp-pur-freight-payables.service';
import { CreateFreightPayableDto } from './dto/create-freight-payable.dto';
import { UpdateFreightPayableDto } from './dto/update-freight-payable.dto';
import { QueryFreightPayableDto } from './dto/query-freight-payable.dto';
import { TransitionFreightPayableDto } from './dto/transition-freight-payable.dto';

@ApiTags('ERP Purchasing — Freight Payables (PP)')
@ApiBearerAuth('erp-jwt')
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/pur/freight-payables')
export class ErpPurFreightPayablesController {
  constructor(private readonly service: ErpPurFreightPayablesService) {}

  @Post()
  @ApiOperation({ summary: 'Create Freight Payable (Hutang Biaya Pengiriman)' })
  create(@Body() dto: CreateFreightPayableDto, @Request() req: any) {
    return this.service.create(dto, req.user?.sub ?? req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List Freight Payables with filters and pagination' })
  findAll(@Query() query: QueryFreightPayableDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get single Freight Payable by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Freight Payable (only when DRAFT or REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdateFreightPayableDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.sub ?? req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Freight Payable (not allowed when POSTED)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.sub ?? req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow transition: SUBMIT / APPROVE / REJECT / POST / REOPEN' })
  transition(
    @Param('id') id: string,
    @Body() dto: TransitionFreightPayableDto,
    @Request() req: any,
  ) {
    return this.service.transition(BigInt(id), dto, req.user?.sub ?? req.user?.id);
  }
}
