import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { CreatePurBidSelectionDto } from './dto/create-pur-bid-selection.dto';
import { QueryPurBidSelectionsDto } from './dto/query-pur-bid-selections.dto';
import { TransitionPurBidSelectionDto } from './dto/transition-pur-bid-selection.dto';
import { UpdatePurBidSelectionDto } from './dto/update-pur-bid-selection.dto';
import { ErpPurBidSelectionsService } from './erp-pur-bid-selections.service';

@ApiTags('ERP Pur Bid Selections')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/pur/bid-selections')
export class ErpPurBidSelectionsController {
  constructor(private readonly service: ErpPurBidSelectionsService) {}

  @Post() @ApiOperation({ summary: 'Create bid comparison (rank quotation lines)' })
  create(@Body() dto: CreatePurBidSelectionDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get() @ApiOperation({ summary: 'List bid selections' })
  findAll(@Query() query: QueryPurBidSelectionsDto) { return this.service.findAll(query); }

  @Get(':id') @ApiOperation({ summary: 'Get bid selection by id' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id') @ApiOperation({ summary: 'Update bid selection (DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdatePurBidSelectionDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Post(':id/transition') @ApiOperation({ summary: 'Workflow action' })
  transition(@Param('id') id: string, @Body() dto: TransitionPurBidSelectionDto, @Request() req: any) { return this.service.transition(BigInt(id), dto, req.user?.id); }

  @Delete(':id') @ApiOperation({ summary: 'Delete bid selection (soft)' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
