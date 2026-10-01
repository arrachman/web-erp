import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpMaterialsController } from './materials.controller';
import { ErpMaterialsService } from './materials.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpMaterialsController],
  providers: [ErpMaterialsService],
  exports: [ErpMaterialsService],
})
export class ErpMaterialsModule {}
