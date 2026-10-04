import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import {
  ErpMfgJobSchedulesController,
  ErpMfgMachinesController,
} from './erp-mfg-schedules.controller';
import { ErpMfgSchedulesService } from './erp-mfg-schedules.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpMfgMachinesController, ErpMfgJobSchedulesController],
  providers: [ErpMfgSchedulesService],
  exports: [ErpMfgSchedulesService],
})
export class ErpMfgSchedulesModule {}
