import { Body, Controller, Get, Param, Put, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { SaveGridsDto } from './dto/save-grid-columns.dto';
import { ErpSysTransactionGridsService } from './erp-sys-transaction-grids.service';

@ApiTags('ERP Transaction Grids (Kustomisasi Grid)')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/transaction-grids')
export class ErpSysTransactionGridsController {
  constructor(private readonly service: ErpSysTransactionGridsService) {}

  @Get('types')
  @ApiOperation({ summary: 'List transaction types (module→transaction tree)' })
  listTypes() {
    return this.service.listTypes();
  }

  @Get(':code/columns')
  @ApiOperation({ summary: 'Get primary-grid columns (compat — read by the live entry grid)' })
  getColumns(@Param('code') code: string) {
    return this.service.getColumns(code);
  }

  @Get(':code/grids')
  @ApiOperation({ summary: 'Get all grids (tabs) + columns for a transaction type' })
  getGrids(@Param('code') code: string) {
    return this.service.getGrids(code);
  }

  @Put(':code/grids')
  @ApiOperation({ summary: 'Replace all grids (tabs) + columns for a transaction type' })
  saveGrids(
    @Param('code') code: string,
    @Body() dto: SaveGridsDto,
    @Request() req: any,
  ) {
    return this.service.saveGrids(code, dto, req.user?.id);
  }
}
