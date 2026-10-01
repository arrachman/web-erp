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
import { BulkErpPartnerCategoryDto, BulkStatusErpPartnerCategoryDto } from './dto/bulk-erp-partner-category.dto';
import { CreateErpPartnerCategoryDto } from './dto/create-erp-partner-category.dto';
import { QueryErpPartnerCategoryDto } from './dto/query-erp-partner-category.dto';
import { UpdateErpPartnerCategoryDto } from './dto/update-erp-partner-category.dto';
import { ErpPartnerCategoriesService } from './erp-partner-categories.service';

@ApiTags('ERP Partner Categories')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/partner-categories')
export class ErpPartnerCategoriesController {
  constructor(private readonly service: ErpPartnerCategoriesService) {}

  @Post()
  @ApiOperation({ summary: 'Create ERP partner category' })
  @ApiResponse({ status: 201, description: 'ERP partner category created' })
  create(@Body() dto: CreateErpPartnerCategoryDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'Get ERP partner category list' })
  @ApiResponse({ status: 200, description: 'List of ERP partner categories' })
  findAll(@Query() query: QueryErpPartnerCategoryDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one ERP partner category' })
  @ApiResponse({ status: 200, description: 'ERP partner category detail' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update ERP partner category' })
  @ApiResponse({ status: 200, description: 'ERP partner category updated' })
  update(
    @Param('id') id: string,
    @Body() dto: UpdateErpPartnerCategoryDto,
    @Request() req: any,
  ) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate ERP partner categories' })
  @ApiResponse({ status: 200, description: 'Bulk status updated' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpPartnerCategoryDto, @Request() req: any) {
    return this.service.bulkUpdateStatus(dto, req.user?.id);
  }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete ERP partner categories' })
  @ApiResponse({ status: 200, description: 'Bulk deleted' })
  bulkDelete(@Body() dto: BulkErpPartnerCategoryDto, @Request() req: any) {
    return this.service.bulkDelete(dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete ERP partner category (soft delete)' })
  @ApiResponse({ status: 200, description: 'ERP partner category deleted' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
