import {
  Body,
  Controller,
  Delete,
  Get,
  Param,
  Post,
  Request,
  UseGuards,
} from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import {
  GeneratePackingUnitsDto,
  SetPackingUnitPackedDto,
} from './dto/packing-unit.dto';
import { ErpSlsPackingUnitsService } from './erp-sls-packing-units.service';

@ApiTags('ERP Sls Packing Units')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/sls/packing-lists/:id/units')
export class ErpSlsPackingListUnitsController {
  constructor(private readonly service: ErpSlsPackingUnitsService) {}

  @Get()
  @ApiOperation({ summary: 'Unit packing per siswa pada sebuah packing list + progres' })
  list(@Param('id') id: string) {
    return this.service.listUnits(id);
  }

  @Post('generate')
  @ApiOperation({ summary: 'Buat unit per siswa dari roster (array / CSV nama,kelas)' })
  generate(@Param('id') id: string, @Body() dto: GeneratePackingUnitsDto, @Request() req: any) {
    return this.service.generate(id, dto, req.user?.id);
  }
}

@ApiTags('ERP Sls Packing Units')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/sls/packing-units')
export class ErpSlsPackingUnitsController {
  constructor(private readonly service: ErpSlsPackingUnitsService) {}

  @Get('overview')
  @ApiOperation({ summary: 'Packing list + jumlah unit (total / sudah packed)' })
  overview() {
    return this.service.overview();
  }

  @Post(':id/pack')
  @ApiOperation({ summary: 'Tandai unit PACKED / kembalikan PENDING' })
  pack(@Param('id') id: string, @Body() dto: SetPackingUnitPackedDto, @Request() req: any) {
    return this.service.setPacked(id, dto.packed, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Hapus unit packing (soft delete)' })
  remove(@Param('id') id: string) {
    return this.service.removeUnit(id);
  }
}
