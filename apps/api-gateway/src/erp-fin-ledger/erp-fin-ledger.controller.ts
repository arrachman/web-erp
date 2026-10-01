import { Controller, Get, Param, Query, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { QueryLedgerDto } from './dto/query-ledger.dto';
import { ErpFinLedgerService } from './erp-fin-ledger.service';

@ApiTags('ERP Fin Ledger')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/fin/ledger')
export class ErpFinLedgerController {
  constructor(private readonly service: ErpFinLedgerService) {}

  @Get()
  @ApiOperation({ summary: 'List ledger entries (read-only)' })
  findAll(@Query() query: QueryLedgerDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get ledger entry' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }
}
