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
import { CreateSlsReturnReceiptDto } from './dto/create-sls-return-receipt.dto';
import { QuerySlsReturnReceiptsDto } from './dto/query-sls-return-receipts.dto';
import { TransitionSlsReturnReceiptDto } from './dto/transition-sls-return-receipt.dto';
import { UpdateSlsReturnReceiptDto } from './dto/update-sls-return-receipt.dto';
import { ErpSlsReturnReceiptsService } from './erp-sls-return-receipts.service';

@ApiTags('ERP Sls Return Receipts')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/sls/return-receipts')
export class ErpSlsReturnReceiptsController {
  constructor(private readonly service: ErpSlsReturnReceiptsService) {}

  @Post()
  @ApiOperation({ summary: 'Create return receipt (header + item lines)' })
  create(@Body() dto: CreateSlsReturnReceiptDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List return receipts (filter by status/date/customer)' })
  findAll(@Query() query: QuerySlsReturnReceiptsDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get return receipt by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update return receipt (only DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdateSlsReturnReceiptDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow action: SUBMIT/APPROVE/REJECT/POST/REOPEN' })
  transition(@Param('id') id: string, @Body() dto: TransitionSlsReturnReceiptDto, @Request() req: any) {
    return this.service.transition(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete return receipt (soft)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
