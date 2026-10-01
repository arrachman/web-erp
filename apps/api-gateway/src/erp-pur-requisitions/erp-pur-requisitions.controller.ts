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
import { CreatePurRequisitionDto } from './dto/create-pur-requisition.dto';
import { QueryPurRequisitionsDto } from './dto/query-pur-requisitions.dto';
import { TransitionPurRequisitionDto } from './dto/transition-pur-requisition.dto';
import { UpdatePurRequisitionDto } from './dto/update-pur-requisition.dto';
import { ErpPurRequisitionsService } from './erp-pur-requisitions.service';

@ApiTags('ERP Pur Requisitions')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/pur/requisitions')
export class ErpPurRequisitionsController {
  constructor(private readonly service: ErpPurRequisitionsService) {}

  @Post()
  @ApiOperation({ summary: 'Create purchase requisition (header + item lines)' })
  create(@Body() dto: CreatePurRequisitionDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List purchase requisitions (filter by status/date/supplier)' })
  findAll(@Query() query: QueryPurRequisitionsDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get purchase requisition by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update purchase requisition (only DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdatePurRequisitionDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow action: SUBMIT/APPROVE/REJECT/POST/REOPEN' })
  transition(@Param('id') id: string, @Body() dto: TransitionPurRequisitionDto, @Request() req: any) {
    return this.service.transition(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete purchase requisition (soft)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
