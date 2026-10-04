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
import {
  CreateInvLotDto,
  FefoQueryDto,
  QueryInvLotsDto,
  UpdateInvLotDto,
} from './dto/inv-lot.dto';
import { ErpInvLotsService } from './erp-inv-lots.service';

@ApiTags('ERP Inv Lots')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/inv/lots')
export class ErpInvLotsController {
  constructor(private readonly service: ErpInvLotsService) {}

  @Get('fefo')
  @ApiOperation({ summary: 'Saran picking FEFO untuk item+gudang+qty (lot aktif, expiry terdekat dulu)' })
  fefo(@Query() query: FefoQueryDto) {
    return this.service.fefo(query.itemId, query.warehouseId, query.quantity);
  }

  @Get()
  @ApiOperation({ summary: 'Daftar lot/batch (server-driven, termasuk saldo turunan)' })
  list(@Query() query: QueryInvLotsDto) {
    return this.service.list(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Detail lot + saldo per gudang + riwayat pergerakan' })
  one(@Param('id') id: string) {
    return this.service.get(id);
  }

  @Post()
  @ApiOperation({ summary: 'Buat lot manual (mis. stok awal); lot dari GRN terbentuk otomatis saat posting' })
  create(@Body() dto: CreateInvLotDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Koreksi metadata lot (tidak mengubah saldo)' })
  update(@Param('id') id: string, @Body() dto: UpdateInvLotDto, @Request() req: any) {
    return this.service.update(id, dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Hapus lot (hanya bila belum punya riwayat pergerakan)' })
  remove(@Param('id') id: string) {
    return this.service.remove(id);
  }
}
