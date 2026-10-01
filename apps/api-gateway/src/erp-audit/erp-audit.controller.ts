import { Controller, Get, Query, UseGuards } from '@nestjs/common';
import { ApiOperation, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { QueryErpAuditLogDto } from './dto/query-erp-audit-log.dto';
import { ErpAuditService } from './erp-audit.service';

@ApiTags('ERP Audit')
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/audit-logs')
export class ErpAuditController {
  constructor(private readonly service: ErpAuditService) {}

  @Get()
  @ApiOperation({ summary: 'List audit logs for a given entity or actor' })
  findAll(@Query() query: QueryErpAuditLogDto) {
    return this.service.findAll(query);
  }
}
