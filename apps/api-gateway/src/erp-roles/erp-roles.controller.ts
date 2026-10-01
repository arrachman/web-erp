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
import { AssignMenusDto } from './dto/assign-menus.dto';
import { AssignPermissionsDto } from './dto/assign-permissions.dto';
import { CreateErpRoleDto } from './dto/create-erp-role.dto';
import { QueryErpRoleDto } from './dto/query-erp-role.dto';
import { UpdateErpRoleDto } from './dto/update-erp-role.dto';
import { ErpRolesService } from './erp-roles.service';

@ApiTags('ERP Roles')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/roles')
export class ErpRolesController {
  constructor(private readonly service: ErpRolesService) {}

  @Post()
  @ApiOperation({ summary: 'Create ERP role' })
  @ApiResponse({ status: 201, description: 'ERP role created' })
  create(@Body() dto: CreateErpRoleDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List ERP roles (paginated)' })
  @ApiResponse({ status: 200, description: 'List of ERP roles' })
  findAll(@Query() query: QueryErpRoleDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one ERP role' })
  @ApiResponse({ status: 200, description: 'ERP role detail' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update ERP role' })
  @ApiResponse({ status: 200, description: 'ERP role updated' })
  update(@Param('id') id: string, @Body() dto: UpdateErpRoleDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete ERP role' })
  @ApiResponse({ status: 200, description: 'ERP role deleted' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }

  @Post(':id/permissions')
  @ApiOperation({ summary: 'Assign (replace) permissions for a role' })
  @ApiResponse({ status: 200, description: 'Permissions assigned' })
  assignPermissions(
    @Param('id') id: string,
    @Body() dto: AssignPermissionsDto,
    @Request() req: any,
  ) {
    return this.service.assignPermissions(BigInt(id), dto, req.user?.id);
  }

  @Get(':id/permissions')
  @ApiOperation({ summary: 'Get permissions assigned to a role' })
  @ApiResponse({ status: 200, description: 'Role permissions list' })
  getPermissions(@Param('id') id: string) {
    return this.service.getPermissions(BigInt(id));
  }

  @Post(':id/menus')
  @ApiOperation({ summary: 'Assign (replace) menus for a role' })
  @ApiResponse({ status: 200, description: 'Menus assigned' })
  assignMenus(@Param('id') id: string, @Body() dto: AssignMenusDto, @Request() req: any) {
    return this.service.assignMenus(BigInt(id), dto, req.user?.id);
  }

  @Get(':id/menus')
  @ApiOperation({ summary: 'Get menus assigned to a role' })
  @ApiResponse({ status: 200, description: 'Role menus list' })
  getMenus(@Param('id') id: string) {
    return this.service.getMenus(BigInt(id));
  }
}
