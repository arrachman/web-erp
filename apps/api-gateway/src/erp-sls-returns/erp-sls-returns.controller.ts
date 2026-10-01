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
import { CreateSlsReturnDto } from './dto/create-sls-return.dto';
import { QuerySlsReturnsDto } from './dto/query-sls-returns.dto';
import { TransitionSlsReturnDto } from './dto/transition-sls-return.dto';
import { UpdateSlsReturnDto } from './dto/update-sls-return.dto';
import { ErpSlsReturnsService } from './erp-sls-returns.service';

@ApiTags('ERP Sls Returns')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/sls/returns')
export class ErpSlsReturnsController {
  constructor(private readonly service: ErpSlsReturnsService) {}

  @Post()
  @ApiOperation({ summary: 'Create sales return (header + item lines)' })
  create(@Body() dto: CreateSlsReturnDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List sales returns (filter by status/date/customer)' })
  findAll(@Query() query: QuerySlsReturnsDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get sales return by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update sales return (only DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdateSlsReturnDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow action: SUBMIT/APPROVE/REJECT/POST/REOPEN' })
  transition(@Param('id') id: string, @Body() dto: TransitionSlsReturnDto, @Request() req: any) {
    return this.service.transition(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete sales return (soft)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
