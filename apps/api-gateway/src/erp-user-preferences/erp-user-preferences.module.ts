import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpUserPreferencesController } from './erp-user-preferences.controller';
import { ErpUserPreferencesService } from './erp-user-preferences.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpUserPreferencesController],
  providers: [ErpUserPreferencesService],
  exports: [ErpUserPreferencesService],
})
export class ErpUserPreferencesModule {}
