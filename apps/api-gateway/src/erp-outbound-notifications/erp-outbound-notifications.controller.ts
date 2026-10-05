import { Controller, Get, Query, UseGuards } from '@nestjs/common';
import { ErpOutboundNotificationsService } from './erp-outbound-notifications.service';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';

/** W6 — log notifikasi untuk admin ERP (bukti terkirim/tercatat per peristiwa). */
@Controller('erp/outbound-notifications')
@UseGuards(ErpJwtAuthGuard)
export class ErpOutboundNotificationsController {
  constructor(private readonly service: ErpOutboundNotificationsService) {}

  @Get('logs')
  listLogs(
    @Query('event') event?: string,
    @Query('status') status?: string,
    @Query('partnerId') partnerId?: string,
    @Query('page') page?: string,
    @Query('limit') limit?: string,
  ) {
    return this.service.listLogs({
      event, status, partnerId,
      page: page ? Number(page) : undefined,
      limit: limit ? Number(limit) : undefined,
    });
  }
}
