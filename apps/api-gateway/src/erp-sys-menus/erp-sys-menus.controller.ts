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
import { BulkErpSysMenuDto, BulkStatusErpSysMenuDto } from './dto/bulk-erp-sys-menu.dto';
import { CreateErpSysMenuDto } from './dto/create-erp-sys-menu.dto';
import { QueryErpSysMenuDto } from './dto/query-erp-sys-menu.dto';
import { ReorderErpSysMenuDto } from './dto/reorder-erp-sys-menu.dto';
import { UpdateErpSysMenuDto } from './dto/update-erp-sys-menu.dto';
import { ErpSysMenusService } from './erp-sys-menus.service';

@ApiTags('ERP Sys Menus')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/sys-menus')
export class ErpSysMenusController {
  constructor(private readonly service: ErpSysMenusService) {}

  @Post()
  @ApiOperation({ summary: 'Create ERP menu item' })
  @ApiResponse({ status: 201, description: 'ERP menu created' })
  create(@Body() dto: CreateErpSysMenuDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List ERP menus (flat)' })
  @ApiResponse({ status: 200, description: 'Flat list of ERP menus' })
  findAll(@Query() query: QueryErpSysMenuDto) {
    return this.service.findAll(query);
  }

  @Get('tree')
  @ApiOperation({ summary: 'Get ERP menu tree (nested)' })
  @ApiResponse({ status: 200, description: 'Nested menu tree' })
  getTree() {
    return this.service.getTree();
  }

  @Get('my-menus')
  @ApiOperation({ summary: 'Get menus accessible to the current ERP user (role-filtered)' })
  @ApiResponse({ status: 200, description: 'Menu tree filtered by user role' })
  getMyMenus(@Request() req: any) {
    return this.service.getMyMenus(req.user.id, req.user.erpLevel);
  }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk update active status for ERP menus' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpSysMenuDto, @Request() req: any) {
    return this.service.bulkUpdateStatus(dto, req.user?.id);
  }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete ERP menus' })
  bulkDelete(@Body() dto: BulkErpSysMenuDto, @Request() req: any) {
    return this.service.bulkDelete(dto, req.user?.id);
  }

  @Post('reorder')
  @ApiOperation({
    summary:
      'Atomically reorder ERP menus (cross-parent supported). Each item sets new parentId + sortOrder.',
  })
  @ApiResponse({ status: 200, description: 'ERP menus reordered' })
  reorder(@Body() dto: ReorderErpSysMenuDto, @Request() req: any) {
    return this.service.reorder(dto, req.user?.id);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one ERP menu' })
  @ApiResponse({ status: 200, description: 'ERP menu detail' })
  @ApiResponse({ status: 404, description: 'ERP menu not found' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update ERP menu' })
  @ApiResponse({ status: 200, description: 'ERP menu updated' })
  update(@Param('id') id: string, @Body() dto: UpdateErpSysMenuDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete ERP menu' })
  @ApiResponse({ status: 200, description: 'ERP menu deleted' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
