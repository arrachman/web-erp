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
import { CreateArReceiptDto } from './dto/create-ar-receipt.dto';
import { QueryArReceiptDto } from './dto/query-ar-receipt.dto';
import { UpdateArReceiptDto } from './dto/update-ar-receipt.dto';
import { ErpFinArReceiptsService } from './erp-fin-ar-receipts.service';

@ApiTags('ERP Fin AR Receipts')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/fin/ar-receipts')
export class ErpFinArReceiptsController {
  constructor(private readonly service: ErpFinArReceiptsService) {}

  @Post()
  @ApiOperation({ summary: 'Create AR receipt' })
  create(@Body() dto: CreateArReceiptDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List AR receipts' })
  findAll(@Query() query: QueryArReceiptDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get AR receipt' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update AR receipt' })
  update(
    @Param('id') id: string,
    @Body() dto: UpdateArReceiptDto,
    @Request() req: any,
  ) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete AR receipt' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
