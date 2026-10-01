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
  BulkErpApprovalRuleDto,
  BulkStatusErpApprovalRuleDto,
} from './dto/bulk-erp-approval-rule.dto';
import { CreateErpApprovalRuleDto } from './dto/create-erp-approval-rule.dto';
import { QueryErpApprovalRuleDto } from './dto/query-erp-approval-rule.dto';
import { UpdateErpApprovalRuleDto } from './dto/update-erp-approval-rule.dto';
import { ErpApprovalRulesService } from './erp-approval-rules.service';

@ApiTags('ERP Approval Rules')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/approval-rules')
export class ErpApprovalRulesController {
  constructor(private readonly service: ErpApprovalRulesService) {}

  @Post()
  @ApiOperation({ summary: 'Create ERP approval rule' })
  @ApiResponse({ status: 201, description: 'Approval rule created' })
  create(@Body() dto: CreateErpApprovalRuleDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List ERP approval rules' })
  @ApiResponse({ status: 200, description: 'List of approval rules' })
  findAll(@Query() query: QueryErpApprovalRuleDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one ERP approval rule' })
  @ApiResponse({ status: 200, description: 'Approval rule detail' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate ERP approval rules' })
  @ApiResponse({ status: 200, description: 'Bulk status updated' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpApprovalRuleDto, @Request() req: any) {
    return this.service.bulkUpdateStatus(dto, req.user?.id);
  }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete ERP approval rules' })
  @ApiResponse({ status: 200, description: 'Bulk deleted' })
  bulkDelete(@Body() dto: BulkErpApprovalRuleDto, @Request() req: any) {
    return this.service.bulkDelete(dto, req.user?.id);
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update ERP approval rule' })
  @ApiResponse({ status: 200, description: 'Approval rule updated' })
  update(@Param('id') id: string, @Body() dto: UpdateErpApprovalRuleDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft delete ERP approval rule' })
  @ApiResponse({ status: 200, description: 'Approval rule deleted' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
