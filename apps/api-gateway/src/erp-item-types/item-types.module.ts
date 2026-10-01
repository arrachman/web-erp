import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpItemKindsController } from './item-types.controller';
import { ErpItemKindsService } from './item-types.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpItemKindsController],
  providers: [ErpItemKindsService],
  exports: [ErpItemKindsService],
})
export class ErpItemKindsModule {}
