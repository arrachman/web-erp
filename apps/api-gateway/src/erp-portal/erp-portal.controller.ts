import {
  Body,
  Controller,
  Get,
  Param,
  Patch,
  Post,
  Query,
  Request,
  UseGuards,
} from '@nestjs/common';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { ErpPortalAuthGuard } from './erp-portal.guard';
import { ErpPortalAccountsService } from './erp-portal-accounts.service';
import { ErpPortalShopService } from './erp-portal-shop.service';
import {
  PortalCreateOrderDto,
  PortalLeadDto,
  PortalLoginDto,
  PortalRegisterDto,
  PortalRejectDto,
  PortalUpdateProfileDto,
} from './dto/erp-portal.dto';

const actorId = (req: any): string | undefined =>
  req.user?.id?.toString() ?? req.user?.sub?.toString() ?? req.user?.userId?.toString();

/** Publik: registrasi, login, lead landing (W1), sorotan katalog landing. */
@Controller('erp/portal')
export class ErpPortalPublicController {
  constructor(
    private readonly accounts: ErpPortalAccountsService,
    private readonly shop: ErpPortalShopService,
  ) {}

  @Post('register')
  register(@Body() dto: PortalRegisterDto) {
    return this.accounts.register(dto);
  }

  @Post('auth/login')
  login(@Body() dto: PortalLoginDto) {
    return this.accounts.login(dto);
  }

  @Post('leads')
  createLead(@Body() dto: PortalLeadDto) {
    return this.accounts.createLead(dto);
  }

  @Get('public/highlights')
  highlights() {
    return this.shop.highlights();
  }
}

/** Portal sekolah (token portal, akun ACTIVE). */
@Controller('erp/portal')
@UseGuards(ErpPortalAuthGuard)
export class ErpPortalController {
  constructor(
    private readonly accounts: ErpPortalAccountsService,
    private readonly shop: ErpPortalShopService,
  ) {}

  @Get('me')
  me(@Request() req: any) {
    return this.accounts.me(req.portalAccount);
  }

  @Patch('me/profile')
  updateProfile(@Request() req: any, @Body() dto: PortalUpdateProfileDto) {
    return this.accounts.updateProfile(req.portalAccount, dto);
  }

  @Get('catalog')
  catalog(
    @Query('search') search?: string,
    @Query('jenjang') jenjang?: string,
    @Query('page') page?: string,
    @Query('pageSize') pageSize?: string,
  ) {
    return this.shop.catalog({
      search,
      jenjang,
      page: page ? Number(page) : undefined,
      pageSize: pageSize ? Number(pageSize) : undefined,
    });
  }

  @Post('orders')
  createOrder(@Request() req: any, @Body() dto: PortalCreateOrderDto) {
    return this.shop.createOrder(req.portalAccount, dto);
  }

  @Get('orders')
  listOrders(@Request() req: any) {
    return this.shop.listOrders(req.portalAccount);
  }

  @Get('orders/:id')
  orderDetail(@Request() req: any, @Param('id') orderId: string) {
    return this.shop.orderDetail(req.portalAccount, orderId);
  }

  @Get('invoices')
  listInvoices(@Request() req: any) {
    return this.shop.listInvoices(req.portalAccount);
  }
}

/** Admin internal (JWT ERP): antrean persetujuan akun + lead (W3). */
@Controller('erp/portal/admin')
@UseGuards(ErpJwtAuthGuard)
export class ErpPortalAdminController {
  constructor(private readonly accounts: ErpPortalAccountsService) {}

  @Get('accounts')
  listAccounts(@Query('status') status?: string, @Query('search') search?: string) {
    return this.accounts.listAccounts(status, search);
  }

  @Get('leads')
  listLeads() {
    return this.accounts.listLeads();
  }

  @Post('accounts/:id/approve')
  approve(@Request() req: any, @Param('id') accountId: string) {
    return this.accounts.approve(accountId, actorId(req));
  }

  @Post('accounts/:id/reject')
  reject(@Param('id') accountId: string, @Body() dto: PortalRejectDto) {
    return this.accounts.reject(accountId, dto.reason);
  }

  @Post('accounts/:id/suspend')
  suspend(@Param('id') accountId: string) {
    return this.accounts.setStatus(accountId, 'SUSPENDED');
  }

  @Post('accounts/:id/activate')
  activate(@Param('id') accountId: string) {
    return this.accounts.setStatus(accountId, 'ACTIVE');
  }
}
