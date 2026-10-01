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
import { ErpPurVendorAdvancesService } from './erp-pur-vendor-advances.service';
import { CreateVendorAdvanceDto } from './dto/create-vendor-advance.dto';
import { UpdateVendorAdvanceDto } from './dto/update-vendor-advance.dto';
import { QueryVendorAdvanceDto } from './dto/query-vendor-advance.dto';
import { TransitionVendorAdvanceDto } from './dto/transition-vendor-advance.dto';

@ApiTags('ERP Purchasing — Vendor Advances (AP)')
@ApiBearerAuth('erp-jwt')
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/pur/vendor-advances')
export class ErpPurVendorAdvancesController {
  constructor(private readonly service: ErpPurVendorAdvancesService) {}

  @Post()
  @ApiOperation({ summary: 'Create Vendor Advance (Uang Muka Pembelian)' })
  create(@Body() dto: CreateVendorAdvanceDto, @Request() req: any) {
    return this.service.create(dto, req.user?.sub ?? req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List Vendor Advances with filters and pagination' })
  findAll(@Query() query: QueryVendorAdvanceDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get single Vendor Advance by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Vendor Advance (only when DRAFT or REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdateVendorAdvanceDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.sub ?? req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Vendor Advance (not allowed when POSTED)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.sub ?? req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow transition: SUBMIT / APPROVE / REJECT / POST / REOPEN' })
  transition(
    @Param('id') id: string,
    @Body() dto: TransitionVendorAdvanceDto,
    @Request() req: any,
  ) {
    return this.service.transition(BigInt(id), dto, req.user?.sub ?? req.user?.id);
  }
}
