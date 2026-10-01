import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpBanksController } from './banks.controller';
import { ErpBanksService } from './banks.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpBanksController],
  providers: [ErpBanksService],
  exports: [ErpBanksService],
})
export class ErpBanksModule {}
