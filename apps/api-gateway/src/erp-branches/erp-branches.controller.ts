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
import { BulkErpBranchDto, BulkStatusErpBranchDto } from './dto/bulk-erp-branch.dto';
import { CreateErpBranchDto } from './dto/create-erp-branch.dto';
import { QueryErpBranchDto } from './dto/query-erp-branch.dto';
import { UpdateErpBranchDto } from './dto/update-erp-branch.dto';
import { ErpBranchesService } from './erp-branches.service';

@ApiTags('ERP Branches')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/branches')
export class ErpBranchesController {
  constructor(private readonly service: ErpBranchesService) {}

  @Post()
  @ApiOperation({ summary: 'Create ERP branch' })
  @ApiResponse({ status: 201, description: 'ERP branch created' })
  create(@Body() dto: CreateErpBranchDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'Get ERP branch list' })
  @ApiResponse({ status: 200, description: 'List of ERP branches' })
  findAll(@Query() query: QueryErpBranchDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one ERP branch' })
  @ApiResponse({ status: 200, description: 'ERP branch detail' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update ERP branch' })
  @ApiResponse({ status: 200, description: 'ERP branch updated' })
  update(@Param('id') id: string, @Body() dto: UpdateErpBranchDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate / deactivate ERP branches' })
  @ApiResponse({ status: 200, description: 'Branches status updated' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpBranchDto, @Request() req: any) {
    return this.service.bulkUpdateStatus(dto, req.user?.id);
  }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete ERP branches' })
  @ApiResponse({ status: 200, description: 'Branches deleted' })
  bulkDelete(@Body() dto: BulkErpBranchDto, @Request() req: any) {
    return this.service.bulkDelete(dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete ERP branch (soft delete)' })
  @ApiResponse({ status: 200, description: 'ERP branch deleted' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
