import {
  Body,
  Controller,
  Delete,
  Get,
  Param,
  Patch,
  Post,
  Put,
  Query,
  Request,
  UseGuards,
} from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpAccountDto, BulkStatusErpAccountDto } from './dto/bulk-erp-account.dto';
import { CreateErpAccountDto } from './dto/create-erp-account.dto';
import { QueryAccountTreeDto, QueryErpAccountDto } from './dto/query-erp-account.dto';
import { UpdateAccountCodeFormatDto } from './dto/update-account-code-format.dto';
import { UpdateErpAccountDto } from './dto/update-erp-account.dto';
import { toBigIntId } from './account-hierarchy';
import { ErpAccountsService } from './erp-accounts.service';

@ApiTags('ERP Accounts')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/accounts')
export class ErpAccountsController {
  constructor(private readonly service: ErpAccountsService) {}

  @Post()
  @ApiOperation({ summary: 'Create ERP account (chart of accounts)' })
  @ApiResponse({ status: 201, description: 'Account created' })
  create(@Body() dto: CreateErpAccountDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List ERP accounts' })
  @ApiResponse({ status: 200, description: 'List of accounts' })
  findAll(@Query() query: QueryErpAccountDto) {
    return this.service.findAll(query);
  }

  @Get('tree')
  @ApiOperation({
    summary: 'Lazy CoA tree: children of one parent (omit parentId for roots)',
  })
  @ApiResponse({ status: 200, description: 'One level of accounts with hasChildren' })
  findTree(@Query() query: QueryAccountTreeDto) {
    return this.service.findTreeChildren(query);
  }

  @Get('code-format')
  @ApiOperation({
    summary:
      'Get current account code format (segments + separator + derived pattern, maxLength, example)',
  })
  @ApiResponse({ status: 200, description: 'Active account code format' })
  getCodeFormat() {
    return this.service.getCodeFormat().then((data) => ({ success: true, data }));
  }

  @Put('code-format')
  @ApiOperation({
    summary: 'Update account code format. Locked when any account row exists (409).',
  })
  @ApiResponse({ status: 200, description: 'Format updated' })
  @ApiResponse({ status: 409, description: 'Locked because accounts already exist' })
  updateCodeFormat(@Body() dto: UpdateAccountCodeFormatDto, @Request() req: any) {
    return this.service
      .updateCodeFormat(dto.segments, dto.separator, req.user?.id)
      .then((data) => ({ success: true, data }));
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one ERP account' })
  @ApiResponse({ status: 200, description: 'Account detail with parent and children' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(toBigIntId(id, 'id'));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update ERP account' })
  @ApiResponse({ status: 200, description: 'Account updated' })
  update(@Param('id') id: string, @Body() dto: UpdateErpAccountDto, @Request() req: any) {
    return this.service.update(toBigIntId(id, 'id'), dto, req.user?.id);
  }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk update account status' })
  @ApiResponse({ status: 200, description: 'Bulk status updated' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpAccountDto, @Request() req: any) {
    return this.service.bulkUpdateStatus(dto, req.user?.id);
  }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft delete accounts' })
  @ApiResponse({ status: 200, description: 'Bulk deleted' })
  bulkDelete(@Body() dto: BulkErpAccountDto, @Request() req: any) {
    return this.service.bulkDelete(dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft delete ERP account' })
  @ApiResponse({ status: 200, description: 'Account deleted' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(toBigIntId(id, 'id'), req.user?.id);
  }
}
