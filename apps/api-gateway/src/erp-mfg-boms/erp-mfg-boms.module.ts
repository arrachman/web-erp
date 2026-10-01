import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpMfgBomsController } from './erp-mfg-boms.controller';
import { ErpMfgBomsService } from './erp-mfg-boms.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpMfgBomsController],
  providers: [ErpMfgBomsService],
  exports: [ErpMfgBomsService],
})
export class ErpMfgBomsModule {}
