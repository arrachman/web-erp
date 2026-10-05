import {
  Body,
  Controller,
  Delete,
  Get,
  Headers,
  Param,
  Patch,
  Post,
  Query,
  Req,
  UnauthorizedException,
  UseGuards,
} from '@nestjs/common';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { ErpWhatsappService } from './erp-whatsapp.service';
import { ErpWaTemplatesService } from './erp-wa-templates.service';
import { ErpWaDevicesService } from './erp-wa-devices.service';
import { ErpWhatsappConfigService } from './erp-whatsapp-config.service';
import {
  ActivateWaDeviceDto,
  CreateWaDeviceDto,
  CreateWaTemplateDto,
  QueryWaLogDto,
  QueryWaTemplateDto,
  SendWaTestDto,
  UpdateWaDeviceDto,
  UpdateWaSettingsDto,
  UpdateWaTemplateDto,
  WaDeviceQrDto,
} from './dto/erp-whatsapp.dto';

type ReqWithUser = { user?: { id?: string | number; sub?: string | number } };
const actorOf = (req: ReqWithUser): string | undefined => {
  const v = req.user?.id ?? req.user?.sub;
  return v !== undefined && v !== null ? String(v) : undefined;
};

/** Admin WhatsApp: template, log, statistik, kirim tes, pengaturan, kesehatan. */
@Controller('erp/wa')
@UseGuards(ErpJwtAuthGuard)
export class ErpWhatsappController {
  constructor(
    private readonly service: ErpWhatsappService,
    private readonly templates: ErpWaTemplatesService,
    private readonly settings: ErpWhatsappConfigService,
  ) {}

  @Get('settings')
  async getSettings() {
    return { success: true, data: await this.settings.summary() };
  }

  @Patch('settings')
  async updateSettings(@Body() dto: UpdateWaSettingsDto) {
    if (dto.sendEnabled !== undefined) await this.settings.setSendEnabled(dto.sendEnabled);
    return { success: true, data: await this.settings.summary(), message: 'Pengaturan disimpan' };
  }

  @Get('connection-health')
  connectionHealth() {
    return this.service.connectionHealth();
  }

  @Get('templates')
  findTemplates(@Query() query: QueryWaTemplateDto) {
    return this.templates.findAll(query);
  }

  @Post('templates')
  createTemplate(@Body() dto: CreateWaTemplateDto, @Req() req: ReqWithUser) {
    return this.templates.create(dto, actorOf(req));
  }

  @Get('templates/:id')
  findTemplate(@Param('id') id: string) {
    return this.templates.findOne(id);
  }

  @Patch('templates/:id')
  updateTemplate(
    @Param('id') id: string,
    @Body() dto: UpdateWaTemplateDto,
    @Req() req: ReqWithUser,
  ) {
    return this.templates.update(id, dto, actorOf(req));
  }

  @Delete('templates/:id')
  removeTemplate(@Param('id') id: string, @Req() req: ReqWithUser) {
    return this.templates.remove(id, actorOf(req));
  }

  @Get('logs')
  findLogs(@Query() query: QueryWaLogDto) {
    return this.service.findAllLogs(query);
  }

  @Get('stats')
  stats() {
    return this.service.getStats();
  }

  @Post('logs/:id/resend')
  resend(@Param('id') id: string, @Req() req: ReqWithUser) {
    return this.service.resendLog(id, actorOf(req));
  }

  @Post('send-test')
  sendTest(@Body() dto: SendWaTestDto, @Req() req: ReqWithUser) {
    return this.service.sendTest(dto, actorOf(req));
  }
}

/** Pairing device WhatsApp (wizard di Pengaturan). */
@Controller('erp/settings')
@UseGuards(ErpJwtAuthGuard)
export class ErpWaDevicesController {
  constructor(private readonly devices: ErpWaDevicesService) {}

  @Get('wa-devices')
  list() {
    return this.devices.listDevices();
  }

  @Post('wa-devices')
  add(@Body() dto: CreateWaDeviceDto) {
    return this.devices.addDevice(dto);
  }

  @Post('wa-devices/qr')
  qr(@Body() dto: WaDeviceQrDto) {
    return this.devices.getDeviceQr(dto);
  }

  @Post('wa-devices/check')
  check(@Body() dto: WaDeviceQrDto) {
    return this.devices.checkDeviceConnected(dto);
  }

  @Post('wa-devices/activate')
  activate(@Body() dto: ActivateWaDeviceDto) {
    return this.devices.activateDevice(dto);
  }

  @Post('wa-devices/update')
  update(@Body() dto: UpdateWaDeviceDto) {
    return this.devices.updateDevice(dto);
  }

  @Delete('wa-devices/:phone')
  remove(@Param('phone') phone: string) {
    return this.devices.removeDevice(phone);
  }

  @Get('wa-status')
  status() {
    return this.devices.activeStatus();
  }
}

/**
 * Webhook status pengiriman dari wa-gateway — publik (tanpa JWT), dilindungi
 * shared secret `X-Webhook-Secret` bila diatur di server.
 */
@Controller('erp/wa')
export class ErpWhatsappWebhookController {
  constructor(
    private readonly service: ErpWhatsappService,
    private readonly settings: ErpWhatsappConfigService,
  ) {}

  @Post('webhook')
  async webhook(
    @Body() body: Record<string, unknown>,
    @Headers('x-webhook-secret') secret?: string,
  ) {
    const expected = await this.settings.webhookSecret();
    if (expected && secret !== expected) {
      throw new UnauthorizedException('Webhook secret tidak valid');
    }
    return this.service.handleWebhook({
      id: body['id'] !== undefined && body['id'] !== null ? String(body['id']) : undefined,
      sender: typeof body['sender'] === 'string' ? body['sender'] : undefined,
      status: typeof body['status'] === 'string' ? body['status'] : undefined,
      state: typeof body['state'] === 'string' ? body['state'] : undefined,
      device: typeof body['device'] === 'string' ? body['device'] : undefined,
      reason: typeof body['reason'] === 'string' ? body['reason'] : undefined,
    });
  }
}
