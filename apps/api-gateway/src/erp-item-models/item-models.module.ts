import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpItemModelsController } from './item-models.controller';
import { ErpItemModelsService } from './item-models.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpItemModelsController],
  providers: [ErpItemModelsService],
  exports: [ErpItemModelsService],
})
export class ErpItemModelsModule {}
