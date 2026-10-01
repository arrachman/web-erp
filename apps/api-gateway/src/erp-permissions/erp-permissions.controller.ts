import { Controller, Get, Param, Query, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { QueryErpPermissionDto } from './dto/query-erp-permission.dto';
import { ErpPermissionsService } from './erp-permissions.service';

@ApiTags('ERP Permissions')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/permissions')
export class ErpPermissionsController {
  constructor(private readonly service: ErpPermissionsService) {}

  @Get()
  @ApiOperation({ summary: 'List ERP permissions (paginated)' })
  @ApiResponse({ status: 200, description: 'List of ERP permissions' })
  findAll(@Query() query: QueryErpPermissionDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one ERP permission' })
  @ApiResponse({ status: 200, description: 'ERP permission detail' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }
}
