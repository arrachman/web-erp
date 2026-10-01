import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpExpeditionsController } from './expeditions.controller';
import { ErpExpeditionsService } from './expeditions.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpExpeditionsController],
  providers: [ErpExpeditionsService],
  exports: [ErpExpeditionsService],
})
export class ErpExpeditionsModule {}
