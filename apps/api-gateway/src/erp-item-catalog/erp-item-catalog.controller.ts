import { Body, Controller, Get, Param, Put, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { QueryItemCatalogDto } from './dto/query-item-catalog.dto';
import { UpsertItemCatalogDto } from './dto/upsert-item-catalog.dto';
import { ErpItemCatalogService } from './erp-item-catalog.service';

@ApiTags('ERP Item Catalog')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/item-catalog')
export class ErpItemCatalogController {
  constructor(private readonly service: ErpItemCatalogService) {}

  @Get()
  @ApiOperation({ summary: 'List items with their school-catalog profiles' })
  list(@Query() q: QueryItemCatalogDto) {
    return this.service.list(q);
  }

  @Get(':itemId')
  @ApiOperation({ summary: 'Get one item with its school-catalog profile' })
  getOne(@Param('itemId') itemId: string) {
    return this.service.getOne(itemId);
  }

  @Put(':itemId')
  @ApiOperation({ summary: 'Create or update the school-catalog profile of an item' })
  upsert(@Param('itemId') itemId: string, @Body() dto: UpsertItemCatalogDto, @Request() req: any) {
    return this.service.upsert(itemId, dto, req.user?.id);
  }
}
