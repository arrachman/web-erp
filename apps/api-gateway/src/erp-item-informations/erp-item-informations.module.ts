import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpItemInformationsController } from './erp-item-informations.controller';
import { ErpItemInformationsService } from './erp-item-informations.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpItemInformationsController],
  providers: [ErpItemInformationsService],
  exports: [ErpItemInformationsService],
})
export class ErpItemInformationsModule {}
