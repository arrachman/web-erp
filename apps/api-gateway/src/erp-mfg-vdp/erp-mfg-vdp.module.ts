import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import {
  ErpMfgJobVdpController,
  ErpMfgVdpController,
} from './erp-mfg-vdp.controller';
import { ErpMfgVdpService } from './erp-mfg-vdp.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpMfgJobVdpController, ErpMfgVdpController],
  providers: [ErpMfgVdpService],
  exports: [ErpMfgVdpService],
})
export class ErpMfgVdpModule {}
