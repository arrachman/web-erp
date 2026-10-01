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
import { CreateMfgBomDto } from './dto/create-mfg-bom.dto';
import { QueryMfgBomsDto } from './dto/query-mfg-boms.dto';
import { TransitionMfgBomDto } from './dto/transition-mfg-bom.dto';
import { UpdateMfgBomDto } from './dto/update-mfg-bom.dto';
import { ErpMfgBomsService } from './erp-mfg-boms.service';

@ApiTags('ERP Mfg BOMs')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/mfg/boms')
export class ErpMfgBomsController {
  constructor(private readonly service: ErpMfgBomsService) {}

  @Post()
  @ApiOperation({ summary: 'Create Manufacturing BOM (header + input/output lines)' })
  create(@Body() dto: CreateMfgBomDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List BOMs (filter by status/date/branch)' })
  findAll(@Query() query: QueryMfgBomsDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get BOM by id (with input/output lines)' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update BOM (only DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdateMfgBomDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow action: SUBMIT/APPROVE/REJECT/REOPEN' })
  transition(@Param('id') id: string, @Body() dto: TransitionMfgBomDto, @Request() req: any) {
    return this.service.transition(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete BOM (soft delete; not allowed when APPROVED)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
