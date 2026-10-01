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
import { ErpSlsFreightReceivablesService } from './erp-sls-freight-receivables.service';
import { CreateFreightReceivableDto } from './dto/create-freight-receivable.dto';
import { UpdateFreightReceivableDto } from './dto/update-freight-receivable.dto';
import { QueryFreightReceivableDto } from './dto/query-freight-receivable.dto';
import { TransitionFreightReceivableDto } from './dto/transition-freight-receivable.dto';

@ApiTags('ERP Sales — Freight Receivables (RP)')
@ApiBearerAuth('erp-jwt')
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/sls/freight-receivables')
export class ErpSlsFreightReceivablesController {
  constructor(private readonly service: ErpSlsFreightReceivablesService) {}

  @Post()
  @ApiOperation({ summary: 'Create Freight Receivable (Tagihan Ongkos Kirim)' })
  create(@Body() dto: CreateFreightReceivableDto, @Request() req: any) {
    return this.service.create(dto, req.user?.sub ?? req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List Freight Receivables with filters and pagination' })
  findAll(@Query() query: QueryFreightReceivableDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get single Freight Receivable by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Freight Receivable (only when DRAFT or REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdateFreightReceivableDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.sub ?? req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Freight Receivable (not allowed when POSTED)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.sub ?? req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow transition: SUBMIT / APPROVE / REJECT / POST / REOPEN' })
  transition(
    @Param('id') id: string,
    @Body() dto: TransitionFreightReceivableDto,
    @Request() req: any,
  ) {
    return this.service.transition(BigInt(id), dto, req.user?.sub ?? req.user?.id);
  }
}
