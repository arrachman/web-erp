import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpHomeWidgetsController } from './erp-home-widgets.controller';
import { ErpHomeWidgetsService } from './erp-home-widgets.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpHomeWidgetsController],
  providers: [ErpHomeWidgetsService],
  exports: [ErpHomeWidgetsService],
})
export class ErpHomeWidgetsModule {}
