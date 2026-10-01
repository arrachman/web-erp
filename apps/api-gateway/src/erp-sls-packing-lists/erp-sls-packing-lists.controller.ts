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
import { CreateSlsPackingListDto } from './dto/create-sls-packing-list.dto';
import { QuerySlsPackingListsDto } from './dto/query-sls-packing-lists.dto';
import { TransitionSlsPackingListDto } from './dto/transition-sls-packing-list.dto';
import { UpdateSlsPackingListDto } from './dto/update-sls-packing-list.dto';
import { ErpSlsPackingListsService } from './erp-sls-packing-lists.service';

@ApiTags('ERP Sls Packing Lists')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/sls/packing-lists')
export class ErpSlsPackingListsController {
  constructor(private readonly service: ErpSlsPackingListsService) {}

  @Post()
  @ApiOperation({ summary: 'Create packing list (header + item lines)' })
  create(@Body() dto: CreateSlsPackingListDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List packing lists (filter by status/date/customer)' })
  findAll(@Query() query: QuerySlsPackingListsDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get packing list by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update packing list (only DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdateSlsPackingListDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow action: SUBMIT/APPROVE/REJECT/POST/REOPEN' })
  transition(@Param('id') id: string, @Body() dto: TransitionSlsPackingListDto, @Request() req: any) {
    return this.service.transition(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete packing list (soft)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
