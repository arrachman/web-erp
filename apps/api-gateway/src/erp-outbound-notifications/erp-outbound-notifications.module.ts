import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpWhatsappModule } from '../erp-whatsapp/erp-whatsapp.module';
import { ErpOutboundNotificationsController } from './erp-outbound-notifications.controller';
import { ErpOutboundNotificationsService } from './erp-outbound-notifications.service';

@Module({
  imports: [PrismaModule, ErpWhatsappModule],
  controllers: [ErpOutboundNotificationsController],
  providers: [ErpOutboundNotificationsService],
  exports: [ErpOutboundNotificationsService],
})
export class ErpOutboundNotificationsModule {}
