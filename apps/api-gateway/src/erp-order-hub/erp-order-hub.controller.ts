import { Body, Controller, Get, Param, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { ImportSiplahDto } from './dto/import-order-hub.dto';
import { QueryOrderHubDto } from './dto/query-order-hub.dto';
import { ErpOrderHubImportService } from './erp-order-hub-import.service';
import { ErpOrderHubService } from './erp-order-hub.service';

@ApiTags('ERP Order Hub')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/order-hub')
export class ErpOrderHubController {
  constructor(
    private readonly service: ErpOrderHubService,
    private readonly importer: ErpOrderHubImportService,
  ) {}

  @Get()
  @ApiOperation({ summary: 'Unified order queue across channels with derived hub stage' })
  @ApiResponse({ status: 200, description: 'Paginated order hub list' })
  findAll(@Query() query: QueryOrderHubDto) {
    return this.service.findAll(query);
  }

  @Get(':orderId')
  @ApiOperation({ summary: 'Order hub detail: order, chain documents, status history' })
  findOne(@Param('orderId') orderId: string) {
    return this.service.findOne(BigInt(orderId));
  }

  @Post('import-siplah')
  @ApiOperation({ summary: 'Import SIPLah orders from pre-parsed export rows (idempotent)' })
  importSiplah(@Body() dto: ImportSiplahDto, @Request() req: any) {
    return this.importer.importSiplah(dto, req.user?.id);
  }

  @Post('sync')
  @ApiOperation({ summary: 'Recompute derived stages for all orders and log transitions' })
  sync(@Request() req: any) {
    return this.service.sync(req.user?.id);
  }
}
