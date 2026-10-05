import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import {
  ErpWaDevicesController,
  ErpWhatsappController,
  ErpWhatsappWebhookController,
} from './erp-whatsapp.controller';
import { ErpWhatsappService } from './erp-whatsapp.service';
import { ErpWaTemplatesService } from './erp-wa-templates.service';
import { ErpWaDevicesService } from './erp-wa-devices.service';
import { ErpWaNotifierService } from './erp-wa-notifier.service';
import { ErpWhatsappConfigService } from './erp-whatsapp-config.service';
import { ErpWhatsappProvider } from './erp-whatsapp-provider.service';

/**
 * WhatsApp (Fase 3 W6) — gateway self-hosted kompatibel Fonnte di :3204.
 * Konfigurasi (URL gateway, token akun/device) hidup di sys_settings
 * module=WHATSAPP — bukan .env. `ErpWhatsappService` diekspor untuk hook
 * notifikasi dari modul lain (portal, penjualan, pengiriman).
 */
@Module({
  imports: [PrismaModule],
  controllers: [ErpWhatsappController, ErpWaDevicesController, ErpWhatsappWebhookController],
  providers: [
    ErpWhatsappService,
    ErpWaTemplatesService,
    ErpWaDevicesService,
    ErpWhatsappConfigService,
    ErpWhatsappProvider,
    ErpWaNotifierService,
  ],
  exports: [ErpWhatsappService, ErpWhatsappConfigService],
})
export class ErpWhatsappModule {}
