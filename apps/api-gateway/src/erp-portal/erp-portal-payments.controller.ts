import { Body, Controller, Get, Headers, Param, Post, Req, UseGuards } from '@nestjs/common';
import { ErpPortalPaymentsService } from './erp-portal-payments.service';
import { ErpPortalAuthGuard } from './erp-portal.guard';
import type { ErpPortalAccount } from '@prisma/client';

type ReqWithAccount = { portalAccount: ErpPortalAccount };

/** W5 — pembayaran tagihan dari portal (intent VA + riwayat). */
@Controller('erp/portal')
@UseGuards(ErpPortalAuthGuard)
export class ErpPortalPaymentsController {
  constructor(private readonly payments: ErpPortalPaymentsService) {}

  @Post('invoices/:invoiceId/pay')
  pay(@Req() req: ReqWithAccount, @Param('invoiceId') invoiceId: string) {
    return this.payments.createForInvoice(req.portalAccount, invoiceId);
  }

  @Post('payments/:paymentId/confirm-sent')
  confirmSent(
    @Req() req: ReqWithAccount,
    @Param('paymentId') paymentId: string,
    @Body() body: { note?: string },
  ) {
    return this.payments.confirmSent(req.portalAccount, paymentId, body?.note);
  }

  @Get('payments')
  list(@Req() req: ReqWithAccount) {
    return this.payments.listForAccount(req.portalAccount);
  }
}

/** W5 — webhook provider pembayaran (publik, terverifikasi HMAC). */
@Controller('erp/portal/payments')
export class ErpPortalPaymentsWebhookController {
  constructor(private readonly payments: ErpPortalPaymentsService) {}

  @Post('webhook/:provider')
  webhook(
    @Param('provider') provider: string,
    @Headers('x-signature') signature: string | undefined,
    @Body() body: { ref?: string; status?: string; amount?: string },
  ) {
    return this.payments.handleWebhook(provider, signature, body ?? {});
  }
}
