import { Body, Controller, Get, Param, Post, Query, Req, UseGuards } from '@nestjs/common';
import { ErpPortalPaymentsService } from './erp-portal-payments.service';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';

const actorId = (req: any): string | undefined =>
  req.user?.id?.toString() ?? req.user?.sub?.toString() ?? req.user?.userId?.toString();

/**
 * W5 — antrean konfirmasi pembayaran transfer manual (admin ERP).
 * Admin memverifikasi mutasi bank, lalu Konfirmasi → AR Receipt terbit
 * dan invoice lunas; Tolak mengembalikan klaim ke pengguna.
 */
@Controller('erp/payments')
@UseGuards(ErpJwtAuthGuard)
export class ErpPortalPaymentsAdminController {
  constructor(private readonly payments: ErpPortalPaymentsService) {}

  @Get()
  list(@Query('status') status?: string) {
    return this.payments.listAdmin(status);
  }

  @Post(':id/confirm')
  confirm(@Param('id') id: string, @Req() req: any) {
    return this.payments.confirmByAdmin(id, actorId(req));
  }

  @Post(':id/reject')
  reject(@Param('id') id: string, @Body() body: { reason?: string }, @Req() req: any) {
    return this.payments.rejectByAdmin(id, body?.reason?.trim() || 'Dana tidak ditemukan di mutasi bank.', actorId(req));
  }

  @Get('transfer-target')
  target() {
    return this.payments.transferTarget();
  }
}
