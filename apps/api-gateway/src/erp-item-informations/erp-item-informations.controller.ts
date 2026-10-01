import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { CreateErpItemInformationDto } from './dto/create-item-information.dto';
import { QueryErpItemInformationDto } from './dto/query-item-information.dto';
import { UpdateErpItemInformationDto } from './dto/update-item-information.dto';
import { ErpItemInformationsService } from './erp-item-informations.service';

@ApiTags('ERP Item Informations')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/item-informations')
export class ErpItemInformationsController {
  constructor(private readonly service: ErpItemInformationsService) {}

  @Post()
  @ApiOperation({ summary: 'Create Item Information' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpItemInformationDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Item Informations' })
  findAll(@Query() query: QueryErpItemInformationDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Item Information by ID' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Get('by-item/:itemId')
  @ApiOperation({ summary: 'Get Item Information by Item ID' })
  findByItemId(@Param('itemId') itemId: string) { return this.service.findByItemId(BigInt(itemId)); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Item Information' })
  update(@Param('id') id: string, @Body() dto: UpdateErpItemInformationDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Item Information' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
