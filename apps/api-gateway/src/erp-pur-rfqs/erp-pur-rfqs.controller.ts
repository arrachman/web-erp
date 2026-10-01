import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { CreatePurRfqDto } from './dto/create-pur-rfq.dto';
import { QueryPurRfqsDto } from './dto/query-pur-rfqs.dto';
import { TransitionPurRfqDto } from './dto/transition-pur-rfq.dto';
import { UpdatePurRfqDto } from './dto/update-pur-rfq.dto';
import { ErpPurRfqsService } from './erp-pur-rfqs.service';

@ApiTags('ERP Pur RFQs')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/pur/rfqs')
export class ErpPurRfqsController {
  constructor(private readonly service: ErpPurRfqsService) {}

  @Post() @ApiOperation({ summary: 'Create RFQ (invite suppliers)' })
  create(@Body() dto: CreatePurRfqDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get() @ApiOperation({ summary: 'List RFQs' })
  findAll(@Query() query: QueryPurRfqsDto) { return this.service.findAll(query); }

  @Get(':id') @ApiOperation({ summary: 'Get RFQ by id' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id') @ApiOperation({ summary: 'Update RFQ (DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdatePurRfqDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Post(':id/transition') @ApiOperation({ summary: 'Workflow action' })
  transition(@Param('id') id: string, @Body() dto: TransitionPurRfqDto, @Request() req: any) { return this.service.transition(BigInt(id), dto, req.user?.id); }

  @Delete(':id') @ApiOperation({ summary: 'Delete RFQ (soft)' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
