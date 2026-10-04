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
import { ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { ErpContractsService } from './erp-contracts.service';
import { ErpBundlesService } from './erp-bundles.service';
import {
  CreateContractPriceDto,
  QueryContractPricesDto,
  UpdateContractPriceDto,
  UpsertBundleDto,
} from './dto/contracts.dto';

const actorId = (req: any): string | undefined =>
  req.user?.id?.toString() ?? req.user?.sub?.toString() ?? req.user?.userId?.toString();

/** Fase 3 W7 — admin ERP: harga kontrak per sekolah + definisi paket/bundle. */
@ApiTags('ERP Contracts')
@Controller('erp/contracts')
@UseGuards(ErpJwtAuthGuard)
export class ErpContractsController {
  constructor(
    private readonly contracts: ErpContractsService,
    private readonly bundles: ErpBundlesService,
  ) {}

  @Get('prices')
  listPrices(@Query() query: QueryContractPricesDto) {
    return this.contracts.list(query);
  }

  @Post('prices')
  createPrice(@Request() req: any, @Body() dto: CreateContractPriceDto) {
    return this.contracts.create(dto, actorId(req));
  }

  @Patch('prices/:id')
  updatePrice(@Request() req: any, @Param('id') rowId: string, @Body() dto: UpdateContractPriceDto) {
    return this.contracts.update(rowId, dto, actorId(req));
  }

  @Delete('prices/:id')
  removePrice(@Param('id') rowId: string) {
    return this.contracts.remove(rowId);
  }

  @Get('bundles')
  listBundles() {
    return this.bundles.list();
  }

  @Get('bundles/:id')
  getBundle(@Param('id') bundleId: string) {
    return this.bundles.get(bundleId);
  }

  @Post('bundles')
  upsertBundle(@Request() req: any, @Body() dto: UpsertBundleDto) {
    return this.bundles.upsert(dto, actorId(req));
  }

  @Delete('bundles/:id')
  removeBundle(@Param('id') bundleId: string) {
    return this.bundles.remove(bundleId);
  }
}
