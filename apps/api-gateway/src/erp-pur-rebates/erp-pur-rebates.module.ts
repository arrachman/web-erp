import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpPurRebatesController } from './erp-pur-rebates.controller';
import { ErpPurRebatesService } from './erp-pur-rebates.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpPurRebatesController],
  providers: [ErpPurRebatesService],
  exports: [ErpPurRebatesService],
})
export class ErpPurRebatesModule {}
