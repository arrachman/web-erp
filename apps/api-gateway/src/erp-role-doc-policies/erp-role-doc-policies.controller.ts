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
import {
  ApiBearerAuth,
  ApiOperation,
  ApiQuery,
  ApiResponse,
  ApiTags,
} from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { CreateRoleDocPolicyDto } from './dto/create-role-doc-policy.dto';
import { QueryRoleDocPolicyDto } from './dto/query-role-doc-policy.dto';
import { UpdateRoleDocPolicyDto } from './dto/update-role-doc-policy.dto';
import { BulkUpsertRoleDocPolicyDto } from './dto/bulk-upsert-role-doc-policy.dto';
import { ErpRoleDocPoliciesService } from './erp-role-doc-policies.service';

@ApiTags('ERP Role Doc Policies')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/role-doc-policies')
export class ErpRoleDocPoliciesController {
  constructor(private readonly service: ErpRoleDocPoliciesService) {}

  /**
   * Returns the allowed creation statuses for the current user + document type.
   * Used by transaction forms to populate the status dropdown.
   */
  @Get('my-allowed-statuses')
  @ApiOperation({ summary: 'Get allowed creation statuses for current user' })
  @ApiQuery({ name: 'documentType', required: true, example: 'CASH_RECEIPT' })
  @ApiResponse({ status: 200, description: 'Array of allowed status strings' })
  async myAllowedStatuses(
    @Query('documentType') documentType: string,
    @Request() req: any,
  ) {
    const statuses = await this.service.getAllowedStatusesForUser(
      req.user.id,
      documentType,
    );
    return { success: true, data: statuses };
  }

  @Get('all-for-role')
  @ApiOperation({ summary: 'Get all policies for a role (no pagination)' })
  @ApiQuery({ name: 'roleId', required: true })
  listAllForRole(@Query('roleId') roleId: string) {
    return this.service.listAllForRole(roleId);
  }

  @Post('bulk')
  @ApiOperation({ summary: 'Bulk upsert role doc policies for a role' })
  bulkUpsert(@Body() dto: BulkUpsertRoleDocPolicyDto, @Request() req: any) {
    return this.service.bulkUpsert(dto, req.user?.id);
  }

  @Post()
  @ApiOperation({ summary: 'Create role doc policy' })
  create(@Body() dto: CreateRoleDocPolicyDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List role doc policies' })
  findAll(@Query() query: QueryRoleDocPolicyDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one role doc policy' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update role doc policy' })
  update(@Param('id') id: string, @Body() dto: UpdateRoleDocPolicyDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft delete role doc policy' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
