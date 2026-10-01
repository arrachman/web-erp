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
import { CreatePurReturnDto } from './dto/create-pur-return.dto';
import { QueryPurReturnsDto } from './dto/query-pur-returns.dto';
import { TransitionPurReturnDto } from './dto/transition-pur-return.dto';
import { UpdatePurReturnDto } from './dto/update-pur-return.dto';
import { ErpPurReturnsService } from './erp-pur-returns.service';

@ApiTags('ERP Pur Returns')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/pur/returns')
export class ErpPurReturnsController {
  constructor(private readonly service: ErpPurReturnsService) {}

  @Post()
  @ApiOperation({ summary: 'Create purchase return / debit note (returnType: DEBIT_NOTE|RETURN_TO_VENDOR)' })
  create(@Body() dto: CreatePurReturnDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List purchase returns (filter by returnType/status/date/supplier)' })
  findAll(@Query() query: QueryPurReturnsDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get purchase return by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update purchase return (only DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdatePurReturnDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow action: SUBMIT/APPROVE/REJECT/POST/REOPEN' })
  transition(@Param('id') id: string, @Body() dto: TransitionPurReturnDto, @Request() req: any) {
    return this.service.transition(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete purchase return (soft)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
