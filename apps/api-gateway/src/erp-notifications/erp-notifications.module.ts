import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpNotificationsController } from './erp-notifications.controller';
import { ErpNotificationsService } from './erp-notifications.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpNotificationsController],
  providers: [ErpNotificationsService],
  exports: [ErpNotificationsService],
})
export class ErpNotificationsModule {}
