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
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import {
  BulkErpHomeWidgetDto,
  BulkStatusErpHomeWidgetDto,
} from './dto/bulk-erp-home-widget.dto';
import { CreateErpHomeWidgetDto } from './dto/create-erp-home-widget.dto';
import { QueryErpHomeWidgetDto } from './dto/query-erp-home-widget.dto';
import { UpdateErpHomeWidgetDto } from './dto/update-erp-home-widget.dto';
import { ErpHomeWidgetsService } from './erp-home-widgets.service';

@ApiTags('ERP Home Widgets')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/home-widgets')
export class ErpHomeWidgetsController {
  constructor(private readonly service: ErpHomeWidgetsService) {}

  @Post()
  @ApiOperation({ summary: 'Create ERP home widget' })
  @ApiResponse({ status: 201, description: 'Home widget created' })
  create(@Body() dto: CreateErpHomeWidgetDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List ERP home widgets' })
  @ApiResponse({ status: 200, description: 'List of home widgets' })
  findAll(@Query() query: QueryErpHomeWidgetDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one ERP home widget' })
  @ApiResponse({ status: 200, description: 'Home widget detail' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk enable/disable ERP home widgets' })
  @ApiResponse({ status: 200, description: 'Bulk status updated' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpHomeWidgetDto, @Request() req: any) {
    return this.service.bulkUpdateStatus(dto, req.user?.id);
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update ERP home widget' })
  @ApiResponse({ status: 200, description: 'Home widget updated' })
  update(@Param('id') id: string, @Body() dto: UpdateErpHomeWidgetDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete ERP home widgets' })
  @ApiResponse({ status: 200, description: 'Bulk deleted' })
  bulkDelete(@Body() dto: BulkErpHomeWidgetDto, @Request() req: any) {
    return this.service.bulkDelete(dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft delete ERP home widget' })
  @ApiResponse({ status: 200, description: 'Home widget deleted' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
