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
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { CreateErpBankAccountDto } from './dto/create-erp-bank-account.dto';
import { UpdateErpBankAccountDto } from './dto/update-erp-bank-account.dto';
import {
  BulkErpBankAccountDto,
  BulkStatusErpBankAccountDto,
  QueryErpBankAccountDto,
} from './dto/query-erp-bank-account.dto';
import { ErpBankAccountsService } from './erp-bank-accounts.service';

@ApiTags('ERP Bank Accounts')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/bank-accounts')
export class ErpBankAccountsController {
  constructor(private readonly service: ErpBankAccountsService) {}

  @Post()
  @ApiOperation({ summary: 'Create ERP bank account' })
  @ApiResponse({ status: 201, description: 'Bank account created' })
  create(@Body() dto: CreateErpBankAccountDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List ERP bank accounts' })
  @ApiResponse({ status: 200, description: 'List of bank accounts' })
  findAll(@Query() query: QueryErpBankAccountDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one ERP bank account' })
  @ApiResponse({ status: 200, description: 'Bank account detail' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate ERP bank accounts' })
  @ApiResponse({ status: 200, description: 'Bulk status updated' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpBankAccountDto, @Request() req: any) {
    return this.service.bulkUpdateStatus(dto, req.user?.id);
  }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete ERP bank accounts' })
  @ApiResponse({ status: 200, description: 'Bulk deleted' })
  bulkDelete(@Body() dto: BulkErpBankAccountDto, @Request() req: any) {
    return this.service.bulkDelete(dto, req.user?.id);
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update ERP bank account' })
  @ApiResponse({ status: 200, description: 'Bank account updated' })
  update(@Param('id') id: string, @Body() dto: UpdateErpBankAccountDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft delete ERP bank account' })
  @ApiResponse({ status: 200, description: 'Bank account deleted' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
