import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpSettingsController } from './erp-settings.controller';
import { ErpSettingsService } from './erp-settings.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpSettingsController],
  providers: [ErpSettingsService],
  exports: [ErpSettingsService],
})
export class ErpSettingsModule {}
